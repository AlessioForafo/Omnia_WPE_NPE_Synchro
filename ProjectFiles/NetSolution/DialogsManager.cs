#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.WebUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.Core;
using System.Linq;
using System.Collections.Generic;
#endregion

public class DialogsManager : BaseNetLogic
{
    private IUAVariable synchDialogsWithAllSessions;
    private IUAVariable synchedDialogIDs;
    private int emptyDialogIdsArrayElementIndex = 0;
    private const string DIALOGID = "DialogID";

    public override void Start()
    {
        synchDialogsWithAllSessions = LogicObject.GetVariable("SynchDialogsWithAllSessions");
        synchedDialogIDs = LogicObject.GetVariable("SynchedDialogIDs");
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void SynchDialogInstances()
    {
        if (!synchDialogsWithAllSessions.Value) return;
        var dialogs = Project.Current.Get("UI").FindNodesByType<DialogType>();
        //OpenDialogs(dialogs);
    }

    [ExportMethod]
    public void OnOpenDialog(int dialogId)
    {
        if (IsDialogAlreadyOpen(dialogId)) return;
        UpdateOpenDialogs(dialogId);
    }

    [ExportMethod]
    public void OnCloseDialog(int dialogId)
    {
        if (IsDialogAlreadyClosed(dialogId)) return;
        RemoveDialogIDFromDialogArray(dialogId);
    }

    private DialogType GetDialogById(int dialogId)
    {
        var dialogs = Project.Current.Get("UI").FindNodesByType<DialogType>();
        var dialog = dialogs.FirstOrDefault(d => d.GetVariable(DIALOGID).Value == dialogId);
        return dialog;
    }

    private bool IsDialogAlreadyOpen(UAValue value)
    {
        int[] dialogIds = synchedDialogIDs.Value.Value as int[];
        return Array.IndexOf(dialogIds, value) != -1;
    }

    private bool IsDialogAlreadyClosed(UAValue value)
    {
        int[] dialogIds = synchedDialogIDs.Value.Value as int[];
        return Array.IndexOf(dialogIds, value) == -1;
    }

    private void UpdateOpenDialogs(int dialogId)
    {
        int[] dialogIds = synchedDialogIDs.Value.Value as int[];
        dialogIds = RemoveDuplicatesAndMark(dialogIds);

        int firstAvailableArrayElement = Array.IndexOf(dialogIds, emptyDialogIdsArrayElementIndex);
        dialogIds[firstAvailableArrayElement] = dialogId;

        synchedDialogIDs.Value = dialogIds;
        UICommands.OpenDialog(Owner, GetDialogById(dialogId));
    }

    private void RemoveDialogIDFromDialogArray(int dialogId)
    {
        int[] dialogIds = synchedDialogIDs.Value.Value as int[];
        dialogIds = RemoveDuplicatesAndMark(dialogIds);

        int indexToRemove = Array.IndexOf(dialogIds, dialogId);
        dialogIds[indexToRemove] = emptyDialogIdsArrayElementIndex;

        synchedDialogIDs.Value = dialogIds;
        CloseDialogInstance(dialogId);
    }

    private void CloseDialogInstance(int dialogId)
    {
        var openDialog = Session.Get("UIRoot").Children.OfType<Dialog>().FirstOrDefault(d => d.GetVariable(DIALOGID).Value == dialogId);
        if (openDialog == null) return;
        openDialog.Close();
    }

    private int[] RemoveDuplicatesAndMark(int[] arr)
    {
        HashSet<int> uniqueNumbers = new HashSet<int>();

        for (int i = 0; i < arr.Length; i++)
        {
            if (!uniqueNumbers.Add(arr[i]))
            {
                arr[i] = emptyDialogIdsArrayElementIndex;
            }
        }

        return arr;
    }

    private void OpenDialogs(System.Collections.Generic.IEnumerable<DialogType> dialogs)
    {
        foreach (DialogType d in dialogs)
        {
            var dialogID = d.GetVariable(DIALOGID).Value;
            if (dialogID == null || dialogID == emptyDialogIdsArrayElementIndex) return;
            if (Array.IndexOf(synchedDialogIDs.Value.Value as Array, dialogID) == -1) return;
            UICommands.OpenDialog(Owner, d);
        }
    }
}

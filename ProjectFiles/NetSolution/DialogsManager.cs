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

        synchDialogsWithAllSessions.VariableChange += SynchDataChanged;
        synchedDialogIDs.VariableChange += SynchDataChanged;
    }

    private void SynchDataChanged(object sender, VariableChangeEventArgs e)
    {
        SynchDialogsBetweenSessions();
    }

    public override void Stop()
    {
        synchDialogsWithAllSessions.VariableChange -= SynchDataChanged;
        synchedDialogIDs.VariableChange -= SynchDataChanged;
    }

    [ExportMethod]
    public void SynchDialogsBetweenSessions()
    {
        if (!synchDialogsWithAllSessions.Value) return;

        var dialogAlreadyOpenedInUISession = Session.Get("UIRoot").Children.OfType<Dialog>();
        int[] openDialogsIds = GetOpenDialogIds();

        foreach (var openDialogId in openDialogsIds)
        {
            if (openDialogId == emptyDialogIdsArrayElementIndex || dialogAlreadyOpenedInUISession.Any(d => d.GetVariable(DIALOGID).Value == openDialogId)) continue;
            var d = GetDialogById(openDialogId);
            UICommands.OpenDialog(Owner, d);
        }

        foreach (var dialog in dialogAlreadyOpenedInUISession)
        {
            if (openDialogsIds.Contains(dialog.GetVariable(DIALOGID).Value)) continue;
            dialog.Close();
        }
    }

    [ExportMethod]
    public void OnOpenDialog(int dialogId)
    {
        if (IsDialogAlreadyOpen(dialogId)) return;
        AddDialogIdToDialogsArray(dialogId);
    }

    [ExportMethod]
    public void OnCloseDialog(int dialogId)
    {
        if (!IsDialogAlreadyOpen(dialogId)) return;
        RemoveDialogIdToDialogsArray(dialogId);
    }

    private DialogType GetDialogById(int dialogId)
    {
        var dialogs = Project.Current.Get("UI").FindNodesByType<DialogType>();
        var dialog = dialogs.FirstOrDefault(d => d.GetVariable(DIALOGID).Value == dialogId);
        return dialog;
    }

    private bool IsDialogAlreadyOpen(UAValue dialogId)
    {
        return Session.Get("UIRoot").Children.OfType<Dialog>().Any(d => d.GetVariable(DIALOGID).Value == dialogId);
    }

    private int[] GetOpenDialogIds() => synchedDialogIDs.Value.Value as int[];

    private void AddDialogIdToDialogsArray(int dialogId)
    {
        int[] openDialogsIds = GetOpenDialogIds();

        int firstAvailableArrayElement = Array.IndexOf(openDialogsIds, emptyDialogIdsArrayElementIndex);
        openDialogsIds[firstAvailableArrayElement] = dialogId;

        synchedDialogIDs.Value = openDialogsIds;
        openDialogsIds = RemoveDuplicatesAndMark(openDialogsIds);
        UICommands.OpenDialog(Owner, GetDialogById(dialogId));
    }

    private void RemoveDialogIdToDialogsArray(int dialogId)
    {
        int[] openDialogsIds = GetOpenDialogIds();
        openDialogsIds = RemoveDuplicatesAndMark(openDialogsIds);

        int indexToRemove = Array.IndexOf(openDialogsIds, dialogId);
        if (indexToRemove != -1)
        {
            openDialogsIds[indexToRemove] = emptyDialogIdsArrayElementIndex;
        }

        synchedDialogIDs.Value = openDialogsIds;
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


}

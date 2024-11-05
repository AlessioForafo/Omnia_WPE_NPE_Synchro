#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Linq;
using FTOptix.Alarm;
using FTOptix.WebUI;
using FTOptix.S7TiaProfinet;
using FTOptix.CommunicationDriver;
using System.Collections.Generic;
#endregion

public class ScreenSessionSynch : BaseNetLogic
{
    private PanelLoader myPanelLoader;
    private IUAVariable synchedScreenID;
    private IUAVariable SynchScreenWithAllSessions;
    private List<ScreenType> projectScreens;
    private const string SCREENID = "ScreenID";

    public override void Start()
    {
        myPanelLoader = (PanelLoader)Owner;
        synchedScreenID = LogicObject.GetVariable("SynchedScreenID");
        SynchScreenWithAllSessions = LogicObject.GetVariable("SynchScreenWithAllSessions");
        projectScreens = Project.Current.Get("UI").FindNodesByType<ScreenType>().ToList();

        if (SynchScreenWithAllSessions.Value) SynchedScreenIDUpdated();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void SynchedScreenIDUpdated()
    {
        if (!SynchScreenWithAllSessions.Value) return;
        LoadSynchedScreen(projectScreens);
    }

    private void LoadSynchedScreen(System.Collections.Generic.IEnumerable<ScreenType> screens)
    {
        foreach (ScreenType s in screens)
        {
            var screenID = s.GetVariable(SCREENID).Value;
            if (screenID == null || screenID == 0) continue;

            if (screenID == synchedScreenID.Value)
            {
                myPanelLoader.ChangePanel(s);
                return;
            }
        }
    }
}

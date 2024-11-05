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
using System.ComponentModel.Design;
using FTOptix.S7TiaProfinet;
using FTOptix.CommunicationDriver;
#endregion

public class SynchScreenID : BaseNetLogic
{
    public override void Start()
    {
        LogicObject.GetVariable("ModelActualScreenID").Value = LogicObject.GetVariable("ScreenID").Value;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}

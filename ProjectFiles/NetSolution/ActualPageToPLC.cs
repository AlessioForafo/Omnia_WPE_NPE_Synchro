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

public class ActualPageToPLC : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        var numPagina = Owner.GetVariable("ID_Pagina");
        if (numPagina != null)
        {
            var pageToPLC = LogicObject.GetVariable("Model_Page_ID");
            pageToPLC.Value = numPagina.Value;
        }
        else
            Log.Warning("ID Pagina non presente nello screen: " + Owner.BrowseName);

               
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}

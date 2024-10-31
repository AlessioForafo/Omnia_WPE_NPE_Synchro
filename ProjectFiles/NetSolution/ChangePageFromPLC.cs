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
#endregion

public class ChangePageFromPLC : BaseNetLogic
{
    PanelLoader myPanelLoader;
    IUAVariable pageFromPLC;    

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        myPanelLoader = (PanelLoader)Owner;
        pageFromPLC = LogicObject.GetVariable("Model_Page_ID");     
        pageFromPLC.VariableChange += PageFromPLC_VariableChange;
        CercaID();
      
    }    

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        pageFromPLC.VariableChange -= PageFromPLC_VariableChange;       
    }

    private void PageFromPLC_VariableChange(object sender, VariableChangeEventArgs e)
    {
        CercaID();
    }

    public void CercaID()
    {
        var UIFolder = Project.Current.Get<Folder>("UI");
        SearchID(UIFolder);
        
    }
    private void SearchID(IUANode obj)
    {
        foreach (var item in obj.Children)
        {
            if (item is Folder)
            {
                SearchID(item);
            }
            else if (item is ScreenType)
            {
                //Log.Info(item.BrowseName);
                var indice = item.GetVariable("ID_Pagina");
                if (indice != null && indice.Value != 0) 
                {
                    if (indice.Value == pageFromPLC.Value)
                    {
                        myPanelLoader.ChangePanel(item.BrowseName);
                        return;
                    }                     
                }
            }   
        }
    }

}

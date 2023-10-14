using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using Battlehub.RTSL.Interface;
using Battlehub.RTEditor.ViewModels;
using SFB;
[Binding]
public class ManageProjectCreater : HierarchicalDataViewModel<ProjectInfo>
{

    public ManageProjectsViewModel MPVM;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [Binding]
    public virtual void OnCreateProject()
    {

        string targetPath = StandaloneFileBrowser.SaveFilePanel("建立新專案", "", "", "");
        if(MPVM != null)
        {
            MPVM.doCreateProject(targetPath);
        }
    }
}

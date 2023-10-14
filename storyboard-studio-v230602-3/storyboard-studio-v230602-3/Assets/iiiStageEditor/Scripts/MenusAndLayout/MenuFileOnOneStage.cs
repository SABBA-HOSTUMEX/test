using Battlehub.UIControls.MenuControl;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTSL.Interface;
using iiiStoryEditor.UI.ViewModels;
using iiiStoryEditor.UI.Views;
using UnityEngine.SceneManagement;
using Battlehub.RTEditor.Models;

using System.Collections.Generic;

using SFB;

[MenuDefinition]
public class MenuFileOnOneStage : MonoBehaviour
{

    private IImporterModel m_importerModel;

    private IRuntimeEditor Editor
    {
        get { return IOC.Resolve<IRuntimeEditor>(); }
    }
    [MenuCommand("MenuFile/Import From File", "RTE_Dialog_ImportFile", priority: 50)]
    public void OverrideImportFromFile()
    {
        // Editor.CreateOrActivateWindow(RuntimeWindowType.ImportFile.ToString());


        //List<string> Extensions = m_importerModel.Extensions.ToList();


        TryImport();
    }

    protected async void TryImport()
    {

        m_importerModel = IOC.Resolve<IImporterModel>();
        IRTE rte = IOC.Resolve<IRTE>();

        rte.IsBusy = true;

        ExtensionFilter ImagesFiles = new ExtensionFilter("ImagesFiles", new string[] { "jpg", "png" });
        ExtensionFilter VideoFiles = new ExtensionFilter("VideoFiles", new string[] { "mp4" });
        ExtensionFilter AudioFiles = new ExtensionFilter("AudioFiles", new string[] { "wav" });
        ExtensionFilter ModelFiles = new ExtensionFilter("ModelFiles", new string[] { "glb", "gltf", "fbx", "vrm" });

        string[] path = StandaloneFileBrowser.OpenFilePanel("選擇影片檔案", "", new ExtensionFilter[] { ImagesFiles, VideoFiles, AudioFiles, ModelFiles }, false);




        if (path != null && path.Length > 0 && path[0] != null)
        {
            string Path = path[0];
            await m_importerModel.ImportAsync(Path, System.IO.Path.GetExtension(Path));
        }


        rte.IsBusy = false;

    }
}

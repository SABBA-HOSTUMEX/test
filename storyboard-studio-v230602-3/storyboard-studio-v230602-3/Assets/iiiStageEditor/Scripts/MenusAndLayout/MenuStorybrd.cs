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
    public class MenuStorybrd : MonoBehaviour
    {
        public StoryBoardNodeEditor nodeEditor;


        private IImporterModel m_importerModel;


        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuFile/New Scene", "建新故事板", priority: 10)]
        public void OverridenNewScene()
        {
            Editor.NewScene();
        }
        /*
        [MenuCommand("Example/Load Scene")]
        public void LoadScene()
        {
            m_wm.Prompt("Enter Scene Name", "My Scene", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                Editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    if (!project.Utils.Exist<Scene>(args.Text))
                    {
                        await Task.Yield();
                        m_wm.MessageBox("Unable to load scene", $"{args.Text} was not found");
                    }
                    else
                    {
                        await project.LoadAsync(args.Text, typeof(Scene));
                    }
                }

                Editor.IsBusy = false;
            });
        }
        */

        [MenuCommand("MenuFile/Save Scene", "儲存故事板", priority: 20)]
        public void OverridenSaveScene()
        {

            RuntimeWindow window = Editor.GetWindow(RuntimeWindowType.Storyboard);
            StoryBoardNodeEditor NodeEditor = window.GetComponentInChildren<StoryBoardNodeEditor>();
            NodeEditor.SaveGraph();
            Editor.SaveScene();

            //NodeEditor.SaveGraph(NodeEditor._savePath);
        }
        
        [MenuCommand("MenuFile/Save Scene As...", "另存故事板", priority: 30)]
        public void OverridenSaveSceneAs()
    {

            RuntimeWindow window = Editor.GetWindow(RuntimeWindowType.Storyboard);
            StoryBoardNodeEditor NodeEditor = window.GetComponentInChildren<StoryBoardNodeEditor>();
            NodeEditor.SaveGraph();
            Editor.CreateOrActivateWindow(RuntimeWindowType.SaveScene.ToString());
        }
        /*
        [MenuCommand("MenuFile/Import Assets", "RTE_Dialog_Import", priority: 40)]
        public void OverrideImportAssets()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.SelectAssetLibrary.ToString());
        }
        */



        [MenuCommand("MenuFile/Import Assets", requiresInstance: true, hide: true, priority: 40)]
        public void OverrideImportAssets() { }


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
            ExtensionFilter VideoFiles  = new ExtensionFilter("VideoFiles", new string[] {  "mp4" });
            ExtensionFilter AudioFiles = new ExtensionFilter("AudioFiles", new string[] {"wav" });
            ExtensionFilter ModelFiles = new ExtensionFilter("ModelFiles", new string[] { "glb", "gltf" ,"fbx","vrm" });

            string[] path = StandaloneFileBrowser.OpenFilePanel("選擇影片檔案", "", new ExtensionFilter[] { ImagesFiles, VideoFiles, AudioFiles, ModelFiles }, false);

            if (path != null && path.Length > 0 && path[0] != null)
            {
                string Path = path[0];
                await m_importerModel.ImportAsync(Path, System.IO.Path.GetExtension(Path));
            }


            rte.IsBusy = false;

        }


    [MenuCommand("MenuFile/Export Scene File",requiresInstance: true, hide: true, priority: 60)]
        public void OverrideExportSceneFile(){}


        [MenuCommand("MenuFile/Close", requiresInstance: true, hide: true, priority: 80)]
        public void OverridenCloseButton() { }

    /*
        [MenuCommand("MenuFile/Close")]
        public void OverridenCloseButton()
        {

            Debug.Log("Close2");
            Editor.Close();
            IOC.ClearAll();
            Application.LoadLevel(0);

        }
    */

}
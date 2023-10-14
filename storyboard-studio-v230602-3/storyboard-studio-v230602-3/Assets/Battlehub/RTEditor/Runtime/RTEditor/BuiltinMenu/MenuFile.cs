using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTEditor
{
    [MenuDefinition(order:-90)]
    public class MenuFile : MonoBehaviour
    {
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuFile/New Scene", "RTE_NewScene", priority: 10)]
        public void NewScene()
        {

            Editor.CreateOrActivateWindow(RuntimeWindowType.NewScene.ToString());
            //Editor.NewScene();
        }

        [MenuCommand("MenuFile/Save Scene", "RTE_Save", priority: 20)]
        public void SaveScene()
        {
            Editor.SaveScene();
        }

        [MenuCommand("MenuFile/Save Scene As...", "RTE_Dialog_SaveAs", priority: 30)]
        public void SaveSceneAs()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.SaveScene.ToString());
        }

        [MenuCommand("MenuFile/Import Assets", "RTE_Dialog_Import", priority:40)]
        public void ImportAssets()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.SelectAssetLibrary.ToString());
        }

        [MenuCommand("MenuFile/Import From File", "RTE_Dialog_ImportFile", priority:50)]
        public void ImportFromFile()
        {
           // Editor.CreateOrActivateWindow(RuntimeWindowType.ImportFile.ToString());
        }

        //liwei edit { 
        //[MenuCommand("MenuFile/Export Scene File", "RTE_Dialog_Export", priority:60)]
        //public void ExportSceneFile()
        //{
        //    Editor.CreateOrActivateWindow(RuntimeWindowType.ExportFile.ToString());
        //}
        // } liwei edit

        [MenuCommand("MenuFile/Manage Projects", "RTE_Dialog_OpenProject", priority:70)]
        public void ManageProjects()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.OpenProject.ToString());
        }
        [MenuCommand("MenuFile/Close", requiresInstance: true, hide: true, priority: 80)]
        public void Close()
        {

            Debug.Log("Close1");
            /*
            Editor.Close();
            IOC.ClearAll();
            Application.LoadLevel(0);
            //SceneManager.LoadScene(0, LoadSceneMode.Single);
            //Application.Quit();
            Restart();
            */

        }
        public  void Restart()
        {
            string[] endings = new string[]{
                 "exe", "x86", "x86_64", "app"
            };
            string executablePath = Application.dataPath + "/..";
            foreach (string file in System.IO.Directory.GetFiles(executablePath))
            {
                foreach (string ending in endings)
                {
                    if (file.ToLower().EndsWith("." + ending))
                    {
                        System.Diagnostics.Process.Start(executablePath + file);
                        Application.Quit();
                        return;
                    }
                }

            }
        }
    }
}



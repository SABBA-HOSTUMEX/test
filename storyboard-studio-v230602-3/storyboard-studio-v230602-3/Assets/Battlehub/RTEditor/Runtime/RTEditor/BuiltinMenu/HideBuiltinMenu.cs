using Battlehub.UIControls.MenuControl;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;

namespace Battlehub.RTEditor
{
    [MenuDefinition]
    public class HideBuiltinMenu : MonoBehaviour
    {
        /*
        [MenuCommand("MenuFile", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuFile() { }
        */

        [MenuCommand("MenuEdit", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuEdit() { }

        /*
        [MenuCommand("MenuWindow", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuWindow() { }

        
        [MenuCommand("MenuHelp", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuAbout() { }
         
        [MenuCommand("MenuGameObject", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuGameObject() { }
        */


        [MenuCommand("MenuFile/Close")]
        public void OverridenCloseButton()
        {

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else


#if UNITY_STANDALONE_OSX
     System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".app"));
    Application.Quit();                               

#else

    System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); //new program
    Application.Quit(); 
#endif
                                        
#endif

        }

        public void restart()
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

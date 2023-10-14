using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace iiiStoryEditor.UI
{
    [MenuDefinition]
    public class RegisterStoryboardWindow : EditorExtension
    {
        [SerializeField]
        private GameObject m_prefab = null;

        [SerializeField]
        private Sprite m_icon = null;
		
        [SerializeField]
        private string m_header = "Storyboard";

        [SerializeField]
        private bool m_isDialog = false;
		
        [SerializeField]
        private int m_maxWindows = -1;

        protected override void OnInit()
        {
            base.OnInit();

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.RegisterWindow("Storyboard", m_header, m_icon, m_prefab, m_isDialog, m_maxWindows);
        }

        [MenuCommand("MenuWindow/Storyboard", "")]
        public void Open()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("Storyboard");
        }
    }
}




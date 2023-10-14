using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using Battlehub;

namespace Battlehub.RTEditor
{
    [MenuDefinition(order: -30)]
    public class MenuLang : MonoBehaviour
    {
        public Localization localization;
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuLang/中文", "RTE_Lang", priority: 10)]
        public void SetLangZHTW()
        {
            localization.LoadStringResources("RTEditor.StringResources");
        }
        [MenuCommand("MenuLang/English", "RTE_Lang", priority:20)]
        public void SetLangENUS()
        {
        }

    }
}



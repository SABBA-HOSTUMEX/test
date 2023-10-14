
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

public class UIScaleOverride : EditorExtension
{
    [SerializeField]
    private float Scale = 1.125f;

    protected override void OnEditorExist()
    {
        ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
        settings.UIScale = Scale;
    }
}
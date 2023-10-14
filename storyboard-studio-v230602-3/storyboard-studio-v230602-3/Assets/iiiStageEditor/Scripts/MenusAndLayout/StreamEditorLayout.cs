using Battlehub.UIControls.DockPanels;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using UnityEngine;

public class StreamEditorLayout : LayoutExtension
{
    protected override void OnBeforeBuildLayout(IWindowManager wm)
    {
        //Hide header toolbar
        //wm.OverrideTools(null);
    }
    protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
    {

        LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
        scene.IsHeaderVisible = true;


        LayoutInfo StoryboardRes = wm.CreateLayoutInfo(RuntimeWindowType.StoryboardResources.ToString());
        //LayoutInfo Animation = wm.CreateLayoutInfo(BuiltInWindowNames.Animation);

        LayoutInfo Game = wm.CreateLayoutInfo(BuiltInWindowNames.Game);
        LayoutInfo Streaming = wm.CreateLayoutInfo(RuntimeWindowType.Empty.ToString() );
        
        LayoutInfo layoutRoot = LayoutInfo.Horizontal(StoryboardRes, scene, ratio: .1f);

        LayoutInfo GameExport = LayoutInfo.Vertical(Game, Streaming, ratio: .4f);
        
        
        LayoutInfo StreamPanel_Storyboard = wm.CreateLayoutInfo(RuntimeWindowType.Storyboard.ToString() );
        LayoutInfo StreamPanel_Audio = wm.CreateLayoutInfo(RuntimeWindowType.StreamingAudio.ToString() );
        LayoutInfo StreamPanel_SreamingExport = wm.CreateLayoutInfo(RuntimeWindowType.SreamingExport.ToString() );


        LayoutInfo StreamPanel_Left = LayoutInfo.Horizontal(StreamPanel_Storyboard, StreamPanel_Audio, ratio: .5f);

        LayoutInfo StreamPanel = LayoutInfo.Horizontal(StreamPanel_Left, StreamPanel_SreamingExport, ratio: .8f);


        LayoutInfo layoutRootWithGameTop = LayoutInfo.Horizontal(layoutRoot, GameExport, ratio: .9f);
        LayoutInfo layoutRootWithGamFull = LayoutInfo.Vertical(layoutRootWithGameTop, StreamPanel, ratio: .7f);

        //wm.CreateWindow(BuiltInWindowNames.Game, true);

        return layoutRootWithGamFull;

    }
}

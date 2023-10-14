using Battlehub.UIControls.DockPanels;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using UnityEngine;

public class StoryboardLayout : LayoutExtension
{
    protected override void OnBeforeBuildLayout(IWindowManager wm)
    {
        //Hide header toolbar
        //wm.OverrideTools(null);
    }
    protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
    {

        LayoutInfo Story = wm.CreateLayoutInfo(RuntimeWindowType.Storyboard.ToString());
        Story.IsHeaderVisible = true;

       // LayoutInfo Scene = wm.CreateLayoutInfo(RuntimeWindowType.Scene.ToString());


        LayoutInfo StoryboardRes = wm.CreateLayoutInfo(RuntimeWindowType.StoryboardResources.ToString());
        //LayoutInfo Animation = wm.CreateLayoutInfo(BuiltInWindowNames.Animation);

        LayoutInfo Game = wm.CreateLayoutInfo(BuiltInWindowNames.Game);
        LayoutInfo Export = wm.CreateLayoutInfo(RuntimeWindowType.StoryExport.ToString() );
        /*
        LayoutInfo[] sceneAndGame = new LayoutInfo[2];
        sceneAndGame[0] = Game;
        sceneAndGame[1] = scene;
        */


        //LayoutInfo ScStory = LayoutInfo.Vertical(Story, Scene, ratio: .8f);

        LayoutInfo layoutRoot = LayoutInfo.Horizontal(StoryboardRes, Story, ratio: .1f);

        LayoutInfo GameExport = LayoutInfo.Vertical(Game, Export, ratio: .1f);
        LayoutInfo layoutRootWithGame = LayoutInfo.Horizontal(layoutRoot, GameExport, ratio: .9f);




        //wm.CreateWindow(BuiltInWindowNames.Game, true);

        return layoutRootWithGame;

    }
}

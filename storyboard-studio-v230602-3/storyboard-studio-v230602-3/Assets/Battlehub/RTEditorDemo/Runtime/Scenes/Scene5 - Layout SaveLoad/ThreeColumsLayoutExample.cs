using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene5
{
    /// <summary>
    /// Creates default layout for this example (inspector, (scene, hierarchy))
    /// </summary>
    public class ThreeColumsLayoutExample : LayoutExtension
    { 
        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            //wm.OverrideTools(null);
        }
        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            
            LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            LayoutInfo AnimationEditor = wm.CreateLayoutInfo("AnimationEditor");
            scene.IsHeaderVisible = true;

            LayoutInfo ScAnimation = LayoutInfo.Vertical(scene, AnimationEditor, ratio: .8f);



            LayoutInfo hierarchy = wm.CreateLayoutInfo(BuiltInWindowNames.Hierarchy);

            LayoutInfo inspector = wm.CreateLayoutInfo(BuiltInWindowNames.Inspector);
            LayoutInfo Project = wm.CreateLayoutInfo(BuiltInWindowNames.Project);
            //LayoutInfo Animation = wm.CreateLayoutInfo(BuiltInWindowNames.Animation);

            LayoutInfo Game = wm.CreateLayoutInfo(BuiltInWindowNames.Game);
            /*
            LayoutInfo[] sceneAndGame = new LayoutInfo[2];
            sceneAndGame[0] = Game;
            sceneAndGame[1] = scene;
            */

            inspector = LayoutInfo.Vertical(inspector, Game, ratio: .9f);


           // LayoutInfo ScAnimation = LayoutInfo.Vertical(scene, Animation, ratio: .8f);

            LayoutInfo HierarchyAssets = LayoutInfo.Vertical(hierarchy, Project, ratio: .6f);
            LayoutInfo sgAndHierarchy = LayoutInfo.Horizontal(HierarchyAssets, ScAnimation, ratio: .15f);
            LayoutInfo layoutRoot = LayoutInfo.Horizontal(sgAndHierarchy, inspector, ratio: .85f);


            return layoutRoot;
            
        }
    }

}

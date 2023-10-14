using Battlehub.RTEditor.Views;
using UnityEngine;
using RuntimeNodeEditor;
using Battlehub.UIControls;
using System;
using UnityEngine.Events;
namespace iiiStoryEditor.UI.Views
{
    public class StoryboardView : View
    {

        public RectTransform editorHolder;
        public StoryBoardNodeEditor editor;

        protected override void Start()
        {
            base.Start();
            Application.targetFrameRate = 60;
            var graph = editor.CreateGraph<NodeGraph>(editorHolder);
            editor.StartEditor(graph);
        }

        [NonSerialized]
        public UnityEvent PlayChanged = new UnityEvent();
    }
}



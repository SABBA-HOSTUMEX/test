using Battlehub.RTEditor.ViewModels;
using Battlehub.RTCommon;
using UnityEngine;
using UnityWeld.Binding;

namespace iiiStoryEditor.UI.ViewModels
{
    [DefaultExecutionOrder(-1)]
    [Binding]
    public class StoryboardToolViewModel : ViewModel
    {

        private bool m_handleValueChange;

        [Binding]
        public virtual bool IsView
        {
            get { return Editor.Tools.Current == RuntimeTool.View; }
            set
            {
                if (value)
                {
                    Editor.Tools.Current = RuntimeTool.View;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsPlay
        {
            get { return Editor.IsPlaying; }
            set
            {
                Editor.IsPlaying = value;
                RaisePropertyChanged(nameof(IsPlay));
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            Editor.Tools.ToolChanged += OnRuntimeToolChanged;

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
            Editor.Undo.StateChanged += OnStateChanged;

            Editor.SceneLoaded += OnSceneLoaded;
            Editor.SceneSaved += OnSceneSaved;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Editor != null)
            {
                Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                Editor.Undo.StateChanged -= OnStateChanged;
                Editor.SceneLoaded -= OnSceneLoaded;
                Editor.SceneSaved -= OnSceneSaved;
            }
        }

        protected void UpdateTogglesState()
        {
            RaisePropertyChanged(nameof(IsView));
        }

        protected virtual void UpdateUndoRedoButtonsState()
        {
           
        }


        protected virtual void OnPlaymodeStateChanged()
        {
            RaisePropertyChanged(nameof(IsPlay));
        }

        protected virtual void OnRuntimeToolChanged()
        {
            UpdateTogglesState();
        }

        protected virtual void OnStateChanged()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnRedoCompleted()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnUndoCompleted()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnSceneSaved()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnSceneLoaded()
        {
            UpdateUndoRedoButtonsState();
        }

    }
}



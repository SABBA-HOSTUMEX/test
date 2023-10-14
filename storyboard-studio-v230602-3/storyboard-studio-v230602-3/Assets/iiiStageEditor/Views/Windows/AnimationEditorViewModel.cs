using Battlehub.RTCommon;
using System;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTEditor;

using UnityObject = UnityEngine.Object;

namespace iiiStoryEditor.UI.ViewModels
{


    [Binding]
    public class AnimationEditorViewModel : ViewModel
    {

        private UnityObject[] m_selectedObjects;
        [Binding]
        public UnityObject[] SelectedObjects
        {
            get { return m_selectedObjects; }
            set
            {
                if (m_selectedObjects != value)
                {
                    UnityObject[] unselectedObjects = m_selectedObjects;
                    m_selectedObjects = value;
                    OnSelectedObjectsChanged(unselectedObjects, m_selectedObjects);
                }
            }
        }



        protected GameObject SelectedGameObject
        {
            get { return SelectedObject as GameObject; }
        }

        protected UnityObject SelectedObject
        {
            get { return m_selectedObjects != null && m_selectedObjects.Length > 0 ? m_selectedObjects[0] : null; }
        }


        public PoseAniEditor AniEditor;

        protected override void Start()
        {
            base.Start();
            AniEditor.gameObject.SetActive(false);//liwei edit for hide ani
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            base.OnDisable();



        }

        protected virtual void Update()
        {
            UnityObject obj = SelectedObject;
        }

        #region Bound UnityEvents

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            OnSave();
        }

        public virtual void OnSave()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsDirty && SelectedObjects != null && SelectedObjects.Length > 0)
            {
                editor.IsDirty = false;
                editor.IsBusy = true;
                editor.SaveAssets(SelectedObjects, result =>
                {
                    editor.IsBusy = false;
                });
            }
        }

        #endregion

        #region Methods

        private void OnSelectedThemeChanged(object sender, ThemeAsset oldValue, ThemeAsset newValue)
        {
        }

        protected virtual void OnSelectedObjectsChanged(UnityObject[] unselectedObjects, UnityObject[] selectedObjects)
        {


            Type objType;
            if (selectedObjects.Length == 0 ||  !OfSameType(selectedObjects, out objType))
            {
                return;
            }

            ExposeToEditor exposeToEditor = null;
            if (objType == typeof(GameObject))
            {
                exposeToEditor = SelectedGameObject.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null && !exposeToEditor.CanInspect)
                {
                    return;
                }
                StoryCharAnimObject CharObj = SelectedGameObject.GetComponent<StoryCharAnimObject>();
                StoryCanAnimObject AniObj = SelectedGameObject.GetComponent<StoryCanAnimObject>();
                if (CharObj != null)
                {

                    AniEditor.initWithEditor(CharObj);
                }
                else if(AniObj != null)
                {
                    AniEditor.initWithEditor(AniObj);
                }
            }
        }
        protected bool OfSameType(UnityObject[] objects, out Type type)
        {
            type = objects[0].GetType();
            for (int i = 1; i < objects.Length; ++i)
            {
                if (type != objects[i].GetType())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}

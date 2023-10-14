using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Battlehub.RTEditor.Views;

namespace iiiStoryEditor.UI.Views
{
    public class StoryboardToolView : View
    {
        [SerializeField]
        private Toggle m_viewToggle = null;

        [SerializeField]
        private Toggle m_playToggle = null;


        [NonSerialized]
        public UnityEvent ViewChanged = new UnityEvent();
        public bool IsView
        {
            get { return m_viewToggle.isOn; }
            set { m_viewToggle.isOn = value; }
        }
        [NonSerialized]
        public UnityEvent PlayChanged = new UnityEvent();
        public bool IsPlay
        {
            get { return m_playToggle.isOn; }
            set { m_playToggle.isOn = value; }
        }


        protected override void Awake()
        {
            base.Awake();
            UnityEventHelper.AddListener(m_viewToggle, tog => tog.onValueChanged, OnViewChanged);
            UnityEventHelper.AddListener(m_playToggle, tog => tog.onValueChanged, OnPlayChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnityEventHelper.RemoveListener(m_viewToggle, tog => tog.onValueChanged, OnViewChanged);
            UnityEventHelper.RemoveListener(m_playToggle, tog => tog.onValueChanged, OnPlayChanged);

        }

        private void OnViewChanged(bool value)
        {
            ViewChanged?.Invoke();
        }


        private void OnPlayChanged(bool value)
        {
            PlayChanged?.Invoke();
        }

    }
}



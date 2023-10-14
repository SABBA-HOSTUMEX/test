﻿using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public delegate void RectTransformChanged();

    public class RectTransformChangeListener : UIBehaviour
    {
        public event RectTransformChanged RectTransformChanged;

        protected override void Start()
        {
            base.Start();
            RaiseRectTransformChanged();
        }
        protected override void OnRectTransformDimensionsChange()
        {
            RaiseRectTransformChanged();
        }

        public void RaiseRectTransformChanged()
        {
            if (RectTransformChanged != null)
            {
                RectTransformChanged();
            }
        }
    }

}

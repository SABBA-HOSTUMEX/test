using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Battlehub.UIControls.MenuControl;

namespace Battlehub.RTEditor.Views
{
    public class SceneView : View
    {
        [NonSerialized]
        public UnityEvent PointerChanged = new UnityEvent();
        public Ray Pointer
        {
            get;
            set;
        }

        [NonSerialized]
        public UnityEvent CameraTransformChanged = new UnityEvent();
        public Transform CameraTransform
        {
            get;
            set;
        }

        public Menu m_menu = null;
        public Vector3 lastHitpoint;
        public GameObject HotSpotsGo;
        public GameObject MarkerGo;

        protected override void Awake()
        {
            base.Awake();
            Window.ActivateOnAnyKey = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CameraTransform = null;
        }

        protected override void OnDragEnter(PointerEventData pointerEventData)
        {
            Pointer = Window.Pointer;
            PointerChanged?.Invoke();

            CameraTransform = Window.Camera.transform;
            CameraTransformChanged?.Invoke();

            base.OnDragEnter(pointerEventData);

            if (CanDropExternalObjects)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        protected override void OnDrag(PointerEventData pointerEventData)
        {
            Pointer = Window.Pointer;
            PointerChanged?.Invoke();

            base.OnDrag(pointerEventData);

            if (CanDropExternalObjects)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Vector3 position;
                Vector2 pos = Input.mousePosition;


                RaycastHit hit;
                Ray ray = Window.Camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 300.0f))
                {
                    lastHitpoint = hit.point;
                    //Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
                }


                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, pos, canvas.worldCamera))
                {
                    return;
                }
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, pos, canvas.worldCamera, out position))
                {
                    m_menu.transform.position = position;
                    m_menu.Open();
                }
            }

            ViewInput.HandleInput();
        }


        public void ClickMenu(string mess)
        {
            if(lastHitpoint != null)
            {

                if (mess == "Create Hotspot")
                {
                    GameObject hot = GameObject.Instantiate(HotSpotsGo);
                    hot.transform.position = lastHitpoint;
                }
                else if (mess == "Create Marker")
                {
                    GameObject marker = GameObject.Instantiate(MarkerGo);
                    marker.transform.position = lastHitpoint;
                }
            }


        }
    }

}

using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneDragDropHandler : MonoBehaviour
{
    IDragDrop m_dragDrop;
    RuntimeWindow m_target;

    void Start()
    {
        m_target = IOC.Resolve<IRTE>().GetWindow(RuntimeWindowType.Scene);
        m_dragDrop = IOC.Resolve<IRTE>().DragDrop;
        //m_dragDrop.Drop += OnDrop;
        m_dragDrop.Drag += OnDrag;
    }

    void OnDestroy()
    {
        if (m_dragDrop != null)
        {
            m_dragDrop.Drop -= OnDrop;
        }
    }

    void OnDrop(PointerEventData pointerEventData)
    {
        if (m_target != null && m_target.IsPointerOver)
        {
         //   Debug.Log(m_dragDrop.DragObjects[0]);
        }
    }


    void OnDrag(PointerEventData pointerEventData)
    {
        if (m_target != null && m_target.IsPointerOver)
        {
         //  Debug.Log(m_dragDrop.DragObjects[0]);
        }
    }
}

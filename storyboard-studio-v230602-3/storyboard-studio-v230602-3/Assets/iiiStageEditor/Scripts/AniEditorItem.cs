using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
public class AniEditorItem : MonoBehaviour
{
    public int frame ;
    public float onSec;
    public TMP_InputField m_frameInputField;
    // Start is called before the first frame update
    public Transform m_transform;
    public List<OnAniFrameTrans> transData;
    public List<GameObject> Points;
    private bool isActive = false;
    private PoseAniEditor PoseEditor;
    public GameObject ActiveBBG;
    public GameObject PointPrefab;
    public GameObject PointRoot;

    public GameObject delbtn;

    void Start()
    {
        if(m_frameInputField != null)
        {
            m_frameInputField.text = Math.Round(onSec, 2).ToString();
        }

        GetPoseEditor();
        SetOnDisactive();
    }

    public PoseAniEditor GetPoseEditor()
    {
        GameObject go = GameObject.FindGameObjectWithTag("AniPoseEditor");

        if (go != null)
        {
            PoseEditor = go.GetComponent<PoseAniEditor>();
        }
        return PoseEditor;
    }
    public void SetMeCurrItem()
    {
        //Debug.Log("SetMeCurrItem");
        GetPoseEditor().SetCurrentEditItem(this);
    }
    public void SetOnActive()
    {
        isActive = true;
        ActiveBBG.SetActive(isActive);
        updateActive();
    }
    public void SetOnDisactive()
    {
        isActive = false;
        ActiveBBG.SetActive(isActive);
        updateActive();
    }
    public void updateActive()
    {
        if (delbtn != null)
        {
            delbtn.SetActive(isActive);
        }
    }
    public void setFrame(string frStr)
    {
        onSec = float.Parse(frStr);
        frame = Convert.ToInt32( onSec *30);
    }
    public void setTransData(Transform toptransform,string uiType = "")
    {
        m_transform = toptransform;
        ;
         
        List<OnAniFrameTrans> newFrameTransSet = new List<OnAniFrameTrans>();
        Points = new List<GameObject>();
        OnAniFrameTrans _oneFrameTrans;

        _oneFrameTrans = saveAniFrameTrans(toptransform);
        newFrameTransSet.Add(_oneFrameTrans);


        addTransPt(toptransform, newFrameTransSet);

        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);


        RectTransform rect = PointRoot.GetComponent<RectTransform>();
        if (uiType == "Inspector")
        {
            rect.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }
        else
        {
            rect.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }
       
        transData = newFrameTransSet;
    }
    public void addTransPt(Transform toptransform, List<OnAniFrameTrans> newFrameTransSet)
    {
        Transform transform;
        OnAniFrameTrans _oneFrameTrans;
        GameObject onePt;
        for (int i = 0; i < toptransform.childCount; i++)
        {
            transform = toptransform.GetChild(i);
            _oneFrameTrans = saveAniFrameTrans(transform);
            onePt = drawOnePoint(_oneFrameTrans, transform.gameObject.name);
            Points.Add(onePt);
            newFrameTransSet.Add(_oneFrameTrans);
            if(transform.childCount > 0)
            {
                addTransPt(transform, newFrameTransSet);
            }
        }
    }
    public OnAniFrameTrans saveAniFrameTrans(Transform transform)
    {
        OnAniFrameTrans _oneFrameTrans = new OnAniFrameTrans();
        _oneFrameTrans.m_lpos = transform.localPosition;
        _oneFrameTrans.m_pos = transform.position;
        _oneFrameTrans.m_scale = transform.localScale;
        _oneFrameTrans.m_rot = transform.rotation;
        _oneFrameTrans.m_lrot = transform.localRotation;
        _oneFrameTrans.m_langle = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);

        if (transform.name== "Earth")
        {
            /*
            Debug.Log(transform.localRotation);
            
            Debug.Log(_oneFrameTrans.m_lrot);
            Debug.Log(transform.localEulerAngles);

            Debug.Log(Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z));
            Debug.Log(Quaternion.Euler(0, 0, -51.836f));


            Debug.Log(_oneFrameTrans.m_langle);
            */
        }
        
        _oneFrameTrans.transform = transform;
        return _oneFrameTrans;
    }

    public void clickDel()
    {
        if(PoseEditor != null)
        {
            PoseEditor.clickDelOne();
        }
    }
    public void clickSave()
    {
        Transform transform;

        OnAniFrameTrans _oneFrameTrans;

        for (int i = 0; i < m_transform.childCount; i++)
        {
            transform = m_transform.GetChild(i);
            updateOnePoint(transform, Points[i]);

            _oneFrameTrans = transData[i+1];
            _oneFrameTrans.m_lpos = transform.localPosition;
            _oneFrameTrans.m_pos = transform.position;
            _oneFrameTrans.m_scale = transform.localScale;
            _oneFrameTrans.m_rot = transform.rotation;
            _oneFrameTrans.m_lrot = transform.localRotation;
            _oneFrameTrans.m_langle = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);

            Debug.Log(_oneFrameTrans.m_langle);
            _oneFrameTrans.transform = transform;

        }
    }
    public void updateOnePoint(Transform _transform, GameObject pt)
    {
        Vector3 ptpos = _transform.localPosition;
        ptpos.z = 0;
        ptpos.x = (ptpos.x * 30);
        ptpos.y = (ptpos.y * 30) ;

        pt.transform.localPosition = ptpos;
        pt.transform.localScale = new Vector3(.5f, .5f, .5f);
    }
    public GameObject drawOnePoint(OnAniFrameTrans _oneFrameTrans, string _name)
    {
        GameObject ptgo;
        OneAniEditPoint pt;
        Vector3 ptpos;
        if (PointPrefab != null)
        {
            ptgo = GameObject.Instantiate(PointPrefab);
            pt = ptgo.gameObject.GetComponent<OneAniEditPoint>();
            if(pt != null)
            {
                pt.target = _oneFrameTrans.transform;
            }
            ptpos = _oneFrameTrans.m_pos;
            ptpos.z = 0;
            ptpos.x = (ptpos.x * 30);
            ptpos.y = (ptpos.y * 30) ;

            ptgo.name = _name;

            ptgo.transform.position = ptpos;
            ptgo.transform.localScale = new Vector3(.5f, .5f, .5f);
            ptgo.transform.parent = transform;
            if(PointRoot != null)
            {
                ptgo.transform.parent = PointRoot.transform;

            }
            return ptgo;
        }
        return null;
        

    }
}

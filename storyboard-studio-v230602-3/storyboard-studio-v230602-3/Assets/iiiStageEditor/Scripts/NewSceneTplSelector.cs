using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.Dialogs;

using Battlehub.RTEditor.Models;

namespace iiiStoryEditor.UI.ViewModels
{
    public class NewSceneTplSelector : MonoBehaviour
    {
        public List<GameObject> BaseObjects = new List<GameObject>();
        public List<NewSceneTplItem> TplList = new List<NewSceneTplItem>();
        public int currentId = 0;
        // Start is called before the first frame update
        void Start()
        {
            ClickToSetTpl(0);
        }

        public void ClickToSetTpl(int id)
        {
            if (TplList.Count > id && TplList[id] != null)
            {

                for (int i = 0; i < TplList.Count; i++)
                {
                    if (i == id)
                    {
                        TplList[i].OnSelected();
                    }
                    else
                    {
                        TplList[i].OnDisSelected();
                    }
                }
                currentId = id;
            }
        }
        public void ClickSelectTemplate()
        {

            IRuntimeEditor Editor = IOC.Resolve<IRuntimeEditor>();
            Editor.NewScene(false);
            StartCoroutine("LoadObjectToNewScene");

        }
        IEnumerator LoadObjectToNewScene()
        {
            yield return new WaitForSeconds(.5f);
            if (TplList[currentId])
            {
                IPlacementModel placement = IOC.Resolve<IPlacementModel>();
                IRuntimeEditor Editor = IOC.Resolve<IRuntimeEditor>();
                GameObject SceneObj;
                if (TplList[currentId].ScenePrefab != null)
                {
                    SceneObj = GameObject.Instantiate(TplList[currentId].ScenePrefab);
                    if (SceneObj != null)
                    {
                        placement.AddGameObjectToScene(SceneObj);
                    }


                    if (BaseObjects.Count > 0)
                    {
                        GameObject addObj;// = GameObject.Instantiate(TplList[currentId].ScenePrefab);
                        bool already = false;
                        for (int i = 0; i < BaseObjects.Count; i++)
                        {
                            if (SceneObj != null)
                            {
                                if(SceneObj.transform.Find(BaseObjects[i].name) == null)
                                {
                                    addObj = GameObject.Instantiate(BaseObjects[i]);
                                    placement.AddGameObjectToScene(addObj);
                                }
                            }
                                
                        }
                    }

                }

                
               
                Dialog _dialog = gameObject.GetComponentInParent<Dialog>();
                if (_dialog != null)
                {
                    _dialog.Close();
                }


            }
        }

    }
}


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Battlehub.RTCommon;
    using Battlehub.UIControls.MenuControl;
    using UnityEngine;
    using Battlehub.RTSL.Interface;

    using UnityEngine.UI;
    using Battlehub.RTEditor;
    using Battlehub;
    using RootMotion.FinalIK;
    using System;
    using UnityObject = UnityEngine.Object;
    using SFB;
    using TMPro;
    
    public class AniObjComponentEditor : Battlehub.RTEditor.Views.View
    {
        public enum EditState{
            Ready,Animation
        }

        private ILocalization m_localization;
        public Battlehub.RTEditor.ComponentEditor compeditor;
        private ExposeToEditor Target;
        bool openAnimtion = false;

        private Battlehub.RTEditor.Views.AnimationView animationView;

        public GameObject AudioPanel = null;
        public GameObject MeshTexPanel = null;
        public GameObject AniPanel = null;
        public GameObject AniSettingPanel = null;
        public GameObject AniTextPanel = null;
        public GameObject stageLinksPanel = null;

        public bool isCharacter = false;


        public EditState editState = EditState.Ready;

        public StoryCanAnimObject aniObject;
        public StoryCharAnimObject charAniObject;

        public AnimationClip[] animations;


        public Slider TimeSlider;
        public TMP_Text timeTipsTxt;


        public TMP_InputField title_text;
        public TMP_InputField desc_text;
        public TMP_InputField link_text;
        public UnityEngine.UI.Toggle link_target;


        public TMP_InputField link_text_top;
        public TMP_InputField link_text_left;
        public TMP_InputField link_text_right;
        public TMP_InputField link_text_down;



    //public int OnSetAniID = 0;
    //public float OnSetAniTime = 0;

    private IEnumerator genAniCoroutine;

        public PoseAniEditor AniEditor;

        //public Dictionary<>
        private Battlehub.RTEditor.IRuntimeEditor Editor
        {
            get { return IOC.Resolve<Battlehub.RTEditor.IRuntimeEditor>(); }
        }
        // Start is called before the first frame update
        void Start()
        {
            m_localization = IOC.Resolve<ILocalization>();
            if (compeditor != null)
            {


                if (compeditor.Component.gameObject.GetComponent<ExposeToEditor>() != null)
                {
                    Target = compeditor.Component.gameObject.GetComponent<ExposeToEditor>();
                }

                closeObjectAnimEdit();
            }
            /*
             * 
             RuntimeWindow AniWin = Editor.GetWindow(RuntimeWindowType.Animation);
            if (AniWin != null)
            {
                animationView = AniWin.GetComponentInChildren<Battlehub.RTEditor.Views.AnimationView>();
            }
             */
            if (Target != null && Target.gameObject != null)
            {
                aniObject = Target.gameObject.GetComponent<StoryCanAnimObject>();

                if (aniObject != null)
                {
                    if (AudioPanel != null && !aniObject.CanAddAudio)
                    {
                        AudioPanel.SetActive(false);
                    }

                    if (MeshTexPanel != null && !aniObject.CanChangeTex)
                    {
                        MeshTexPanel.SetActive(false);
                    }

                    if (TimeSlider != null)
                    {
                        TimeSlider.SetValueWithoutNotify(aniObject.OnSetAniTime);


                        timeTipsTxt.text = aniObject.OnSetAniTime.ToString();
                    }
                }
                charAniObject = Target.gameObject.GetComponent<StoryCharAnimObject>();

                if (AniEditor != null)
                {
                    if(charAniObject != null)
                    {
                        AniEditor.initWithEditor(charAniObject);
                    }
                    else
                    {
                        AniEditor.initWithEditor(aniObject);
                    }

                }

                genAniCoroutine = WaitGenRuntimeAni(.5f);
            }

        updateComponentFunctions();

        //liwei edit {
        AniPanel.SetActive(false);
        compeditor.EditorsPanel.gameObject.SetActive(false);
        // } liwei edit
    }
    public void initPanels()
        {
            title_text.text = aniObject.title;
            desc_text.text = aniObject.desctext;
            link_text.text = aniObject.authorLink;

            if (aniObject.authorTarget != null)
            {
                link_target.isOn = (aniObject.authorTarget == 1) ? true : false;
            }




            if (aniObject.type == StoryCanAnimObject.ObjectType.GameObject)
            {
                AniTextPanel.SetActive(false);
                stageLinksPanel.SetActive(false);

            }
            else if(aniObject.type == StoryCanAnimObject.ObjectType.Hotspot)
            {
                AniPanel.SetActive(false);
                stageLinksPanel.SetActive(false);
            }
            else if (aniObject.type == StoryCanAnimObject.ObjectType.StageScene)
            {
                string[] links = aniObject.authorLink.Split(',');

                if(links.Length > 3)
                {

                    link_text_top.text = links[0];
                    link_text_left.text = links[1];
                    link_text_right.text = links[2];
                    link_text_down.text = links[3];
                }

                AniPanel.SetActive(false);
                AniTextPanel.SetActive(false);
            }
        }
        public void updateComponentFunctions()
        {

            initPanels();
        }
        public void openObjectAnimEdit()
        {
            openAnimtion = true;
        
            if (AniSettingPanel != null)
            {
                AniSettingPanel.SetActive(true);
            }
            if(charAniObject != null)
            {
            charAniObject.InitIkEffectors();
            }
            if (Target != null)
            {
            }
        }
        public void CreateNewAnimClip()
        {
            if (animationView == null)
            {
                Editor.CreateOrActivateWindow(RuntimeWindowType.Animation.ToString());
                RuntimeWindow AniWin = Editor.GetWindow(RuntimeWindowType.Animation);
                animationView = AniWin.GetComponentInChildren<Battlehub.RTEditor.Views.AnimationView>();
            }
            if(AniEditor != null)
            {

                AniEditor.gameObject.SetActive(true);
            }
        /*
        if (animationView == null)
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.AnimationEditor.ToString());
            RuntimeWindow AniWin = Editor.GetWindow(RuntimeWindowType.AnimationEditor);
            //animationView = AniWin.GetComponentInChildren<Battlehub.RTEditor.Views.AnimationView>();
        }
        */

        if (Target != null)
            {
              OnCreateAnimClick();
            }
        }


        public void OnCreateAnimClick()
        {

            //ISaveAssetDialog saveAssetDialog = IOC.Resolve<ISaveAssetDialog>();
            RuntimeAnimationClip clip = GenJustRuntimeAniClip();

            GameObject go = Target.gameObject;
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (Target.GetComponent<RuntimeAnimation>() == null)
            {
                Target.gameObject.AddComponent<RuntimeAnimation>();

            }
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            RuntimeAnimationProperty property = genPresetAniPosBlank(go, aniObject.OnSetAniTime);
            clip.Add(property);
            property = genPresetAniScaleBlank(go, aniObject.OnSetAniTime);
            clip.Add(property);
            property = genPresetAniRotatBlank(go, aniObject.OnSetAniTime);
            clip.Add(property);


            animation.Loop = true;
            List<RuntimeAnimationClip> Clips = new List<RuntimeAnimationClip>();
            Clips.Add(clip);
            animation.SetClips(Clips, 0);
        
           IRTE rte = IOC.Resolve<IRTE>();


            rte.Selection.activeGameObject = null;
            rte.Selection.activeGameObject = go;


            //animation.Refresh();
        
            if (animationView != null)
           {
               animationView.OnAnimationClipsChanged();
               animationView.SetRecording(true);
           }

            /*
            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            saveAssetDialog.Asset = clip;
            saveAssetDialog.AssetIcon = settings.SelectedTheme.GetIcon($"RTEAsset_{typeof(RuntimeAnimationClip).FullName}");
            saveAssetDialog.SaveCompleted += OnSaveCompleted;

            */

        }

        public  async void saveAnimateClip(UnityObject clip, string targetPath)
        {
            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
            byte[] preview = previewUtility.CreatePreviewData(clip);
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using (await project.LockAsync())
            {
                await project.SaveAsync(targetPath, clip, preview);
            }
        }
        public void OnSaveCompleted(ISaveAssetDialog sender, UnityObject asset)
        {

            Debug.Log("AniObj C OnSaveCompleted");
            sender.SaveCompleted -= OnSaveCompleted;
            if (animationView != null)
            {
                animationView.OnSaveCompleted(sender, asset);
            }

            if (asset == null)
            {
                return;
            }

            RuntimeAnimationClip clip = (RuntimeAnimationClip)asset;
            GameObject go = Target.gameObject;
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            RuntimeAnimationProperty property = genBasePositionAni(go, 1);


            clip.Add(property);

            IRTE rte = IOC.Resolve<IRTE>();
            rte.Selection.activeGameObject = null;
            rte.Selection.activeGameObject = go;

            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            animation.Refresh();
            if (animationView != null)
            {
                animationView.SetRecording(true);
            }

        }
        public void OnAniSetChanged(Int32 val)
        {
            aniObject.OnSetAniID = val;
            StopCoroutine(genAniCoroutine);
            StartCoroutine(genAniCoroutine);
        }

        public void OnAniTimeChanged(Single val)
        {
            aniObject.OnSetAniTime = val;
            if(timeTipsTxt != null)
            {
                timeTipsTxt.text = val.ToString();
            }
            StopCoroutine(genAniCoroutine);
            StartCoroutine(genAniCoroutine);

        }

        public void OnLinkChannged(string val)
        {
            aniObject.authorLink = val;
        }
        public void OnLinkTargetChannged(int val)
        {
            aniObject.authorTarget = val;
        }
        public void OnLinkTargetChannged(bool val)
        {
            aniObject.authorTarget = (val == true)?1:0;
        }
    public IEnumerator WaitGenRuntimeAni(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            UpdatePresetAni();
        }
        public void UpdatePresetAni()
        {
            bool isUseRuntimeAnimation = true;



            if(aniObject.OnSetAniTime <= 0)
            {
                aniObject.OnSetAniTime = 1;
            }
            if (!isUseRuntimeAnimation)
            {
            
                if (Target.GetComponent<Animation>() == null)
                {
                    Target.gameObject.AddComponent<Animation>();

                }
                Animation animation = Target.gameObject.GetComponent<Animation>();
           
                if (animations.Length > aniObject.OnSetAniID)
                {
                    /*
                    AnimationClip clip = animations[aniObject.OnSetAniID - 1];
                    animation.clip = clip;

                    animation.wrapMode = WrapMode.Loop;
                    animation.clip.wrapMode = WrapMode.Loop;
                    */


                    // define animation curve
                    AnimationCurve translateX = AnimationCurve.Linear(0.0f, 0.0f, 2.0f, 2.0f);
                    AnimationClip clip = new AnimationClip();
                    clip.legacy = true;
                    clip.SetCurve("", typeof(Transform), "localPosition.x", translateX);
                    animation.AddClip(clip, "test");
                    animation.Play("test");

                }
                /*
                AnimationCurve rotateY = AnimationCurve.Linear(0.0f, 0.0f, 2.0f, 270.0f);
                AnimationClip animationClip = new AnimationClip();
                animationClip.SetCurve("", typeof(Transform), "localEulerAngles.y", rotateY);
                animation.AddClip(animationClip, "test");
                animation.Play("test");
                */
            }
            else
            {
                RuntimeAnimationClip clip = GenJustRuntimeAniClip();
                GameObject go = Target.gameObject;
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                if (Target.GetComponent<RuntimeAnimation>() == null)
                {
                    Target.gameObject.AddComponent<RuntimeAnimation>();

                }
                RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
                RuntimeAnimationProperty property = genBasePositionAni(go, aniObject.OnSetAniTime);

                if (aniObject.OnSetAniID == 1)
                {

                    property = genPresetAniRotat(go, aniObject.OnSetAniTime);

                }
                else if (aniObject.OnSetAniID == 2)
                {

                    property = genPresetAniUpdown(go, aniObject.OnSetAniTime);
                }
                else if (aniObject.OnSetAniID == 3)
                {

                    property = genPresetAniLeftRight(go, aniObject.OnSetAniTime);
                }
                else if (aniObject.OnSetAniID == 4)
                {

                    property = genPresetAniScale(go, aniObject.OnSetAniTime);
                }
                animation.Loop = true;
                clip.Add(property);
                List<RuntimeAnimationClip> Clips = new List<RuntimeAnimationClip>();
                Clips.Add(clip);
                animation.SetClips(Clips, 0);

            }

        }
        public void closeObjectAnimEdit()
        {
            openAnimtion = false;
            if(AniSettingPanel != null)
            {
                AniSettingPanel.SetActive(false);
            }
        }

        public void ClickToSetAudio()
        {
            if (Target != null && aniObject != null)
            {


                Editor.IsBusy = true;

                var extensions = new[] {
                    new ExtensionFilter("Sound Files", "mp3", "wav" ),
                };
                string[] path = StandaloneFileBrowser.OpenFilePanel("選擇聲音檔案", "", extensions, false);

                if (path != null && path.Length > 0 && path[0] != null)
                {
                    aniObject.setAudioPath(path[0]);

                }
                Editor.IsBusy = false;

            }
        }

        public void ClickToInitAnimation()
        {
            if (openAnimtion)
            {
                closeObjectAnimEdit();
            }
            else
            {
                openObjectAnimEdit();
            }

        }
        public void ClickToOffAnimation()
        {
            closeObjectAnimEdit();

            if (Target != null)
            {
                if (charAniObject != null)
                {
                    if (charAniObject.BodyIk != null)
                    {
                        charAniObject.BodyIk.Disable();
                        charAniObject.BodyIk.gameObject.SetActive(false);
                    }
                }
            }
        }

        public RuntimeAnimationClip   GenJustRuntimeAniClip()
        {
            RuntimeAnimationClip clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();
            clip.name = "動畫";// m_localization.GetString("ID_RTEditor_AnimationView_NewAnimationClip", "動畫");
            return clip;
        }

    
        public RuntimeAnimationProperty genRuntimeAnimationPropertyRow(RuntimeAnimationProperty property,string pname, Keyframe[] keyframes)
        {


            return new RuntimeAnimationProperty
            {
                Parent = property,
                Component = property.Component,
                ComponentTypeName = property.ComponentTypeName,
                PropertyDisplayName = pname,
                PropertyName = pname,
                Curve = new AnimationCurve
                {
                    keys = keyframes
                }

            };
        }
        public RuntimeAnimationProperty genBasePositionAni(GameObject go,float Sec)
        {
            if (go != null)
            {
                RuntimeAnimationProperty property = getAnimationPositionProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.position.x),
                        new Keyframe(Sec, go.transform.position.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.position.y),
                        new Keyframe(Sec, go.transform.position.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.position.z),
                        new Keyframe(Sec, go.transform.position.z)
                    })
                };

                return property;
            }
            return null;
        }


        public RuntimeAnimationProperty getAnimationPositionProperty(GameObject go)
        {
            RuntimeAnimationProperty property = new RuntimeAnimationProperty();
            property.Component = go.GetComponent<Transform>();
            property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
            property.ComponentDisplayName = "Transform";
            property.AnimationPropertyName = "m_LocalPosition";
            property.PropertyDisplayName = "位置";
            property.PropertyName = "LocalPosition";
            return property;
        }

        public RuntimeAnimationProperty getAnimationRotationProperty(GameObject go)
        {
       
            RuntimeAnimationProperty property = new RuntimeAnimationProperty();
            property.Component = go.GetComponent<Transform>();
            property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
            property.ComponentDisplayName = "Transform";
            property.AnimationPropertyName = "m_LocalEuler";
            property.PropertyDisplayName = "旋轉";
            property.PropertyName = "LocalEuler";

            return property;
        }

        public RuntimeAnimationProperty getAnimationScaleProperty(GameObject go)
        {

            RuntimeAnimationProperty property = new RuntimeAnimationProperty();
            property.Component = go.GetComponent<Transform>();
            property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
            property.ComponentDisplayName = "Transform";
            property.AnimationPropertyName = "m_LocalScale";
            property.PropertyDisplayName = "縮放";
            property.PropertyName = "LocalScale";
            return property;
        }
    
        public RuntimeAnimationProperty genPresetAniRotat(GameObject go, float Sec)
        {
            if (go != null)
            {
                //Vector3 rotStart = aniObject.rotatSet;// Quaternion.Euler(go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z);
                //Vector3 rotEnd = aniObject.rotatSet2;// Quaternion.Euler(go.transform.localRotation.x+ aniObject.rotatSet.x, go.transform.localRotation.y, go.transform.localRotation.z + aniObject.rotatSet.z);
                RuntimeAnimationProperty property = getAnimationRotationProperty(go);
            
                property.Children = new List<RuntimeAnimationProperty> 
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0,go.transform.localRotation.x),
                        new Keyframe(Sec, go.transform.localRotation.x)
                    }),
                    /*
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, transform.localRotation.y),
                        new Keyframe(Sec, transform.localRotation.y +360)
                    }),
                    */

                    new RuntimeAnimationProperty
                    {
                        Parent = property,
                        Component = property.Component,
                        ComponentTypeName = property.ComponentTypeName,
                        PropertyDisplayName = "y",
                        PropertyName = "y",
                        Curve = AnimationCurve.Linear(0.0f, transform.localRotation.y, Sec, transform.localRotation.y +360)

                    },

                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, transform.localRotation.z),
                        new Keyframe(Sec, transform.localRotation.z)
                    })
                };

                return property;
            }
            return null;
        }

        public RuntimeAnimationProperty genPresetAniUpdown(GameObject go, float Sec)
        {
            if (go != null)
            {

                RuntimeAnimationProperty property = getAnimationPositionProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.position.x),
                        new Keyframe(Sec, go.transform.position.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.position.y),
                        new Keyframe(Sec/2, go.transform.position.y+.5f),
                        new Keyframe(Sec, go.transform.position.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.position.z),
                        new Keyframe(Sec, go.transform.position.z)
                    })
                };
                return property;
            }
            return null;
        }
        public RuntimeAnimationProperty genPresetAniLeftRight(GameObject go, float Sec)
        {
            if (go != null)
            {

                RuntimeAnimationProperty property = getAnimationRotationProperty(go);

                property.Children = new List<RuntimeAnimationProperty>
                {
                     genRuntimeAnimationPropertyRow(property, "x", new[]{
                        new Keyframe(0, go.transform.position.x),
                        new Keyframe(Sec, go.transform.position.x+2)
                    })
                };

                return property;
            }
            return null;
        }
        public RuntimeAnimationProperty genPresetAniScale(GameObject go, float Sec)
        {
            if (go != null)
            {
                RuntimeAnimationProperty property = getAnimationScaleProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.localScale.x),
                        new Keyframe(Sec/2, go.transform.localScale.x*1.2f),
                        new Keyframe(Sec, go.transform.localScale.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.localScale.y),
                        new Keyframe(Sec/2, go.transform.localScale.y*1.2f),
                        new Keyframe(Sec, go.transform.localScale.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.localScale.z),
                        new Keyframe(Sec/2, go.transform.localScale.z*1.2f),
                        new Keyframe(Sec, go.transform.localScale.z)
                    })
                };

                return property;
            }
            return null;
        }
        public RuntimeAnimationProperty genPresetAniBlank(GameObject go, float Sec)
        {
            if (go != null)
            {
                RuntimeAnimationProperty property = getAnimationPositionProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.position.x),
                        new Keyframe(Sec, go.transform.position.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.position.y),
                        new Keyframe(Sec, go.transform.position.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.position.z),
                        new Keyframe(Sec, go.transform.position.z)
                    })
                };

                return property;
            }
            return null;
        }

        public RuntimeAnimationProperty genPresetAniPosBlank(GameObject go, float Sec)
        {
            if (go != null)
            {
                RuntimeAnimationProperty property = getAnimationPositionProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.position.x),
                        new Keyframe(Sec, go.transform.position.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.position.y),
                        new Keyframe(Sec, go.transform.position.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.position.z),
                        new Keyframe(Sec, go.transform.position.z)
                    })
                };

                return property;
            }
            return null;
        }


        public RuntimeAnimationProperty genPresetAniScaleBlank(GameObject go, float Sec)
        {
            if (go != null)
            {
                RuntimeAnimationProperty property = getAnimationScaleProperty(go);
                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0, go.transform.localScale.x),
                        new Keyframe(Sec, go.transform.localScale.x)
                    }),
                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0, go.transform.localScale.y),
                        new Keyframe(Sec, go.transform.localScale.y)
                    }),
                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, go.transform.localScale.z),
                        new Keyframe(Sec, go.transform.localScale.z)
                    })
                };

                return property;
            }
            return null;
        }
        public RuntimeAnimationProperty genPresetAniRotatBlank(GameObject go, float Sec)
        {
            if (go != null)
            {
                //Vector3 rotStart = aniObject.rotatSet;// Quaternion.Euler(go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z);
                //Vector3 rotEnd = aniObject.rotatSet2;// Quaternion.Euler(go.transform.localRotation.x+ aniObject.rotatSet.x, go.transform.localRotation.y, go.transform.localRotation.z + aniObject.rotatSet.z);
                RuntimeAnimationProperty property = getAnimationRotationProperty(go);

                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0,go.transform.localRotation.x),
                        new Keyframe(Sec, go.transform.localRotation.x)
                    }),

                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0,go.transform.localRotation.y),
                        new Keyframe(Sec, go.transform.localRotation.y)
                    }),

                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, transform.localRotation.z),
                        new Keyframe(Sec, transform.localRotation.z)
                    })
                };

                return property;
            }
            return null;
        }


        public void OnTitleChanged(string val)
        {
            aniObject.title = val;
        }

        public void OnDescTextChanged(string val)
        {
            aniObject.desctext = val;
        }


        public void OnStageLinksChanged(string val)
        {
        aniObject.authorLink = link_text_top.text +","+ link_text_left.text+","+ link_text_right.text + "," + link_text_down.text;

        }

}

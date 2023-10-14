using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System.IO;
using UnityGLTF;
using UnityGLTF.Timeline;
using UnityEngine.UI;
using Autodesk.Fbx;
using SFB;
using TMPro;
using iiiStoryEditor;

public class ExportSceneWindow : MonoBehaviour
{
    public GameObject ExportScenePrefab;
    IRuntimeSelection m_selection;
    public TMP_Text ExportDesc;
    public GameObject[] exportObects;
    public FbxExport fbxExport;
    

    // Start is called before the first frame update
    void Start()
    {

        m_selection = IOC.Resolve<IRTE>().Selection;
        m_selection.SelectionChanged += OnSelectionChanged;

        //ExportDesc

        if (m_selection.gameObjects != null && m_selection.gameObjects.Length > 0)
        {
            exportObects = m_selection.gameObjects;
            if (exportObects.Length > 0)
            {
                /*
                Object[] objects = new Object[exportObects.Length];
                for (int i = 0; i < exportObects.Length; i++)
                {
                    objects[i] = exportObects[i] as Object;
                }
                */
            }
        }
        else
        {
            
            ExposeToEditor[] ExposeToEditors = GameObject.FindObjectsOfType<ExposeToEditor>();
            ExposeToEditor one;
            List<GameObject> ExportObjectOnRoots = new List<GameObject>();
            if (ExposeToEditors.Length > 0)
            {
                for (int i = 0; i < ExposeToEditors.Length; i++)
                {
                    one = ExposeToEditors[i];
                    if (one.gameObject.transform.parent == null)
                    {
                        ExportObjectOnRoots.Add(one.gameObject);
                    }
                }
                exportObects = ExportObjectOnRoots.ToArray();
            }

        }


        if (ExportDesc != null)
        {
            ExportDesc.text = exportObects.Length.ToString();
        }

    }


    

    public void Click_Export_Gltf()
    {
       
        GLTFSceneExporter exporter =  createGltfGlbExporter();
        ExportToGltf(exporter);
    }

    public void Click_Export_Glb()
    {

        GLTFSceneExporter exporter = createGltfGlbExporter();
        ExportToGlb(exporter);
    }

    public GLTFSceneExporter createGltfGlbExporter()
    {
        if (exportObects.Length > 0)
        {
            Transform[] ExportTransforms = new Transform[exportObects.Length];
            Transform one;

            Object oo;
            UnityEngine.Object uo;
            

            if (exportObects.Length == 1)
            {
                ExportTransforms[0] = exportObects[0].transform;
            }
            else
            {
                for (int i = 0; i < exportObects.Length; i++)
                {
                    one = exportObects[i].transform;
                    ExportTransforms[i] = one;

                }
            }

            GLTFSceneExporter exporter = new GLTFSceneExporter(ExportTransforms, new ExportOptions());


            for (int i = 0; i < ExportTransforms.Length; i++)
            {
                Debug.Log("one.ExportTransforms.Length2: "+ ExportTransforms.Length);
                one = ExportTransforms[i];

                Animator animator = one.GetComponent<Animator>();
                if (false && animator != null)
                {
                    //AnimationClip[] clips = UnityEditor.AnimationUtility.GetAnimationClips(transform.gameObject);
                    //var animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                    

                    AnimatorClipInfo[] aniClipInfos = animator.GetCurrentAnimatorClipInfo(0);
                    AnimationClip clip = aniClipInfos[0].clip;
                    //Debug.Log("ExportAnimationClip clip:::" + clip.name+":"+ clip.averageDuration+":" + clip.apparentSpeed + ":" + clip.averageAngularSpeed + ":" + clip.averageSpeed);
                    //exporter.ExportAnimationClip(clip, "AnimatorClip", transform,1);
                    RuntimeAnimatorController _runtimeAnimController = animator.runtimeAnimatorController;

                    /*
                    anirecorder = new GLTFRecorder(animator.transform, true);
                    recTime = clip.averageDuration;
                    startrectime = (float)CurrentTime ;
                    anirecorder.StartRecording(CurrentTime);
                    StartCoroutine(_AniUpdateRecording());
                    */
                }else if (one.GetComponent<RuntimeAnimation>() != null)
                {
                    RuntimeAnimation runtimeAnimation = one.GetComponent<RuntimeAnimation>();
                    Animation animation = one.GetComponent<Animation>();
                    RuntimeAnimationClip runClip;
                    if (runtimeAnimation != null && runtimeAnimation.Clips != null)
                    {
                        AnimationClip[] clips = new AnimationClip[animation.GetClipCount()];
                        AnimationClip newclip;
                        if (runtimeAnimation != null && runtimeAnimation.ClipsCount > 0)
                        {
                            for (var rai = 0; rai < runtimeAnimation.ClipsCount; rai++)
                            {
                                newclip = new AnimationClip();
                                newclip.legacy = true;
                                runClip = runtimeAnimation.Clips[rai];
                                newclip = runClip.Clip;
                                animation.AddClip(newclip, "clip-" + i + "-" + rai);

                                clips[rai] = runClip.Clip;
                            }
                        }

                        animation.clip = clips[0];

                        //exporter.ExportAnimationClips(one, clips);
                        //GLTFRecorder


                        Debug.Log("exporter animation:" + one.name);

                    }
                }
                else
                {
                    if (one.GetComponent<Animation>() != null)
                    {
                        Debug.Log("one.GetComponent<Animation>() != null: ");
                        Animation animation = one.GetComponent<Animation>();
                        if (animation != null && animation.clip != null)
                        {

                           //exporter.ExportRuntimeAnimationClip(animation.clip, "clip-", one.transform,1);
                            Debug.Log("exporter animation:" + one.name);

                        }
                    }

                }
            }
            return exporter;
        }
        return null;

    }

    /*
    protected GLTFRecorder anirecorder;
    protected float startrectime = -1;
    protected float recTime = -1;
    public bool IsAniRecording => anirecorder?.IsRecording ?? false;

    private double CurrentTime =>
#if UNITY_2020_1_OR_NEWER
                    Time.timeAsDouble;
#else
			        Time.time;
#endif


    private void FinishOneAni()
    {
        anirecorder.EndRecording("Assets/Recordings/Recorded_aniani.glb");
        
    }
    protected virtual void AniUpdateRecording()
    {
        if (CurrentTime > anirecorder.LastRecordedTime)
        {
            anirecorder.UpdateRecording(CurrentTime);

            float recedTime = (float)CurrentTime - startrectime;
            if (recedTime > recTime)
            {
                FinishOneAni();

            }
        }
    }

    private IEnumerator _AniUpdateRecording()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (!IsAniRecording)
                yield break;

            AniUpdateRecording();
        }
    }
    */

    public async void ExportToGltf(GLTFSceneExporter exporter)
    {
        IRTE editor = IOC.Resolve<IRTE>();
        editor.IsBusy = true;
        

        string fileName = "";

        #if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            string targetPath = "/Users/milkywong/Documents/UnityWorks/storyboard-studio-html/public/data/test.gltf";// " / Users/milkywong/Documents/UnityWorks/storyboard-studio-builds/test.gltf";
        #else
                string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "gltf"); // "/Users/milkywong/Documents/UnityWorks/storyboard-studio/_Exports/test.gltf";// StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "gltf");
       
        #endif

        if (targetPath != null && targetPath != "")
        {
            exporter.SaveGLTFandBin(Path.GetDirectoryName(targetPath), Path.GetFileNameWithoutExtension(targetPath));
        }

        editor.IsBusy = false;
    }

    public async void ExportToGlb(GLTFSceneExporter exporter)
    {
        IRTE editor = IOC.Resolve<IRTE>();
        editor.IsBusy = true;

        string fileName = "";
        string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "glb");
        if (targetPath != null && targetPath !="")
        {
            exporter.SaveGLB(Path.GetDirectoryName(targetPath), Path.GetFileNameWithoutExtension(targetPath));
        }

        editor.IsBusy = false;
    }

    public void Click_Export_Fbx()
    {



        IRTE editor = IOC.Resolve<IRTE>();
        editor.IsBusy = true;

        string fileName = "";

        #if UNITY_STANDALONE_OSX

                string targetPath = "/Users/milkywong/Documents/UnityWorks/storyboard-studio-builds/models/out.fbx";
        #else


        
                        string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "fbx");
        #endif

        if (targetPath != null && targetPath != "")
        {

            if (fbxExport != null)
            {
                fbxExport.ExportObjects(exportObects, targetPath);

            }

        }

        editor.IsBusy = false;

        /*
        string fbxFilePath = Application.dataPath;
        fbxFilePath = Path.Combine(fbxFilePath, "FbxEmptyScene.fbx");
        fbxFilePath = Path.GetFullPath(fbxFilePath);
        if(fbxExport != null)
        {
            fbxExport.ExportObjects(exportObects, fbxFilePath);

        }
        */
    }



    void OnSelectionChanged(Object[] unselectedObjects)
    {
            if (unselectedObjects != null)
            {
                for (int i = 0; i < unselectedObjects.Length; ++i)
                {
                    Object unselected = unselectedObjects[i];
                }
            }

            if (m_selection.objects != null)
            {
                for (int i = 0; i < m_selection.objects.Length; ++i)
                {
                    Object selected = m_selection.objects[i];
                }
            }
    }

    void OnDestroy()
    {
            if (m_selection != null)
            {
                m_selection.SelectionChanged -= OnSelectionChanged;
            }
    }



}
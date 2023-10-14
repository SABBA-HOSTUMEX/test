using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;
using iiiStoryEditor.UI.ViewModels.ViewModels;
using System.Collections.Generic;
using UnityEngine ;
using TMPro;
using UnityGLTF;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System.IO;
using SFB;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using UnityEngine.Video;
namespace iiiStoryEditor.UI.ViewModels
{
    [Binding]
    public class ExportHtmlViewModel : ViewModel
    {
        public StoryExportViewModel SeVM;
        public StoryBoardNodeEditor NodeEditor;

        public TMP_Text LabelTxt;
        public List<OneSceneNode> storynodes;

        public string exportPathRoot;
        public string exportPath;
        public string exportFilename;

        public string exportType = "";


        public UnityEngine.UI.Toggle exeToggle;
        private bool isexe = true;
        protected override void Start()
        {
            base.Start();
        }
        public void initByStoryExportViewModel(StoryExportViewModel _sevm)
        {
            SeVM = _sevm;
            NodeEditor = _sevm.NodeEditor;
            initNodes();


        }

        public void initNodes()
        {
            Debug.Log("initNodes:" );
            storynodes = NodeEditor.GetNodesByStory();

            if(LabelTxt != null)
            {
                LabelTxt.text = "匯出 " + storynodes.Count + "組物件到網頁";
            }

            Debug.Log("storynodes:" + storynodes.Count);
        }
        public async void ClickoToExport_360()
        {
            exportType = "360";
            ClickoToExport();
        }
        public async void ClickoToExport_vr()
        {
            exportType = "vr";
            ClickoToExport();
        }

        public async void ClickoToExport()
        {
            if(storynodes != null)
            {

                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                string fileName = "";
                string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "html"); // "/Users/milkywong/Documents/UnityWorks/storyboard-studio/_Exports/test.gltf";// StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", fileName, "gltf");


                if (targetPath != null && targetPath != "")
                {
                    exportPath = Path.GetDirectoryName(targetPath);
                    exportPathRoot = exportPath;
                    exportFilename = Path.GetFileNameWithoutExtension(targetPath);


                    if (isexe)
                    {
                        makeZipFile();
                        exportPath += "/resources/app/";
                        exportFilename = "web";
                    }
                }

                string exportPathWithFolder = exportPath + "/" + exportFilename;
                string exportDataPath = exportPathWithFolder + "/data";

                try
                {
                    if (!Directory.Exists(exportDataPath))
                    {
                        Directory.CreateDirectory(exportDataPath);
                    }

                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                }



                StoryBrdData sbData = new StoryBrdData();
                sbData.version = 1;
                sbData.type = exportType;
                sbData.cuts = new StoryCut[storynodes.Count];

                StoryCut cut;

                GLTFSceneExporter exporter;// = createGltfGlbExporter();

                OneSceneNode storyNode;
                GameObject[] exportObects ;

                List<StoryVideodata> scvideos;
                StoryVideodata svdata;

                VideoPlayer[] scvplayers;

                VideoPlayer vplayer;
                string m_name;
                string videoFileName;
                string stagesLiHtml = "";
                Camera[] cams;//= storyNode.m_obj.GetComponentsInChildren<Camera>(true);

                for (int i =0; i< storynodes.Count; i++)
                {
                    scvideos = null;
                    storyNode = storynodes[i];
                    if (storyNode.m_obj != null)
                    {

                        m_name = "shot" + i.ToString();
                        exportObects = new GameObject[1];
                        exportObects[0] = storyNode.m_obj;

                        cams = storyNode.m_obj.GetComponentsInChildren<Camera>(true);
                        if(cams.Length > 0)
                        {

                            for (int c = 0; c < cams.Length; c++)
                            {
                                cams[c].gameObject.SetActive(true);
                            }

                        }
                        exporter = createGltfGlbExporter(exportObects);
                        exporter.SaveGLTFandBin(exportDataPath, m_name);


                        cut = new StoryCut();
                        cut.index = i;
                        cut.n_type = storyNode.type.ToString();
                        cut.m_type = "gltf";
                        cut.m_path = "data";
                        cut.m_filename = m_name + ".gltf";
                        cut.m_name = storyNode.m_obj.name;
                        cut.m_img = Texture2DToBase64(storyNode.m_previewTexture);

                        cut.m_time = storyNode.runsec;


                        stagesLiHtml += "<li><a href=\""+ exportFilename + ".html#s"+ i + "\" target='_top'>" + storyNode.m_obj.name + "</a></li>";


                        if (storyNode.m_obj.GetComponent<VideoPlayer>() != null)
                        {
                            vplayer = storyNode.m_obj.GetComponent<VideoPlayer>();
                            if (vplayer.url != "" && File.Exists(vplayer.url)) 
                            {
                                 videoFileName = Path.GetFileName(vplayer.url);

                                if (!File.Exists(exportDataPath + "/" + videoFileName))
                                {
                                    File.Copy(vplayer.url, exportDataPath + "/" + videoFileName);
                                }

                                cut.video_url = videoFileName;
                            }
                        }
                        scvideos = new List<StoryVideodata>();

                        scvplayers = storyNode.m_obj.GetComponentsInChildren<VideoPlayer>();

                        if (scvplayers != null)
                        {
                            for(int v = 0; v < scvplayers.Length; v++)
                            {
                                vplayer = scvplayers[v];
                                if(vplayer.url != "")
                                {
                                    videoFileName = Path.GetFileName(vplayer.url);

                                    if(!File.Exists(exportDataPath + "/" + videoFileName))
                                    {
                                        File.Copy(vplayer.url, exportDataPath + "/" + videoFileName);
                                    }
                                    svdata = new StoryVideodata();
                                    svdata.m_name = vplayer.gameObject.name;
                                    svdata.video_url = videoFileName;
                                    scvideos.Add(svdata);
                                }
                            }

                            cut.videos = scvideos;

                        }


                        StoryCanAnimObject[] storyCanAnimObjects = storyNode.m_obj.GetComponentsInChildren<StoryCanAnimObject>();
                        StoryCanAnimObject animObject;
                        StoryAudiodata sadata;
                        List<StoryAudiodata> scaudios = new List<StoryAudiodata>();


                        List<StoryLinkdata> sclinks = new List<StoryLinkdata>();
                        StoryLinkdata slinkdata;

                        List<StoryAnidata> scanimations = new List<StoryAnidata>();
                        StoryAnidata sanidata;

                        List<StoryInfodata> scInfos = new List<StoryInfodata>();
                        StoryInfodata sinfodata;

                        string audioFileName;

                        if (storyCanAnimObjects != null)
                        {
                            for (int s = 0; s < storyCanAnimObjects.Length; s++)
                            {
                                animObject = storyCanAnimObjects[s];
                                if (animObject.audioLink  != "")
                                {
                                    audioFileName = Path.GetFileName(animObject.audioLink);

                                    if (!File.Exists(exportDataPath + "/" + audioFileName))
                                    {
                                        File.Copy(animObject.audioLink, exportDataPath + "/" + audioFileName);
                                    }
                                    sadata = new StoryAudiodata();
                                    sadata.m_name = animObject.gameObject.name;
                                    sadata.audio_url = audioFileName;
                                    scaudios.Add(sadata);
                                }


                                if(animObject.GetComponent<RuntimeAnimation>() != null)
                                {
                                    RuntimeAnimation runtimeAnimation = animObject.GetComponent<RuntimeAnimation>();
                                    if(runtimeAnimation.Clips.Count > 0)
                                    {
                                        RuntimeAnimationClip clip = runtimeAnimation.Clips[0];
                                        sanidata = new StoryAnidata();
                                        sanidata.m_name = animObject.gameObject.name;
                                        sanidata.anim_name = clip.name;
                                        scanimations.Add(sanidata);
                                    }


                                }


                                if (animObject.authorLink != "")
                                {
                                    slinkdata = new StoryLinkdata();
                                    slinkdata.m_name = animObject.gameObject.name;
                                    slinkdata.link_url = animObject.authorLink;
                                    sclinks.Add(slinkdata);
                                }




                                if (animObject.title != "")
                                {
                                    sinfodata = new StoryInfodata();
                                    sinfodata.m_name = animObject.gameObject.name;
                                    sinfodata.m_title = animObject.title;
                                    sinfodata.m_desc = animObject.desctext;
                                    sinfodata.m_link  = animObject.authorLink;
                                    sinfodata.extlink = animObject.authorTarget;
                                    scInfos.Add(sinfodata);
                                }
                            }

                            cut.anims = scanimations;
                            cut.audios = scaudios;
                            cut.videos = scvideos;
                            cut.links = sclinks;
                            cut.infos = scInfos;

                        }


                        if(storyNode.type == OneSceneNode.ScNodeType.Minimap)
                        {

                            sbData.minimap = cut;
                            sbData.cuts[i] = cut;
                        }
                        else
                        {

                            sbData.cuts[i] = cut;
                        }
                    }
                    
                }

                //System.IO.File.Copy("Resources/web-template", exportPath);


                gentemplateHtml(exportPathWithFolder, stagesLiHtml);

                string sbDataJson = JsonUtility.ToJson(sbData);
                System.IO.File.WriteAllText(exportPathWithFolder + "/"+ exportFilename + ".json", sbDataJson);


                editor.IsBusy = false;
            }
        }

        public void UnZipp(string srcDirPath, string destDirPath)
        {
            ZipInputStream zipIn = null;
            FileStream streamWriter = null;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destDirPath));

                zipIn = new ZipInputStream(File.OpenRead(srcDirPath));
                ZipEntry entry;

                while ((entry = zipIn.GetNextEntry()) != null)
                {
                    string dirPath = Path.GetDirectoryName(destDirPath + entry.Name);

                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    if (!entry.IsDirectory)
                    {
                        streamWriter = File.Create(destDirPath + entry.Name);
                        int size = 2048;
                        byte[] buffer = new byte[size];

                        while ((size = zipIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            streamWriter.Write(buffer, 0, size);
                        }
                    }

                    streamWriter.Close();
                }
            }
            catch (System.Threading.ThreadAbortException lException)
            {
                // do nothing
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (zipIn != null)
                {
                    zipIn.Close();
                }

                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }

        public void makeZipFile()
        {


            TextAsset textAsset = Resources.Load("exportexepack", typeof(TextAsset)) as TextAsset;
            System.IO.File.WriteAllBytes(exportPathRoot+ "/Exportexepack.zip", textAsset.bytes);

            string FilePath = exportPathRoot + "/Exportexepack.zip"; // Path.Combine( Application.streamingAssetsPath, "Exportexepack.zip");


            UnZipp(FilePath, exportPathRoot+"/");


            System.IO.File.Delete(FilePath);


        }
        public void gentemplateHtml(string exportPathWithFolder,string stagesLi)
        {
            if (!Directory.Exists(exportPathWithFolder))
            {
                Directory.CreateDirectory(exportPathWithFolder);
            }
            if (!Directory.Exists(exportPathWithFolder + "/js"))
            {
                Directory.CreateDirectory(exportPathWithFolder + "/js");
            }
            if (!Directory.Exists(exportPathWithFolder + "/css"))
            {
                Directory.CreateDirectory(exportPathWithFolder + "/css");
            }
            if (!Directory.Exists(exportPathWithFolder + "/images"))
            {
                Directory.CreateDirectory(exportPathWithFolder + "/images");
            }

            var db = Resources.Load<TextAsset>("web-template/index");
             
            string html = db.text;

            html = html.Replace("<!--Stages Li-->", stagesLi);
            byte[] indexdata = System.Text.Encoding.UTF8.GetBytes(html); //db.bytes;
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/index.html", indexdata);


            System.IO.File.WriteAllBytes(exportPathWithFolder + "/minimap.html", indexdata);


            db = Resources.Load<TextAsset>("web-template/main");
            byte[] filedata = db.bytes;
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/"+ exportFilename + ".html", filedata);
            db = Resources.Load<TextAsset>("web-template/css/app");
            filedata = db.bytes;
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/css/app.css", filedata);

            db = Resources.Load<TextAsset>("web-template/js/app");
            html = db.text;
            html = html.Replace("storydata.json", exportFilename+".json");
            filedata = System.Text.Encoding.UTF8.GetBytes(html); 
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/js/app.js", filedata);

            db = Resources.Load<TextAsset>("web-template/js/chunk-vendors");
            filedata = db.bytes;
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/js/chunk-vendors.js", filedata);


            Texture2D texture = Resources.Load< Texture2D > ("web-template/favicon");

            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

            newTexture.SetPixels(0, 0, texture.width, texture.height, texture.GetPixels());
            newTexture.Apply();
            byte[] bytes = newTexture.EncodeToPNG();
            //Tell unity to delete the texture, by default it seems to keep hold of it and memory crashes will occur after too many screenshots.

            filedata = newTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/favicon.png", filedata);



            texture = Resources.Load<Texture2D>("web-template/images/icon-audio");
            newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            newTexture.SetPixels(0, 0, texture.width, texture.height, texture.GetPixels());
            newTexture.Apply();

            filedata = newTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/images/icon-audio.png", filedata);


            texture = Resources.Load<Texture2D>("web-template/images/icon-link");
            newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            newTexture.SetPixels(0, 0, texture.width, texture.height, texture.GetPixels());
            newTexture.Apply();

            filedata = newTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(exportPathWithFolder + "/images/icon-link.png", filedata);

            Destroy(newTexture);

            try
            {

                //Destroy(texture);
            }
            catch(System.Exception e)
            {

            }


        }
        public string Texture2DToBase64(Texture2D t2d)
        {
            byte[] bytesArr = t2d.EncodeToJPG();
            string strbaser64 = System.Convert.ToBase64String(bytesArr);
            return strbaser64;
        }


        public GLTFSceneExporter createGltfGlbExporter(GameObject[] exportObects)
        {
            if (exportObects.Length > 0)
            {
                Transform[] ExportTransforms = new Transform[exportObects.Length];
                Transform one;

                if (exportObects.Length == 1)
                {
                    ExportTransforms[0] = exportObects[0].transform;
                }
                else
                {
                    for (int i = 0; i < exportObects.Length; i++)
                    {

                        GameObject go = exportObects[i];

                        Camera[] cams = go.GetComponentsInChildren<Camera>(true);
                        if(cams.Length > 0)
                        {
                            for (int c = 0; c < cams.Length; c++)
                            {
                                cams[c].gameObject.SetActive(true);
                                cams[c].enabled = true;
                            }
                        }

                        one = exportObects[i].transform;

                        ExportTransforms[i] = one;

                    }
                }
                GLTFSceneExporter exporter = new GLTFSceneExporter(ExportTransforms, new ExportOptions());
                
                for (int i = 0; i < ExportTransforms.Length; i++)
                {
                    one = ExportTransforms[i];
                    if (one.GetComponent<Animation>() != null)
                    {
                        Animator animator = one.GetComponent<Animator>();
                        Animation animation = one.GetComponent<Animation>();
                        if (animator != null)
                        {
                            //AnimationClip[] clips = UnityEditor.AnimationUtility.GetAnimationClips(transform.gameObject);
                            //var animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                            // Debug.Log("animator: " + animator + "=> " + animatorController);
                            //exporter.ExportAnimationClips(transform, clips, animator, animatorController);
                        }


                        if (animation != null && animation.clip != null)
                        {


                            AnimationClip[] clips = new AnimationClip[animation.GetClipCount()];

                            RuntimeAnimation runtimeAnimation = one.GetComponent<RuntimeAnimation>();
                            AnimationClip newclip;
                            RuntimeAnimationClip runClip;
                            if (runtimeAnimation.ClipsCount > 0)
                            {
                                for (var rai = 0; rai < runtimeAnimation.ClipsCount; rai++)
                                {
                                    newclip = new AnimationClip();
                                    newclip.legacy = true;
                                    runClip = runtimeAnimation.Clips[rai];
                                    newclip = runClip.Clip;
                                    animation.AddClip(newclip, "clip-" + i + "-" + rai);
                                    Debug.Log("animator: " + "clip-" + i + "-" + rai);
                                    clips[rai] = runClip.Clip;
                                }
                            }

                            animation.clip = clips[0];

                            Debug.Log("exporter animation:" + one.name);

                        }
                    }

                }
                return exporter;
            }
            return null;

        }

        public void ExportExeToggle(bool val)
        {
            isexe = val;
        }
    }



}



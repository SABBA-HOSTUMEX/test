using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;
using System.Collections.Generic;
using FfmpegUnity;
using System;
using Battlehub.RTCommon;
using UnityEngine;
using FfmpegUnity;
using UnityEngine.UI;
using Battlehub.UIControls.DockPanels;
using SFB;
using Battlehub.RTEditor;
using System.Collections;

using System.IO;using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.Video;



namespace iiiStoryEditor.UI.ViewModels.ViewModels
{

    [Binding]
    public class ExportVideoViewModel : ViewModel
    {

        //public StoryBoardNodeEditor NodeEditor;
        public FfmpegCaptureCommand ffCaptureCmd; 
        public StoryExportViewModel SeVM;
        public OneSceneNode currNode;

        public RenderTexture rt;
        public Camera m_camera;
        public AudioListener m_audiolistener;
        public RawImage m_preview;

        public bool isCreating = false;

        public int outpngid = 0;

        public Text StateText;
        public Slider PcSlider;

        public int outWidth = 1920;
        public int outHeight = 1080;
        public int framerate = 30;

        public string outDir = "";
        public string outFilename = "output";


        public Region gameViewRegion;

        public int exportFrame = 0;
        public int exportTotalFrame = 0;
        public float waitToNext = 0;
        public int exportCutFrame = 0;
        public int exportCutFrameOfRate = 0;

        public TMP_Dropdown sizeDropdown;

        public AudioCapture AudioCapture;

        public float[] SampleData;

        private FileStream fileStream;
        public const int OutSampleRate = 48000;

        private List<VideoPlayer> NodeVideoPlayers;


        protected override void Start()
        {
            base.Start();
            outDir = Application.persistentDataPath;// "{PERSISTENT_DATA_PATH}";

            if(sizeDropdown != null)
            {
                sizeDropdown.onValueChanged.AddListener(delegate {
                    SizeDropdownValueChanged(sizeDropdown);
                });
            }
        }
        public void initByStoryExportViewModel(StoryExportViewModel _sevm)
        {
            SeVM = _sevm;
            SeVM.NodeEditor.VideoExportVM = this;


        }
        public void clickStartExport()
        {
            //startExport();
/*
#if UNITY_STANDALONE_OSX


            outDir = Path.GetDirectoryName(Application.persistentDataPath);
            Debug.Log(outDir);
            DateTime theTime = DateTime.Now;
            outFilename = Path.GetFileNameWithoutExtension("video"+ theTime.ToString("HHmmss") + ".mp4");

            startExport();

#else
*/
            string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出影片", "", "video", "mp4");
            if (targetPath != null && targetPath != "")
            {
                outDir = Path.GetDirectoryName(targetPath);
                outFilename = Path.GetFileNameWithoutExtension(targetPath);

                startExport();
            }
//#endif

        }
        void SizeDropdownValueChanged(TMP_Dropdown ddown)
        {

            string sizeStr = ddown.options[ddown.value].text;
            Debug.Log(sizeStr);
            switch (sizeStr)
            {
                case "Full HD（1920×1080）":

                    outWidth = 1920;
                    outHeight = 1080;

                    break;
                case "WQVGA（480×272）":
                    outWidth = 480;
                    outHeight = 272;
                    break;
                case "HD（1280×720）":
                    outWidth = 1280;
                    outHeight = 720;
                    break;
                case "2K （2048×1080）":
                    outWidth = 2048;
                    outHeight = 1080;
                    break;

                case "2KHD （2560X1440）":
                    outWidth = 2560;
                    outHeight = 1440;
                    break;
                    
                case "4K UHD（3840×2160）":
                    outWidth = 3840;
                    outHeight = 2160;
                    break;

            }
            Debug.Log(outWidth+":"+ outHeight);
            updateOptions();
        }
        public void startExport()
        {
            if (SeVM.NodeEditor != null)
            {
                
                rt = new RenderTexture(outWidth, outHeight, 16, RenderTextureFormat.ARGB32);
                outpngid = 0;
                if (ffCaptureCmd != null)
                {
                    UpdateCamera(SeVM.NodeEditor.CurrViewCamera);

                }

                string tmpPath = outDir + "/tmp";
                try
                {
                    if (!Directory.Exists(tmpPath))
                    {
                        Directory.CreateDirectory(tmpPath);
                    }

                    Debug.Log(SeVM.NodeEditor.TotalSec + "*" + framerate);
                    exportTotalFrame = SeVM.NodeEditor.TotalSec * framerate;


                    SeVM.isRecording = true;
                    StartCaptureAudio();


                    //StartCaptureFrame();

                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
        public void StartCaptureAudio()
        {

            SeVM.RecordState = "RecAudio";

            SeVM.NodeEditor.StartExportStory("wav");
            exportFrame = 0;
            SetExporState("影片輸出中...");
            SetExportPc(0f);

            string wavFilePath = outDir + "/tmp/audio.wav";
            if (File.Exists(wavFilePath))
            {
                File.Delete(wavFilePath);
            }

            if (AudioCapture != null)
            {
                AudioCapture.enabled = true;
                AudioCapture.gameObject.SetActive(true);
                AudioCapture.fileName = wavFilePath;
                AudioCapture.StartRecording();
            }

        }
        /*
        public void OnAudioDataFilterRead(float[] data, int channels)
        {
        }*/
        public void StartCaptureFrame()
        {
            Debug.Log("StartCaptureFrame");

            SetExporState("影片輸出中...");
            SetExportPc(.4f);

            updateOptions();
            exportFrame = 0;
            SeVM.NodeEditor.StartExportStory("mp4");
            SeVM.RecordState = "RecFrame";
            SeVM.NodeEditor.updateTotalSec();
        }
        public void updateOptions()
        {

            //Debug.Log("updateOptions");
            //Debug.Log(ffCaptureCmd.RunOptions);

            string options = "-r "+ framerate + " " +
            " -f image2 " +
            " -s " + outWidth + "x" + outHeight + " " +
            " -i \""+ outDir + "/tmp/%6d.png\"" +
            " -i \"" + outDir + "/tmp/audio.wav\"" +
            " -vcodec libopenh264" +
            " -crf 30" +
            " -pix_fmt yuv420p" +
            " \"" + outDir + "/" + outFilename + ".mp4\"";


            ffCaptureCmd.setOption(options);
            //ffCaptureCmd.RunOptions = options;


        }
        public void onWidthChange(string wid_str)
        {
            outWidth = int.Parse(wid_str);
            updateOptions();
        }
        public void onHeightChange(string hei_str)
        {
            Debug.Log("onHeightChange");
            outHeight = int.Parse(hei_str); ;
            updateOptions();
        }
        public void UpdateCamera(Camera cam)
        {
            if (ffCaptureCmd != null && cam != null)
            {
                if (cam != null)
                {
                    m_camera = cam;
                    if (rt != null)
                    {
                        m_camera.targetTexture = rt;
                    }

                    if(m_camera.gameObject.GetComponent<AudioListener>() != null)
                    {
                        m_audiolistener = m_camera.gameObject.GetComponent<AudioListener>();
                    }
                    else
                    {
                        m_audiolistener = m_camera.gameObject.AddComponent<AudioListener>();
                    }

                    currNode = SeVM.NodeEditor.currentNode as OneSceneNode;

                    onCameraUpdate();
                }
            }
        }

        public void onCameraUpdate()
        {
            NodeVideoPlayers = new List<VideoPlayer>();

            if (currNode && currNode.m_obj != null)
            {
                VideoPlayer[] vs = currNode.m_obj.GetComponentsInChildren<VideoPlayer>();
                if(vs.Length > 0)
                {
                    for(int i= 0 ; i < vs.Length; i++)
                    {
                        NodeVideoPlayers.Add(vs[i]);
                    }
                }
                exportCutFrame = 0;
            }
            

        }

        public void onNodePlayStop()
        {
            //Debug.Log("ExportVideoViewModel onNodePlayStop");
            if (ffCaptureCmd != null)
            {
                //Debug.Log("ExportVideoViewModel ffCaptureCmd");
                if (SeVM.isRecording && SeVM.RecordState == "RecFrame")
                {
                    SetExportPc(.8f);
                    updateOptions();
                    SeVM.isRecording = false;
                    ffCaptureCmd.ExecuteFfmpeg();
                    isCreating = true;

                }
                if (SeVM.isRecording && SeVM.RecordState == "RecAudio")
                {
                    Debug.Log("RecAudio On Play Stop");

                    SetExportPc(.3f);
                    if (AudioCapture != null)
                    {
                        AudioCapture.StopRecording();
                        AudioCapture.gameObject.SetActive(false);
                        DestroyImmediate(AudioCapture.gameObject);
                    }
                    SeVM.RecordState = "";

                    StartCaptureFrame();
                }
            }
        }
        public void SetExporState(string msg)
        {
            StateText.text = msg;
        }
        public void SetExportPc(float pc)
        {
            PcSlider.value = pc;
        }
        public void ExportFinish()
        {
            SetExporState("影片輸出中完成");
            SetExportPc(1);

            StartCoroutine(closeExportWindow());
        }
        IEnumerator closeExportWindow()
        {

            yield return new WaitForSeconds(5);
            isCreating = false;

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            
            Region ExportVideoRegion = gameObject.GetComponentInParent<Region>();

            Debug.Log("ExportVideoRegion");
            if (ExportVideoRegion != null)
            {
                try
                {

                    ExportVideoRegion.Destroy();
                }catch(Exception e)
                {

                }
            }
        }


        private void SaveAudioFile()
        {

            Debug.Log("SaveAudioFile");

        }

        void Update()
        {
            if (isCreating)
            {
                //Debug.Log("ffCaptureCmd Progress" + ffCaptureCmd.Progress);
                //Debug.Log("ffCaptureCmd RemainingTime" + ffCaptureCmd.RemainingTime);
                //Debug.Log("ffCaptureCmd IsRunning" + ffCaptureCmd.IsRunning);

                if (ffCaptureCmd.IsFinished)
                {
                    isCreating = false;
                    if (StateText != null)
                    {
                        StateText.text = "影片輸出完成";
                        string tmpPath = outDir + "/tmp";
                        if (Directory.Exists(tmpPath)) {
                            Directory.Delete(tmpPath, true);
                        }
                        ExportFinish();
                    }
                }
                else
                {
                    if (StateText != null)
                    {
                       //StateText.text = "影片輸出中：" + ffCaptureCmd.RemainingTime.ToString();
                    }
                }
            }
            if (waitToNext > 0)
            {
                waitToNext -= 1;

                if (waitToNext <= 0)
                {
                    Time.timeScale = 1f;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RuntimeWindow gameView = Editor.GetWindow(RuntimeWindowType.Game);

            if(gameView != null)
            {
                gameViewRegion = gameView.GetComponentInParent<Region>();
                if (gameViewRegion != null)
                {
                    gameViewRegion.CloseAllTabs();
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            /*
            RuntimeWindow gameView = Editor.GetWindow(RuntimeWindowType.Game);
            if (gameView == null)
            {
                Editor.CreateWindow(RuntimeWindowType.Game.ToString());
            }
            */

            if (gameViewRegion != null)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                LayoutInfo Game = wm.CreateLayoutInfo(BuiltInWindowNames.Game);
                gameViewRegion.Build(Game);

                SeVM.NodeEditor.VideoExportVM = null;

            }
        }
        public void OnPlayedFrame()
        {
            if (SeVM.isRecording && SeVM.RecordState == "RecAudio")
            {

                exportFrame += 1;
                float pc = ((float)exportFrame / (float)exportTotalFrame)/3;

                Debug.Log("OnPlayedFrame RecAudio:" + exportFrame + "," + exportTotalFrame);
                SetExportPc(pc);

            }
            if (SeVM.isRecording && SeVM.RecordState == "RecFrame")
            {
                SeVM.NodeEditor.onRecordPause();
                Time.timeScale = 0f;
                drawOneFrame();
                Debug.Log("OnPlayedFrame RecFrame:" + exportFrame +","+ exportTotalFrame);
                float pc = .4f + (((float)exportFrame / (float)exportTotalFrame) / 4f);
                SetExportPc(pc);

            }
            captureAudioSample();

        }

        public void captureAudioSample()
        {
            if(m_audiolistener != null)
            {
                
            }
        }

        void drawOneFrame()
        {

            if (rt != null)
            {

                if(NodeVideoPlayers.Count > 0)
                {

                }
                float vframrate;
                for (int i = 0; i < NodeVideoPlayers.Count; i++)
                {
                    vframrate = exportCutFrame * ( NodeVideoPlayers[i].frameRate/ framerate);



                    exportCutFrameOfRate = (int)(vframrate + 1);
                    Debug.Log("exportCutFrameOfRate:" + exportCutFrameOfRate);
                    NodeVideoPlayers[i].frame = exportCutFrameOfRate;
                    NodeVideoPlayers[i].Pause();
                }


                isCreating = false;
                if (StateText != null)
                {
                    //StateText.text = "影片製作中:"+ exportFrame +"/"+ exportTotalFrame;
                    //GetNodesByStory
                }

                if (m_camera != null)
                {
                    m_camera.Render();

                }
                if (m_preview != null)
                {
                    m_preview.texture = rt;

                }
                exportCutFrame += 1;
                exportFrame += 1;


                if (exportFrame >= 1 && exportFrame < 300)
                {
#if UNITY_EDITOR
                       // UnityEditor.EditorApplication.isPaused = true;
#endif
                }

                byte[] bytes = toTexture2D(rt).EncodeToPNG();
                outpngid += 1;
                string filename = outpngid.ToString().PadLeft(6, '0'); outpngid.ToString();
                string path = outDir + "/tmp/" + filename + ".png";



                System.IO.File.WriteAllBytes(path, bytes);


                waitToNext = 10;

            }
        }
        

        Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(outWidth, outHeight, TextureFormat.RGB24, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }


}



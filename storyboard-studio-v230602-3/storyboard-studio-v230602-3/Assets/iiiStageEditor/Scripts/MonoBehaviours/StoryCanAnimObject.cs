using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Battlehub.RTEditor;

using NLayer;

public class StoryCanAnimObject : MonoBehaviour
{

    public enum ObjectType
    {
        GameObject,Hotspot, Instruct,StageScene
    }
    public ObjectType type = ObjectType.GameObject;

    public bool CanAni = true;
    //public bool CanAni = false;
    public bool CanChangeTex = true;
    public bool CanAddAudio = true;

    /*
    public Vector3 rotatSet = new Vector3(0, 0, 0);
    public Vector3 rotatSet2 = new Vector3(0, 0, 0);
    */

    public int OnSetAniID = 0;
    public float OnSetAniTime = 0;
    public string audioLink = "";
    public string authorLink = "";
    public int authorTarget = 0;

    public string title = "";
    public string desctext = "";


    private AudioSource audioSource;
    private AudioClip audioClip;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setAudioPath(string path)
    {
        audioLink = path;

        //updateIfHavAudio();
    }
    public void updateIfHavAudio()
    {
        Debug.Log("updateIfHavAudio");

        if(audioLink != null)
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }


            string ext = Path.GetExtension(audioLink);
            if (ext == ".mp3")
            {

                audioClip = LoadMp3(audioLink);

            }
            else if (ext == ".wav")
            {
                audioClip = LoadWav(audioLink);

                if (audioClip == null)
                {
                    audioLink = "";
                }
            }

            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
        

    }
    private void OnEnable()
    {
        //updateIfHavAudio();
    }
    public static AudioClip LoadMp3(string filePath)
    {
        string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);

        MpegFile mpegFile = new MpegFile(filePath);

        // assign samples into AudioClip
        AudioClip ac = AudioClip.Create(filename,
                                        (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels),
                                        mpegFile.Channels,
                                        mpegFile.SampleRate,
                                        true,
                                        data => { int actualReadCount = mpegFile.ReadSamples(data, 0, data.Length); },
                                        position => { mpegFile = new MpegFile(filePath); });

       

        return ac;
    }

    public static AudioClip  LoadWav(string filePath)
    {
        AudioClip audioClip = null;

        if (File.Exists(filePath))
        {
            byte[] wavFile = File.ReadAllBytes(filePath);
            audioClip  = OpenWavParser.ByteArrayToAudioClip(wavFile);
        }
        else
        {
            Debug.Log("File not found");
        }
        return audioClip;

    }

    public void LoadWavFile(string path)
    {
        //string path = string.Format("{0}/{1}", Application.persistentDataPath, filename);
        AudioClip audioClip = WavUtility.ToAudioClip(path);
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    IEnumerator GetAudioToClip(string fullPath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
    }

    private void OnRuntimeEditorOpened()
    {
        //Debug.Log("Editor Opened");
    }

    private void OnRuntimeEditorClosed()
    {
        //Debug.Log("Editor Closed");
    }

    private void RuntimeAwake()
    {
        //Debug.Log("Awake in play mode");
    }

    private void RuntimeStart()
    {
        updateIfHavAudio();
        //Debug.Log("Start in play mode");
    }

    private void OnRuntimeDestroy()
    {
        //Debug.Log("Destroy in play mode");
    }

    private void OnRuntimeActivate()
    {
        //Debug.Log("Game View activated");
    }

    private void OnRuntimeDeactivate()
    {
        //Debug.Log("Game View deactivated");
    }

}

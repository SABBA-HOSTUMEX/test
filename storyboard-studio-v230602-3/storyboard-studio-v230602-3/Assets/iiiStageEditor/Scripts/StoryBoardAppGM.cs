using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using UnityEngine.UI;

public class StoryBoardAppGM : MonoBehaviour
{
    private static StoryBoardAppGM _instance;
    public static StoryBoardAppGM Instance { get { return _instance; } }
	// Start is called before the first frame update
	public List<ExposeToEditor> StoryCutGos =new List<ExposeToEditor>();
	public GameObject EffectorPrefab;
	public GameObject GameObject2DPredfab;
	public GameObject HumanActionOPrefab;
	public Animator animator;

	public PoseAniEditor AniEditor;
	public Battlehub.Localization localization;


	public  HumanBodyBones[] Bodybones = new[]{
		HumanBodyBones.Neck,
		HumanBodyBones.Head,

		HumanBodyBones.Hips,
		HumanBodyBones.Spine,
		HumanBodyBones.Chest,
		HumanBodyBones.UpperChest,

		HumanBodyBones.LeftShoulder,
		HumanBodyBones.LeftUpperArm,
		HumanBodyBones.LeftLowerArm,
		HumanBodyBones.LeftHand,

		HumanBodyBones.RightShoulder,
		HumanBodyBones.RightUpperArm,
		HumanBodyBones.RightLowerArm,
		HumanBodyBones.RightHand,

		HumanBodyBones.LeftUpperLeg,
		HumanBodyBones.LeftLowerLeg,
		HumanBodyBones.LeftFoot,
		HumanBodyBones.LeftToes,

		HumanBodyBones.RightUpperLeg,
		HumanBodyBones.RightLowerLeg,
		HumanBodyBones.RightFoot,
		HumanBodyBones.RightToes,
	};
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}


		DontDestroyOnLoad(this.gameObject);
		if(PlayerPrefs.GetString("LocaleSet") != null)
        {
			localization.Locale = PlayerPrefs.GetString("LocaleSet");
		}
		//
	}

	public CanvasScaler[] CanvasScalers;

	
	void Start()
    {

		int qualityLevel = QualitySettings.GetQualityLevel();
		Debug.Log("qualityLevel:" + qualityLevel);
	}
	public void OnLangChanged(int langid)
    {
		//localization.LoadStringResources("RTEditor.StringResources.en-US");
	}
	
}

public enum TransitMode
{
	None, Alpha, Deg45, Vertical, Diamond, Textfade
}

[Serializable]
public class StoryBrdData
{
	public int version;
	public string type;
	public StoryCut[] cuts;
	public StoryCut minimap;
}
[Serializable]
public class StoryCut
{
	public int index;
	public string n_type;
	public string m_type;
	public string m_path;
	public string m_filename;
	public string m_name;
	public string m_img;
	public string video_url;
	public int m_time;
	public List<StoryVideodata> videos;
	public List<StoryAudiodata> audios;
	public List<StoryAnidata> anims;
	public List<StoryLinkdata> links;
	public List<StoryInfodata> infos;
}


[Serializable]
public class StoryLinkdata
{
	public string m_name;
	public string link_url;
}

[Serializable]
public class StoryInfodata
{
	public string m_name;
	public string m_title;
	public string m_desc;
	public string m_link;
	public int extlink;
}

[Serializable]
public class StoryVideodata
{
	public string m_name;
	public string video_url;
}

[Serializable]
public class StoryAudiodata
{
	public string m_name;
	public string audio_url;
}

[Serializable]
public class StoryAnidata
{
	public string m_name;
	public string anim_name;
}

[Serializable]
public class OnAniFrameTrans
{
	public Transform transform;
	public int frame;
	public Vector3 m_lpos;
	public Vector3 m_pos;
	public Vector3 m_scale;
	public Vector3 m_angle;
	public Vector3 m_langle;
	public Quaternion m_rot;
	public Quaternion m_lrot;

}

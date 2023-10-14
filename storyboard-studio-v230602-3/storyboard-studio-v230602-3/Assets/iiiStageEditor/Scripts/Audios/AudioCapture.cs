using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.IO;
using UnityEngine;
using iiiStoryEditor.UI.ViewModels.ViewModels;
[RequireComponent(typeof(AudioListener))]
public class AudioCapture : MonoBehaviour
{
	public static AudioCapture Instance { get; private set; }

	public static string fileName;
	public static bool IsRecording { get; private set; }
	public static bool IsSaveAudioOnApplicationQuit = true;
	public const int OutputSampleRate = 48000;

	private const int headerSize = 44;
	private FileStream fileStream;
	public bool recordFile = true;


	public void StartRecording()
	{
		if (IsRecording)
		{
			Debug.LogError("The recording cannot be started because it is already running");
			return;
		}
		IsRecording = true;
		
		CreateAudioFile();

		Debug.Log("Recording started");
	}

	/// <summary>
	/// Stops and saves recorded audio
	/// </summary>
	public void StopRecording()
	{
		if (!IsRecording)
		{
			return;
		}
		IsRecording = false;
		SaveAudioFile();

	}

	/// <summary>
	/// Creates an audio file at the specified path and fills it with empty bytes for now
	/// </summary>
	private void CreateAudioFile()
	{
		string allFilePath = fileName;
		fileStream = new FileStream(allFilePath, FileMode.Create);
		for (int i = 0; i < headerSize; i++)
			fileStream.WriteByte(new byte());
	}

	/// <summary>
	/// The algorithm for saving audio in the WAVE format
	/// </summary>
	private void SaveAudioFile()
	{

		if(true)
        {
			fileStream.Seek(0, SeekOrigin.Begin);
			fileStream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
			fileStream.Write(BitConverter.GetBytes(fileStream.Length - 8), 0, 4);
			fileStream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
			fileStream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
			fileStream.Write(BitConverter.GetBytes(16), 0, 4);
			fileStream.Write(BitConverter.GetBytes(1), 0, 2);
			fileStream.Write(BitConverter.GetBytes(2), 0, 2);
			fileStream.Write(BitConverter.GetBytes(OutputSampleRate), 0, 4);
			fileStream.Write(BitConverter.GetBytes(OutputSampleRate * 4), 0, 4);
			fileStream.Write(BitConverter.GetBytes(4), 0, 2);
			fileStream.Write(BitConverter.GetBytes(16), 0, 2);
			fileStream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
			fileStream.Write(BitConverter.GetBytes(fileStream.Length - headerSize), 0, 4);
			fileStream.Close();
			fileStream.Dispose();
		}
		
	}

	/// <summary>
	/// This is called by unity when bunch of audio samples gets accumulates
	/// </summary>
	/// <param name="data"></param>
	/// <param name="channels"></param>
	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (!IsRecording)
			return;


		Debug.Log("OnAudioFilterRead");
		byte[] bytesData = new byte[data.Length * 2];
		for (int i = 0; i < data.Length; i++)
			BitConverter.GetBytes((short)(data[i] * 32767)).CopyTo(bytesData, i * 2);
		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	/// <summary>
	/// Automatic file saving when exiting the application
	/// </summary>
	private void OnApplicationQuit()
	{
		if (!IsSaveAudioOnApplicationQuit || !IsRecording)
			return;
		StopRecording();
	}

	/// <summary>
	/// Preparing for this instance to work
	/// </summary>
	private void Start()
	{
		/*
		if (Instance == null)
			Instance = this;
		else
			Destroy(Instance.gameObject);
		AudioSettings.outputSampleRate = OutputSampleRate;

		// play sounds that are marked as PlayOnAwake
		AudioSource[] allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		for (int i = 0; i < allAudioSources.Length; i++)
			if (allAudioSources[i].playOnAwake)
				allAudioSources[i].Play();

		*/
	}
}
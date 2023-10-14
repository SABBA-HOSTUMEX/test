using UnityEngine;
using UnityEngine.Video;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using System.Threading.Tasks;
using System.IO;
using Battlehub.RTSL.Interface;
using System;
using System.Threading;

using UnityObject = UnityEngine.Object;

namespace iiiStoryEditor.RTImporter
{


    public class VideoImporterAsync : ProjectFileImporterAsync
    {
        public override string FileExt
        {
            get { return ".mp4"; }
        }

        public override string IconPath
        {
            get { return "Icons/mp4"; }
        }

        public override int Priority
        {
            get { return int.MinValue; }
        }

        public override Type TargetType
        {
            get { return typeof(VideoClip); }
        }

        public override async Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken)
        {
            // byte[] bytes = filePath.Contains("://") ? await DownloadBytesAsync(filePath) : File.ReadAllBytes(filePath);

            GameObject StoryBrdGMObj = GameObject.FindGameObjectWithTag("StoryEditorGM");

            if(StoryBrdGMObj != null)
            {
                StoryBoardAppGM StoryBrdGM = StoryBrdGMObj.GetComponent<StoryBoardAppGM>();

                if(StoryBrdGM != null && StoryBrdGM.GameObject2DPredfab != null)
                {

                    GameObject go = GameObject.Instantiate(StoryBrdGM.GameObject2DPredfab);

                    VideoPlayer videoPlayer = go.AddComponent<VideoPlayer>();
                    videoPlayer.playOnAwake = false;
                    videoPlayer.url = filePath;
                    videoPlayer.isLooping = true;
                    videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
                    //videoPlayer.targetMaterialRenderer = renderder;
                    //videoPlayer.Play();
                    try
                    {
                        if (go != null)
                        {
                            Debug.Log("mp4mp4");
                            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
                            byte[] preview = previewUtility.CreatePreviewData(videoPlayer);

                            using (await project.LockAsync())
                            {
                                await project.SaveAsync(targetPath, go, preview);
                            }
                        }
                        else
                        {
                            throw new FileImporterException($"Unable to load audio {filePath}");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FileImporterException(e.Message, e);
                    }
                    finally
                    {
                        // UnityObject.Destroy(videoClip);
                    }

                }
                

            }
            
            
        }

        private float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(array, i * 4, 4);
                floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
            }
            return floatArr;
        }

    }
}

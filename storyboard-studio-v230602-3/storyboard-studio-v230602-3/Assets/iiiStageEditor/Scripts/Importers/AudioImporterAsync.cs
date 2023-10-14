using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using System.Threading.Tasks;
using System.IO;
using Battlehub.RTSL.Interface;
using System;
using System.Threading;
using System;

using UnityObject = UnityEngine.Object;

namespace iiiStoryEditor.RTImporter
{
    public class wavImporterAsync : mp3ImporterAsync
    {
        public override string FileExt
        {
            get { return ".wav"; }
        }
    }


    public class mp3ImporterAsync : ProjectFileImporterAsync
    {
        public override string FileExt
        {
            get { return ".mp3"; }
        }

        public override string IconPath
        {
            get { return "Importers/Png"; }
        }

        public override int Priority
        {
            get { return int.MinValue; }
        }

        public override Type TargetType
        {
            get { return typeof(Texture2D); }
        }

        public override async Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken)
        {
            byte[] bytes = filePath.Contains("://") ?
                await DownloadBytesAsync(filePath) :
                File.ReadAllBytes(filePath);
            AudioClip audioClip;


            try
            {

                //float[] f = ConvertByteToFloat(bytes);
                audioClip = OpenWavParser.ByteArrayToAudioClip(bytes);


                if (audioClip != null)
                {
                    
                    IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
                    byte[] preview = previewUtility.CreatePreviewData(audioClip);

                    using (await project.LockAsync())
                    {
                        await project.SaveAsync(targetPath, audioClip, preview);
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
               // UnityObject.Destroy(audioClip);
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

using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using System.Threading.Tasks;
using System.IO;
using Battlehub.RTSL.Interface;
using System;
using System.Threading;
using System;
using UniVRM10;

using UnityObject = UnityEngine.Object;

namespace iiiStoryEditor.RTImporter
{

    public class VrmImporterAsync : ProjectFileImporterAsync
    {
        public override string FileExt
        {
            get { return ".vrm"; }
        }

        public override string IconPath
        {
            get { return "Importers/vrm"; }
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
            Vrm10Instance vrm10Instance = await Vrm10.LoadPathAsync(filePath);
            GameObject StoryBrdGMObj = GameObject.FindGameObjectWithTag("StoryEditorGM");
            try
            {

                if (vrm10Instance != null)
                {
                    vrm10Instance.gameObject.AddComponent<ExposeToEditor>();
                    Animator animator = vrm10Instance.gameObject.GetComponent<Animator>();
                    Avatar avatar;
                    IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
                    
                    if (animator != null)
                    {
                        if (StoryBrdGMObj != null)
                        {

                            StoryBoardAppGM StoryBrdGM = StoryBrdGMObj.GetComponent<StoryBoardAppGM>();
                            if (StoryBrdGM != null) {

                                if (StoryBrdGM.animator != null)
                                {
                                    animator.runtimeAnimatorController = StoryBrdGM.animator.runtimeAnimatorController;
                                }
                                else
                                {

                                }

                            }
                            else
                            {
                               // Debug.LogWarning("StoryBrdGM == null");
                            }
                        }
                        StoryCharAnimObject storyCharAnimObject = vrm10Instance.gameObject.AddComponent<StoryCharAnimObject>();
                        storyCharAnimObject.vrmpath = filePath;


                    }
                    else
                    {

                       //Debug.LogWarning("StoryBrdGMObj == null", animator);
                    }

                    

                    byte[] preview = previewUtility.CreatePreviewData(vrm10Instance);

                    using (await project.LockAsync())
                    {
                        await project.SaveAsync(targetPath, vrm10Instance, preview);
                    }

                }
                else
                {
                    throw new FileImporterException($"Unable to load vrm {filePath}");
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


    }
}

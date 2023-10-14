using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using System.Threading.Tasks;
using System.IO;
using Battlehub.RTSL.Interface;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using TriLibCore;
using TriLibCore.Extensions;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace iiiStoryEditor.RTImporter
{
    public class FbxImporterAsync : ProjectFileImporterAsync
    {

        string _filePath = "";
        string _targetPath = "";
        IProjectAsync _project = null;
        private List<AnimationClip> _animations;
        private Animation _animation;
        [SerializeField]
        protected Dropdown PlaybackAnimation;

        public override int Priority
        {
            get { return int.MinValue; }
        }
        public override string FileExt
        {
            get { return ".fbx"; }
        }

        public override string IconPath
        {
            get { return "Importers/FBX"; }
        }

        public override Type TargetType
        {
            get { return typeof(GameObject); }
        }

        public override async Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken)
        {
            _targetPath = targetPath;
            _project = project;


            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;
            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            AssetLoader.LoadModelFromFile(filePath, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);

        }

       private void OnError(IContextualizedError obj)
        {
            Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
        }

        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {
            Debug.Log($"Loading Model. Progress: {progress:P}");
            Debug.Log(assetLoaderContext);
        }

        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            Debug.Log("Materials loaded. Model fully loaded.");
        }

        private void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            Debug.Log("Model loaded. Loading materials.");

            _animation = null;
            if (assetLoaderContext != null && assetLoaderContext.RootGameObject != null)
            {
                _animation = assetLoaderContext.RootGameObject.GetComponent<Animation>();
                if (_animation != null)
                {
                    
                    _animations = _animation.GetAllAnimationClips();
                     
                    Debug.Log("_animations.Count:"+ _animations.Count);
                    if (_animations.Count > 0)
                    {
                        //PlaybackAnimation.interactable = true;
                        for (var i = 0; i < _animations.Count; i++)
                        {
                            var animationClip = _animations[i];
                        }


                        if(_animations.Count > 1)
                        {
                            AnimationClip clip = _animations[0];
                            if (clip.name == "Take 001")
                            {
                                _animations.RemoveAt(0);

                            }
                        }

                        if(_animations[0] != null)
                        {

                            _animation.clip = _animations[0];
                            _animation.playAutomatically = true;
                            _animation.Play();

                        }
                    }
                    else
                    {
                        _animation = null;
                    }

                }

                //PlaybackAnimation.value = 0;
                // StopAnimation();
                // RootGameObject = assetLoaderContext.RootGameObject;
            }
            if (_animation == null)
            {
                //PlaybackAnimation.interactable = false;
                //PlaybackAnimation.captionText.text = "No Animations";
                Debug.Log("No Animations");
            }
            GameObject go = assetLoaderContext.RootGameObject;
            go.AddComponent<ExposeToEditor>();
            go.AddComponent<StoryCanAnimObject>();
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            ProjectItem folder = _project.Utils.GetFolder(Path.GetDirectoryName(_targetPath));
            //editor.CreatePrefabAsync(folder, go.GetComponent<ExposeToEditor>(), true);

            IRTE editor2 = IOC.Resolve<IRTE>();
            editor2.IsBusy = false;
        }

    }
}

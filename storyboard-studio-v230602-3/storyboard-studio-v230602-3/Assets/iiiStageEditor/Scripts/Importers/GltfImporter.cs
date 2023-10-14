using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.RTCommon;
using System.Threading.Tasks;
using System.IO;
using Battlehub.RTSL.Interface;
using System;
using System.Threading;

using UnityObject = UnityEngine.Object;
using Battlehub.RTEditor.Models;

using System.Collections.Generic;
using TriLibCore;
using TriLibCore.Extensions;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;

namespace iiiStoryEditor.RTImporter
{
    
    public class GlbImporterAsync : GltfImporterAsync
    {
        public override string FileExt
        {
            get { return ".glb"; }
        }
    }

    public class GltfImporterAsync : ProjectFileImporterAsync
    {

        string _filePath = "";
        string _targetPath = "";
        IProjectAsync _project = null;
        private List<AnimationClip> _animations;
        private Animation _animation;



        public override int Priority
        {
            get { return int.MinValue; }
        }

        public override string FileExt
        {
            get { return ".gltf"; }
        }

        public override string IconPath
        {
            get { return "Importers/GLTF"; }
        }

        public override Type TargetType
        {
            get { return typeof(GameObject); }
        }

        public override async Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken)
        {

            _targetPath = targetPath;
            _project = project;
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

                    Debug.Log("_animations.Count:" + _animations.Count);
                    if (_animations.Count > 0)
                    {
                        for (var i = 0; i < _animations.Count; i++)
                        {
                            var animationClip = _animations[i];
                        }
                        if (_animations[0] != null)
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

            }
            if (_animation == null)
            {
                Debug.Log("No Animations");
            }
            GameObject go = assetLoaderContext.RootGameObject;
            go.AddComponent<ExposeToEditor>();
            go.AddComponent<StoryCanAnimObject>();
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            ProjectItem folder = _project.Utils.GetFolder(Path.GetDirectoryName(_targetPath));
            editor.CreatePrefabAsync(folder, go.GetComponent<ExposeToEditor>(), true);

        }

    }
}

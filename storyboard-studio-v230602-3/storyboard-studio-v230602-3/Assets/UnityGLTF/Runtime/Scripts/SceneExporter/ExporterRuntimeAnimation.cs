#if UNITY_EDITOR
#define ANIMATION_EXPORT_SUPPORTED
#else
#endif
#define ANIMATION_NOT_SUPPORTED
#if ANIMATION_EXPORT_SUPPORTED && (UNITY_ANIMATION || !UNITY_2019_1_OR_NEWER)
#define ANIMATION_SUPPORTED
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GLTF.Schema;
using UnityEngine;
using UnityEngine.Playables;
using UnityGLTF.Extensions;
using Object = UnityEngine.Object;
using Battlehub.RTEditor;


namespace UnityGLTF
{
	public partial class GLTFSceneExporter
	{
		#if ANIMATION_NOT_SUPPORTED
			private readonly Dictionary<(AnimationClip clip, float speed), GLTFAnimation> _clipToAnimation = new Dictionary<(AnimationClip, float), GLTFAnimation>();
			private readonly Dictionary<(AnimationClip clip, float speed, string targetPath), Transform> _clipAndSpeedAndPathToExportedTransform = new Dictionary<(AnimationClip, float, string), Transform>();
		
			private static int AnimationBakingFramerate = 30; // FPS
			private static bool BakeAnimationData = true;


		#endif
		public void ExportRuntimeAnimationFromNode(ref Transform transform)
		{
			exportAnimationFromNodeMarker.Begin();


		#if ANIMATION_NOT_SUPPORTED
			RuntimeAnimation runtimeAnimation = transform.GetComponent<RuntimeAnimation>();
			if (runtimeAnimation != null && runtimeAnimation.Clips != null) {
				ExportRuntimeAnimationClips(transform, runtimeAnimation.Clips);
			}
		#endif

			exportAnimationFromNodeMarker.End();
		}

		public void ExportRuntimeAnimationClips(Transform nodeTransform, IList<RuntimeAnimationClip> clips)
		{
			for (int i = 0; i < clips.Count; i++)
			{
				if (!clips[i]) continue;
				var speed = 1f;
				ExportRuntimeAnimationClip(clips[i], clips[i].name, nodeTransform, speed);
			}
		}

		public GLTFAnimation ExportRuntimeAnimationClip(RuntimeAnimationClip runtimeclip, string name, Transform node, float speed)
		{
			if (!runtimeclip || !runtimeclip.Clip) return null;
			GLTFAnimation anim = GetOrCreateRuntimrAnimation(runtimeclip, name, speed);

			anim.Name = name;

			ConvertRuntimeClipToGLTFAnimation(runtimeclip, node, anim, speed);

			if ( !_root.Animations.Contains(anim))//anim.Channels.Count > 0 && anim.Samplers.Count > 0 &&
			{
				_root.Animations.Add(anim);
				_animationClips.Add((node, runtimeclip.Clip));

			}
			return anim;
		}

		private GLTFAnimation GetOrCreateRuntimrAnimation(RuntimeAnimationClip runtimeclip, string searchForDuplicateName, float speed)
		{
			var existingAnim = default(GLTFAnimation);
			if (_exportOptions.MergeClipsWithMatchingNames)
			{
				// Check if we already exported an animation with exactly that name. If yes, we want to append to the previous one instead of making a new one.
				// This allows to merge multiple animations into one if required (e.g. a character and an instrument that should play at the same time but have individual clips).
				existingAnim = _root.Animations?.FirstOrDefault(x => x.Name == searchForDuplicateName);
			}

			// TODO when multiple AnimationClips are exported, we're currently not properly merging those;
			// we should only export the GLTFAnimation once but then apply that to all nodes that require it (duplicating the animation but not the accessors)
			// instead of naively writing over the GLTFAnimation with the same data.
			var animationClipAndSpeed = (runtimeclip.Clip, speed);
			if (existingAnim == null)
			{
				if (_clipToAnimation.TryGetValue(animationClipAndSpeed, out existingAnim))
				{
					// we duplicate the clip it was exported before so we can retarget to another transform.
					existingAnim = new GLTFAnimation(existingAnim, _root);
				}
			}

			GLTFAnimation anim = existingAnim != null ? existingAnim : new GLTFAnimation();

			// add to set of already exported clip-state pairs
			if (!_clipToAnimation.ContainsKey(animationClipAndSpeed))
				_clipToAnimation.Add(animationClipAndSpeed, anim);

			return anim;
		}

		private void ConvertRuntimeClipToGLTFAnimation(RuntimeAnimationClip runtimeclip, Transform transform, GLTFAnimation animation, float speed)
		{
			convertClipToGLTFAnimationMarker.Begin();

			AnimationClip clip = runtimeclip.Clip;

			// 1. browse clip, collect all curves and create a TargetCurveSet for each target
			Dictionary<string, TargetCurveSet> targetCurvesBinding = new Dictionary<string, TargetCurveSet>();

			#if ANIMATION_NOT_SUPPORTED
			CollectRuntimeClipCurves(transform.gameObject, runtimeclip, targetCurvesBinding);
			// Baking needs all properties, fill missing curves with transform data in 2 keyframes (start, endTime)
			// where endTime is clip duration
			// Note: we should avoid creating curves for a property if none of it's components is animated


			GenerateMissingRuntimeCurves(clip.length, transform, ref targetCurvesBinding);

			#endif


			if (BakeAnimationData)
			{

				// Bake animation for all animated nodes
				foreach (string target in targetCurvesBinding.Keys)
				{
					var hadAlreadyExportedThisBindingBefore = _clipAndSpeedAndPathToExportedTransform.TryGetValue((clip, speed, target), out var alreadyExportedTransform);


					Transform targetTr = target.Length > 0 ? transform.Find(target) : transform;
					int newTargetId = targetTr ? GetTransformIndex(targetTr) : -1;



					var targetTrShouldNotBeExported = targetTr && !targetTr.gameObject.activeInHierarchy && !settings.ExportDisabledGameObjects;

					if (hadAlreadyExportedThisBindingBefore && newTargetId < 0)
					{
						// warn: the transform for this binding exists, but its Node isn't exported. It's probably disabled and "Export Disabled" is off.
						if (targetTr)
						{
							Debug.LogWarning("An animated transform is not part of _exportedTransforms, is the object disabled? " + LogObject(targetTr), targetTr);
						}

						// we need to remove the channels and samplers from the existing animation that was passed in if they exist
						int alreadyExportedChannelTargetId = GetTransformIndex(alreadyExportedTransform);
						animation.Channels.RemoveAll(x => x.Target.Node != null && x.Target.Node.Id == alreadyExportedChannelTargetId);

						if (settings.UseAnimationPointer)
						{
							animation.Channels.RemoveAll(x =>
							{
								if (x.Target.Extensions != null && x.Target.Extensions.TryGetValue(KHR_animation_pointer.EXTENSION_NAME, out var ext) && ext is KHR_animation_pointer animationPointer)
								{
									var obj = animationPointer.animatedObject;
									if (obj is Component c)
										obj = c.transform;
									if (obj is Transform tr2 && tr2 == alreadyExportedTransform)
										return true;
								}
								return false;
							});
						}

						// TODO remove all samplers from this animation that were targeting the channels that we just removed
						// TODO: this doesn't work because we're punching holes in the sampler order; all channel sampler IDs would need to be adjusted as well.

						continue;
					}

					if ( hadAlreadyExportedThisBindingBefore)
					{
						int alreadyExportedChannelTargetId = GetTransformIndex(alreadyExportedTransform);

						for (int i = 0; i < animation.Channels.Count; i++)
						{
							var existingTarget = animation.Channels[i].Target;
							if (existingTarget.Node != null && existingTarget.Node.Id != alreadyExportedChannelTargetId) continue;

							// if we're here it means that an existing AnimationChannel already targets the same node that we're currently targeting.
							// Without KHR_animation_pointer, that just means we reuse the existing data and tell it to target a new node.
							// With KHR_animation_pointer, we need to do the same, and retarget the path to the new node.
							if (existingTarget.Extensions != null && existingTarget.Extensions.TryGetValue(KHR_animation_pointer.EXTENSION_NAME, out var ext) && ext is KHR_animation_pointer animationPointer)
							{
								// Debug.Log($"export? {!targetTrShouldNotBeExported} - {nameof(existingTarget)}: {L(existingTarget)}, {nameof(animationPointer)}: {L(animationPointer.animatedObject)}, {nameof(alreadyExportedTransform)}: {L(alreadyExportedTransform)}, {nameof(targetTr)}: {L(targetTr)}");
								var obj = animationPointer.animatedObject;
								Transform animatedTransform = default;
								if (obj is Component comp) animatedTransform = comp.transform;
								else if (obj is GameObject go) animatedTransform = go.transform;
								if (animatedTransform == alreadyExportedTransform)
								{
									if (targetTrShouldNotBeExported)
									{
										// Debug.LogWarning("Need to remove this", null);
									}
									else
									{
										if (animationPointer.animatedObject is GameObject)
										{
											animationPointer.animatedObject = targetTr.gameObject;
											animationPointer.channel = existingTarget;
											animationPointerResolver.Add(animationPointer);
										}
										else if (animationPointer.animatedObject is Component)
										{
											var targetType = animationPointer.animatedObject.GetType();
											var newTarget = targetTr.GetComponent(targetType);
											if (newTarget)
											{
												animationPointer.animatedObject = newTarget;
												animationPointer.channel = existingTarget;
												animationPointerResolver.Add(animationPointer);
											}
										}
									}
								}
								else if (animationPointer.animatedObject is Material m)
								{
									var renderer = targetTr.GetComponent<MeshRenderer>();
									if (renderer)
									{
										// TODO we don't have a good way right now to solve this if there's multiple materials on this renderer...
										// would probably need to keep the clip path / binding around and check if that uses a specific index and so on
										var newTarget = renderer.sharedMaterial;
										if (newTarget)
										{
											animationPointer.animatedObject = newTarget;
											animationPointer.channel = existingTarget;
											animationPointerResolver.Add(animationPointer);
										}
									}
								}
							}
							else if (targetTr)
							{
								existingTarget.Node = new NodeId()
								{
									Id = newTargetId,
									Root = _root
								};
							}
						}
						continue;
					}

					if (targetTrShouldNotBeExported)
					{
						Debug.Log("Object " + targetTr + " is disabled, not exporting animated curve " + target, targetTr);
						continue;
					}

					// add to cache: this is the first time we're exporting that particular binding.
					if (targetTr)
						_clipAndSpeedAndPathToExportedTransform.Add((clip, speed, target), targetTr);

					var curve = targetCurvesBinding[target];
					var speedMultiplier = Mathf.Clamp(speed, 0.01f, Mathf.Infinity);

					// Initialize data
					// Bake and populate animation data
					float[] times = null;

					// arbitrary properties require the KHR_animation_pointer extension
					bool sampledAnimationData = false;
					if (settings.UseAnimationPointer && curve.propertyCurves != null && curve.propertyCurves.Count > 0)
					{
						var curves = curve.propertyCurves;
						foreach (KeyValuePair<string, PropertyCurve> c in curves)
						{
							var prop = c.Value;
							if (BakePropertyAnimation(prop, clip.length, AnimationBakingFramerate, speedMultiplier, out times, out var values))
							{
								AddAnimationData(prop.target, prop.propertyName, animation, times, values);

								sampledAnimationData = true;
							}
						}
					}

					// TODO these should be moved into curve.propertyCurves as well
					// TODO should filter by possible propertyCurve string names at that point to avoid
					// moving KHR_animation_pointer data into regular animations
					if (curve.translationCurves.Any(x => x != null))
					{
						var trp2 = new PropertyCurve(targetTr, "translation") { propertyType = typeof(Vector3) };
						trp2.curve.AddRange(curve.translationCurves);
						if (BakePropertyAnimation(trp2, clip.length, AnimationBakingFramerate, speedMultiplier, out times, out var values2))
						{
							AddAnimationData(targetTr, trp2.propertyName, animation, times, values2);
							sampledAnimationData = true;
						}
					}

					if (curve.rotationCurves.Any(x => x != null))
					{
						var trp3 = new PropertyCurve(targetTr, "rotation") { propertyType = typeof(Quaternion) };
						trp3.curve.AddRange(curve.rotationCurves.Where(x => x != null));
						if (BakePropertyAnimation(trp3, clip.length, AnimationBakingFramerate, speedMultiplier, out times, out var values3))
						{
							AddAnimationData(targetTr, trp3.propertyName, animation, times, values3);
							sampledAnimationData = true;
						}

					}

					if (curve.scaleCurves.Any(x => x != null))
					{
						var trp4 = new PropertyCurve(targetTr, "scale") { propertyType = typeof(Vector3) };
						trp4.curve.AddRange(curve.scaleCurves);
						if (BakePropertyAnimation(trp4, clip.length, AnimationBakingFramerate, speedMultiplier, out times, out var values4))
						{
							AddAnimationData(targetTr, trp4.propertyName, animation, times, values4);
							sampledAnimationData = true;
						}
					}

					if (curve.weightCurves.Any(x => x.Value != null))
					{
						var trp5 = new PropertyCurve(targetTr, "weights") { propertyType = typeof(float) };
						trp5.curve.AddRange(curve.weightCurves.Values);
						if (BakePropertyAnimation(trp5, clip.length, AnimationBakingFramerate, speedMultiplier, out times, out var values5))
						{
							var targetComponent = targetTr.GetComponent<SkinnedMeshRenderer>();
							AddAnimationData(targetComponent, trp5.propertyName, animation, times, values5);
							sampledAnimationData = true;
						}
					}

					if (!sampledAnimationData)
						Debug.LogWarning("Warning: empty animation curves for " + target + " in " + clip + " from " + transform, transform);
				}
			}
			else
			{
				Debug.LogError("Only baked animation is supported for now. Skipping animation", null);
			}
			convertClipToGLTFAnimationMarker.End();
		}



#if ANIMATION_NOT_SUPPORTED

		public enum AnimationKeyRotationType
		{
			Unknown,
			Quaternion,
			Euler
		}

		public class PropertyCurve
		{
			public string propertyName;
			public Type propertyType;
			public List<AnimationCurve> curve;
			public List<string> curveName;
			public Object target;

			public PropertyCurve(Object target, string propertyName)
			{
				this.target = target;
				this.propertyName = propertyName;
				curve = new List<AnimationCurve>();
				curveName = new List<string>();
			}

			public void AddCurve(AnimationCurve animCurve, string name)
			{
				this.curve.Add(animCurve);
				this.curveName.Add(name);
			}

			public float Evaluate(float time, int index)
			{
				if (index < 0 || index >= curve.Count)
				{
					// common case: A not animated but RGB is.
					// TODO this should actually use the value from the material.
					if (propertyType == typeof(Color) && index == 3)
						return 1;

					throw new ArgumentOutOfRangeException(nameof(index), $"PropertyCurve {propertyName} ({propertyType}) has only {curve.Count} curves but index {index} was accessed for time {time}");
				}

				return curve[index].Evaluate(time);
			}

			internal bool Validate()
			{
				if (propertyType == typeof(Color))
				{
					var hasEnoughCurves = curve.Count == 4;
					if (!hasEnoughCurves)
					{
						UnityEngine.Debug.LogWarning("Animating single channels for colors is not supported. Please add at least one keyframe for all channels (RGBA): " + propertyName, target);
						return false;
					}
				}

				return true;
			}

			/// <summary>
			/// Call this method once before beginning to evaluate curves
			/// </summary>
			internal void SortCurves()
			{
				// If we animate a color property in Unity and start by creating keys for green then the green curve will be at index 0
				// This method ensures that the curves are in a known order e.g. rgba (instead of green red blue alpha)
				if (curve?.Count > 0 && curveName.Count > 0)
				{
					if (propertyType == typeof(Color))
					{
						FillTempLists();
						var indexOfR = FindIndex(name => name.EndsWith(".r"));
						var indexOfG = FindIndex(name => name.EndsWith(".g"));
						var indexOfB = FindIndex(name => name.EndsWith(".b"));
						var indexOfA = FindIndex(name => name.EndsWith(".a"));
						for (var i = 0; i < curve.Count; i++)
						{
							var curveIndex = i;
							if (i == 0) curveIndex = indexOfR;
							else if (i == 1) curveIndex = indexOfG;
							else if (i == 2) curveIndex = indexOfB;
							else if (i == 3) curveIndex = indexOfA;
							if (curveIndex >= 0 && curveIndex != i)
							{
								this.curve[i] = _tempList1[curveIndex]; ;
								this.curveName[i] = _tempList2[curveIndex]; ;
							}
						}
					}
				}
			}

			private static readonly List<AnimationCurve> _tempList1 = new List<AnimationCurve>();
			private static readonly List<string> _tempList2 = new List<string>();

			private void FillTempLists()
			{
				_tempList1.Clear();
				_tempList2.Clear();
				_tempList1.AddRange(curve);
				_tempList2.AddRange(curveName);
			}

			public int FindIndex(Predicate<string> test)
			{
				for (var i = 0; i < curveName.Count; i++)
				{
					if (test(curveName[i]))
						return i;
				}
				return -1;
			}


		}

		internal struct TargetCurveSet
		{
#pragma warning disable 0649
			public AnimationCurve[] translationCurves;
			public AnimationCurve[] rotationCurves;
			public AnimationCurve[] scaleCurves;
			public Dictionary<string, AnimationCurve> weightCurves;
			public PropertyCurve propertyCurve;
#pragma warning restore

			public Dictionary<string, PropertyCurve> propertyCurves;

			// for KHR_animation_pointer
			

			private static MemberInfo FindMemberOnTypeIncludingBaseTypes(Type type, string memberName)
			{
				while (type != null)
				{
					var member = type.GetMember(memberName, BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (member.Length > 0) return member[0];
					type = type.BaseType;
				}
				return null;
			}

			public void Init()
			{
				translationCurves = new AnimationCurve[3];
				rotationCurves = new AnimationCurve[4];
				scaleCurves = new AnimationCurve[3];
				weightCurves = new Dictionary<string, AnimationCurve>();
			}
		}

		private static string LogObject(object obj)
		{
			if (obj == null) return "null";

			if (obj is Component tr)
				return $"{tr.name} (InstanceID: {tr.GetInstanceID()}, Type: {tr.GetType()})";
			if (obj is GameObject go)
				return $"{go.name} (InstanceID: {go.GetInstanceID()})";

			return obj.ToString();
		}


		private bool BakePropertyAnimation(PropertyCurve prop, float length, float bakingFramerate, float speedMultiplier, out float[] times, out object[] values)
		{
			times = null;
			values = null;

			if (!prop.Validate()) return false;

			var nbSamples = Mathf.Max(1, Mathf.CeilToInt(length * bakingFramerate));
			var deltaTime = length / nbSamples;

			var _times = new List<float>(nbSamples * 2);
			var _values = new List<object>(nbSamples * 2);

			var curveCount = prop.curve.Count;
			var keyframes = prop.curve.Select(x => x.keys).ToArray();
			var keyframeIndex = new int[curveCount];

			prop.SortCurves();

			var vector3Scale = SchemaExtensions.CoordinateSpaceConversionScale.ToUnityVector3Raw();

			// Assuming all the curves exist now
			for (var i = 0; i < nbSamples; ++i)
			{
				var time = i * deltaTime;
				if (i == nbSamples - 1) time = length;

				for (var k = 0; k < curveCount; k++)
					while (keyframeIndex[k] < keyframes[k].Length - 1 && keyframes[k][keyframeIndex[k]].time < time)
						keyframeIndex[k]++;

				var isConstant = false;
				for (var k = 0; k < curveCount; k++)
					isConstant |= float.IsInfinity(keyframes[k][keyframeIndex[k]].inTangent);

				if (isConstant && _times.Count > 0)
				{
					var lastTime = _times[_times.Count - 1];
					var t0 = lastTime + 0.0001f;
					if (i != nbSamples - 1)
						time += deltaTime * 0.999f;
					_times.Add(t0 / speedMultiplier);
					_times.Add(time / speedMultiplier);
					var success = AddValue(time);
					success &= AddValue(time);
					if (!success) return false;
				}
				else
				{
					var t0 = time / speedMultiplier;
					_times.Add(t0);
					if (!AddValue(t0)) return false;
				}

				bool AddValue(float t)
				{
					if (prop.curve.Count == 1)
					{
						var value = prop.curve[0].Evaluate(t);
						_values.Add(value);
					}
					else
					{
						var type = prop.propertyType;

						if (typeof(Vector2) == type)
						{
							_values.Add(new Vector2(prop.Evaluate(t, 0), prop.Evaluate(t, 1)));
						}
						else if (typeof(Vector3) == type)
						{
							var vec = new Vector3(prop.Evaluate(t, 0), prop.Evaluate(t, 1), prop.Evaluate(t, 2));
							_values.Add(vec);
						}
						else if (typeof(Vector4) == type)
						{
							_values.Add(new Vector4(prop.Evaluate(t, 0), prop.Evaluate(t, 1), prop.Evaluate(t, 2), prop.Evaluate(t, 3)));
						}
						else if (typeof(Color) == type)
						{
							// TODO should actually access r,g,b,a separately since any of these can have curves assigned.
							var r = prop.Evaluate(t, 0);
							var g = prop.Evaluate(t, 1);
							var b = prop.Evaluate(t, 2);
							var a = prop.Evaluate(t, 3);
							_values.Add(new Color(r, g, b, a));
						}
						else if (typeof(Quaternion) == type)
						{
							if (prop.curve.Count == 3)
							{
								Quaternion eulerToQuat = Quaternion.Euler(prop.Evaluate(t, 0), prop.Evaluate(t, 1), prop.Evaluate(t, 2));
								_values.Add(new Quaternion(eulerToQuat.x, eulerToQuat.y, eulerToQuat.z, eulerToQuat.w));
							}
							else if (prop.curve.Count == 4)
							{
								_values.Add(new Quaternion(prop.Evaluate(t, 0), prop.Evaluate(t, 1), prop.Evaluate(t, 2), prop.Evaluate(t, 3)));
							}
							else
							{
								Debug.LogError(null, $"Rotation animation has {prop.curve.Count} curves, expected Euler Angles (3 curves) or Quaternions (4 curves). This is not supported, make sure to animate all components of rotations. Animated object {prop.target}", prop.target);
							}
						}
						else if (typeof(float) == type)
						{
							foreach (var val in prop.curve)
								_values.Add(val.Evaluate(t));
						}
						else
						{
							switch (prop.propertyName)
							{
								case "MotionT":
								case "MotionQ":
									// Ignore
									break;
								default:
									Debug.LogWarning(null, "Property is animated but can't be exported - Name: " + prop.propertyName + ", Type: " + prop.propertyType + ". Does its target exist? You can enable KHR_animation_pointer export in the Project Settings to export more animated properties.");
									break;

							}
							return false;
						}
					}

					return true;
				}
			}

			times = _times.ToArray();
			values = _values.ToArray();
			RemoveUnneededKeyframes(ref times, ref values);

			return true;
		}


		private void CollectRuntimeClipCurves(GameObject root, RuntimeAnimationClip runtimeclip, Dictionary<string, TargetCurveSet> targetCurves)
		{
			AnimationClip clip = runtimeclip.Clip;
			foreach (RuntimeAnimationProperty property in runtimeclip.Properties)
			{
				//Debug.Log("property.PropertyName:" + property.AnimationPropertyName);

				AnimationCurve curve = property.Curve;
				var propertyName = property.AnimationPropertyName;
				var containsPosition = propertyName.Contains("m_LocalPosition");
				var containsScale = propertyName.Contains("m_LocalScale");
				var containsRotation = propertyName.ToLowerInvariant().Contains("localrotation");
				var containsEuler = propertyName.ToLowerInvariant().Contains("localeuler");
				var containsBlendShapeWeight = propertyName.StartsWith("blendShape.", StringComparison.Ordinal);
				var containsCompatibleData = containsPosition || containsScale || containsRotation || containsEuler || containsBlendShapeWeight;


				if (!containsCompatibleData && !settings.UseAnimationPointer)
				{
					Debug.LogWarning("No compatible data found in clip binding: " + propertyName, clip);
					continue;
                }

				if (!targetCurves.ContainsKey(property.PropertyPath))
				{
					TargetCurveSet curveSet = new TargetCurveSet();
					curveSet.Init();
					targetCurves.Add(property.PropertyPath, curveSet);

                }
				TargetCurveSet current = targetCurves[property.PropertyPath];
				if (containsCompatibleData)
                {
					foreach (RuntimeAnimationProperty oneproperty in property.Children)
					{

						var onePropertyName = oneproperty.PropertyName;
								curve = oneproperty.Curve;
								if (containsPosition)
								{
									if (onePropertyName == "x")
										current.translationCurves[0] = curve;
									else if (onePropertyName == "y")
										current.translationCurves[1] = curve;
									else if (onePropertyName == "z")
										current.translationCurves[2] = curve;
								}else if (containsScale)
								{
									if (onePropertyName == "x")
										current.scaleCurves[0] = curve;
									else if (onePropertyName == "y")
										current.scaleCurves[1] = curve;
									else if (onePropertyName == "z")
										current.scaleCurves[2] = curve;
								}
								else if (containsRotation)
								{
									if (onePropertyName == "x")
										current.rotationCurves[0] = curve;
									else if (onePropertyName == "y")
										current.rotationCurves[1] = curve;
									else if (onePropertyName == "z")
										current.rotationCurves[2] = curve;
									else if (onePropertyName == "w")
										current.rotationCurves[3] = curve;
								}
								else if (containsEuler)
								{
									if (onePropertyName == "x")
										current.rotationCurves[0] = curve;
									else if (onePropertyName == "y")
										current.rotationCurves[1] = curve;
									else if (onePropertyName == "z")
										current.rotationCurves[2] = curve;
								}
								// ReSharper disable once ConditionIsAlwaysTrueOrFalse
								else if (containsBlendShapeWeight)
								{
									var weightName = onePropertyName.Substring("blendShape.".Length);
									current.weightCurves.Add(weightName, curve);
								}
								else if (settings.UseAnimationPointer)
								{
									/*
									var obj = AnimationUtility.GetAnimatedObject(root, binding);
									if (obj)
									{
										current.AddPropertyCurves(obj, curve, binding);
										targetCurves[binding.path] = current;
									}

									continue;
									*/

								}


						targetCurves[property.PropertyPath] = current;


					}
				}



				/*
				foreach (RuntimeAnimationProperty oneproperty in property.Children)
				{
					Debug.Log("oneproperty.PropertyName:"+oneproperty.PropertyName);
					Keyframe[] kfs = oneproperty.Curve.keys;
					foreach (Keyframe k in kfs)
					{


					}
				}
				*/
			}
						/*
#if UNITY_EDITOR

						

						// object reference curves - in some cases animated data can be contained in here, e.g. for SpriteRenderers.
						// this only makes sense when AnimationPointer is on, and someone needs to resolve the data to something in the glTF later via KHR_animation_pointer_Resolver
						if (settings.UseAnimationPointer)
						{
							var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
							foreach (var binding in objectBindings)
							{
								var obj = AnimationUtility.GetAnimatedObject(root, binding);
								switch (obj)
								{
									case SpriteRenderer spriteRenderer:
										if (!spriteRenderer.sprite) continue;
										if (binding.propertyName != "m_Sprite") continue;

										var spriteSheet = spriteRenderer.sprite;
										var spriteSheetPath = AssetDatabase.GetAssetPath(spriteSheet);
										// will only work with all sprites from the same spritesheet right now
										var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spriteSheetPath);

										var path = binding.propertyName;
										if (!targetCurves.ContainsKey(path))
										{
											var curveSet = new TargetCurveSet();
											curveSet.Init();
											targetCurves.Add(path, curveSet);
										}

										TargetCurveSet current = targetCurves[path];
										var objectKeys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
										var curve = new AnimationCurve();
										var keyframes = new List<Keyframe>();
										var lastKeyframe = default(Keyframe);
										for (var index = 0; index < objectKeys.Length; index++)
										{
											var objectKey = objectKeys[index];
											var spriteIndex = objectKeys[index].value ? Array.IndexOf(sprites, objectKeys[index].value) : 0;
											var kf = new Keyframe(objectKey.time, spriteIndex);

											// create intermediate keyframe to make sure we dont have interpolation between sprites
											// TODO better would be to allow configuring a constant track
											if ((int)lastKeyframe.value != (int)kf.value)
											{
												var intermediate = new Keyframe(kf.time - 0.0001f, lastKeyframe.value);
												keyframes.Add(intermediate);
											}
											keyframes.Add(kf);
											lastKeyframe = kf;
										}
										curve.keys = keyframes.ToArray();
										current.AddPropertyCurves(obj, curve, binding);
										targetCurves[path] = current;

										break;
								}
							}
						}

#endif

						*/
					}

		private void GenerateMissingRuntimeCurves(float endTime, Transform tr, ref Dictionary<string, TargetCurveSet> targetCurvesBinding)
		{

			var keyList = targetCurvesBinding.Keys.ToList();
			foreach (string target in keyList)
			{
				Transform targetTr = target.Length > 0 ? tr.Find(target) : tr;
				if (targetTr == null)
					continue;

				TargetCurveSet current = targetCurvesBinding[target];

				if (current.weightCurves.Count > 0)
				{
					// need to sort and generate the other matching curves as constant curves for all blend shapes
					var renderer = targetTr.GetComponent<SkinnedMeshRenderer>();
					var mesh = renderer.sharedMesh;
					var shapeCount = mesh.blendShapeCount;

					// need to reorder weights: Unity stores the weights alphabetically in the AnimationClip,
					// not in the order of the weights.
					var newWeights = new Dictionary<string, AnimationCurve>();
					for (int i = 0; i < shapeCount; i++)
					{
						var shapeName = mesh.GetBlendShapeName(i);
						var shapeCurve = current.weightCurves.ContainsKey(shapeName) ? current.weightCurves[shapeName] : CreateConstantCurve(renderer.GetBlendShapeWeight(i), endTime);
						newWeights.Add(shapeName, shapeCurve);
					}

					current.weightCurves = newWeights;
				}

				if (current.propertyCurves?.Count > 0)
				{
					foreach (var kvp in current.propertyCurves)
					{
						var prop = kvp.Value;
						if (prop.propertyType == typeof(Color))
						{
							var memberName = prop.propertyName;
							if (TryGetCurrentValue(prop.target, memberName, out var value))
							{
								// Generate missing color channels (so an animated color has always keyframes for all 4 channels)

								var col = (Color)value;

								var hasRedChannel = prop.FindIndex(v => v.EndsWith(".r")) >= 0;
								var hasGreenChannel = prop.FindIndex(v => v.EndsWith(".g")) >= 0;
								var hasBlueChannel = prop.FindIndex(v => v.EndsWith(".b")) >= 0;
								var hasAlphaChannel = prop.FindIndex(v => v.EndsWith(".a")) >= 0;

								if (!hasRedChannel) AddMissingCurve(memberName + ".r", col.r);
								if (!hasGreenChannel) AddMissingCurve(memberName + ".g", col.g);
								if (!hasBlueChannel) AddMissingCurve(memberName + ".b", col.b);
								if (!hasAlphaChannel) AddMissingCurve(memberName + ".a", col.a);

								void AddMissingCurve(string curveName, float constantValue)
								{
									var curve = CreateConstantCurve(constantValue, endTime);
									prop.curve.Add(curve);
									prop.curveName.Add(curveName);
								}
							}
						}
					}
				}

				targetCurvesBinding[target] = current;

			
			}
		}
		private static readonly Dictionary<(Type type, string name), MemberInfo> memberCache = new Dictionary<(Type type, string name), MemberInfo>();
		private static bool TryGetCurrentValue(object instance, string memberName, out object value)
		{
			if (instance == null || memberName == null)
			{
				value = null;
				return false;
			}

			var key = (instance.GetType(), memberName);
			if (!memberCache.TryGetValue(key, out var member))
			{
				var type = instance.GetType();
				while (type != null)
				{
					member = type
						.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
						.FirstOrDefault();
					if (member != null)
					{
						memberCache.Add(key, member);
						break;
					}
					type = type.BaseType;
				}
			}

			if (member == null)
			{
				value = null;
				return false;
			}

			switch (member)
			{
				case FieldInfo field:
					value = field.GetValue(instance);
					return true;
				case PropertyInfo property:
					value = property.GetValue(instance);
					return true;
				default:
					value = null;
					return false;
			}
		}
		private AnimationCurve CreateConstantCurve(float value, float endTime)
		{
			// No translation curves, adding them
			AnimationCurve curve = new AnimationCurve();
			curve.AddKey(0, value);
			curve.AddKey(endTime, value);
			return curve;
		}


#endif
	}


}

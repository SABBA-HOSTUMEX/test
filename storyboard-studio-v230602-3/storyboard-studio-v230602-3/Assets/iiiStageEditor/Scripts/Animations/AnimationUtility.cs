using System.Linq;
using UnityEngine;
using Battlehub.RTEditor;

public static class AnimationUtility
{
    public static AnimationClip LoadAnimationClipFromFbx(string fbxName, string clipName)
    {
        var clips = Resources.LoadAll<AnimationClip>(fbxName);
        return clips.FirstOrDefault(clip => clip.name == clipName);
    }

    public static GameObject CreateEffector(string name, Vector3 position, Quaternion rotation)
    {
        var effector = Resources.Load("Effector/EffectorPoint", typeof(GameObject)) as GameObject;
        return CreateEffectorFromGO(name, effector, position, rotation);
    }

    public static GameObject CreateBodyEffector(string name, Vector3 position, Quaternion rotation)
    {
        var prefab = Resources.Load("Effector/EffectorPoint", typeof(GameObject)) as GameObject;
        var effector = CreateEffectorFromGO(name, prefab, position, rotation);
        effector.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        return effector;


    }

    public static GameObject CreateEffectorFromGO(string name, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var effector = Object.Instantiate(prefab);
        effector.name = name;
        effector.transform.position = position;
        effector.transform.rotation = rotation;
        effector.transform.localScale = Vector3.one * 0.15f;
        var meshRenderer = effector.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.magenta;
        return effector;
    }

    static public Color FadeEffectorColorByWeight(Color original, float weight)
    {
        Color color = original * (0.2f + 0.8f * weight);
        color.a = (0.2f + 0.5f * weight);
        return color;
    }

    public static RuntimeAnimationProperty getAnimationPositionProperty(Transform transform, string path = "")
    {
        RuntimeAnimationProperty property = new RuntimeAnimationProperty();
        property.Component = transform;
        property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
        property.ComponentDisplayName = "Transform";
        property.AnimationPropertyName = "m_LocalPosition";
        property.PropertyDisplayName = "位置";
        property.PropertyName = "LocalPosition";
        if (path != "")
        {
            property.PropertyPath = path;

        }

        return property;
    }

    public static RuntimeAnimationProperty getAnimationRotationProperty(Transform transform, string path = "")
    {


        RuntimeAnimationProperty property = new RuntimeAnimationProperty();
        property.Component = transform;
        property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
        property.ComponentDisplayName = "Transform";
        property.AnimationPropertyName = "m_LocalRotation";
        property.PropertyDisplayName = "旋轉";
        property.PropertyName = "localRotation";

        if (path != "")
        {
            property.PropertyPath = path;

        }
        return property;
    }

    public static RuntimeAnimationProperty getAnimationScaleProperty(Transform transform, string path = "")
    {

        RuntimeAnimationProperty property = new RuntimeAnimationProperty();
        property.Component = transform;
        property.ComponentTypeName = "UnityEngine.Transform, UnityEngine.CoreModule";
        property.ComponentDisplayName = "Transform";
        property.AnimationPropertyName = "m_LocalScale";
        property.PropertyDisplayName = "縮放";
        property.PropertyName = "LocalScale";

        if (path != "")
        {
            property.PropertyPath = path;

        }
        return property;
    }

    public static RuntimeAnimationProperty genRuntimeAnimationPropertyRow(RuntimeAnimationProperty property, string pname, Keyframe[] keyframes)
    {


        return new RuntimeAnimationProperty
        {
            Parent = property,
            Component = property.Component,
            ComponentTypeName = property.ComponentTypeName,
            PropertyDisplayName = pname,
            PropertyName = pname,
            PropertyPath = property.PropertyPath,
            Curve = new AnimationCurve
            {
                keys = keyframes
            }

        };
    }

}

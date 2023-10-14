using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentLight<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public LightShadows shadows;

        [ProtoMember(257)]
        public float shadowStrength;

        [ProtoMember(271)]
        public LightType type;

        [ProtoMember(272)]
        public float spotAngle;

        [ProtoMember(273)]
        public PersistentColor<TID> color;

        [ProtoMember(274)]
        public float colorTemperature;

        [ProtoMember(275)]
        public float intensity;

        [ProtoMember(276)]
        public float bounceIntensity;

        [ProtoMember(277)]
        public int shadowCustomResolution;

        [ProtoMember(278)]
        public float shadowBias;

        [ProtoMember(279)]
        public float shadowNormalBias;

        [ProtoMember(280)]
        public float shadowNearPlane;

        [ProtoMember(281)]
        public float range;

        [ProtoMember(283)]
        public PersistentLightBakingOutput<TID> bakingOutput;

        [ProtoMember(284)]
        public int cullingMask;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Light uo = (Light)obj;
            shadows = uo.shadows;
            shadowStrength = uo.shadowStrength;
            type = uo.type;
            spotAngle = uo.spotAngle;
            color = uo.color;
            colorTemperature = uo.colorTemperature;
            intensity = uo.intensity;
            bounceIntensity = uo.bounceIntensity;
            shadowCustomResolution = uo.shadowCustomResolution;
            shadowBias = uo.shadowBias;
            shadowNormalBias = uo.shadowNormalBias;
            shadowNearPlane = uo.shadowNearPlane;
            range = uo.range;
            bakingOutput = uo.bakingOutput;
            cullingMask = uo.cullingMask;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Light uo = (Light)obj;
            uo.shadows = shadows;
            uo.shadowStrength = shadowStrength;
            uo.type = type;
            uo.spotAngle = spotAngle;
            uo.color = color;
            uo.colorTemperature = colorTemperature;
            uo.intensity = intensity;
            uo.bounceIntensity = bounceIntensity;
            uo.shadowCustomResolution = shadowCustomResolution;
            uo.shadowBias = shadowBias;
            uo.shadowNormalBias = shadowNormalBias;
            uo.shadowNearPlane = shadowNearPlane;
            uo.range = range;
            uo.bakingOutput = bakingOutput;
            uo.cullingMask = cullingMask;
            return uo;
        }
    }
}


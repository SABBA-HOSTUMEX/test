using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UniVRM10;
using UniVRM10.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace UniVRM10.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVrm10Instance<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID Vrm;

        [ProtoMember(257)]
        public PersistentVrm10InstanceSpringBone<TID> SpringBone;

        [ProtoMember(258)]
        public Vrm10Instance.UpdateTypes UpdateType;

        [ProtoMember(259)]
        public bool DrawLookAtGizmo;

        [ProtoMember(260)]
        public TID Gaze;

        [ProtoMember(261)]
        public VRM10ObjectLookAt.LookAtTargetTypes LookAtTargetType;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Vrm10Instance uo = (Vrm10Instance)obj;
            Vrm = ToID(uo.Vrm);
            SpringBone = uo.SpringBone;
            UpdateType = uo.UpdateType;
            DrawLookAtGizmo = uo.DrawLookAtGizmo;
            Gaze = ToID(uo.Gaze);
            LookAtTargetType = uo.LookAtTargetType;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Vrm10Instance uo = (Vrm10Instance)obj;
            uo.Vrm = FromID(Vrm, uo.Vrm);
            uo.SpringBone = SpringBone;
            uo.UpdateType = UpdateType;
            uo.DrawLookAtGizmo = DrawLookAtGizmo;
            uo.Gaze = FromID(Gaze, uo.Gaze);
            uo.LookAtTargetType = LookAtTargetType;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(Vrm, context);
            AddSurrogateDeps(SpringBone, context);
            AddDep(Gaze, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Vrm10Instance uo = (Vrm10Instance)obj;
            AddDep(uo.Vrm, context);
            AddSurrogateDeps(uo.SpringBone, v_ => (PersistentVrm10InstanceSpringBone<TID>)v_, context);
            AddDep(uo.Gaze, context);
        }
    }
}


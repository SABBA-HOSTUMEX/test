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
    public partial class PersistentMonoBehaviour<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public bool useGUILayout;

        [ProtoMember(257)]
        public bool runInEditMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MonoBehaviour uo = (MonoBehaviour)obj;
            useGUILayout = uo.useGUILayout;
            runInEditMode = uo.runInEditMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MonoBehaviour uo = (MonoBehaviour)obj;
            uo.useGUILayout = useGUILayout;
            uo.runInEditMode = runInEditMode;
            return uo;
        }
    }
}


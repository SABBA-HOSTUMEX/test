using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.Video;
using UnityEngine.Video.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Video.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVideoPlayer<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(257)]
        public string url;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            VideoPlayer uo = (VideoPlayer)obj;
            url = uo.url;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            VideoPlayer uo = (VideoPlayer)obj;
            uo.url = url;
            return uo;
        }
    }
}


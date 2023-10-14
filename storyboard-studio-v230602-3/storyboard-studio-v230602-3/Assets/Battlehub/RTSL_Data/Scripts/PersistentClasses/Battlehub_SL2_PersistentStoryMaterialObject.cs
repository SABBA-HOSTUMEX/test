using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Video;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentStoryMaterialObject<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(257)]
        public string m_videoUrl;

        [ProtoMember(258)]
        public TID videoPlayer;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StoryMaterialObject uo = (StoryMaterialObject)obj;
            m_videoUrl = uo.m_videoUrl;
            videoPlayer = ToID(uo.videoPlayer);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StoryMaterialObject uo = (StoryMaterialObject)obj;
            uo.m_videoUrl = m_videoUrl;
            uo.videoPlayer = FromID(videoPlayer, uo.videoPlayer);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(videoPlayer, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            StoryMaterialObject uo = (StoryMaterialObject)obj;
            AddDep(uo.videoPlayer, context);
        }
    }
}


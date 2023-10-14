using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentStoryCanAnimObject<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public bool CanAni;

        [ProtoMember(257)]
        public bool CanChangeTex;

        [ProtoMember(258)]
        public bool CanAddAudio;

        [ProtoMember(261)]
        public int OnSetAniID;

        [ProtoMember(262)]
        public float OnSetAniTime;

        [ProtoMember(263)]
        public string audioLink;

        [ProtoMember(264)]
        public StoryCanAnimObject.ObjectType type;

        [ProtoMember(265)]
        public string authorLink;

        [ProtoMember(266)]
        public string title;

        [ProtoMember(267)]
        public string desctext;

        [ProtoMember(268)]
        public int authorTarget;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StoryCanAnimObject uo = (StoryCanAnimObject)obj;
            CanAni = uo.CanAni;
            CanChangeTex = uo.CanChangeTex;
            CanAddAudio = uo.CanAddAudio;
            OnSetAniID = uo.OnSetAniID;
            OnSetAniTime = uo.OnSetAniTime;
            audioLink = uo.audioLink;
            type = uo.type;
            authorLink = uo.authorLink;
            title = uo.title;
            desctext = uo.desctext;
            authorTarget = uo.authorTarget;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StoryCanAnimObject uo = (StoryCanAnimObject)obj;
            uo.CanAni = CanAni;
            uo.CanChangeTex = CanChangeTex;
            uo.CanAddAudio = CanAddAudio;
            uo.OnSetAniID = OnSetAniID;
            uo.OnSetAniTime = OnSetAniTime;
            uo.audioLink = audioLink;
            uo.type = type;
            uo.authorLink = authorLink;
            uo.title = title;
            uo.desctext = desctext;
            uo.authorTarget = authorTarget;
            return uo;
        }
    }
}


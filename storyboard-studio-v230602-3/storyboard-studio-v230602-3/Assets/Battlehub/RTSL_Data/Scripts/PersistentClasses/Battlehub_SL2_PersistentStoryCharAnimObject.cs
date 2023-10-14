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
    public partial class PersistentStoryCharAnimObject<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(261)]
        public int ActionCode;

        [ProtoMember(264)]
        public string vrmpath;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StoryCharAnimObject uo = (StoryCharAnimObject)obj;
            ActionCode = uo.ActionCode;
            vrmpath = uo.vrmpath;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StoryCharAnimObject uo = (StoryCharAnimObject)obj;
            uo.ActionCode = ActionCode;
            uo.vrmpath = vrmpath;
            return uo;
        }
    }
}


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
    public partial class PersistentStoryboardScene<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public string m_data;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StoryboardScene uo = (StoryboardScene)obj;
            m_data = uo.m_data;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StoryboardScene uo = (StoryboardScene)obj;
            uo.m_data = m_data;
            return uo;
        }
    }
}


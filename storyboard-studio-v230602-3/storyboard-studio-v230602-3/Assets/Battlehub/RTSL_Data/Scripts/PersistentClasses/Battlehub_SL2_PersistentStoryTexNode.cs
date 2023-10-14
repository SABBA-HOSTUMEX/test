using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentStoryTexNode<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(258)]
        public TID planeObj;

        [ProtoMember(259)]
        public TID camObj;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StoryTexNode uo = (StoryTexNode)obj;
            planeObj = ToID(uo.planeObj);
            camObj = ToID(uo.camObj);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StoryTexNode uo = (StoryTexNode)obj;
            uo.planeObj = FromID(planeObj, uo.planeObj);
            uo.camObj = FromID(camObj, uo.camObj);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(planeObj, context);
            AddDep(camObj, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            StoryTexNode uo = (StoryTexNode)obj;
            AddDep(uo.planeObj, context);
            AddDep(uo.camObj, context);
        }
    }
}


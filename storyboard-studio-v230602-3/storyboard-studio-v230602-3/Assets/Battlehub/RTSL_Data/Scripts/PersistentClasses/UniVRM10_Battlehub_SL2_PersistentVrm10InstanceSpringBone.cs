using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UniVRM10;
using UniVRM10.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UniVRM10.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVrm10InstanceSpringBone<TID> : PersistentSurrogate<TID>
    {
        
        public static implicit operator Vrm10InstanceSpringBone(PersistentVrm10InstanceSpringBone<TID> surrogate)
        {
            if(surrogate == null) return default(Vrm10InstanceSpringBone);
            return (Vrm10InstanceSpringBone)surrogate.WriteTo(new Vrm10InstanceSpringBone());
        }
        
        public static implicit operator PersistentVrm10InstanceSpringBone<TID>(Vrm10InstanceSpringBone obj)
        {
            PersistentVrm10InstanceSpringBone<TID> surrogate = new PersistentVrm10InstanceSpringBone<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}


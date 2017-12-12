using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public abstract class ISyncInterface : MonoBehaviour
    {
        public abstract void InitSync(AvatarInfo info);
        public abstract void NetworkAttack(GCPlayerCmd sk);
        public abstract void NetworkBuff(GCPlayerCmd gc);
        public abstract void NetworkAttribute(GCPlayerCmd gc);
        public abstract void Revive(GCPlayerCmd gc);
        public abstract void SetLevel(AvatarInfo info);
        public abstract void SetPositionAndDir(AvatarInfo info);
        public abstract void DoNetworkDamage(GCPlayerCmd cmd);
        public abstract void NetworkRemoveBuff(GCPlayerCmd cmd);
        public abstract void Dead(GCPlayerCmd cmd);
        public abstract bool CheckSyncState();
        public abstract MyVec3 GetServerVelocity();
        public abstract Vector3 GetServerPos();

        public abstract Vector3 GetCurInfoPos();
        public abstract Vector3 GetCurInfoSpeed();
        public abstract void AddFakeMove();
    }
}

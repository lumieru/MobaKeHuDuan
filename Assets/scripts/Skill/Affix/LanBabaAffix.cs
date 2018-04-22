using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    //对应的ID Event顺序绑定起来  SkillID ---> EventID---> Buff
    //Buff 角色身上状态 从服务器同步
    //开始 结束  更新 以及 状态 同步
    public class LanBabaAffix : AffixSpawn
    {
        public GameObject effect;
        //客户端执行逻辑
        //服务器上执行逻辑
        public override void OnEnter()
        {
            base.OnEnter();
            var ply = modify.gameObject;
            eff = GameObject.Instantiate<GameObject>(effect);
            eff.transform.parent = ply.transform;
            Util.InitGameObject(eff);
        }

        public override void OnExit()
        {
            GameObject.Destroy(eff);
            base.OnExit();
        }
        private GameObject eff;
    }
}
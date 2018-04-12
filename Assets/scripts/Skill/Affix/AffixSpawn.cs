using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class AffixSpawn : MonoBehaviour
    {
        //数值方面
        //表现
        //Buff时间
        //逻辑脚本来执行代码
        public string AffixName = "None";
        public int defenseAdd = 0;
        public float duration = 10;



        public virtual void OnEnter()
        {
            inBuff = true;
        }

        public virtual void OnExit()
        {
            inBuff = false;
        }

        private bool inBuff = false;
        public float startTime;
        public ModifyComponent modify;
    }
}
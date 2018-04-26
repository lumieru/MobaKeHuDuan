using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        None,
        Attack,
        Skill1, //技能1
    }

    [System.Serializable]
    public class ActionConfig
    {
        public ActionType type = ActionType.Attack;
        public float totalTime;
        public float hitTime;
        public string aniName = "AbilityR";
        public float skillAttackRange = 8;
        public float skillAttackTargetDist = 8;
        public int skillId;//技能伤害计算的ID
        public bool needEnemy = false; //锁定目标技能必须有目标才可以释放 
    }

    public class NpcConfig : MonoBehaviour
    {
        /// <summary>
        /// NPC还是玩家配置
        /// </summary>
        public bool IsPlayer = false;
        /// <summary>
        /// 职业ID 或者 NPCID
        /// </summary>
        public int npcTemplateId;
        public List<ActionConfig> actionList;

        #region NPC
        //普通攻击
        //4个技能的配置
        //小怪和NPC的普通攻击配置
        public string normalAttack = "monsterSingle";
        //npc的视觉范围
        public float eyeSightDistance = 9.5f;
        //对NPC攻击范围小兵
        public float attackRangeDist = 10;
        //NPC 普通技能ID
        public int attackSkill = 1;
        public int dropGold = 10;
        /// <summary>
        /// NPC 回归原点最大距离
        /// </summary>
        public float maxMoveRange2 = 11;
        public int XPGain = 50;
        #endregion

        #region PlayerAndNPC
        //玩家和NPC均有影响
        public float moveSpeed = 5;
        //玩家回血速度
        public float hpRecover = 0;
        public float damageToTower = 1.0f;
        #endregion


        public ActionConfig GetAction(ActionType tp)
        {
            foreach (var a in actionList)
            {
                if (a.type == tp)
                {
                    return a;
                }
            }
            return new ActionConfig() { type = ActionType.None };
        }

        public ActionConfig GetActionBySkillId(int skillId)
        {
            foreach (var a in actionList)
            {
                if (a.skillId == skillId)
                {
                    return a;
                }
            }
            return null;
        }


        public static  NpcConfig defaultConfig()
        {
            if(_default == null)
            {
                var go = new GameObject();
                var nc = go.AddMissingComponent<NpcConfig>();
                nc.npcTemplateId = -1;
                _default = nc;
                GameObject.DontDestroyOnLoad(go);
            }
            return _default;
        }
        private static NpcConfig _default;
    }
   
}

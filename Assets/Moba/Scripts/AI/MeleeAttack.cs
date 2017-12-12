using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

/// <summary>
/// 定时播放一个技能动作
/// 特效和子弹 实例 自己的配置和执行逻辑
/// </summary>
public class MeleeAttack : SkillState 
{
    SkillStateMachine skillStateMachine;
    SkillFullInfo activeSkill;
    private ObjectCommand cmd;
    private float holdTime;
    public override void EnterState()
    {
        base.EnterState();
        cmd = aiCharacter.lastCmd;
        activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();
        skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData, GetAttr().transform.position);
        var time = Util.FrameToFloat(aiCharacter.lastCmd.skillAction.RunFrame);
        var dir =  cmd.skillAction.Dir;
        var physics = aiCharacter.GetAttr().GetComponent<IPhysicCom>();
        physics.TurnToDir(dir);
        holdTime = time;
        aiCharacter.PlayAniInTime("creep_attack1", time);

    }
    public override IEnumerator RunLogic()
    {
        var passTime = 0.0f;
        while(!quit && passTime < holdTime)
        {
            passTime += Time.deltaTime;
            yield return null;
        }
        if (!quit)
        {
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }
    }


}

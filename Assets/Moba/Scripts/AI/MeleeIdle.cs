using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class MeleeIdle : IdleState
{
    public override void EnterState()
    {
        base.EnterState();
        aiCharacter.SetIdle();
    }
    public override IEnumerator RunLogic()
    {
        var monSync = GetAttr().GetComponent<MonsterSync>();
        while (!quit)
        {
            var isNetMove = MobaUtil.IsServerMoveBySpeedOrPos(monSync);
            if (isNetMove)
            {
                aiCharacter.ChangeState(AIStateEnum.MOVE);
            }
            yield return null;
        }
    }
}

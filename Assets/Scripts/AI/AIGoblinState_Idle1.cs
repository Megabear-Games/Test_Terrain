using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGoblinState_Idle1 : AIGoblinState
{
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    // Private
    float _idleTime = 0.0f;
    float _timer    = 0.0f;

    public override AIStateType GetStateType()
    {
        Debug.Log("State Type being fetched by state machine");
        return AIStateType.Idle;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Idle State");
        base.OnEnterState();
        if (_goblinStateMachine == null)
            return;

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        _goblinStateMachine.NavAgentControl(true, false);
        _goblinStateMachine.speed       = 0;
        _goblinStateMachine.seeking     = 0;
        _goblinStateMachine.attackType  = 0;
        _goblinStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        if (_goblinStateMachine == null)
            return AIStateType.Idle;

        if (_goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if (_goblinStateMachine.VisualThreat.type == AITargetType.Audio)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        _timer += Time.deltaTime;
        if (_timer > _idleTime)
        {
            return AIStateType.Patrol;
        }

        return AIStateType.Idle;
    }

  
}

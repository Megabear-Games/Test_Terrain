using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGoblinState_Alerted1 : AIGoblinState
{
    [SerializeField] [Range(1,60)] float _maxDuration   = 10.0f;
    [SerializeField] float _wayPointAngleThreshold      = 90.0f;
    [SerializeField] float _threatAngleThreshold        = 10.0f;
    [SerializeField] float _directionChangeTime         = 1.5f;

    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_goblinStateMachine == null)
            return;

        _goblinStateMachine.NavAgentControl(true, false);
        _goblinStateMachine.speed       = 0;
        _goblinStateMachine.seeking     = 0;
        _goblinStateMachine.attackType  = 0;

        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }

    public override AIStateType OnUpdate()
    {
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        if (_timer <= 0.0f)
        {
            _goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.GetWaypointPosition(false));
            _goblinStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;
        }

        if(_goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }


        if(_goblinStateMachine.AudioThreat.type==AITargetType.Audio)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        float angle;
        if(_goblinStateMachine.targetType==AITargetType.Audio)
        {
            angle = AIState.FindSignedAngle(_goblinStateMachine.transform.forward, _goblinStateMachine.targetPosition - _goblinStateMachine.transform.position);
            
            if(_goblinStateMachine.targetType==AITargetType.Audio && Mathf.Abs(angle) <_threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if(_directionChangeTimer > _directionChangeTime)
            {
                if (Random.value < _goblinStateMachine.intelligence)
                {
                    _goblinStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _goblinStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }
            }
            
        }
        else if(_goblinStateMachine.targetType==AITargetType.Waypoint && !_goblinStateMachine.navAgent.pathPending)
        {
            angle = AIState.FindSignedAngle(_goblinStateMachine.transform.forward,
                                            _goblinStateMachine.navAgent.steeringTarget - _goblinStateMachine.transform.position);

            if (Mathf.Abs(angle) < _wayPointAngleThreshold) return AIStateType.Patrol;
            if(_directionChangeTimer > _directionChangeTime)
            {
                _goblinStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;
            }
        }
        else
        {
            if(_directionChangeTimer > _directionChangeTime)
            {
                _goblinStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;
            }
        }

        return AIStateType.Alerted;
    }
}

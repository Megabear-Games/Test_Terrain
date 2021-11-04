using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIGoblinState_Patrol1 : AIGoblinState
{
    // Inspector Assigned

    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 8.0f)] float _speed = 3.0f;



    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    // -----------------------------------------------------------------------
    //  Name    :   OnEnterState
    //  Desc    :   Called by the State Machine when first transitioned into
    //              this state
    // -----------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_goblinStateMachine == null)
            return;

        _goblinStateMachine.NavAgentControl(true, false);
        _goblinStateMachine.speed = _speed;
        _goblinStateMachine.seeking = 0;
        _goblinStateMachine.attackType = 0;
        //_goblinStateMachine.ClearTarget();
        // Set Destination
        _goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.GetWaypointPosition(false));

        // Make sure NavAgent is switched on
        _goblinStateMachine.navAgent.isStopped = false;
    }

    /// <summary>
    /// Called by the state machine each frame to give this
    /// state a time-slice to update itself
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        // Do we have a visual threat that is the player
        if (_goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        // Sound is the third highest priority
        if (_goblinStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _goblinStateMachine.SetTarget(_goblinStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }


        if (_goblinStateMachine.navAgent.pathPending)
        {
            _goblinStateMachine.speed = 0;
            return AIStateType.Patrol;
        }
        else
            _goblinStateMachine.speed = _speed;

        float angle = Vector3.Angle(_goblinStateMachine.transform.forward, (_goblinStateMachine.navAgent.steeringTarget - _goblinStateMachine.transform.position));
        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if (!_goblinStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(_goblinStateMachine.navAgent.desiredVelocity);
            _goblinStateMachine.transform.rotation = Quaternion.Slerp(_goblinStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        if (_goblinStateMachine.navAgent.isPathStale ||
            !_goblinStateMachine.navAgent.hasPath ||
            _goblinStateMachine.navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.GetWaypointPosition(true));
        }

        // Stay in Patrol State
        return AIStateType.Patrol;
    }



    public override void OnDestinationReached(bool isReached)
    {
        if (_goblinStateMachine == null || !isReached)
            return;

        if (_goblinStateMachine.targetType == AITargetType.Waypoint)
            _goblinStateMachine.GetWaypointPosition(true);
    }

    /*public override void OnAnimatorIKUpdated()
    {
        if (_goblinStateMachine == null)
            return;

        //Debug.Log("IK Goals being set");
        _goblinStateMachine.animator.SetLookAtPosition(_goblinStateMachine.targetPosition);
        _goblinStateMachine.animator.SetLookAtWeight(0.55f);
    }*/
}

using UnityEngine;
using System.Collections;

public class AIGoblinState_Attack1 : AIGoblinState
{
	// Inspector Assigned
	[SerializeField] [Range(0, 10)]			float	_speed					= 0.0f;
	[SerializeField]						float	_stoppingDistance		= 1.0f;
	[SerializeField] [Range(0.0f, 1.0f)]	float	_lookAtWeight			= 0.7f;
	[SerializeField] [Range(0.0f, 90.0f)]	float	_lookAtAngleThreshold	= 15.0f;
	[SerializeField] float _slerpSpeed = 5.0f;


	// Private Variables
	private float _currentLookAtWeight = 0.0f;

	// Mandatory Overrides
	public override AIStateType GetStateType() { return AIStateType.Attack; }

	// Default Handlers
	public override void OnEnterState()
	{
		Debug.Log("Entering Attack State");

		base.OnEnterState();
		if (_goblinStateMachine == null)
			return;

		// Configure State Machine
		_goblinStateMachine.NavAgentControl(true, false);
		_goblinStateMachine.seeking = 0;
		_goblinStateMachine.attackType = Random.Range(1, 100); ;
		_goblinStateMachine.speed = _speed;
		_currentLookAtWeight = 0.0f;
	}

	public override void OnExitState()
	{
		_goblinStateMachine.attackType = 0;
	}

	// ---------------------------------------------------------------------
	// Name	:	OnUpdateAI
	// Desc	:	The engine of this state
	// ---------------------------------------------------------------------
	public override AIStateType OnUpdate()
	{
		Vector3 targetPos;
		Quaternion newRot;

		if (Vector3.Distance(_goblinStateMachine.transform.position, _goblinStateMachine.targetPosition) < _stoppingDistance)
			_goblinStateMachine.speed = 0;
		else
			_goblinStateMachine.speed = _speed;

		// Do we have a visual threat that is the player
		if (_goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player)
		{
			// Set new target
			_goblinStateMachine.SetTarget(_stateMachine.VisualThreat);

			// If we are not in melee range any more than fo back to pursuit mode
			if (!_goblinStateMachine.inMeleeRange) return AIStateType.Pursuit;

			if (!_goblinStateMachine.useRootRotation)
			{
				// Keep the goblin facing the player at all times
				targetPos = _goblinStateMachine.targetPosition;
				targetPos.y = _goblinStateMachine.transform.position.y;
				newRot = Quaternion.LookRotation(targetPos - _goblinStateMachine.transform.position);
				_goblinStateMachine.transform.rotation = Quaternion.Slerp(_goblinStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
			}

			_goblinStateMachine.attackType = Random.Range(1, 100);

			return AIStateType.Attack;
		}

		// PLayer has stepped outside out FOV or hidden so face in his/her direction and then
		// drop back to Alerted mode to give the AI a chance to re-aquire target
		if (!_goblinStateMachine.useRootRotation)
		{
			targetPos = _goblinStateMachine.targetPosition;
			targetPos.y = _goblinStateMachine.transform.position.y;
			newRot = Quaternion.LookRotation(targetPos - _goblinStateMachine.transform.position);
			_goblinStateMachine.transform.rotation = newRot;
		}

		// Stay in Patrol State
		return AIStateType.Alerted;
	}

	// -----------------------------------------------------------------------
	// Name	:	OnAnimatorIKUpdated
	// Desc	:	Override IK Goals
	// -----------------------------------------------------------------------
	public override void OnAnimatorIKUpdated()
	{
		if (_goblinStateMachine == null)
			return;

		if (Vector3.Angle(_goblinStateMachine.transform.forward, _goblinStateMachine.targetPosition - _goblinStateMachine.transform.position) < _lookAtAngleThreshold)
		{
			_goblinStateMachine.animator.SetLookAtPosition(_goblinStateMachine.targetPosition + Vector3.up);
			_currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
			_goblinStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
		}
		else
		{
			_currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
			_goblinStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
		}
	}
}

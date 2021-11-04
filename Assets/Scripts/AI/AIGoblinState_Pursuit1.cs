using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// -----------------------------------------------------------------
// Class	: AIGoblinState_Pursuit1
// Desc		: A Goblin state used for pursuing a target
// -----------------------------------------------------------------
public class AIGoblinState_Pursuit1 : AIGoblinState
{
	[SerializeField] [Range(0, 10)] private float _speed = 1.0f;
	[SerializeField] private float _slerpSpeed = 5.0f;
	[SerializeField] private float _repathDistanceMultiplier = 0.035f;
	[SerializeField] private float _repathVisualMinDuration = 0.05f;
	[SerializeField] private float _repathVisualMaxDuration = 5.0f;
	[SerializeField] private float _repathAudioMinDuration = 0.25f;
	[SerializeField] private float _repathAudioMaxDuration = 5.0f;
	[SerializeField] private float _maxDuration = 40.0f;

	// Private Fields
	private float _timer = 0.0f;
	private float _repathTimer = 0.0f;

	// Mandatory Overrides
	public override AIStateType GetStateType() { return AIStateType.Pursuit; }

	// Default Handlers
	public override void OnEnterState()
	{
		Debug.Log("Entering Pursuit State");

		base.OnEnterState();
		if (_goblinStateMachine == null)
			return;

		// Configure State Machine
		_goblinStateMachine.NavAgentControl(true, false);
		_goblinStateMachine.seeking = 0;
		_goblinStateMachine.attackType = 0;

		// Goblins will only pursue for so long before breaking off
		_timer = 0.0f;
		_repathTimer = 0.0f;


		// Set path
		_goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.targetPosition);
		_goblinStateMachine.navAgent.isStopped = false;

	}

	// ---------------------------------------------------------------------
	// Name	:	OnUpdateAI
	// Desc	:	The engine of this state
	// ---------------------------------------------------------------------
	public override AIStateType OnUpdate()
	{
		_timer += Time.deltaTime;
		_repathTimer += Time.deltaTime;

		if (_timer > _maxDuration)
			return AIStateType.Patrol;

		// IF we are chasing the player and have entered the melee trigger then attack
		if (_stateMachine.targetType == AITargetType.Visual_Player && _goblinStateMachine.inMeleeRange)
		{
			return AIStateType.Attack;
		}

		// Otherwise this is navigation to areas of interest so use the standard target threshold
		if (_goblinStateMachine.isTargetReached)
		{
			switch (_stateMachine.targetType)
			{

				// If we have reached the source
				case AITargetType.Audio:
					_stateMachine.ClearTarget();    // Clear the threat
					return AIStateType.Alerted;     // Become alert and scan for targets
			}
		}


		// If for any reason the nav agent has lost its path then call then drop into alerted state
		// so it will try to re-aquire the target or eventually giveup and resume patrolling
		if (_goblinStateMachine.navAgent.isPathStale ||
			(!_goblinStateMachine.navAgent.hasPath && !_goblinStateMachine.navAgent.pathPending) ||
			_goblinStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
		{
			return AIStateType.Alerted;
		}

		if (_goblinStateMachine.navAgent.pathPending)
			_goblinStateMachine.speed = 0;
		else
		{
			_goblinStateMachine.speed = _speed;

			// If we are close to the target that was a player and we still have the player in our vision then keep facing right at the player
			if (!_goblinStateMachine.useRootRotation && _goblinStateMachine.targetType == AITargetType.Visual_Player && _goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player && _goblinStateMachine.isTargetReached)
			{
				Vector3 targetPos = _goblinStateMachine.targetPosition;
				targetPos.y = _goblinStateMachine.transform.position.y;
				Quaternion newRot = Quaternion.LookRotation(targetPos - _goblinStateMachine.transform.position);
				_goblinStateMachine.transform.rotation = newRot;
			}
			else
			// SLowly update our rotation to match the nav agents desired rotation BUT only if we are not persuing the player and are really close to him
			if (!_stateMachine.useRootRotation && !_goblinStateMachine.isTargetReached)
			{
				// Generate a new Quaternion representing the rotation we should have
				Quaternion newRot = Quaternion.LookRotation(_goblinStateMachine.navAgent.desiredVelocity);

				// Smoothly rotate to that new rotation over time
				_goblinStateMachine.transform.rotation = Quaternion.Slerp(_goblinStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
			}
			else if (_goblinStateMachine.isTargetReached)
			{
				return AIStateType.Alerted;
			}
		}

		// Do we have a visual threat that is the player
		if (_goblinStateMachine.VisualThreat.type == AITargetType.Visual_Player)
		{
			// The position is different - maybe same threat but it has moved so repath periodically
			if (_goblinStateMachine.targetPosition != _goblinStateMachine.VisualThreat.position)
			{
				// Repath more frequently as we get closer to the target (try and save some CPU cycles)
				if (Mathf.Clamp(_goblinStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
				{
					// Repath the agent
					_goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.VisualThreat.position);
					_repathTimer = 0.0f;
				}
			}
			// Make sure this is the current target
			_goblinStateMachine.SetTarget(_goblinStateMachine.VisualThreat);

			// Remain in pursuit state
			return AIStateType.Pursuit;
		}

		// If our target is the last sighting of a player then remain
		// in pursuit as nothing else can override
		if (_goblinStateMachine.targetType == AITargetType.Visual_Player)
			return AIStateType.Pursuit;

		else
		if (_goblinStateMachine.AudioThreat.type == AITargetType.Audio)
		{

			if (_goblinStateMachine.targetType == AITargetType.Audio)
			{
				// Get unique ID of the collider of our target
				int currentID = _goblinStateMachine.targetColliderID;

				// If this is the same light
				if (currentID == _goblinStateMachine.AudioThreat.collider.GetInstanceID())
				{
					// The position is different - maybe same threat but it has moved so repath periodically
					if (_goblinStateMachine.targetPosition != _goblinStateMachine.AudioThreat.position)
					{
						// Repath more frequently as we get closer to the target (try and save some CPU cycles)
						if (Mathf.Clamp(_goblinStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
						{
							// Repath the agent
							_goblinStateMachine.navAgent.SetDestination(_goblinStateMachine.AudioThreat.position);
							_repathTimer = 0.0f;
						}
					}

					_goblinStateMachine.SetTarget(_goblinStateMachine.AudioThreat);
					return AIStateType.Pursuit;
				}
				else
				{
					_goblinStateMachine.SetTarget(_goblinStateMachine.AudioThreat);
					return AIStateType.Alerted;
				}
			}
		}

		// Default
		return AIStateType.Pursuit;
	}


}
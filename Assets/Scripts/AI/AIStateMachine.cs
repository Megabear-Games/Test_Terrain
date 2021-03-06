using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Visual_Player,  Audio, Visual_Sight }
public enum AITriggerEventType { Enter, Stay, Exit }

// --------------------------------------------------------------------------
//  Class       :   AITarget
//  Desc        :   Describes a potential target to the AI System
// --------------------------------------------------------------------------
public struct AITarget
{
    private AITargetType _type;          // The type of target
    private Collider _collider;      // The collider
    private Vector3 _position;      // Current position in the world
    private float _distance;      // Distance from player
    private float _time;          // Time the target was last ping'd

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType type, Collider collider, Vector3 position, float distance)
    {
        _type = type;
        _collider = collider;
        _position = position;
        _distance = distance;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _time = 0.0f;
        _distance = Mathf.Infinity;
    }
}

//--------------------------------------------------------------------------------
// Class        :   AIStateMachine
// Desc         :   Base class for all AI State Machines
//--------------------------------------------------------------------------------

public abstract class AIStateMachine : MonoBehaviour
{
    // Public
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    // Protected
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;
    protected bool _isTargetReached = false;
    protected List<Rigidbody> _bodyParts = new List<Rigidbody>();
    protected int _aiBodyPartLayer = -1;

    // Protected Inspector Assigned
    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] Transform _rootBone = null;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    [SerializeField] protected AIWaypointNetwork _waypointNetwork = null;
    [SerializeField] protected bool _randomPatrol = false;
    [SerializeField] protected int _currentWaypoint = -1;
    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f;

    // Component Cache
    protected Animator      _animator   = null;
    protected NavMeshAgent  _navAgent   = null;
    protected Collider      _collider   = null;
    protected Transform     _transform  = null;

    // Public Properties
    public bool         isTargetReached { get { return _isTargetReached; } }
    public bool         inMeleeRange    { get; set; }
    public Animator     animator        { get { return _animator; } }
    public NavMeshAgent navAgent        { get { return _navAgent; } }
    public Vector3      sensorPosition
    {
        get
        {
            if (_sensorTrigger == null) return Vector3.zero;
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null) return 0.0f;
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.localScale.x,
                                        _sensorTrigger.radius * _sensorTrigger.transform.localScale.y);

            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.localScale.z);
        }
    }

    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public AITargetType targetType { get { return _target.type; } }
    public Vector3 targetPosition { get { return _target.position; } }
    public int targetColliderID
    {
        get
        {
            if (_target.collider)
                return _target.collider.GetInstanceID();
            else
                return -1;
        }
    }

    //----------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Cache components
    //----------------------------------------------------------------------------------
    protected virtual void Awake()
    {
        // Cache all frequently accessed components
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        // Get BodyPart Layer
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (AIManager.instance != null)
        {
            // Register state Machines with Scene Database
            if (_collider)      AIManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) AIManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }
        /*if(_rootBone != null)
        {
            Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();

            foreach(Rigidbody bodyPart in bodies)
            {
                if(bodyPart !=null && bodyParts.gameObject.layer == _aiBodyPartLayer)
                {
                    _bodyParts.Add(bodyPart);
                    GameSceneManager.instance.RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
                }
            }
        }*/
    }

    // --------------------------------------------------------------------
    // Name:    Start
    // Desc:    Called by unity proior to first update to setup the object
    // --------------------------------------------------------------------
    protected virtual void Start()
    {
        if (_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if (script != null)
            {
                script.parentStateMachine = this;
            }
        }

        // Fetch all states on this game object
        // 全てのAI行動を呼び出して配列に保管する
        AIState[] states = GetComponents<AIState>();

        // Loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                // Add this state to the state dictionary
                _states[state.GetStateType()] = state;

                // And set the parent state machine of this state
                state.SetStateMachine(this);
            }
        }

        // Set the current state
        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }

        if (_animator)
        {
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }

    //-----------------------------------------------------------------------------------------
    //  Name  : GetWaypointPosition
    //  Desc  : Fetched the world space position of the state machine's currently
    //          set waypoint with optional increment
    //-----------------------------------------------------------------------------------------
    public Vector3 GetWaypointPosition(bool increment)
    {
        if (_currentWaypoint == -1)
        {
            if (_randomPatrol)
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            else
                _currentWaypoint = 0;
        }
        else if (increment)
            NextWaypoint();

        // Fetch the new waypoint from the waypoint list
        if (_waypointNetwork.Waypoints[_currentWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

            // This is our new target position
            SetTarget(AITargetType.Waypoint,
                        null,
                        newWaypoint.position,
                        Vector3.Distance(newWaypoint.position, transform.position));

            return newWaypoint.position;
        }

        return Vector3.zero;
    }

    // -------------------------------------------------------------------------
    // Name	:	NextWaypoint
    // Desc	:	Called to select a new waypoint. Either randomly selects a new
    //			waypoint from the waypoint network or increments the current
    //			waypoint index (with wrap-around) to visit the waypoints in
    //			the network in sequence. Sets the new waypoint as the the
    //			target and generates a nav agent path for it
    // -------------------------------------------------------------------------
    private void NextWaypoint()
    {
        // Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1)
        {
            // Keep generating random waypoint until we find one that isn't the current one
            // NOTE: Very important that waypoint networks do not only have one waypoint :)
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            }
        }
        else
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;


    }

    //---------------------------------------------------------------------
    // Name:    SetTarget(Overload)
    // Desc:    Sets the current target and configures the target trigger
    //---------------------------------------------------------------------
    public void SetTarget(AITargetType target, Collider collider, Vector3 position, float distance)
    {
        // Set the target info
        _target.Set(target, collider, position, distance);

        // Configure and enable the target trigger at the correct
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    //---------------------------------------------------------------------
    // Name:    SetTarget(Overload)
    // Desc:    Sets the current target and configures the target trigger
    //          This method allowd for specifiying a cusom stopping
    //---------------------------------------------------------------------
    public void SetTarget(AITargetType target, Collider collider, Vector3 position, float distance, float stoppingDistance)
    {
        // Set the target info
        _target.Set(target, collider, position, distance);
        // Configure and enable the target trigger at the correct
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    //---------------------------------------------------------------------
    // Name:    SetTarget(Overload)
    // Desc:    Sets the current target and configures the target trigger
    //---------------------------------------------------------------------
    public void SetTarget(AITarget target)
    {
        // Set the target 
        _target = target;
        // Configure and enable the target trigger at the correct
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    //---------------------------------------------------------------------------
    // Name :   ClearTarget
    // Nesc :   Clears the current target
    //---------------------------------------------------------------------------

    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
    }

    //----------------------------------------------------------------------------
    // Name:    FixedUpdate
    // Desc:    Called by Unity with each tick of the Physics system. It clears the
    //          audio and visual threats each update and re-calculates the distance
    //          to the current target
    //----------------------------------------------------------------------------
    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }

        _isTargetReached = false;
    }

    //----------------------------------------------------------------------------
    // Name:    Update
    // Desc:    Called by Unity each frame. Gives the current state a chance to 
    //          update itself and perform transactions.
    //----------------------------------------------------------------------------
    protected virtual void Update()
    {
        if (_currentState == null) return;

        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    //----------------------------------------------------------------------------
    // Name:    OnTriggerEnter
    // Desc:    Called by Physics system when the AI's Main collider enters its
    //          trigger. This allows the child state to know when it has entered 
    //          the sphere of influence of a waypoint or last player sighted 
    //          position.
    //----------------------------------------------------------------------------
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;

        // Notify child state
        if (_currentState)
            _currentState.OnDestinationReached(true);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;
    }

    //----------------------------------------------------------------------------
    // Name:    OnTriggerExit
    // Desc:    Informs the child state that the AI entity is no longer at its 
    //          destination(typically true when a nwe target has been set by the
    //          child.) 
    //----------------------------------------------------------------------------
    protected void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;

        _isTargetReached = false;

        if (_currentState != null)
            _currentState.OnDestinationReached(false);
    }

    //----------------------------------------------------------------------------
    // Name:    OnTriggerEvent
    // Desc:    Called by our AISensor component when an AI aggravator has entered
    //          exited the sensor trigger
    //----------------------------------------------------------------------------
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_currentState != null)
            _currentState.OnTriggerEvent(type, other);
    }

    //----------------------------------------------------------------------------
    // Name:    OnAnimatorMove
    // Desc:    Called by Unity after root motion has been evaluated but not applied   
    //          to the object. This allows us to determine via code what to do with
    //          the root motion information
    //----------------------------------------------------------------------------
    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null)
            _currentState.OnAnimatorUpdated();
    }

    //----------------------------------------------------------------------------
    // Name:    OnAnimatorIK
    // Desc:    Called by Unity just prior to the IK system being updated giving us  
    //          a chance chance to setup IK Targets and wieghts
    //----------------------------------------------------------------------------
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIKUpdated();
    }

    //----------------------------------------------------------------------------
    // Name:    NavAgentControl
    // Desc:    Configure the NavMeshAgent to enable/disable auto updates of  
    //          position/rotation to our transform
    //----------------------------------------------------------------------------
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    //--------------------------------------------------------------------------------
    // Name:    AddRootMotionRequest
    // Desc:    Called by the state Machine Behaviours to Enable/Disable root motion
    //--------------------------------------------------------------------------------
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }

    //--------------------------------------------------------------------------------
    // Name:    AddRootMotionRequest
    // Desc:    Called by the state Machine Behaviours to Enable/Disable root motion
    //--------------------------------------------------------------------------------
    /*public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)
    {

    }*/
}


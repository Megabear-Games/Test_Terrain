using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------------------------
//  CLASS    :  AIGoblinStateMachine
//  DESC     :  State Machine used by goblin characters
//--------------------------------------------------------------------------------
public class AIGoblinStateMachine : AIStateMachine
{
    // Inspector Assigned
    [SerializeField] [Range(10.0f, 360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int _health = 100;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;


    // Private
    private int _seeking = 0;
    private int _attackType = 0;
    private float _speed = 0.0f;

    // Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _attackHash = Animator.StringToHash("Attack");

    // Public Properties
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public float intelligence { get { return _intelligence; } }
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public int seeking { get { return seeking; } set { _seeking = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    //----------------------------------------------------------
    // Name :   Update
    // Desc :   Refresh the animator with up-to-date values for
    //          its parameters
    //----------------------------------------------------------

    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            _animator.SetFloat  (_speedHash, _speed);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
        }
    }

    /*public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)
    {
        if (AIManager.instance != null && GameSceneManager.instance.bloodParticles != null)
        {
            ParticleSystem sys = GameSceneManager.instance.bloodParticles;
            sys.transform.position = position;
            var settings = sys.main;
            settings.simulationSpace = ParticleSystemSimulationSpace.World;
            sys.Emit(60);
        }
        health -= damage;

        float hitstrength = force.magnitude;
        bool shouldRagdoll = (hitstrength > 1.0f);
        if (health <= 0) shouldRagdoll = true;

        if (_navAgent)
            _navAgent.speed = 0;

        if (shouldRagdoll)
        {
            if (_navAgent) _navAgent.enabled = false;
            if (_animator) _animator.enabled = false;
            if (_collider) _collider.enabled = false;

            inMelleRange = false;

            foreach (Rigidbody body in _bodyParts)
            {
                if (body)
                    body.isKinematic = false;
            }

            if (hitstrength > 1.0f)
            {
                bodyPart.AddForce(force, ForceMode.Impulse);
            }
        }
    }*/
}

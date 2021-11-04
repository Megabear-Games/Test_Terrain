using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllManager : MonoBehaviour
{
    // Component 
    private CharacterController _characterController;
    public Animator _animator;
    public static CharacterControllManager instace;

    //
    private Transform meshPlayer;
    private Collider axeCollider;
    private Collider footCollider;

    // move 
    private float inputX;
    private float inputZ;
    private Vector3 v_movement;
    private float moveSpeed;
    private float gravity;


    //Combo
    public bool isAttacking = false;

    private void Awake()
    {
        instace = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 0.1f;
        gravity = 0.5f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        meshPlayer = player.transform.GetChild(0);
        _characterController = player.GetComponent<CharacterController>();
        _animator = player.GetComponent<Animator>();
        axeCollider = GameObject.Find("great_axe").GetComponent<BoxCollider>();
        footCollider = GameObject.Find("MellowDwarf:LeftFoot").GetComponent<CapsuleCollider>();
        axeCollider.enabled = false;
        footCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
        if(inputX == 0 && inputZ == 0)
        {
            _animator.SetBool("isRun", false);
        }
        else
        {
            _animator.SetBool("isRun", true);
        }

        if(Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        // gravity
        if(_characterController.isGrounded)
        {
            v_movement.y = 0f;
        }
        else
        {
            v_movement.y -= gravity * Time.deltaTime;
        }

        // movement
        v_movement = new Vector3(inputX * moveSpeed, v_movement.y, inputZ * moveSpeed);
        _characterController.Move(v_movement);

        // mesh rotate
        if(inputX !=0 || inputZ !=0)
        {
            Vector3 lookDir = new Vector3(v_movement.x, 0, v_movement.z);
            meshPlayer.rotation = Quaternion.LookRotation(lookDir);
        }
       
    }

    public void Attack()
    {
        if(!isAttacking)
        {
            isAttacking = true;
        }
    }

    public void AxeCollisionEnable()
    {
        axeCollider.enabled = true;
    }

    public void AxeColliderDisable()
    {
        axeCollider.enabled = false;
    }

    public void FootCollisionEnable()
    {
        footCollider.enabled = true;
    }

    public void FootColliderDisable()
    {
        footCollider.enabled = false;
    }

}

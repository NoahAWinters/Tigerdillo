 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
 
public class PlayerActions : MonoBehaviour //Rework
{
    //Declare Reference Vars
    //PlayerInput _playerInput;
    PlayerControls _controls;
    CharacterController _character;
    Animator _animator;
 
    //Animator hashes
    int _isMovingHash; //Triggers movement animation
    int _isCrouchingHash; //Triggers animation related to crouching
    int _isJumpingHash; //Handles if player is jumping
    int _isAttackingHash; //Will handle attacks
    int _isShellSmashHash; //Handles ground pound animation
    int _isPouncingHash;
 
    //Movement Vars and input values
    Vector2 _currMoveInput; //WASD && LS movement
    Vector3 _currMove; //Current movement vector
    Vector3 _currMoveCrouch; //Current movment modified with the crouching modifier
    Vector3 _appliedMove; //For handle gravity calculaions
    //Vector3 rotatedMove; //Rotated applied move to go in direction of camera
    bool _isMovePressed; //Returns true if movement is pressed
    bool _isCrouchPressed; //Returns true if the crouch button is pressed
 
    //Jump Vars and input values
    float _initialJumpVelocity; //Initial force behind a jump
    float _initialJumpVelocityFlip; //Initial force behind a flip jump (may be able to be phased out)
    [SerializeField] float _maxJumpHeight = 0.25f;
    [SerializeField] float _maxJumpTime = 0.75f; //Check if these need to be serialized
    [SerializeField] float _fallModifier = 2.0f;
    bool _isJumping;
    bool _jumpAnimating;
    

    ////Attack vars and input values
    AttackType _currentAttack; //Type of current attack being performed
    bool _isAttacking;

    //Ground Pound
    bool _isShellSmashing; //Whether player is ground pounding in general
    bool _shellSmashAnimating; //Whether the animation is going

    //Pounce Dash
    bool _isPouncing;
    [SerializeField] float _dashSpeed;
    [SerializeField] float _dashTime;
    bool _pounceCancel; //only toggle if it hits a wall

 
    //StateBooleans
    bool _isCrouching;    
 
    //Handle-Function overrides, act as a way to easily cancel all of a certain Handle function
    bool stopMovement;
    bool stopRotation;
    bool stopGravity;
    bool stopJump;
    bool stopActions;
    bool stopAnimations;
   
    /////Constants
    //Movement
    [SerializeField] float moveSpeed;// General move speed
    [SerializeField] float groundPoundForce; // force of moving down during ground pound
    [SerializeField] float crouchSpeedMod = 0.5f; //To reduce speed by 2
    [SerializeField] float rotationFactorPerFrame = 15.0f;//How fast to rotate when moving. Most likely doesnt need to be serial
    float groundedGravity = -1.05f;
    float fallingGravity;
    float externalModifier = 1;
    float currentSpeed; //Im not sure why this is here, I need to figure that out


    // #region /////Getters and Setters
    public float CurrentMovementX { get { return _currMove.x; } set { _currMove.x = value; } }
    public float CurrentMovementY { get { return _currMove.y; } set { _currMove.y = value; } }
    public float CurrentMovementZ { get { return _currMove.z; } set { _currMove.z = value; } }
    public float CurrentMovementWithCrouchX { get { return _currMoveCrouch.x; } set { _currMoveCrouch.x = value; } }
    public float CurrentMovementWithCrouchY { get { return _currMoveCrouch.y; } set { _currMoveCrouch.y = value; } }
    public float CurrentMovementWithCrouchZ { get { return _currMoveCrouch.z; } set { _currMoveCrouch.z = value; } }
    public bool IsGroundPounding { get { return _isShellSmashing; } }
    // #endregion
 
 
    #region Unity Functions
    void Start()
    {
        //Set reference vars
        //_playerInput = new PlayerInput();
        //_controls = new PlayerControls(this);
        _controls = GetComponent<PlayerControls>();
        _character = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

 
        //Default to expecting no attack
        _currentAttack = AttackType.NONE;

        //SetAnimator Hashes
        SetAnimatorHashes();
        SetJumpVars();
    }
 
    void FixedUpdate()
    {    
        HandleMovement(); //General Movement
        HandleRotation(); //Face movement direction
        HandleGravity(); //Gravity and falling
        HandleJump(); //All jump
        HandleActions(); //World Interactions
        HandleAnimations(); //Trigger appropriate animations not triggered by specifc actions
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {   
        //Check if you hit a wall
        if(!_character.isGrounded && hit.normal.y < 0.1f)
        {
            Debug.DrawRay(hit.point, hit.normal, Color.cyan, 1.25f);
            //_pounceCancel = true;
            if(_currentAttack == AttackType.POUNCE)
            {
                Debug.Log("Sweet Home Alabama");
            }
        }
        //Check if you are on a ramp
    }
    #endregion
 
    #region Setup Functions
    void SetJumpVars() //Primarily used to determine falling gravity and initial jump velocities
    {
        float timeToMaxJumpApex = _maxJumpTime/2;
        fallingGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToMaxJumpApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToMaxJumpApex;
        _initialJumpVelocityFlip = _initialJumpVelocity * .85f;
    }
 
    void SetAnimatorHashes() //Sets animator parameters to Hash values
    {
        _isMovingHash = Animator.StringToHash("isMoving");
        _isCrouchingHash = Animator.StringToHash("isCrouching");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _isShellSmashHash = Animator.StringToHash("isShellSmashing");
        _isPouncingHash = Animator.StringToHash("isPouncing");
    }
    #endregion
 
    #region Handle functions
    void HandleMovement()//Handle directional input
    {
        if(!stopMovement)
        {
            if(!_isShellSmashing && !_shellSmashAnimating)
            {
                if(_controls.Crouch)
                {
                    _appliedMove.x = _currMoveCrouch.x;
                    _appliedMove.z = _currMoveCrouch.z;
                   // currentSpeed = moveSpeed * crouchSpeedMod;
                }
                else
                {
                    _appliedMove.x = _currMove.x;
                    _appliedMove.z = _currMove.z;
                   // currentSpeed = moveSpeed;
                }
            }
        }
        _appliedMove = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * _appliedMove;
        _character.Move(_appliedMove * moveSpeed * Time.deltaTime);          
       
    }
 
    void HandleAnimations()//Change states in animator
    {
        if(!stopAnimations)
        {
            bool isWalking = _animator.GetBool(_isMovingHash);
   
            //Check if player's walking animation should be playing
            if(_controls.Move && !isWalking)
            {
                _animator.SetBool(_isMovingHash, true);
            }
            else if(!_controls.Move && isWalking)
            {
                _animator.SetBool(_isMovingHash, false);
            }
   
            //Check if player is crouching
            if((_controls.Crouch && !_isCrouching))
            {
                _animator.SetBool(_isCrouchingHash, true);
                _isCrouching = true;
            }
            else if((!_controls.Crouch && _isCrouching))
            {
                _animator.SetBool(_isCrouchingHash, false);
                _isCrouching = false;
            }
   
            //Check if player is ground pounding
            if((_isJumping && _controls.Crouch))
            {
                //_animator.SetBool(_isShellSmashHash, true);
                _animator.SetBool(_isJumpingHash, true);
            }
            else if(_character.isGrounded && _isShellSmashing)// !_isJumping)
            {
                StartCoroutine(EndGroundPound());          
            }
        }
    }
 
    void HandleRotation()//Handle rotation to face direction of movement
    {
        if(!stopRotation)
        {
            //Change position player is facing
            Vector3 positionToLookAt;
            positionToLookAt.x = _currMove.x;
            positionToLookAt.y = 0.0f;
            positionToLookAt.z = _currMove.z;
   
            //Current rotation of the player
            Quaternion currentRotation =  transform.rotation;
   
            if(_controls.Move && !_isShellSmashing)
            {
                Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
                //transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
   
                //Rotate playermovement towards direction
                float temp = _appliedMove.y;
                _appliedMove.y = 0;
                transform.rotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(_appliedMove), rotationFactorPerFrame * Time.deltaTime);
                _appliedMove.y = temp; //To avoid weird falling
                //transform.rotation.y = 0.0f;
            }
        }
    }
 
    void HandleGravity()//Make character fall if not grounded
    {
        //Handle Gravity Logic
        if(!stopGravity)
        {
            bool isFalling = _character.velocity.y < groundedGravity && !_controls.Jump; //Detect if player is falling and the jump button is not pressed. Allows for jump to only be as long as wanted
 
            //If grounded, gravity is constant. Else, calculate new velocity based on Verlet Integration
            if(_character.isGrounded)
            {
                //_animator.SetBool(isFallingHash, false);
                if(_jumpAnimating)
                {
                    //Handle Animation Param
                    _animator.SetBool(_isJumpingHash, false);
                    _jumpAnimating = false;
                }
                _isPouncing = false;
 
                _currMove.y = groundedGravity;
                _appliedMove.y = groundedGravity;
            }
            else
            {
                _appliedMove.y = VelocityVerletIntegration(isFalling);
                if(_character.velocity.y < groundedGravity)
                {
                    //_animator.SetBool(isFallingHash, true);
                }
            }
        }
    }
 
    void HandleJump()//Allow player to jump
    {
        if(!stopJump)
        {
            //Handle Jump Logic
            if(!_isJumping && _character.isGrounded && _controls.Jump)
            {
                if(_isCrouching) //Do jump in shell form
                {
                    //Handle Animation Param
                    _animator.SetBool(_isJumpingHash, true);
               
                    _jumpAnimating = true;
   
                    _isJumping = true;
                    _currMoveCrouch.y = _initialJumpVelocityFlip;
                    _currMove.y = _initialJumpVelocityFlip;
                    _appliedMove.y = _initialJumpVelocityFlip;
                }
                else //Do normal jump
                {
                    //Handle Animation Param
                    _animator.SetBool(_isJumpingHash, true);
                    _jumpAnimating = true;
   
                    _isJumping = true;
                    _currMove.y = _initialJumpVelocity;
                    _appliedMove.y = _initialJumpVelocity;
                }
            }
            else if (!_controls.Jump && _isJumping && _character.isGrounded)//Play around with this if double jump is wanted
            {
                _isJumping = false;
            }
        }
    }
 
    void HandleActions()//Handle player actions such as Ground Pound, and other interactions
    {
        if(!stopActions || _currentAttack == AttackType.NONE)
        {
            if(!_isAttacking && _controls.Attack)
            {
                if(_isJumping)
                {
                    if(_isCrouching && !_isShellSmashing) //Trigger Shell Smash
                    {
                        _shellSmashAnimating = true;
                        _isShellSmashing = true;
                        _animator.SetBool(_isShellSmashHash, true);

                        stopGravity = true;
                        stopActions = true;
                        stopMovement = true;

                        _currentAttack = AttackType.SHELLSMASH;
                        _isAttacking = true;
                        StartCoroutine(StartGroundPound());
                    }
                    else if(!_isCrouching && !_isPouncing) //Toggle Claw Pouce
                    {
                        _isPouncing = true;

                        //Handle Animation Param
                        _animator.SetBool(_isPouncingHash, true);
                        //_jumpAnimating = true;

                        _currentAttack = AttackType.POUNCE;
                        _isAttacking = true;

                        StartCoroutine(Pounce());
                    }
                }
            }
        }
    }
    #endregion
 
    #region Coroutines
    IEnumerator StartGroundPound()//Initialize Groundpound animations
    {
        _isShellSmashing  = true;
        _appliedMove = Vector3.zero;
        yield return new WaitForSeconds(.5f);
        stopGravity = false;
        _appliedMove.y = (groundPoundForce - fallingGravity);
    }

    IEnumerator EndGroundPound()//Finish the animation
    {
        yield return new WaitForSeconds(.25f);
        _animator.SetBool(_isShellSmashHash, false);
        _animator.SetBool(_isJumpingHash, false);
       
        _isShellSmashing = false;
        _shellSmashAnimating = false;
        _isAttacking = false;
       
        stopActions = false;
        stopMovement = false;
        _isShellSmashing  = false;

        _currentAttack = AttackType.NONE;
    }


    IEnumerator Pounce()//Initialize Groundpound animations
    {
        float startTime = Time.time;
        _appliedMove.y = 0f;
        _currMove.y = 0;
        externalModifier = .05f;
        stopJump = true;

        while(Time.time < startTime + _dashTime && !_pounceCancel && !_character.isGrounded)
        {
            _character.Move(_appliedMove * _dashSpeed * Time.deltaTime);
            yield return null;
        }

        _animator.SetBool(_isPouncingHash, false);
        stopJump = false;
        _pounceCancel = false;
        externalModifier = 1f;
        _currentAttack = AttackType.NONE;
        _isAttacking = false;
    }
 
    #endregion
 
    #region Usefull
    float VelocityVerletIntegration(bool isFalling) // Adjust velocity for gravity to avoid inconsistent landings
    {
        float modifier = 1;//If player is falling, make the fall go faster
        if(isFalling)
        {
            modifier = _fallModifier;
            //externalModifier = 1;
        }
 
        float previousYVel = _currMove.y;
        _currMove.y = _currMove.y + (fallingGravity * externalModifier * modifier * Time.deltaTime);
 
        return Mathf.Max((previousYVel + _currMove.y) * 0.5f, -20.0f); //return the next velocity for the y value
    }
    #endregion
 
}
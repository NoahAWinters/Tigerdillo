 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
 
public class PlayerMovement : MonoBehaviour //Depricated
{
    //Declare Reference Vars
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;
    [SerializeField]Transform _cameraTransform;
    [SerializeField]Collider _collider;
 
    //Animator hashes
    int _isMovingHash; //Triggers movement animation
    int _isCrouchingHash; //Triggers animation related to crouching
    int _isJumpingHash; //Handles if player is jumping
    int _isAttackingHash; //Will handle attacks
    int _isGPHash; //Handles ground pound animation
 
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
    

    //Kitty Krawl vars

    //Attack vars and input values
    AttackType _currentAttack; //Type of current attack being performed

    //Controller Fields
    bool _isNorthPressed;
    bool _isSouthPressed;  //Returns true if A or [Space] is pressed
    bool _isEastPressed;   //Returns true if B or [R] is pressed
    bool _isWestPressed;   //Returns true if X or [Tab] is pressed
 
    //StateBooleans
    bool _isCrouching;
    bool _isJumping;
    bool _isAttacking;
    bool _isFlipping; //Determine if player is flipping
    bool _jumpAnimating;
    bool _isGroundPounding; //Whether player is ground pounding in general
    bool _groundPoundAnimating; //Whether the animation is going
    bool canGroundPound; //Appears to be defunct, need to carefully remove
    
 
    //Handle-Function overrides, act as a way to easily cancel all of a certain Handle function
    bool stopMovement;
    bool stopRotation;
    bool stopGravity;
    bool stopJump;
    bool stopActions;
    bool stopAnimations;
 
    //Debug mode toggle
    bool debugMode;
   
    /////Constants
    //Movement
    [SerializeField] FloatReference moveSpeed;// General move speed
    [SerializeField] FloatReference groundPoundForce; // force of moving down during ground pound
    [SerializeField] float crouchSpeedMod = 0.5f; //To reduce speed by 2
    [SerializeField] float rotationFactorPerFrame = 15.0f;//How fast to rotate when moving. Most likely doesnt need to be serial
    float groundedGravity = -1.05f;
    float fallingGravity;
    float currentSpeed; //Im not sure why this is here, I need to figure that out


    #region /////Getters and Setters
    //Stoppers
    public bool IsStopMovement { get {return stopMovement; } }
    public bool IsStopRotation { get {return stopRotation; } }
    public bool IsStopGravity { get {return stopGravity; } }
    public bool IsStopJump { get {return stopJump; } }
    public bool IsStopActions { get {return stopActions; } }
    public bool IsStopAnimations { get {return stopAnimations; } }
    //Input
    public Vector2 MovementInput { get {return _currMoveInput; } }
    public bool IsMovementPressed { get { return _isMovePressed; } }
    public bool IsCrouching { get { return _isCrouchPressed; } }
    public bool IsJumping { get { return _isSouthPressed; } }
    public bool IsAttacking { get { return _isWestPressed; } }
    //Movement Values
    public Vector3 CurrentMovement { get { return _currMove; } }
    public Vector3 CurrentMovementWithCrouch { get { return _currMoveInput; } }
    public bool IsGroundPounding { get { return _isGroundPounding; } }
    #endregion
 
 
    #region Unity Functions
    void Awake()
    {
        //Set reference vars
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

 
        //Default to expecting no attack
        _currentAttack = AttackType.NONE;

        //SetAnimator Hashes
        SetAnimatorHashes();
 
        //Setup Controls
        SetMovementControls();
        SetCrouchControls();
        SetJumpControls();
        SetAttackControls();
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
 
    void OnEnable()
    {
        _playerInput.Gameplay.Enable();
    }
 
    void OnDisable()
    {
        _playerInput.Gameplay.Disable();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {   
        //Check if you hit a wall
        if(!_characterController.isGrounded && hit.normal.y < 0.1f)
        {
            Debug.DrawRay(hit.point, hit.normal, Color.cyan, 1.25f);
        }
    }
    #endregion
 
    #region OnInput Functions
    void OnMovementInput(InputAction.CallbackContext ctx) //Set if movemnet has been triggered
    {
        _currMoveInput = ctx.ReadValue<Vector2>();
 
        _currMove.x = _currMoveInput.x;//Standard movement
        _currMove.z = _currMoveInput.y;
 
        _currMoveCrouch.x = _currMoveInput.x * crouchSpeedMod; //Reduce speed if crouching
        _currMoveCrouch.z = _currMoveInput.y * crouchSpeedMod;
 
        _isMovePressed = (_currMoveInput.x != 0 || _currMoveInput.y != 0);
       
    }
 
    void OnCrouchInput(InputAction.CallbackContext ctx)//Detect if crouch is pressed
    {
        _isCrouchPressed = ctx.ReadValueAsButton();  
 
        //Set can Ground pound
        if(_isJumping && !_isCrouchPressed) //Groundpound can happen if player is jumping without pressing crouch
        {
            canGroundPound = true;
        }    
        else if(_isFlipping && !_isCrouchPressed) //Groundpound can happen if player flips but only after releasing crouch
        {
            _isFlipping = false;
            _isJumping = true;
            canGroundPound = true;
        }
        else if(!_isJumping && !_isFlipping && !_characterController.isGrounded)
        {
            _isJumping = true;
            canGroundPound = true;
        }
        else
        {
            canGroundPound = false;
        }
    }
 
    void OnJumpInput(InputAction.CallbackContext ctx)//Detect if jump was pressed
    {
        _isSouthPressed = ctx.ReadValueAsButton();          
    }

    void OnAttackInput
    (InputAction.CallbackContext ctx)//Detect if attack was pressed
    {
        _isWestPressed = ctx.ReadValueAsButton();          
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
 
    void SetCrouchControls() //Enables Crouch controls
    {
        //_playerInput.Gameplay.Crouch.started += OnCrouchInput; //Detect if Crouch has been activated
        _playerInput.Gameplay.Crouch.performed += OnCrouchInput;   //Detect changes in Crouch input
        _playerInput.Gameplay.Crouch.canceled += OnCrouchInput; //Detect if Crouch has finished
    }
 
    void SetMovementControls() //Enables Movment Controls
    {
        _playerInput.Gameplay.Movement.started += OnMovementInput;   //Detect LS or WASD Movement
        _playerInput.Gameplay.Movement.performed += OnMovementInput;   //Detect changes in LS Movement
        _playerInput.Gameplay.Movement.canceled += OnMovementInput;   //Detect end of WASD Movement
    }
 
    void SetJumpControls() //Enables Jump Controls
    {
        _playerInput.Gameplay.Jump.started += OnJumpInput; //Detect if Jump has been triggered
        _playerInput.Gameplay.Jump.canceled += OnJumpInput; //Detect if Jump finished
    }

    void SetAttackControls() //Enables Attack Controls
    {
        _playerInput.Gameplay.Attack.started += OnAttackInput; //Detect if Jump has been triggered
        _playerInput.Gameplay.Attack.canceled += OnAttackInput; //Detect if Jump finished
    }
 
    void SetAnimatorHashes() //Sets animator parameters to Hash values
    {
        _isMovingHash = Animator.StringToHash("isMoving");
        _isCrouchingHash = Animator.StringToHash("isCrouching");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _isGPHash = Animator.StringToHash("isAttacking");
    }
    #endregion
 
    #region Handle functions
    void HandleMovement()//Handle directional input
    {
        if(!stopMovement)
        {
            if(!_isGroundPounding && !_groundPoundAnimating)
            {
                if(_isCrouching)
                {
                    _appliedMove.x = _currMoveCrouch.x;
                    _appliedMove.z = _currMoveCrouch.z;
                    currentSpeed = moveSpeed.Value * crouchSpeedMod;
                }
                else
                {
                    _appliedMove.x = _currMove.x;
                    _appliedMove.z = _currMove.z;
                    currentSpeed = moveSpeed.Value;
                }
            }
        }
        _appliedMove = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * _appliedMove;
        _characterController.Move(_appliedMove * moveSpeed.Value * Time.deltaTime);          
       
    }
 
    void HandleAnimations()//Change states in animator
    {
        if(!stopAnimations)
        {
            bool isWalking = _animator.GetBool(_isMovingHash);
   
            //Check if player's walking animation should be playing
            if(_isMovePressed && !isWalking)
            {
                _animator.SetBool(_isMovingHash, true);
            }
            else if(!_isMovePressed && isWalking)
            {
                _animator.SetBool(_isMovingHash, false);
            }
   
            //Check if player is crouching
            if((_isCrouchPressed && !_isCrouching))
            {
                _animator.SetBool(_isCrouchingHash, true);
                _isCrouching = true;
            }
            else if((!_isCrouchPressed && _isCrouching))
            {
                _animator.SetBool(_isCrouchingHash, false);
                _isCrouching = false;
            }
   
            //Check if player is ground pounding
            if((_isJumping && _isCrouchPressed))
            {
                //_animator.SetBool(_isGPHash, true);
                _animator.SetBool(_isJumpingHash, true);
            }
            else if(_characterController.isGrounded && _isGroundPounding)// !_isJumping)
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
   
            if(_isMovePressed && !_isGroundPounding)
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
            bool isFalling = _characterController.velocity.y < groundedGravity && !_isSouthPressed; //Detect if player is falling and the jump button is not pressed. Allows for jump to only be as long as wanted
 
            //If grounded, gravity is constant. Else, calculate new velocity based on Verlet Integration
            if(_characterController.isGrounded)
            {
                //_animator.SetBool(isFallingHash, false);
                if(_jumpAnimating)
                {
                    //Handle Animation Param
                    _animator.SetBool(_isJumpingHash, false);
                    _jumpAnimating = false;
                }
 
                _currMove.y = groundedGravity;
                _appliedMove.y = groundedGravity;
            }
            else
            {
                _appliedMove.y = VelocityVerletIntegration(isFalling);
                if(_characterController.velocity.y < groundedGravity)
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
            if(!_isJumping && _characterController.isGrounded && _isSouthPressed)
            {
                if(_isCrouching ) //Do flipparooni, a higher jump (OLD)
                {
                    //canGroundPound = false;
                    //Handle Animation Param
                    _animator.SetBool(_isJumpingHash, true);
                    //_animator.SetBool(_isCrouchingHash, false);
               
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
            else if (!_isSouthPressed && _isJumping && _characterController.isGrounded)//Play around with this if double jump is wanted
            {
                _isJumping = false;
            }
        }
    }
 
    void HandleActions()//Handle player actions such as Ground Pound, and other interactions
    {
        if(!stopActions)
        {
            // if(_isGroundPounding && !_groundPoundAnimating) //handle GroundPound if Jumping (OLD)
            // {    
            //     if(debugMode)
            //     {    
            //         Debug.Log("Ground Pound Initiated");
            //     }
            //     _groundPoundAnimating = true;
            //     _animator.SetBool(_isGPHash, true);
            //     stopGravity = true;
            //     stopActions = true;
            //     stopMovement = true;
            //     StartCoroutine(StartGroundPound());
            // }
            if(!_isAttacking && _isWestPressed)
            {
                _isAttacking = true;

                if(_isCrouching && _isJumping)
                {
                    if(debugMode)
                    {    
                        Debug.Log("Ground Pound Initiated");
                    }
                    _groundPoundAnimating = true;
                    _isGroundPounding = true;
                    _animator.SetBool(_isGPHash, true);
                    stopGravity = true;
                    stopActions = true;
                    stopMovement = true;
                    StartCoroutine(StartGroundPound());
                }
            }
        }
    }
    #endregion
 
    #region Coroutines
    IEnumerator StartGroundPound()//Initialize Groundpound animations
    {
        if(debugMode)
        {
            Debug.Log("Ground Pound Coroutine Start");
        }
        _isGroundPounding  = true;
        _appliedMove = Vector3.zero;
        yield return new WaitForSeconds(.5f);
        stopGravity = false;
        _appliedMove.y = (groundPoundForce.Value - fallingGravity);
    }
 
    IEnumerator EndGroundPound()//Finish the animation
    {
        yield return new WaitForSeconds(.25f);
        _animator.SetBool(_isGPHash, false);
        _animator.SetBool(_isJumpingHash, false);
       
        _isGroundPounding = false;
        _groundPoundAnimating = false;
        _isAttacking = false;
       
        stopActions = false;
        stopMovement = false;
        _isGroundPounding  = false;
        if(debugMode)
        {
            Debug.Log("Ground Pound Finished");
        }
    }
    #endregion
 
    #region Usefull
    float VelocityVerletIntegration(bool isFalling) // Adjust velocity for gravity to avoid inconsistent landings
    {
        float modifier = 1;//If player is falling, make the fall go faster
        if(isFalling)
        {
            modifier = _fallModifier;
        }
 
        float previousYVel = _currMove.y;
        _currMove.y = _currMove.y + (fallingGravity * modifier * Time.deltaTime);
 
        return Mathf.Max((previousYVel + _currMove.y) * 0.5f, -20.0f); //return the next velocity for the y value
    }
    #endregion
 
}
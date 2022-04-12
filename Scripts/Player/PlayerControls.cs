using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    PlayerInput _playerInput; //Access the input doc
    PlayerActions _playerActions;

    Vector2 _currMoveInput;

    bool _isMovePressed;
    bool _isCrouchPressed;
    bool _isNorthPressed;
    bool _isSouthPressed;
    bool _isEastPressed;
    bool _isWestPressed;

    //GetSet
    public Vector2 MoveInput { get { return _currMoveInput; } }    
    public bool Move { get { return _isMovePressed; } }
    public bool Crouch { get { return _isCrouchPressed; } }
    public bool North { get { return _isNorthPressed; } }
    public bool Jump { get { return _isSouthPressed; } }
    public bool East { get { return _isEastPressed; } }
    public bool Attack { get { return _isWestPressed; } }

    #region Magic Functions
    void Awake()
    {
        _playerInput = new PlayerInput();
        _playerActions = GetComponent<PlayerActions>();

        _playerInput.Gameplay.Crouch.performed += OnCrouchInput;   //Detect changes in Crouch input
        _playerInput.Gameplay.Crouch.canceled += OnCrouchInput; //Detect if Crouch has finished

        _playerInput.Gameplay.Movement.started += OnMovementInput;   //Detect LS or WASD Movement
        _playerInput.Gameplay.Movement.performed += OnMovementInput;   //Detect changes in LS Movement
        _playerInput.Gameplay.Movement.canceled += OnMovementInput;   //Detect end of WASD Movement

        _playerInput.Gameplay.Jump.started += OnJumpInput; //Detect if Jump has been triggered
        _playerInput.Gameplay.Jump.canceled += OnJumpInput; //Detect if Jump finished

        _playerInput.Gameplay.Attack.started += OnAttackInput; //Detect if Jump has been triggered
         _playerInput.Gameplay.Attack.performed += OnAttackInput; 
        _playerInput.Gameplay.Attack.canceled += OnAttackInput; //Detect if Jump finished
    }

    void OnEnable()
    {
        _playerInput.Gameplay.Enable();
    }
 
    void OnDisable()
    {
        _playerInput.Gameplay.Disable();
    }
    #endregion

    #region Input Functions
    void OnMovementInput(InputAction.CallbackContext ctx) //Set if movemnet has been triggered
    {
        _currMoveInput = ctx.ReadValue<Vector2>();

        _playerActions.CurrentMovementX = _currMoveInput.x;//Standard movement
        _playerActions.CurrentMovementZ = _currMoveInput.y;
 
        _playerActions.CurrentMovementWithCrouchX = _currMoveInput.x * 2; //Reduce speed if crouching
        _playerActions.CurrentMovementWithCrouchZ = _currMoveInput.y * 2;

        _isMovePressed = (_currMoveInput.x != 0 || _currMoveInput.y != 0);
    }

    void OnCrouchInput(InputAction.CallbackContext ctx)//Detect if crouch is pressed
    {
        _isCrouchPressed = ctx.ReadValueAsButton();
    } 

    void OnAttackInput(InputAction.CallbackContext ctx)//Detect if attack was pressed
    {
        _isWestPressed = ctx.ReadValueAsButton();          
    }

    void OnJumpInput(InputAction.CallbackContext ctx)//Detect if jump was pressed
    {
        _isSouthPressed = ctx.ReadValueAsButton();          
    }
    #endregion
}

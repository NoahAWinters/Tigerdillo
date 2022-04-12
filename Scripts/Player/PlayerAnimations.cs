// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerAnimations : MonoBehaviour
// {
//     //Reference Vars
//     [SerializeField] PlayerActions _actions; //To fill the variables
//     [SerializeField] PlayerControls _controls; //To check if a button has been pressed
//     [SerializeField] Animator _animator;

//     //Animator Hashes
//     int _isMovingHash;
//     int _isCrouchingHash;
//     int _isJumpingHash;
//     int _isAttackingHash;

//     //Constructor
//     // public PlayerAnimations(PlayerActions actions, PlayerControls controls)
//     // {
//     //     _actions = actions;
//     //     _controls = controls;
//     //     _animator = actions.Animator;
//     // }
    

//     //Custom Functions
//     void SetAnimatorHashes() //Sets animator parameters to Hash values
//     {
//         _isMovingHash = Animator.StringToHash("isMoving");
//         _isCrouchingHash = Animator.StringToHash("isCrouching");
//         _isJumpingHash = Animator.StringToHash("isJumping");
//         _isAttackingHash = Animator.StringToHash("isAttacking");
//     }

//     void FixedUpdate()//Change states in animator
//     {
//         if(!_actions.StopAnimations)
//         {
//             bool isWalking = _animator.GetBool(_isMovingHash);
   
//             //Check if player's walking animation should be playing
//             if(_controls.Move && !isWalking)
//             {
//                 _animator.SetBool(_isMovingHash, true);
//             }
//             else if(!_controls.Move && isWalking)
//             {
//                 _animator.SetBool(_isMovingHash, false);
//             }
   
//             //Check if player is crouching
//             if((_controls.Crouch && !_actions.IsCrouching))
//             {
//                 _animator.SetBool(_isCrouchingHash, true);
//                 _actions.IsCrouching = true;;
//             }
//             else if((!_controls.Crouch && _actions.IsCrouching))
//             {
//                 _animator.SetBool(_isCrouchingHash, false);
//                 _actions.IsCrouching = false;
//             }
   
//             //Check if player is ground pounding
//             if((_actions.IsJumping && _controls.Crouch))
//             {
//                 //_animator.SetBool(_isGPHash, true);
//                 _animator.SetBool(_isJumpingHash, true);
//             }
//             else if(_actions.Controller.isGrounded && _actions.IsGroundPounding)// !_isJumping)
//             {
//                 _actions.EndGroundPoundTrigger();          
//             }
//         }
//     }
// }

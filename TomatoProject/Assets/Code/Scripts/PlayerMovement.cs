using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour{

    #region Rotation
    [SerializeField] private float smoothRotation = 0.05f;
    private float currentVelocity;
    #endregion
    #region Movement
    [SerializeField] private float speed = 5f;

    private CharacterController _characterController;
    private Vector2 _input;
    private Vector3 direction;
    #endregion

    private float _gravity = -9.8f;
    private float _velocity;
    [SerializeField] private float gravityMultiplier = 3.0f;

    private void Awake(){
        
        _characterController = GetComponent<CharacterController>();
    }

    private void Update(){

        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
    }

    private void ApplyRotation(){

        //Player rotation
        if (_input.sqrMagnitude == 0) return;

        var targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothRotation);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }

    private void ApplyMovement(){

        //Move player
        _characterController.Move(direction * speed * Time.deltaTime);
    }

    private void ApplyGravity(){

        if (_characterController.isGrounded && _velocity < 0.0f){
            _velocity = -1.0f;
        }else{
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }

        direction.y = _velocity;
    }

    public void Move(InputAction.CallbackContext context){

        //Get input as a Vector2 to set movement direction
        _input = context.ReadValue<Vector2>();
        direction = new Vector3(_input.x, 0f, _input.y);
    }
}
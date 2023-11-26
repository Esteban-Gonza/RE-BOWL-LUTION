using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour{

    [Header("Movement")]
    #region Rotation
    [SerializeField] private float smoothRotation = 0.05f;
    private float currentVelocity;
    #endregion

    #region Movement
    [SerializeField] private float speed = 5f;
    public Vector3 direction;
    private Vector2 _input;
    private float inputMagnitude;
    private bool isRunning;
    #endregion

    [Header("References")]
    private CharacterController _characterController;
    private Animator playerAnimator;

    private void Awake(){
        _characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update(){
        PlayerMovementandRotation();
        InputMagnitude();

        if (inputMagnitude > 0.1 && Input.GetKey(KeyCode.LeftShift)){
            speed = 10;
            isRunning = true;
        }else{
            speed = 5f;
            isRunning = false;
        }
    }

    private void PlayerMovementandRotation(){

        //Rotate player
        var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        var skeedDirection = matrix.MultiplyPoint3x4(direction);

        if (_input.sqrMagnitude == 0) return;

        var targetAngle = Mathf.Atan2(skeedDirection.x, skeedDirection.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothRotation);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

        //Move player
        _characterController.Move(skeedDirection * speed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context){

        //Get input as a Vector2 to set movement direction
        _input = context.ReadValue<Vector2>();
        direction = new Vector3(_input.x, 0f, _input.y);
    }

    void InputMagnitude(){

        //Calculate the Input Magnitude
        inputMagnitude = new Vector2(direction.x, direction.z).sqrMagnitude;

        playerAnimator.SetFloat("InputMagnitude", inputMagnitude);
        playerAnimator.SetBool("IsRunning", isRunning);
    }

    private void OnDisable(){

        playerAnimator.SetFloat("InputMagnitude", 0);
    }
}
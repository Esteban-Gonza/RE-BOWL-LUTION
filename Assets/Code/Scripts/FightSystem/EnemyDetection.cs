using UnityEngine;

public class EnemyDetection : MonoBehaviour{

    private Vector3 inputDirection;
    public LayerMask layerMask;

    [Header("Reference")]
    [SerializeField] private PlayerMovement movementInput;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private Combat combatScript;
    [SerializeField] private Enemy currentTarget;

    public GameObject cam;

    private void Start(){
        movementInput = GetComponentInParent<PlayerMovement>();
        combatScript = GetComponentInParent<Combat>();
    }

    private void Update(){
        
        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        inputDirection = forward * movementInput.direction.z + right * movementInput.direction.x;
        inputDirection = inputDirection.normalized;

        RaycastHit enemiesInfo;

        if(Physics.SphereCast(transform.position, 3f, inputDirection, out enemiesInfo, 10, layerMask)){

            if (enemiesInfo.collider.transform.GetComponent<Enemy>().IsAttackable())
                currentTarget = enemiesInfo.collider.transform.GetComponent<Enemy>();
        }
    }

    public Enemy CurrentTarget(){
        return currentTarget;
    }

    public void SetCurrentTarget(Enemy target){
        currentTarget = target;
    }

    public float InputMagnitude(){
        return inputDirection.magnitude;
    }

    /*private void OnDrawGizmos(){
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, inputDirection);
        Gizmos.DrawWireSphere(transform.position, 1);
        if (CurrentTarget() != null)
            Gizmos.DrawSphere(CurrentTarget().transform.position, 1f);
    }*/
}
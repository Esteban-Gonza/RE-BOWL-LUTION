using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Enemy : MonoBehaviour {

    [Header("Stats")]
    public int health = 3;
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    [Header("References")]
    public Animator animator;
    private Combat playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private CharacterController characterController;

    // Coroutines
    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;

    [Header("Events")]
    public UnityEvent<Enemy> OnDamage;
    public UnityEvent<Enemy> OnStopMoving;
    public UnityEvent<Enemy> OnRetreat;

    private void Start() {
        enemyManager = GetComponentInParent<EnemyManager>();

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindObjectOfType<Combat>();
        enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        playerCombat.OnHit.AddListener((x) => OnPlayerHit(x));
        playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
        playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    IEnumerator EnemyMovement() {
        yield return new WaitUntil(() => isWaiting == true);

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1) {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        } else {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    private void Update() {

        transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));

        MoveEnemy(moveDirection);
    }

    void OnPlayerHit(Enemy target) {

        if (target == this) {

            StopEnemyCoroutines();
            DamageCoroutine = StartCoroutine(HitCoroutine());

            enemyDetection.SetCurrentTarget(null);
            isLockedTarget = false;
            OnDamage.Invoke(this);

            health--;

            if (health <= 0) {
                Death();
                return;
            }

            animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);

            StopMoving();
        }

        IEnumerator HitCoroutine() {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;
        }
    }

    void OnPlayerCounter(Enemy target){
        if (target == this){
            PrepareAttack(false);
        }
    }

    void OnPlayerTrajectory(Enemy target){
        if(target == this){
            StopEnemyCoroutines();
            isLockedTarget = true;
            PrepareAttack(false);
            StopMoving();
        }
    }

    void Death(){

        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailability(this, false);
    }

    public void SetRetreat(){
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepareRetreat());

        IEnumerator PrepareRetreat(){
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            isRetreating = true;
            moveDirection = -Vector3.forward;
            isMoving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, playerCombat.transform.position) > 4);
            isRetreating = false;
            StopMoving();

            isWaiting = true;
            MovementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    public void SetAttack(){
        isWaiting = false;

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());

        IEnumerator PrepAttack(){
            PrepareAttack(true);
            yield return new WaitForSeconds(.2f);
            moveDirection = Vector3.forward;
            isMoving = true;
        }
    }

    void PrepareAttack(bool active){
        isPreparingAttack = active;

        if (active){

        }else{
            StopMoving();
        }
    }

    void MoveEnemy(Vector3 direction) {

        moveSpeed = 1;

        if (direction == Vector3.forward)
            moveSpeed = 8;
        if (direction == -Vector3.forward)
            moveSpeed = 2;

        // Set animator values
        animator.SetFloat("InputMagnitude", (characterController.velocity.normalized.magnitude * direction.z) / (5 / moveSpeed), .2f, Time.deltaTime);
        animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        if (!isMoving)
            return;

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;
        Vector3 moveDir = Vector3.zero;

        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward)
            finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left)
            finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward)
            finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left)
            moveSpeed /= 1.5f;

        moveDir += finalDirection * moveSpeed * Time.deltaTime;

        characterController.Move(moveDir);

        if (!isPreparingAttack)
            return;

        if(Vector3.Distance(transform.position, playerCombat.transform.position) < 2){

            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
    }

    void Attack(){
        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        animator.SetTrigger("Punch");
    }

    public void HitEvent(){
        if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
            playerCombat.DamageEvent();

        PrepareAttack(false);
        SoundManager.instance.PlayHit();
    }

    public void StopMoving(){
        isMoving = false;
        moveDirection = Vector3.zero;
        if (characterController.enabled)
            characterController.Move(moveDirection);
    }

    void StopEnemyCoroutines(){

        PrepareAttack(false);

        if (isRetreating){
            if(RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if (DamageCoroutine != null)
            StopCoroutine(DamageCoroutine);

        if(MovementCoroutine != null)
            StopCoroutine(MovementCoroutine);
    }

    #region States
    public bool IsAttackable(){
        return health > 0;
    }

    public bool IsPreparingAttack(){
        return isPreparingAttack;
    }

    public bool IsRetreating(){
        return isRetreating;
    }

    public bool IsLockedTarget(){
        return isLockedTarget;
    }

    public bool IsStunned(){
        return isStunned;
    }
    #endregion
}
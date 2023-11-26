using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Combat : MonoBehaviour {

    int animationCount = 0;
    string[] attacks;
    [SerializeField] private float health = 5;
    [SerializeField] private float attackCooldown;

    [Header("References")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject instructions;
    private Enemy[] enemyScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private PlayerMovement playerMovement;
    private CharacterController playerCharacterController;
    private Animator animator;

    private Enemy lockedTarget;

    [SerializeField] private Transform punchPosition;
    [SerializeField] private Transform lastHitFocusObject;

    [Header("States")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;

    // Coroutines
    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;

    [Space][Header("Events")]
    public UnityEvent<Enemy> OnTrajectory;
    public UnityEvent<Enemy> OnHit;
    public UnityEvent<Enemy> OnCounterAttack;

    private void Start() {
        playerCharacterController = GetComponent<CharacterController>();
        enemyScript = FindObjectsOfType<Enemy>();
        enemyManager = FindObjectOfType<EnemyManager>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        gameOverScreen.SetActive(false);
        StartCoroutine(ShowInstructions());
    }

    void AttackCheck() {

        if (isAttackingEnemy)
            return;

        // Check if detection set an enemy
        if (enemyDetection.CurrentTarget() == null) {
            if (enemyManager.AliveEnemyCount() == 0) {
                Attack(null, 0);
                return;
            } else {
                lockedTarget = enemyManager.RandomEnemy();
            }
        }

        // If the player is moving, use directional detection to set the enemy
        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        // chech again if target is set
        if (lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        Attack(lockedTarget, TargetDistance(lockedTarget));
    }

    public void Attack(Enemy target, float distance) {
        //Types of attack animation
        attacks = new string[] { "PunchingRight", "Uppercut", "FlyingKick", "PunchingLeft" };

        // Attack to nothing if target is null
        if (target == null) {
            AttackType("PunchingLeft", .2f, null, 0);
            return;
        }

        if (distance < 15) {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            string attackString = IsLastHit() ? attacks[Random.Range(0, attacks.Length)] : attacks[animationCount];
            AttackType(attackString, attackCooldown, target, .65f);
        } else {
            lockedTarget = null;
            AttackType("Punching_Left", .2f, null, 0);
        }
    }

    void AttackType(string attackTrigger, float cooldown, Enemy target, float movementDuration) {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(IsLastHit() ? 1.5f : cooldown));

        // Check last enemy
        if (IsLastHit())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
            return;

        target.StopMoving();
        MoveTorwardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration) {
            isAttackingEnemy = true;
            playerMovement.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            playerMovement.enabled = true;
        }

        IEnumerator FinalBlowCoroutine() {
            Time.timeScale = 0.5f;
            lastHitFocusObject.position = lockedTarget.transform.position;
            yield return new WaitForSecondsRealtime(2);
            Time.timeScale = 1f;
            Debug.Log("Moment of scale");
        }
    }

    void MoveTorwardsTarget(Enemy target, float duration) {

        OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .2f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    void CounterCheck(){

        if (isCountering || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack.Invoke(lockedTarget);

        if(TargetDistance(lockedTarget) > 2){
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        float duration = .2f;
        animator.SetTrigger("Dodge");
        transform.DOLookAt(lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if(counterCoroutine != null)
            StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float duration){
            isCountering = true;
            playerMovement.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;
        }
    }

    float TargetDistance(Enemy target) {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target) {

        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    public void HitEvent() {

        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0)
            return;

        OnHit.Invoke(lockedTarget);
        SoundManager.instance.PlayHit();
    }

    public void DamageEvent(){
        health--;

        if (health == 0){
            Death();
            return;
        }

        animator.SetTrigger("Hit");

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());

        IEnumerator DamageCoroutine(){
            playerMovement.enabled = false;
            yield return new WaitForSeconds(.5f);
            playerMovement.enabled = true;
        }
    }

    void Death(){
        this.enabled = false;
        playerMovement.enabled = false;
        animator.SetTrigger("Death");
        gameOverScreen.SetActive(true);
        for(int i = 0; i < enemyScript.Length; i++){
            enemyScript[i].animator.SetFloat("InputMagnitude", 0f);
            enemyScript[i].enabled = false;
        }
    }

    Enemy ClosestCounterEnemy(){

        float minDistance = 100;
        int finalIndex = 0;

        for(int i = 0; i < enemyManager.allEnemies.Length; i++){
            Enemy enemy = enemyManager.allEnemies[i].enemyScript;

            if (enemy.IsPreparingAttack()){
                if(Vector3.Distance(transform.position, enemy.transform.position) < minDistance){

                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return enemyManager.allEnemies[finalIndex].enemyScript;
    }

    IEnumerator ShowInstructions(){
        instructions.SetActive(true);
        yield return new WaitForSeconds(8f);
        instructions.SetActive(false);
    }

    bool IsLastHit(){
        if (lockedTarget == null)
            return false;

        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.health <= 1;
    }

    #region Input

    private void OnCounter(){
        CounterCheck();
    }

    private void OnAttack(){
        AttackCheck();
    }

    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour{

    private Enemy[] enemies;
    public EnemyStruct[] allEnemies;

    List<int> enemyIndexes;

    // AI Loop
    private Coroutine AI_Loop_Coroutine;

    public int aliveEnemyCount;

    private void Start(){

        enemies = GetComponentsInChildren<Enemy>();

        allEnemies = new EnemyStruct[enemies.Length];

        for(int i = 0; i < allEnemies.Length; i++){

            allEnemies[i].enemyScript = enemies[i];
            allEnemies[i].enemyAvailability = true;
        }

        StartAI();
    }

    public void StartAI(){
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(Enemy enemy){

        if(AliveEnemyCount() == 0){
            StopCoroutine(AI_Loop(null));
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        Enemy attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
            attackingEnemy = RandomEnemy();
        if (attackingEnemy == null)
            yield break;

        yield return new WaitUntil(() => attackingEnemy.IsRetreating() == false);
        yield return new WaitUntil(() => attackingEnemy.IsLockedTarget() == false);
        yield return new WaitUntil(() => attackingEnemy.IsStunned() == false);

        attackingEnemy.SetAttack();
        yield return new WaitUntil(() => attackingEnemy.IsPreparingAttack() == false);

        attackingEnemy.SetRetreat();
        yield return new WaitForSeconds(Random.Range(0, 0.5f));

        if(AliveEnemyCount() > 0){
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
        }
    }

    public Enemy RandomEnemy(){

        enemyIndexes = new List<int>();

        for(int i = 0; i < allEnemies.Length; i++){

            if (allEnemies[i].enemyAvailability)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        Enemy randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public Enemy RandomEnemyExcludingOne(Enemy exclude){

        enemyIndexes = new List<int>();

        for(int i =0; i < allEnemies.Length; i++){
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript != exclude){
                enemyIndexes.Add(i);
            }
        }

        if(enemyIndexes.Count == 0){
            return null;
        }

        Enemy randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public int AvailableEnemyCount(){

        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++){

            if (allEnemies[i].enemyAvailability)
                count++;
        }

        return count;
    }

    public bool AnEnemyIsPreparingAttack(){

        foreach(EnemyStruct enemyStruct in allEnemies){
        
            if(enemyStruct.enemyScript.IsPreparingAttack())
                return true;
        }

        return false;
    }

    public int AliveEnemyCount(){

        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++){
            if (allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        aliveEnemyCount = count;
        return count;
    }

    public void SetEnemyAvailability(Enemy enemy, bool state){

        for(int i = 0; i < allEnemies.Length; i++){

            if (allEnemies[i].enemyScript == enemy)
                allEnemies[i].enemyAvailability = state;
        }

        if (FindObjectOfType<EnemyDetection>().CurrentTarget() == enemy)
            FindObjectOfType<EnemyDetection>().SetCurrentTarget(null);
    }
}

[System.Serializable]
public struct EnemyStruct{
    public Enemy enemyScript;
    public bool enemyAvailability;
}
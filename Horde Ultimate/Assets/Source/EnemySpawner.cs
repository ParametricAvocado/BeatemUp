using UnityEngine;

public class EnemySpawnSubRoutine
{
    public GameObject enemyPrefab;
    public int amount;
    public float delay;
    public float cooldown;

    public bool IsComplete => SpawnedCount == amount;

    public int SpawnedCount { get; private set; } = 0;
    public float CurrentCooldown { get; private set; } = 0;

    EnemySpawnRoutine parentRoutine;

    public void Initialize(EnemySpawnRoutine routine)
    {
        parentRoutine = routine;
    }

    public void Start()
    {
        SpawnedCount = 0;
        CurrentCooldown = delay;
    }

    public void Update()
    {
        if (IsComplete) return;

        CurrentCooldown -= Time.deltaTime;
        if (CurrentCooldown <= 0)
        {
            if (SpawnedCount < amount)
            {
                parentRoutine.ParentSpawner.SpawnEnemy(enemyPrefab);
                SpawnedCount++;
            }

            CurrentCooldown = cooldown;
        }
    }
}

public class EnemySpawnRoutine
{
    public EnemySpawnSubRoutine[] subroutines;
    public EnemySpawner ParentSpawner { get; private set; }
    public bool IsComplete { get; private set; } = false;

    public void Initialize(EnemySpawner spawner)
    {
        ParentSpawner = spawner;
        for (int i = 0; i < subroutines.Length; i++)
        {
            subroutines[i].Initialize(this);
        }
    }

    public void Start()
    {
        IsComplete = false;
        for (int i = 0; i < subroutines.Length; i++)
        {
            subroutines[i].Start();
        }
    }

    public void Update()
    {
        IsComplete = true;
        for (int i = 0; i < subroutines.Length; i++)
        {
            subroutines[i].Update();
            if (!subroutines[i].IsComplete) IsComplete = false;
        }
    }
}

public class EnemySpawner : MonoBehaviour
{
    public PlayerCharacter playerCharacter;
    public EnemySpawnRoutine[] enemySpawnRoutines;

    public float spawnRadius = 10;
    EnemySpawnRoutine currentRoutine;

    private void Start()
    {
        for (int i = 0; i < enemySpawnRoutines.Length; i++)
        {
            enemySpawnRoutines[i].Initialize(this);
        }
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        float angle = Random.Range(0, 360);
        Vector3 spawnOffset = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * spawnRadius;
        Instantiate(enemyPrefab, playerCharacter.transform.position + spawnOffset, Quaternion.LookRotation(-spawnOffset));
    }

    private void Update()
    {
        if (!playerCharacter || playerCharacter.IsDead) return;


        if (currentRoutine == null)
        {
            currentRoutine = enemySpawnRoutines[Random.Range(0, enemySpawnRoutines.Length)];
            currentRoutine.Start();
        }
        else
        {
            currentRoutine.Update();

            if (currentRoutine.IsComplete)
            {
                currentRoutine = null;
            }
        }
    }
}

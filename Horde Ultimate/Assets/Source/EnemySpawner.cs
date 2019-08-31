using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public PlayerCharacter playerCharacter;
    public GameObject enemyPrefab;

    public float spawnRadius = 10;
    public float spawnCooldown = 5;

    float spawnTimeLeft;

    private void OnEnable()
    {
        spawnTimeLeft = spawnCooldown;
    }
    private void Update()
    {
        if (spawnTimeLeft > 0)
        {
            spawnTimeLeft -= Time.deltaTime;
        }
        else
        {
            float angle = Random.Range(0, 360);
            Vector3 spawnOffset = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * spawnRadius;
            Instantiate(enemyPrefab, playerCharacter.transform.position + spawnOffset, Quaternion.LookRotation(-spawnOffset));
            spawnTimeLeft = spawnCooldown;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyCharacter : Character
{
    PlayerCharacter targetPlayer;

    void Start()
    {
        targetPlayer = FindObjectOfType<PlayerCharacter>();
        animator.SetFloat(hRun, 1);
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        Vector3 direction = Vector3.ProjectOnPlane(targetPlayer.transform.position - transform.position, Vector3.up).normalized;
        Move(direction);

    }

    private void Update()
    {
        if (IsDead) return;
        if (Vector3.Distance(transform.position, targetPlayer.transform.position) < maxAttackDistance)
        {

        }
    }

    public override void OnDamage()
    {
        if (!IsDead)
        {
            IsDead = true;
            characterController.enabled = false;

            animator.SetTrigger(hDeath);
            animator.SetFloat(hDeathRandom, Random.Range(0, 6));

            transform.DOShakePosition(damageShakeDuration, 0.1f, 40);
            Destroy(gameObject, destroyAfterDeathTime);
        }
    }
}

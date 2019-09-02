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
        if (!CanMove) return;
        if (!targetPlayer || targetPlayer.IsDead) return;

        Vector3 direction = Vector3.ProjectOnPlane(targetPlayer.transform.position - transform.position, Vector3.up).normalized;
        Move(direction);
    }

    private void Update()
    {
        if (!CanAttack) return;

        if (!targetPlayer || targetPlayer.IsDead) return;

        if (Vector3.Distance(transform.position, targetPlayer.transform.position) < maxAttackDistance)
        {
            if (CanAttackCharacter(targetPlayer))
            {
                AttackCharacter(targetPlayer);
            }
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        FindObjectOfType<KillCounter>().AddKill();
    }
}

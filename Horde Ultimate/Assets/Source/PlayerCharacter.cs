using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

public class PlayerCharacter : Character
{
    [Header("Input")]
    public float dragDistance = 0.05f;
    public float dragTime;
    public float maxAngle = 45;

    bool isDragging = false;

    Vector2 pointerPosition;
    Vector2 dragStart;

    Vector2 screenResolution;
    float screenAspect;

    Collider[] enemyColliders = new Collider[10];

    private void Start()
    {
        screenResolution = new Vector2(Screen.width, Screen.height);
        screenAspect = screenResolution.x / screenResolution.y;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        pointerPosition = Input.mousePosition / screenResolution;

        if (isDragging)
        {
            Vector2 delta = pointerPosition - dragStart;
            delta.y /= screenAspect;

            if (delta.magnitude > dragDistance)
            {
                Swipe(delta);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void BeginDrag()
    {
        isDragging = true;
        dragStart = pointerPosition;
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    private void Swipe(Vector2 swipeVector)
    {
        EndDrag();

        if (IsAttacking) return;

        Vector3 attackDirection = new Vector3(swipeVector.x, 0, swipeVector.y).normalized;
        AttackMove(attackDirection);
    }

    private void AttackMove(Vector3 direction)
    {
        int overlapped = Physics.OverlapSphereNonAlloc(transform.position, maxAttackDistance, enemyColliders, EnemyMask);

        Vector3 toEnemy;
        float enemyDistance;

        float closestDistance = float.MaxValue;
        Collider closestCollider = null;
        Vector3 closestDirection = Vector3.zero;

        attackTarget = null;


        for (int i = 0; i < overlapped; i++)
        {
            toEnemy = Vector3.ProjectOnPlane(enemyColliders[i].transform.position - transform.position, Vector3.up);
            enemyDistance = toEnemy.magnitude;
            if (Vector3.Angle(direction, toEnemy) < maxAngle)
            {
                if (enemyDistance < closestDistance)
                {
                    closestCollider = enemyColliders[i];
                    closestDistance = enemyDistance;
                    closestDirection = toEnemy.normalized;
                }
            }
        }
        IsAttacking = true;

        Vector3 attackMovePosition;


        if (closestCollider)
        {
            attackTarget = closestCollider.GetComponent<Character>();

            attackMovePosition = transform.position + closestDirection * (closestDistance - minAttackDistance);
            transform.rotation = Quaternion.LookRotation(closestDirection);
        }
        else
        {
            attackMovePosition = transform.position + direction * maxAttackDistance;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        BeginAttackMove(attackMovePosition);
    }
}

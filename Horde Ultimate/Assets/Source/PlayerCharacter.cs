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

    bool swipeQueued;
    Vector2 queuedSwipe;
    float screenAspect;

    private void Start()
    {
        screenResolution = new Vector2(Screen.width, Screen.height);
        screenAspect = screenResolution.x / screenResolution.y;
    }

    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
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

        DequeueSwipe();
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
        swipeQueued = true;
        queuedSwipe = swipeVector;
    }

    public void DequeueSwipe()
    {
        if (!swipeQueued || !CanAttack) return;

        swipeQueued = false;
        Vector3 attackDirection = new Vector3(queuedSwipe.x, 0, queuedSwipe.y).normalized;

        var closestCharacter = GetClosestCharacterInCone(maxAttackDistance, attackDirection, maxAngle, EnemyMask);
        if (closestCharacter)
        {
            DoTargetedAction(closestCharacter);
        }
        else
        {
            AttackDirection(attackDirection);
        }
    }
}

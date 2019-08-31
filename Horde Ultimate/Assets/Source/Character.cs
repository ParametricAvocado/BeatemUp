using DG.Tweening;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Animator animator;

    [Header("Combat")]
    public float minAttackDistance = 0.3f;
    public float maxAttackDistance = 5;
    public float attackMoveDuration = 0.1f;
    public Ease attackMoveEasing = Ease.OutQuint;

    public float damageShakeDuration = 0.2f;
    public float destroyAfterDeathTime = 1f;
    public float hitStopScale = 0.2f;
    public float hitStopDuration = 0.2f;
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Status")]
    public bool IsAttacking = false;
    public bool IsDead = false;

    public Character attackTarget;


    public static LayerMask PlayerMask => 1 << LayerMask.NameToLayer("Player");
    public static LayerMask EnemyMask => 1 << LayerMask.NameToLayer("Enemy");

    public static int hMirror = Animator.StringToHash("Mirror");
    public static int hRun = Animator.StringToHash("Run");
    public static int hAttackBegin = Animator.StringToHash("AttackBegin");
    public static int hAttackComplete = Animator.StringToHash("AttackComplete");
    public static int hAttackRandom = Animator.StringToHash("AttackRandom");
    public static int hDeath = Animator.StringToHash("Death");
    public static int hDeathRandom = Animator.StringToHash("DeathRandom");


    public virtual void OnDamage() { }

    protected void BeginAttackMove(Vector3 position)
    {
        animator.SetBool(hMirror, !animator.GetBool(hMirror));
        animator.SetTrigger(hAttackBegin);
        animator.SetFloat(hAttackRandom, Random.Range(0, 2));
        var moveTween = transform.DOMove(position, attackMoveDuration).SetEase(attackMoveEasing);
        moveTween.OnComplete(OnAttackMoveComplete);
    }

    protected void OnAttackMoveComplete()
    {
        animator.SetTrigger(hAttackComplete);

        IsAttacking = false;
        if (attackTarget)
        {
            attackTarget.OnDamage();
            TimeManager.Instance.HitStop(hitStopScale, hitStopDuration);
        }
    }

    protected void Move(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);

        characterController.Move(direction * moveSpeed * Time.deltaTime);
    }
}
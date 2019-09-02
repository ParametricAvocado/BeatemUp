using DG.Tweening;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Animator animator;

    [Header("Combat")]
    public bool enableAttack = true;
    public bool enableParry = false;
    [Space]
    public float targetlessAttackDistance = 2;
    public float minAttackDistance = 0.3f;
    public float maxAttackDistance = 5;
    public float attackMoveDuration = 0.1f;
    public Ease attackMoveEasing = Ease.OutQuint;
    [Space]
    public float parriedAttackPushDistance = 1f;
    public float parriedAttackMoveDuration = 0.3f;
    public Ease parriedAttackMoveEasing = Ease.OutQuint;
    [Space]
    public float parrySuccessPushDistance = 1f;
    public float parrySuccessMoveDuration = 0.1f;
    public Ease parrySuccessMoveEasing = Ease.OutQuint;
    [Space]
    public float damageShakeDuration = 0.2f;
    public float destroyAfterDeathTime = 1f;
    [Space]
    public TimeManager.TimescaleEvent attackBeginHitStop;
    public TimeManager.TimescaleEvent attackCompleteHitStop;
    public TimeManager.TimescaleEvent parryHitStop;
    public TimeManager.TimescaleEvent deathHitStop;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Status")]
    public bool IsAttacking = false;
    public bool IsParrying = false;
    public bool HasSuccessfullyParried = false;
    public bool IsStaggered = false;
    public bool IsDead = false;
    public bool IsBusy => (IsAttacking || IsParrying || IsStaggered);
    public bool CanMove => !(IsDead || IsBusy);
    public bool CanAttack => !IsDead && enableAttack && (!IsBusy || HasSuccessfullyParried);
    public bool CanParry => !IsDead && enableParry && (!IsBusy || HasSuccessfullyParried);

    public int TargettedByCount = 0;

    public Character targetCharacter;
    public Vector3 attackDirection;

    public UnityEvent onDeath;

    protected Collider[] potentialTargetColliders = new Collider[10];
    protected int potentialTargetCount = 0;

    public static LayerMask PlayerMask => 1 << LayerMask.NameToLayer("Player");
    public static LayerMask EnemyMask => 1 << LayerMask.NameToLayer("Enemy");

    public static int hMirror = Animator.StringToHash("Mirror");
    public static int hRun = Animator.StringToHash("Run");
    public static int hAttackBegin = Animator.StringToHash("AttackBegin");
    public static int hAttackParried = Animator.StringToHash("AttackParried");
    public static int hAttackComplete = Animator.StringToHash("AttackComplete");
    public static int hParryBegin = Animator.StringToHash("ParryBegin");
    public static int hParryComplete = Animator.StringToHash("ParryComplete");
    public static int hDeath = Animator.StringToHash("Death");
    public static int hRandom = Animator.StringToHash("Random");
    public static int hAttackVariations = Animator.StringToHash("AttackVariations");
    public static int hParryVariations = Animator.StringToHash("ParryVariations");
    public static int hDeathVariations = Animator.StringToHash("DeathVariations");

    protected bool CanAttackCharacter(Character target)
    {
        return CanAttack && !target.IsAttacking;
    }

    protected bool CanParryCharacter(Character target)
    {
        return CanParry && enableParry;
    }

    private Vector3 GetPlanarVectorToTarget()
    {
        return Vector3.ProjectOnPlane(targetCharacter.transform.position - transform.position, Vector3.up);
    }

    private void SetTarget(Character target)
    {
        if (target == targetCharacter) return;

        if (targetCharacter)
        {
            targetCharacter.TargettedByCount--;
            targetCharacter = null;
        }

        if (target)
        {
            targetCharacter = target;
            targetCharacter.TargettedByCount++;
        }
    }

    public virtual void OnDamage(Character instigator)
    {

        if (IsDead)
            return;

        if (IsParrying)
        {
            OnParrySuccess(instigator.attackDirection);
        }
        else
        {
            IsDead = true;
            characterController.enabled = false;
            FlipAnimationSides();
            animator.SetTrigger(hDeath);
            animator.SetFloat(hRandom, Random.Range(0, (int)animator.GetFloat(hDeathVariations)));

            OnDeath();
        }
    }

    public virtual void OnDeath()
    {

        TimeManager.Instance.PlayTimescaleEvent(deathHitStop);

        transform.DOKill();

        transform.DOShakePosition(damageShakeDuration, 0.1f, 40);
        Destroy(gameObject, destroyAfterDeathTime);
        onDeath.Invoke();
    }

    protected void GetPotentialCharacterInRange(float range, LayerMask targetLayer)
    {
        potentialTargetCount = Physics.OverlapSphereNonAlloc(transform.position, range, potentialTargetColliders, targetLayer);
    }

    protected Character GetClosestCharacterInRange(float range, LayerMask targetLayer)
    {
        GetPotentialCharacterInRange(range, targetLayer);

        return potentialTargetColliders.OrderBy(t => Vector3.Distance(t.transform.position, transform.position)).FirstOrDefault().GetComponent<Character>();
    }

    protected Character GetClosestCharacterInCone(float range, Vector3 direction, float angle, LayerMask targetLayer)
    {
        GetPotentialCharacterInRange(range, targetLayer);
        Vector3 toEnemy;
        float enemyDistance;

        float closestDistance = range;
        Collider closestCollider = null;
        Character closestCharacter = null;

        for (int i = 0; i < potentialTargetCount; i++)
        {
            toEnemy = Vector3.ProjectOnPlane(potentialTargetColliders[i].transform.position - transform.position, Vector3.up);
            enemyDistance = toEnemy.magnitude;
            if (Vector3.Angle(direction, toEnemy) < angle)
            {
                if (enemyDistance <= closestDistance)
                {
                    closestCollider = potentialTargetColliders[i];
                    closestDistance = enemyDistance;
                }
            }
        }

        if (closestCollider)
            closestCharacter = closestCollider.GetComponent<Character>();


        return closestCharacter;
    }

    protected void DoTargetedAction(Character target)
    {

        if (CanAttackCharacter(target))
        {
            AttackCharacter(target);
        }
        else if (CanParryCharacter(target))
        {
            ParryAttack(target);
        }
    }

    protected void AttackCharacter(Character target)
    {
        SetTarget(target);
        Vector3 toTarget = GetPlanarVectorToTarget();

        float distance = toTarget.magnitude;
        Vector3 direction = toTarget / distance;

        FaceDirection(direction);
        attackDirection = direction;

        BeginAttackMove(transform.position + direction * (distance - minAttackDistance));
    }


    protected void AttackDirection(Vector3 direction)
    {
        SetTarget(null);

        FaceDirection(direction);
        attackDirection = direction;

        BeginAttackMove(transform.position + direction * targetlessAttackDistance);
    }

    protected void ParryAttack(Character target)
    {
        SetTarget(target);
        IsParrying = true;
        HasSuccessfullyParried = false;

        FaceDirection(GetPlanarVectorToTarget());

        animator.SetTrigger(hParryBegin);
        animator.SetFloat(hRandom, Random.Range(0, (int)animator.GetFloat(hParryVariations)));
    }

    protected void BeginAttackMove(Vector3 position)
    {
        TimeManager.Instance.PlayTimescaleEvent(attackBeginHitStop);

        HasSuccessfullyParried = false;
        IsAttacking = true;

        FlipAnimationSides();

        animator.SetTrigger(hAttackBegin);
        animator.SetFloat(hRandom, Random.Range(0, (int)animator.GetFloat(hAttackVariations)));

        var moveTween = transform.DOMove(position, attackMoveDuration).SetEase(attackMoveEasing);
        moveTween.OnComplete(OnAttackMoveComplete);
    }

    protected void OnAttackMoveComplete()
    {
        IsAttacking = false;
        animator.ResetTrigger(hAttackBegin);

        if (targetCharacter)
        {
            if (targetCharacter.IsParrying)
            {
                IsStaggered = true;
                OnAttackParried(-attackDirection);
            }
            else
            {
                animator.SetTrigger(hAttackComplete);
            }

            targetCharacter.OnDamage(this);
            SetTarget(null);
            TimeManager.Instance.PlayTimescaleEvent(attackCompleteHitStop);
        }
        else
        {
            animator.SetTrigger(hAttackComplete);
        }

    }

    protected void OnParrySuccess(Vector3 direction)
    {
        HasSuccessfullyParried = true;
        TimeManager.Instance.PlayTimescaleEvent(parryHitStop);
        animator.ResetTrigger(hParryBegin);
        animator.SetTrigger(hParryComplete);

        var moveTween = transform.DOMove(transform.position + direction * parrySuccessPushDistance, parrySuccessMoveDuration).SetEase(parrySuccessMoveEasing);
        moveTween.OnComplete(OnParrySuccessComplete);
    }

    protected void OnParrySuccessComplete()
    {
        IsParrying = false;

    }


    protected void OnAttackParried(Vector3 direction)
    {
        animator.SetTrigger(hAttackParried);

        var moveTween = transform.DOMove(transform.position + direction * parriedAttackPushDistance, parriedAttackMoveDuration).SetEase(parriedAttackMoveEasing);
        moveTween.OnComplete(OnAttackParriedComplete);
    }

    protected void OnAttackParriedComplete()
    {
        if (IsDead) return;
        IsStaggered = false;
    }

    protected void FaceDirection(Vector3 direction)
    {
        if (direction.y != 0)
        {
            direction.y = 0;
        }
        transform.rotation = Quaternion.LookRotation(direction);
    }

    protected void Move(Vector3 direction)
    {
        if (!CanMove) return;
        FaceDirection(direction);
        characterController.Move(direction * moveSpeed * Time.deltaTime);
    }

    private void FlipAnimationSides()
    {
        animator.SetBool(hMirror, !animator.GetBool(hMirror));
    }
}
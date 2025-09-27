using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // 基本参数
    public float visionRange = 15f; // 视野范围
    public float visionAngle = 110f; // 视野角度
    public float attackRange = 2.5f; // 攻击范围
    public float patrolRange = 10f; // 巡逻范围
    public float maxHateValue = 100f; // 最大仇恨值
    public float chaseSpeed = 3.5f; // 追逐速度
    public float patrolSpeed = 1.5f; // 巡逻速度

    // 内部组件
    private NavMeshAgent agent;
    private Animator animator;

    // 仇恨值系统
    private Dictionary<Transform, float> hateDictionary = new Dictionary<Transform, float>();
    private Transform currentTarget;

    // 巡逻相关
    private Vector3 homePosition;
    private Vector3 currentPatrolTarget;
    private bool isPatrolling;
    
    // 状态枚举
    private enum AIState { Patrolling, Chasing, Attacking }
    private AIState currentState = AIState.Patrolling;

    // 攻击相关
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    public GameObject projectilePrefab; // 可选：用于远程攻击的投射物
    
    // 可视化辅助
    [Header("Visualization")]
    public Material normalMaterial;
    public Material alertMaterial;
    public Renderer bodyRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        homePosition = transform.position;
        currentPatrolTarget = GetRandomPatrolPosition();
        
        agent.speed = patrolSpeed;
        isPatrolling = true;
    }

    void Update()
    {
        // 更新仇恨值（随时间减少）
        UpdateHateValues();
        
        // 状态机
        switch (currentState)
        {
            case AIState.Patrolling:
                PatrolBehavior();
                break;
                
            case AIState.Chasing:
                ChaseBehavior();
                break;
                
            case AIState.Attacking:
                AttackBehavior();
                break;
        }
        
        UpdateAnimations();
        UpdateVisualState();
    }
    
    void UpdateHateValues()
    {
        List<Transform> keys = new List<Transform>(hateDictionary.Keys);
        
        foreach (var key in keys)
        {
            if (key != null)
            {
                // 每秒减少5点仇恨值
                hateDictionary[key] -= 5 * Time.deltaTime;
                
                // 移除仇恨值归零的目标
                if (hateDictionary[key] <= 0)
                    hateDictionary.Remove(key);
            }
            else
            {
                hateDictionary.Remove(key);
            }
        }
    }

    void PatrolBehavior()
    {
        // 每帧更新仇恨目标
        UpdateTargetByHate();
        
        // 计算目标距离
        float distanceToTarget = agent.remainingDistance;
        
        // 到达巡逻点则重新选择新目标
        if (distanceToTarget <= agent.stoppingDistance)
        {
            currentPatrolTarget = GetRandomPatrolPosition();
            agent.SetDestination(currentPatrolTarget);
        }
        
        // 更新巡逻状态
        agent.speed = patrolSpeed;
    }

    void ChaseBehavior()
    {
        if (currentTarget == null) 
        {
            SetState(AIState.Patrolling);
            return;
        }
        
        // 检查是否在攻击范围内
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            SetState(AIState.Attacking);
            agent.isStopped = true;
            return;
        }
        
        // 更新追逐速度
        agent.speed = chaseSpeed;
        
        // 继续追逐
        agent.SetDestination(currentTarget.position);
    }

    void AttackBehavior()
    {
        if (currentTarget == null) 
        {
            SetState(AIState.Patrolling);
            agent.isStopped = false;
            return;
        }
        
        // 面向目标
        FaceTarget();
        
        // 计算距离
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        
        // 如果目标离开攻击范围，则重新追逐
        if (distance > attackRange)
        {
            SetState(AIState.Chasing);
            agent.isStopped = false;
            return;
        }
        
        // 尝试攻击
        if (canAttack)
            StartCoroutine(PerformAttack());
    }
    
    IEnumerator PerformAttack()
    {
        canAttack = false;
        
        // 播放攻击动画
        // animator.SetTrigger("Attack");
        
        // 实际伤害处理（示例为近战攻击，可以修改为远程）
        DealDamage(currentTarget.GetComponent<Health>());
        
        // 冷却时间
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    void DealDamage(Health targetHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            // 攻击增加仇恨值
            AddHate(targetHealth.transform, 5f);
        }
    }
    
    // 面向当前目标
    void FaceTarget()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // 检测玩家或其他可攻击目标
        if (other.CompareTag("Player"))
        {
            Transform target = other.transform;
            
            // 检查是否在视野范围内
            if (IsInVisionRange(target) && IsInLineOfSight(target))
            {
                // 添加到仇恨列表
                AddHate(target, 10f);
                SetState(AIState.Chasing);
            }
        }
        Debug.Log("check"); 
    }
    
    // 添加仇恨值
    public void AddHate(Transform target, float amount)
    {
        if (hateDictionary.ContainsKey(target))
        {
            hateDictionary[target] += amount;
            if (hateDictionary[target] > maxHateValue)
                hateDictionary[target] = maxHateValue;
        }
        else
        {
            hateDictionary.Add(target, amount);
        }
    }
    
    // 更新仇恨目标选择
    void UpdateTargetByHate()
    {
        Transform highestHateTarget = null;
        float highestHate = 0;
        
        foreach (var pair in hateDictionary)
        {
            if (pair.Value > highestHate && pair.Key != null)
            {
                highestHate = pair.Value;
                highestHateTarget = pair.Key;
            }
        }
        
        if (highestHateTarget != null)
        {
            currentTarget = highestHateTarget;
            
            // 只要在视野内且有仇恨值就保持追逐状态
            if (IsInVisionRange(currentTarget) && IsInLineOfSight(currentTarget))
            {
                SetState(AIState.Chasing);
            }
        }
        else
        {
            currentTarget = null;
        }
    }
    
    // 检查目标是否在视野范围内
    bool IsInVisionRange(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        Debug.Log("distance: " + distance);
        return distance <= visionRange;
    }
    
    // 检查是否有直接的视线
    bool IsInLineOfSight(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        
        // 检查是否在视野角度内
        if (angle < visionAngle / 2f)
        {
            // 检查是否有障碍物阻挡视线
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1f, direction, out hit, visionRange))
            {
                if (hit.transform == target)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    // 获取随机巡逻位置
    Vector3 GetRandomPatrolPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRange;
        Vector3 randomPosition = new Vector3(
            homePosition.x + randomPoint.x, 
            homePosition.y, 
            homePosition.z + randomPoint.y);
            
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomPosition, out navHit, 2f, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        return homePosition;
    }
    
    // 更新状态
    void SetState(AIState newState)
    {
        currentState = newState;
    }
    
    // 更新动画
    void UpdateAnimations()
    {
        // animator.SetFloat("Speed", agent.velocity.magnitude);
    }
    
    // 更新视觉状态（可选）
    void UpdateVisualState()
    {
        if (currentState == AIState.Patrolling)
            bodyRenderer.material = normalMaterial;
        else
            bodyRenderer.material = alertMaterial;
    }
    
    // 可视化调试
    void OnDrawGizmosSelected()
    {
        // 绘制视野范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        // 绘制视野角度
        Vector3 leftAngle = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightAngle = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftAngle);
        Gizmos.DrawLine(transform.position, transform.position + rightAngle);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制巡逻范围
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(homePosition, patrolRange);
        }
    }
}

// 其他目标需要的健康系统（示例）
public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }
    
    void Die()
    {
        Destroy(gameObject);
    }
}
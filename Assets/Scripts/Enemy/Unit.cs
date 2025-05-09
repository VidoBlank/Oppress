using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Unit : MonoBehaviour
{
    [Header("寻路对象")]
    public PathFindTarget pathFindingTarget = null;

    [Header("偏移距离")]
    public float minOffsetDistance = 0.1f;

    public List<Stair> pathFindingStairs = new List<Stair>(); // 目标楼梯寻路

    public Direction unitDirection; // 行动方向

    public BoxCollider2D platform;

    // 缓存常用组件
    protected Animator _animator;
    protected Collider2D _collider;
    protected Transform _transform;

    public bool stairMoving = false;
    private bool dropping = false;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _collider = GetComponent<Collider2D>();
        _transform = transform;

        if (GetComponent<PathFindTarget>() == null)
            gameObject.AddComponent<PathFindTarget>();
    }

    public void StartMovingThroughStairs(Vector3 startPoint, Vector3 endPoint)
    {
        if (!stairMoving)
        {
            // 确保角色朝向正确
            if (endPoint.x > startPoint.x)
            {
                ChangeDirecition(Direction.Right);
            }
            else if (endPoint.x < startPoint.x)
            {
                ChangeDirecition(Direction.Left);
            }
            StartCoroutine(MoveThroughStairs(startPoint, endPoint));
        }
    }

    IEnumerator MoveThroughStairs(Vector3 startPoint, Vector3 endPoint)
    {
        stairMoving = true;
        float speed = GetMovingSpeed();
        float duration = Vector3.Distance(startPoint, endPoint) / speed;
        float time = 0f;

        // 保持朝向
        if (endPoint.x > startPoint.x)
            ChangeDirecition(Direction.Right);
        else if (endPoint.x < startPoint.x)
            ChangeDirecition(Direction.Left);

        while (time < duration)
        {
            if (IsCharacterKnockedDown())
            {
#if UNITY_EDITOR
                Debug.Log($"{_transform.name} 在楼梯移动时倒地，取消楼梯运动！");
#endif
                stairMoving = false;
                yield break;
            }
            _transform.position = Vector3.Lerp(startPoint, endPoint, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _transform.position = endPoint;
        stairMoving = false;
        AdjustDirectionAfterStairs();
    }

    /// <summary>
    /// 检测角色是否倒地
    /// </summary>
    private bool IsCharacterKnockedDown()
    {
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
            return pc.isKnockedDown;
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.isDead)
            return true;
        return false;
    }

    /// <summary>
    /// 获取角色或怪物的移动速度
    /// </summary>
    private float GetMovingSpeed()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
            return enemy.movingSpeed;
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
            return 2.5f;
        return 2.5f;
    }

    private void AdjustDirectionAfterStairs()
    {
        if (pathFindingTarget == null) return;

        if (pathFindingTarget.transform.position.x > _transform.position.x)
        {
            ChangeDirecition(Direction.Right);
#if UNITY_EDITOR
            Debug.Log($"{_transform.name} 楼梯移动完成，调整朝向 → 右");
#endif
        }
        else if (pathFindingTarget.transform.position.x < _transform.position.x)
        {
            ChangeDirecition(Direction.Left);
#if UNITY_EDITOR
            Debug.Log($"{_transform.name} 楼梯移动完成，调整朝向 ← 左");
#endif
        }
        _transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        if (pathFindingTarget != null && pathFindingTarget.gameObject.CompareTag("EmptyPathFindTarget"))
        {
            Destroy(pathFindingTarget.gameObject);
        }
        GameObject targetPrefab = Resources.Load<GameObject>("PathFind/Target");
        GameObject targetInstance = Instantiate(targetPrefab, CoreController.Instance.scene);
#if UNITY_EDITOR
        Debug.Log("生成目标坐标：" + targetInstance.transform.position);
#endif
        targetInstance.transform.position = targetPosition;
#if UNITY_EDITOR
        Debug.Log("修改后目标坐标：" + targetInstance.transform.position);
#endif
        SetTarget(targetInstance);
        StartCoroutine(WaitForFindingStairsPath());
    }

    public void UnsetTargetPosition()
    {
        pathFindingStairs = new List<Stair>();
        if (pathFindingTarget != null && pathFindingTarget.gameObject.CompareTag("EmptyPathFindTarget"))
            Destroy(pathFindingTarget.gameObject);
        pathFindingTarget = null;
    }

    public void SetTarget(GameObject targetObject)
    {
        if (targetObject.GetComponent<Collider2D>() != null)
        {
            if (targetObject.GetComponent<PathFindTarget>() == null)
                targetObject.AddComponent<PathFindTarget>();
            pathFindingTarget = targetObject.GetComponent<PathFindTarget>();
        }
    }

    IEnumerator WaitForFindingStairsPath()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        FindingStairsPath();
    }

    /// <summary>
    /// 简单寻路（用于目标距离较近的情况）：通过射线检测楼梯进行路径寻找
    /// </summary>
    public void FindingStairsPath()
    {
        if (pathFindingTarget == null || stairMoving)
            return;
        if (Vector2.Distance(pathFindingTarget.transform.position, _transform.position) < minOffsetDistance)
            return;

        LayerMask targetLayerMask = (1 << 10) | (1 << 12) | (1 << 7);
        Vector2 pos = new Vector2(_transform.position.x + _collider.offset.x, _transform.position.y + _collider.offset.y);
        Ray2D rightRay = new Ray2D(pos, Vector3.right);
#if UNITY_EDITOR
        Debug.DrawRay(rightRay.origin, rightRay.direction * 50f, Color.red);
#endif
        RaycastHit2D[] hits = Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<PathFindTarget>() == pathFindingTarget && pathFindingTarget.transform.position.x > _transform.position.x)
            {
                ChangeDirecition(Direction.Right);
                return;
            }
        }
        Ray2D leftRay = new Ray2D(pos, Vector3.left);
#if UNITY_EDITOR
        Debug.DrawRay(leftRay.origin, leftRay.direction * 50f, Color.red);
#endif
        hits = Physics2D.RaycastAll(leftRay.origin, leftRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<PathFindTarget>() == pathFindingTarget && pathFindingTarget.transform.position.x < _transform.position.x)
            {
                ChangeDirecition(Direction.Left);
                return;
            }
        }

        // 遍历楼梯节点
        List<StairTrigger> untraversed = new List<StairTrigger>();
        targetLayerMask = (1 << 10) | (1 << 12);
        hits = Physics2D.RaycastAll(leftRay.origin, leftRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("StairTrigger"))
            {
                StairTrigger st = hit.collider.GetComponent<StairTrigger>();
                if (st != null)
                {
                    if (untraversed.Count == 0)
                        untraversed.Add(st);
                    else
                    {
                        bool added = false;
                        for (int i = 0; i < untraversed.Count; i++)
                        {
                            if (Vector2.Distance(st.transform.position, _transform.position) < Vector2.Distance(untraversed[i].transform.position, _transform.position))
                            {
                                untraversed.Insert(i, st);
                                added = true;
                                break;
                            }
                        }
                        if (!added)
                            untraversed.Add(st);
                    }
                }
            }
        }
        hits = Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("StairTrigger"))
            {
                StairTrigger st = hit.collider.GetComponent<StairTrigger>();
                if (st != null)
                {
                    if (untraversed.Count == 0)
                        untraversed.Add(st);
                    else
                    {
                        bool added = false;
                        for (int i = 0; i < untraversed.Count; i++)
                        {
                            if (Vector2.Distance(st.transform.position, _transform.position) < Vector2.Distance(untraversed[i].transform.position, _transform.position))
                            {
                                untraversed.Insert(i, st);
                                added = true;
                                break;
                            }
                        }
                        if (!added)
                            untraversed.Add(st);
                    }
                }
            }
        }
        bool found = false;
        List<Stair> pathStairs = new List<Stair>();
        if (untraversed.Count > 0)
        {
            for (int i = 0; i < untraversed.Count; i++)
            {
                if (found) break;
                if (!pathStairs.Contains(untraversed[i].stair))
                    pathStairs.Add(untraversed[i].stair);
                if (pathFindingTarget != null && untraversed[i].GetAnotherStairTrigger().ViewCheckPath(new List<Stair>(), pathStairs, pathFindingTarget))
                {
#if UNITY_EDITOR
                    Debug.Log("找到路径");
#endif
                    pathFindingStairs = pathStairs;
                    found = true;
                }
            }
        }
        if (found)
        {
            MovingToTarget();
        }
        else
        {
            if (pathFindingTarget.transform.position.x < _transform.position.x)
                ChangeDirecition(Direction.Left);
            else
                ChangeDirecition(Direction.Right);
#if UNITY_EDITOR
            Debug.Log("未找到对应楼梯路径");
#endif
        }
    }

    public virtual void MovingToTarget()
    {
        Vector2 pos = new Vector2(_transform.position.x + _collider.offset.x, _transform.position.y + _collider.offset.y);
        Ray2D rightRay = new Ray2D(pos, Vector3.right);
        Ray2D leftRay = new Ray2D(pos, Vector3.left);
        LayerMask targetLayerMask = (1 << 10) | (1 << 12) | (1 << 7);

        RaycastHit2D[] hits = Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<PathFindTarget>() == pathFindingTarget && pathFindingTarget.transform.position.x > _transform.position.x)
            {
#if UNITY_EDITOR
                Debug.Log("检测到目标，向右走");
#endif
                ChangeDirecition(Direction.Right);
                pathFindingStairs = new List<Stair>();
                return;
            }
        }

        hits = Physics2D.RaycastAll(leftRay.origin, leftRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<PathFindTarget>() == pathFindingTarget && pathFindingTarget.transform.position.x < _transform.position.x)
            {
#if UNITY_EDITOR
                Debug.Log("检测到目标，向左走");
#endif
                ChangeDirecition(Direction.Left);
                pathFindingStairs = new List<Stair>();
                return;
            }
        }

        hits = Physics2D.RaycastAll(leftRay.origin, leftRay.direction, 50f, targetLayerMask);
        if (pathFindingStairs == null || pathFindingStairs.Count == 0)
            return;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("StairTrigger") && hit.collider.GetComponent<StairTrigger>().stair == pathFindingStairs[0])
            {
#if UNITY_EDITOR
                Debug.Log("检测到楼梯，向左走");
#endif
                ChangeDirecition(Direction.Left);
                return;
            }
        }

        hits = Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("StairTrigger") && hit.collider.GetComponent<StairTrigger>().stair == pathFindingStairs[0])
            {
#if UNITY_EDITOR
                Debug.Log("检测到楼梯，向右走");
#endif
                ChangeDirecition(Direction.Right);
                return;
            }
        }
    }

    public virtual void ChangeDirecition(Direction direction)
    {
        unitDirection = direction;
    }

    public void GetPlatform(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BoxCollider2D>() != null && collision.gameObject.CompareTag("Ground"))
            platform = collision.gameObject.GetComponent<BoxCollider2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BoxCollider2D>() != null && collision.gameObject.CompareTag("Ground") && dropping)
            StartCoroutine(PlatformIgnoreCoroutine(collision.collider));
    }

    public void ResetPlatform(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BoxCollider2D>() != null && platform == collision.gameObject.GetComponent<BoxCollider2D>())
            platform = null;
    }

    public void IgnoreStair(StairTrigger stairTrigger)
    {
        Physics2D.IgnoreCollision(_collider, stairTrigger.stair.GetComponent<Collider2D>());
    }

    public void StopIgnoreStrair(StairTrigger stairTrigger)
    {
        Physics2D.IgnoreCollision(_collider, stairTrigger.stair.GetComponent<Collider2D>(), false);
    }

    IEnumerator PlatformIgnoreCoroutine(Collider2D collider2D)
    {
        if (collider2D != null)
            Physics2D.IgnoreCollision(_collider, collider2D);
        yield return new WaitUntil(() => !dropping);
        Physics2D.IgnoreCollision(_collider, collider2D, false);
    }

    public void SetDropping(float dropTime)
    {
        float time = dropTime;
        if (GetComponent<Enemy>() != null)
            time = dropTime * (5 / GetComponent<Enemy>().movingSpeed);
        else if (GetComponent<PlayerController>() != null)
            time = dropTime * (5 / GetComponent<PlayerController>().attribute.movingSpeed);
        dropping = true;
        StartCoroutine(DroppingCoroutine(time));
    }

    IEnumerator DroppingCoroutine(float dropTime)
    {
        yield return new WaitForSeconds(dropTime);
        dropping = false;
    }

    protected void DrawView()
    {
        Vector2 pos = new Vector2(_transform.position.x + _collider.offset.x, _transform.position.y + _collider.offset.y);
        Ray2D rightRay = new Ray2D(pos, Vector3.right);
        Debug.DrawRay(rightRay.origin, rightRay.direction * 50f, Color.red);
        Ray2D leftRay = new Ray2D(pos, Vector3.left);
        Debug.DrawRay(leftRay.origin, leftRay.direction * 50f, Color.red);
    }
    public virtual void MouseCheck() { }

}

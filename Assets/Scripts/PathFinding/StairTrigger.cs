using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public Stair stair;
    public List<Unit> exitList = new List<Unit>();

    public void SetStair(Stair targetStair)
    {
        stair = targetStair;
    }

    public StairTrigger GetAnotherStairTrigger()
    {
        return stair.BelowTrigger == this ? stair.AboveTrigger : stair.BelowTrigger;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Unit unit = collision.GetComponent<Unit>();
        if (unit == null) return;
        if (stair == null || unit.pathFindingTarget == null) return;

        Vector3 unitPos = unit.transform.position;
        Vector3 targetPos = unit.pathFindingTarget.transform.position;

        // 当目标在上层时，只允许使用楼梯的下端进入（即：只有在 stair.BelowTrigger 时响应）
        if (targetPos.y > unitPos.y && this != stair.BelowTrigger)
        {
            Debug.Log($"{unit.gameObject.name} 目标在上层，但触发器 {gameObject.name} 不是楼梯下端，忽略此触发器！");
            return;
        }

        // 当目标在下层时，只允许使用楼梯的上端进入（即：只有在 stair.AboveTrigger 时响应）
        if (targetPos.y < unitPos.y && this != stair.AboveTrigger)
        {
            Debug.Log($"{unit.gameObject.name} 目标在下层，但触发器 {gameObject.name} 不是楼梯上端，忽略此触发器！");
            return;
        }

        // 计算角色当前 y 坐标与目标 y 坐标的差距
        float yDifference = Mathf.Abs(targetPos.y - unitPos.y);
        // 如果高度差太小，则认为在同一层，不进入楼梯
        if (yDifference < 1.5f)
        {
            Debug.Log($"🚫 {unit.gameObject.name} 目标 {unit.pathFindingTarget.name} 在同一层，忽略楼梯！");
            return;
        }

        // 根据当前触发器类型决定走上楼还是下楼
        if (stair.BelowTrigger == this) // 只有此端才允许上楼
        {
            Debug.Log($"{unit.gameObject.name} 触发了 {stair.gameObject.name} 的下端，开始上楼");
            unit.StartMovingThroughStairs(stair.GetStartWaypoint(), stair.GetEndWaypoint());
        }
        else if (stair.AboveTrigger == this) // 只有此端才允许下楼
        {
            Debug.Log($"{unit.gameObject.name} 触发了 {stair.gameObject.name} 的上端，开始下楼");
            unit.StartMovingThroughStairs(stair.GetEndWaypoint(), stair.GetStartWaypoint());
        }
    }


    public bool ViewCheckPath(List<Stair> traversedStairs, List<Stair> pathStairs, PathFindTarget pathFindingTarget)
    {
        Vector3 diretion = Quaternion.Euler(0f, 0f, 0f) * Vector3.right;
        Vector2 pointPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().offset.x, transform.position.y + GetComponent<Collider2D>().offset.y);
        Ray2D rightRay = new Ray2D(pointPosition, diretion);


        LayerMask targetLayerMask = (1 << 10) | (1 << 12) | (1 << 7); //对楼梯与目标进行检测

        Debug.Log(gameObject.name + "进行楼梯层检测，其射线起始位置为" + rightRay.origin);

        foreach (RaycastHit2D hit2D in Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask))
        {
            Debug.Log("射线物体名字" + hit2D.collider.name + "射线物体位置" + hit2D.transform.position + "相交点的坐标：" + hit2D.point);
        }

        foreach (RaycastHit2D hit2D in Physics2D.RaycastAll(rightRay.origin, rightRay.direction, 50f, targetLayerMask))
        {
            if (hit2D.collider.GetComponent<PathFindTarget>() && hit2D.collider.GetComponent<PathFindTarget>() == pathFindingTarget)
            {
                Debug.Log("寻找到目标对象" + pathFindingTarget.gameObject.name);
                return true;
            }
            if (hit2D.collider.CompareTag("StairTrigger") && !traversedStairs.Contains(hit2D.collider.GetComponent<StairTrigger>().stair) && !pathStairs.Contains(hit2D.collider.GetComponent<StairTrigger>().stair) && hit2D.collider.GetComponent<StairTrigger>() != this)//若为未遍历过的楼梯节点则进行遍历
            {
                pathStairs.Add(hit2D.collider.GetComponent<StairTrigger>().stair); //添加至路径节点
                if (hit2D.collider.GetComponent<StairTrigger>().GetAnotherStairTrigger().ViewCheckPath(traversedStairs, pathStairs, pathFindingTarget)) //递归遍历
                    return true;
            }
        }

        diretion = Quaternion.Euler(0f, 0f, 0f) * Vector3.left;
        pointPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().offset.x, transform.position.y + GetComponent<Collider2D>().offset.y);
        Ray2D leftRay = new Ray2D(pointPosition, diretion);

        foreach (RaycastHit2D hit2D in Physics2D.RaycastAll(leftRay.origin, leftRay.direction, 50f, targetLayerMask))
        {
            if (hit2D.collider.GetComponent<PathFindTarget>() && hit2D.collider.GetComponent<PathFindTarget>() == pathFindingTarget)
            {
                Debug.Log("寻找到目标对象" + pathFindingTarget.gameObject.name);
                return true;
            }
            if (hit2D.collider.CompareTag("StairTrigger") && !traversedStairs.Contains(hit2D.collider.GetComponent<StairTrigger>().stair) && !pathStairs.Contains(hit2D.collider.GetComponent<StairTrigger>().stair) && hit2D.collider.GetComponent<StairTrigger>() != this)//若为未遍历过的楼梯节点则进行遍历
            {
                pathStairs.Add(hit2D.collider.GetComponent<StairTrigger>().stair); //添加至路径节点
                if (hit2D.collider.GetComponent<StairTrigger>().GetAnotherStairTrigger().ViewCheckPath(traversedStairs, pathStairs, pathFindingTarget)) //递归遍历
                    return true;
            }
        }

        //未找到节点，将路径节点删除，继续遍历新节点
        pathStairs.Remove(stair);
        traversedStairs.Add(stair);

        return false;
    }
}



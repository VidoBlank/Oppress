using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum LevelSize { Small, Middle, Big }

public class LevelGenerator : MonoBehaviour
{
    [Tooltip("是否启用双向起点")]
    public bool enableStartRoomBothSides = false;

    [Header("房间预制体设置")]
    public GameObject startRoomPrefab;
    public GameObject[] commonRoomPrefabs;
    public GameObject[] specialRoomPrefabs;
    public GameObject[] stairRoomPrefabs;
    public GameObject[] monsterRoomPrefabs;

    [Header("关卡参数")]
    public LevelSize levelSize = LevelSize.Small;

    [Header("分支怪物房设置")]
    [Range(0f, 1f)] public float branchProbability = 0.3f;
    public int maxBranchPerFloor = 2;
    public int maxMonsterRoomsTotal = 8;

    [Header("房间数量参数")]
    public int minRoomsPerFloor = 2;
    public int maxRoomsPerFloor = 6;
    [Range(0f, 1f)] public float specialRoomProbability = 0.2f;

    [Header("楼梯房控制")]
    [Tooltip("每层至多生成几个楼梯房间")]
    public int maxStairsPerFloor = 1;
    [Header("楼梯额外生成概率")]
    [Tooltip("第二个及以后的楼梯出现概率")]
    [Range(0f, 1f)] public float additionalStairProbability = 0.2f;

    [Header("房间墙壁厚度（共享厚度）")]
    public float sharedWallThickness = 0.5f;

    private int columns;
    private bool[,] occupancyGrid;
    private RoomModule[,] roomMap;
    private List<GameObject> generatedRooms = new List<GameObject>();

    public void GenerateLevel()
    {
        ClearLevel();

        // 1. 计算总楼层与房间数
        int totalFloors, totalRoomsTarget;
        switch (levelSize)
        {
            case LevelSize.Small:
                totalFloors = Random.Range(1, 3);
                totalRoomsTarget = Random.Range(4, 9);
                break;
            case LevelSize.Middle:
                totalFloors = Random.Range(2, 5);
                totalRoomsTarget = Random.Range(12, 17);
                break;
            default:
                totalFloors = Random.Range(4, 7);
                totalRoomsTarget = Random.Range(18, 31);
                break;
        }
        totalRoomsTarget = Mathf.Min(totalRoomsTarget, totalFloors * maxRoomsPerFloor);

        // 2. 分配每层房间数
        int remaining = totalRoomsTarget;
        int[] roomsPerFloor = new int[totalFloors];
        for (int f = 0; f < totalFloors; f++)
        {
            int floorsLeft = totalFloors - f;
            int maxThis = (f == totalFloors - 1)
                ? remaining
                : remaining - (floorsLeft - 1) * minRoomsPerFloor;
            roomsPerFloor[f] = Random.Range(minRoomsPerFloor,
                Mathf.Min(maxRoomsPerFloor, maxThis) + 1);
            remaining -= roomsPerFloor[f];
        }
        if (totalFloors > 1)
            roomsPerFloor[0] = Mathf.Max(2, roomsPerFloor[0]);

        // 3. 初始化网格
        columns = maxRoomsPerFloor;
        occupancyGrid = new bool[totalFloors, columns];
        roomMap = new RoomModule[totalFloors, columns];

        // 每层可存多个楼梯列
        List<int>[] stairColsList = new List<int>[totalFloors];
        for (int f = 0; f < totalFloors; f++)
            stairColsList[f] = new List<int>();

        // 4. 填充格子
        int startCol = enableStartRoomBothSides && Random.value < 0.5f
            ? columns - 1
            : 0;
        occupancyGrid[0, startCol] = true;

        for (int f = 0; f < totalFloors; f++)
        {
            // 4.1 生成本层连续房段
            int len = roomsPerFloor[f];
            int segStart = Random.Range(0, columns - len + 1);
            for (int c = segStart; c < segStart + len; c++)
                occupancyGrid[f, c] = true;

            // 4.2 非顶层：主楼梯 + 额外概率递减（互不相邻，且不与上一层楼梯 origin 重合）
            if (f < totalFloors - 1)
            {
                // 候选列表：本层这一段所有格子
                List<int> candidates = new List<int>();
                for (int c = segStart; c < segStart + len; c++)
                    candidates.Add(c);

                // —— 新增：剔除上一层楼梯 origin，避免垂直相邻两个 origin —— 
                if (f > 0)
                {
                    foreach (int prev in stairColsList[f - 1])
                        candidates.Remove(prev);
                }

                // ① 随机一个主楼梯
                int mainStair = candidates[Random.Range(0, candidates.Count)];
                stairColsList[f].Add(mainStair);
                occupancyGrid[f, mainStair] = true;
                occupancyGrid[f + 1, mainStair] = true;

                // 从候选中移除主楼梯及其左右相邻格
                candidates.Remove(mainStair);
                candidates.Remove(mainStair - 1);
                candidates.Remove(mainStair + 1);

                // ② 额外楼梯，概率递减
                float p = additionalStairProbability;
                for (int i = 1; i < maxStairsPerFloor && candidates.Count > 0; i++)
                {
                    if (Random.value < p)
                    {
                        int idx = Random.Range(0, candidates.Count);
                        int stairCol = candidates[idx];
                        stairColsList[f].Add(stairCol);
                        occupancyGrid[f, stairCol] = true;
                        occupancyGrid[f + 1, stairCol] = true;

                        // 同样移除它及相邻，防止水平相邻
                        candidates.RemoveAt(idx);
                        candidates.Remove(stairCol - 1);
                        candidates.Remove(stairCol + 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }


        // 5. 按格子实例化房间
        for (int f = 0; f < totalFloors; f++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (!occupancyGrid[f, c]) continue;
                bool isStart = f == 0 && c == startCol;
                bool isStair = stairColsList[f].Contains(c);

                GameObject prefab = isStart
                    ? startRoomPrefab
                    : isStair
                        ? stairRoomPrefabs[
                            Random.Range(0, stairRoomPrefabs.Length)]
                        : (specialRoomPrefabs.Length > 0 &&
                           Random.value < specialRoomProbability)
                            ? specialRoomPrefabs[
                                Random.Range(0, specialRoomPrefabs.Length)]
                            : commonRoomPrefabs[
                                Random.Range(0, commonRoomPrefabs.Length)];

                GameObject go = Instantiate(prefab,
                    Vector3.zero, Quaternion.identity, transform);
                RoomModule rm = go.GetComponent<RoomModule>();
                rm.floorNumber = f;
                rm.isStair = isStair;

                float gw = rm.gridWidth;
                float gh = rm.gridHeight;
                float originX = (c - (columns - 1) * 0.5f) * gw;
                float originY = f * (gh - sharedWallThickness);

                Transform cp = rm.GetConnectionPoint(ConnectionType.Left);
                Vector3 target = new Vector3(originX, originY + gh * 0.5f, 0f);
                go.transform.position += target - cp.position;

                roomMap[f, c] = rm;
                generatedRooms.Add(go);
            }
        }

        // 6-a. 水平方向打通墙壁
        for (int f = 0; f < totalFloors; f++)
            for (int c = 0; c < columns - 1; c++)
            {
                var a = roomMap[f, c];
                var b = roomMap[f, c + 1];
                if (a != null && b != null)
                {
                    a.DisableWall(ConnectionType.Right);
                    b.DisableWall(ConnectionType.Left);
                }
            }

        // 6-b. 垂直方向：下层楼梯打开上层底墙，否则打开下层顶墙
        for (int f = 0; f < totalFloors - 1; f++)
            for (int c = 0; c < columns; c++)
            {
                var lower = roomMap[f, c];
                var upper = roomMap[f + 1, c];
                if (lower == null || upper == null) continue;
                if (lower.floorNumber + 1 != upper.floorNumber) continue;

                if (lower.isStair)
                    upper.DisableWall(ConnectionType.Bottom);
                else
                    lower.DisableWall(ConnectionType.Top);
            }

        // 7. 分支怪物房（保持原逻辑）
        int placed = 0;
        for (int f = 1; f < totalFloors; f++)
        {
            var list = new List<(int c, RoomModule rm)>();
            for (int c = 0; c < columns; c++)
                if (roomMap[f, c] != null)
                    list.Add((c, roomMap[f, c]));
            if (list.Count < 2) continue;

            int count = 0;
            foreach (var (c, baseRm) in new[] { list[0], list[^1] })
            {
                if (count >= maxBranchPerFloor ||
                    placed >= maxMonsterRoomsTotal)
                    break;
                if (Random.value > branchProbability) continue;

                ConnectionType dir =
                    c == list[0].c ? ConnectionType.Left : ConnectionType.Right;
                if (!baseRm.IsWallIntact(dir)) continue;

                var mp = monsterRoomPrefabs[
                    Random.Range(0, monsterRoomPrefabs.Length)];
                var mgo = Instantiate(mp, Vector3.zero,
                    Quaternion.identity, transform);
                var mm = mgo.GetComponent<RoomModule>();
                mm.floorNumber = f;

                Transform cp2 = mm.GetConnectionPoint(Opposite(dir));
                Transform bp = baseRm.GetConnectionPoint(dir);
                mgo.transform.position =
                    bp.position - (cp2.position - mgo.transform.position);

                baseRm.DisableWall(dir);
                
                generatedRooms.Add(mgo);
                placed++;
                count++;
            }
        }
    }

    public void ClearLevel()
    {
        foreach (var r in generatedRooms)
            if (r) DestroyImmediate(r);
        generatedRooms.Clear();
    }

    private ConnectionType Opposite(ConnectionType d) => d switch
    {
        ConnectionType.Left => ConnectionType.Right,
        ConnectionType.Right => ConnectionType.Left,
        ConnectionType.Top => ConnectionType.Bottom,
        ConnectionType.Bottom => ConnectionType.Top,
        _ => d
    };
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Main : MonoBehaviour
{
    public static Main Instance { get; private set; }
    LevelNode[] levelNodes;

    public Transform LevelNodeContainer;
    public Transform LevelLineContainer;

    public Button enterLevelButton;
    public Button passLevelButton;
    public Button nextStageButton;

    // 当前选中的节点
    private LevelNode currentSelectedNode;

    // 新增：用于保存从关卡场景返回时的目标节点 UID，以及关卡完成标记
    public static int currentLevelUid = -1;
    public static bool levelCompleted = false;

    void Start()
    {
        levelNodes = LevelNodeContainer.GetComponentsInChildren<LevelNode>();
        GameData.Instance.LevelPath.ToList();
        if (GameData.Instance.LevelPath.Count == 0)
        {
            ResetAllNodes();
        }

        for (var i = 0; i < levelNodes.Length; i++)
        {
            levelNodes[i].OnClick += OnNodeClick;
            levelNodes[i].LoadState();
        }

        UpdatePathView();

        if (enterLevelButton != null)
        {
            enterLevelButton.onClick.AddListener(OnEnterLevelClicked);
            enterLevelButton.gameObject.SetActive(false);
        }
        if (passLevelButton != null)
        {
            passLevelButton.onClick.AddListener(OnPassLevelClicked);
            passLevelButton.gameObject.SetActive(false);
        }
        if (nextStageButton != null)
        {
            nextStageButton.onClick.AddListener(OnPassLevelClicked);
            nextStageButton.gameObject.SetActive(false);
        }

        // 新增：如果关卡完成标记存在，则更新目标节点为通过
        if (levelCompleted && currentLevelUid >= 0)
        {
            Debug.Log($"Map: 检测到关卡完成，节点 {currentLevelUid} 将被标记为通过");
            MarkNodeAsPassed(currentLevelUid);
            // 重置静态标记
            levelCompleted = false;
            currentLevelUid = -1;
        }
    }

    public void ResetAllNodes()
    {
        foreach (var node in levelNodes)
        {
            node.InPath = false;
            node.isSelectable = false;
            node.IsPass = false;
            node.SaveState();
        }
    }

    private void OnNodeClick(LevelNode node)
    {
        var paths = GameData.Instance.LevelPath;

        if (node.InPath && paths.Count > 0)
        {
            var lastPathNodeUid = paths[paths.Count - 1].levelUid;
            var lastPathNode = GetNode(lastPathNodeUid);
            if (node == lastPathNode)
            {
                GameData.Instance.PopLevelPath();
                UpdatePathView();
                enterLevelButton?.gameObject.SetActive(false);
                passLevelButton?.gameObject.SetActive(false);
                nextStageButton?.gameObject.SetActive(false);
                currentSelectedNode = null;
                return;
            }
        }

        if (paths.Count > 0)
        {
            var lastPathNodeUid = paths[paths.Count - 1].levelUid;
            var lastLevelData = GameData.Instance.GetLevelData(lastPathNodeUid);
            if (!lastLevelData.IsPassed)
            {
                Debug.Log("前一个节点尚未通过，无法点击新的节点！");
                return;
            }
        }

        bool canPathTo;
        if (paths.Count > 0)
        {
            var lastPathNodeUid = paths[paths.Count - 1].levelUid;
            var lastPathNode = GetNode(lastPathNodeUid);
            canPathTo = lastPathNode.CanPathLinkTo(node);
        }
        else
        {
            canPathTo = node == GetFirstNode();
        }

        if (canPathTo)
        {
            GameData.Instance.PushLevelPath(node.uid);
            UpdatePathView();
        }

        currentSelectedNode = node;
        if (node.isEndPoint)
        {
            enterLevelButton?.gameObject.SetActive(false);
            passLevelButton?.gameObject.SetActive(false);
            nextStageButton?.gameObject.SetActive(true);
        }
        else
        {
            enterLevelButton?.gameObject.SetActive(true);
            passLevelButton?.gameObject.SetActive(true);
            nextStageButton?.gameObject.SetActive(false);
        }
    }

    private void OnEnterLevelClicked()
    {
        if (PlayerTeamManager.Instance.selectedUnitPrefabs == null ||
            PlayerTeamManager.Instance.selectedUnitPrefabs.Count == 0)
        {
            Debug.LogWarning("未选中干员，无法加入关卡！");
            return;
        }

        if (currentSelectedNode != null && !string.IsNullOrEmpty(currentSelectedNode.sceneName))
        {
            // 记录当前选中节点的 UID（后续用于关卡完成时更新）
            currentLevelUid = currentSelectedNode.uid;
            Debug.Log($"记录当前节点 UID: {currentLevelUid}, 进入场景: {currentSelectedNode.sceneName}");
            LevelNode.currentRewardRP = currentSelectedNode.rewardRP;
            SceneManager.LoadScene(currentSelectedNode.sceneName);
        }
        else
        {
            Debug.LogWarning("当前节点未设置 sceneName，或不存在选中节点。");
        }
    }

    // 原有的 OnPassLevelClicked 用于 Map 场景中手动标记节点通过
    public void OnPassLevelClicked()
    {
        if (currentSelectedNode != null)
        {
            var levelData = GameData.Instance.GetLevelData(currentSelectedNode.uid);
            levelData.IsPassed = true;
            Debug.Log($"节点 {currentSelectedNode.uid} 已标记为通过！");
            currentSelectedNode.IsPass = true;
            currentSelectedNode.SaveState();
            currentSelectedNode.ResetColor();
            UpdatePathView();

            enterLevelButton?.gameObject.SetActive(false);
            passLevelButton?.gameObject.SetActive(false);
            nextStageButton?.gameObject.SetActive(false);
        }
    }

    // 新增：直接根据节点 UID将该节点标记为通过
    public void MarkNodeAsPassed(int uid)
    {
        LevelNode node = GetNode(uid);
        if (node != null)
        {
            var levelData = GameData.Instance.GetLevelData(uid);
            levelData.IsPassed = true;
            Debug.Log($"Map: 节点 {uid} 已标记为通过（MarkNodeAsPassed）");
            node.IsPass = true;
            node.SaveState();
            node.ResetColor();
            UpdatePathView();

            enterLevelButton?.gameObject.SetActive(false);
            passLevelButton?.gameObject.SetActive(false);
            nextStageButton?.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Map: 未找到 UID 为 {uid} 的节点！");
        }
    }

    void UpdatePathView()
    {
        foreach (var node in levelNodes)
        {
            node.InPath = false;
            node.isSelectable = false;
        }

        var paths = GameData.Instance.LevelPath;
        foreach (var pathNode in paths)
        {
            var node = GetNode(pathNode.levelUid);
            if (pathNode.IsPassed || pathNode == paths[paths.Count - 1])
            {
                node.InPath = true;
                node.isSelectable = true;
            }
        }

        if (paths.Count == 0)
        {
            if (levelNodes.Length > 0)
                levelNodes[0].isSelectable = true;
        }
        else
        {
            var lastNode = GetNode(paths[paths.Count - 1].levelUid);
            if (lastNode != null && lastNode.IsPass)
            {
                foreach (var linkedNode in lastNode.Links)
                {
                    linkedNode.isSelectable = true;
                }
            }
        }

        foreach (var node in levelNodes)
        {
            node.ResetColor();
        }
    }

    public LevelNode GetNode(int uid)
    {
        return levelNodes[uid];
    }

    LevelNode GetFirstNode()
    {
        return levelNodes[0];
    }

    public void Gen() { }

    void Update() { }
}



#if UNITY_EDITOR
[CustomEditor(typeof(Main))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Main myScript = (Main)target;
        DrawDefaultInspector();
        if (GUILayout.Button("UpdateLinks"))
        {
            var levelNodes = myScript.LevelNodeContainer.GetComponentsInChildren<LevelNode>();
            var LineTemplate = myScript.LevelLineContainer.GetChild(0).gameObject;

            for (int i = 0; i < levelNodes.Length; i++)
            {
                LevelNode node = levelNodes[i];
                if (node != null)
                {
                    node.SetUid(i);
                }
            }

            for (int i = 0; i < levelNodes.Length; i++)
            {
                LevelNode node = levelNodes[i];
                if (node != null)
                {
                    GenNodeLine(node, LineTemplate, myScript.LevelLineContainer);
                }
            }
            EditorUtility.SetDirty(myScript);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void GenNodeLine(LevelNode node, GameObject LineTemplate, Transform LevelLineContainer)
    {
        for (var i = 0; i < node.LinkLines.Count; i++)
        {
            var line = node.LinkLines[i];
            if (line != null)
            {
                GameObject.DestroyImmediate(line.gameObject);
            }
        }
        node.LinkLines.Clear();
        for (var i = 0; i < node.Links.Count; i++)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(LineTemplate);
            // 使用 PrefabUtility.InstantiatePrefab 来保持预制体的实例化
            var newLine = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath),
                LevelLineContainer
            );

            newLine.SetActive(true);
            var line = newLine.GetComponent<LevelLinkLine>();
            line.Init(node, node.Links[node.LinkLines.Count]);
            node.LinkLines.Add(line);
            line.UpdateLine();
        }
        EditorUtility.SetDirty(node);
    }
}
#endif

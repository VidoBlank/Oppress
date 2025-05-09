using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class LevelNode : MonoBehaviour
{
    public TextMeshPro Text;
    public List<LevelNode> Links;
    public List<LevelLinkLine> LinkLines;
    public GameObject cube;
    public string sceneName;

    public int uid;  // 唯一编号
    public Action<LevelNode> OnClick;

    // 节点状态
    public bool InPath;
    public bool IsPass;
    public bool isSelectable = true; // 如果为 false，则表示该节点不可选择

    // 新增：是否为终点
    public bool isEndPoint = false;

    // 定义各状态下的颜色
    private Color normalColor = new Color(0.7f, 0.7f, 0.7f);           // 普通节点：灰色
    private Color unselectableColor = new Color(0.4f, 0.4f, 0.4f);       // 不可选择：更深灰
    private Color hoverColor = Color.white;                              // 鼠标悬停：白色
    private Color selectedColor = Color.yellow;                          // 选中节点：黄色
    private Color passedColor = new Color(0.1f, 0.1f, 0.1f);             // 已通过：更深一级灰

    [Header("奖励资源")]
    public int rewardRP = 10; // 自己在Inspector调
                              // 静态记录当前关卡奖励RP
    public static int currentRewardRP = 0;

    void Start()
    {
        Text.text = uid.ToString();
        ResetColor();
    }

    public bool CanPathLinkTo(LevelNode target)
    {
        foreach (var linkNode in Links)
        {
            if (linkNode == target)
            {
                return true;
            }
        }
        return false;
    }

    public void SetUid(int id)
    {
        this.uid = id;
        Text.text = id.ToString();
    }

    public void SetColor(Color c)
    {
        if (cube != null)
        {
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
#if UNITY_EDITOR
                // 编辑器状态下防止材质泄露
                if (!Application.isPlaying)
                    cubeRenderer.sharedMaterial.color = c;
                else
                    cubeRenderer.material.color = c;
#else
            cubeRenderer.material.color = c;
#endif
            }
        }
    }


    void OnMouseDown()
    {
        // 如果该节点不可选择或已通过，则点击无效
        if (!isSelectable || IsPass)
            return;
        OnClick?.Invoke(this);
    }

    void OnMouseEnter()
    {
        if (isSelectable && !InPath && !IsPass)
        {
            SetColor(hoverColor);
        }
    }

    void OnMouseExit()
    {
        if (isSelectable && !InPath && !IsPass)
        {
            ResetColor();
        }
    }

    /// <summary>
    /// 根据节点状态重置颜色，并更新连线颜色
    /// </summary>
    internal void ResetColor()
    {
        if (!isSelectable)
        {
            SetColor(unselectableColor);
        }
        else if (IsPass)
        {
            SetColor(passedColor);
        }
        else if (InPath)
        {
            SetColor(selectedColor);
        }
        else
        {
            SetColor(normalColor);
        }

        // 更新所有连线的颜色：
        // 如果两端节点都已通过，或者都处于路径中，则连线显示黄色，否则显示普通灰色
        foreach (var line in LinkLines)
        {
            if (line != null)
            {
                if (line.from != null && line.to != null)
                {
                    if ((line.from.IsPass && line.to.IsPass) ||
                        (line.from.InPath && line.to.InPath))
                    {
                        line.SetColor(selectedColor);
                    }
                    else
                    {
                        line.SetColor(normalColor);
                    }
                }
                else
                {
                    line.SetColor(normalColor);
                }
            }
        }
    }

    // 储存节点状态的方法
    public void LoadState()
    {
        var levelData = GameData.Instance.GetLevelData(uid);
        IsPass = levelData.IsPass;
        isSelectable = levelData.IsSelectable;
    }

    public void SaveState()
    {
        var levelData = GameData.Instance.GetLevelData(uid);
        levelData.IsPass = IsPass;
        levelData.InPath = InPath;
        levelData.IsSelectable = isSelectable;
    }
}

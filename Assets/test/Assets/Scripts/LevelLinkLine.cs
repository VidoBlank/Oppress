using System;
using UnityEngine;

public class LevelLinkLine : MonoBehaviour
{
    public LevelNode from;

    public LevelNode to;

    public GameObject connectedCube;


    internal void Init(LevelNode a, LevelNode b)
    {
        from = a;
        to = b;
        gameObject.name = $"Link {a.uid} > {b.uid}";
    }

    public void UpdateLine()
    {
        if (from == null || to == null) return;
        Vector3 midpoint = (from.transform.position + to.transform.position) / 2;
        connectedCube.transform.position = midpoint;
        Vector3 direction = to.transform.position - from.transform.position;
        float distance = direction.magnitude;
        connectedCube.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        connectedCube.transform.localScale = new Vector3(0.1f, 0.1f, distance);
    }

    public void SetColor(Color c)
    {
        Renderer cubeRenderer = connectedCube.GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = c;
        }
    }

}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFX_BloodDecalLayers : MonoBehaviour
{
    public LayerMask DecalLayers = 1;
    public DecalLayersProperty DecalRenderingMode = DecalLayersProperty.DrawToSelectedLayers;
    public DepthMode LayerDepthResoulution = DepthMode.FullScreen;

    DepthTextureMode defaultMode;
    RenderTexture rt;
    Camera depthCamera;

    void OnEnable()
    {
        var currentCam = GetComponent<Camera>();
        defaultMode = currentCam.depthTextureMode;
        if (currentCam.renderingPath == RenderingPath.Forward)
        {
            currentCam.depthTextureMode |= DepthTextureMode.Depth;
        }

        var go = new GameObject("DecalLayersCamera");
        go.transform.parent = currentCam.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        depthCamera = go.AddComponent<Camera>();
        depthCamera.CopyFrom(currentCam);
        depthCamera.renderingPath = RenderingPath.Forward;
        depthCamera.depth = currentCam.depth - 1;
        depthCamera.cullingMask = DecalLayers;

        CreateDepthTexture();
        depthCamera.targetTexture = rt;
        Shader.SetGlobalTexture("_LayerDecalDepthTexture", rt);
        Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS");

        if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers) Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
    }

    void OnDisable()
    {
        GetComponent<Camera>().depthTextureMode = defaultMode;
        rt.Release();
        Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS");
        if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers) Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
    }

    private void Update()
    {
        if (Screen.width != rt.width || Screen.height != rt.height)
        {
            depthCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            CreateDepthTexture();
            depthCamera.targetTexture = rt;
            Shader.SetGlobalTexture("_LayerDecalDepthTexture", rt);
        }
    }

    void CreateDepthTexture()
    {
        switch (LayerDepthResoulution)
        {
            case DepthMode.FullScreen:
                rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
                break;
            case DepthMode.HalfScreen:
                rt = RenderTexture.GetTemporary((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f), 24, RenderTextureFormat.Depth);
                break;
            case DepthMode.QuarterScreen:
                rt = RenderTexture.GetTemporary((int)(Screen.width * 0.25f), (int)(Screen.height * 0.25f), 24, RenderTextureFormat.Depth);
                break;
            default:
                break;
        };
    }


    public enum DecalLayersProperty
    {
        DrawToSelectedLayers,
        IgnoreSelectedLayers
    }

    public enum DepthMode
    {
        FullScreen,
        HalfScreen,
        QuarterScreen
    }
}

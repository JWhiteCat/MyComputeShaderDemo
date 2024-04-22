using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;

public class ThreeTextures : MonoBehaviour
{
    public ComputeShader shader;
    public Material mat_st;
    public Material mat_append;
    public Material mat_rt;

    private int size = 128;
    private int kernel;

    private ComputeBuffer stBuffer;
    private ComputeBuffer appendBuffer;
    private RenderTexture rt;

    private Texture2D stTexture;
    private Texture2D appendTexture;

    void Start()
    {
        stTexture = new Texture2D(size, size, TextureFormat.ARGB32, false); //TextureFormat.RGBAFloat
        stTexture.filterMode = FilterMode.Point;

        appendTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        appendTexture.filterMode = FilterMode.Point;

        stBuffer = new ComputeBuffer(size * size, sizeof(float) * 4, ComputeBufferType.Default);
        appendBuffer = new ComputeBuffer(size * size, sizeof(float) * 4, ComputeBufferType.Append);
        rt = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32); //RenderTextureFormat.ARGBFloat
        rt.enableRandomWrite = true;
        rt.Create(); //似乎可以不加这一行

        mat_st.SetTexture("_MainTex", stTexture);
        mat_append.SetTexture("_MainTex", appendTexture);
        mat_rt.SetTexture("_MainTex", rt);

        shader.SetBuffer(kernel, "StBuffer", stBuffer);
        appendBuffer.SetCounterValue(0);
        shader.SetBuffer(kernel, "AppendBuffer", appendBuffer);
        shader.SetTexture(kernel, "RT", rt);

        kernel = shader.FindKernel("CSMain");
        shader.Dispatch(kernel, size / 8, size / 8, 1);

        #region test

        Vector4[] stData = new Vector4[size * size];
        stBuffer.GetData(stData);

        #endregion

        Color[] colors = new Color[size * size];
        stBuffer.GetData(colors);
        stTexture.SetPixels(colors);
        stTexture.Apply();

        appendBuffer.GetData(colors);
        appendTexture.SetPixels(colors);
        appendTexture.Apply();
    }

    private void OnDisable()
    {
        stBuffer?.Release();
        stBuffer = null;

        appendBuffer?.Release();
        appendBuffer = null;

        rt?.Release();
        rt = null;
    }
}
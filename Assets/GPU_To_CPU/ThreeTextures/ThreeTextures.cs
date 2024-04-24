using UnityEngine;

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
        Color[] colors = new Color[size * size];

        {
            stTexture = new Texture2D(size, size, TextureFormat.ARGB32, false); //TextureFormat.RGBAFloat
            stTexture.filterMode = FilterMode.Point;

            stBuffer = new ComputeBuffer(size * size, sizeof(float) * 4, ComputeBufferType.Default);

            mat_st.SetTexture("_MainTex", stTexture);

            shader.SetBuffer(kernel, "StBuffer", stBuffer);

            #region test

            Vector4[] stData = new Vector4[size * size];
            stBuffer.GetData(stData);

            #endregion
        }

        {
            appendTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            appendTexture.filterMode = FilterMode.Point;

            appendBuffer = new ComputeBuffer(size * size, sizeof(float) * 4, ComputeBufferType.Append);

            mat_append.SetTexture("_MainTex", appendTexture);

            appendBuffer.SetCounterValue(0);
            shader.SetBuffer(kernel, "AppendBuffer", appendBuffer);
        }

        {
            rt = new RenderTexture(size, size, 0); //RenderTextureFormat.ARGBFloat
            // rt.filterMode = FilterMode.Point;
            rt.enableRandomWrite = true;
            rt.Create(); //似乎可以不加这一行

            mat_rt.SetTexture("_MainTex", rt);

            shader.SetTexture(kernel, "RT", rt);
        }

        kernel = shader.FindKernel("CSMain");
        shader.Dispatch(kernel, size / 8, size / 8, 1);

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
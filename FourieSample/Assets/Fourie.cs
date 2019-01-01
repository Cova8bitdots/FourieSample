using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Fourie : EditorWindow
{
    
    //-----------------------------------
    // メニュー
    //-----------------------------------
    #region ===== MENU =====
    // Menu に登録

    [MenuItem ("SOUND/Fourie", false, 100)]
    private static void OpenAuthoringTool()
    {
        EditorWindow.GetWindow<Fourie>( false, "Fourie" );
    } 
    #endregion //) ===== MENU =====

    const int DEFAULT_N = 100;
    const int TEX_COUNT = 8;
    const int PERIOD = 440;
    const int TEX_HEIGHT = 200;
    const int WIDTH = PERIOD * 2;// 2周期分

    int MAX_N = DEFAULT_N;
    float[][] samples = new float[DEFAULT_N][];
    static readonly int[] N_ARRAY = new int[]{1,2,3,5,10,20,50,100};
    Texture2D[] textures = new Texture2D[TEX_COUNT];
    Texture2D rectWaveTex = null;
    Color lineColor = Color.red;
    Vector2 scrollPos;

    
    void OnGUI()
    {
        MAX_N = Mathf.Clamp( EditorGUILayout.IntField( "次数", MAX_N), 1, 1024 );
        lineColor = EditorGUILayout.ColorField( "描画色", lineColor);
        if( GUILayout.Button("生成"))
        {
            Generate();
        }

        scrollPos = EditorGUILayout.BeginScrollView( scrollPos );
        {
            if( rectWaveTex != null )
            {
                GUILayout.Label( $"矩形波");
                GUILayout.Label(rectWaveTex);
            }
            for (int i = 0; i < TEX_COUNT; i++)
            {
                GUILayout.Label( $"N={N_ARRAY[i]}");
                GUILayout.Label(textures[i]);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void Generate()
    {
        // Init
        float[][] waveTex = new float[TEX_COUNT][];
        samples = new float[MAX_N][];
        for (int j = 0; j < WIDTH; j++)
        {
            for (int i = 0; i < MAX_N; i++)
            {
                samples[i] = new float[WIDTH];
                samples[i][j]= 0f;
            }
            for (int i = 0; i < TEX_COUNT; i++)
            {
                waveTex[i] = new float[WIDTH];
                waveTex[i][j]= 0f;
            }
        }
        // 基本波生成
        const float omega = 2.0f * Mathf.PI / (float)PERIOD;
        // const float coeff = 4.0f / Mathf.PI;
        for (int i = 1; i <=MAX_N; i++)
        {
            float a = 2.0f*i -1.0f;
            for (int j = 0; j < WIDTH; j++)
            {
                // samples[i-1][j] =coeff * Mathf.Sin(a * omega  * j ) / a;
                samples[i-1][j] = Mathf.Sin(a * omega  * j ) / a;
            }
        }
        // 合成波生成
        for (int i = 0; i < MAX_N; i++)
        {
            for (int j = 0; j < N_ARRAY.Length; j++)
            {
                if( i < N_ARRAY[j])
                {
                    AddWave( j, ref waveTex, samples[i]);
                    break;
                }
            }

        }

        // 波形描画
        for (int i = 0; i < TEX_COUNT; i++)
        {
            textures[i] = new Texture2D(WIDTH, TEX_HEIGHT);
            Normalize( ref waveTex[i], TEX_HEIGHT /2);
            for (int j = 0; j < WIDTH; j++)
            {
                int height = Mathf.Clamp( Mathf.RoundToInt( waveTex[i][j] + TEX_HEIGHT /2), 0, TEX_HEIGHT-1);
                textures[i].SetPixel(j, height, lineColor);
            }
            textures[i].Apply();
        }

        // Draw Rectanglar Waveform
        rectWaveTex = new Texture2D(WIDTH, TEX_HEIGHT);
        for (int i = 0; i < WIDTH; i++)
        {
            if( i % (PERIOD/2) == 0 )
            {
                for (int j = 0; j < TEX_HEIGHT; j++)
                {
                    rectWaveTex.SetPixel(i, j, lineColor);
                }
            }
            if(i% PERIOD < PERIOD /2 )
            {
                rectWaveTex.SetPixel(i, TEX_HEIGHT-1, lineColor);
            }
            else
            {
                rectWaveTex.SetPixel(i, 0, lineColor);
            }
        }
        rectWaveTex.Apply();
        
    }

    /// <summary>
    /// 波形の合成
    /// </summary>
    /// <param name="_startIndex"></param>
    /// <param name="_waveTex"></param>
    /// <param name="_samples"></param>
    void AddWave( int _startIndex, ref float[][] _waveTex, float[] _samples)
    {
        for (int i = _startIndex; i < TEX_COUNT; i++)
        {
            for (int j = 0; j < WIDTH; j++)
            {
                _waveTex[i][j] += _samples[j];
            }
        }
    }

    /// <summary>
    /// 図形描画用の正規化
    /// </summary>
    /// <param name="_waveTex"></param>
    /// <param name="_maxHeight"></param>
    void Normalize( ref float[] _waveTex, int _maxHeight)
    {
        float max = 0f;
        for (int i = 0; i < WIDTH; i++)
        {
            max = Mathf.Max( max, Mathf.Abs(_waveTex[i]));
        }

        // Normalize
        for (int i = 0; i < WIDTH; i++)
        {
            _waveTex[i] = _waveTex[i] / max * _maxHeight;
        }
    }
}

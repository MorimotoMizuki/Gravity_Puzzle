using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 画面のアスペクト比の変更処理
/// </summary>
[ExecuteAlways]
public class Aspect_Keeper_Gravity_Puzzle : MonoBehaviour
{
    [Header("対象のカメラ")]
    [SerializeField] private Camera Target_Camera;

    [Header("目的の画面解像度")]
    public Vector2 Aspect_vec;//600*900

    //変更前のスクリーンサイズ
    private int _Prev_Width = 0;
    private int _Prev_Height = 0;

    void Update()
    {
        //画面サイズが変更された場合のみ、Viewportを更新
        //if (_Prev_Width != Screen.width || _Prev_Height != Screen.height)
            UpdateCameraViewport();
    }

    /// <summary>
    /// カメラのViewportをアスペクト比に応じて調整
    /// </summary>
    private void UpdateCameraViewport()
    {
        var screen_aspect = Screen.width / (float)Screen.height; //画面のアスペクト比
        var target_aspect = Aspect_vec.x / Aspect_vec.y;         //目的のアスペクト比       
        var mag_rate = target_aspect / screen_aspect;       //目的アスペクト比にするための倍率       
        var viewport_rect = new Rect(0, 0, 1, 1);                //Viewport初期値でRectを作成

        if (mag_rate < 1)
        {
            viewport_rect.width = mag_rate;                         //使用する横幅を変更           
            viewport_rect.x = 0.5f - viewport_rect.width * 0.5f;    //中央寄せ
        }
        else
        {
            viewport_rect.height = 1 / mag_rate;                    //使用する縦幅を変更            
            viewport_rect.y = 0.5f - viewport_rect.height * 0.5f;   //中央寄せ
        }
        //現在の画面サイズを保存
        _Prev_Width = Screen.width;
        _Prev_Height = Screen.height;

        //カメラのViewportに適用
        Target_Camera.rect = viewport_rect;
    }
}

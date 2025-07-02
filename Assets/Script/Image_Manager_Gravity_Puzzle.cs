using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Image_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("ブロックのベース画像")]
    public Sprite _BlockBase_Img;

    [Header("ブロックの上下左右画像")]
    public Sprite[] _Block_img;

    [Header("キャラクターの通常アニメーション画像")]
    public Sprite[] _Player_Normal_img;
    [Header("キャラクターの落下アニメーション画像")]
    public Sprite[] _Player_Fall_img;
    [Header("風船の通常アニメーション画像")]
    public Sprite[] _Balloon_Normal_img;

    [Header("タイマー画像")]
    public Image _HP_Fill;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// オブジェクトの表示切り替え
    /// </summary>
    /// <param name="target_obj">表示を切り替えるオブジェクト</param>
    /// <param name="is_active">true : 表示, false : 非表示</param>
    public void Change_Active(GameObject target_obj, bool is_active)
    {
        target_obj.SetActive(is_active);
    }

    /// <summary>
    /// 画像変更
    /// </summary>
    /// <param name="change_img_obj">変更元オブジェクト</param>
    /// <param name="target_img">変更先の画像</param>
    public void Change_Image(Image change_img_obj, Sprite target_img)
    {
        //変更元のオブジェクトが無い場合は終了
        if (change_img_obj == null) return;

        //画像変更
        change_img_obj.sprite = target_img;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Image_Manager_Gravity_Puzzle : MonoBehaviour
{
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

}

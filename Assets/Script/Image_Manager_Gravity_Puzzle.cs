using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Image_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("�^�C�}�[�摜")]
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
    /// �I�u�W�F�N�g�̕\���؂�ւ�
    /// </summary>
    /// <param name="target_obj">�\����؂�ւ���I�u�W�F�N�g</param>
    /// <param name="is_active">true : �\��, false : ��\��</param>
    public void Change_Active(GameObject target_obj, bool is_active)
    {
        target_obj.SetActive(is_active);
    }

}

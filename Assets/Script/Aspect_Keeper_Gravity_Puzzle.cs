using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ʂ̃A�X�y�N�g��̕ύX����
/// </summary>
[ExecuteAlways]
public class Aspect_Keeper_Gravity_Puzzle : MonoBehaviour
{
    [Header("�Ώۂ̃J����")]
    [SerializeField] private Camera Target_Camera;

    [Header("�ړI�̉�ʉ𑜓x")]
    public Vector2 Aspect_vec;//600*900

    //�ύX�O�̃X�N���[���T�C�Y
    private int _Prev_Width = 0;
    private int _Prev_Height = 0;

    void Update()
    {
        //��ʃT�C�Y���ύX���ꂽ�ꍇ�̂݁AViewport���X�V
        //if (_Prev_Width != Screen.width || _Prev_Height != Screen.height)
            UpdateCameraViewport();
    }

    /// <summary>
    /// �J������Viewport���A�X�y�N�g��ɉ����Ē���
    /// </summary>
    private void UpdateCameraViewport()
    {
        var screen_aspect = Screen.width / (float)Screen.height; //��ʂ̃A�X�y�N�g��
        var target_aspect = Aspect_vec.x / Aspect_vec.y;         //�ړI�̃A�X�y�N�g��       
        var mag_rate = target_aspect / screen_aspect;       //�ړI�A�X�y�N�g��ɂ��邽�߂̔{��       
        var viewport_rect = new Rect(0, 0, 1, 1);                //Viewport�����l��Rect���쐬

        if (mag_rate < 1)
        {
            viewport_rect.width = mag_rate;                         //�g�p���鉡����ύX           
            viewport_rect.x = 0.5f - viewport_rect.width * 0.5f;    //������
        }
        else
        {
            viewport_rect.height = 1 / mag_rate;                    //�g�p����c����ύX            
            viewport_rect.y = 0.5f - viewport_rect.height * 0.5f;   //������
        }
        //���݂̉�ʃT�C�Y��ۑ�
        _Prev_Width = Screen.width;
        _Prev_Height = Screen.height;

        //�J������Viewport�ɓK�p
        Target_Camera.rect = viewport_rect;
    }
}

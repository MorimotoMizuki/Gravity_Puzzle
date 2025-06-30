using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Game_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("�Q�[���I�[�o�[���ɕ\������I�u�W�F�N�g")]
    [SerializeField] private GameObject _GameOver_obj;
    [Header("�Q�[���N���A���ɕ\������I�u�W�F�N�g")]
    [SerializeField] private GameObject _GameClear_obj;

    //�^�C�}�[�֌W
    private float _Limit_time;   //��������
    private float _Current_time; //�c�莞��

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //�Q�[����ʈȊO�̏ꍇ�͏I��
        if (GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID != GrovalConst_Gravity_Puzzle.Screen_ID.GAME)
            return;

        switch (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE)
        {
            //�ҋ@�t�F�[�Y
            case GrovalConst_Gravity_Puzzle.GameState.READY:
                {
                    break;
                }
            //�X�e�[�W�����t�F�[�Y
            case GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE:
                {
                    Reset_Stage();  //�X�e�[�W���Z�b�g
                    Create_Stage(); //�X�e�[�W����
                    break;
                }
            //�Q�[���v���C�t�F�[�Y
            case GrovalConst_Gravity_Puzzle.GameState.PLAYING:
                {
                    Timer(); //�^�C�}�[
                    break;
                }
            //�Q�[���N���A�t�F�[�Y
            case GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR:
                {
                    //�Q�[���N���A���ɕ\������I�u�W�F�N�g��\��
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, true);
                    break;
                }
            //�Q�[���I�[�o�[�t�F�[�Y
            case GrovalConst_Gravity_Puzzle.GameState.GAMEOVER:
                {
                    //�Q�[���I�[�o�[���ɕ\������I�u�W�F�N�g��\��
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, true);
                    break;
                }
        }
    }

    /// <summary>
    /// �X�e�[�W����
    /// </summary>
    private void Create_Stage()
    {
        GrovalNum_Gravity_Puzzle.sImageManager._HP_Fill.fillAmount = Mathf.InverseLerp(0, _Limit_time, _Current_time); //�^�C�}�[�\��������

        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.PLAYING;
    }

    /// <summary>
    /// �X�e�[�W���Z�b�g
    /// </summary>
    private void Reset_Stage()
    {
        //�Q�[���N���A���ɕ\������I�u�W�F�N�g��\��
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, false);
        //�Q�[���I�[�o�[���ɕ\������I�u�W�F�N�g��\��
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, false);
    }


    #region ���Ԋ֌W ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// �������Ԑݒ�
    /// </summary>
    /// <param name="time">��������</param>
    public void Set_Limit_Time(float time)
    {
        _Current_time = time;
        _Limit_time = time;
    }

    /// <summary>
    /// �������Ԍv��
    /// </summary>
    /// <returns>�Q�[���I�[�o�[�̉�</returns>
    private bool Timer()
    {
        _Current_time -= Time.deltaTime;

        if (_Current_time <= 0.0f)
        {
            _Current_time = 0.0f;
            GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
            return true;
        }

        GrovalNum_Gravity_Puzzle.sImageManager._HP_Fill.fillAmount = Mathf.InverseLerp(0, _Limit_time, _Current_time);

        return false;
    }

    #endregion ------------------------------------------------------------------------------------------------------------
}

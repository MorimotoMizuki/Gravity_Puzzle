using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Screen_Change_Gravity_Puzzle : MonoBehaviour
{
    [Header("�e��ʂ̃L�����o�X�I�u�W�F�N�g")]
    public Canvas[] _Screen_Canvas;

    [Header("�t�F�[�h������摜")]
    [SerializeField] private Image _Fade_Img;

    [Header("��ʂ̃t�F�[�h����")]
    private float _Fade_Speed = 0.5f;

    //�Q�[�������ʑJ�ڃt���O
    private bool _Is_Screen_Judge = false;
    //�Q�[�������ʂɑJ�ڂ�����ڔ���p
    private bool _Is_Judge_First = true;

    // Start is called before the first frame update
    void Start()
    {
        //�S�Ẳ�ʂ��\��
        for (int i = 0; i < _Screen_Canvas.Length; i++)
            _Screen_Canvas[i].gameObject.SetActive(false);
        //�^�C�g����ʂ�\��
        _Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.TITLE].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //�^�C�g����ʃN���b�N
        if(GrovalNum_Gravity_Puzzle.sClickManager._Is_Title_Screen_Click)
        {
            //�\����\�����ID�p
            GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;

            //��ʐ؂�ւ�
            if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                Screen_Change_Start(display_id, invisible_id, true);

            GrovalNum_Gravity_Puzzle.sClickManager._Is_Title_Screen_Click = false;
        }

        //�{�^���N���b�N������
        Clicked_Button();

        //�Q�[�����莞����
        Game_Judge_Screen();
    }

    /// <summary>
    /// ��ʐ؂�ւ��̃R���[�`�����Ăяo���֐�
    /// </summary>
    /// <param name="display_id">�\����������ʂ�����ID</param>
    /// <param name="invisible_id">��\���ɂ�������ʂ�����ID</param>
    /// <param name="is_fade">�t�F�[�h���s����</param>
    public void Screen_Change_Start(GrovalConst_Gravity_Puzzle.Screen_ID display_id, GrovalConst_Gravity_Puzzle.Screen_ID invisible_id, bool is_fade)
    {
        //���ID�����ݒ�̏ꍇ�͏I��
        if (display_id == GrovalConst_Gravity_Puzzle.Screen_ID.NONE || invisible_id == GrovalConst_Gravity_Puzzle.Screen_ID.NONE) return;

        //�R���[�`���J�n
        StartCoroutine(Screen_Change_Coroutine(display_id, invisible_id, is_fade));
    }

    /// <summary>
    ///  ��ʐ؂�ւ��R���[�`��
    /// </summary>
    /// <param name="display_id">�\�������ʂ�����ID</param>
    /// <param name="invisible_id">��\���ɂ����ʂ�����ID</param>
    /// <param name="is_fade">�t�F�[�h���s����</param>
    /// <returns>�R���[�`���p IEnumerator</returns>
    private IEnumerator Screen_Change_Coroutine(GrovalConst_Gravity_Puzzle.Screen_ID display_id, GrovalConst_Gravity_Puzzle.Screen_ID invisible_id, bool is_fade)
    {
        Color fade_color = _Fade_Img.color; //�t�F�[�h����F

        if (is_fade)
        {
            //�t�F�[�h����F�̐ݒ�
            if (display_id == GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR)
                fade_color = Color.white;
            else
                fade_color = Color.black;

            //�t�F�[�h�C��
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.FADE].gameObject, true);
            yield return StartCoroutine(Fade(0f, 1f, fade_color)); //�������F
        }

        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)invisible_id].gameObject, false); //��ʔ�\��            
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)display_id].gameObject, true);    //��ʕ\��

        GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID = display_id;      //���݂̉�ʏ��X�V        

        //���ݕ\������Ă�����
        switch (display_id)
        {
            //�^�C�g�����
            case GrovalConst_Gravity_Puzzle.Screen_ID.TITLE:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;//�ҋ@�t�F�[�Y
                    GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL = 1; //�X�e�[�W���x����1�ɂ���
                    break;
                }
            //�Q�[�����
            case GrovalConst_Gravity_Puzzle.Screen_ID.GAME:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE;//�}�b�v�����t�F�[�Y

                    //�X�e�[�W�̐������Ԑݒ�
                    {
                        int index = GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL - 1; //�z��Ăяo���C���f�N�X
                        int time = 60; //��������

                        //���Ԃ��ݒ肳��Ă���ꍇ
                        if (index <= GrovalNum_Gravity_Puzzle.sGamePreference._Time.Length)
                            time = GrovalNum_Gravity_Puzzle.sGamePreference._Time[index];

                        //�^�C�}�[�̎��Ԃ�ݒ�
                        GrovalNum_Gravity_Puzzle.sGameManager.Set_Limit_Time(time);
                    }
                    break;
                }
            //�N���A���
            case GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;//�ҋ@�t�F�[�Y
                    _Is_Judge_First = true;
                    break;
                }
        }

        Debug.Log(GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID);

        if (is_fade)
        {
            // �t�F�[�h�A�E�g
            yield return StartCoroutine(Fade(1f, 0f, fade_color)); //�F������
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.FADE].gameObject, false);
        }
    }

    /// <summary>
    /// �t�F�[�h�R���[�`��
    /// </summary>
    /// <param name="from">���݂̉�ʂ� alpha�l</param>
    /// <param name="to">�ŏI�I�Ȃ̉�ʂ� alpha�l</param>
    /// <param name="color">�t�F�[�h����F</param>
    /// <returns>�R���[�`���p�� IEnumerator</returns>
    private IEnumerator Fade(float from, float to, Color color)
    {
        float timer = 0f; //�o�ߎ��Ԃ̏�����

        //�t�F�[�h���Ԓ����[�v
        while (timer < _Fade_Speed)
        {
            float alpha = Mathf.Lerp(from, to, timer / _Fade_Speed);        //�w�莞�ԓ���alpha�l����            
            _Fade_Img.color = new Color(color.r, color.g, color.b, alpha);  //��Ԃ���alpha�l��p���ĐF��ݒ�(rgb�͂��̂܂܂�alpha�l�̂ݕύX)           
            timer += Time.deltaTime;                                        //�o�ߎ��Ԃ����Z            
            yield return null;                                              //1�t���[���ҋ@
        }
        _Fade_Img.color = new Color(color.r, color.g, color.b, to);         //�ŏI�I�ȐF��ݒ�
    }

    /// <summary>
    /// �Q�[�������ʑJ�ڃt���Otrue
    /// </summary>
    private void Set_Is_Screen_Judge()
    {
        _Is_Screen_Judge = true;
    }

    /// <summary>
    /// �Q�[�����莞����
    /// </summary>
    private void Game_Judge_Screen()
    {
        if (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE == GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR &&
            _Is_Judge_First)
        {
            //�x������
            Invoke("Set_Is_Screen_Judge", GrovalNum_Gravity_Puzzle.sGamePreference._Judge_Screen_Latency);
            _Is_Judge_First = false;
        }

        if (_Is_Screen_Judge)
        {
            //�\����\�����ID�p
            GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR;
            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;

            //��ʐ؂�ւ�
            if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                Screen_Change_Start(display_id, invisible_id, true);

            _Is_Screen_Judge = false;
        }
    }

    /// <summary>
    /// �{�^���N���b�N������
    /// </summary>
    private void Clicked_Button()
    {
        //�{�^���N���b�N
        for (GrovalConst_Gravity_Puzzle.Button_ID i = GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP; i <= GrovalConst_Gravity_Puzzle.Button_ID.TITLE; i++)
        {
            if (GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)i])
            {
                //�\����\�����ID�p
                GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

                switch (i)
                {
                    //�M�u�A�b�v�{�^��
                    case GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP:
                        {
                            GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
                            break;
                        }
                    //�l�N�X�g�{�^��
                    case GrovalConst_Gravity_Puzzle.Button_ID.NEXT:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;
                            break;
                        }
                    //���v���C�{�^��
                    case GrovalConst_Gravity_Puzzle.Button_ID.REPLAY:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            break;
                        }
                    //�^�C�g���{�^��
                    case GrovalConst_Gravity_Puzzle.Button_ID.TITLE:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            break;
                        }
                }

                //��ʐ؂�ւ�
                if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                    Screen_Change_Start(display_id, invisible_id, true);

                GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)i] = false;
            }
        }
    }
}

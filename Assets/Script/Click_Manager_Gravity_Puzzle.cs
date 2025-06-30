using System.Collections;
using System.Collections.Generic;
using Common_Gravity_Puzzle;
using UnityEditor;
using UnityEngine;

public class Click_Manager_Gravity_Puzzle : MonoBehaviour
{
    //�{�^���N���b�N�ۃt���O
    [HideInInspector]
    public bool[] _Is_Button;

    //�X�N���[���N���b�N�ۃt���O
    [HideInInspector]
    public bool _Is_Title_Screen_Click = false;
    private bool _Is_Title_First = true;

    //�N���b�N�t���O
    private bool _Is_Touch_or_Click_down;   //�N���b�N�܂��̓^�b�`���J�n���ꂽ�u��
    private bool _Is_Touch_or_Click_up;     //�N���b�N�܂��̓^�b�`���I�������u��
    private bool _Is_Touch_or_Click_active; //�N���b�N�܂��̓^�b�`���p�����Ă����(�h���b�O��)

    // Start is called before the first frame update
    void Start()
    {
        _Is_Button = new bool[4];
    }

    // Update is called once per frame
    void Update()
    {
        //�N���b�N�܂��̓^�b�`���J�n���ꂽ�u�ԁi�����n�߁j�����o����
        _Is_Touch_or_Click_down = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        //�N���b�N�܂��̓^�b�`���I�������u�ԁi�������j�����o����
        _Is_Touch_or_Click_up = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
        //�N���b�N�܂��̓^�b�`���p�����Ă���ԁi�h���b�O���j�����o����
        _Is_Touch_or_Click_active = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);

        switch(GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID)
        {
                case GrovalConst_Gravity_Puzzle.Screen_ID.TITLE:
                {
                    //��ʃN���b�N����
                    if(_Is_Touch_or_Click_down && _Is_Title_First)
                    {
                        _Is_Title_Screen_Click = true;
                        _Is_Title_First = false;
                    }
                    break;
                }
                case GrovalConst_Gravity_Puzzle.Screen_ID.GAME:
                {
                    //���d�N���b�N��h������ : �t���O������
                    if (!_Is_Title_First)
                        _Is_Title_First = true;

                    break;
                }
        }

        //�G�X�P�[�v�L�[����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();  //�Q�[���I��
        }

    }

    //�l�N�X�g�{�^�� : �N���A���
    public void Button_Clicked_Next()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.NEXT] = true;                   //�{�^���t���Otrue
    }
    //���v���C�{�^�� : �Q�[���I�[�o�[���
    public void Button_Clicked_Replay()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.REPLAY] = true;                 //�{�^���t���Otrue
    }
    //�^�C�g���{�^�� : �Q�[���I�[�o�[���
    public void Button_Clicked_Title()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.TITLE] = true;                  //�{�^���t���Otrue
    }
    //�M�u�A�b�v�{�^�� : �Q�[�����
    public void Button_Clicked_GiveUp()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP] = true;                  //�{�^���t���Otrue
    }

    /// <summary>
    /// �Q�[���I���֐�
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // �G�f�B�^��ł̓v���C���[�h���~
        EditorApplication.isPlaying = false;
#else
            // �r���h��̓A�v�����I��
            Application.Quit();
#endif
    }

}

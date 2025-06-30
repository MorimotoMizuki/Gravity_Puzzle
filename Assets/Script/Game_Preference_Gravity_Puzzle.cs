using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common_Gravity_Puzzle;

public class Game_Preference_Gravity_Puzzle : MonoBehaviour
{
    [Header("�e�X�N���v�g")]
    [SerializeField] private Game_Manager_Gravity_Puzzle  game_manager;
    [SerializeField] private Image_Manager_Gravity_Puzzle image_manager;
    [SerializeField] private Screen_Change_Gravity_Puzzle screen_change;
    [SerializeField] private Click_Manager_Gravity_Puzzle click_manager;

    [Header("�X�e�[�W�̐�������(�b)")]
    public int[] _Time;

    [Header("�Q�[�������ʂɑJ�ڂ���ҋ@����(�b)")]
    public float _Judge_Screen_Latency = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        GrovalNum_Gravity_Puzzle.sGamePreference = this;
        GrovalNum_Gravity_Puzzle.sGameManager = game_manager;
        GrovalNum_Gravity_Puzzle.sImageManager = image_manager;
        GrovalNum_Gravity_Puzzle.sScreenChange = screen_change;
        GrovalNum_Gravity_Puzzle.sClickManager = click_manager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

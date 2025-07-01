using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common_Gravity_Puzzle;

public class Game_Preference_Gravity_Puzzle : MonoBehaviour
{
    [Header("各スクリプト")]
    [SerializeField] private Game_Manager_Gravity_Puzzle  game_manager;
    [SerializeField] private Image_Manager_Gravity_Puzzle image_manager;
    [SerializeField] private Screen_Change_Gravity_Puzzle screen_change;
    [SerializeField] private Click_Manager_Gravity_Puzzle click_manager;
    [SerializeField] private Csv_Roader_Gravity_Puzzle    csv_roder;

    [Header("ステージの制限時間(秒)")]
    public int[] _Time;

    [Header("ゲーム判定画面に遷移する待機時間(秒)")]
    public float _Judge_Screen_Latency = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        GrovalNum_Gravity_Puzzle.sGamePreference = this;
        GrovalNum_Gravity_Puzzle.sGameManager = game_manager;
        GrovalNum_Gravity_Puzzle.sImageManager = image_manager;
        GrovalNum_Gravity_Puzzle.sScreenChange = screen_change;
        GrovalNum_Gravity_Puzzle.sClickManager = click_manager;
        GrovalNum_Gravity_Puzzle.sCsvRoader = csv_roder;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

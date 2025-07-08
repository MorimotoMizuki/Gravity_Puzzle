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

    [Header("ステージごとの初期の重力の方向")]
    public GrovalConst_Gravity_Puzzle.Flick_ID[] _First_Gravity_Dir;

    [Header("マスク画像の透明度の最大値と最小値")]
    public float _Max_Mask_Alpha = 0.8f;
    public float _Min_Mask_Alpha = 0.2f;

    [Header("ゲーム判定画面に遷移する待機時間(秒)")]
    public float _Judge_Screen_Latency = 1.0f;

    [Header("キャラクターの常時アニメーション切り替えカウント")]
    public int _Character_Anim_Change_cnt = 30;
    [Header("キャラクターのクラッシュアニメーション切り替えカウント")]
    public int _Character_CrashAnim_Change_cnt = 10;

    [Header("ブロックの矢印の回転スピード")]
    public float _BlockArrow_RotSpeed = 120.0f;


    // Start is called before the first frame update
    void Start()
    {
        GrovalNum_Gravity_Puzzle.sGamePreference = this;
        GrovalNum_Gravity_Puzzle.sGameManager = game_manager;
        GrovalNum_Gravity_Puzzle.sImageManager = image_manager;
        GrovalNum_Gravity_Puzzle.sScreenChange = screen_change;
        GrovalNum_Gravity_Puzzle.sClickManager = click_manager;
        GrovalNum_Gravity_Puzzle.sCsvRoader = csv_roder;

        //60fpsに設定
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Game_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("ゲームオーバー時に表示するオブジェクト")]
    [SerializeField] private GameObject _GameOver_obj;
    [Header("ゲームクリア時に表示するオブジェクト")]
    [SerializeField] private GameObject _GameClear_obj;

    //タイマー関係
    private float _Limit_time;   //制限時間
    private float _Current_time; //残り時間

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //ゲーム画面以外の場合は終了
        if (GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID != GrovalConst_Gravity_Puzzle.Screen_ID.GAME)
            return;

        switch (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE)
        {
            //待機フェーズ
            case GrovalConst_Gravity_Puzzle.GameState.READY:
                {
                    break;
                }
            //ステージ生成フェーズ
            case GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE:
                {
                    Reset_Stage();  //ステージリセット
                    Create_Stage(); //ステージ生成
                    break;
                }
            //ゲームプレイフェーズ
            case GrovalConst_Gravity_Puzzle.GameState.PLAYING:
                {
                    Timer(); //タイマー
                    break;
                }
            //ゲームクリアフェーズ
            case GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR:
                {
                    //ゲームクリア時に表示するオブジェクトを表示
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, true);
                    break;
                }
            //ゲームオーバーフェーズ
            case GrovalConst_Gravity_Puzzle.GameState.GAMEOVER:
                {
                    //ゲームオーバー時に表示するオブジェクトを表示
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, true);
                    break;
                }
        }
    }

    /// <summary>
    /// ステージ生成
    /// </summary>
    private void Create_Stage()
    {
        GrovalNum_Gravity_Puzzle.sImageManager._HP_Fill.fillAmount = Mathf.InverseLerp(0, _Limit_time, _Current_time); //タイマー表示初期化

        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.PLAYING;
    }

    /// <summary>
    /// ステージリセット
    /// </summary>
    private void Reset_Stage()
    {
        //ゲームクリア時に表示するオブジェクトを表示
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, false);
        //ゲームオーバー時に表示するオブジェクトを表示
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, false);
    }


    #region 時間関係 ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 制限時間設定
    /// </summary>
    /// <param name="time">制限時間</param>
    public void Set_Limit_Time(float time)
    {
        _Current_time = time;
        _Limit_time = time;
    }

    /// <summary>
    /// 制限時間計測
    /// </summary>
    /// <returns>ゲームオーバーの可否</returns>
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

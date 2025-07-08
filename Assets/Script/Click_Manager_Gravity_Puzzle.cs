using System.Collections;
using System.Collections.Generic;
using Common_Gravity_Puzzle;
using UnityEditor;
using UnityEngine;

public class Click_Manager_Gravity_Puzzle : MonoBehaviour
{
    //ボタンクリック可否フラグ
    [HideInInspector] public bool[] _Is_Button;

    //スクリーンクリック可否フラグ
    [HideInInspector] public bool _Is_Title_Screen_Click = false;
    private bool _Is_Title_First = true;

    //フリック関係フラグ
    [HideInInspector] public bool _Is_flick_start = false;  //フリックスタート
    [HideInInspector] public bool _Is_flick_end = false;    //フリックエンド

    //クリックフラグ
    private bool _Is_Touch_or_Click_down;   //クリックまたはタッチが開始された瞬間
    private bool _Is_Touch_or_Click_up;     //クリックまたはタッチが終了した瞬間
    private bool _Is_Touch_or_Click_active; //クリックまたはタッチが継続している間(ドラッグ中)

    // Start is called before the first frame update
    void Start()
    {
        _Is_Button = new bool[5];
    }

    // Update is called once per frame
    void Update()
    {
        //クリックまたはタッチが開始された瞬間（押し始め）を検出する
        _Is_Touch_or_Click_down = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        //クリックまたはタッチが終了した瞬間（離した）を検出する
        _Is_Touch_or_Click_up = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
        //クリックまたはタッチが継続している間（ドラッグ中）を検出する
        _Is_Touch_or_Click_active = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);

        switch(GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID)
        {
                case GrovalConst_Gravity_Puzzle.Screen_ID.TITLE:
                {
                    //画面クリック判定
                    if(_Is_Touch_or_Click_down && _Is_Title_First)
                    {
                        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
                        _Is_Title_Screen_Click = true;
                        _Is_Title_First = false;
                    }
                    break;
                }
                case GrovalConst_Gravity_Puzzle.Screen_ID.GAME:
                {
                    //多重クリックを防ぐ処理 : フラグ初期化
                    if (!_Is_Title_First)
                        _Is_Title_First = true;

                    //ゲームプレイ中以外は終了
                    if (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE != GrovalConst_Gravity_Puzzle.GameState.PLAYING)
                        break;

                    //フリック許可がfalseの場合は終了
                    if (GrovalNum_Gravity_Puzzle.sGameManager._Is_Flick == false)
                        break;

                    //フリック処理
                    if (_Is_Touch_or_Click_down)
                    {
                        _Is_flick_start = true; //始点
                    }
                    if(_Is_Touch_or_Click_up)
                    {
                        _Is_flick_end = true;   //終点
                    }
                    break;
                }
        }

        //エスケープキー入力
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();  //ゲーム終了
        }

    }

    //ネクストボタン : クリア画面
    public void Button_Clicked_Next()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.NEXT] = true;      //ボタンフラグtrue
        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
    }
    //リプレイボタン : ゲームオーバー画面
    public void Button_Clicked_Replay()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.REPLAY] = true;    //ボタンフラグtrue
        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
    }
    //タイトルボタン : ゲームオーバー画面
    public void Button_Clicked_Title()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.TITLE] = true;     //ボタンフラグtrue
        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
    }
    //ギブアップボタン : ゲーム画面
    public void Button_Clicked_GiveUp()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP] = true;    //ボタンフラグtrue
        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
    }
    //スタートボタン : ゲーム画面
    public void Button_Clicked_Start()
    {
        _Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.START] = true;     //ボタンフラグtrue
        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BUTTON_CLICK); //SE再生
    }

    /// <summary>
    /// ゲーム終了関数
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // エディタ上ではプレイモードを停止
        EditorApplication.isPlaying = false;
#else
            // ビルド後はアプリを終了
            Application.Quit();
#endif
    }

    /// <summary>
    /// マウス or タッチの入力位置を返す
    /// </summary>
    /// <returns>マウス or タッチの入力位置</returns>
    public Vector2 GetInputPosition()
    {
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        else
            return Input.mousePosition;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;

public class Screen_Change_Gravity_Puzzle : MonoBehaviour
{
    [Header("各画面のキャンバスオブジェクト")]
    public Canvas[] _Screen_Canvas;

    [Header("フェードさせる画像")]
    [SerializeField] private Image _Fade_Img;

    [Header("画面のフェード時間")]
    private float _Fade_Speed = 0.5f;

    //ゲーム判定画面遷移フラグ
    private bool _Is_Screen_Judge = false;
    //ゲーム判定画面に遷移する一回目判定用
    private bool _Is_Judge_First = true;

    // Start is called before the first frame update
    void Start()
    {
        //全ての画面を非表示
        for (int i = 0; i < _Screen_Canvas.Length; i++)
            _Screen_Canvas[i].gameObject.SetActive(false);
        //タイトル画面を表示
        _Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.TITLE].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //タイトル画面クリック
        if(GrovalNum_Gravity_Puzzle.sClickManager._Is_Title_Screen_Click)
        {
            //表示非表示画面ID用
            GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;

            //画面切り替え
            if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                Screen_Change_Start(display_id, invisible_id, true);

            GrovalNum_Gravity_Puzzle.sClickManager._Is_Title_Screen_Click = false;
        }

        //ボタンクリック時処理
        Clicked_Button();

        //ゲーム判定時処理
        Game_Judge_Screen();
    }

    /// <summary>
    /// 画面切り替えのコルーチンを呼び出す関数
    /// </summary>
    /// <param name="display_id">表示したい画面を示すID</param>
    /// <param name="invisible_id">非表示にしたい画面を示すID</param>
    /// <param name="is_fade">フェードを行う可否</param>
    public void Screen_Change_Start(GrovalConst_Gravity_Puzzle.Screen_ID display_id, GrovalConst_Gravity_Puzzle.Screen_ID invisible_id, bool is_fade)
    {
        //画面IDが未設定の場合は終了
        if (display_id == GrovalConst_Gravity_Puzzle.Screen_ID.NONE || invisible_id == GrovalConst_Gravity_Puzzle.Screen_ID.NONE) return;

        //コルーチン開始
        StartCoroutine(Screen_Change_Coroutine(display_id, invisible_id, is_fade));
    }

    /// <summary>
    ///  画面切り替えコルーチン
    /// </summary>
    /// <param name="display_id">表示する画面を示すID</param>
    /// <param name="invisible_id">非表示にする画面を示すID</param>
    /// <param name="is_fade">フェードを行う可否</param>
    /// <returns>コルーチン用 IEnumerator</returns>
    private IEnumerator Screen_Change_Coroutine(GrovalConst_Gravity_Puzzle.Screen_ID display_id, GrovalConst_Gravity_Puzzle.Screen_ID invisible_id, bool is_fade)
    {
        Color fade_color = _Fade_Img.color; //フェードする色

        if (is_fade)
        {
            //フェードする色の設定
            if (display_id == GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR)
                fade_color = Color.white;
            else
                fade_color = Color.black;

            //フェードイン
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.FADE].gameObject, true);
            yield return StartCoroutine(Fade(0f, 1f, fade_color)); //透明→色
        }

        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)invisible_id].gameObject, false); //画面非表示            
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)display_id].gameObject, true);    //画面表示

        GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID = display_id;      //現在の画面情報更新        

        //現在表示されている画面
        switch (display_id)
        {
            //タイトル画面
            case GrovalConst_Gravity_Puzzle.Screen_ID.TITLE:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;//待機フェーズ
                    GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL = 1; //ステージレベルを1にする
                    break;
                }
            //ゲーム画面
            case GrovalConst_Gravity_Puzzle.Screen_ID.GAME:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE;//マップ生成フェーズ

                    //ステージの制限時間設定
                    {
                        int index = GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL - 1; //配列呼び出すインデクス
                        int time = 60; //制限時間

                        //時間が設定されている場合
                        if (index <= GrovalNum_Gravity_Puzzle.sGamePreference._Time.Length)
                            time = GrovalNum_Gravity_Puzzle.sGamePreference._Time[index];

                        //タイマーの時間を設定
                        GrovalNum_Gravity_Puzzle.sGameManager.Set_Limit_Time(time);
                    }
                    break;
                }
            //クリア画面
            case GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR:
                {
                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;//待機フェーズ
                    _Is_Judge_First = true;
                    break;
                }
        }

        Debug.Log(GrovalNum_Gravity_Puzzle.gNOW_SCREEN_ID);

        if (is_fade)
        {
            // フェードアウト
            yield return StartCoroutine(Fade(1f, 0f, fade_color)); //色→透明
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Screen_Canvas[(int)GrovalConst_Gravity_Puzzle.Screen_ID.FADE].gameObject, false);
        }
    }

    /// <summary>
    /// フェードコルーチン
    /// </summary>
    /// <param name="from">現在の画面の alpha値</param>
    /// <param name="to">最終的なの画面の alpha値</param>
    /// <param name="color">フェードする色</param>
    /// <returns>コルーチン用の IEnumerator</returns>
    private IEnumerator Fade(float from, float to, Color color)
    {
        float timer = 0f; //経過時間の初期化

        //フェード時間中ループ
        while (timer < _Fade_Speed)
        {
            float alpha = Mathf.Lerp(from, to, timer / _Fade_Speed);        //指定時間内でalpha値を補間            
            _Fade_Img.color = new Color(color.r, color.g, color.b, alpha);  //補間したalpha値を用いて色を設定(rgbはそのままでalpha値のみ変更)           
            timer += Time.deltaTime;                                        //経過時間を加算            
            yield return null;                                              //1フレーム待機
        }
        _Fade_Img.color = new Color(color.r, color.g, color.b, to);         //最終的な色を設定
    }

    /// <summary>
    /// ゲーム判定画面遷移フラグtrue
    /// </summary>
    private void Set_Is_Screen_Judge()
    {
        _Is_Screen_Judge = true;
    }

    /// <summary>
    /// ゲーム判定時処理
    /// </summary>
    private void Game_Judge_Screen()
    {
        if (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE == GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR &&
            _Is_Judge_First)
        {
            //遅延処理
            Invoke("Set_Is_Screen_Judge", GrovalNum_Gravity_Puzzle.sGamePreference._Judge_Screen_Latency);
            _Is_Judge_First = false;
        }

        if (_Is_Screen_Judge)
        {
            //表示非表示画面ID用
            GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.CLEAR;
            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;

            //画面切り替え
            if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                Screen_Change_Start(display_id, invisible_id, true);

            _Is_Screen_Judge = false;
        }
    }

    /// <summary>
    /// ボタンクリック時処理
    /// </summary>
    private void Clicked_Button()
    {
        //ボタンクリック
        for (GrovalConst_Gravity_Puzzle.Button_ID i = GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP; i <= GrovalConst_Gravity_Puzzle.Button_ID.TITLE; i++)
        {
            if (GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)i])
            {
                //表示非表示画面ID用
                GrovalConst_Gravity_Puzzle.Screen_ID display_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE, invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.NONE;

                switch (i)
                {
                    //ギブアップボタン
                    case GrovalConst_Gravity_Puzzle.Button_ID.GIVEUP:
                        {
                            GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
                            break;
                        }
                    //ネクストボタン
                    case GrovalConst_Gravity_Puzzle.Button_ID.NEXT:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;
                            break;
                        }
                    //リプレイボタン
                    case GrovalConst_Gravity_Puzzle.Button_ID.REPLAY:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            break;
                        }
                    //タイトルボタン
                    case GrovalConst_Gravity_Puzzle.Button_ID.TITLE:
                        {
                            display_id = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;
                            invisible_id = GrovalConst_Gravity_Puzzle.Screen_ID.GAME;
                            break;
                        }
                }

                //画面切り替え
                if (display_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE && invisible_id != GrovalConst_Gravity_Puzzle.Screen_ID.NONE)
                    Screen_Change_Start(display_id, invisible_id, true);

                GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)i] = false;
            }
        }
    }
}

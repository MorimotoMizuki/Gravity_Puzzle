using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common_Gravity_Puzzle;
using static Unity.Collections.AllocatorManager;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Game_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("ゲームオーバー時に表示するオブジェクト")]
    [SerializeField] private GameObject _GameOver_obj;
    [Header("ゲームクリア時に表示するオブジェクト")]
    [SerializeField] private GameObject _GameClear_obj;

    [Header("スタートボタンオブジェクト")]
    public GameObject _Start_Button_obj;

    [Header("オブジェクトのプレハブ")]
    [SerializeField] private GameObject[] _Obj_prefab;


    [Header("ブロックの生成エリア : ブロックの親オブジェクト")]
    public RectTransform _Obj_area;

    //[HideInInspector] public int _Character_cnt = 0;        //着地判定のあるキャラクターの合計数
    //[HideInInspector] public int _Character_ground_cnt = 0; //着地したキャラクターの数
    [HideInInspector] public int _Balloon_sum = 0;          //風船の合計数
    [HideInInspector] public int _Balloon_cnt = 0;          //風船の獲得数

    //ドアのフェーズ状態
    [HideInInspector] public GrovalConst_Gravity_Puzzle.Goal_Stage _Goal_Stage = GrovalConst_Gravity_Puzzle.Goal_Stage.READY; 

    [HideInInspector] public bool _Is_Flick; //フリック許可フラグ

    //キャラクターの配列
    private List<Obj_Gravity_Puzzle> _Character_list = new List<Obj_Gravity_Puzzle>();

    //マップデータ
    private List<List<int>> _StageMap = new List<List<int>>();
    //ブロックのサイズ
    [HideInInspector] public float _BlockSize = 0.0f;

    //フリック処理用
    private Vector2 _Start_touch_pos;    //フリックの始点
    private Vector2 _End_touch_pos;      //フリックの終点 
    //フリック方向ID
    [HideInInspector] public GrovalConst_Gravity_Puzzle.Flick_ID _Flick_id = GrovalConst_Gravity_Puzzle.Flick_ID.DOWN;

    //タイマー関係
    private float _Limit_time;   //制限時間
    private float _Current_time; //残り時間

    private int _Name_index = 0;

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
                    //スタートボタンフラグがtrueの場合
                    if (GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.START])
                    {
                        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE;  //ステージ生成フェーズ
                        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Start_Button_obj, false);                 //スタートボタン非表示
                        GrovalNum_Gravity_Puzzle.sClickManager._Is_Button[(int)GrovalConst_Gravity_Puzzle.Button_ID.START] = false;
                    }
                    break;
                }
            //ステージ生成フェーズ
            case GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE:
                {
                    Create_Stage(); //ステージ生成
                    break;
                }
            //ゲームプレイフェーズ
            case GrovalConst_Gravity_Puzzle.GameState.PLAYING:
                {
                    Timer(); //タイマー
                    Flick_Permit();//フリック許可判定
                    Flick(); //フリック処理
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
                    //SE再生
                    GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play_BGM_Stop(GrovalConst_Gravity_Puzzle.SE_ID.GAMEOVER);

                    GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;
                    break;
                }
        }
    }

    /// <summary>
    /// ステージ生成
    /// </summary>
    private void Create_Stage()
    {
        string index = $"stage{GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL}";

        //マップデータにステージデータがあるかチェック
        if(GrovalNum_Gravity_Puzzle.sCsvRoader._MapData.ContainsKey(index))
            _StageMap = GrovalNum_Gravity_Puzzle.sCsvRoader._MapData[index];
        //マップデータに無い場合はステージ1にする
        else
            _StageMap = GrovalNum_Gravity_Puzzle.sCsvRoader._MapData["stage1"];

        //ブロックのサイズ設定
        _BlockSize = _Obj_area.sizeDelta.x / _StageMap[0].Count;

        //オブジェクトマップ生成
        Create_Obj_Map(_StageMap);

        //初期の重力方向設定
        int index_num = GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL - 1;
        if (index_num >= GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir.Length  ||                                 //インデクスが範囲外の場合 または
            GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir[index_num] == GrovalConst_Gravity_Puzzle.Flick_ID.NONE) //フリックIDがNONEの場合
            _Flick_id = GrovalConst_Gravity_Puzzle.Flick_ID.DOWN;
        else
            _Flick_id = GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir[index_num];

        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.PLAYING;
    }

    /// <summary>
    /// ステージリセット
    /// </summary>
    public void Reset_Stage()
    {
        //ゲームクリア時に表示するオブジェクトを表示
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, false);
        //ゲームオーバー時に表示するオブジェクトを表示
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, false);

        //オブジェクトエリア内の子オブジェクトを全て削除
        foreach (Transform child in _Obj_area)
            Delete_Obj(child.gameObject);

        //キャラクターのリストの中身を全て削除
        _Character_list.Clear();

        //初期化
        _BlockSize = 0.0f;
        _Balloon_sum = 0;
        _Balloon_cnt = 0;
        _Name_index = 0;
        _Goal_Stage = GrovalConst_Gravity_Puzzle.Goal_Stage.READY;
    }

    /// <summary>
    /// オブジェクトマップ生成
    /// </summary>
    /// <param name="map">マップデータ</param>
    private void Create_Obj_Map(List<List<int>> map)
    {
        //マップの初期座標 : 左上端
        Vector2 pos = new Vector2(0 + _BlockSize / 2, 0 - _BlockSize / 2);

        for (int y = 0; y < map.Count; y++)
        {
            for (int x = 0; x < map[y].Count; x++)
            {
                int index = map[y][x];
                if(index != 0)
                {
                    GameObject obj = Instantiate(_Obj_prefab[index - 1], _Obj_area);
                    obj.GetComponent<RectTransform>().anchoredPosition = pos;
                    obj.name = $"{(GrovalConst_Gravity_Puzzle.Obj_ID)index}_{_Name_index}";

                    //着地判定をするキャラクターを計測
                    switch(index)
                    {
                        case (int)GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
                        case (int)GrovalConst_Gravity_Puzzle.Obj_ID.BOX:
                        case (int)GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL:
                            _Character_list.Add(obj.GetComponent<Obj_Gravity_Puzzle>()); //キャラクターのリスト
                        break;
                        case (int)GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
                            _Balloon_sum++;     //風船の数
                            _Character_list.Add(obj.GetComponent<Obj_Gravity_Puzzle>()); //キャラクターのリスト
                            break;
                    }

                    _Name_index++;
                }
                pos.x += _BlockSize;
            }
            pos.x = 0 + _BlockSize / 2;
            pos.y -= _BlockSize;
        }
    }

    /// <summary>
    /// オブジェクトの削除処理
    /// </summary>
    /// <param name="target_obj">対象のゲームオブジェクト</param>
    public void Delete_Obj(GameObject target_obj)
    {
        Destroy(target_obj);
    }

    /// <summary>
    /// ドアの開閉状態を判定
    /// </summary>
    public void Door_Judge()
    {
        if (_Balloon_cnt == _Balloon_sum && _Goal_Stage == GrovalConst_Gravity_Puzzle.Goal_Stage.READY)
        {
            _Goal_Stage = GrovalConst_Gravity_Puzzle.Goal_Stage.IMG_CHANGE;

            GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.DOOR_MOVE); //SE再生
        }
    }

    /// <summary>
    /// マスク画像のアルファ値の減少
    /// </summary>
    public void Dec_Mask_Alpha()
    {
        //アルファ値の幅
        float dec_alpha = GrovalNum_Gravity_Puzzle.sGamePreference._Max_Mask_Alpha - GrovalNum_Gravity_Puzzle.sGamePreference._Min_Mask_Alpha;
        //減少するアルファ値を風船の合計数で割って求める
        dec_alpha /= _Balloon_sum;
        //マスク画像のアルファ値を減少させる
        GrovalNum_Gravity_Puzzle.sImageManager.Decrement_Alpha(GrovalNum_Gravity_Puzzle.sImageManager._Mask_obj, dec_alpha);
    }

    #region フリック関係 --------------------------------------------------------------------------------------------------

    /// <summary>
    /// フリック処理
    /// </summary>
    private void Flick()
    {
        if (GrovalNum_Gravity_Puzzle.sClickManager._Is_flick_start)
        {
            //始点
            _Start_touch_pos = GrovalNum_Gravity_Puzzle.sClickManager.GetInputPosition();
            GrovalNum_Gravity_Puzzle.sClickManager._Is_flick_start = false;
        }
        if(GrovalNum_Gravity_Puzzle.sClickManager._Is_flick_end)
        {
            //終点
            _End_touch_pos = GrovalNum_Gravity_Puzzle.sClickManager.GetInputPosition();
            GrovalNum_Gravity_Puzzle.sClickManager._Is_flick_end = false;

            //始点と終点からフリック方向ベクトルを計算
            Vector2 dir = _End_touch_pos - _Start_touch_pos;

            //フリックの方向を取得
            GrovalConst_Gravity_Puzzle.Flick_ID flick_id = FlickDirection(dir);
            if (flick_id != _Flick_id && flick_id != GrovalConst_Gravity_Puzzle.Flick_ID.NONE)
            {
                GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.GRAVITY_CHANGE); //SE再生

                _Flick_id = flick_id;
                //キャラクターの着地フラグなどを初期化
                for(int i = 0; i < _Character_list.Count; i++)
                {
                    _Character_list[i]._IsGround = false;
                    _Character_list[i]._Is_first_ground = false;
                }
            }
        }
    }

    /// <summary>
    /// フリック方向の特定関数
    /// </summary>
    /// <param name="dir">フリックの方向ベクトル</param>
    /// <returns>フリックの方向を示すID</returns>
    private GrovalConst_Gravity_Puzzle.Flick_ID FlickDirection(Vector2 dir)
    {
        //ベクトルの長さが　30.0f 以下の場合は終了
        if (dir.magnitude < 30.0f)
        {
            Debug.Log("タップ判定");
            return GrovalConst_Gravity_Puzzle.Flick_ID.NONE;
        }

        //ベクトルの角度(ラジアン)を度に変換
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 4方向に分類（右：-45〜45度、上：45〜135度、左：135〜225度、下：225〜315度）
        if ((angle >= 0 && angle < 45f) || (angle >= 315f && angle < 360f))
        {
            return GrovalConst_Gravity_Puzzle.Flick_ID.RIGHT;
        }
        else if (angle >= 45f && angle < 135f)
        {
            return GrovalConst_Gravity_Puzzle.Flick_ID.UP;
        }
        else if (angle >= 135f && angle < 225f)
        {
            return GrovalConst_Gravity_Puzzle.Flick_ID.LEFT;
        }
        else // 225°～315°
        {
            return GrovalConst_Gravity_Puzzle.Flick_ID.DOWN;
        }
    }

    /// <summary>
    /// フリック許可判定
    /// </summary>
    private void Flick_Permit()
    {
        //全てのキャラクターの着地フラグがtrueの場合(削除済みのオブジェクトを除く)はフリックを許可
        for(int i = 0; i < _Character_list.Count; i++)
        {
            if (_Character_list[i]._IsGround == false && _Character_list[i] != null)
            {
                _Is_Flick = false;
                return;
            }
        }

        _Is_Flick = true;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region 時間関係 ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 制限時間設定
    /// </summary>
    /// <param name="time">制限時間</param>
    public void Set_Limit_Time(float time)
    {
        _Current_time = time;
        _Limit_time = time;
        //タイマー初期表示
        GrovalNum_Gravity_Puzzle.sImageManager._HP_Fill.fillAmount = Mathf.InverseLerp(0, _Limit_time, _Current_time);
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

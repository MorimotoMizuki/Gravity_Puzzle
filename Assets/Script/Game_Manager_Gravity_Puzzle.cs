using System.Collections.Generic;
using UnityEngine;

using Common_Gravity_Puzzle;

public class Game_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("ゲームオーバー時,ゲームクリア時に表示するオブジェクト")]
    [SerializeField] private GameObject _GameOver_obj;
    [SerializeField] private GameObject _GameClear_obj;

    [Header("スタートボタンオブジェクト")]
    public GameObject _Start_Button_obj;

    [Header("オブジェクトのプレハブ")]
    [SerializeField] private GameObject[] _Obj_prefab;

    [Header("ブロックの生成エリア : ブロックの親オブジェクト")]
    public RectTransform _Obj_area;

    [HideInInspector] public int _Balloon_sum = 0;  //風船の合計数
    [HideInInspector] public int _Balloon_cnt = 0;  //風船の獲得数

    //ドアのフェーズ状態
    [HideInInspector] public GrovalConst_Gravity_Puzzle.Door_Stage _Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.READY;

    //フリック処理用
    [HideInInspector] public bool _Is_Flick;    //フリック許可フラグ
    private Vector2 _Start_touch_pos;           //フリックの始点
    private Vector2 _End_touch_pos;             //フリックの終点 
    //フリック方向ID
    [HideInInspector] public GrovalConst_Gravity_Puzzle.Gravity_ID _Gravity_id = GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN;

    //キャラクターの配列
    private List<Obj_Gravity_Puzzle> _Character_list = new List<Obj_Gravity_Puzzle>();
    //マップデータ
    private List<List<int>> _StageMap = new List<List<int>>();
    //ブロックのサイズ
    [HideInInspector] public float _BlockSize = 0.0f;

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

        //各フェーズの処理
        switch (GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE)
        {         
            case GrovalConst_Gravity_Puzzle.GameState.CREATING_STAGE:   //ステージ生成フェーズ

                Create_Stage(); //ステージ生成
            break;
            case GrovalConst_Gravity_Puzzle.GameState.PLAYING:          //ゲームプレイフェーズ

                Timer();        //タイマー
                Flick_Permit(); //フリック許可判定
                Flick();        //フリック処理
            break;          
            case GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR:        //ゲームクリアフェーズ             

                GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameClear_obj, true);     //ゲームクリア時に表示するオブジェクトを表示
            break;          
            case GrovalConst_Gravity_Puzzle.GameState.GAMEOVER:         //ゲームオーバーフェーズ             

                GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_GameOver_obj, true);      //ゲームオーバー時に表示するオブジェクトを表示
                GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play_BGM_Stop(GrovalConst_Gravity_Puzzle.SE_ID.GAMEOVER); //SE再生
                GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.READY;               //待機フェーズへ
            break;
        }
    }

    #region ステージ関係 --------------------------------------------------------------------------------------------------

    /// <summary>
    /// ステージ生成
    /// </summary>
    private void Create_Stage()
    {
        string index = $"stage{GrovalNum_Gravity_Puzzle.gNOW_STAGE_LEVEL}";
        //マップデータにステージデータがあるかチェック
        if (GrovalNum_Gravity_Puzzle.sCsvRoader._MapData.ContainsKey(index))
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
        //インデクスが範囲外の場合 または フリックIDがNONEの場合
        if (index_num >= GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir.Length ||  
            GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir[index_num] == GrovalConst_Gravity_Puzzle.Gravity_ID.NONE)
            _Gravity_id = GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN;
        else
            _Gravity_id = GrovalNum_Gravity_Puzzle.sGamePreference._First_Gravity_Dir[index_num];

        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.PLAYING; //ゲームプレイフェーズへ
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
                if (index != 0)
                {
                    //オブジェクト生成
                    GameObject obj = Instantiate(_Obj_prefab[index - 1], _Obj_area);
                    //座標設定
                    obj.GetComponent<RectTransform>().anchoredPosition = pos;
                    //名前設定
                    obj.name = $"{(GrovalConst_Gravity_Puzzle.Obj_ID)index}";

                    //キャラクターを計測
                    switch (index)
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
                }
                pos.x += _BlockSize;
            }
            pos.x = 0 + _BlockSize / 2;
            pos.y -= _BlockSize;
        }
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
        _Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.READY;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

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
            GrovalConst_Gravity_Puzzle.Gravity_ID flick_id = FlickDirection(dir);
            if (flick_id != _Gravity_id && flick_id != GrovalConst_Gravity_Puzzle.Gravity_ID.NONE)
            {
                GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.GRAVITY_CHANGE); //SE再生

                _Gravity_id = flick_id;
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
    private GrovalConst_Gravity_Puzzle.Gravity_ID FlickDirection(Vector2 dir)
    {
        //ベクトルの長さが　30.0f 以下の場合は終了
        if (dir.magnitude < 30.0f)
            return GrovalConst_Gravity_Puzzle.Gravity_ID.NONE;

        //ベクトルの角度(ラジアン)を度に変換
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 4方向に分類（右：-45〜45度、上：45〜135度、左：135〜225度、下：225〜315度）
        if ((angle >= 0 && angle < 45f) || (angle >= 315f && angle < 360f))
            return GrovalConst_Gravity_Puzzle.Gravity_ID.RIGHT;
        else if (angle >= 45f && angle < 135f)
            return GrovalConst_Gravity_Puzzle.Gravity_ID.UP;
        else if (angle >= 135f && angle < 225f)
            return GrovalConst_Gravity_Puzzle.Gravity_ID.LEFT;
        else
            return GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN;
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
        //時間計測
        _Current_time -= Time.deltaTime;
        //タイマーが 0 以下になった場合
        if (_Current_time <= 0.0f)
        {
            _Current_time = 0.0f;
            //ゲームオーバー
            GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
            return true;
        }
        //タイマー(UI)に反映
        GrovalNum_Gravity_Puzzle.sImageManager._HP_Fill.fillAmount = Mathf.InverseLerp(0, _Limit_time, _Current_time);
        return false;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// オブジェクトの削除処理
    /// </summary>
    /// <param name="target_obj">対象のゲームオブジェクト</param>
    public void Delete_Obj(GameObject target_obj)
    {
        Destroy(target_obj);
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

    /// <summary>
    /// ドアの開閉状態を判定
    /// </summary>
    public void Door_Judge()
    {
        //風船の獲得数が風船の合計数が等しい場合 かつ 待機フェーズの場合
        if (_Balloon_cnt == _Balloon_sum && _Goal_Stage == GrovalConst_Gravity_Puzzle.Door_Stage.READY)
        {
            _Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.IMG_CHANGE; //ゴール時のフェーズ状態 を 画像変更フェーズへ
            GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.DOOR_MOVE); //SE再生
        }
    }
}

using System.Drawing;
using Common_Gravity_Puzzle;
using UnityEngine;
using UnityEngine.UI;

public class Obj_Gravity_Puzzle : MonoBehaviour
{
    #region 共通 ----------------------------------------------------------------------------------------------------------

    private Image           _Img;       //画像情報
    private BoxCollider2D   _Collider;  //コライダー情報
    private RectTransform   _Rect;      //RectTransform情報
    private Rigidbody2D     _Rigid2D;   //Rigidbody2D情報                                      
    private Camera          _MainCamera;//カメラ

    //オブジェクトID
    private GrovalConst_Gravity_Puzzle.Obj_ID _Obj_ID;
    //現在の重力の向き
    private GrovalConst_Gravity_Puzzle.Gravity_ID _Now_gravity_id = GrovalConst_Gravity_Puzzle.Gravity_ID.NONE;
    //重力の方向ベクトル
    private Vector2 _Gravity_Dir;
    //移動ベクトル
    private Vector2 _Move_vec = new Vector2();

    #endregion ------------------------------------------------------------------------------------------------------------

    #region キャラクター --------------------------------------------------------------------------------------------------

    [Header("判定範囲")]
    [SerializeField] private float _CheckDistance = 0.4f;
    [SerializeField] private float _AddRayDistance = 0.4f;

    [Header("地面レイヤー")]
    [SerializeField] private LayerMask _GroundLayer;
    [Header("当たり判定レイヤー")]
    [SerializeField] private LayerMask _HitLayer;

    [HideInInspector] public bool _IsGround;                //着地フラグ
    private bool _PrevIsGround = false;                     //着地判定用
    [HideInInspector] public bool _Is_first_ground = true;  //初期着地判定

    //アニメーションカウント
    private int _Anim_cnt = 0;
    //アニメーション画像インデクス
    private int _Anim_index = 0;

    private enum Player_Stage
    {
        READY, GOAL, CRASH, SPIKE, END,
    }
    //プレイヤーのフェーズ状態
    private Player_Stage _Player_Stage = Player_Stage.READY;

    #endregion ------------------------------------------------------------------------------------------------------------

    #region ブロック ------------------------------------------------------------------------------------------------------
    private enum Block_State
    {
        READY, ROTATION, IMG_CHANGE,
    }
    //ブロックのフェーズ状態
    private Block_State _Block_State = Block_State.READY;

    [Header("矢印ブロック")]
    [SerializeField] private Image _Block_Arrow;

    private float _Arrow_RotSpeed = 0.0f; //矢印の回転スピード

    private bool _Is_setting = false; //設定フラグ
    #endregion ------------------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        //オブジェクトID設定
        _Obj_ID = Obj_Identification();

        //オブジェクトの各情報を取得
        _Img        = GetComponent<Image>();
        _Collider   = GetComponent<BoxCollider2D>();
        _Rect       = GetComponent<RectTransform>();
        _Rigid2D    = GetComponent<Rigidbody2D>();
        _MainCamera = Camera.main; //カメラ取得
    }

    // Update is called once per frame
    void Update()
    {
        //画面外処理
        if (Out_Screen(gameObject))
            Out_Screen_Move();

        //各オブジェクトの処理
        switch (_Obj_ID)
        {
            //プレイヤー
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Player_Normal_img[0], false); //オブジェクトの詳細設定
                Gravity_Move(_Rigid2D, false);  //重力処理
                Player_Move(); //プレイヤーの処理
            break;
            //風船
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Balloon_Normal_img[0], false); //オブジェクトの詳細設定
                Gravity_Move(_Rigid2D, true); //重力処理
                Balloon_Move(); //風船の処理
            break;
            //ブロック
            case GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK:
               
                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._BlockBase_Img, true); //オブジェクトの詳細設定
                Block_Move(); //ブロックの処理
            break;
            //ドア
            case GrovalConst_Gravity_Puzzle.Obj_ID.DOOR:
               
                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Door_img[0], true); //オブジェクトの詳細設定
                Door_Move(); //ドア処理
            break;
            //箱
            case GrovalConst_Gravity_Puzzle.Obj_ID.BOX:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Box_img, false); //オブジェクトの詳細設定
                Gravity_Move(_Rigid2D, false); //重力処理
            break;
            //トゲボール
            case GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._SpikeBall_img, false); //オブジェクトの詳細設定
                Gravity_Move(_Rigid2D, false); //重力処理
            break;
            //右向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.RIGHT_SPIKE:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Spile_img[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.RIGHT]   , false);
            break;
            //左向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.LEFT_SPIKE:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Spile_img[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.LEFT]    , false);
            break;
            //上向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.UP_SPIKE:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Spile_img[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.UP]      , false);
            break;
            //下向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.DOWN_SPIKE:

                Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Spile_img[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN]    , false);
            break;
        }
    }

    #region 各オブジェクトの処理 ------------------------------------------------------------------------------------------

    /// <summary>
    /// プレイヤーの処理
    /// </summary>
    private void Player_Move()
    {
        if (_IsGround && !_PrevIsGround) //着地した瞬間に判定
            Hit_Ray_Judge(_Gravity_Dir); //レイキャストの当たり判定
        _PrevIsGround = _IsGround;

        if (!_IsGround)
            Hit_Balloon(_Gravity_Dir);

        switch (_Player_Stage)
        {
            case Player_Stage.READY:
            {
                if (_IsGround)  //常時アニメーション処理 : 通常時
                    Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Normal_img, GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt);
                else            //常時アニメーション処理 : 落下時
                    Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Fall_img, GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt);
                break;
            }
            case Player_Stage.GOAL:
            {
                Door_Move(); //ドアの処理
                break;
            }
            case Player_Stage.CRASH:
            {
                if (Short_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Crash_img, GrovalNum_Gravity_Puzzle.sGamePreference._Character_CrashAnim_Change_cnt))
                    MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID.HIT_BOX); //キャラクター死亡時処理
                break;
            }
            case Player_Stage.SPIKE:
            {
                if (Short_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Spike_img, GrovalNum_Gravity_Puzzle.sGamePreference._Character_CrashAnim_Change_cnt))
                    MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID.HIT_SPIKE); //キャラクター死亡時処理
                break;
            }
        }
    }

    /// <summary>
    /// 風船の処理
    /// </summary>
    private void Balloon_Move()
    {
        Hit_Ray_Judge(_Gravity_Dir); //レイキャストの当たり判定
        //常時アニメーション処理 : 通常時
        Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Balloon_Normal_img, GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt);
    }

    /// <summary>
    /// ブロックの処理
    /// </summary>
    private void Block_Move()
    {
        switch (_Block_State)
        {
            //待機フェーズ
            case Block_State.READY:
            {
                //フリックが行われた場合
                if (_Now_gravity_id != GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id)
                {
                    //画像変更 : ブロックのベース画像
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._BlockBase_Img);
                    //矢印ブロックの表示
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, true);
                    //角度設定
                    _Block_Arrow.transform.eulerAngles = new Vector3(0.0f, 0.0f, GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)_Now_gravity_id]);

                    //回転の幅が90度を超えている場合 : 回転スピードを2倍にする
                    if (Mathf.DeltaAngle(_Block_Arrow.transform.eulerAngles.z, GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id]) > 90.0f)
                        _Arrow_RotSpeed = GrovalNum_Gravity_Puzzle.sGamePreference._BlockArrow_RotSpeed * 2;
                    else
                        _Arrow_RotSpeed = GrovalNum_Gravity_Puzzle.sGamePreference._BlockArrow_RotSpeed;

                    _Block_State = Block_State.ROTATION; //回転フェーズへ
                }
                break;
            }
            //回転フェーズ
            case Block_State.ROTATION:
            {
                float currentAngle = _Block_Arrow.transform.eulerAngles.z;                                                          //現在の角度                    
                float targetAngle = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id];   //目的の角度                   
                float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);                                                      //最短角度差                   
                float dir = Mathf.Sign(angleDiff);//回転方向（+なら左回転、-なら右回転）

                //回転処理
                float rot_frame = _Arrow_RotSpeed * Time.deltaTime * dir;
                _Block_Arrow.transform.Rotate(0f, 0f, rot_frame);

                //一定範囲以内になったら完了とみなす（例：1度以内）
                if (Mathf.Abs(angleDiff) < GrovalConst_Gravity_Puzzle.ARROW_ROt_COMPLETE_THRSHOLD)
                {
                    //角度をピタッと揃える（端数を切る）
                    Vector3 fixedAngle = _Block_Arrow.transform.eulerAngles;
                    fixedAngle.z = targetAngle;
                    _Block_Arrow.transform.eulerAngles = fixedAngle;
                    //画像変更フェーズへ
                    _Block_State = Block_State.IMG_CHANGE;
                    //重力の向き更新
                    if (GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id != GrovalConst_Gravity_Puzzle.Gravity_ID.NONE)
                        _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id;
                }
                break;
            }
            //画像変更フェーズ
            case Block_State.IMG_CHANGE:
            {
                //矢印ブロックの非表示
                GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, false);
                //画像変更 : 上下左右ブロックの画像
                GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._BlockDir_img[(int)_Now_gravity_id]);
                _Block_State = Block_State.READY; //待機フェーズへ
                break;
            }
        }
    }

    /// <summary>
    /// ドアの処理
    /// </summary>
    private void Door_Move()
    {
        switch (GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage)
        {
            case GrovalConst_Gravity_Puzzle.Door_Stage.IMG_CHANGE:
            {
                if (_Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.DOOR)
                {
                    //ドアの画像変更 : 開いた画像に変更
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Door_img[1]);
                }
                else if (_Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER)
                {
                    GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.PLAYER_IN;
                }
                break;
            }
            case GrovalConst_Gravity_Puzzle.Door_Stage.PLAYER_IN:
            {
                if (_Obj_ID != GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER)
                    break;

                _Anim_cnt++;
                if (_Anim_cnt == 10)
                {
                    //プレイヤーの削除
                    GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject);
                    _Anim_cnt = 0;
                    GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.DOOR_CLOSE;
                }
                break;
            }
            case GrovalConst_Gravity_Puzzle.Door_Stage.DOOR_CLOSE:
            {
                if (_Obj_ID != GrovalConst_Gravity_Puzzle.Obj_ID.DOOR)
                    break;

                _Anim_cnt++;
                if (_Anim_cnt == 20)
                {
                    //ドアの画像変更 : 閉じた画像に変更
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Door_img[0]);
                    GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.CLEAR;
                    _Anim_cnt = 0;
                }

                break;
            }
            case GrovalConst_Gravity_Puzzle.Door_Stage.CLEAR:
            {
                //ゲームクリア判定
                GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR;

                GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.DOOR_MOVE); //SE再生
                GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage = GrovalConst_Gravity_Puzzle.Door_Stage.END;
                break;
            }
        }
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region オブジェクトの詳細設定 ----------------------------------------------------------------------------------------

    /// <summary>
    /// オブジェクトの識別
    /// </summary>
    /// <returns>オブジェクトID</returns>
    private GrovalConst_Gravity_Puzzle.Obj_ID Obj_Identification()
    {
        //Name_to_Obj_ID: 文字列（部分一致用）とオブジェクトIDのマッピング辞書
        foreach (var pair in GrovalConst_Gravity_Puzzle.Name_to_Obj_ID)
        {
            //ゲームオブジェクトの名前に、辞書のキー（判別用文字列）が含まれているか確認
            if (gameObject.name.Contains(pair.Key))
            {
                //一致した場合、対応するオブジェクトIDを返す
                return pair.Value;
            }
        }

        //どのキーとも一致しない場合、NONEを返す（未識別の意味）
        return GrovalConst_Gravity_Puzzle.Obj_ID.NONE;
    }

    /// <summary>
    /// オブジェクトの詳細設定
    /// </summary>
    /// <param name="is_collider_size">コライダーのサイズ変更フラグ</param>
    /// <param name="size">オブジェクトサイズ : デフォルトでGameManagerの_BlockSize</param>
    private void Obj_Setting(Sprite img, bool is_collider_size, Vector2 size = default)
    {
        //ブロックのサイズが未設定では無い場合 かつ 設定フラグがfalse の場合
        if (GrovalNum_Gravity_Puzzle.sGameManager._BlockSize == 0.0f ||
            _Is_setting)
            return;

        if (size == default)
        {
            size = new Vector2(
                GrovalNum_Gravity_Puzzle.sGameManager._BlockSize,
                GrovalNum_Gravity_Puzzle.sGameManager._BlockSize
            );
        }
        //オブジェクトサイズを設定
        _Rect.sizeDelta = new Vector2(size.x, size.y);
        //コライダーのサイズを設定
        if (is_collider_size)
            _Collider.size = new Vector2(size.x, size.y);

        //画像の初期設定
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, img);

        //重力の向き更新
        if (GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id != GrovalConst_Gravity_Puzzle.Gravity_ID.NONE)
            _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id;

        switch (_Obj_ID)
        {
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
            case GrovalConst_Gravity_Puzzle.Obj_ID.BOX:
            case GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL:
                {
                    transform.SetAsLastSibling();//最前面に表示
                    break;
                }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK:
                {
                    //矢印ブロックを非表示にする
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, false);
                    //画像変更 : 上下左右ブロックの画像
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._BlockDir_img[(int)_Now_gravity_id]);
                    break;
                }
        }
        _Is_setting = true;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region 当たり判定 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// 当たり判定
    /// </summary>
    /// <param name="collision">衝突側のコライダー</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (_Obj_ID)
        {
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
            {
                //風船との衝突
                if (collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.BALLOON]))
                {
                    //風船の獲得数増やす
                    GrovalNum_Gravity_Puzzle.sGameManager._Balloon_cnt++;
                    //風船の削除
                    GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(collision.gameObject);

                    GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BALLOON_GET); //SE再生
                    GrovalNum_Gravity_Puzzle.sGameManager.Door_Judge();     //ドアの開閉判定
                    GrovalNum_Gravity_Puzzle.sGameManager.Dec_Mask_Alpha(); //マスク画像のアルファ値を減少させる
                }
                //ドアとの衝突 : 開いている状態のドア
                if (collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.DOOR]) && 
                    GrovalNum_Gravity_Puzzle.sGameManager._Goal_Stage == GrovalConst_Gravity_Puzzle.Door_Stage.IMG_CHANGE &&
                    _Player_Stage == Player_Stage.READY)
                {
                    _Player_Stage = Change_Animation(Player_Stage.GOAL); //アニメーション変更時処理
                }
                //箱との当たり判定
                else if (collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.BOX_DIED]) &&
                         _Player_Stage == Player_Stage.READY)
                {
                    if (_IsGround)
                        _Player_Stage = Change_Animation(Player_Stage.CRASH); //アニメーション変更時処理
                }
                //スパイクボールとトゲの当たり判定
                else if ((collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_BALL]) ||
                          collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_DIR])) &&
                          _Player_Stage == Player_Stage.READY)
                {
                    if (_IsGround)
                        _Player_Stage = Change_Animation(Player_Stage.SPIKE); //アニメーション変更時処理
                }
                break;
            }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
            {
                //箱との当たり判定
                if (collision.gameObject.layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.BOX_DIED]))
                    MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID.HIT_BOX); //キャラクター死亡時処理
                break;
            }
        }
    }

    /// <summary>
    /// レイキャストの当たり判定
    /// </summary>
    /// <param name="dir">重力の方向</param>
    private void Hit_Ray_Judge(Vector2 dir)
    {
        switch (_Obj_ID)
        {
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
                {
                    RaycastHit2D hit_down = Physics2D.Raycast(gameObject.transform.position, dir, _CheckDistance, _HitLayer);
                    if (hit_down.collider != null && hit_down.collider.gameObject != this.gameObject)
                    {
                        int layer = hit_down.collider.gameObject.layer;
                        //スパイクボールとトゲ
                        if (layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_BALL]) ||
                            layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_DIR]))
                            _Player_Stage = Change_Animation(Player_Stage.SPIKE); //アニメーション変更時処理
                    }
                    Vector2 inv_dir = -dir;
                    RaycastHit2D hit_up = Physics2D.Raycast(gameObject.transform.position, inv_dir, _CheckDistance, _HitLayer);
                    if (hit_up.collider != null && hit_up.collider.gameObject != this.gameObject)
                    {
                        int layer = hit_up.collider.gameObject.layer;
                        //箱の下
                        if (layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.BOX_DIED]))
                            _Player_Stage = Change_Animation(Player_Stage.CRASH); //アニメーション変更時処理
                    }
                    break;
                }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
                {
                    RaycastHit2D hit_down = Physics2D.Raycast(gameObject.transform.position, dir, _AddRayDistance, _HitLayer);
                    if (hit_down.collider != null && hit_down.collider.gameObject != this.gameObject)
                    {
                        int layer = hit_down.collider.gameObject.layer;
                        //スパイクボールとトゲ
                        if (layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_BALL]) ||
                            layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.SPIKE_DIR]))
                            MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID.HIT_SPIKE); //キャラクター死亡時処理
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// 風船とプレイヤーの当たり判定 : レイキャスト
    /// </summary>
    /// <param name="dir">重力の方向</param>
    private void Hit_Balloon(Vector2 dir)
    {
        RaycastHit2D hit_down = Physics2D.Raycast(gameObject.transform.position, dir, _CheckDistance, _HitLayer);
        if (hit_down.collider != null && hit_down.collider.gameObject != this.gameObject)
        {
            int layer = hit_down.collider.gameObject.layer;
            if (layer == LayerMask.NameToLayer(GrovalConst_Gravity_Puzzle.Layer_Name[GrovalConst_Gravity_Puzzle.Layer_ID.BALLOON]))
            {
                //風船の獲得数増やす
                GrovalNum_Gravity_Puzzle.sGameManager._Balloon_cnt++;
                //風船の削除
                GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(hit_down.collider.gameObject);

                GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play(GrovalConst_Gravity_Puzzle.SE_ID.BALLOON_GET); //SE再生
                GrovalNum_Gravity_Puzzle.sGameManager.Door_Judge();     //ドアの開閉判定
                GrovalNum_Gravity_Puzzle.sGameManager.Dec_Mask_Alpha(); //マスク画像のアルファ値を減少させる
            }
        }
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region 着地処理 ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 着地判定
    /// </summary>
    /// <returns>着地フラグ</returns>
    private void Ground_Check(Vector2 dir)
    {
        //足元にRayを飛ばして当たったレイヤーが Ground の場合 : 着地
        Vector3 base_pos = transform.position;
        base_pos.x += dir.x * _AddRayDistance;
        base_pos.y += dir.y * _AddRayDistance;

        RaycastHit2D hit = Physics2D.Raycast(base_pos, dir, _CheckDistance, _GroundLayer);
        if (hit.collider != null && hit.collider.gameObject != this.gameObject)
        {
            if (!_IsGround)
            {
                //位置調整
                RectTransform ground_rect = hit.collider.gameObject.GetComponent<RectTransform>(); //当たった側のRectTransformを取得
                Vector2 ground_pos = ground_rect.localPosition;         //ローカル座標を代入
                ground_pos.y += ground_rect.rect.height * (dir.y * -1); //座標に画像幅 * 重力方向の逆 で落下座標を計算
                ground_pos.x += ground_rect.rect.width * (dir.x * -1);
                _Rect.transform.localPosition = ground_pos;             //落下座標に強制移動

                //衝突したオブジェクトの情報を取得
                Obj_Gravity_Puzzle hit_obj = hit.collider.gameObject.GetComponent<Obj_Gravity_Puzzle>();

                //着地フラグがあるオブジェクト同士の判定
                if (Ground_Obj_Judge(this, hit_obj))
                {
                    //当たったBOXの着地フラグがtrueの場合
                    if (hit_obj._IsGround == true)
                    {
                        Ground_Process(); //着地時処理
                    }
                }
                else
                    Ground_Process(); //着地時処理
            }
        }
        else
            _IsGround = false;
    }

    /// <summary>
    /// 着地フラグがあるオブジェクト同士の判定
    /// </summary>
    /// <param name="this_obj">オブジェクト</param>
    /// <param name="hit_obj">衝突したオブジェクト</param>
    /// <returns>判定の可否</returns>
    private bool Ground_Obj_Judge(Obj_Gravity_Puzzle this_obj, Obj_Gravity_Puzzle hit_obj)
    {
        //BOX, SPIKE_BALL
        if (this_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BOX || this_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL)
        {
            //BOX, SPIKE_BALL
            if (hit_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BOX || hit_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL)
            {
                return true;
            }
        }
        //BALLOON, BALLOON
        else if (this_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON &&
           hit_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON)
        {
            return true;
        }
        else if (this_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER &&
                (hit_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BOX ||
                 hit_obj._Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 着地時処理
    /// </summary>
    private void Ground_Process()
    {
        if (!_Is_first_ground)
            return;

        _Rigid2D.velocity = Vector2.zero; //完全に止める
        _Rigid2D.constraints = RigidbodyConstraints2D.FreezeAll;//全て固定

        if (_Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER && _Player_Stage != Player_Stage.GOAL)
            Change_Animation(_Player_Stage, GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt);

        _IsGround = true;
        _Is_first_ground = false;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region 範囲外処理 ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// オブジェクトの画面外判定
    /// </summary>
    /// <param name="target_obj">ゲームオブジェクト</param>
    /// <returns></returns>
    private bool Out_Screen(GameObject target_obj)
    {
        //targetUI（RectTransform）のワールド座標をカメラのビューポート座標に変換
        //ビューポート座標は (0,0) が画面左下、(1,1) が画面右上を示す
        Vector3 viewport_pos = _MainCamera.WorldToViewportPoint(target_obj.transform.position);

        //ビューポート座標が 0～1 の範囲外であれば、画面外にあると判定
        if (viewport_pos.x < 0 || viewport_pos.x > 1 || viewport_pos.y < 0 || viewport_pos.y > 1)
            return true;    //画面外
        else
            return false;   //画面内
    }

    /// <summary>
    /// オブジェクトの画面外処理
    /// </summary>
    private void Out_Screen_Move()
    {
        //プレイヤー, 風船の場合
        if (_Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER || _Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON)
            MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID.GAMEOVER); //キャラクター死亡時処理
        else
            GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject); //オブジェクト削除
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    #region アニメーション ------------------------------------------------------------------------------------------------

    /// <summary>
    /// アニメーション変更時の変数のリセットなど
    /// </summary>
    /// <returns>対象のフラグ</returns>
    private Player_Stage Change_Animation(Player_Stage stage, int anim_cnt = default)
    {
        if (anim_cnt == default)
            anim_cnt = 0;

        _Anim_cnt   = anim_cnt;
        _Anim_index = 0;
        return stage;
    }

    /// <summary>
    /// 常時アニメーション処理
    /// </summary>
    /// <param name="target_img">対象の画像オブジェクト</param>
    /// <param name="change_img">アニメーションするSprite配列</param>
    private void Normal_Animation(Image target_img, Sprite[] change_img, int anim_change_frame)
    {
        _Anim_cnt++;
        if (_Anim_cnt > anim_change_frame)
        {
            //画像変更
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(target_img, change_img[_Anim_index]);

            //インデクス設定
            if (_Anim_index < change_img.Length - 1)
                _Anim_index++;
            else
                _Anim_index = 0;
            _Anim_cnt = 0;
        }
    }

    /// <summary>
    /// 一回だけ再生されるアニメーション処理
    /// </summary>
    /// <param name="target_img"></param>
    /// <param name="change_img"></param>
    private bool Short_Animation(Image target_img, Sprite[] change_img, int anim_change_frame)
    {
        _Anim_cnt++;
        if (_Anim_cnt > anim_change_frame)
        {
            //画像変更
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(target_img, change_img[_Anim_index]);

            //インデクス設定
            if (_Anim_index < change_img.Length - 1)
                _Anim_index++;
            else
            {
                return true;
            }
            _Anim_cnt = 0;
        }
        return false;
    }

    #endregion ------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 重力処理
    /// </summary>
    /// <param name="rigid">対象のRigidbody2D</param>
    /// <param name="is_reverse">反転フラグ</param>
    private void Gravity_Move(Rigidbody2D rigid, bool is_reverse)
    {
        if (_Now_gravity_id != GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id || _Is_first_ground)
        {
            //重力の向き更新
            if (GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id != GrovalConst_Gravity_Puzzle.Gravity_ID.NONE)
                _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Gravity_id;
            //角度
            Vector3 angle = transform.eulerAngles;
            switch (_Now_gravity_id)
            {
                case GrovalConst_Gravity_Puzzle.Gravity_ID.RIGHT:
                    _Move_vec = Vector2.right;
                    angle.z = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.LEFT];    //重力と逆方向
                break;
                case GrovalConst_Gravity_Puzzle.Gravity_ID.LEFT:
                    _Move_vec = Vector2.left;
                    angle.z = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.RIGHT];   //重力と逆方向
                break;
                case GrovalConst_Gravity_Puzzle.Gravity_ID.UP:
                    _Move_vec = Vector2.up;
                    angle.z = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN];    //重力と逆方向
                break;
                case GrovalConst_Gravity_Puzzle.Gravity_ID.DOWN:
                    _Move_vec = Vector2.down;
                    angle.z = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalConst_Gravity_Puzzle.Gravity_ID.UP];      //重力と逆方向
                break;
            }

            //角度変更
            transform.eulerAngles = angle;
            //反転フラグがtrueの場合はベクトルを反転
            if (is_reverse)
            {
                _Move_vec.x *= -1;
                _Move_vec.y *= -1;
            }

            //重力方向ベクトル
            _Gravity_Dir = _Move_vec;
            _Gravity_Dir = _Gravity_Dir.normalized; //正規化

            //回転だけ固定
            _Rigid2D.constraints = RigidbodyConstraints2D.FreezeRotation;

            //着地フラグ初期化
            _IsGround = false;
            _Is_first_ground = true;
        }

        //着地判定
        Ground_Check(_Gravity_Dir);

        //空中判定の場合
        if (!_IsGround)
        {
            _Move_vec *= GrovalNum_Gravity_Puzzle.sGamePreference._Gravity_Speed;
            //一定方向力を加える
            rigid.AddForce(_Move_vec, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// キャラクター死亡時処理
    /// </summary>
    private void MainCharacter_Died(GrovalConst_Gravity_Puzzle.SE_ID  se_id = default)
    {
        //削除
        GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject);
        //ゲームオーバー
        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;

        if (se_id == default)
            return;

        GrovalNum_Gravity_Puzzle.sMusicManager.SE_Play_BGM_Stop(se_id); //SE再生
    }
}

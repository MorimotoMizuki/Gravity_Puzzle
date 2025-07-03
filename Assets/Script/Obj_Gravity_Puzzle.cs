using System.Collections;
using System.Collections.Generic;
using Common_Gravity_Puzzle;
using UnityEngine;
using UnityEngine.UI;

public class Obj_Gravity_Puzzle : MonoBehaviour
{
    private Image _Img;
    private BoxCollider2D _Collider;
    private RectTransform _Rect;
    private Rigidbody2D _Rigid2D;

    [Header("地面判定用")]
    [SerializeField] private Transform _GroundCheck;
    [Header("判定範囲")]
    [SerializeField] private float _CheckDistance = 0.1f;
    [Header("地面レイヤー")]
    [SerializeField] private LayerMask _GroundLayer;

    //着地フラグ
    [HideInInspector] public bool _IsGround;
    private bool _Is_first_ground = true;

    //オブジェクト識別辞書
    private static readonly Dictionary<string, GrovalConst_Gravity_Puzzle.Obj_ID> _Name_to_Obj_ID
    = new Dictionary<string, GrovalConst_Gravity_Puzzle.Obj_ID>
    {
        { "PLAYER", GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER },
        { "BALLOON", GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON },
        { "BLOCK", GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK },
        { "DOOR", GrovalConst_Gravity_Puzzle.Obj_ID.DOOR },
        { "BOX", GrovalConst_Gravity_Puzzle.Obj_ID.BOX },
        { "SPIKE_BALL", GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL },
        { "RIGHT_SPIKE", GrovalConst_Gravity_Puzzle.Obj_ID.RIGHT_SPIKE },
        { "LEFT_SPIKE", GrovalConst_Gravity_Puzzle.Obj_ID.LEFT_SPIKE },
        { "UP_SPIKE", GrovalConst_Gravity_Puzzle.Obj_ID.UP_SPIKE },
        { "DOWN_SPIKE", GrovalConst_Gravity_Puzzle.Obj_ID.DOWN_SPIKE },
    };

    //オブジェクトID
    private GrovalConst_Gravity_Puzzle.Obj_ID _Obj_ID;
    //現在の重力の向き
    private GrovalConst_Gravity_Puzzle.Flick_ID _Now_gravity_id = GrovalConst_Gravity_Puzzle.Flick_ID.NONE;
    //重力の方向ベクトル
    private Vector2 _Gravity_Dir;
    //移動ベクトル
    private Vector2 _Move_vec = new Vector2();
    //ゲームマネージャーに通達する着地カウントフラグ : 重力変更ごとにリセット
    private bool _Is_ground_first = true;
    //アニメーションカウント
    private int _Anim_cnt = 0;
    //アニメーション画像インデクス
    private int _Anim_index = 0;

    #region プレイヤー ----------------------------------------------------------------------------------------------------

    private bool _Is_Goal = false;
    private bool _Is_Crash = false;

    #endregion ------------------------------------------------------------------------------------------------------------

    #region ブロック --------------------------------------------------------------------------------------------------
    private enum Block_State
    {
        READY, ROtATION, IMG_CHANGE,
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

        _Img = GetComponent<Image>();
        _Collider = GetComponent<BoxCollider2D>();
        _Rect = GetComponent<RectTransform>();
        _Rigid2D = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (_Obj_ID)
        {
            //プレイヤー
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
                {              
                    if (Out_Screen(gameObject)) //画面外に出た場合 : ゲームオーバー
                    {
                        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
                        GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject); //プレイヤー削除
                    }

                    Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Player_Normal_img[0], false); //オブジェクトの詳細設定
                    Gravity_Move(_Rigid2D, false); //重力処理

                    if(_Is_Crash)
                    {
                        Crash_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Crash_img);
                    }
                    else
                    {
                        if (_IsGround)
                            Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Normal_img);  //常時アニメーション処理 : 通常時
                        else
                            Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Player_Fall_img);    //常時アニメーション処理 : 落下時
                    }


                    break;
                }
            //風船
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
                {                    
                    if (Out_Screen(gameObject)) //画面外に出た場合 : ゲームオーバー
                    {
                        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
                        GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject); //風船削除
                    }
                    Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Balloon_Normal_img[0], false); //オブジェクトの詳細設定
                    Gravity_Move(_Rigid2D, true);  //重力処理

                    Normal_Animation(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Balloon_Normal_img); //常時アニメーション処理 : 通常時

                    break;
                }
            //ブロック
            case GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK:
                {
                    Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._BlockBase_Img, true); //オブジェクトの詳細設定
                    Block_Move();
                    break;
                }
            //ドア
            case GrovalConst_Gravity_Puzzle.Obj_ID.DOOR:
                {
                    Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Door_Normal_img, true); //オブジェクトの詳細設定

                    if(GrovalNum_Gravity_Puzzle.sGameManager._Is_Open_Door)
                    {
                        //ドアの画像変更 : 開いた画像に変更
                        GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Door_Open_img);
                    }

                    break;
                }
            //箱
            case GrovalConst_Gravity_Puzzle.Obj_ID.BOX:
                {
                    if (Out_Screen(gameObject)) //画面外に出た場合 : ゲームオーバー
                    {
                        GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMEOVER;
                        GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject); //箱削除
                    }

                    Obj_Setting(GrovalNum_Gravity_Puzzle.sImageManager._Box_img, false); //オブジェクトの詳細設定
                    Gravity_Move(_Rigid2D, false); //重力処理
                    break;
                }
            //トゲボール
            case GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL:
                {
                    //Obj_Setting(true); //オブジェクトの詳細設定
                    break;
                }
            //右向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.RIGHT_SPIKE:
                {
                    break;
                }
            //左向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.LEFT_SPIKE:
                {
                    break;
                }
            //上向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.UP_SPIKE:
                {
                    break;
                }
            //下向きのトゲ
            case GrovalConst_Gravity_Puzzle.Obj_ID.DOWN_SPIKE:
                {
                    break;
                }
        }
    }

    /// <summary>
    /// 当たり判定
    /// </summary>
    /// <param name="collision">衝突側のコライダー</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイヤー以外は終了
        if (_Obj_ID != GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER) return;

        //風船との衝突
        if (collision.gameObject.name.Contains("BALLOON"))
        {
            //風船の削除
            GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(collision.gameObject);
            //着地判定のあるキャラクターの合計数を減らす
            GrovalNum_Gravity_Puzzle.sGameManager._Character_cnt--;
            //風船の獲得数増やす
            GrovalNum_Gravity_Puzzle.sGameManager._Balloon_cnt++;
            return;
        }
        //ドアとの衝突 : 開いている状態のドア
        if (collision.gameObject.name.Contains("DOOR") && GrovalNum_Gravity_Puzzle.sGameManager._Is_Open_Door)
        {
            if(!_Is_Goal)
            {
                //ゲームクリア判定
                GrovalNum_Gravity_Puzzle.gNOW_GAMESTATE = GrovalConst_Gravity_Puzzle.GameState.GAMECLEAR;
                _Is_Goal = true;
            }
        }
        //箱との当たり判定
        if (collision.gameObject.name.Contains("BOX_DIED"))
        {
            if(!_Is_Crash)
            {
                _Is_Crash = true;
                Debug.Log("箱HIT");
            }
        }
    }

    /// <summary>
    /// オブジェクトの識別
    /// </summary>
    /// <returns>オブジェクトID</returns>
    private GrovalConst_Gravity_Puzzle.Obj_ID Obj_Identification()
    {
        foreach (var pair in _Name_to_Obj_ID)
        {
            if (gameObject.name.Contains(pair.Key))
            {
                return pair.Value;
            }
        }

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
        if(is_collider_size)
            _Collider.size = new Vector2(size.x, size.y);

        //画像の初期設定
        GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, img);

        //重力の向き更新
        if (GrovalNum_Gravity_Puzzle.sGameManager._Flick_id != GrovalConst_Gravity_Puzzle.Flick_ID.NONE)
            _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Flick_id;

        switch(_Obj_ID)
        {
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
                {
                    transform.SetAsLastSibling();//最前面に
                    break;
                }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
                {
                    transform.SetAsLastSibling();//最前面に
                    break;
                }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK:
                {
                    //矢印ブロックを非表示にする
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, false);
                    //画像変更 : 上下左右ブロックの画像
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Block_img[(int)_Now_gravity_id]);
                    break;
                }
            case GrovalConst_Gravity_Puzzle.Obj_ID.BOX:
                {
                    break;
                }
        }
        _Is_setting = true;
    }

    /// <summary>
    /// 重力処理
    /// </summary>
    /// <param name="rigid">対象のRigidbody2D</param>
    /// <param name="is_reverse">反転フラグ</param>
    private void Gravity_Move(Rigidbody2D rigid, bool is_reverse)
    {
        if (_Now_gravity_id != GrovalNum_Gravity_Puzzle.sGameManager._Flick_id || _Is_first_ground)
        {
            //重力の向き更新
            if (GrovalNum_Gravity_Puzzle.sGameManager._Flick_id != GrovalConst_Gravity_Puzzle.Flick_ID.NONE)
                _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Flick_id;


            //角度
            Vector3 angle = transform.eulerAngles;
            switch (_Now_gravity_id)
            {
                case GrovalConst_Gravity_Puzzle.Flick_ID.RIGHT:
                    {
                        _Move_vec = Vector2.right;
                        _Gravity_Dir = Vector2.right;
                        angle.z = 90.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.LEFT:
                    {
                        _Move_vec = Vector2.left;
                        _Gravity_Dir = Vector2.left;
                        angle.z = 270.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.UP:
                    {
                        _Move_vec = Vector2.up;
                        _Gravity_Dir = Vector2.up;
                        angle.z = 180.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.DOWN:
                    {
                        _Move_vec = Vector2.down;
                        _Gravity_Dir = Vector2.down;
                        angle.z = 0.0f;
                        break;
                    }
            }

            //角度変更
            transform.eulerAngles = angle;
            //反転フラグがtrueの場合はベクトルを反転
            if (is_reverse)
            {
                _Move_vec.x *= -1;
                _Move_vec.y *= -1;
                _Gravity_Dir.x *= -1;
                _Gravity_Dir.y *= -1;
            }

            _Is_ground_first = true;
            _IsGround = false;
            _Is_first_ground = false;
        }

        //着地判定
        Ground_Check(_Gravity_Dir);

        if (!_IsGround)
        {
            _Move_vec *= 1.04f;
            //一定方向力を加える
            rigid.AddForce(_Move_vec, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// オブジェクトの画面外判定
    /// </summary>
    /// <param name="target_obj">ゲームオブジェクト</param>
    /// <returns></returns>
    private bool Out_Screen(GameObject target_obj)
    {
        //targetUI（RectTransform）のワールド座標をカメラのビューポート座標に変換
        //ビューポート座標は (0,0) が画面左下、(1,1) が画面右上を示す
        Vector3 viewport_pos = Camera.main.WorldToViewportPoint(target_obj.transform.position);

        //ビューポート座標が 0～1 の範囲外であれば、画面外にあると判定
        if (viewport_pos.x < 0 || viewport_pos.x > 1 || viewport_pos.y < 0 || viewport_pos.y > 1)
            return true;    //画面外
        else
            return false;   //画面内
    }

    /// <summary>
    /// 着地判定
    /// </summary>
    /// <returns>着地フラグ</returns>
    private void Ground_Check(Vector2 dir)
    {
        Vector3 origin = _GroundCheck.position;
        Vector3 direction = dir * _CheckDistance;

        // 可視化：赤いRayがSceneビューに表示されます
        Debug.DrawRay(origin, direction, Color.red);

        //足元にRayを飛ばして当たったレイヤーが Ground の場合 : 着地
        RaycastHit2D hit = Physics2D.Raycast(_GroundCheck.position, dir, _CheckDistance, _GroundLayer);
        if (hit.collider != null &&  hit.collider.gameObject != this.gameObject)
        {
            if (!_IsGround)
            {
                RectTransform ground_rect = hit.collider.gameObject.GetComponent<RectTransform>(); //当たった側のRectTransformを取得
                Vector2 ground_pos = ground_rect.localPosition;         //ローカル座標を代入
                ground_pos.y += ground_rect.rect.height *( dir.y * -1); //座標に画像幅 * 重力方向の逆 で落下座標を計算
                ground_pos.x += ground_rect.rect.width  *( dir.x * -1);
                _Rect.transform.localPosition = ground_pos;             //落下座標に強制移動

                _Rigid2D.velocity = Vector2.zero; // 完全に止める
                Debug.Log($"{_Obj_ID}_{hit.collider.name}");
                Debug.Log($"{GrovalNum_Gravity_Puzzle.sGameManager._Character_ground_cnt}/{GrovalNum_Gravity_Puzzle.sGameManager._Character_cnt}");

                _IsGround = true;

                if(_Is_ground_first)//着地したキャラクターの数を増やす
                {
                    GrovalNum_Gravity_Puzzle.sGameManager._Character_ground_cnt++;
                    _Is_ground_first = false;
                }
                return;
            }
        }
        else
        {
            _IsGround = false;
        }
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
                    if (_Now_gravity_id != GrovalNum_Gravity_Puzzle.sGameManager._Flick_id)
                    {
                        //画像変更 : ブロックのベース画像
                        GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._BlockBase_Img);
                        //矢印ブロックの表示
                        GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, true);
                        //角度
                        Vector3 angle = _Block_Arrow.gameObject.transform.eulerAngles;

                        angle.z = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)_Now_gravity_id];

                        _Block_Arrow.gameObject.transform.eulerAngles = angle;

                        //回転の幅が90度を超えている場合 : 回転スピードを2倍にする
                        if (Mathf.DeltaAngle(_Block_Arrow.transform.eulerAngles.z, GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalNum_Gravity_Puzzle.sGameManager._Flick_id]) > 90.0f)
                            _Arrow_RotSpeed = GrovalNum_Gravity_Puzzle.sGamePreference._BlockArrow_RotSpeed * 2;
                        else
                            _Arrow_RotSpeed = GrovalNum_Gravity_Puzzle.sGamePreference._BlockArrow_RotSpeed;

                        _Block_State = Block_State.ROtATION; //回転フェーズへ
                    }
                    break;
                }
            //回転フェーズ
            case Block_State.ROtATION:
                {                
                    float currentAngle = _Block_Arrow.transform.eulerAngles.z;                                                      //現在の角度                    
                    float targetAngle = GrovalConst_Gravity_Puzzle.DIR_ANGLE[(int)GrovalNum_Gravity_Puzzle.sGameManager._Flick_id]; //目的の角度                   
                    float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);                                                  //最短角度差                   
                    float dir = Mathf.Sign(angleDiff);//回転方向（+なら左回転、-なら右回転）

                    //回転処理
                    float rot_frame = _Arrow_RotSpeed * Time.deltaTime * dir;
                    _Block_Arrow.transform.Rotate(0f, 0f, rot_frame);

                    //一定範囲以内になったら完了とみなす（例：1度以内）
                    if (Mathf.Abs(angleDiff) < 1.0f)
                    {
                        //角度をピタッと揃える（端数を切る）
                        Vector3 fixedAngle = _Block_Arrow.transform.eulerAngles;
                        fixedAngle.z = targetAngle;
                        _Block_Arrow.transform.eulerAngles = fixedAngle;
                        //画像変更フェーズへ
                        _Block_State = Block_State.IMG_CHANGE;
                        //重力の向き更新
                        if (GrovalNum_Gravity_Puzzle.sGameManager._Flick_id != GrovalConst_Gravity_Puzzle.Flick_ID.NONE)
                            _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Flick_id;
                    }
                    break;
                }
            //画像変更フェーズ
            case Block_State.IMG_CHANGE:
                {
                    //矢印ブロックの非表示
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, false);
                    //画像変更 : 上下左右ブロックの画像
                    GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Block_img[(int)_Now_gravity_id]);
                    _Block_State = Block_State.READY; //待機フェーズへ
                    break;
                }
        }
    }

    /// <summary>
    /// 常時アニメーション処理
    /// </summary>
    /// <param name="target_img">対象の画像オブジェクト</param>
    /// <param name="change_img">アニメーションするSprite配列</param>
    private void Normal_Animation(Image target_img, Sprite[] change_img)
    {
        _Anim_cnt++;
        if(_Anim_cnt > GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt)
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

    private void Crash_Animation(Image target_img, Sprite[] change_img)
    {
        _Anim_cnt++;
        if (_Anim_cnt > GrovalNum_Gravity_Puzzle.sGamePreference._Character_Anim_Change_cnt)
        {
            //画像変更
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(target_img, change_img[_Anim_index]);

            //インデクス設定
            if (_Anim_index < change_img.Length - 1)
                _Anim_index++;
            else
            {
                //削除
                GrovalNum_Gravity_Puzzle.sGameManager.Delete_Obj(gameObject);
            }
            _Anim_cnt = 0;
        }
    }
}

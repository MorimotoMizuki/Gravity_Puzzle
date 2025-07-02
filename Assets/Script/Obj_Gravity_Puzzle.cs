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
    //移動ベクトル
    private Vector2 _Move_vec = new Vector2();

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
        _Img = GetComponent<Image>();
        _Collider = GetComponent<BoxCollider2D>();
        _Rect = GetComponent<RectTransform>();
        _Rigid2D = GetComponent<Rigidbody2D>();

        //オブジェクトID設定
        _Obj_ID = Obj_Identification();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_Obj_ID)
        {
            //プレイヤー
            case GrovalConst_Gravity_Puzzle.Obj_ID.PLAYER:
                {
                    Obj_Setting(false); //オブジェクトの詳細設定
                    Gravity_Move(_Rigid2D, false);
                    break;
                }
            //風船
            case GrovalConst_Gravity_Puzzle.Obj_ID.BALLOON:
                {
                    Obj_Setting(false); //オブジェクトの詳細設定
                    Gravity_Move(_Rigid2D, true);
                    break;
                }
            //ブロック
            case GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK:
                {
                    Obj_Setting(true); //オブジェクトの詳細設定
                    Block_Move();
                    break;
                }
            //ドア
            case GrovalConst_Gravity_Puzzle.Obj_ID.DOOR:
                {
                    Obj_Setting(true); //オブジェクトの詳細設定
                    break;
                }
            //箱
            case GrovalConst_Gravity_Puzzle.Obj_ID.BOX:
                {
                    Obj_Setting(true); //オブジェクトの詳細設定
                    break;
                }
            //トゲボール
            case GrovalConst_Gravity_Puzzle.Obj_ID.SPIKE_BALL:
                {
                    Obj_Setting(true); //オブジェクトの詳細設定
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
            return;
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
    private void Obj_Setting(bool is_collider_size, Vector2 size = default)
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

        //重力の向き更新
        if (GrovalNum_Gravity_Puzzle.sGameManager._Flick_id != GrovalConst_Gravity_Puzzle.Flick_ID.NONE)
            _Now_gravity_id = GrovalNum_Gravity_Puzzle.sGameManager._Flick_id;

        if(_Obj_ID == GrovalConst_Gravity_Puzzle.Obj_ID.BLOCK)
        {
            //矢印ブロックを非表示にする
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Active(_Block_Arrow.gameObject, false);
            //画像変更 : 上下左右ブロックの画像
            GrovalNum_Gravity_Puzzle.sImageManager.Change_Image(_Img, GrovalNum_Gravity_Puzzle.sImageManager._Block_img[(int)_Now_gravity_id]);
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
                        _Move_vec.x = 1;
                        _Move_vec.y = 0;
                        angle.z = 90.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.LEFT:
                    {
                        _Move_vec.x = -1;
                        _Move_vec.y = 0;
                        angle.z = 270.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.UP:
                    {
                        _Move_vec.x = 0;
                        _Move_vec.y = 1;
                        angle.z = 180.0f;
                        break;
                    }
                case GrovalConst_Gravity_Puzzle.Flick_ID.DOWN:
                    {
                        _Move_vec.x = 0;
                        _Move_vec.y = -1;
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
            }

            _IsGround = false;
            _Is_first_ground = false;
        }

        //着地判定
        Ground_Check();

        if (!_IsGround)
        {
            _Move_vec *= 1.04f;
            //一定方向力を加える
            rigid.AddForce(_Move_vec, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// 着地判定
    /// </summary>
    /// <returns>着地フラグ</returns>
    private void Ground_Check()
    {
        //足元にRayを飛ばして当たったレイヤーが Ground の場合 : 着地
        RaycastHit2D hit = Physics2D.Raycast(_GroundCheck.position, Vector2.down, _CheckDistance, _GroundLayer);
        if (hit.collider != null)
        {
            if (!_IsGround)
            {
                _IsGround = true;
                //着地したキャラクターの数を増やす
                GrovalNum_Gravity_Puzzle.sGameManager._Character_ground_cnt++;
                Debug.Log($"{_Obj_ID}_{GrovalNum_Gravity_Puzzle.sGameManager._Character_ground_cnt}");
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
}

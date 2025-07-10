using System.Collections.Generic;

namespace Common_Gravity_Puzzle
{
    /// <summary>
    /// 共通定数
    /// </summary>
    public static class GrovalConst_Gravity_Puzzle
    {
        //上下左右の角度
        public static readonly float[] DIR_ANGLE = { 270.0f, 90.0f, 0.0f, 180.0f };

        //オブジェクト識別辞書
        public static readonly Dictionary<string, Obj_ID> Name_to_Obj_ID 
        = new Dictionary<string, Obj_ID>
        {
            { "PLAYER"      , Obj_ID.PLAYER },
            { "BALLOON"     , Obj_ID.BALLOON },
            { "BLOCK"       , Obj_ID.BLOCK },
            { "DOOR"        , Obj_ID.DOOR },
            { "BOX"         , Obj_ID.BOX },
            { "SPIKE_BALL"  , Obj_ID.SPIKE_BALL },
            { "RIGHT_SPIKE" , Obj_ID.RIGHT_SPIKE },
            { "LEFT_SPIKE"  , Obj_ID.LEFT_SPIKE },
            { "UP_SPIKE"    , Obj_ID.UP_SPIKE },
            { "DOWN_SPIKE"  , Obj_ID.DOWN_SPIKE },
        };

        //レイヤー識別辞書
        public static readonly Dictionary<Layer_ID, string> Layer_Name
        = new Dictionary<Layer_ID, string>
        {
            {Layer_ID.GROUND,       "Ground" },
            {Layer_ID.PLAYER,       "Player" },
            {Layer_ID.BALLOON,      "Balloon" },
            {Layer_ID.BOX,          "Box" },
            {Layer_ID.DOOR,         "Door" },
            {Layer_ID.SPIKE_BALL,   "Spike_Ball" },
            {Layer_ID.SPIKE_DIR,    "Spike_Dir" },
            {Layer_ID.BOX_DIED,     "Box_Died" },
        };

        //ブロックの矢印の回転の誤差角度
        public static readonly float ARROW_ROt_COMPLETE_THRSHOLD = 1.0f;

        /// <summary>
        /// 画面ID
        /// </summary>
        public enum Screen_ID
        { 
            TITLE,  //タイトル画面
            GAME,   //ゲーム画面
            CLEAR,  //クリア画面
            FADE,   //フェード画面
            NONE,
        }

        /// <summary>
        /// BGMのID
        /// </summary>
        public enum BGM_ID
        {
            TITLE,  //タイトル画面時
            GAME,   //ゲーム画面時
        }

        /// <summary>
        /// SEのID
        /// </summary>
        public enum SE_ID
        {
            GAMECLEAR,      //ゲームクリア時
            GAMEOVER,       //ゲームオーバー時
            GRAVITY_CHANGE, //重力変更時
            BALLOON_GET,    //風船獲得時
            DOOR_MOVE,      //ドア開閉時
            BUTTON_CLICK,   //ボタンクリック時
            COUNTDOWN,      //カウントダウン時
            HIT_SPIKE,      //トゲ衝突時
            HIT_BOX,        //箱衝突時
        }

        /// <summary>
        /// ボタンID
        /// </summary>
        public enum Button_ID
        {
            GIVEUP, //ギブアップボタン
            NEXT,   //ネクストボタン
            REPLAY, //リプレイボタン
            TITLE,  //タイトルへボタン
            START,  //スタートボタン
        }

        /// <summary>
        /// ゲームの状態
        /// </summary>
        public enum GameState
        {
            READY,          //待機
            CREATING_STAGE, //ステージ生成
            PLAYING,        //ゲームプレイ
            GAMECLEAR,      //ゲームクリア
            GAMEOVER,       //ゲームオーバー
        }

        /// <summary>
        /// 重力のID
        /// </summary>
        public enum Gravity_ID
        {
            NONE = -1,
            RIGHT,      //右
            LEFT,       //左
            UP,         //上
            DOWN,       //下
        }

        /// <summary>
        /// オブジェクトのID
        /// </summary>
        public enum Obj_ID
        {
            NONE,       
            PLAYER,     //プレイヤー
            BALLOON,    //風船
            BLOCK,      //ブロック
            DOOR,       //ドア
            BOX,        //箱
            SPIKE_BALL, //トゲボール
            RIGHT_SPIKE,//右向きのトゲ
            LEFT_SPIKE, //左向きのトゲ
            UP_SPIKE,   //上向きのトゲ
            DOWN_SPIKE, //下向きのトゲ
        }

        /// <summary>
        /// レイヤーのID
        /// </summary>
        public enum Layer_ID
        {
            GROUND,     //地面
            PLAYER,     //プレイヤー
            BALLOON,    //風船
            BOX,        //箱
            DOOR,       //ドア
            SPIKE_BALL, //スパイクボール
            SPIKE_DIR,  //トゲ
            BOX_DIED,   //箱の下の部分
        }

        /// <summary>
        /// ドアの状態
        /// </summary>
        public enum Door_Stage
        {
            READY,      //待機フェーズ
            IMG_CHANGE, //画像変更フェーズ : ドアの画像
            PLAYER_IN,  //プレイヤーインフェーズ
            DOOR_CLOSE, //ドアを閉じるフェーズ
            CLEAR,      //クリアフェーズ
            END,        //終了フェーズ
        }
    }

    /// <summary>
    /// 共通変数
    /// </summary>
    public static class GrovalNum_Gravity_Puzzle
    {
        //現在の画面ID
        public static GrovalConst_Gravity_Puzzle.Screen_ID gNOW_SCREEN_ID = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;

        //現在のフェーズ状態
        public static GrovalConst_Gravity_Puzzle.GameState gNOW_GAMESTATE;

        //現在のステージレベル
        public static int gNOW_STAGE_LEVEL = 1;

        //各スクリプト
        public static Game_Manager_Gravity_Puzzle       sGameManager;
        public static Game_Preference_Gravity_Puzzle    sGamePreference;
        public static Image_Manager_Gravity_Puzzle      sImageManager;
        public static Screen_Change_Gravity_Puzzle      sScreenChange;
        public static Click_Manager_Gravity_Puzzle      sClickManager;
        public static Csv_Roader_Gravity_Puzzle         sCsvRoader;
        public static Music_Manager_Gravity_Puzzle      sMusicManager;
    }
}

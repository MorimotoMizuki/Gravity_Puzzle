
namespace Common_Gravity_Puzzle
{
    /// <summary>
    /// 共通定数
    /// </summary>
    public static class GrovalConst_Gravity_Puzzle
    {
        //上下左右の角度
        public static readonly float[] DIR_ANGLE = { 270.0f, 90.0f, 0.0f, 180.0f };

        /// <summary>
        /// 画面ID
        /// </summary>
        public enum Screen_ID
        { 
            TITLE,
            GAME,
            CLEAR,
            FADE,
            NONE,
        }

        /// <summary>
        /// ボタンID
        /// </summary>
        public enum Button_ID
        {
            GIVEUP,
            NEXT,
            REPLAY,
            TITLE,
            START,
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

        //フリックID
        public enum Flick_ID
        {
            NONE = -1,
            RIGHT,
            LEFT,
            UP,
            DOWN, 
        }

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
        /// ゴールの状態
        /// </summary>
        public enum Goal_Stage
        {
            READY,
            IMG_CHANGE,
            PLAYER_IN,
            DOOR_CLOSE,
            CLEAR,
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
        public static int gNOW_STAGE_LEVEL = 5;

        //各スクリプト
        public static Game_Manager_Gravity_Puzzle sGameManager;
        public static Game_Preference_Gravity_Puzzle sGamePreference;
        public static Image_Manager_Gravity_Puzzle sImageManager;
        public static Screen_Change_Gravity_Puzzle sScreenChange;
        public static Click_Manager_Gravity_Puzzle sClickManager;
        public static Csv_Roader_Gravity_Puzzle sCsvRoader;
    }
}

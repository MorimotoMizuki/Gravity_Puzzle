
namespace Common_Gravity_Puzzle
{
    /// <summary>
    /// 共通定数
    /// </summary>
    public static class GrovalConst_Gravity_Puzzle
    {
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
        public static Game_Manager_Gravity_Puzzle sGameManager;
        public static Game_Preference_Gravity_Puzzle sGamePreference;
        public static Image_Manager_Gravity_Puzzle sImageManager;
        public static Screen_Change_Gravity_Puzzle sScreenChange;
        public static Click_Manager_Gravity_Puzzle sClickManager;
    }
}

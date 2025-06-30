
namespace Common_Gravity_Puzzle
{
    /// <summary>
    /// ���ʒ萔
    /// </summary>
    public static class GrovalConst_Gravity_Puzzle
    {
        /// <summary>
        /// ���ID
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
        /// �{�^��ID
        /// </summary>
        public enum Button_ID
        {
            GIVEUP,
            NEXT,
            REPLAY,
            TITLE,
        }

        /// <summary>
        /// �Q�[���̏��
        /// </summary>
        public enum GameState
        {
            READY,          //�ҋ@
            CREATING_STAGE, //�X�e�[�W����
            PLAYING,        //�Q�[���v���C
            GAMECLEAR,      //�Q�[���N���A
            GAMEOVER,       //�Q�[���I�[�o�[
        }

    }
    
    /// <summary>
    /// ���ʕϐ�
    /// </summary>
    public static class GrovalNum_Gravity_Puzzle
    {
        //���݂̉��ID
        public static GrovalConst_Gravity_Puzzle.Screen_ID gNOW_SCREEN_ID = GrovalConst_Gravity_Puzzle.Screen_ID.TITLE;

        //���݂̃t�F�[�Y���
        public static GrovalConst_Gravity_Puzzle.GameState gNOW_GAMESTATE;

        //���݂̃X�e�[�W���x��
        public static int gNOW_STAGE_LEVEL = 1;

        //�e�X�N���v�g
        public static Game_Manager_Gravity_Puzzle sGameManager;
        public static Game_Preference_Gravity_Puzzle sGamePreference;
        public static Image_Manager_Gravity_Puzzle sImageManager;
        public static Screen_Change_Gravity_Puzzle sScreenChange;
        public static Click_Manager_Gravity_Puzzle sClickManager;
    }
}

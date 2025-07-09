using System.Collections;
using UnityEngine;

using Common_Gravity_Puzzle;

public class Music_Manager_Gravity_Puzzle : MonoBehaviour
{
    [Header("BGMとSEのAudioSource")]
    [SerializeField] private AudioSource audio_source_bgm;
    [SerializeField] private AudioSource audio_source_se;

    [Header("BGMのフェード時間")]
    [SerializeField] private float bgm_fade_time = 0.5f;

    //各画面のBGMクリップ
    [Header("各画面のBGM")]
    [SerializeField] private AudioClip[] bgm_clip;

    //BGMの音量
    [Header("各BGMの音量")]
    [Range(0f, 1f)]
    [SerializeField] private float[] bgm_volume;

    //各SEクリップ
    [Header("各SE")]
    [SerializeField] private AudioClip[] se_clip;

    //SEの音量
    [Header("各SEの音量")]
    [Range(0f, 1f)]
    [SerializeField] private float[] se_volume;

    // Start is called before the first frame update
    void Start()
    {
        //BGMを明示的にロード　(画面のカクツキを減らすため)
        for (int i = 0; i < bgm_clip.Length; i++)
            bgm_clip[i].LoadAudioData();
        //SEを明示的にロード
        for (int i = 0; i < se_clip.Length; i++)
            se_clip[i].LoadAudioData();

        //タイトルBGMを再生
        BGM_Change(GrovalConst_Gravity_Puzzle.BGM_ID.TITLE);
    }

    #region BGM関係 -------------------------------------------------------------

    /// <summary>
    /// BGM切り替え
    /// </summary>
    /// <param name="bgm_id">変えたいBGMの種類を示すID</param>
    public void BGM_Change(GrovalConst_Gravity_Puzzle.BGM_ID bgm_id)
    {
        //BGMのフェードと切り替えコルーチン
        StartCoroutine(Fade_And_Change_BGM(bgm_id, bgm_fade_time));
    }

    /// <summary>
    /// BGMを探す
    /// </summary>
    /// <param name="bgm_id">BGMの種類を示すID</param>
    /// <returns>bgm_idが示すBGMの AudioClip</returns>
    private AudioClip Search_BGM_Index_Num(GrovalConst_Gravity_Puzzle.BGM_ID bgm_id)
    {
        if ((int)bgm_id >= bgm_clip.Length)
        {
            Debug.LogError(bgm_id + "番目のBGMが割り当てられていません");
            return null;
        }

        return bgm_clip[(int)bgm_id];
    }

    /// <summary>
    /// BGM再生　フェード
    /// </summary>
    /// <param name="bgm_id">BGMの種類を示すID</param>
    /// <param name="fade_time">BGMがフェードする時間(秒)</param>
    /// <returns>コルーチン用 IEnumerator</returns>
    private IEnumerator Fade_And_Change_BGM(GrovalConst_Gravity_Puzzle.BGM_ID bgm_id, float fade_time)
    {
        //BGMを探して割り当てる
        AudioClip new_bgm = Search_BGM_Index_Num(bgm_id);

        //エラーチェック
        if (audio_source_bgm == null || new_bgm == null)
        {
            Debug.LogError("BGM未設定");
            yield break;
        }

        //BGMの音量設定
        float volume = 0.5f;
        if ((int)bgm_id < bgm_volume.Length)
            volume = bgm_volume[(int)bgm_id];

        audio_source_bgm.volume = volume;

        // フェードアウト
        float start_volume = audio_source_bgm.volume;
        for (float t = 0; t < fade_time; t += Time.deltaTime)
        {
            audio_source_bgm.volume = Mathf.Lerp(start_volume, 0, t / fade_time);
            yield return null;
        }

        audio_source_bgm.Stop();         //BGM停止
        audio_source_bgm.clip = new_bgm; //BGM変更       
        audio_source_bgm.loop = true;    //ループ再生設定
        audio_source_bgm.Play();         //BGM再生

        //BGMの音量設定
        if ((int)bgm_id < bgm_volume.Length)
        {
            audio_source_bgm.volume = bgm_volume[(int)bgm_id];
            volume = bgm_volume[(int)bgm_id];
        }

        // フェードイン
        for (float t = 0; t < fade_time; t += Time.deltaTime)
        {
            audio_source_bgm.volume = Mathf.Lerp(0, start_volume, t / fade_time);
            yield return null;
        }

        //最終音量調整
        audio_source_bgm.volume = volume;
    }


    #endregion ------------------------------------------------------------------

    #region SE関係 --------------------------------------------------------------

    /// <summary>
    /// SEの停止
    /// </summary>
    public void SE_Stop()
    {
        audio_source_se.Stop();
    }

    /// <summary>
    /// SEを探す
    /// </summary>
    /// <param name="se_id">SEの種類を示すID</param>
    /// <returns>se_idが示すSEの AudioClip</returns>
    private AudioClip Search_SE_Index_Num(GrovalConst_Gravity_Puzzle.SE_ID se_id)
    {
        if ((int)se_id >= se_clip.Length)
        {
            Debug.LogError(se_id + "番目のSEが割り当てられていません");
            return null;
        }

        return se_clip[(int)se_id];
    }

    /// <summary>
    /// BGMを止めてSEを再生
    /// </summary>
    /// <param name="se_id">SEの種類を示すID</param>
    public void SE_Play_BGM_Stop(GrovalConst_Gravity_Puzzle.SE_ID se_id)
    {
        //AudioSourceが割り当てられていない場合
        if (audio_source_bgm == null)
        {
            Debug.LogError("audio_source_bgmがnullです");
            return;
        }

        //BGMを停止する
        audio_source_bgm.Stop();

        //SEを再生
        SE_Play(se_id);
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="se_id">SEの種類を示すID</param>
    public void SE_Play(GrovalConst_Gravity_Puzzle.SE_ID se_id)
    {
        //SEを探して割り当てる
        AudioClip play_se = Search_SE_Index_Num(se_id);

        //SEが割り当てられていない場合
        if (audio_source_se == null || play_se == null)
        {
            Debug.LogError("SE未設定");
            return;
        }

        //SEの音量設定
        float volume = 0.5f;
        if ((int)se_id < se_volume.Length)
            volume = se_volume[(int)se_id];

        audio_source_se.volume = volume; //音量
        audio_source_se.clip = play_se;  //SEクリップ
        audio_source_se.loop = false;    //ループしない

        //SEの単発再生
        audio_source_se.Play();
    }

    #endregion ------------------------------------------------------------------

}

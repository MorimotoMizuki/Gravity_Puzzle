using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Csv_Roader_Gravity_Puzzle : MonoBehaviour
{
    //パネルマップ辞書 : 各ステージの名前(例：stage1)をキーに、マップデータ(2次元リスト)を保持する辞書
    [HideInInspector]
    public Dictionary<string, List<List<int>>> _MapData = new Dictionary<string, List<List<int>>>();

    // Start is called before the first frame update
    void Start()
    {
        //マップデータ読み込み処理
        _MapData = Road_Map_Date("Obj_Map");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// マップ読み込み
    /// </summary>
    /// <param name="date_path">読み込むファイルのパス</param>
    /// <returns>読み込んだデータの Dictionary</returns>
    public Dictionary<string, List<List<int>>> Road_Map_Date(string date_path)
    {
        Dictionary<string, List<List<int>>> map_date_dict = new Dictionary<string, List<List<int>>>();

        // Resourcesフォルダ内のCSVファイルを読み込む
        TextAsset csvFile = Resources.Load<TextAsset>(date_path);
        if (csvFile == null)
        {
            Debug.LogError("CSVファイルが見つかりません：" + date_path);
            return map_date_dict;
        }

        // 改行で分割（空行は無視）
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        string currentStage = "";                   // 現在読み込んでいるステージ名（例：stage1）
        List<List<int>> currentStageMap = null;     // 現在のステージのマップデータ（一時的に保持）

        foreach (string line in lines)
        {
            // ステージ名の行を検出（例：stage1, stage2）
            if (line.StartsWith("stage"))
            {
                // 前のステージのデータがあれば、辞書に追加
                if (!string.IsNullOrEmpty(currentStage))
                {
                    map_date_dict[currentStage] = currentStageMap;
                }

                // 新しいステージの準備
                currentStage = line.Trim();               // ステージ名を保存
                currentStageMap = new List<List<int>>();  // 新しいマップデータのリストを初期化
            }
            else
            {
                // CSVの数値行（マップの1行分）を読み込む
                string[] cells = line.Split(',');
                List<int> row = new List<int>();

                // 各セル（カンマ区切り）をintに変換して追加
                foreach (string cell in cells)
                {
                    if (int.TryParse(cell, out int num))
                    {
                        row.Add(num);
                    }
                }

                // 行をステージマップに追加
                currentStageMap.Add(row);
            }
        }

        // 最後のステージデータを辞書に追加
        if (!string.IsNullOrEmpty(currentStage) && currentStageMap != null)
        {
            map_date_dict[currentStage] = currentStageMap;
        }

        // デバッグ表示：すべてのステージを出力
        //foreach (var stage in map_date_dict)
        //{
        //    Debug.Log($"▼ {stage.Key} のマップデータ");

        //    foreach (var row in stage.Value)
        //    {
        //        Debug.Log(string.Join(",", row)); // 1行のデータをカンマでつなげて表示
        //    }
        //}

        //foreach (var key in map_date_dict.Keys)
        //{
        //    Debug.Log($"{key} 値の数: {map_date_dict[key].Count}");
        //}
        //Debug.Log("マップデータの読み込み完了！");

        return map_date_dict;
    }

}

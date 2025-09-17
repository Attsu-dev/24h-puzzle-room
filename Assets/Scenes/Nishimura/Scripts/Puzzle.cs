using UnityEngine;
using TMPro;  // TextMeshProを使う時に必要

public abstract class Puzzle : MonoBehaviour
{
    [SerializeField] private TextAsset japaneseWordList; // 単語リスト(txt)をInspectorで割り当て
    protected string[] words;   // 読み込んだ単語リスト

    protected virtual void Awake()
    {
        // テキストファイルを行ごとに読み込む
        if (japaneseWordList != null)
        {
            words = japaneseWordList.text
                .Replace("\r", "")   // 改行コードを整理
                .Split('\n');
        }
    }


    // 次の問題を作成し、答えを返す
    public abstract string CreateNextQuestion();
}

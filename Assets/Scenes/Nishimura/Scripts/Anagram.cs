using UnityEngine;
using TMPro;  // TextMeshProを使う時に必要

public class Anagram : MonoBehaviour
{
    [SerializeField] private TextAsset japaneseWordList; // 単語リスト(txt)をInspectorで割り当て
    [SerializeField] private TextMeshPro puzzleText; // 子のTextMeshProを割り当て

    private string[] words;   // 読み込んだ単語リスト
    private string answer;    // 正解の単語

    void Start()
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
    public string CreateNextQuestion()
    {
        if (words == null || words.Length == 0) return "";

        // ランダムに1つ選ぶ
        answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer)) { return CreateNextQuestion(); }

        // 文字をシャッフル
        char[] chars = answer.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            int rnd = Random.Range(i, chars.Length);
            (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
        }

        // TextMeshProに表示
        if (puzzleText != null)
        {
            puzzleText.text = new string(chars);
        }
        return answer;
    }
}

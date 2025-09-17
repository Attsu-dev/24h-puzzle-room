using UnityEngine;
using TMPro;  // TextMeshProを使う時に必要

public class Anagram : Puzzle
{
    [SerializeField] private TextMeshPro puzzleText; // 子のTextMeshProを割り当て

    // 次の問題を作成し、答えを返す
    public override string CreateNextQuestion()
    {
        if (words == null || words.Length == 0) return "";

        // ランダムに1つ選ぶ
        string answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 2) { return CreateNextQuestion(); }

        // 文字をシャッフル
        char[] chars = answer.ToCharArray();

        while (new string (chars) == answer) // 元の文字列と同じ場合は再シャッフル
        {
            for (int i = 0; i < chars.Length; i++)
            {
                int rnd = Random.Range(i, chars.Length);
                (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
            }
        }

        // TextMeshProに表示
        if (puzzleText != null)
        {
            puzzleText.text = new string(chars);
        }
        return answer;
    }
}

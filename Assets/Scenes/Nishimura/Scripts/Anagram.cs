using UnityEngine;
using TMPro;  // TextMeshPro���g�����ɕK�v

public class Anagram : Puzzle
{
    [SerializeField] private TextMeshPro puzzleText; // �q��TextMeshPro�����蓖��

    // ���̖����쐬���A������Ԃ�
    public override string CreateNextQuestion()
    {
        if (words == null || words.Length == 0) return "";

        // �����_����1�I��
        string answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 2) { return CreateNextQuestion(); }

        // �������V���b�t��
        char[] chars = answer.ToCharArray();

        while (new string (chars) == answer) // ���̕�����Ɠ����ꍇ�͍ăV���b�t��
        {
            for (int i = 0; i < chars.Length; i++)
            {
                int rnd = Random.Range(i, chars.Length);
                (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
            }
        }

        // TextMeshPro�ɕ\��
        if (puzzleText != null)
        {
            puzzleText.text = new string(chars);
        }
        return answer;
    }
}

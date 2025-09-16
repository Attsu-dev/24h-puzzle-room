using UnityEngine;
using TMPro;  // TextMeshPro���g�����ɕK�v

public class Anagram : MonoBehaviour
{
    [SerializeField] private TextAsset japaneseWordList; // �P�ꃊ�X�g(txt)��Inspector�Ŋ��蓖��
    [SerializeField] private TextMeshPro puzzleText; // �q��TextMeshPro�����蓖��

    private string[] words;   // �ǂݍ��񂾒P�ꃊ�X�g
    private string answer;    // �����̒P��

    void Start()
    {
        // �e�L�X�g�t�@�C�����s���Ƃɓǂݍ���
        if (japaneseWordList != null)
        {
            words = japaneseWordList.text
                .Replace("\r", "")   // ���s�R�[�h�𐮗�
                .Split('\n');
        }
    }

    // ���̖����쐬���A������Ԃ�
    public string CreateNextQuestion()
    {
        if (words == null || words.Length == 0) return "";

        // �����_����1�I��
        answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer)) { return CreateNextQuestion(); }

        // �������V���b�t��
        char[] chars = answer.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            int rnd = Random.Range(i, chars.Length);
            (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
        }

        // TextMeshPro�ɕ\��
        if (puzzleText != null)
        {
            puzzleText.text = new string(chars);
        }
        return answer;
    }
}

using UnityEngine;
using TMPro;  // TextMeshPro���g�����ɕK�v

public abstract class Puzzle : MonoBehaviour
{
    [SerializeField] private TextAsset japaneseWordList; // �P�ꃊ�X�g(txt)��Inspector�Ŋ��蓖��
    protected string[] words;   // �ǂݍ��񂾒P�ꃊ�X�g

    protected virtual void Awake()
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
    public abstract string CreateNextQuestion();
}

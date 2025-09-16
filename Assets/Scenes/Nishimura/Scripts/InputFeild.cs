using UnityEngine;
using TMPro;

public class InputFeild : MonoBehaviour
{
    public TMP_InputField inputField; // Inspector�Ŋ��蓖��

    void Start()
    {
        // ���͊m�莞�ɌĂ΂��
        inputField.onSubmit.AddListener(OnSubmitAnswer);
    }

    void OnSubmitAnswer(string text)
    {
        GameMaster.Instance.OnSubmitAnswer(text);
    }
}
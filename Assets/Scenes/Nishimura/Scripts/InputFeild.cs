using UnityEngine;
using TMPro;

public class InputFeild : MonoBehaviour
{
    public TMP_InputField inputField; // Inspector‚ÅŠ„‚è“–‚Ä

    void Start()
    {
        // “ü—ÍŠm’èŽž‚ÉŒÄ‚Î‚ê‚é
        inputField.onSubmit.AddListener(OnSubmitAnswer);
    }

    void OnSubmitAnswer(string text)
    {
        GameMaster.Instance.OnSubmitAnswer(text);
    }
}
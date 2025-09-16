using UnityEngine;
using UnityEngine.UI;

public class ClickBackButton : MonoBehaviour
{
    public Button backButton;  // Inspector�Ń{�^�������蓖��
    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void OnBackButtonClicked()
    {
        GameMaster.Instance.OnBackButtonClicked();
    }
}

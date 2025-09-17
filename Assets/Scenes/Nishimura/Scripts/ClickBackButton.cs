using UnityEngine;
using UnityEngine.UI;

public class ClickBackButton : MonoBehaviour
{
    [SerializeField] private Button backButton;
    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void OnBackButtonClicked()
    {
        GameMaster.Instance.OnBackButtonClicked();
    }
}

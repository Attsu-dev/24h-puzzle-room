using UnityEngine;
using UnityEngine.UI;

public class ClickBackButton : MonoBehaviour
{
    public Button backButton;  // Inspector‚Åƒ{ƒ^ƒ“‚ðŠ„‚è“–‚Ä
    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void OnBackButtonClicked()
    {
        GameMaster.Instance.OnBackButtonClicked();
    }
}

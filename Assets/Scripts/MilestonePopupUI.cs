using UnityEngine;
using TMPro;

public class MilestonePopupUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI popupText;
    public float displayDuration = 1.5f;
    public float fadeSpeed = 2f;

    private float timer = 0f;
    private bool isShowing = false;

    public void ShowPopup(string message)
    {
        popupText.text = message;
        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
        isShowing = true;
        timer = displayDuration;
    }

    void Update()
    {
        if (!isShowing) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;

            if (canvasGroup.alpha <= 0f)
            {
                isShowing = false;
                gameObject.SetActive(false);
            }
        }
    }
}

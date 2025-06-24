using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [TextArea(2, 6)] public string[] tutorialTexts;
    public TextMeshProUGUI tutorialText;
    public GameObject tutorialPanel;
    public Button continueButton;
    public static bool IsTutorialActive { get; private set; }
    public GridPlacer gridPlacer;


    private int currentIndex = 0;

    void Start()
    {
        IsTutorialActive = true;
        tutorialPanel.SetActive(true);
        tutorialText.text = tutorialTexts[currentIndex];
        continueButton.onClick.AddListener(NextMessage);
    }

    void NextMessage()
    {
        currentIndex++;
        if (currentIndex < tutorialTexts.Length)
        {
            tutorialText.text = tutorialTexts[currentIndex];
        }
        else
        {
            tutorialPanel.SetActive(false);
            IsTutorialActive = false;
            gridPlacer.InitializeSpawnTile();
        }
    }

}

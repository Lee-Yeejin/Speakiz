using UnityEngine;
using UnityEngine.UI;

public class SurveyManager : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button nextButton;

    private void Start()
    {
        button1.onClick.AddListener(() => OnButtonClicked(button1, 10));
        button2.onClick.AddListener(() => OnButtonClicked(button2, 30));
        button3.onClick.AddListener(() => OnButtonClicked(button3, 50));
        button4.onClick.AddListener(() => OnButtonClicked(button4, 70));
        button5.onClick.AddListener(() => OnButtonClicked(button5, 90));
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnButtonClicked(Button button, int score)
    {
        ResetButtonColors();

        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = Color.yellow;
        colorBlock.highlightedColor = Color.yellow;
        colorBlock.pressedColor = Color.yellow;
        colorBlock.selectedColor = Color.yellow;
        button.colors = colorBlock;

        
        CentralSurveyManager.instance.SetScore(score);
    }

    private void ResetButtonColors()
    {
        ColorBlock defaultColorBlock = button1.colors;
        defaultColorBlock.normalColor = Color.white;
        defaultColorBlock.highlightedColor = Color.white;
        defaultColorBlock.pressedColor = Color.white;
        defaultColorBlock.selectedColor = Color.white;

        button1.colors = defaultColorBlock;
        button2.colors = defaultColorBlock;
        button3.colors = defaultColorBlock;
        button4.colors = defaultColorBlock;
        button5.colors = defaultColorBlock;
    }

    private void OnNextButtonClicked()
    {
        if (CentralSurveyManager.instance.GetCurrentQuestionIndex() == 5)
        {
            CentralSurveyManager.instance.CalculateAndDisplayLevel();
        }
    }
}

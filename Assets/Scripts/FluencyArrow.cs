using UnityEngine;
using UnityEngine.UI;

public class FluencyArrow : MonoBehaviour
{
    /*public FluencyUIController fluencyUIController;
    public Button nextButton;      // 다음 씬으로 이동할 버튼
    public Button previousButton;  // 이전 씬으로 이동할 버튼

    private int currentSceneIndex = 0;
    private int totalScenes = 3; // 씬의 총 개수 (0, 1, 2)

    void Start()
    {
        // 버튼 클릭 시 메서드 연결
        nextButton.onClick.AddListener(NextScene);
        previousButton.onClick.AddListener(PreviousScene);

        UpdateArrowVisibility();
    }

    public void NextScene()
    {
        if (currentSceneIndex < totalScenes - 1)
        {
            currentSceneIndex++;
            fluencyUIController.SetCanvasIndex(currentSceneIndex);
            UpdateArrowVisibility();
        }
    }

    public void PreviousScene()
    {
        if (currentSceneIndex > 0)
        {
            currentSceneIndex--;
            fluencyUIController.SetCanvasIndex(currentSceneIndex);
            UpdateArrowVisibility();
        }
    }

    private void UpdateArrowVisibility()
    {
        previousButton.gameObject.SetActive(currentSceneIndex > 0);
        nextButton.gameObject.SetActive(currentSceneIndex < totalScenes - 1);
    }*/
}
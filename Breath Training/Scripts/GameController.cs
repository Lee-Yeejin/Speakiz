using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리 네임스페이스 추가

public class GameController : MonoBehaviour
{
    public GameObject gameSuccessUI; // 게임 성공 UI 참조
    public GameObject gameFailUI; // 게임 실패 UI 참조
    public Mic micScript; // Mic 스크립트 참조
    public Text attemptsText; // 게임 성공 UI에 표시할 Attempts Text 참조
    public Text failAttemptsText; // 게임 실패 UI에 표시할 Attempts Text 참조
    public Text successBalloonsText; // 성공한 풍선 개수를 표시할 Text 참조

    public GameObject image1; // 성공한 풍선 1개일 때 활성화할 이미지
    public GameObject image2; // 성공한 풍선 2개일 때 활성화할 이미지
    public GameObject image3; // 성공한 풍선 3개일 때 활성화할 이미지
    public GameObject image4; // 성공한 풍선 4개일 때 활성화할 이미지

    public GameObject FCanvas; // F캔버스 참조 추가
    public GameObject currentCanvas; // 현재 캔버스 참조 추가

    void Start()
    {
        gameSuccessUI.SetActive(false); // 게임 시작 시 성공 UI 숨김
        gameFailUI.SetActive(false); // 게임 시작 시 실패 UI 숨김
        FCanvas.SetActive(false); // F캔버스 숨김

        // 모든 이미지를 비활성화
        image1.SetActive(false);
        image2.SetActive(false);
        image3.SetActive(false);
        image4.SetActive(false);
    }

    void Update()
    {
        CheckEndGameCondition();
        UpdateSuccessImages(); // 성공한 풍선 개수에 따라 이미지 업데이트
    }

    public void CheckEndGameCondition()
    {
        if (micScript.successfulAttempts >= 5)
        {
            gameSuccessUI.SetActive(true);
            attemptsText.text = micScript.totalAttempts.ToString() + " 회";
            micScript.enabled = false;
        }
        else if (micScript.totalAttempts >= 10)
        {
            gameFailUI.SetActive(true);
            failAttemptsText.text = micScript.totalAttempts.ToString() + " 회";
            successBalloonsText.text = "성공 풍선 개수 : ";
            micScript.enabled = false;
        }
    }

    public void UpdateSuccessImages()
    {
        image1.SetActive(false);
        image2.SetActive(false);
        image3.SetActive(false);
        image4.SetActive(false);

        if (micScript.successfulAttempts == 1)
        {
            image1.SetActive(true);
        }
        else if (micScript.successfulAttempts == 2)
        {
            image2.SetActive(true);
        }
        else if (micScript.successfulAttempts == 3)
        {
            image3.SetActive(true);
        }
        else if (micScript.successfulAttempts == 4)
        {
            image4.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame 버튼이 눌렸습니다.");
        micScript.SaveFinalDataToPlayerPrefs(); // PlayerPrefs에 데이터 저장

        // PlayerPrefs에 저장된 데이터로 TrainingFetcher 업데이트
        TrainingFetcher.instance.LoadBreathTrainingData();
        //TrainingFetcher.instance.LoadPronunciationTrainingData();

        // FCanvas 활성화
        FCanvas.SetActive(true);
        gameSuccessUI.SetActive(false);
        gameFailUI.SetActive(false);
        micScript.enabled = false;

        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }
    }

}
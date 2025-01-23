using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    public Text questionText;  // 질문 텍스트
    public Text choice1Text;   // 선택지 1 텍스트
    public Text choice2Text;   // 선택지 2 텍스트
    public Text choice3Text;   // 선택지 3 텍스트
    public Text feedbackText;  // 음성 인식 결과 또는 피드백 텍스트

    public GameObject completionPopup;  // 완료 팝업 UI 요소 추가
                                        // 집 모양 버튼 클릭 시 호출될 메서드
    
    public GameObject FCanvas; // F캔버스 참조 추가
    public GameObject currentCanvas; // 현재 캔버스 참조 추가

    // 선택지 텍스트의 색깔 변경 함수
    public void HighlightChoice(int choiceIndex)
    {
        // 모든 선택지의 색깔을 초기화
        choice1Text.color = Color.black;
        choice2Text.color = Color.black;
        choice3Text.color = Color.black;

        // 선택된 선택지의 색깔 변경
        switch (choiceIndex)
        {
            case 1:
                choice1Text.color = Color.blue;
                break;
            case 2:
                choice2Text.color = Color.blue;
                break;
            case 3:
                choice3Text.color = Color.blue;
                break;
        }
    }

    // 질문 및 선택지 텍스트 설정 함수
    public void SetupQuestion(string question, string choice1, string choice2, string choice3)
    {
        // 질문과 선택지를 설정
        questionText.text = question;
        choice1Text.text = choice1;
        choice2Text.text = choice2;
        choice3Text.text = choice3;

        // 선택지의 색상을 모두 검정색으로 초기화
        choice1Text.color = Color.black;
        choice2Text.color = Color.black;
        choice3Text.color = Color.black;

        feedbackText.text = "";  // 음성인식 결과 초기화
    }

    // STT 결과를 화면에 표시하는 함수
    public void DisplaySTTResult(string result)
    {
        feedbackText.text = $"나의 대답: {result}";
    }

    // 저장 상태를 Console에 출력
    public void LogSaveStatus(string statusMessage)
    {
        Debug.Log($"저장 상태: {statusMessage}");
    }
    
    // 홈 버튼 누르면 training 페이지로 전환
    public void OnHomeButtonClick() 
    {
        Debug.Log("Training Canvas로 전환");

        // 현재 진행 중인 질문과 답변 저장
        var dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.SaveCurrentDialogueData(); // 진행 중인 데이터를 저장
        }

        // 게임 데이터를 TrainingFetcher로 바로 로드
        TrainingFetcher.instance.LoadFluencyTrainingData(); // 유창성으로 수정.

        // FCanvas 활성화
        if (FCanvas != null)
        {
            FCanvas.SetActive(true);
        }

        // 현재 캔버스 비활성화
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }
    }

    // 다시하기 버튼 누르면 canvas1부터 재실행
    public void replayButtonClick()
    {
        ////여기채우삼 리플레이
    }

    // 완료 팝업 표시 함수
    public void ShowCompletionPopup()
    {
        completionPopup.SetActive(true);  // 완료 팝업을 활성화
    }

    // 완료 팝업을 숨기는 함수
    public void HideCompletionPopup()
    {
        completionPopup.SetActive(false);  // 완료 팝업을 비활성화
    }

    // 나가기 버튼을 클릭했을 때 호출되는 함수
    public void OnExitButtonClick()
    {
        Application.Quit();  // 게임 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 테스트할 때는 플레이 모드 종료
#endif
    }//////////////이 코드는 게임 안에서만

}
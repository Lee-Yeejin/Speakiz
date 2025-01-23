using UnityEngine;
using System.Threading.Tasks;

public class DialogueManager5 : MonoBehaviour
{
    public SpeechSynthesisManager speechSynthesisManager; // TTS 매니저 참조
    public UIManager uiManager; // UIManager 참조 추가

    async void Start()
    {
        await WaitForSpeechSynthesisInitialization();
        DisplayGoodbyeMessage();
        PlayGoodbyeMessage();
    }

    private async Task WaitForSpeechSynthesisInitialization()
    {
        while (!speechSynthesisManager.isInitialized)
        {
            Debug.Log("SpeechSynthesisManager 초기화 대기 중...");
            await Task.Delay(100); // 100ms 대기 후 다시 확인
        }
    }

    private void DisplayGoodbyeMessage()
    {
        string goodbyeMessage = "잘가~ 오늘 함께해서 즐거웠어~";
        uiManager.questionText.text = goodbyeMessage; // UI에 메시지 표시
                                                      // Goodbye 메시지를 PlayerPrefs에 저장
        PlayerPrefs.SetString("GoodbyeMessage", goodbyeMessage);
        PlayerPrefs.Save(); // 저장한 데이터 즉시 적용
    }

    private async void PlayGoodbyeMessage()
    {
        string goodbyeMessage = "잘가~ 오늘 함께해서 즐거웠어~";
        await speechSynthesisManager.SpeakText(goodbyeMessage);

        ShowCompletionPopup();  // TTS가 끝나면 완료 팝업 표시
    }

    private void ShowCompletionPopup()
    {
        uiManager.ShowCompletionPopup();  // UIManager에서 팝업 표시
    }
}
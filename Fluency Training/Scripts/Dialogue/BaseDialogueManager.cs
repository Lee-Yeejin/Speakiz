using UnityEngine;
using System.Threading.Tasks;

public abstract class BaseDialogueManager : MonoBehaviour
{
    public UIManager uiManager;
    public SpeechSynthesisManager speechSynthesisManager;
    public PronunciationEvaluationManager pronunciationEvaluationManager;

    protected int conversationStage = 0; // 대화 단계
    protected int currentFlow = 0; // 대화 흐름 (각 씬에 따라 다름)

    // 대화의 예상 텍스트를 반환하는 추상 메서드 추가
    private string recordedFilePath; // 녹음된 파일 경로 저장

    // 녹음된 파일 경로를 설정하는 메서드 (공통)
    public virtual void SetRecordedFilePath(string path)
    {
        recordedFilePath = path;
        Debug.Log($"녹음된 파일 경로가 설정되었습니다: {path}");
    }
    protected abstract Task SetupInitialDialogue(); // 각 씬에서 구현할 초기 설정
    protected abstract void ProcessResponse(string userResponse); // 대화 진행 처리

    // 각 씬에서 예상되는 텍스트를 반환하는 추상 메서드
    public abstract string GetExpectedResponse(string userResponse);

    protected virtual async void Start()
    {
        await SetupInitialDialogue();
    }

    public async void OnUserResponse(string userResponse)
    {
        userResponse = RemovePunctuation(userResponse);
        uiManager.DisplaySTTResult(userResponse);

        string expectedText = GetExpectedResponse(userResponse); // 각 씬에서 구현된 예상 텍스트 가져오기

        if (!string.IsNullOrEmpty(expectedText) && userResponse.Equals(expectedText, System.StringComparison.OrdinalIgnoreCase))
        {
            // 발음 평가를 위한 녹음 파일 사용
            if (!string.IsNullOrEmpty(recordedFilePath))
            {
                pronunciationEvaluationManager.EvaluatePronunciation(recordedFilePath, expectedText);
                ClearRecordedFilePath(); // 발음 평가 완료 후 파일 경로 초기화
            }

            pronunciationEvaluationManager.SaveUserResponse(expectedText, userResponse);
            Debug.Log("발음 평가 데이터가 저장되었습니다.");
        }
        else
        {
            Debug.LogWarning($"응답 불일치: 예상 = {expectedText}, 실제 = {userResponse}. 저장되지 않음.");
        }

        ProcessResponse(userResponse);
    }
    private void ClearRecordedFilePath()
    {
        recordedFilePath = string.Empty;
        Debug.Log("녹음된 파일 경로가 초기화되었습니다.");
    }

    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
    }
}
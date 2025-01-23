using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;

public class SpeechRecognitionManager4: MonoBehaviour
{
    private SpeechRecognizer recognizer;
    public UIManager uiManager; // UIManager와 연동
    public BaseDialogueManager dialogueManager; // 다형성을 위해 BaseDialogueManager로 설정

    public static SpeechRecognitionManager4 Instance; // Singleton Instance
    private DialogueManager currentDialogueManager;

    private bool isRecognizing = false;

    private string recognizedText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 현재 DialogueManager를 설정하는 메서드
    public void SetDialogueManager(DialogueManager dialogueManager)
    {
        currentDialogueManager = dialogueManager;
    }

    // 음성 인식의 결과를 여기에 저장한다고 가정
    public void StoreRecognitionResult(string result)
    {
        recognizedText = result;
    }

    // recognizedText를 반환하는 메서드 추가
    public string GetRecognizedText()
    {
        return recognizedText;
    }

    private void Start()
    {
        var config = SpeechConfig.FromSubscription("1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y", "koreacentral");
        recognizer = new SpeechRecognizer(config, "ko-KR");

        // 현재 Scene에 따라 DialogueManager를 동적으로 참조
        dialogueManager = FindCurrentDialogueManager();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager를 찾을 수 없습니다.");
        }
    }
    public void UpdateDialogueManager(GameObject activeCanvas)
    {
        dialogueManager = activeCanvas.GetComponentInChildren<BaseDialogueManager>();
        if (dialogueManager != null)
        {
            Debug.Log($"새로운 DialogueManager가 설정되었습니다: {dialogueManager.name}");
        }
        else
        {
            Debug.LogError("활성화된 Canvas에서 DialogueManager를 찾을 수 없습니다.");
        }
    }

    private BaseDialogueManager FindCurrentDialogueManager()
    {
        // FCanvas는 모든 Canvas의 부모 오브젝트
        var fCanvas = GameObject.Find("FT_Canvas");
        if (fCanvas == null)
        {
            Debug.LogError("FCanvas를 찾을 수 없습니다.");
            return null;
        }

        // FCanvas 하위의 활성화된 Canvas에서 DialogueManager 찾기
        foreach (Transform child in fCanvas.transform)
        {
            if (child.gameObject.activeSelf) // 활성화된 Canvas만 검색
            {
                var dialogueManager = child.GetComponentInChildren<BaseDialogueManager>();
                if (dialogueManager != null)
                {
                    Debug.Log($"현재 활성화된 Canvas의 DialogueManager를 찾았습니다: {dialogueManager.name}");
                    return dialogueManager;
                }
            }
        }

        Debug.LogError("활성화된 Canvas에서 DialogueManager를 찾을 수 없습니다.");
        return null; // 활성화된 Canvas가 없거나 DialogueManager가 없는 경우
    }

    // 음성 인식 시작
    public async Task StartRecognition()
    {
        if (isRecognizing)
        {
            Debug.Log("이미 음성 인식 중입니다.");
            return;
        }

        isRecognizing = true;
        Debug.Log("음성 인식 중...");

        var result = await recognizer.RecognizeOnceAsync();
        isRecognizing = false;

        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            recognizedText = result.Text; // 인식된 텍스트를 저장
            Debug.Log($"인식된 텍스트: {recognizedText}");
            uiManager.DisplaySTTResult(recognizedText);

            // DialogueManager에 전달
            if (dialogueManager != null)
            {
                dialogueManager.OnUserResponse(recognizedText);
            }
            else
            {
                Debug.LogError("현재 DialogueManager가 설정되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"음성 인식 실패: {result.Reason}");
        }
    }

    // 음성 인식 종료
    public void StopRecognition()
    {
        if (isRecognizing)
        {
            isRecognizing = false;
            Debug.Log("음성 인식이 강제로 종료되었습니다.");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System;
using System.IO;
using UnityEngine.SceneManagement;


public class StartSpeechRecognition3: MonoBehaviour
{
    public SpeechRecognitionManager3 speechManager;
    public Button startButton;
    public SpeechSynthesisManager ttsManager; // SpeechSynthesisManager 참조 추가
    public PronunciationEvaluationManager pronunciationEvaluationManager;

    private bool isEvaluatingPronunciation = false; // 발음 평가 중 여부
    private bool isSceneTransitioning = false; // 장면 전환 중 여부
    private AudioClip recordedClip;
    private string microphoneDevice;

    void Start()
    {
        // 버튼 클릭 시 음성 인식 시작 함수 호출
        startButton.onClick.AddListener(() => StartRecognitionWithoutTimeout().ConfigureAwait(false));
    }
    public void OnSpeechButtonPressed()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            StartRecording(); // 녹음 시작

            // 음성 인식을 위한 SpeechRecognitionManager의 메서드를 호출
            StartRecognitionAndProcess(); // 변경된 메서드 호출
        }
        else
        {
            Debug.LogError("마이크 장치가 감지되지 않았습니다.");
        }
    }
    private void StartRecording()
    {
        recordedClip = Microphone.Start(microphoneDevice, false, 15, 16000); // 최대 15초 녹음
        Debug.Log("녹음이 시작되었습니다.");
    }

    private void StopRecording()
    {
        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice);
            Debug.Log("녹음이 종료되었습니다.");

            // 녹음된 파일을 저장합니다.
            string filePath = SaveRecording(); // 녹음 파일을 저장하고 파일 경로를 반환받음

            // 현재 씬의 DialogueManager에 녹음된 파일 경로 전달
            var dialogueManager = FindCurrentDialogueManager();
            if (dialogueManager != null)
            {
                dialogueManager.SetRecordedFilePath(filePath);
            }
            else
            {
                Debug.LogError("DialogueManager를 찾을 수 없습니다.");
            }
        }
    }
    private BaseDialogueManager FindCurrentDialogueManager()
    {
        // 현재 씬에 따라 DialogueManager를 동적으로 참조합니다.
        if (SceneManager.GetActiveScene().name == "Scene01_SchoolOutside")
            return FindObjectOfType<DialogueManager>(); // 예시로 각 씬의 DialogueManager 클래스를 다르게 지정
        else if (SceneManager.GetActiveScene().name == "Scene02_Board")
            return FindObjectOfType<DialogueManager2>();
        else if (SceneManager.GetActiveScene().name == "Scene03_Gym")
            return FindObjectOfType<DialogueManager3>();
        else if (SceneManager.GetActiveScene().name == "Scene04_Library")
            return FindObjectOfType<DialogueManager4>();
        else
            return null; // 해당 씬에 맞는 DialogueManager가 없을 경우
    }
    private string SaveRecording()
    {
        // 고유한 파일 이름 생성 (UUID 사용 또는 날짜/시간 사용)
        string fileName = $"recordedAudio_{System.DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (recordedClip == null)
        {
            Debug.LogError("녹음된 오디오 클립이 없습니다.");
            return string.Empty;
        }

        // 녹음된 오디오를 WAV 파일로 저장하는 로직
        var samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        byte[] wavFile = WavUtility.FromAudioClip(recordedClip); // WAV 유틸리티를 사용하여 변환
        File.WriteAllBytes(filePath, wavFile);
        Debug.Log($"녹음된 파일이 저장되었습니다: {filePath}");

        return filePath; // 저장된 파일 경로 반환
    }

    // 음성 인식을 시작하는 함수, TTS 중에는 음성 인식을 시작하지 않음
    public async Task StartRecognitionWithoutTimeout()
    {
        // TTS가 진행 중이면 녹음 버튼을 무시
        if (ttsManager.isSpeaking || isEvaluatingPronunciation || isSceneTransitioning)
        {
            Debug.Log("녹음 버튼이 비활성화되었습니다.");
            return;
        }

        Debug.Log("음성 인식 버튼이 클릭되었습니다.");
        StartRecording(); // 녹음 시작
        await StartRecognitionAndProcess(); // 비동기 작업을 기다립니다
    }

    // 새로 추가한 비동기 음성 인식 및 발음 평가 메서드
    private async Task StartRecognitionAndProcess()
    {
        try
        {
            await speechManager?.StartRecognition();  // 음성 인식 시작 및 대기

            StopRecording();

            // 발음 평가 시작: 오디오 파일 경로와 예상 텍스트 전달
            string userResponse = speechManager.GetRecognizedText();  // 음성 인식 결과 가져오기

            if (string.IsNullOrEmpty(userResponse))
            {
                Debug.LogError("음성 인식 결과가 비어 있습니다.");
                return;
            }

            Debug.Log($"인식된 텍스트: {userResponse}");

            string expectedText = GetExpectedTextFromDialogueManager(userResponse);
            if (string.IsNullOrEmpty(expectedText))
            {
                Debug.LogError("예상 텍스트가 비어 있습니다.");
                return;
            }

            string audioFilePath = SaveRecording();

            if (!IsFileReady(audioFilePath))
            {
                Debug.LogError("오디오 파일 경로가 올바르지 않거나 파일이 존재하지 않습니다.");
                return;
            }

            StartPronunciationEvaluation(audioFilePath, expectedText);
        }
        catch (Exception ex)
        {
            Debug.LogError($"음성 인식 처리 중 오류 발생: {ex.Message}");
        }
    }

    private bool IsFileReady(string filePath)
    {
        int maxAttempts = 10;
        int attempt = 0;
        while (!File.Exists(filePath) && attempt < maxAttempts)
        {
            System.Threading.Thread.Sleep(100); // 100ms 지연
            attempt++;
        }
        return File.Exists(filePath);
    }
    // 예상 텍스트를 가져오는 메서드
    private string GetExpectedTextFromDialogueManager(string userResponse)
    {
        var dialogueManager = FindCurrentDialogueManager();
        if (dialogueManager != null)
        {
            // 현재 대화에서 예상되는 텍스트 가져오기 (사용자의 상황에 맞게 메서드 구현 필요)
            return dialogueManager.GetExpectedResponse(userResponse);
        }
        else
        {
            Debug.LogError("DialogueManager를 찾을 수 없습니다.");
            return string.Empty;
        }
    }


    // 발음 평가가 시작될 때 호출하는 함수
    public void StartPronunciationEvaluation(string audioFilePath, string expectedText)
    {
        isEvaluatingPronunciation = true;

        // 발음 평가를 위해 두 개의 매개변수 (오디오 파일 경로와 예상 텍스트) 전달
        pronunciationEvaluationManager.EvaluatePronunciation(audioFilePath, expectedText);
    }


    // 발음 평가가 종료될 때 호출하는 함수
    public void EndPronunciationEvaluation()
    {
        isEvaluatingPronunciation = false;
    }

    // 장면 전환이 시작될 때 호출하는 함수
    public void StartSceneTransition()
    {
        isSceneTransitioning = true;
    }

    // 장면 전환이 완료될 때 호출하는 함수
    public void EndSceneTransition()
    {
        isSceneTransitioning = false;
    }
}
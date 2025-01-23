using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class SpeechToText : MonoBehaviour
{
    public Button startButton;  // 음성 인식을 시작하는 버튼
    public Button playRecordingButtonPanelToActivate; // panelToActivate에 있는 PlayRecordingButton
    public Button playRecordingButtonFinalFailPanel; // finalFailPanel에 있는 PlayRecordingButton

    private bool isPanelToActivateButtonClicked = false; // panelToActivate 버튼 클릭 여부 추적
    private bool isFinalFailPanelButtonClicked = false;  // finalFailPanel 버튼 클릭 여부 추적
    public static SpeechToText Instance;

    public Image playRecordingButtonImagePanelToActivate; // panelToActivate 버튼의 이미지
    public Image playRecordingButtonImageFinalFailPanel;  // finalFailPanel 버튼의 이미지
    private Vector3 originalScalePanelToActivate;
    private Vector3 originalScaleFinalFailPanel;

    public Button stopRecordButton; // StopRecordButton 추가
    public Button nextButton;
    //public Text resultText;     // 인식된 텍스트를 표시하는 UI 텍스트
    //public Text assessmentText; // 발음 평가 결과를 표시하는 UI 텍스트

    public GameObject panel1;
    public GameObject panel2;  // 특정 UI 패널 (panel2)
    public GameObject panel3;  // 특정 UI 패널 (panel3)
    public GameObject panelToActivate;  // 특정 이벤트 후 활성화할 패널
    public GameObject againPanel; // 3번 이하 실패일 때 활성화할 패널
    public GameObject finalFailPanel; //3번째 실패일 때 활성화할 패널
    public GameObject image1;
    public GameObject defaultImage;  // 기본 이미지
    public GameObject recordImage;   // 녹음 중 이미지
    public GameObject successImage;  // 단어 성공 이미지
    public GameObject failImage;     // 단어 실패 이미지

    public GameObject text1;     // PanelToActivate 안의 text1
    public GameObject text2;      // PanelToActivate 안의 text2
    public GameObject ftext1;     // FinalFailPanel 안의 ftext1
    public GameObject ftext2;     // FinalFailPanel 안의 ftext2
    public GameObject text3;     // 3번째 단어일 때 활성화할 text3
    public GameObject ftext3;
    public ParticleSystem confettiParticle;  // 파티클 시스템 변수
    private int currentWordIndex = 0; // 현재 단어의 인덱스를 추적하기 위한 변수
    private BGMController bgmController;  // BGMController 참조

    private SpeechRecognizer recognizer;  // 음성 인식기를 위한 변수
    private string subscriptionKey = "1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y"; // Azure 구독 키 (실제 키로 대체해야 함)
    private string region = "koreacentral"; // Azure 지역 (실제 지역으로 대체해야 함)
    private string language = "ko-KR";     // 음성 인식 언어 (한국어)
    private SpeechConfig config;           // 음성 인식 구성 변수


    private SynchronizationContext unityContext; // Unity 메인 스레드에서 UI 업데이트를 위한 컨텍스트

    //private bool stopButtonClicked = false;  // 중지 버튼이 클릭되었는지 여부를 추적
    private int attemptCount = 0;  // 발음 시도 횟수
    private string audioFilePath;  // 녹음 파일 경로 저장
    private AudioClip recordedClip; // 녹음된 오디오 클립
    private string microphoneName = null; // 기본 마이크 장치 이름
    private bool isRecording = false;
    //private int currentRecordingIndex = 0;  // 녹음본 인덱스
    //private string resourcesPath = "Resources/Audio/";
    private string sessionFolderPath;
    private int recordingCount = 1;  // 녹음 횟수를 추적하기 위한 변수
    private SpeechRecognitionResult recognizedResult;  // 인식된 결과 저장 변수
    private string recognizedText;  // 인식된 텍스트를 저장하는 변수

    // recognizedText를 저장할 리스트 추가
    List<string> recognizedTexts = new List<string>(); // 빈 리스트로 초기화
    List<string> videoPaths = new List<string>();
    private List<string> selectedWords;
    private List<AudioClip> audioClips = new List<AudioClip>(); // 녹음된 AudioClip 리스트
    List<float> accuracyScores = new List<float>(); // accuracyScore를 저장할 리스트
    private List<string> feedbacks = new List<string>();
    void Start()
    {
        bgmController = FindObjectOfType<BGMController>();  // 씬에서 BGMController 찾기
        // Unity의 SynchronizationContext를 캡처하여 메인 스레드에서 UI를 업데이트할 수 있도록 함
        unityContext = SynchronizationContext.Current;

        // SpeechConfig 초기화: 구독 키와 지역을 사용하여 설정
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        config.SpeechRecognitionLanguage = language; // 음성 인식 언어 설정

        // startButton에 클릭 이벤트 리스너 추가
        startButton.onClick.AddListener(() =>
        {
            // successImage 또는 recordImage가 활성화되어 있는 경우, 아무 동작도 하지 않음
            if (recordImage.activeSelf || successImage.activeSelf || failImage.activeSelf)
            {
                Debug.Log("현재 상태에서는 startButton을 눌러도 동작하지 않음.");
                return; // 클릭해도 아무 동작 안 함
            }

            // 위 조건을 통과하면 음성 인식 시작
            StartRecognition(); // 음성 인식 및 녹음 시작
        });
        // stopRecordButton에 클릭 이벤트리스너 추가
        stopRecordButton.onClick.AddListener(() =>
        {
            // 녹음을 중지하는 함수 호출
            StopRecording();
        });

        // 버튼 이미지의 초기 크기를 저장
        originalScalePanelToActivate = playRecordingButtonPanelToActivate.transform.localScale;
        originalScaleFinalFailPanel = playRecordingButtonFinalFailPanel.transform.localScale;

        // PlayRecordingButton 클릭 시 오디오 재생 및 이미지 애니메이션 실행
        playRecordingButtonPanelToActivate.onClick.AddListener(() =>
        {
            if (!isPanelToActivateButtonClicked) // 버튼이 한 번도 클릭되지 않았을 때만 동작
            {
                PlayRecording(playRecordingButtonImagePanelToActivate);  // 이미지 애니메이션 추가
                text1.SetActive(true);
                text2.SetActive(false);
                isPanelToActivateButtonClicked = true; // 버튼 클릭 플래그 설정
            }
            else
            {
                Debug.Log("playRecordingButtonPanelToActivate는 한 번만 눌릴 수 있습니다.");
            }
        });

        playRecordingButtonFinalFailPanel.onClick.AddListener(() =>
        {
            if (!isFinalFailPanelButtonClicked) // 버튼이 한 번도 클릭되지 않았을 때만 동작
            {
                PlayRecording(playRecordingButtonImageFinalFailPanel);  // 이미지 애니메이션 추가
                ftext1.SetActive(true);
                ftext2.SetActive(false);
                isFinalFailPanelButtonClicked = true; // 버튼 클릭 플래그 설정
            }
            else
            {
                Debug.Log("playRecordingButtonFinalFailPanel은 한 번만 눌릴 수 있습니다.");
            }
        });
        // 패널 초기화 시 초기화 함수에 버튼 플래그 리셋 포함
        ResetPanelToActivate();
        ResetFinalFailPanel();
        stopRecordButton.onClick.AddListener(StopRecording);  // StopRecordButton 자동 클릭 설정

        // 사용 가능한 마이크 장치 이름 설정
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("마이크를 찾을 수 없습니다.");
        }

        // 새로운 게임 세션이 시작될 때, 고유한 세션 폴더를 생성
        CreateNewSessionFolder();
        // 초기 비활성화
        text2.SetActive(false);
        ftext2.SetActive(false);
        text3.SetActive(false);
        ftext3.SetActive(false);
        startButton.gameObject.SetActive(false);
        finalFailPanel.gameObject.SetActive(false);
        againPanel.gameObject.SetActive(false);
        panelToActivate.SetActive(false);
        image1.SetActive(false);
        recordImage.SetActive(false);
        successImage.SetActive(false);
        failImage.SetActive(false);
        stopRecordButton.gameObject.SetActive(false);  // 초기에는 비활성화
        confettiParticle.Stop();
    }
    // 새로운 세션 폴더 생성 메서드
    void CreateNewSessionFolder()
    {
        string baseFolder = Path.Combine(Application.persistentDataPath, "AudioFiles");

        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        // 고유한 세션 폴더 이름 생성 (Session_1, Session_2, ...)
        int sessionNumber = 1;
        do
        {
            sessionFolderPath = Path.Combine(baseFolder, $"Session_{sessionNumber}");
            sessionNumber++;
        } while (Directory.Exists(sessionFolderPath));

        // 세션 폴더 생성
        Directory.CreateDirectory(sessionFolderPath);

        // PlayerPrefs에 세션 넘버 저장
        PlayerPrefs.SetInt("sessionNumber", sessionNumber - 1);
        PlayerPrefs.Save(); // 저장 강제 적용

        Debug.Log($"새로운 세션 폴더 생성됨: {sessionFolderPath}");
        Debug.Log($"저장된 세션 넘버: {PlayerPrefs.GetInt("sessionNumber")}");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }

    }
    /// 음성인식과 음성녹음 시작 메서드
    async void StartRecognition()
    {
        if (recognizer != null)
        {
            // 이미 recognizer가 실행 중이면 먼저 중지하고 리소스를 해제
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognized -= RecognizedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.Dispose();
            recognizer = null;
        }

        // 녹음 시작 이미지로 전환
        successImage.SetActive(false);
        defaultImage.SetActive(false);
        recordImage.SetActive(true);

        // 음성 인식 설정
        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        recognizer = new SpeechRecognizer(config, audioConfig);
        recognizer.Recognized += RecognizedHandler;
        recognizer.Canceled += CanceledHandler;

        // 녹음 시작
        StartRecording(); // 여기서 녹음 시작
        stopRecordButton.gameObject.SetActive(true);  // StopRecordButton 활성화
        Debug.Log("음성인식 시작");
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    }

    // 녹음 시작 메서드
    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("녹음할 마이크 장치가 없습니다.");
            return;
        }

        if (isRecording || Microphone.IsRecording(null))
        {
            Debug.LogWarning("이미 녹음 중입니다.");
            return;
        }

        isRecording = true;
        recordedClip = Microphone.Start(null, false, 3, 44100);
        if (Microphone.IsRecording(null))
        {
            Debug.Log("녹음 중...");
        }
        else
        {
            Debug.LogError("녹음이 시작되지 않았습니다.");
        }
    }


    /// 음성인식결과를 처리하는 이벤트 핸들러
    private async void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        // 음성인식이 성공한 경우
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            // 사용자가 실제로 말한 내용을 인식
            recognizedText = e.Result.Text.TrimEnd('.');
            Debug.Log($"음성 인식 성공! 인식 결과: '{recognizedText}'");

            // 현재 단어의 인덱스를 업데이트
            currentWordIndex = WordGameController.Instance.GetCurrentWordIndex(); // 단어 인덱스 가져오기

            // Step 1: 음성 인식 결과와 목표 단어 비교
            string targetWord = WordGameController.Instance.GetCurrentWord(); // 목표 단어
            Debug.Log($"목표 단어: '{targetWord}', 인식된 텍스트: '{recognizedText}'");



            // 인식된 텍스트가 현재 게임의 단어와 일치하는지 확인
            if (recognizedText.Equals(targetWord, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("단어 일치");

                StopRecognition();

                unityContext.Post(_ =>
                {
                    defaultImage.gameObject.SetActive(false);
                    recordImage.gameObject.SetActive(false);
                    successImage.gameObject.SetActive(true);
                }, null);


                // 1초 대기 후 활성화,비활성화
                await Task.Delay(1500);
                unityContext.Post(_ =>
                {
                    panel1.gameObject.SetActive(false);
                    panel2.gameObject.SetActive(false);          // panel2 비활성화
                    panel3.gameObject.SetActive(false);          // panel3 비활성화
                    startButton.gameObject.SetActive(false);

                    // 패널을 활성화하기 전에 상태 초기화
                    ResetPanelToActivate();  // text1을 활성화하고 text2를 비활성화

                    panelToActivate.gameObject.SetActive(true);  // panelToActivate 활성화
                    WordGameController.Instance.IncrementSlider(); // 슬라이더 업데이트
                    defaultImage.gameObject.SetActive(true);
                    successImage.gameObject.SetActive(false);
                }, null);


                // 발음 평가 수행
                //PerformPronunciationAssessment(audioFilePath);

            }
            else // 인식된 텍스트와 선택된 단어가 일치하지 않으면
            {
                if (attemptCount < 2)
                {
                    //시도 횟수 증가
                    attemptCount++;
                    Debug.Log($"단어가 일치하지 않음. 시도 횟수: {attemptCount}/3");
                    StopRecognition(); // 음성인식과 녹음 중단

                    //실패 이미지로 전환
                    unityContext.Post(_ =>
                    {
                        defaultImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(true);
                    }, null);

                    // 1초 대기 후 다시 해보자는 패널 활성화
                    await Task.Delay(1000);
                    unityContext.Post(_ =>
                    {
                        againPanel.SetActive(true);  // againpanel 활성화
                    }, null);

                    // 2.5초 대기 후 패널 비활성화하고 다시 녹음 시작
                    await Task.Delay(2500);
                    unityContext.Post(_ =>
                    {
                        againPanel.SetActive(false);  // panelToActivate 비활성화
                        defaultImage.gameObject.SetActive(true);
                        failImage.gameObject.SetActive(false);
                    }, null);
                }


                //if (attemptCount == 3) //3번째 틀렸을 때
                else if (attemptCount == 2)
                {
                    //시도 횟수 증가
                    attemptCount++;

                    Debug.Log("3번째 시도 실패");

                    StopRecognition();

                    //실패 이미지로 전환
                    unityContext.Post(_ =>
                    {
                        defaultImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                        successImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(true);
                    }, null);



                    // 1초 대기 후 활성화,비활성화
                    await Task.Delay(1500);
                    unityContext.Post(_ =>
                    {
                        panel1.gameObject.SetActive(false);
                        panel2.gameObject.SetActive(false);          // panel2 비활성화
                        panel3.gameObject.SetActive(false);          // panel3 비활성화
                        startButton.gameObject.SetActive(false);

                        // 패널을 활성화하기 전에 상태 초기화
                        ResetFinalFailPanel();  // ftext1을 활성화하고 ftext2를 비활성화

                        finalFailPanel.gameObject.SetActive(true);  // panelToActivate 활성화

                        WordGameController.Instance.IncrementSlider(); // 슬라이더 업데이트
                        defaultImage.gameObject.SetActive(true);
                        successImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                    }, null);

                    // 발음 평가 수행
                    //PerformPronunciationAssessment(audioFilePath);
                }

            }


        }
        else
        {
            Debug.LogWarning($"음성인식 실패 이유: {e.Result.Reason}");
        }
    }

    private async void PerformPronunciationAssessment(string audioFilePath, string targetWord, string recognizedText)
    {
        var pronunciationConfig = new PronunciationAssessmentConfig(
            targetWord,  // 목표 단어 설정
            GradingSystem.HundredMark,
            Granularity.Phoneme,  // 음소 단위 평가를 활성화
            true);

        var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);  // 녹음된 파일을 입력으로 사용

        using (var recognizer = new SpeechRecognizer(config, audioConfig))
        {
            pronunciationConfig.ApplyTo(recognizer);  // 발음 평가 설정을 적용

            var result = await recognizer.RecognizeOnceAsync();  // 녹음된 파일로 발음 평가 진행

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var pronunciationResult = PronunciationAssessmentResult.FromResult(result);
                if (pronunciationResult != null)
                {
                    // 1. 단어 전체에 대한 정확도 점수 출력
                    var accuracyScore = pronunciationResult.AccuracyScore;
                    accuracyScores.Add((float)accuracyScore);  // 발음 평가 점수를 accuracyScores 리스트에 추가
                    Debug.Log($"발음 평가 - 단어 전체 정확도 점수: {accuracyScore}");

                    // 2. 음소 단위로 평가된 결과 출력
                    List<string> targetPhonemes = KoreanPhonemeSplitter.SplitIntoPhonemes(targetWord); // 목표 단어를 음소로 분리
                    List<string> recognizedPhonemes = KoreanPhonemeSplitter.SplitIntoPhonemes(recognizedText); // 인식된 단어를 음소로 분리
                    float lowestPhonemeScore = 100.0f;  // 가장 낮은 음소의 점수를 추적
                    string lowestPhoneme = "";  // 가장 낮은 점수를 받은 음소
                    string feedback = "";  // 피드백을 저장할 변수
                    string lowestPhonemePosition = "";  // 해당 음소의 위치 정보 (초성, 중성, 종성)
                    int lowestPhonemeIndex = -1;  // 가장 낮은 점수를 받은 음소의 인덱스

                    // 3. 음소 단위로 비교 및 피드백 생성
                    int phonemeIndex = 0;
                    foreach (var wordDetail in pronunciationResult.Words)
                    {
                        foreach (var phoneme in wordDetail.Phonemes)
                        {
                            if (phonemeIndex < targetPhonemes.Count)
                            {
                                string targetPhoneme = targetPhonemes[phonemeIndex];
                                string recognizedPhoneme = (phonemeIndex < recognizedPhonemes.Count) ? recognizedPhonemes[phonemeIndex] : "없음";  // 인식된 음소가 없으면 "없음"
                                string syllable;  // out 매개변수를 받을 변수 선언
                                string phonemePosition = KoreanPhonemeSplitter.GetPhonemePosition(targetWord, phonemeIndex, out syllable);

                                Debug.Log($"음소: {targetPhoneme} / 비교된 음소: {recognizedPhoneme} / 정확도: {phoneme.AccuracyScore} / 위치: {phonemePosition}");

                                // 가장 낮은 점수의 음소 찾기
                                if (phoneme.AccuracyScore < lowestPhonemeScore)
                                {
                                    lowestPhonemeScore = (float)phoneme.AccuracyScore;
                                    lowestPhoneme = targetPhoneme;
                                    lowestPhonemePosition = phonemePosition;
                                    lowestPhonemeIndex = phonemeIndex;
                                }

                                phonemeIndex++;
                            }
                        }
                    }

                    // 4. 가장 낮은 음소가 속한 음절을 찾고, 초성, 중성, 종성 위치를 찾아 피드백에 포함
                    if (lowestPhonemeIndex >= 0)
                    {
                        string syllable = "";  // 음소가 속한 음절을 저장할 변수
                        string phonemePosition = KoreanPhonemeSplitter.GetPhonemePosition(targetWord, lowestPhonemeIndex, out syllable); // 음소 위치와 음절 찾기

                        // 피드백 생성
                        feedback = $"\r\n<b><size=42><color=#8388FF>'{targetWord}'에서 {syllable}의 {phonemePosition} {lowestPhoneme}의 발음이 부족합니다.</color></size></b>";

                        // 5. 가장 낮은 음소에 대한 피드백 생성
                        switch (lowestPhoneme)
                        {
                            case "ㄱ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄱ 소리의 특징과 올바른 발음 방법> - </size></b> \r\nㄱ은 무성 파열음으로, 성대가 진동하지 않고 혀의 뒷부분(연구개)을 입천장 가까이에 대어 소리를 냅니다. 공기가 차단되었다가 풀리면서 소리가 나야 하며, 혀의 힘을 적절히 조절해 부드럽게 소리가 이어지도록 합니다. \r\n\r\n <b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀를 입천장에 너무 강하게 대면 소리가 딱딱하거나 막힐 수 있습니다. 이럴 때는 손을 목에 대어 진동을 느끼게 하여 혀의 긴장을 풀고, 소리가 자연스럽게 이어지도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n목에 손을 대어 진동을 느끼거나, 거울을 사용해 혀의 위치를 시각적으로 확인할 수 있게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㄱ\" 소리를 반복해서 내며, 혀의 위치와 진동을 느낍니다.\r\n간단한 음절 연습 : \"가\", \"거\" 같은 음절을 반복해 연습합니다.\r\n";
                                break;
                            case "ㄴ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄴ 소리의 특징과 올바른 발음 방법> - </size></b> \r\nㄴ은 비음으로, 성대가 진동하며 혀끝을 윗니 뒤쪽(치경)에 닿게 하여 소리를 냅니다. 공기가 코를 통해 원활하게 나가도록 해야 하며, 혀의 움직임이 자연스러워야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀가 너무 강하게 닿거나 공기가 코를 통해 잘 빠져나가지 않으면 소리가 막히거나 흐릿하게 들릴 수 있습니다. 코를 막고 발음해 보게 하여 구강 내 공기의 흐름을 느끼게 하고, 혀의 위치를 적절히 조절합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b>\r\n코를 막고 발음하면서 입 안에서 공기가 차오르는 느낌을 느껴보도록 하고, 거울을 사용해 혀의 위치를 시각적으로 확인하게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㄴ\" 소리를 반복하여 혀끝이 올바른 위치에 닿아 있는지 느껴봅니다.\r\n간단한 음절 연습 : \"나\", \"노\" 같은 짧은 소리를 반복하며 발음을 연습합니다.\r\n";
                                break;
                            case "ㄷ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄷ 소리의 특징과 올바른 발음 방법> - </size></b> \r\nㄷ은 무성 파열음으로, 성대가 진동하지 않고 혀끝을 윗니 뒤쪽(치경)에 가볍게 대고 떼면서 소리를 냅니다. 공기가 차단되었다가 터지듯이 나와야 하며, 혀의 움직임을 부드럽게 조절합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀를 너무 세게 대어 소리가 딱딱하게 들리거나, 혀가 잘 움직이지 않아 소리가 흐릿할 수 있습니다. 혀의 긴장을 풀고, 공기가 자연스럽게 흘러나가도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 보면서 혀의 움직임을 관찰하고, 손으로 입 앞에서 나오는 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㄷ\" 소리를 반복하며 혀의 위치와 공기 흐름을 느낍니다.\r\n간단한 음절 연습 : \"다\", \"도\" 같은 간단한 소리를 연습해 부드럽게 발음하도록 합니다.\r\n";
                                break;
                            case "ㄹ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄹ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㄹ은 치경 접근음으로, 혀끝을 윗니 바로 뒤쪽에 대고 떨림 없이 소리를 냅니다. 혀가 부드럽게 떨지 않고 정확하게 닿아야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀를 너무 세게 떨거나, 떨림이 부족해서 소리가 불명확할 수 있습니다. 혀의 힘을 조절해 떨지 않도록 하며, 소리가 자연스럽게 이어지도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n혀끝이 윗니 뒤에 닿는 순간을 느껴보도록 하고, 거울을 사용해 혀의 위치를 확인할 수 있게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㄹ\" 소리를 반복하면서 혀가 올바른 위치에 있는지 확인합니다.\r\n간단한 음절 연습 : \"라\", \"루\" 같은 소리를 반복합니다.\r\n";
                                break;
                            case "ㅁ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅁ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅁ은 비음으로, 성대가 진동하며 입술을 닫고 코로 공기를 내보내면서 소리를 냅니다. 코를 통한 공기 흐름이 원활해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n입술을 너무 꽉 닫거나, 코를 통해 공기가 잘 나오지 않으면 소리가 약하거나 막힐 수 있습니다. 코를 막고 발음해 보게 하여 공기 흐름을 느끼게 하고, 입술의 긴장을 풀어줍니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n코를 손으로 막고 발음해 보면서 구강 내 공기 흐름을 인식하게 하고, 코에서 진동이 느껴지는지 확인해 주세요.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅁ\" 소리를 반복하여 코로 공기가 나가는지 확인합니다.\r\n간단한 음절 연습 : \"마\", \"모\" 같은 짧은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅂ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅂ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅂ은 무성 파열음으로, 성대가 진동하지 않으며 입술을 가볍게 닫았다가 열며 소리를 냅니다. 공기가 터지듯 나오는 것이 특징입니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n입술을 너무 세게 닫거나, 소리가 약하게 들리면 발음이 부정확해질 수 있습니다. 입술의 힘을 적절히 조절해 부드럽게 소리가 이어지도록 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 입술이 닫히고 열리는 모습을 확인하고, 손으로 입 앞에서 공기의 흐름을 느끼게 해 주세요.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅂ\" 소리를 반복하여 입술의 움직임을 확인합니다.\r\n간단한 음절 연습 : \"바\", \"보\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅅ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅅ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅅ은 무성 마찰음으로, 성대가 진동하지 않으며 혀끝을 아랫니 근처에 가깝게 두고 공기를 흘려보내면서 소리를 냅니다. 공기가 부드럽게 흐르도록 해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀를 너무 앞으로 내밀거나, 공기가 지나치게 세게 나올 수 있습니다. 혀의 위치를 조절하여 공기가 부드럽게 흐르도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n거울을 사용해 혀의 위치를 시각적으로 확인하고, 손으로 입 앞에서 나오는 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅅ\" 소리를 반복하여 혀의 위치와 공기 흐름을 조절합니다.\r\n간단한 음절 연습 : \"사\", \"수\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅇ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅇ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅇ은 무성 마찰음으로, 성대가 진동하지 않고 목소리만으로 발음하며, 혀와 입술의 움직임을 최소화해야 합니다. 공기가 자연스럽게 목에서 나오도록 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀와 입술이 지나치게 움직이거나 목소리로만 소리를 내지 못할 때가 있습니다. 이럴 때는 손을 목에 대어 진동을 느끼게 하고, 소리가 목구멍에서 나오는 것을 인식하게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n손을 목에 대어 진동을 느끼며 소리가 자연스럽게 나오는지 확인하고, 거울을 통해 혀와 입술이 움직이지 않는지 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅇ\" 소리를 반복하여 목에서 소리가 부드럽게 나오는지 느껴봅니다.\r\n간단한 음절 연습 : \"아\", \"오\" 같은 간단한 소리를 연습합니다.\r\n";
                                break;
                            case "ㅈ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅈ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅈ은 무성 파찰음으로, 성대가 진동하지 않고 혀끝을 윗니 뒤에 가깝게 대며 발음합니다. 공기를 부드럽게 흘려보내며 소리가 나도록 해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n소리가 너무 세게 나오거나 혀의 위치가 정확하지 않아서 소리가 불명확할 수 있습니다. 혀의 위치를 거울로 보며 조절하고, 소리를 부드럽게 내도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 혀의 위치를 시각적으로 확인하고, 손으로 입 앞에서 나오는 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㅈ\" 소리를 반복하여 혀가 올바른 위치에 있는지 확인합니다.\r\n간단한 음절 연습 : \"자\", \"주\" 같은 소리를 반복합니다.\r\n";
                                break;
                            case "ㅊ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅊ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅊ은 무성 파찰음으로, 성대가 진동하지 않으며 혀끝을 윗니 뒤에 가깝게 대고 공기를 터뜨리듯 소리를 냅니다. 소리가 부드럽게 나와야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀가 너무 강하게 닿아 소리가 거칠게 나올 수 있습니다. 혀의 힘을 조금 풀고 부드럽게 발음하도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n거울을 통해 혀의 위치를 보며 연습하고, 손으로 입 앞에서 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㅊ\" 소리를 반복하여 혀끝의 위치와 공기의 흐름을 느껴봅니다.\r\n간단한 음절 연습 : \"차\", \"초\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅋ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅋ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅋ은 무성 파열음으로, 성대가 진동하지 않고 혀의 뒷부분을 입천장에 대고 소리를 냅니다. 공기를 강하게 터뜨리듯 발음해야 하며, 소리가 부드럽게 나와야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀를 너무 강하게 대어 소리가 딱딱하거나 막힐 수 있습니다. 혀의 긴장을 풀고, 공기가 자연스럽게 흘러나오도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n손을 목에 대고 진동을 느끼며 소리가 원활하게 나오는지 확인하고, 거울을 통해 혀의 위치를 시각적으로 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅋ\" 소리를 반복하여 혀의 움직임을 느껴봅니다.\r\n간단한 음절 연습 : \"카\", \"코\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅌ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅌ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅌ은 무성 파열음으로, 성대가 진동하지 않고 혀끝을 윗니 뒤쪽에 가볍게 대어 공기를 터뜨리듯 소리를 냅니다. 소리가 부드럽게 나오도록 해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀가 너무 강하게 닿아 소리가 딱딱하거나 막힐 수 있습니다. 혀의 긴장을 풀어 부드럽게 소리를 내도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 혀의 위치를 시각적으로 확인하고, 손으로 공기의 흐름을 느끼게 해 주세요.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㅌ\" 소리를 반복하며 혀가 올바른 위치에 있는지 확인합니다.\r\n간단한 음절 연습 : \"타\", \"토\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㅍ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅍ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅍ은 무성 파열음으로, 성대가 진동하지 않고 입술을 닫았다가 열면서 소리를 냅니다. 공기가 자연스럽게 터지듯 나오도록 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n입술을 너무 세게 닫거나 소리가 약하게 나올 수 있습니다. 입술의 긴장을 풀어 부드럽게 발음하도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n거울을 통해 입술의 움직임을 보게 하고, 손으로 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㅍ\" 소리를 반복하여 입술의 움직임을 확인합니다.\r\n간단한 음절 연습 : \"파\", \"포\" 같은 소리를 연습합니다.";
                                break;
                            case "ㅎ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅎ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅎ은 무성 마찰음으로, 성대가 진동하지 않고 목구멍에서 부드럽게 공기를 내보내며 발음합니다. 목이 조이지 않도록 주의해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n목을 지나치게 조이면 소리가 막히거나 약하게 나올 수 있습니다. 이때는 목을 편안하게 하고 자연스럽게 공기를 내뱉도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -</size></b> \r\n손을 목에 대고 진동을 느끼며 소리가 원활하게 나오는지 확인하고, 거울을 통해 공기의 흐름을 시각적으로 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅎ\" 소리를 반복하여 공기가 부드럽게 나오는지 느껴봅니다.\r\n간단한 음절 연습 : \"하\", \"호\" 같은 소리를 연습합니다.\r\n";
                                break;
                            case "ㄲ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄲ 소리의 특징과 올바른 발음 방법> - </size></b> \r\nㄲ은 된소리(경음) 파열음으로, 성대가 진동하지 않고 혀의 뒷부분(연구개)을 입천장에 강하게 대고 소리를 냅니다. 공기가 짧고 단단하게 터져 나와야 하며, 발음할 때 혀의 긴장을 적절히 유지하는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀에 과도한 힘을 주면 소리가 지나치게 딱딱하게 들릴 수 있고, 너무 약하면 된소리 특유의 단단함이 부족해질 수 있습니다. 이럴 때는 혀의 힘을 적절히 조절하고, 손을 목에 대어 진동을 느끼게 하여 발음을 부드럽게 이어가도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n손을 목에 대어 진동을 느껴보고, 거울을 사용해 혀의 위치를 시각적으로 확인하면서 소리가 단단하게 나는지 체크합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㄲ\" 소리를 반복하여 혀의 힘과 위치를 조절하며 단단하게 소리가 나오는지 확인합니다.\r\n간단한 음절 연습 : \"까\", \"꼬\" 등의 짧은 소리를 반복하며 발음의 정확성을 높입니다.\r\n";
                                break;
                            case "ㄸ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㄸ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㄸ은 된소리(경음) 파열음으로, 혀끝을 윗니 뒤쪽(치경)에 강하게 대고 소리를 냅니다. 공기가 짧고 단단하게 터져 나와야 하며, 발음할 때 혀끝에 적당한 힘을 줍니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀를 너무 세게 밀면 소리가 딱딱하거나 막힐 수 있으며, 반대로 힘이 부족하면 된소리 특유의 강한 발음이 나오지 않습니다. 혀의 긴장을 적절히 조절하고, 공기의 흐름을 인식하면서 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 혀끝이 윗니 뒤에 정확하게 닿아 있는지 시각적으로 확인하고, 손으로 공기의 흐름을 느껴보게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㄸ\" 소리를 반복하여 혀의 힘과 위치를 조절합니다.\r\n간단한 음절 연습 : \"따\", \"또\" 같은 짧은 소리를 반복하며 정확한 발음을 연습합니다.\r\n";
                                break;
                            case "ㅃ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅃ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅃ은 된소리(경음) 파열음으로, 입술을 강하게 닫았다가 열며 공기를 짧고 단단하게 터뜨리듯 발음합니다. 소리가 짧고 명확하게 들려야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n입술을 지나치게 강하게 닫으면 발음이 딱딱하게 들리고, 반대로 너무 약하면 된소리의 특징이 잘 나타나지 않을 수 있습니다. 입술의 힘을 적절히 조절하여 부드럽고 단단하게 소리를 이어가도록 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 입술이 닫히고 열리는 모습을 시각적으로 확인하고, 손으로 입 앞에서 나오는 공기의 흐름을 느껴봅니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅃ\" 소리를 반복하여 입술의 힘과 움직임을 조절합니다.\r\n간단한 음절 연습 : \"빠\", \"뽀\" 같은 짧은 소리를 반복하여 정확한 발음을 연습합니다.\r\n";
                                break;
                            case "ㅆ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅆ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅆ은 된소리(경음) 마찰음으로, 성대가 진동하지 않으며 혀끝을 아랫니 근처에 더 가까이 대고 강하게 공기를 내보내며 소리를 냅니다. 소리가 명확하고 강하게 나와야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b>\r\n혀를 너무 앞으로 내밀거나, 소리가 지나치게 강해질 수 있습니다. 혀의 위치를 조절하며 공기가 부드럽게 흐르도록 지도하고, 소리를 내는 동안 손으로 공기의 흐름을 느끼게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 통해 혀의 위치를 시각적으로 확인하며, 손으로 입 앞에서 나오는 공기의 흐름을 느끼도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -</size></b> \r\n소리 내기 연습 : \"ㅆ\" 소리를 반복하며 혀의 위치와 소리의 강도를 조절합니다.\r\n간단한 음절 연습 : \"싸\", \"쏘\" 같은 짧은 소리를 반복하여 발음의 정확성을 높입니다.\r\n";
                                break;
                            case "ㅉ":
                                feedback += " \r\n\r\n<b><size=40><자음 ㅉ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅉ은 된소리(경음) 파찰음으로, 성대가 진동하지 않고 혀끝을 윗니 바로 뒤에 강하게 대고 발음합니다. 소리가 짧고 강하게 터져 나와야 하며, 혀의 힘을 적절히 유지해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -</size></b> \r\n혀를 너무 세게 대어 소리가 거칠게 나올 수 있으며, 반대로 약하게 대면 발음의 강도가 부족할 수 있습니다. 혀의 힘을 조절하여 부드럽고 단단하게 발음하도록 지도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b>\r\n거울을 사용해 혀의 위치를 시각적으로 확인하고, 소리를 낼 때 손으로 공기의 흐름을 느껴봅니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b>\r\n소리 내기 연습 : \"ㅉ\" 소리를 반복하여 혀끝이 윗니 뒤에 정확히 닿아 있는지 확인합니다.\r\n간단한 음절 연습 : \"짜\", \"쭈\" 같은 짧은 소리를 반복하여 발음의 정확성을 높입니다.\r\n";
                                break;

                            // 모음 피드백
                            case "ㅏ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅏ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅏ는 열린 발음으로, 입을 크게 벌리고 목소리를 자연스럽게 내면서 발음합니다. 혀는 낮게 위치하며, 입천장과의 거리가 멀어야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n입을 너무 적게 벌리면 소리가 막힐 수 있습니다. 이럴 때는 거울을 보며 입을 크게 벌리는 연습을 하도록 유도합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n손으로 목을 가볍게 두드리며 소리를 내어 진동을 느끼게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"아\" 소리를 반복합니다.";
                                break;
                            case "ㅑ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅑ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅑ는 \"ㅏ\"와 유사하나, 시작할 때 \"이\" 소리의 요소가 포함됩니다. 혀는 더 앞으로 위치하며 입을 벌립니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n혀가 뒤로 가면 소리가 흐릿해질 수 있습니다. 혀의 위치를 조정하며 거울을 통해 시각적으로 확인하게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n혀의 위치를 느끼며 소리를 내는 동안 손을 입앞에 대어 공기의 흐름을 느끼도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"야\" 소리를 반복합니다.";
                                break;
                            case "ㅐ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅐ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅐ는 입을 넓게 벌리면서 발음하는 중간 모음입니다. 혀는 약간 앞으로 위치하고 입술은 자연스럽게 펴져야 합니다. 소리를 내는 동안 부드럽고 안정적으로 발음하는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅐ\" 소리를 발음할 때 자주 발생하는 문제는 소리가 \"ㅏ\"나 \"ㅔ\"와 혼동되는 것입니다. 이럴 경우, \"ㅏ\"를 발음한 뒤에 \"ㅔ\"로 연결하여 발음해 보며, 각 소리의 차이를 명확하게 인식할 수 있도록 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b> </size></b>\r\n입을 벌리고 손으로 입안의 모양을 느끼게 하여 올바른 위치를 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - \r\n소리 내기 연습: \"애 소리를 반복합니다.";
                                break;
                            case "ㅒ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅒ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅒ는 \"ㅐ\"와 유사하지만, \"이\"의 요소가 추가되어 더 부드럽게 발음됩니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅒ\"를 발음할 때 소리가 부정확하게 나올 수 있습니다. 이럴 경우, \"ㅐ\"와 \"ㅣ\"를 각각 발음해 본 후, 그 사이의 중간 소리를 의식적으로 연습하여 올바른 발음으로 연결할 수 있도록 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n손으로 목에 가볍게 대고 발음하면서 진동을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"얘\" 소리를 반복합니다.";
                                break;
                            case "ㅓ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅓ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅓ는 낮고 편안한 발음으로, 입을 약간 벌리고 혀는 중간 위치에 두어 발음합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n소리가 너무 높은 음으로 발음될 수 있습니다. 이럴 때는 목소리를 낮추어 발음하게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n손으로 목을 가볍게 두드리며 발음할 때의 진동을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"어\" 소리를 반복합니다.";
                                break;
                            case "ㅕ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅕ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅕ는 \"ㅓ\"를 발음할 때 혀를 앞쪽으로 살짝 이동시켜 발음하는 소리입니다. 입은 약간 벌린 상태에서 부드럽게 발음합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n혀의 위치가 부정확하면 소리가 부정확해질 수 있습니다. 거울을 사용해 발음할 때 혀를 앞쪽에 정확히 위치시키도록 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n혀의 위치를 느끼고 소리를 내면서 손으로 입앞에 대어 공기의 흐름을 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"여\" 소리를 반복합니다.";
                                break;
                            case "ㅔ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅔ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅔ는 \"ㅐ\"와 유사하지만, 발음할 때 혀를 약간 낮추고 입술에 긴장을 주어 소리를 명확하게 발음합니다. 혀는 아래쪽에 위치하고, 입은 옆으로 약간 더 벌려야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅔ\"를 발음할 때 소리가 불명확하게 들릴 수 있습니다. 이때는 \"ㅐ\"를 발음한 후 \"ㅔ\"로 전환해 보도록 하여 두 소리의 차이를 확실히 느끼며 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n혀의 위치를 느끼고 손으로 목을 가볍게 두드리며 소리를 내어 진동을 느끼게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"에\" 소리를 반복합니다.";
                                break;
                            case "ㅖ":
                                feedback += " \r\\r\n<b><size=40><모음 ㅖ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅖ는 \"ㅔ\"에 혀의 앞부분을 올려 발음하는 소리입니다. 입을 약간 벌리고 부드럽게 발음하며, 혀의 위치를 잘 조정하여 명확한 소리를 내는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅖ\"를 발음할 때 소리가 불명확하게 나올 수 있습니다. 이 경우, \"ㅣ\"와 \"ㅖ\"를 따로 연습하여 발음한 후, 두 소리를 자연스럽게 이어서 \"ㅖ\"로 발음해 보도록 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n입을 벌린 상태에서 소리를 내며 손으로 입앞에서 공기의 흐름을 느껴보도록 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"예\" 소리를 반복합니다.";
                                break;
                            case "ㅗ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅗ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅗ는 둥글게 입을 벌리고 발음하는 소리입니다. 혀는 중간 위치에서 약간 올라갑니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅗ\"를 발음할 때 입술이 너무 세게 긴장하면 소리가 흐릿하게 들릴 수 있습니다. 이 경우, 입술을 부드럽게 둥글게 하여 자연스럽게 발음하도록 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n입술을 손으로 만지며 부드럽게 움직이게 하고 소리를 내어 진동을 느끼게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"오\" 소리를 반복합니다.";
                                break;
                            case "ㅛ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅛ 소리의 특징과 올바른 발음 방법> - </size></b>r\nㅛ는 \"ㅣ\"의 발음에 \"ㅗ\"의 요소가 결합된 소리입니다. 입술을 둥글게 하면서 부드럽게 발음하고, 혀는 높게 위치시켜 자연스럽게 소리를 내는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n입술의 긴장이 부족할 경우 소리가 모호해질 수 있습니다. 이럴 때는 소리의 시작을 \"ㅣ\"로 명확하게 해보게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n소리를 내면서 입술의 움직임을 느끼고 확인하게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"요\" 소리를 반복합니다.";
                                break;
                            case "ㅚ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅚ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅚ는 \"ㅗ\"와 \"ㅣ\"의 요소가 결합된 소리입니다. 발음할 때 입술은 처음에 둥글게 하여 약간 벌리며, 발음이 진행됨에 따라 자연스럽게 \"ㅣ\"의 느낌이 더해져 입술이 살짝 넓어지는 형태가 됩니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n소리가 부정확할 수 있으며, 이때는 \"ㅗ\"와 \"ㅣ\"를 분리해 연습하게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n소리를 내면서 손으로 입술의 모양을 만져보아 확인하여 발음 시 입술의 위치와 형태를 느끼며 올바른 발음을 연습합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"외\" 소리를 반복합니다.";
                                break;
                            case "ㅘ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅘ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅘ는 \"ㅗ\"와 \"ㅏ\"의 결합으로, 입술은 처음에 둥글게 시작하여 \"ㅗ\"의 발음을 낸 후, 입술의 모양을 부드럽게 펴면서 \"ㅏ\"로 전환해야 합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅘ\"를 발음할 때 소리가 불명확하게 들릴 수 있습니다. 이럴 경우, \"ㅗ\"와 \"ㅏ\"를 각각 명확하게 발음하여 두 소리의 차이를 확실히 느끼도록 연습하는 것이 좋습니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n소리를 내면서 손으로 입술의 모양을 만져보아 확인하여 발음 시 입술의 위치와 형태를 느끼며 올바른 발음을 연습합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"와\" 소리를 반복합니다.";
                                break;
                            case "ㅙ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅙ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅙ는 \"ㅗ\"와 \"ㅐ\"의 결합으로 이루어진 소리입니다. 발음할 때 입술은 처음에 둥글게 유지하면서 약간 벌리게 됩니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅙ\"를 발음할 때 소리가 불명확하게 들릴 수 있습니다. 이럴 경우, \"ㅗ\"와 \"ㅐ\"를 각각 분리해 발음하여 두 모음의 차이를 확실히 느끼도록 연습하는 것이 좋습니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n소리를 내면서 손으로 입술의 모양을 만져보아 확인하여 발음 시 입술의 위치와 형태를 느끼며 올바른 발음을 연습합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"왜\" 소리를 반복합니다.";
                                break;
                            case "ㅜ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅜ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅜ는 입술을 둥글게 하고 혀는 중간 위치에 두어 발음합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n입술이 너무 많이 긴장하면 소리가 막힐 수 있습니다. 이럴 때는 입술을 부드럽게 조절하여 소리를 내게 합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n입술의 움직임을 손으로 느끼고 소리를 내어 진동을 느끼게 합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"우\" 소리를 반복합니다.";
                                break;
                            case "ㅠ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅠ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅠ는 입술을 둥글게 하고 발음하는 소리입니다. 발음할 때 입술은 둥글게 유지하며, 혀는 높고 앞쪽에 위치시켜야 합니다. 이때 입술이 둥글게 된 상태에서 소리가 부드럽게 나도록 조절하는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅠ\"를 발음할 때 소리가 부정확하게 나올 수 있습니다. 이럴 경우, \"ㅜ\"를 분명하게 발음한 후, 입술을 둥글게 유지하면서 혀의 앞부분을 높게 올려 \"ㅠ\"를 발음하도록 연습하는 것이 좋습니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n입술의 위치를 느끼며 소리를 내고 손으로 공기의 흐름을 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"유\" 소리를 반복합니다.";
                                break;
                            case "ㅟ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅟ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅟ는 \"ㅜ\"와 \"ㅣ\"의 결합으로 이루어진 소리입니다. 발음할 때, 처음에는 입술을 둥글게 하여 \"ㅜ\"처럼 발음한 후, 입술을 양 옆으로 늘리며 \"ㅣ\"의 느낌을 더합니다. \r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅟ\"를 발음할 때 소리가 흐릿하게 나올 수 있습니다. 이럴 경우, \"ㅜ\"와 \"ㅣ\"를 각각 분명하게 발음하여 두 모음의 차이를 명확히 인식하도록 연습하는 것이 중요합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n소리를 내면서 손으로 입술의 모양을 만져보아 확인하여 발음 시 입술의 위치와 형태를 느끼며 올바른 발음을 연습합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"위\"소리를 반복합니다.";
                                break;
                            case "ㅝ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅝ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅝ는 \"ㅜ\"와 \"ㅓ\"의 결합으로 이루어진 소리입니다. 발음할 때 입술을 둥글게 하여 소리를 내며, 입은 약간 벌리게 됩니다. 혀는 중간 높이에 위치시켜 자연스럽게 발음하는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅝ\"를 발음할 때 소리가 불분명하게 들릴 수 있습니다. 이럴 때는 \"ㅜ\"와 \"ㅓ\"를 각각 명확하게 구분하여 발음하는 연습이 필요합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n소리를 내는 동안 손으로 입술의 모양을 확인하고 진동을 느끼며, 올바른 발음을 위한 입술의 위치를 감지합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"워\" 소리를 반복합니다.";
                                break;
                            case "ㅞ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅞ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅞ는 \"ㅜ\"와 \"ㅔ\"의 결합으로 이루어진 소리입니다. 발음할 때, 처음에는 입술을 둥글게 하여 \"ㅜ\"처럼 소리를 내며, 이 상태에서 입술을 양 옆으로 부드럽게 벌리면서 \"ㅔ\"의 발음으로 전환합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅞ\"를 발음할 때 소리가 흐릿하게 들릴 수 있습니다. 이럴 때는 \"ㅜ\"와 \"ㅔ\"를 분리하여 각각 명확하게 발음하는 연습이 필요합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n입술의 움직임을 손으로 느끼고 소리를 내어 진동을 확인하며, 발음할 때 입술의 정확한 위치와 모양을 인식합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"웨 소리를 반복합니다.";
                                break;
                            case "ㅡ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅡ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅡ는 입을 수평으로 길게 벌리고, 혀는 중간 위치에서 평평하게 유지합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> -  </size></b>\r\n\"ㅡ\"를 발음할 때 소리가 약하게 들릴 수 있습니다. 이럴 경우, 혀의 위치를 낮추고 입술의 긴장을 적절히 조절하여 소리를 더 분명하게 내도록 연습하는 것이 중요합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> - </size></b> \r\n입술과 턱을 손으로 가볍게 만지며 소리를 내고, 발음 시 발생하는 진동을 느끼면서 올바른 입의 위치와 모양을 인식합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"으\" 소리를 반복합니다.";
                                break;
                            case "ㅣ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅣ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅣ는 입을 양 옆으로 벌리고 혀를 높게 위치시켜 발음하는 소리입니다. 이때 소리를 부드럽게 내는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅣ\"를 발음할 때 소리가 부정확하게 나올 수 있습니다. 이럴 경우, 혀의 앞부분을 높여서 입의 안쪽에서 혀끝이 윗니의 뒤쪽 가까이에 위치하도록 조정하여 명확한 소리를 내도록 연습합니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n혀의 위치를 느끼며 소리를 내고, 손으로 목의 진동을 확인합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> - </size></b> \r\n소리 내기 연습: \"이\" 소리를 반복합니다.";
                                break;
                            case "ㅢ":
                                feedback += " \r\n\r\n<b><size=40><모음 ㅢ 소리의 특징과 올바른 발음 방법> - </size></b>\r\nㅢ는 \"ㅡ\"와 \"ㅣ\"의 결합으로 이루어진 소리입니다. 발음할 때 약간의 긴장을 주어 혀를 높게 위치시키고, 입술은 자연스럽게 벌려야 합니다. 소리를 낼 때, \"ㅡ\"를 먼저 발음한 후 \"ㅣ\"로 부드럽게 이어지는 것이 중요합니다.\r\n\r\n<b><size=40><발음할 때 자주 생기는 문제와 교정 전략> - </size></b> \r\n\"ㅢ\"를 발음할 때 소리가 부정확하게 나올 수 있습니다. 이럴 경우, \"ㅡ\"와 \"ㅣ\"를 각각 명확하게 발음하여 두 모음의 차이를 분명히 인식하도록 연습하는 것이 좋습니다.\r\n\r\n<b><size=40><감각을 활용한 발음 연습> -  </size></b>\r\n입술과 혀의 움직임을 손으로 느끼며 소리를 내고, 손으로 목의 진동을 확인하여 올바른 발음을 인식합니다.\r\n\r\n<b><size=40><차근차근 따라 하는 발음 연습> -  </size></b>\r\n소리 내기 연습: \"의\" 소리를 반복합니다.";
                                break;


                        }
                        feedbacks.Add(feedback);
                        Debug.Log($"가장 낮은 점수를 받은 음소: {lowestPhoneme} / 정확도: {lowestPhonemeScore} / 피드백: {feedback}");

                    }
                    else
                    {
                        Debug.LogWarning("음소를 찾을 수 없습니다.");
                    }

                    // WordGameController에서 selectedWords와 videoPaths 가져오기
                    List<string> selectedWords = WordGameController.Instance.GetSelectedWords();
                    //        List<string> videoPaths = WordGameController.Instance.GetVideoPaths();

                    // 선택된 단어와 동영상 경로 및 recognizedTexts를 JSON에 저장
                    WordDataManager.SaveData(selectedWords, recognizedTexts, /*audioClips, videoPaths,*/ accuracyScores, feedbacks);
                    Debug.Log("발음 평가 점수와 함께 데이터 저장 완료.");

                }
            }
            else
            {
                Debug.LogWarning($"발음 평가 실패 이유: {result.Reason}");
            }
        }
    }

    public class KoreanPhonemeSplitter
    {
        private static readonly char[] InitialConsonants =  // 초성 배열
        {
        'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
    };

        private static readonly char[] Vowels =  // 중성 배열
        {
        'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ'
    };

        private static readonly char[] FinalConsonants =  // 종성 배열
        {
        '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
    };

        // 단어를 음소로 분리하는 메서드
        public static List<string> SplitIntoPhonemes(string word)
        {
            List<string> phonemes = new List<string>();

            foreach (char syllable in word)
            {
                if (syllable >= 0xAC00 && syllable <= 0xD7A3)  // 한글 음절 범위 내에 있을 경우
                {
                    int unicodeIndex = syllable - 0xAC00;
                    int initialIndex = unicodeIndex / (21 * 28);  // 초성 인덱스 계산
                    int vowelIndex = (unicodeIndex % (21 * 28)) / 28;  // 중성 인덱스 계산
                    int finalIndex = unicodeIndex % 28;  // 종성 인덱스 계산

                    phonemes.Add(InitialConsonants[initialIndex].ToString());  // 초성 추가
                    phonemes.Add(Vowels[vowelIndex].ToString());  // 중성 추가

                    if (finalIndex > 0)  // 종성이 있을 경우에만 추가
                    {
                        phonemes.Add(FinalConsonants[finalIndex].ToString());
                    }
                }
                else  // 한글 음절이 아닌 경우 그대로 추가
                {
                    phonemes.Add(syllable.ToString());
                    Debug.Log($"한글 음절이 아님: {syllable}");
                }
            }

            return phonemes;
        }


        public static string GetSyllableAtPhonemeIndex(string word, int phonemeIndex)
        {
            List<string> phonemes = SplitIntoPhonemes(word);  // 단어를 음소로 분리
            int currentPhonemeIndex = 0;

            for (int i = 0; i < word.Length; i++)
            {
                char syllable = word[i];
                if (syllable >= 0xAC00 && syllable <= 0xD7A3)  // 한글 음절 범위 내에 있을 경우
                {
                    int unicodeIndex = syllable - 0xAC00;
                    int phonemesInSyllable = (unicodeIndex % 28 > 0) ? 3 : 2;  // 종성이 있으면 3개, 없으면 2개

                    // phonemeIndex가 현재 음절 내에 있는지 확인
                    if (phonemeIndex >= currentPhonemeIndex && phonemeIndex < currentPhonemeIndex + phonemesInSyllable)
                    {
                        return syllable.ToString();  // 해당 음절을 반환
                    }

                    currentPhonemeIndex += phonemesInSyllable;
                }
            }

            return "알 수 없음";
        }

        public static string GetPhonemePosition(string word, int phonemeIndex, out string syllable)
        {
            List<string> phonemes = SplitIntoPhonemes(word); // 단어를 음소로 분리
            int currentPhonemeIndex = 0;

            // 음절을 먼저 나누어 각 음절에서 음소 위치를 추적
            for (int i = 0; i < word.Length; i++)
            {
                char currentSyllable = word[i];
                if (currentSyllable >= 0xAC00 && currentSyllable <= 0xD7A3)  // 한글 음절 범위 내에 있을 경우
                {
                    int unicodeIndex = currentSyllable - 0xAC00;
                    int phonemesInSyllable = (unicodeIndex % 28 > 0) ? 3 : 2;  // 종성이 있으면 3개, 없으면 2개

                    // phonemeIndex가 현재 음절 내에 속하는지 확인
                    if (phonemeIndex >= currentPhonemeIndex && phonemeIndex < currentPhonemeIndex + phonemesInSyllable)
                    {
                        int positionInSyllable = phonemeIndex - currentPhonemeIndex;

                        // 해당 음절을 반환
                        syllable = currentSyllable.ToString();

                        // 음소 위치에 따라 초성, 중성, 종성을 반환
                        if (positionInSyllable == 0)
                            return "초성";
                        else if (positionInSyllable == 1)
                            return "중성";
                        else if (positionInSyllable == 2 && phonemesInSyllable == 3)  // 종성이 있는 경우만 종성 반환
                            return "종성";
                    }

                    currentPhonemeIndex += phonemesInSyllable;  // 음소 인덱스를 이동
                }
            }

            syllable = "알 수 없음";  // 음절을 찾을 수 없을 때
            return "알 수 없음";
        }


    }


    /// 음성인식과 음성녹음 중단 메서드
    async void StopRecognition()
    {
        //음성인식 중단
        if (recognizer != null)
        {
            Debug.Log("음성인식 중단");
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            // 음성 인식 중단 후 StopRecordButton을 자동 클릭 (녹음 중단)
            unityContext.Post(_ =>
            {
                Debug.Log("StopRecordButton 자동 클릭");
                stopRecordButton.onClick.Invoke();  // StopRecordButton 자동 클릭
            }, null);
            recognizer.Recognized -= RecognizedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.Dispose();
            recognizer = null;
        }
        else
        {
            Debug.LogWarning("음성인식기 초기화되지 않음.");
        }

        // 음성 인식 중단 후 결과 텍스트 초기화
        //resultText.text = "";
        // recognizedText = null;
    }

    // 녹음 중지 메서드
    public void StopRecording()
    {


        if (isRecording || Microphone.IsRecording(null))
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("녹음 중단!");

            // 녹음된 오디오 클립의 길이를 출력 (녹음 완료 후)
            if (recordedClip != null)
            {
                // WordGameController에서 현재 단어를 가져옴
                string currentWord = WordGameController.Instance.GetCurrentWord();

                // 올바르게 발음한 경우 녹음 저장
                if (recognizedText.Equals(WordGameController.Instance.GetCurrentWord(), StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("요기!! (정확한 발음)");

                    // 녹음 파일 저장 (currentWord를 넘겨줌)
                    SaveRecording(recordedClip, currentWord);

                    // 녹음된 오디오 클립을 리스트에 추가
                    audioClips.Add(recordedClip);

                    // 해당 recognizedText 저장
                    recognizedTexts.Add(recognizedText);
                    Debug.Log("현재 저장된 recognizedTexts: " + string.Join(", ", recognizedTexts));
                    attemptCount = 0;
                    Debug.Log("attemptCount 초기화됨");
                }
                //3번째 시도인 경우 녹음 저장
                if (attemptCount == 3)
                {
                    Debug.Log("요기!! (3번째 시도 실패~~!)");

                    // 녹음 파일 저장 (currentWord를 넘겨줌)
                    SaveRecording(recordedClip, currentWord);

                    // 녹음된 오디오 클립을 리스트에 추가
                    audioClips.Add(recordedClip);

                    // 해당 recognizedText 저장
                    recognizedTexts.Add(recognizedText);
                    Debug.Log("현재 저장된 recognizedTexts: " + string.Join(", ", recognizedTexts));
                    attemptCount = 0;
                    Debug.Log("attemptCount 초기화됨");

                }



            }
            else
            {
                Debug.LogWarning("녹음된 오디오 클립이 없습니다.");
            }
        }
        else
        {
            // Debug.LogError("녹음이 진행되지 않았습니다. 녹음 종료 실패.");
        }
    }

    public void PlayRecording(Image buttonImage)
    {
        if (recordedClip != null)
        {
            Debug.Log("녹음 파일 재생 중...");
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 오디오 소스 상태 확인
            audioSource.volume = 1.0f;  // 볼륨을 100%로 설정
            audioSource.mute = false;   // 음소거 해제
            audioSource.clip = recordedClip;
            audioSource.Play();

            // 배경음악 정지 처리
            if (bgmController != null)
            {
                bgmController.SetRecordingPlayingStatus(true);  // 녹음이 재생 중임을 BGMController에 알림
            }

            // 오디오 재생 중 이미지 애니메이션 시작
            StartCoroutine(AnimateButtonWhilePlaying(audioSource, buttonImage));

            Debug.Log("녹음 파일 재생 완료");
            if (panelToActivate.activeSelf)
            {
                StartCoroutine(WaitForAudioToEnd(audioSource, panelToActivate, text1, text2));
            }
            else if (finalFailPanel.activeSelf)
            {
                StartCoroutine(WaitForAudioToEnd(audioSource, finalFailPanel, ftext1, ftext2));
            }

            // 녹음 파일이 끝나면 배경음악을 다시 시작하도록 설정
            StartCoroutine(WaitForAudioToEnd(audioSource));
        }
        else
        {
            Debug.LogError("재생할 녹음 파일이 없습니다.");
        }
    }

    private IEnumerator WaitForAudioToEnd(AudioSource audioSource)
    {
        // 녹음이 끝날 때까지 기다린 후, 배경음악을 재개하도록 처리
        yield return new WaitWhile(() => audioSource.isPlaying);

        if (bgmController != null)
        {
            bgmController.SetRecordingPlayingStatus(false);  // 녹음이 끝났으므로 배경음악 재개
        }
    }
    // 이미지 애니메이션 코루틴
    private IEnumerator AnimateButtonWhilePlaying(AudioSource audioSource, Image buttonImage)
    {
        Vector3 originalScale = buttonImage.transform.localScale;
        Vector3 enlargedScale = originalScale * 1.2f;  // 살짝 확대된 크기

        while (audioSource.isPlaying)
        {
            // 0.5초 동안 이미지가 커졌다가 원래 크기로 돌아옴 (확대)
            yield return StartCoroutine(ScaleButton(buttonImage, enlargedScale, 0.5f));
            // 0.5초 동안 이미지가 다시 원래 크기로 돌아옴 (축소)
            yield return StartCoroutine(ScaleButton(buttonImage, originalScale, 0.5f));
        }

        // 오디오 재생이 끝나면 이미지를 원래 크기로 돌림
        buttonImage.transform.localScale = originalScale;
    }
    // 버튼 크기를 부드럽게 변화시키는 코루틴
    private IEnumerator ScaleButton(Image buttonImage, Vector3 targetScale, float duration)
    {
        Vector3 initialScale = buttonImage.transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            buttonImage.transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        buttonImage.transform.localScale = targetScale;
    }
    // 오디오 재생 완료 후 동작을 처리하는 코루틴
    private IEnumerator WaitForAudioToEnd(AudioSource audioSource, GameObject panelToDeactivate, GameObject textToDeactivate, GameObject textToActivate)
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        // 오디오 재생 완료 후 1초 대기
        yield return new WaitForSeconds(0.5f);

        // 오디오 재생 완료 후 text 교체
        textToDeactivate.SetActive(false);
        textToActivate.SetActive(true);
        // 단어가 3번째일 때만 text3 또는 ftext3 활성화
        if (currentWordIndex == 2)
        {
            text3.SetActive(true);   // 3번째 단어일 때 text3 활성화
            text2.SetActive(false);  // text2 비활성화
            ftext3.SetActive(true);  // 3번째 단어일 때 ftext3 활성화
            ftext2.SetActive(false); // ftext2 비활성화
        }
        else
        {
            text3.SetActive(false);  // 3번째 단어가 아니면 text3 비활성화
            text2.SetActive(true);   // text2 활성화
            ftext3.SetActive(false); // 3번째 단어가 아니면 ftext3 비활성화
            ftext2.SetActive(true);  // ftext2 활성화
        }
        // text2 또는 ftext2가 활성화될 때만 파티클 재생
        if (textToActivate == text2 || textToActivate == ftext2)
        {
            confettiParticle.Play();  // 파티클 재생
            Debug.Log("파티클 재생 시작!");
        }
        else
        {
            confettiParticle.Stop();  // 다른 경우 파티클 재생 중지
            Debug.Log("파티클 중지됨.");
        }
        Debug.Log("오디오 재생 완료 후 대기 후 동작 실행");

        // 2초 대기 후 panelToDeactivate 비활성화 및 다음 버튼 클릭
        yield return new WaitForSeconds(4);
        panelToDeactivate.SetActive(false);  // 패널 비활성화
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.Invoke();

        // panelToActivate 초기화 (panelToActivate 비활성화 후 text1, text2 상태를 초기화)
        ResetPanelToActivate();  // text1 활성화, text2 비활성화

        // finalFailPanel 초기화 (finalFailPanel 비활성화 후 ftext1, ftext2 상태를 초기화)
        ResetFinalFailPanel();  // ftext1 활성화, ftext2 비활성화

        attemptCount = 0;
        Debug.Log("attemptCount 초기화됨");
    }

    // panelToActivate를 초기 상태로 복구하는 메서드
    private void ResetPanelToActivate()
    {
        text1.SetActive(true);  // text1 활성화
        text2.SetActive(false); // text2 비활성화
        text3.SetActive(false); // text3 비활성화
        // 버튼 클릭 플래그 리셋
        isPanelToActivateButtonClicked = false;

        Debug.Log("panelToActivate 초기화 완료");
    }

    // finalFailPanel을 초기 상태로 복구하는 메서드
    private void ResetFinalFailPanel()
    {
        ftext1.SetActive(true);  // ftext1 활성화
        ftext2.SetActive(false); // ftext2 비활성화
        ftext3.SetActive(false); // ftext3 비활성화
        // 버튼 클릭 플래그 리셋
        isFinalFailPanelButtonClicked = false;

        Debug.Log("finalFailPanel 초기화 완료");
    }



    private void SaveRecording(AudioClip clip, string word)
    {
        if (clip == null || clip.length == 0)
        {
            Debug.LogWarning("녹음된 파일이 없거나 비어 있습니다.");
            return;
        }

        // 녹음 파일 이름을 고유하게 설정 (recordedAudio_1.wav, recordedAudio_2.wav, ...)
        string filename = $"recordedAudio_{word}_{recordingCount}.wav";
        string filePath = Path.Combine(sessionFolderPath, filename);
        Debug.Log("파일 경로: " + filePath);

        try
        {
            byte[] wavData = WavUtility.FromAudioClip(clip);  // AudioClip을 WAV 데이터로 변환
            File.WriteAllBytes(filePath, wavData);  // 파일 저장

            Debug.Log("녹음 파일 저장 완료: " + filePath);

            recordingCount++;  // 녹음 횟수 증가 (다음 녹음 파일 이름 고유하게 설정)
        }
        catch (System.Exception ex)
        {
            Debug.LogError("녹음 파일 저장 중 오류 발생: " + ex.Message);
        }
        // 녹음된 파일로 발음 평가 진행
        PerformPronunciationAssessment(filePath, word, recognizedText);  // 저장된 파일로 발음 평가
    }



    /// 음성 인식이 취소되거나 실패했을 때 호출되는 이벤트 핸들러
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Debug.LogError($"음성인식 취소&실패 이유: {e.ErrorDetails}");
    }

    void Update()
    {

        // panel2와 panel3이 활성화되었을 때 버튼 활성화
        if (panel2.activeSelf && panel3.activeSelf)
        {

            startButton.gameObject.SetActive(true);
        }
    }




}
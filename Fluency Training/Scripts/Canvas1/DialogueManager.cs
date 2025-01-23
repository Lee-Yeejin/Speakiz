using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class DialogueManager : BaseDialogueManager
{
    public FluencyDataSaver dataSaver; // FluencyDataSaver 참조 추가//////////////////////////
    public GameObject Canvas1;
    public GameObject Canvas2;
    private string recordedFilePath; // 녹음된 파일 경로 저장
    public SpeechRecognitionManager speechRecognitionManager; // SpeechRecognitionManager 참조 추가

    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // 첫 번째 질문 설정 및 저장
        string question1 = "오늘 날씨가 어떤 것 같아?";///////////////////
        dataSaver.SaveQuestion(question1); // 질문 저장//////////////////////

        uiManager.SetupQuestion(
            question1,///////////////////////
            "해가 쨍쨍하고 맑은 것 같아",
            "지금은 맑은데 오후에 비가 온대",
            "오늘 아침에 늦게 일어났어"
        );

        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1;
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "해가 쨍쨍하고 맑은 것 같아")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장////////////////////////
                currentFlow = 1; // 다음 질문 흐름 설정
                return userResponse;
            }
            else if (userResponse == "지금은 맑은데 오후에 비가 온대")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장////////////////////////
                currentFlow = 2; // 다음 질문 흐름 설정
                return userResponse;
            }
            else if (userResponse == "오늘 아침에 늦게 일어났어")
            {
                dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음////////////////////
                return userResponse; // 틀린 답
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "날씨가 좋아서 걸어왔어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "늦어서 차 타고 왔어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "버스랑 지하철 중에 무엇을 탈지 고민이야")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
                    return userResponse; // 틀린 답으로 저장
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "나올 때 우산 챙겼어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "급하게 나오느라 우산을 못 챙겼어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "우리 집엔 우산이 많아")
                {
                    dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음
                    return userResponse; // 틀린 답으로 저장
                }
            }
        }
        return "";
    }

    protected override void ProcessResponse(string userResponse)
    {
        switch (conversationStage)
        {
            case 1:
                HandleFirstStage(userResponse);
                break;
            case 2:
                if (currentFlow == 1)
                    HandleSecondStageForWeatherQuestion(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForUmbrellaQuestion(userResponse);
                break;
        }
        // 대화가 종료되었을 경우 데이터 저장///////////////////////////////////////
        if (conversationStage == 2 && dialogueDataIsComplete())
        {
            dataSaver.SaveToPlayerPrefs(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator WaitAndProcessNextQuestion(float delay, System.Action nextQuestionAction)
    {
        Debug.Log($"대기 중... {delay}초 후 다음 질문으로 이동");
        yield return new WaitForSeconds(delay);
        nextQuestionAction?.Invoke();
        Debug.Log("다음 질문으로 이동 완료");
    }

    private async Task WaitForSpeechSynthesisInitialization()
    {
        while (!speechSynthesisManager.isInitialized)
        {
            Debug.Log("SpeechSynthesisManager 초기화 대기 중...");
            await Task.Delay(100);
        }
    }

    private bool IsAnswerCorrect(string response, string choice)
    {
        return response.Trim().Equals(choice, System.StringComparison.OrdinalIgnoreCase);
    }

    private async void HandleFirstStage(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            currentFlow = 1;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupWalkingToSchoolQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupUmbrellaQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "아침에 늦게 일어나는 날도 있지! 그런데 오늘 날씨에 대해 알고 싶어. 해가 쨍쨍했는지, 비가 올 것 같았는지 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForWeatherQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "날씨가 좋을 땐 걷는 게 최고지!";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "버스와 지하철 각각의 장단점이 있지. 그런데 오늘 학교에 어떻게 왔는지가 궁금해. 걸어서 왔어?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);

            string question3 = "늦었을 땐 차가 최고지!";//////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장//////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForUmbrellaQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // question3 저장////////////////////////////////
            string question3 = "잘했어! 비가 오면 우산이 꼭 필요하지.";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            string question3 = "그럼 이따 비 오면 나랑 우산 같이 쓰고 가자!";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "우산이 많이 있구나! 그런데 너가 우산을 챙겼는지 궁금해.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupWalkingToSchoolQuestion()
    {
        string question2 = "그니까! 날씨 좋다. 오늘 학교에 걸어서 왔어?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장///////////////////

        uiManager.SetupQuestion(
            question2,
            "날씨가 좋아서 걸어왔어",
            "버스랑 지하철 중에 무엇을 탈지 고민이야",
            "늦어서 차 타고 왔어"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private async void SetupUmbrellaQuestion()
    {
        string question2 = "오후에 비 온다고? 너 우산 갖고 왔어?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장///////////////////

        uiManager.SetupQuestion(
            question2,
            "나올 때 우산 챙겼어",
            "급하게 나오느라 우산을 못 챙겼어",
            "우리 집엔 우산이 많아"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private void EndDialogue()
    {
        // 점수 생성 및 저장
        dataSaver.GenerateRandomScores(); // 유창성, 발음, 억양 점수 생성
        dataSaver.SaveScoresToPlayerPrefs(); // 점수 저장

        int canvasIndex = (Canvas1.activeSelf) ? 1 : 2; // 현재 Canvas의 인덱스
        dataSaver.SaveToPlayerPrefs(canvasIndex);
        dataSaver.ClearData(); // 기존 데이터 초기화

        Canvas1.SetActive(false);
        Canvas2.SetActive(true);
    }

    private bool dialogueDataIsComplete()
    {
        return dataSaver.GetDialogueDataCount() >= 5; // 예: 질문 3개와 올바른 대답 2개가 저장되었는지 확인
    }


    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
    }

    public void SaveCurrentDialogueData()
    {
        if (dataSaver != null)
        {
            // 현재 질문 저장 (이미 저장되었는지 확인)
            if (!dataSaver.IsQuestionSaved(uiManager.questionText.text))
            {
                dataSaver.SaveQuestion(uiManager.questionText.text);
            }

            // 현재 사용자의 답변 저장
            string userResponse = speechRecognitionManager?.GetRecognizedText();
            if (!string.IsNullOrEmpty(userResponse))
            {
                bool isCorrect = IsAnswerCorrect(userResponse, uiManager.choice1Text.text) ||
                                 IsAnswerCorrect(userResponse, uiManager.choice2Text.text);
                dataSaver.SaveAnswer(userResponse, isCorrect);
            }

            // PlayerPrefs에 저장
            dataSaver.SaveToPlayerPrefs(SceneManager.GetActiveScene().buildIndex);

            Debug.Log("현재 진행 중인 데이터를 저장했습니다.");
        }
        else
        {
            Debug.LogWarning("DataSaver가 연결되지 않았습니다.");
        }
    }
}
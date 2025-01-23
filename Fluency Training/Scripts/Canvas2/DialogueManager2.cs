using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class DialogueManager2 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager2 speechRecognitionManager; // SpeechRecognitionManager 참조 추가
    public GameObject Canvas2;
    public GameObject Canvas3;
    public GameObject Canvas4;

    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // 첫 번째 질문 설정 및 저장
        string question1 = "안녕! 좋은 아침이야! 혹시 다음 교시 무슨 과목인지 아니?";///////////////////
        dataSaver.SaveQuestion(question1); // 질문 저장//////////////////////

        // 첫 번째 질문과 선택지 설정
        uiManager.SetupQuestion(
            question1,
            "오늘 칠교시야",  // 틀린 선택지
            "체육시간이야",
            "자습시간이야"
        );

        // 첫 번째 질문을 TTS로 출력
        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1; // 첫 번째 대화 단계
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "체육시간이야")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장////////////////////////
                currentFlow = 1;
                return userResponse;
            }
            else if (userResponse == "자습시간이야")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장////////////////////////
                currentFlow = 2;
                return userResponse;
            }
            else if (userResponse == "오늘 칠교시야")
            {
                dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음/////
                return userResponse; // 틀린 답
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "깜빡하고 체육복을 집에 두고 왔어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "체육복 갖고 왔어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "체육시간엔 더워서 반팔 입어야 될 것 같아")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
                    return userResponse; // 틀린 답
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "어제 수학 숙제를 못해서 수학 공부할 거야")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
                else if (userResponse == "나는 책 읽는 것을 좋아해")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
                    return userResponse; // 틀린 답
                }
                else if (userResponse == "내일 영어시험이 있어서 영어 공부할 거야")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return userResponse;
                }
            }
        }
        return ""; // 예상되지 않은 답변
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
                    HandleSecondStageForGymClothesQuestion(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForStudyQuestion(userResponse);
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
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            currentFlow = 1;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupGymClothesQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupStudyQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "오늘 7교시구나. 그런데 나는 다음 교시가 뭔지 궁금해. 다음 교시 무슨 과목이야?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForGymClothesQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "체육복 두 개 갖고 있는데, 내가 체육복 빌려줄게!";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene03_Gym")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "체육복을 갖고 왔구나!";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene03_Gym")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "체육 하다 보면 땀이 나기 마련이지. 근데 나는 네가 체육복을 갖고 왔는지가 더 궁금해. 체육복 갖고 왔어?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForStudyQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // question3 저장////////////////////////////////
            string question3 = "수학 숙제 있었구나. 내가 도와줄게. 같이하자.";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene04_Library")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "독서를 좋아하는구나. 그런데 나는 오늘 자습시간에 네가 어떤 것을 공부할지가 궁금해. 어떤 거 공부할 거야?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            string question3 = "나도 내일 시험 준비로 단어 외워야겠다.";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene04_Library")));
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupGymClothesQuestion()
    {
        string question2 = "체육복 갖고 왔어?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장///////////////////

        uiManager.SetupQuestion(
            question2,
            "깜빡하고 체육복을 집에 두고 왔어",
            "체육복 갖고 왔어",
            "체육시간엔 더워서 반팔 입어야 될 것 같아" // 틀린 선택지
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private async void SetupStudyQuestion()
    {
        string question2 = "오늘 도서관에서 자습한다고 했었지! 어떤 거 공부할거야?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장///////////////////

        uiManager.SetupQuestion(
            question2,
            "어제 수학 숙제를 못해서 수학 공부할 거야",
            "나는 책 읽는 것을 좋아해", // 틀린 선택지
            "내일 영어시험이 있어서 영어 공부할 거야"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private void EndDialogue(string nextScene)
    {
        // 점수 생성 및 저장
        dataSaver.GenerateRandomScores(); // 유창성, 발음, 억양 점수 생성
        dataSaver.SaveScoresToPlayerPrefs(); // 점수 저장

        int canvasIndex = (Canvas2.activeSelf) ? 2 : (nextScene == "Scene03_Gym") ? 3 : 4;
        if (!dialogueDataIsComplete())
        {
            Debug.LogWarning($"Canvas {canvasIndex}: 대화 데이터가 완전하지 않습니다. 전환할 수 없습니다.");
            return;
        }

        dataSaver.SaveToPlayerPrefs(canvasIndex);
        dataSaver.ClearData(); // 기존 데이터 초기화

        Canvas2.SetActive(false);
        if (nextScene == "Scene03_Gym")
        {
            Canvas3.SetActive(true);
        }
        else if (nextScene == "Scene04_Library")
        {
            Canvas4.SetActive(true);
        }
    }

    private bool dialogueDataIsComplete()
    {
        return dataSaver.GetDialogueDataCount() >= 5; // 예: 질문 3개와 올바른 대답 2개가 저장되었는지 확인
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
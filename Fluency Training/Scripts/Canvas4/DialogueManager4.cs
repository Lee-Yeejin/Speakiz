using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DialogueManager4 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager4 speechRecognitionManager; // SpeechRecognitionManager 참조 추가
    public GameObject Canvas4;
    public GameObject Canvas5;
    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // 첫 번째 질문 설정 및 저장
        string question1 = "오늘 도서관에서 어떻게 시간을 보내는 게 좋을까?";///////////////////
        dataSaver.SaveQuestion(question1); // 질문 저장//////////////////////

        // 첫 번째 질문과 선택지 설정
        uiManager.SetupQuestion(
            "오늘 도서관에서 어떻게 시간을 보내는 게 좋을까?",
            "읽고 싶은 책을 골라 보는 것도 좋을 것 같아",
            "도서관은 너무 조용한 것 같아", // 틀린 선택지
            "공부를 하면서 시간을 보내는 것도 좋을 것 같아"
        );

        // TTS로 질문 출력
        await speechSynthesisManager.SpeakText("오늘 도서관에서 어떻게 시간을 보내는 게 좋을까?");
        conversationStage = 1; // 첫 번째 대화 단계
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "읽고 싶은 책을 골라 보는 것도 좋을 것 같아")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/////
                currentFlow = 1;
                return "읽고 싶은 책을 골라 보는 것도 좋을 것 같아";
            }
            else if (userResponse == "공부를 하면서 시간을 보내는 것도 좋을 것 같아")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/////
                currentFlow = 2;
                return "공부를 하면서 시간을 보내는 것도 좋을 것 같아";
            }
            else if (userResponse == "도서관은 너무 조용한 것 같아")
            {
                dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음/////
                return "도서관은 너무 조용한 것 같아"; // 틀린 답
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "나는 줄거리가 있는 소설책을 좋아해")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/
                    return "나는 줄거리가 있는 소설책을 좋아해";
                }
                else if (userResponse == "나는 위인전을 좋아해")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/
                    return "나는 위인전을 좋아해";
                }
                else if (userResponse == "나는 책을 빌리는 것을 좋아해")
                {
                    dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음/////
                    return userResponse; // 틀린 답
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "어제 일찍 잠들어서 못했어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/
                    return "어제 일찍 잠들어서 못했어";
                }
                else if (userResponse == "어제 책을 학교에 두고 와서 못했어")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/
                    return "어제 책을 학교에 두고 와서 못했어";
                }
                else if (userResponse == "공부를 하면 졸린 것 같아")
                {
                    dataSaver.SaveAnswer(userResponse, false); // 틀린 대답 저장하지 않음///
                    return userResponse; // 틀린 답
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
                    HandleSecondStageForBookType(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForStudy(userResponse);
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
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupBookQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupStudyQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "도서관은 조용해서 집중이 잘 되는 것 같아! 그런데 나는 오늘 도서관에서 어떻게 시간을 보낼지 고민이야.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForBookType(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "소설이 집중이 잘 되긴 하지!그럼 오늘은 소설책을 골라봐야겠다!";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = "소설이 집중이 잘 되긴 하지! 그럼 오늘은 소설책을 골라봐야겠다!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "위인전은 역사와 인물을 알아가는 재미지!";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = "위인전은 역사와 인물을 알아가는 재미지!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "책을 빌리는 것을 좋아하다니! 독서를 즐겨하는구나! 그런데 나는 너가 어떤 종류의 책을 좋아하는지 궁금해";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForStudy(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // question3 저장////////////////////////////////
            string question3 = "어제 많이 피곤했구나! 피곤할 땐 일찍 자는게 최고지!";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = "어제 많이 피곤했구나! 피곤할 땐 일찍 자는게 최고지!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            // question3 저장////////////////////////////////
            string question3 = "책을 챙기는 걸 깜빡했구나.";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = "책을 챙기는 걸 깜빡했구나.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "공부할 때 피곤하면 자고 일어나서 하는 건 어때? 그런데 어제 너가 왜 공부를 못했는지 궁금해.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupBookQuestion()
    {
        string question2 = "너는 어떤 종류의 책을 좋아해?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장///

        uiManager.SetupQuestion(
            "너는 어떤 종류의 책을 좋아해?",
            "나는 줄거리가 있는 소설책을 좋아해",
            "나는 위인전을 좋아해",
            "나는 책을 빌리는 것을 좋아해" // 틀린 선택지
        );
        await speechSynthesisManager.SpeakText("너는 어떤 종류의 책을 좋아해?");
        conversationStage = 2;
    }

    private async void SetupStudyQuestion()
    {
        string question2 = "그래야겠다! 넌 어제 시험 공부 했어?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장//

        uiManager.SetupQuestion(
            "그래야겠다! 넌 어제 시험 공부 했어?",
            "공부를 하면 졸린 것 같아", // 틀린 선택지
            "어제 일찍 잠들어서 못했어",
            "어제 책을 학교에 두고 와서 못했어"
        );
        await speechSynthesisManager.SpeakText("그래야겠다! 넌 어제 시험 공부 했어?");
        conversationStage = 2;
    }
    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
    }
    private void EndDialogue()
    {
        // 점수 생성 및 저장
        dataSaver.GenerateRandomScores(); // 유창성, 발음, 억양 점수 생성
        dataSaver.SaveScoresToPlayerPrefs(); // 점수 저장

        Debug.Log("대화가 끝났습니다.");
        if (Canvas4 != null && Canvas5 != null)
        {
            int canvasIndex = (Canvas4.activeSelf) ? 3 : 4; // 현재 Canvas의 인덱스
            dataSaver.SaveToPlayerPrefs(canvasIndex);

            // 현재 Canvas를 비활성화하고, 새로운 Canvas를 활성화
            Canvas4.SetActive(false);  // 기존 Canvas 비활성화
            Canvas5.SetActive(true);  // Scene02_Board에 해당하는 Canvas 활성화
        }
        else
        {
            Debug.LogError("Canvas 오브젝트가 설정되지 않았습니다. 인스펙터에서 mainCanvas 및 boardCanvas를 확인해주세요.");
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
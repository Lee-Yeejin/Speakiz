using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DialogueManager3 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager3 speechRecognitionManager; // SpeechRecognitionManager 참조 추가
    public GameObject Canvas3;
    public GameObject Canvas5;
    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // 첫 번째 질문과 선택지 설정
        string question1 = "오늘 체육 시간에 어떤 운동을 하면 좋을까요?";///////////////////
        dataSaver.SaveQuestion(question1); // 질문 저장//////////////////////

        uiManager.SetupQuestion(
            question1,
            "공을 이용한 운동을 하면 좋을 것 같아요",
            "그림을 그리면 좋을 것 같아요", // 틀린 선택지
            "팀을 나눠서 함께 할 수 있는 운동을 하면 좋을 것 같아요"
        );

        // 첫 번째 질문을 TTS로 출력
        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1; // 첫 번째 대화 단계
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "공을 이용한 운동을 하면 좋을 것 같아요")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/////////////
                currentFlow = 1;
                return "공을 이용한 운동을 하면 좋을 것 같아요";
            }
            else if (userResponse == "팀을 나눠서 함께 할 수 있는 운동을 하면 좋을 것 같아요")
            {
                dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장/////////////
                currentFlow = 2;
                return "팀을 나눠서 함께 할 수 있는 운동을 하면 좋을 것 같아요";
            }
            else if (userResponse == "그림을 그리면 좋을 것 같아요")
            {
                dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
                return "그림을 그리면 좋을 것 같아요"; // 틀린 답
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "저는 배구가 재미있을 것 같아요")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return "저는 배구가 재미있을 것 같아요";
                }
                else if (userResponse == "저는 피구를 하고 싶어요")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return "저는 피구를 하고 싶어요";
                }
                else if (userResponse == "저는 음악을 듣고 싶어요")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
                    return userResponse; // 틀린 답
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "이어달리기를 하면 좋을 것 같아요")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return "이어달리기를 하면 좋을 것 같아요";
                }
                else if (userResponse == "협동심을 기를 수 있는 피구를 하면 좋을 것 같아요")
                {
                    dataSaver.SaveAnswer(userResponse, true); // 올바른 대답 저장
                    return "협동심을 기를 수 있는 피구를 하면 좋을 것 같아요";
                }
                else if (userResponse == "다 같이 요리를 하면 좋을 것 같아요")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  틀린 대답 저장하지 않음
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
                    HandleSecondStageForBallGame(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForTeamwork(userResponse);
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
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupBallQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "그림 그리는 것도 재미있지! 하지만 지금은 체육 시간이야. 어떤 운동을 하면 좋을까?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupTeamworkQuestion));
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForBallGame(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "배구는 정말 재미있지! 오늘 배구 기본기를 연습해보자!";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = "배구는 정말 재미있지! 오늘 배구 기본기를 연습해보자!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "음악을 들으면서 운동을 하는 것도 좋지만, 어떤 운동을 하고 싶은지 말해줘.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS 반응 추가 저장///////////////////////////////
            string question3 = "피구는 정말 신나는 게임이지! 팀을 나눠서 게임을 시작해볼까?";///////////
            dataSaver.SaveQuestion(question3); // TTS 반응 저장///////////////////

            uiManager.questionText.text = "피구는 정말 신나는 게임이지! 팀을 나눠서 게임을 시작해볼까?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForTeamwork(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // question3 저장////////////////////////////////
            string question3 = "이어달리기는 협동심을 기르는 좋은 방법이지! 그럼 팀을 나눠볼까?";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = "이어달리기는 협동심을 기르는 좋은 방법이지! 그럼 팀을 나눠볼까?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            // question3 저장////////////////////////////////
            string question3 = "피구는 협동심을 기르기에 좋은 운동이지! 같이 즐겁게 해보자.";
            dataSaver.SaveQuestion(question3); // TTS 반응 저장

            uiManager.questionText.text = "피구는 협동심을 기르기에 좋은 운동이지! 같이 즐겁게 해보자.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "요리도 협동심을 기르지만, 지금은 체육시간이야. 어떤 운동을 할지 생각해봐.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "잘 못 들었어, 다시 한번 말해줄래?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupBallQuestion()
    {
        string question2 = "좋은 생각이야! 그럼 공을 이용한 운동 중에 어떤 운동을 하고 싶어?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장////////////////

        uiManager.SetupQuestion(
            "좋은 생각이야! 그럼 공을 이용한 운동 중에 어떤 운동을 하고 싶어?",
            "저는 배구가 재미있을 것 같아요",
            "저는 피구를 하고 싶어요",
            "저는 음악을 듣고 싶어요" // 틀린 선택지
        );
        await speechSynthesisManager.SpeakText("좋은 생각이야! 그럼 공을 이용한 운동 중에 어떤 운동을 하고 싶어?");
        conversationStage = 2;
    }

    private async void SetupTeamworkQuestion()
    {
        string question2 = "팀을 나눠서 함께 하는 운동 중에 뭘 하면 좋을까?";////////////////
        dataSaver.SaveQuestion(question2); // 두 번째 질문 저장////////////////

        uiManager.SetupQuestion(
            "팀을 나눠서 함께 하는 운동 중에 뭘 하면 좋을까?",
            "다 같이 요리를 하면 좋을 것 같아요", // 틀린 선택지
            "이어달리기를 하면 좋을 것 같아요",
            "협동심을 기를 수 있는 피구를 하면 좋을 것 같아요"
        );
        await speechSynthesisManager.SpeakText("팀을 나눠서 함께 하는 운동 중에 뭘 하면 좋을까?");
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
        if (Canvas3 != null && Canvas5 != null)
        {
            int canvasIndex = (Canvas3.activeSelf) ? 3 : 4; // 현재 Canvas의 인덱스
            dataSaver.SaveToPlayerPrefs(canvasIndex);
            

            // 현재 Canvas를 비활성화하고, 새로운 Canvas를 활성화
            Canvas3.SetActive(false);  // 기존 Canvas 비활성화
            Canvas5.SetActive(true);  // Scene05에 해당하는 Canvas 활성화
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
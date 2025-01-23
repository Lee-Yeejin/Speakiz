using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;  // 발음 평가 관련 네임스페이스

public class PronunciationEvaluationManager : MonoBehaviour
{
    private string subscriptionKey = "1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y";
    private string serviceRegion = "koreacentral";

    // 발음 평가를 진행하는 메서드
    public async Task EvaluatePronunciation(string audioFilePath, string expectedText)
    {
        if (string.IsNullOrEmpty(audioFilePath) || string.IsNullOrEmpty(expectedText))
        {
            Debug.LogError("오디오 파일 경로 또는 예상 텍스트가 제공되지 않았습니다.");
            return;
        }

        if (!System.IO.File.Exists(audioFilePath))
        {
            Debug.LogError($"지정된 오디오 파일이 존재하지 않습니다: {audioFilePath}");
            return;
        }

        // Azure Speech Config 생성
        var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);

        // 오디오 파일을 사용한 입력 설정
        var audioInput = AudioConfig.FromWavFileInput(audioFilePath);

        // 발음 평가 설정 (평가할 텍스트, 점수 방식, 평가 단위)
        var pronunciationAssessmentConfig = new PronunciationAssessmentConfig(expectedText, GradingSystem.HundredMark, Granularity.Phoneme);

        using (var recognizer = new SpeechRecognizer(config, audioInput))
        {
            // 발음 평가를 수행하도록 설정
            pronunciationAssessmentConfig.ApplyTo(recognizer);

            Debug.Log("발음 평가를 진행 중입니다...");

            // 음성을 인식하고 발음 평가 수행
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                // 발음 평가 결과 추출
                var pronunciationResult = PronunciationAssessmentResult.FromResult(result);

                Debug.Log($"정확도 점수: {pronunciationResult.AccuracyScore} / 100");

                // 추가적으로 점수를 UI에 반영하거나 사용자에게 피드백을 제공할 수 있습니다.
                // 예: uiManager.DisplayPronunciationScore(pronunciationResult.AccuracyScore);
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Debug.LogWarning("발음 평가를 위한 일치하는 음성을 찾지 못했습니다.");
            }
            /*else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechRecognitionCancellationDetails.FromResult(result);
                Debug.LogError($"발음 평가가 취소되었습니다. 이유: {cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Debug.LogError($"에러 코드: {cancellation.ErrorCode}");
                    Debug.LogError($"에러 메시지: {cancellation.ErrorDetails}");
                }
            }*/
        }
    }

    // 사용자 응답 데이터를 파일에 저장하는 메서드
    public void SaveUserResponse(string expectedText, string userResponse)
    {
        // 파일 이름 생성 (고유 ID 기반)
        string fileName = $"{Application.persistentDataPath}/Response_{System.Guid.NewGuid()}.json";

        // 응답 데이터 생성
        var data = new ResponseData
        {
            expectedText = expectedText,
            userResponse = userResponse
        };

        // JSON으로 직렬화 후 파일 저장
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(fileName, json);

        Debug.Log($"사용자 응답 데이터가 저장되었습니다: {fileName}");
    }

    // 사용자 응답 데이터 구조
    [System.Serializable]
    private class ResponseData
    {
        public string expectedText;  // 예상 텍스트
        public string userResponse;  // 사용자 응답 텍스트
    }
}

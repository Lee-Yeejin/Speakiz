using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;

public class SpeechSynthesisManager : MonoBehaviour
{
    private SpeechSynthesizer synthesizer;
    public bool isInitialized = false; // 초기화 여부를 확인하는 플래그
    public bool isSpeaking = false; // TTS 진행 상태 변수

    async void Start()
    {
        // Azure Speech SDK 설정 및 초기화
        await InitializeSpeechSynthesizer();
    }

    // 비동기적으로 SpeechSynthesizer 초기화
    private async Task InitializeSpeechSynthesizer()
    {
        var config = SpeechConfig.FromSubscription("1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y", "koreacentral");
        synthesizer = new SpeechSynthesizer(config);

        if (synthesizer != null)
        {
            Debug.Log("SpeechSynthesizer 초기화 성공");
            isInitialized = true; // 초기화 완료
        }
        else
        {
            Debug.LogError("SpeechSynthesizer 초기화 실패");
        }
    }

    // 텍스트를 음성으로 변환하는 함수
    public async Task SpeakText(string text)
    {
        if (!isInitialized)
        {
            Debug.LogError("SpeechSynthesizer가 아직 초기화되지 않았습니다.");
            return;
        }

        isSpeaking = true;  // TTS 시작 시 상태를 true로 설정
        var result = await synthesizer.SpeakTextAsync(text);
        isSpeaking = false; // TTS 완료 후 상태를 false로 설정

        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            Debug.Log("TTS 완료: " + text);
        }
        else
        {
            Debug.LogError("TTS 실패: " + result.Reason);
        }
    }
}
using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    // 패널 오브젝트 (Inspector에서 panel2, panel3를 할당합니다)
    public GameObject panel2;
    public GameObject panel3;

    // 녹음이 재생 중인지 여부를 체크할 변수
    private bool isRecordingPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();  // 게임 시작 시 배경음악 재생
        }
    }

    void Update()
    {
        // 녹음이 재생 중이거나, panel2 또는 panel3이 활성화되었을 때 배경음악 일시 정지
        if (isRecordingPlaying || (panel2 != null && panel2.activeSelf) || (panel3 != null && panel3.activeSelf))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();  // 배경음악 일시 정지
            }
        }
        else
        {
            // 패널이 비활성화되고 녹음이 끝나면 배경음악 재개
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();  // 배경음악 재개
            }
        }
    }

    // 외부에서 호출할 수 있도록 isRecordingPlaying을 설정하는 메서드
    public void SetRecordingPlayingStatus(bool isPlaying)
    {
        isRecordingPlaying = isPlaying;
    }
}

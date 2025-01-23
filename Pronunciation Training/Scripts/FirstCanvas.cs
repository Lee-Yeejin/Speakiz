using UnityEngine;
using UnityEngine.UI; // 버튼을 사용하기 위해 필요

public class FirstCanvas : MonoBehaviour
{
    public Button startButton; // 버튼을 인스펙터에서 연결
    public PanelController panelController; // PanelController를 인스펙터에서 연결

    void Start()
    {
        // 버튼 클릭 시 PanelController의 OnButtonClick 호출
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }
    }

    void OnStartButtonClick()
    {
        if (panelController != null)
        {
            panelController.OnButtonClick(); // PanelController의 OnButtonClick 메서드 호출
        }

        // FirstCanvas 비활성화
        gameObject.SetActive(false); // 이 스크립트가 붙어 있는 게임 오브젝트를 비활성화
    }
}

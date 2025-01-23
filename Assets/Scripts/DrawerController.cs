using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DrawerController : MonoBehaviour
{
    public GameObject drawerPanel;
    public Button closeAreaButton;
    public CanvasGroup mainPanelCanvasGroup;
    public GameObject targetCanvas; // 전환될 목표 캔버스
    public GameObject currentCanvas; // 현재 활성화된 캔버스
    public Text user_nameText; // 드로어 패널에 표시될 사용자 이름 텍스트
    private bool isDrawerOpen = false;

    // 사용자 이름을 저장하는 변수 (서버에서 가져온 이름)
    private string user_name = "사용자 이름"; // 여기에 실제 사용자 이름을 서버에서 가져와 저장

    void Start()
    {
        // 필수 오브젝트들이 설정되었는지 확인
        if (drawerPanel == null)
        {
            Debug.LogError("DrawerPanel이 설정되지 않았습니다.");
        }

        if (mainPanelCanvasGroup == null)
        {
            Debug.LogError("MainPanel의 CanvasGroup이 설정되지 않았습니다.");
        }

        if (closeAreaButton == null)
        {
            Debug.LogError("CloseAreaButton이 설정되지 않았습니다.");
        }

        if (user_nameText == null)
        {
            Debug.LogError("사용자 이름을 표시할 Text 컴포넌트가 설정되지 않았습니다.");
        }

        // 드로어 외부를 클릭할 수 없도록 시작 시 비활성화
        closeAreaButton.gameObject.SetActive(false);

        // CloseDrawer 메서드를 closeAreaButton에 연결
        closeAreaButton.onClick.AddListener(CloseDrawer);

        // 사용자 이름을 드로어 패널에 표시
        Updateuser_nameUI();
    }

    // 사용자 이름을 드로어 패널에 업데이트하는 메서드
    private void Updateuser_nameUI()
    {
        if (user_nameText != null)
        {
            user_nameText.text = user_name;
        }
    }

    public void ToggleDrawer()
    {
        // 드로어의 열림/닫힘 상태를 토글
        isDrawerOpen = !isDrawerOpen;

        // 드로어 패널과 드로어 외부 버튼의 활성화 상태를 설정
        drawerPanel.SetActive(isDrawerOpen);
        closeAreaButton.gameObject.SetActive(isDrawerOpen);

        // 메인 패널의 상호작용 가능 여부를 설정
        SetMainPanelInteractable(!isDrawerOpen);
    }

    public void CloseDrawer()
    {
        // 드로어를 닫는 메서드
        isDrawerOpen = false;
        drawerPanel.SetActive(false);
        closeAreaButton.gameObject.SetActive(false);
        SetMainPanelInteractable(true);
    }

    private void SetMainPanelInteractable(bool interactable)
    {
        // 메인 패널의 상호작용 가능 여부와 Raycast의 활성화를 설정
        if (mainPanelCanvasGroup != null)
        {
            mainPanelCanvasGroup.interactable = interactable;
            mainPanelCanvasGroup.blocksRaycasts = interactable;
        }
    }

    public void SwitchCanvas()
    {
        // 목표 캔버스가 설정되어 있는지 확인
        if (targetCanvas != null)
        {
            // 현재 캔버스를 비활성화하고 목표 캔버스를 활성화
            if (currentCanvas != null)
            {
                currentCanvas.SetActive(false);
            }

            targetCanvas.SetActive(true);

            // 캔버스 전환 후 드로어를 닫음
            CloseDrawer();

            // 목표 캔버스 내의 첫 번째 InputField에 포커스를 설정
            var inputField = targetCanvas.GetComponentInChildren<InputField>();
            if (inputField != null)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
        else
        {
            Debug.LogError("TargetCanvas가 설정되지 않았습니다.");
        }
    }
}

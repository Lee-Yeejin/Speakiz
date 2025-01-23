using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GlobalDrawer : MonoBehaviour
{
    public static GlobalDrawer instance;

    public GameObject drawerPanel;
    public Button closeAreaButton;
    private GameObject currentCanvas;
    public Text user_nameText; // 사용자 이름을 표시할 Text UI

    private bool isDrawerOpen = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (drawerPanel == null)
        {
            Debug.LogError("DrawerPanel이 설정되지 않았습니다.");
        }

        if (closeAreaButton == null)
        {
            Debug.LogError("CloseAreaButton이 설정되지 않았습니다.");
        }

        if (user_nameText == null)
        {
            Debug.LogError("사용자 이름을 표시할 Text 컴포넌트가 설정되지 않았습니다.");
        }

        closeAreaButton.gameObject.SetActive(false);
        closeAreaButton.onClick.AddListener(CloseDrawer);

        // UserDataFetcher에서 사용자 이름 가져와서 UI 업데이트
        UpdateUserNameUI();
    }

    private void UpdateUserNameUI()
    {
        if (UserDataFetcher.instance != null && user_nameText != null)
        {
            user_nameText.text = UserDataFetcher.instance.GetUserName();
        }
    }

    public void ToggleDrawer(GameObject callingCanvas)
    {
        currentCanvas = callingCanvas;
        isDrawerOpen = !isDrawerOpen;
        drawerPanel.SetActive(isDrawerOpen);
        closeAreaButton.gameObject.SetActive(isDrawerOpen);
    }

    public void CloseDrawer()
    {
        isDrawerOpen = false;
        drawerPanel.SetActive(false);
        closeAreaButton.gameObject.SetActive(false);
    }

    public void SwitchCanvas(GameObject targetCanvas)
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(true);
            if (currentCanvas != null)
            {
                currentCanvas.SetActive(false);
            }
            CloseDrawer();
        }
        else
        {
            Debug.LogError("TargetCanvas가 설정되지 않았습니다.");
        }
    }
}

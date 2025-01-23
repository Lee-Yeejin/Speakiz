using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string gameSceneName = "BreathGame"; // 게임 씬의 이름

    public void SwitchToGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}

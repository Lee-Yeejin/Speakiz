using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameData
{
    public float bt_length;
    public float bt_pitch1;
    public float bt_pitch2;
    public float bt_pitch3;
    public float bt_pitch4;
    public float bt_pitch5;
    public float bt_pitch6;
    public int bt_times;
    public int bt_success;
}

public class Mic : MonoBehaviour
{
    AudioSource aud;

    public GameObject sphereObjectPrefab;
    public Transform[] sphereSpawnPoints;
    public Text timeText;
    public Text statusText;
    public BalloonSliderController balloonSliderController;
    public Text attemptsText;
    public CircleTimer circleTimer;

    public int totalAttempts = 0;
    public int successfulAttempts = 0;

    bool isRecording = false;
    bool isSoundDetected = false;
    float detectionThreshold = 0.01f;
    float soundLostTime = 0f;
    float maxSoundLostDuration = 0.1f;

    float soundDetectedTime = 0f;
    public float maxSize = 1.2f;
    public float maxGrowthTime = 3f;
    public float holdTime = 1f;

    List<GameObject> spheres = new List<GameObject>();
    Vector3 initialPosition = new Vector3(0.028f, 0.751f, -4.328f);

    private float[] pitchLevels = new float[6];

    private GameController gameController;

    private List<float> btLengths = new List<float>();
    private List<float> btPitch1 = new List<float>();
    private List<float> btPitch2 = new List<float>();
    private List<float> btPitch3 = new List<float>();
    private List<float> btPitch4 = new List<float>();
    private List<float> btPitch5 = new List<float>();
    private List<float> btPitch6 = new List<float>();
    private int btSuccessCount = 0;

    private List<Color> balloonColors = new List<Color>
    {
        Color.red, Color.green, Color.blue, Color.yellow, Color.magenta // 풍선 색상 목록
    };

    void Start()
    {
        aud = GetComponent<AudioSource>();
        gameController = FindObjectOfType<GameController>();
    }

    void Update()
    {
        if (isRecording)
        {
            float[] samples = new float[256];
            aud.GetOutputData(samples, 0);
            float level = 0;
            foreach (float sample in samples)
            {
                level += Mathf.Abs(sample);
            }
            level /= samples.Length;

            if (level >= detectionThreshold && level <= 0.1f)
            {
                soundLostTime = 0f;
                if (!isSoundDetected)
                {
                    isSoundDetected = true;
                    circleTimer.SetSoundDetection(true);
                    circleTimer.StartTimer();
                }
                soundDetectedTime += Time.deltaTime;

                // pitchLevels 업데이트
                UpdatePitchLevels(level);
            }
            else
            {
                soundLostTime += Time.deltaTime;

                if (soundLostTime >= maxSoundLostDuration)
                {
                    if (isSoundDetected)
                    {
                        isSoundDetected = false;
                        circleTimer.SetSoundDetection(false);
                        circleTimer.StopTimer();

                        if (soundDetectedTime < 3f)
                        {
                            StopRecording();
                            StartCoroutine(ShrinkObjectOverTime(sphereObjectPrefab, 1f));
                            UpdateStatusText("실패! 다시 시도해보세요");
                            timeText.text = "호흡 지속 시간 : 0.00초";
                            totalAttempts++;
                            SaveAndSendGameData(false);
                            CheckEndCondition();
                            soundDetectedTime = 0f;
                            isRecording = false;
                            UpdateAttemptsText();
                        }
                    }
                }
            }

            if (level >= detectionThreshold && level <= 0.1f)
            {
                isSoundDetected = true;
                UpdateStatusText("호흡 지속 중입니다.");
                soundDetectedTime += Time.deltaTime;

                if (soundDetectedTime < maxGrowthTime)
                {
                    float growthRatio = soundDetectedTime / maxGrowthTime;
                    Vector3 targetScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * maxSize, growthRatio);
                    sphereObjectPrefab.transform.localScale = targetScale;
                    timeText.text = "호흡 지속 시간 : " + soundDetectedTime.ToString("F2") + "초";
                }
                else
                {
                    StopRecording();
                    sphereObjectPrefab.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
                    UpdateStatusText("성공!");
                    timeText.text = "호흡 지속 시간 : 3.00초";
                    successfulAttempts++;
                    totalAttempts++;
                    MoveSphereToSpawnPoint();
                    CreateNewSphere();
                    SaveAndSendGameData(true);
                    CheckEndCondition();
                    isRecording = false;
                    isSoundDetected = false;
                    circleTimer.SetSoundDetection(false);
                    circleTimer.StopTimer();
                    UpdateAttemptsText();
                }
            }
        }
    }

    void UpdatePitchLevels(float level)
    {
        if (soundDetectedTime >= 0.5f && pitchLevels[0] == 0)
        {
            pitchLevels[0] = level;
            Debug.Log("Pitch level 1 set to: " + pitchLevels[0]);
        }
        if (soundDetectedTime >= 1.0f && pitchLevels[1] == 0)
        {
            pitchLevels[1] = level;
            Debug.Log("Pitch level 2 set to: " + pitchLevels[1]);
        }
        if (soundDetectedTime >= 1.5f && pitchLevels[2] == 0)
        {
            pitchLevels[2] = level;
            Debug.Log("Pitch level 3 set to: " + pitchLevels[2]);
        }
        if (soundDetectedTime >= 2.0f && pitchLevels[3] == 0)
        {
            pitchLevels[3] = level;
            Debug.Log("Pitch level 4 set to: " + pitchLevels[3]);
        }
        if (soundDetectedTime >= 2.5f && pitchLevels[4] == 0)
        {
            pitchLevels[4] = level;
            Debug.Log("Pitch level 5 set to: " + pitchLevels[4]);
        }
        if (soundDetectedTime >= 2.99f && pitchLevels[5] == 0)
        {
            pitchLevels[5] = level;
            Debug.Log("Pitch level 6 set to: " + pitchLevels[5]);
        }
    }

    void SaveAndSendGameData(bool success)
    {
        btLengths.Add(soundDetectedTime);
        btPitch1.Add(pitchLevels[0]);
        btPitch2.Add(pitchLevels[1]);
        btPitch3.Add(pitchLevels[2]);
        btPitch4.Add(pitchLevels[3]);
        btPitch5.Add(pitchLevels[4]);
        btPitch6.Add(pitchLevels[5]);

        if (success) btSuccessCount++;
        pitchLevels = new float[6]; // 초기화
        UpdateAttemptsText();
    }

    public void SaveFinalDataToPlayerPrefs()
    {
        float lengthAverage = GetAverage(btLengths);
        float level1Average = GetAverage(btPitch1);
        float level2Average = GetAverage(btPitch2);
        float level3Average = GetAverage(btPitch3);
        float level4Average = GetAverage(btPitch4);
        float level5Average = GetAverage(btPitch5);
        float level6Average = GetAverage(btPitch6);

        PlayerPrefs.SetFloat("length", lengthAverage);
        PlayerPrefs.SetFloat("bt_level1", level1Average);
        PlayerPrefs.SetFloat("bt_level2", level2Average);
        PlayerPrefs.SetFloat("bt_level3", level3Average);
        PlayerPrefs.SetFloat("bt_level4", level4Average);
        PlayerPrefs.SetFloat("bt_level5", level5Average);
        PlayerPrefs.SetFloat("bt_level6", level6Average);
        PlayerPrefs.SetInt("bt_success_cnt", btSuccessCount);

        PlayerPrefs.Save();
    }

    float GetAverage(List<float> values)
    {
        float sum = 0f;
        foreach (float value in values) sum += value;
        return values.Count > 0 ? sum / values.Count : 0f;
    }

    void StopRecording()
    {
        isRecording = false;
        aud.Stop();
        Microphone.End(Microphone.devices[0]);
        
        CheckEndCondition();
    }

    IEnumerator ShrinkObjectOverTime(GameObject obj, float duration)
    {
        Vector3 initialScale = obj.transform.localScale;
        Vector3 targetScale = Vector3.one * 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = targetScale;
    }

    void UpdateStatusText(string message) => statusText.text = message;

    void MoveSphereToSpawnPoint()
    {
        if (spheres.Count < sphereSpawnPoints.Length)
        {
            int index = spheres.Count;
            Vector3 spawnPosition = sphereSpawnPoints[index].position;
            sphereObjectPrefab.transform.position = spawnPosition;
        }
    }

    void CreateNewSphere()
    {
        if (spheres.Count < 5)
        {
            Vector3 originalPosition = initialPosition;
            GameObject newSphere = Instantiate(sphereObjectPrefab, originalPosition, Quaternion.identity);
            newSphere.transform.localScale = Vector3.one * 0.5f;

            // 풍선을 BT_Canvas의 자식으로 설정
            newSphere.transform.SetParent(gameController.currentCanvas.transform, true);

            int colorIndex = successfulAttempts % balloonColors.Count;
            newSphere.GetComponent<Renderer>().material.color = balloonColors[colorIndex];

            //AudioSource newSphereAudio = newSphere.AddComponent<AudioSource>();
            //newSphereAudio.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
            //newSphereAudio.loop = false;
            //newSphereAudio.Play();

            spheres.Add(newSphere);
           // StartRecordingForSphere(newSphereAudio);
            sphereObjectPrefab = spheres[spheres.Count - 1];
            balloonSliderController?.AddBalloon();
        }
    }

    /*void StartRecordingForSphere(AudioSource audioSource)
    {
        aud = audioSource;
        isRecording = true;
    }*/

    public int GetSphereCount() => spheres.Count;

    void CheckEndCondition()
    {
        if (totalAttempts >= 10 || successfulAttempts >= 5)
        {
            gameController.CheckEndGameCondition();
            SaveFinalDataToPlayerPrefs();
            string endMessage = successfulAttempts >= 5 ? "호흡 훈련 종료. 아주 잘했어!" : "호흡 훈련 종료. 더 힘내보자!";
            UpdateStatusText(endMessage);
            isRecording = false;
            this.enabled = false;
        }
    }

    public void UpdateAttemptsText() => attemptsText.text = totalAttempts + "/10";

    public void Recsnd()
    {
        if (Microphone.devices.Length > 0)
        {
            aud.clip = Microphone.Start(Microphone.devices[0], false, 10, 16000);
            while (!(Microphone.GetPosition(null) > 0)) { }
            aud.Play();
            isRecording = true;
            soundDetectedTime = 0f;
            isSoundDetected = false;
        }
    }

   
}
/*using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DateDropdown : MonoBehaviour
{
    public Dropdown dateDropdown;  // 드롭다운 UI
    public Text selectedDateText;  // 선택된 날짜를 표시할 텍스트
    public string trainingType;    // 훈련 종류 (호흡, 조음, 유창성)

    void Start()
    {
        // 드롭다운 초기화
        dateDropdown.ClearOptions();

        // 최근 일주일치 날짜를 리스트로 저장
        List<string> dates = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            DateTime day = DateTime.Now.AddDays(-i);
            dates.Add(day.ToString("M월 dd일"));
        }

        // 드롭다운에 날짜 추가
        dateDropdown.AddOptions(dates);

        // 드롭다운의 값이 변경되었을 때 호출되는 리스너 추가
        dateDropdown.onValueChanged.AddListener(delegate { OnDateSelected(); });

        // 기본 선택된 날짜를 텍스트에 설정
        OnDateSelected();
    }

    // 드롭다운에서 날짜를 선택하면 호출되는 함수
    public void OnDateSelected()
    {
        string selectedDate = dateDropdown.options[dateDropdown.value].text; // 선택한 날짜 가져오기
        selectedDateText.text = selectedDate; // 선택한 날짜를 텍스트에 표시

        // 선택된 날짜에 맞는 데이터를 가져옴 (훈련 종류에 따라)
        FetchTrainingDataForSelectedDate();
    }

    // 선택한 날짜에 맞는 훈련 데이터를 가져오는 함수
    private void FetchTrainingDataForSelectedDate()
    {
        // TrainingDataFetcher를 이용하여 훈련 종류와 날짜에 맞는 데이터를 가져옴
        TrainingDataFetcher.instance.FetchTrainingData(trainingType, dateDropdown.options[dateDropdown.value].text);
    }
}
*/
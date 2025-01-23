using UnityEngine;
using UnityEngine.UI;
using System;

public class DateDisplay : MonoBehaviour
{
    public Text dateText;  

    void Start()
    {
       
        dateText.text = DateTime.Now.ToString("M¿ù ddÀÏ");
    }
}

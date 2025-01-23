using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class login : MonoBehaviour
{
    public GameObject loginCanvas;
    public GameObject trainingCanvas;
    public GameObject signUpCanvas;

    public void GoTotraining()
    {
        loginCanvas.SetActive(false);
        trainingCanvas.SetActive(true);
    }

    public void GoToSignUp()
    {
        loginCanvas.SetActive(false);
        signUpCanvas.SetActive(true);
    }

  
}


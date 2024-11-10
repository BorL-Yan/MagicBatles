using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealtAmplitude : MonoBehaviour
{
    public Image image;
    
    public void UpdateHealth(int values)
    {
        try
        {
            image.fillAmount = values/100f;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        //Debug.Log(values);
    }
}

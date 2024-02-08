using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionOrder : MonoBehaviour
{
    public ColorSchemeManager colorManager;
    public CameraPosition cameraManager;
    public Image buttonOne;
    public Image buttonTwo;
    public Image buttonThree;
    public Image buttonFour;
    

    public void FirstOrder()
    {
        colorManager.skyOrder = new int[] {1,2,4,3};
        cameraManager.cameraOrder = new int[] {1,2,4,3};
        buttonOne.color = Color.green;
    }

    public void SecondOrder()
    {
        colorManager.skyOrder = new int[] {2,3,1,4};
        cameraManager.cameraOrder = new int[] {2,3,1,4};
        buttonTwo.color = Color.green;
    }
    
    public void ThirdOrder()
    {
        colorManager.skyOrder = new int[] {3,4,2,1};
        cameraManager.cameraOrder = new int[] {3,4,2,1};
        buttonThree.color = Color.green;
    }
    
    public void FourthOrder()
    {
        colorManager.skyOrder = new int[] {4,1,3,2};
        cameraManager.cameraOrder = new int[] {4,1,3,2};
        buttonFour.color = Color.green;
    }
}

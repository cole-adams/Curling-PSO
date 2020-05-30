using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedDropdown : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1;
    }

    public void UpdateTime(int index)
    {
        if (index == 1)
        {
            Time.timeScale = 1;
        } else if (index == 2)
        {
            Time.timeScale = 2;
        } else if (index == 3)
        {
            Time.timeScale = 4;
        } else if (index == 4)
        {
            Time.timeScale = 10;
        }
    }
}

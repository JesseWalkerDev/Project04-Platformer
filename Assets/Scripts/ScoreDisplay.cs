using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    private TMP_Text display;
    // Start is called before the first frame update
    void Start()
    {
        display = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        display.text = "Check Point: " + CameraFollow.checkPointCount + "/" + CameraFocusZone.allZones.Count;
    }
}

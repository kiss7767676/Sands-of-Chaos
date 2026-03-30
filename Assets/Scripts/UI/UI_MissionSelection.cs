using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_MissionSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionDesciprtion;

    public void Awake()
    {
        Cursor.visible = true;
    }   

    public void UpdateMissionDesicription(string text)
    {
        missionDesciprtion.text = text;
    }
}

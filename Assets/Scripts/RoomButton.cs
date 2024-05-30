using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text Buttontext;

    private RoomInfo Info;


    public void SetButtonDetails(RoomInfo inputinfo)
    {
        Info = inputinfo;
        Buttontext.text = Info.Name;

    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(Info);
    }
}
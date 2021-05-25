using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startmenu;
    public TMP_InputField username;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance!=this)
        {
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        startmenu.SetActive(false);
        Client.instance.connecttoServer();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject playerprefab;
    private string ipaddress;
    public TMP_Text IPaddress;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        ipaddress = GetLocalIPAddress();
        Debug.Log("Server IPaddress : " + ipaddress);
        IPaddress.text = ipaddress;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        Server.Start(20, 26950);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerprefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject playerprefab;

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
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
#if UNITY_EDITOR
       // Debug.Log("Build the project and start the server");
#else
        Server.Start(20, 26950);
#endif
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerprefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}

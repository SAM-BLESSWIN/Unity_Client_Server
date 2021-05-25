using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localplayerprefab;
    public GameObject playerprefab;

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else if(Instance!=this)
        {
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id,string _playername,Vector3 _postion,Quaternion _rotation)
    {
        GameObject _player;
        if(Client.instance.myid==_id)
        {
            _player = Instantiate(localplayerprefab, _postion, _rotation);
        }
        else
        {
            _player = Instantiate(playerprefab, _postion, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _playername;

        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}

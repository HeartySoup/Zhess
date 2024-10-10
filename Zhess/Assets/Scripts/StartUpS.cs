using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpS : MonoBehaviour
{
    GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Play");
        Player.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

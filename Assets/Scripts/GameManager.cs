using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject spawnpoint;

    public float timeLeft = 180; // in seconds

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        // if (!isServer)
        //     return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            // restart match
        }
    }
}

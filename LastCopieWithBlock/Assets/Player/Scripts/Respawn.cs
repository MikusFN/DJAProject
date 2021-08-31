using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour {
    private Vector3 location;
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Alpha5))
        {
            SetRespawnPoint(transform.position);
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            GoRespawnPoint();
        }
    }
    private void SetRespawnPoint(Vector3 position)
    {
        location = position;
    }
    private void GoRespawnPoint()
    {
        transform.position = location;
    }
}

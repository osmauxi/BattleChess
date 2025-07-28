using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting : MonoBehaviour
{
    [SerializeField] private float Vspeed = 0.2f;
    [SerializeField] private float Hspeed = 0.2f;
    private Vector3 currentPos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
        if (Input.GetKey(KeyCode.W)) 
        {
            currentPos.z += Vspeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentPos.z -= Vspeed;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            currentPos.x -= Hspeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            currentPos.x += Hspeed;
        }
        transform.position = currentPos;
    }
}

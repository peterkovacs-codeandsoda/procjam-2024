using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    
    [SerializeField]
    float speed;
    
    [SerializeField]
    float turnSpeed;

    float waitingTime;

    void Update()
    {
        waitingTime += Time.deltaTime;
        if (waitingTime > 2.0f)
        {
            float rotation = Input.GetAxis("Horizontal") * -turnSpeed * Time.deltaTime;
            transform.Translate(0, speed * Time.deltaTime, 0);
            transform.Rotate(0, 0, rotation);
        }
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }
}

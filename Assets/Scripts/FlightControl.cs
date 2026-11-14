using UnityEngine;

public class FlightControl : MonoBehaviour
{
    float Yaw;
    float Pitch;
    float Roll;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float TargetYaw = 0;
        float TargetPitch = 0;
        float TargetRoll = 0;

        transform.position += transform.forward * Time.deltaTime * 10f; // Move forward at a speed of 10 units per second
        if (Input.GetKey(KeyCode.D))
        {
            TargetRoll -= 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            TargetRoll += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            TargetPitch -= 1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            TargetPitch += 1f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            TargetYaw -= 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            TargetYaw += 1f;
        }

        Roll = Mathf.Lerp(Roll, TargetRoll, Time.deltaTime * 5f);
        Pitch = Mathf.Lerp(Pitch, TargetPitch, Time.deltaTime * 5f);
        Yaw = Mathf.Lerp(Yaw, TargetYaw, Time.deltaTime * 5f);
        /*
        if (Input.GetKey(KeyCode.D)) 
        {
            Roll -= 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Roll += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Pitch -= 1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            Pitch += 1f;
        }
        */
        Vector3 rotatespd = new Vector3(Pitch, Yaw, Roll) * Time.deltaTime * 90f;
        transform.Rotate(rotatespd);
    }
}

using UnityEngine;

public class FlightControl : MonoBehaviour
{
    float Yaw;
    float Pitch;
    float Roll;
    float Speed;

    [SerializeField]
    GameObject CamNode;
    [SerializeField]
    Camera camera;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FlightInputUpdate();
        CamUpdate();

    }

    private void FlightInputUpdate() //Sets a target rotation based on input and lerps to it
    {
        float TargetYaw = 0;
        float TargetPitch = 0;
        float TargetRoll = 0;

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

        Roll = Mathf.Lerp(Roll, TargetRoll, Time.deltaTime * 3f);
        Pitch = Mathf.Lerp(Pitch, TargetPitch, Time.deltaTime * 3f);
        Yaw = Mathf.Lerp(Yaw, TargetYaw, Time.deltaTime * 3f);

        Vector3 rotatespd = new Vector3(Pitch, Yaw, Roll) * Time.deltaTime * 120f;
        transform.Rotate(rotatespd);

        float TargetSpeed = 10;
        if (Input.GetKey(KeyCode.Space))
        {
            TargetSpeed += 10f;
        }
        Speed = Mathf.Lerp(Speed, TargetSpeed, Time.deltaTime * 4f);
        transform.position += transform.forward * Time.deltaTime * Speed;
    }

    void CamUpdate()  //Bend camera the other way when plane rotates.
    {
        CamNode.transform.localEulerAngles = -10 * new Vector3(Pitch, Yaw, Roll);
        camera.fieldOfView = 40 + Speed * 2;
    }

}

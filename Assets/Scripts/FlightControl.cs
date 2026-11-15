using UnityEngine;

//This script is attached to the player prefab and controls how it files camera movement accordingly.
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

        //Inputs, determine where the plane should rotate to.
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

        //Slowly lerp rotation speed to target rotation speed to smooth it out.
        Roll = Mathf.Lerp(Roll, TargetRoll, Time.deltaTime * 3f);
        Pitch = Mathf.Lerp(Pitch, TargetPitch, Time.deltaTime * 3f);
        Yaw = Mathf.Lerp(Yaw, TargetYaw, Time.deltaTime * 3f);

        //Apply current rotation speed
        Vector3 rotatespd = new Vector3(Pitch, Yaw, Roll) * Time.deltaTime * 120f;
        transform.Rotate(rotatespd);

        //Similar idea for speed forward.
        float TargetSpeed = 10;
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift))
        {
            TargetSpeed += 10f;
        }
        Speed = Mathf.Lerp(Speed, TargetSpeed, Time.deltaTime * 4f);
        transform.position += transform.forward * Time.deltaTime * Speed;
    }

    void CamUpdate()
    {
        CamNode.transform.localEulerAngles = -10 * new Vector3(Pitch, Yaw, Roll); //Bend camera the other way when plane rotates.
        camera.fieldOfView = 40 + Speed * 2; //Zoom out when plane goes fast.
    }

}

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class ros_subs_v1 : MonoBehaviour
{
    public string topic = "bluerov1/cmd_vel";
    public veolicty_control_v1 veloicty_control_v1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.veloicty_control_v1 = GetComponent<veolicty_control_v1>();
        ROSConnection.GetOrCreateInstance().Subscribe<RosMessageTypes.Geometry.TwistMsg>(topic,VelocityChange);
    }

    // Update is called once per frame
    void VelocityChange(RosMessageTypes.Geometry.TwistMsg velocityMessage)
    {
        Debug.Log(""+ velocityMessage);
        this.veloicty_control_v1.moveVelocity(velocityMessage);
    }
}

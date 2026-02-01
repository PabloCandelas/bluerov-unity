using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;

public class CameraImagePublisher : MonoBehaviour
{
    public camera_publisher cam;
    ROSConnection ros;
    public string topicName = "/unity_camera/image_r2";

    [Header("Publish Settings")]
    public float publishFPS = 15f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        if (cam == null)
        {
            Debug.LogError("Camera reference not assigned!");
            enabled = false;
            return;
        }

        StartCoroutine(PublishLoop());
    }

    private System.Collections.IEnumerator PublishLoop()
    {
        float interval = 1f / publishFPS;
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (true)
        {
            PublishFrame();
            yield return wait;
        }
    }

    private void PublishFrame()
    {
        if (cam.jpg == null || cam.jpg.Length == 0)
            return;

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(cam.jpg);
        byte[] rgbBytes = tex.GetRawTextureData();

        int w = tex.width;   // <-- NEW patch: use actual cropped width
        int h = tex.height;  // <-- NEW patch: use actual cropped height

        Destroy(tex);

        float t = Time.realtimeSinceStartup;
        int sec = (int)Mathf.Floor(t);
        uint nsec = (uint)((t - sec) * 1e9f);

        ImageMsg msg = new ImageMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                stamp = new TimeMsg(sec, nsec),
                frame_id = "unity_camera"
            },
            height = (uint)h,
            width = (uint)w,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(w * 3),
            data = rgbBytes
        };

        ros.Publish(topicName, msg);
    }
}

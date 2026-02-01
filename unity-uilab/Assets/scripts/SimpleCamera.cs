using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using TMPro; // Needed for Text
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.ROSGeometry; // Sometimes needed
using Unity.Robotics.ROSTCPConnector.MessageGeneration; // Usually where proper types live

public class ROVDashboard : MonoBehaviour
{
    [Header("Video Settings")]
    public RawImage displayImage;
    public string imageTopic = "/bluerov2/camera/image";

    [Header("Battery Settings")]
    public TextMeshProUGUI batteryText;
    public string batteryTopic = "/bluerov2/battery";

    private ROSConnection ros;
    private Texture2D texture2D;
    private byte[] rawData;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        // 1. Subscribe to Video
        ros.Subscribe<ImageMsg>(imageTopic, ReceiveImage);

        // 2. Subscribe to Battery 
        // We removed the invalid parameter. 
        // This makes the code valid, but Unity might still ignore the "Best Effort" data.
        ros.Subscribe<BatteryStateMsg>(batteryTopic, ReceiveBattery);
    }

    void ReceiveBattery(BatteryStateMsg msg)
    {
        // msg.voltage is usually the total voltage.
        // msg.percentage might be -1 or 0 depending on the driver, so we use voltage.
        
        // Update the text on the UI
        batteryText.text = $"BATTERY: {msg.voltage:F1} V";

        // Optional: Change color if low battery (e.g., below 14V)
        if (msg.voltage < 14.0f)
            batteryText.color = Color.red;
        else
            batteryText.color = Color.green;
    }

    void ReceiveImage(ImageMsg msg)
    {
        // ... (Existing Video Logic) ...
        int width = (int)msg.width;
        int height = (int)msg.height;

        if (texture2D == null || texture2D.width != width || texture2D.height != height)
        {
            texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
            displayImage.texture = texture2D;
            rawData = new byte[width * height * 3];
        }

        if (msg.encoding.Contains("bgr8"))
        {
            byte[] rosData = msg.data;
            for (int i = 0; i < rosData.Length; i += 3)
            {
                rawData[i] = rosData[i + 2];
                rawData[i + 1] = rosData[i + 1];
                rawData[i + 2] = rosData[i];
            }
            texture2D.LoadRawTextureData(rawData);
        }
        else
        {
            texture2D.LoadRawTextureData(msg.data);
        }
        texture2D.Apply();
    }
}
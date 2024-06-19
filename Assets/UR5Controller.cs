using UnityEngine;

public class UR5Controller : MonoBehaviour
{
    public GameObject RobotBase;
    public float[] jointValues = new float[6];
    private GameObject[] jointList = new GameObject[6];
    private float[] upperLimit = { 180f, 180f, 180f, 180f, 180f, 180f };
    private float[] lowerLimit = { -180f, -180f, -180f, -180f, -180f, -180f };

    void Start()
    {
        initializeJoints();
    }

    void LateUpdate()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 currentRotation = jointList[i].transform.localEulerAngles;
            currentRotation.z = jointValues[i];
            jointList[i].transform.localEulerAngles = currentRotation;
        }
    }

    void OnGUI()
    {
        int boundary = 20;
        int labelHeight = Application.isEditor ? 20 : 40;
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = labelHeight;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        for (int i = 0; i < 6; i++)
        {
            GUI.Label(new Rect(boundary, boundary + (i * 2 + 1) * labelHeight, labelHeight * 4, labelHeight), "Joint " + i + ": ");
            jointValues[i] = GUI.HorizontalSlider(new Rect(boundary + labelHeight * 4, boundary + (i * 2 + 1) * labelHeight + labelHeight / 4, labelHeight * 5, labelHeight), jointValues[i], lowerLimit[i], upperLimit[i]);
        }
    }

    void initializeJoints()
    {
        var RobotChildren = RobotBase.GetComponentsInChildren<Transform>();
        for (int i = 0; i < RobotChildren.Length; i++)
        {
            switch (RobotChildren[i].name)
            {
                case "control0":
                    jointList[0] = RobotChildren[i].gameObject;
                    break;
                case "control1":
                    jointList[1] = RobotChildren[i].gameObject;
                    break;
                case "control2":
                    jointList[2] = RobotChildren[i].gameObject;
                    break;
                case "control3":
                    jointList[3] = RobotChildren[i].gameObject;
                    break;
                case "control4":
                    jointList[4] = RobotChildren[i].gameObject;
                    break;
                case "control5":
                    jointList[5] = RobotChildren[i].gameObject;
                    break;
            }
        }
    }

    public void UpdateJointValues(float[] newJointValues)
    {
        for (int i = 0; i < 6; i++)
        {
            jointValues[i] = Mathf.Clamp(newJointValues[i], lowerLimit[i], upperLimit[i]);
        }
    }
}

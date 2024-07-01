using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UR5HistoricalDataRequester : MonoBehaviour
{
    public UR5Controller ur5Controller;
    private string crateDbUrl = "http://fiware.macwin.pt:4200/_sql";

    public void RequestHistoricalData()
    {
        StartCoroutine(GetHistoricalData());
    }

    IEnumerator GetHistoricalData()
    {
        string query = @"
            {
                ""stmt"": ""SELECT actual_q FROM mtopeniot.etdevice WHERE time_index >= '2024-07-01T14:55:00.000Z' AND time_index <= '2024-07-01T15:05:00.000Z' ORDER BY time_index ASC""
            }";

        UnityWebRequest www = UnityWebRequest.Put(crateDbUrl, query);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            UnityEngine.Debug.Log("Received JSON: " + jsonResponse);

            // Parse JSON response
            CrateDbResponse response = JsonConvert.DeserializeObject<CrateDbResponse>(jsonResponse);
            List<float[]> jointValuesList = new List<float[]>();

            foreach (var row in response.rows)
            {
                if (row.Length > 0)
                {
                    // Extract the JSON string from the row and parse it
                    string jointDataJson = row[0];
                    JointData jointData = JsonConvert.DeserializeObject<JointData>(jointDataJson);
                    if (jointData != null && jointData.value != null)
                    {
                        jointValuesList.Add(jointData.value);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Invalid joint data or missing values.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Row is empty.");
                }
            }

            if (jointValuesList.Count > 0)
            {
                StartCoroutine(ReplayJointValues(jointValuesList));
            }
            else
            {
                UnityEngine.Debug.LogError("No valid joint values found.");
            }
        }
    }

    IEnumerator ReplayJointValues(List<float[]> jointValuesList)
    {
        foreach (var jointValues in jointValuesList)
        {
            // Convert radians to degrees
            float[] jointValuesDegrees = new float[jointValues.Length];
            for (int i = 0; i < jointValues.Length; i++)
            {
                jointValuesDegrees[i] = jointValues[i] * Mathf.Rad2Deg;
            }

            ur5Controller.UpdateJointValues(jointValuesDegrees);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private class CrateDbResponse
    {
        public List<string[]> rows { get; set; }
    }

    private class JointData
    {
        public string type { get; set; }
        public float[] value { get; set; }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UR5DataReceiver : MonoBehaviour
{
    public UR5Controller ur5Controller;
    private string url = "http://fiware.macwin.pt:1026/v2/entities/Device:braco001/attrs/actual_q"; // URL do Orion Context Broker

    private bool fetchingData = false;
    private Coroutine fetchCoroutine;

    void Start()
    {
        // Inicialmente não começamos a buscar dados.
    }

    void Update()
    {
        // O Update não é necessário aqui.
    }

    public void StartFetchingData()
    {
        if (!fetchingData)
        {
            fetchingData = true;
            fetchCoroutine = StartCoroutine(FetchDataEverySecond());
        }
    }

    public void StopFetchingData()
    {
        if (fetchingData)
        {
            fetchingData = false;
            if (fetchCoroutine != null)
            {
                StopCoroutine(fetchCoroutine);
            }
        }
    }

    private IEnumerator FetchDataEverySecond()
    {
        while (fetchingData)
        {
            yield return GetJointValues();
            yield return new WaitForSeconds(1f); // Aguarda 1 segundo antes de fazer a próxima solicitação
        }
    }

    private IEnumerator GetJointValues()
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Fiware-Service", "openiot");
        www.SetRequestHeader("Fiware-ServicePath", "/");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError(www.error);
        }
        else
        {
            // Parse JSON response
            string jsonResponse = www.downloadHandler.text;
            UnityEngine.Debug.Log("Received JSON: " + jsonResponse);

            // Parse JSON response to RootObject
            RootObject rootObject = JsonUtility.FromJson<RootObject>(jsonResponse);

            if (rootObject != null && rootObject.value != null && rootObject.value.value != null)
            {
                // Extract joint values in radians
                float[] jointValuesRadians = rootObject.value.value;

                // Convert radians to degrees
                float[] jointValuesDegrees = new float[jointValuesRadians.Length];
                for (int i = 0; i < jointValuesRadians.Length; i++)
                {
                    jointValuesDegrees[i] = jointValuesRadians[i] * Mathf.Rad2Deg;
                }

                // Update joint values in UR5Controller
                ur5Controller.UpdateJointValues(jointValuesDegrees);
            }
            else
            {
                UnityEngine.Debug.LogError("Received JSON does not contain valid joint values.");
            }
        }
    }

    [System.Serializable]
    private class RootObject
    {
        public string type;
        public ValueObject value;
        public Metadata metadata;
    }

    [System.Serializable]
    private class ValueObject
    {
        public string type;
        public float[] value;
    }

    [System.Serializable]
    private class Metadata
    {
        public TimeInstant TimeInstant;
    }

    [System.Serializable]
    private class TimeInstant
    {
        public string type;
        public string value;
    }
}

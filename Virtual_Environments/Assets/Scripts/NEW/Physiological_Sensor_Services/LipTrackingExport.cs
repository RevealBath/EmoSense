using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;

public class LipTrackingExport : MonoBehaviour
{
    private UnityLipData uld;
    private Dictionary<LipShape, float> lipWeightings;
    private LipData ld;

    // Start is called before the first frame update
    void Start()
    {
        uld = new UnityLipData();
        lipWeightings = new Dictionary<LipShape, float>();
        ld = new LipData();
    }

    // Update is called once per frame
    void Update()
    {
        if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING)
        {
            Debug.LogError("SRanipal Lip Framework is not working. Please check the SRanipal Manager.");
            return;
        }

        bool result = SRanipal_Lip.GetLipWeightings(out lipWeightings);
        SRanipal_Lip_API.GetLipData(ref ld);
        
        if (result)
        {
            uld.currLipData = ld;
            uld.currLipWeightings = lipWeightings;
            
            //float[] x = ld.prediction_data.blend_shape_weight;
            //int i = 0;
            //foreach (var pair in lipWeightings)
            //{
            //    string key = pair.Key.ToString();
            //    float value = pair.Value;
            //    Debug.Log(key + ": " + value + " - XVALUE: "+ x[i]);
            //    i++;
            //}
        }
        else
        {
            Debug.Log("Invalid Lip Data");
        }
    }

    public UnityLipData getLatestLipData() { return uld; }
}

public struct UnityLipData
{
    public LipData currLipData { get; set; }
    public Dictionary<LipShape, float> currLipWeightings { get; set; }

    public string getLipWeightingsAsCSVString()
    {
        string weightings = "";

        foreach (var pair in currLipWeightings)
        {
            string key = pair.Key.ToString();
            float value = pair.Value;
            weightings += value+",";
        }
        weightings = weightings.Remove(weightings.Length - 1, 1);
        return weightings;
    }
}

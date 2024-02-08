using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AffectResponseExport : MonoBehaviour
{
    public StudyManager studyManager;

    private CalibrationData cd;
    private string filename;    
    private StreamWriter writeFile;

    private string csv_header = "Unity_Frame,Unity_Time_Stamp,Participant_ID,Condition_Order,Exercise_Intensity,Emotion_Scene_Count,Shown_Scene," +
        "Valence_Rating,Arousal_Rating,Fear_Rating,Stress_Rating,Sad_Rating,Bored_Rating,Calm_Rating,Content_Rating,Happy_Rating,Excited_Rating";

    // Start is called before the first frame update
    void Start()
    {
        CoolDownManager.OnAffectResponsesReady += CoolDownManager_OnAffectResponsesReady;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CoolDownManager_OnAffectResponsesReady(AffectResponses ar)
    {
        writeFile.WriteLine(Time.frameCount + "," +
            System.DateTime.Now.ToString("dd-MM-yyyy - HH:mm:ss.ffffff", System.Globalization.CultureInfo.InvariantCulture) + "," +
            studyManager.participantID.ToString() + "," +
            studyManager.conditionOrder.ToString() + "," +
            studyManager.exerciseIntensity.ToString() + "," +
            ar.emotionSceneCount.ToString() + "," +
            ar.ExposureScene.ToString() + "," +
            ar.valenceValue.ToString() + "," +
            ar.arousalValue.ToString() + "," +
            ar.fearValue.ToString() + "," +
            ar.stressValue.ToString() + "," +
            ar.sadValue.ToString() + "," +
            ar.boredValue.ToString() + "," +
            ar.calmValue.ToString() + "," +
            ar.contentValue.ToString() + "," +
            ar.happyValue.ToString() + "," +
            ar.excitedValue.ToString());
        writeFile.Flush();

        if(ar.emotionSceneCount > 3)        
            CloseAffectResponseFile();
    }

    public void WriteAffectResponseHeader()
    {
        cd = studyManager.GetCalibrationData();
        System.DateTime today = System.DateTime.Now;
        filename = Application.dataPath + "/CSV_Data/" + studyManager.participantID + "_" + studyManager.conditionOrder + "_" + studyManager.exerciseIntensity + "_Affect_Responses_"
            + today.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".csv";
        writeFile = new StreamWriter(filename, false);
        writeFile.WriteLine(csv_header);
    }

    void OnApplicationQuit()
    {
        if (writeFile != null)
            writeFile.Close();
    }

    private void CloseAffectResponseFile()
    {
        if (writeFile != null)
            writeFile.Close();
    }
}

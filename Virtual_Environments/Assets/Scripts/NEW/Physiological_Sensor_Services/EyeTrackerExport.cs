using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using System.IO;
using System;
using System.Linq;
using ViveSR.anipal.Eye;
using System.Net.Mail;
using System.Net;

public class EyeTrackerExport : MonoBehaviour
{
    public Vector3 tobiiDir;
    public Vector3 sRanipalDir;
    public UnityEyeData ued;

    private bool isBlinking = false;
    private float current_blinkDuration = 0.0f;
    private float current_IBI = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ued = new UnityEyeData();
    }

    // Update is called once per frame
    void Update()
    {
        var TobiiEyeTrackingDataWorld = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        var TobiiEyeTrackingDataLocal = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        EyeData SranipalEyeTrackingData = new EyeData();
        SRanipal_Eye_API.GetEyeData(ref SranipalEyeTrackingData);

        Vector3 SranipalGazeOriginCombinedLocal, SranipalGazeDirectionCombinedLocal;

        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out SranipalGazeOriginCombinedLocal, out SranipalGazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out SranipalGazeOriginCombinedLocal, out SranipalGazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out SranipalGazeOriginCombinedLocal, out SranipalGazeDirectionCombinedLocal)) { }
        
        if (TobiiEyeTrackingDataWorld.GazeRay.IsValid)
        {
            ued.tobiiGazePosWorld = TobiiEyeTrackingDataWorld.GazeRay.Origin;
            ued.tobiiGazeDirWorld = TobiiEyeTrackingDataWorld.GazeRay.Direction;            
        }
        else
        {
            ued.tobiiGazePosWorld = new Vector3(-1, -1, -1);
            ued.tobiiGazeDirWorld = new Vector3(-1, -1, -1);
        }

        if (TobiiEyeTrackingDataLocal.GazeRay.IsValid)
        {
            ued.tobiiGazePosLocal = TobiiEyeTrackingDataLocal.GazeRay.Origin;
            ued.tobiiGazeDirLocal = TobiiEyeTrackingDataLocal.GazeRay.Direction;
        }
        else
        {
            ued.tobiiGazePosLocal = new Vector3(-1, -1, -1);
            ued.tobiiGazeDirLocal = new Vector3(-1, -1, -1);
        }

        ued.eyeClosedLeft = TobiiEyeTrackingDataWorld.IsLeftEyeBlinking;
        ued.eyeClosedRight = TobiiEyeTrackingDataWorld.IsRightEyeBlinking;

        if (ued.eyeClosedLeft & ued.eyeClosedRight)
        {
            if (isBlinking)
            {
                current_blinkDuration += Time.deltaTime;
            }
            else
            {
                isBlinking = true;
                current_blinkDuration = Time.deltaTime / 2;
                current_IBI += Time.deltaTime / 2;
                ued.current_interBlinkInterval = current_IBI;
            }
        }
        else
        {
            if (isBlinking)
            {
                isBlinking = false;
                //store blink duration.
                if (current_blinkDuration <= 0.7f)
                {
                    current_blinkDuration += Time.deltaTime / 2;
                    ued.current_blinkDuration = current_blinkDuration;
                    current_IBI = Time.deltaTime / 2;
                }
                else
                {
                    //handle invalid blink
                    current_IBI = Time.deltaTime + current_blinkDuration;
                }
            }
            else
                current_IBI += Time.deltaTime;
        }

        ued.SranipalGazeDirLocal = SranipalGazeDirectionCombinedLocal;
        ued.SranipalGazePosLocal = SranipalGazeOriginCombinedLocal;

        if (SranipalEyeTrackingData.verbose_data.left.pupil_diameter_mm > 0)
            ued.pupilDilationLeft = SranipalEyeTrackingData.verbose_data.left.pupil_diameter_mm;
        else
            ued.pupilDilationLeft = -1.0f;

        if(SranipalEyeTrackingData.verbose_data.right.pupil_diameter_mm > 0)
            ued.pupilDilationRight = SranipalEyeTrackingData.verbose_data.right.pupil_diameter_mm;
        else
            ued.pupilDilationRight = -1.0f;

        tobiiDir = TobiiEyeTrackingDataLocal.GazeRay.Direction;
        sRanipalDir = SranipalGazeDirectionCombinedLocal;
    }

    public UnityEyeData getLatestEyeData() { return ued; }    
}

/***
 * Data Packet for both Sranipal and TobiiEye Data for internal Unity Scripts to use. 
 */
public struct UnityEyeData
{
    public Vector3 tobiiGazePosWorld { get; set; }
    public Vector3 tobiiGazeDirWorld { get; set; }
    public Vector3 tobiiGazePosLocal { get; set; }
    public Vector3 tobiiGazeDirLocal { get; set; }
    public Vector3 SranipalGazePosLocal { get; set; }
    public Vector3 SranipalGazeDirLocal { get; set; }
    public float pupilDilationLeft { get; set; }
    public float pupilDilationRight { get; set; }
    public bool eyeClosedLeft { get; set; }
    public bool eyeClosedRight { get; set; }
    public float current_blinkDuration { get; set; }
    public float current_interBlinkInterval { get; set; }
}
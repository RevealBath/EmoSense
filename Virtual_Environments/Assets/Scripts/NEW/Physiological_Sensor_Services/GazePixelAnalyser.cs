using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using ViveSR.anipal.Eye;
using UnityEngine.Rendering;

public class GazePixelAnalyser : MonoBehaviour
{
    public Camera renderCamera; // Reference to the VR camera (output)
    public RenderTexture render;
    private GazePixelData gpd;
    private string savePath = "Assets/SavedImage.png";

    public float fovealGrayScale;
    public float parafovealGrayScale;
    public float headGrayScale;
    public float imageGrayScale;
    //private CommandBuffer commandBuffer;

    private void Start()
    {
        render = new RenderTexture(270,300,24);
        //render = new RenderTexture(1440,1600,24);
        renderCamera.targetTexture = render;
        gpd = new GazePixelData();
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        // Get the gaze direction from the headset's forward vector
        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;

        // Get Vive Sranipal Data
        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }

        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);

        // Create a temporary Texture2D to read pixel data from the RenderTexture
        Texture2D tempTexture = new Texture2D(render.width, render.height, TextureFormat.RGB48, false);
        RenderTexture.active = render;
        tempTexture.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        // Create a temporary Texture2D to read pixel data from the RenderTexture
        gpd.foveal_gray_scale_value = SamplePixelsInCircularRegion(tempTexture, GazeDirectionCombined, 2.0f); //foveal
        gpd.parafoveal_gray_scale_value = SamplePixelsInCircularRegion(tempTexture, GazeDirectionCombined, 10.0f); //parafoveal
        gpd.headpoint_gray_scale_value = SamplePixelsInCircularRegion(tempTexture, Camera.main.transform.forward, 10.0f); //headpoint
        gpd.image_gray_scale_value = SamplePixelsOfCameraImage(tempTexture); //image
                
        fovealGrayScale = gpd.foveal_gray_scale_value;
        parafovealGrayScale = gpd.parafoveal_gray_scale_value;
        headGrayScale = gpd.headpoint_gray_scale_value;
        imageGrayScale = gpd.image_gray_scale_value;

        // keyboard input for screenshot
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Encode the Texture2D to a PNG file
            byte[] bytes = tempTexture.EncodeToPNG();

            // Save the PNG file to disk
            System.IO.File.WriteAllBytes(savePath, bytes);

            // Reset the active RenderTexture
            RenderTexture.active = null;

            Debug.Log("RenderTexture saved as PNG: " + savePath);
        }
        Destroy(tempTexture);
    }

    private float SamplePixelsInCircularRegion(Texture2D r_texture, Vector3 gazeDirection, float visualAngle)
    {
        Vector2 uv = new Vector2(0.5f + gazeDirection.x * 0.5f, 0.5f + gazeDirection.y * 0.5f);

        // Convert UV coordinates to pixel coordinates
        int x = Mathf.RoundToInt(uv.x * r_texture.width);
        int y = Mathf.RoundToInt(uv.y * r_texture.height);

        // Calculate the number of pixels in each direction based on visual angle
        float radiusInRadians = Mathf.Deg2Rad * visualAngle;
        int numOfPixels = Mathf.RoundToInt(radiusInRadians * gazeDirection.magnitude * Mathf.Max(r_texture.width, r_texture.height));
        //float radiusInPixels = visualAngle * Mathf.Max(r_texture.width, r_texture.height) / 360f;

        // Calculate the average grayscale value
        float totalGrayScale = 0f;
        int pixelCount = 0;

        for (int i = -numOfPixels; i <= numOfPixels; i++)
        {
            for (int j = -numOfPixels; j <= numOfPixels; j++)
            {
                int sampleX = x + i;
                int sampleY = y + j;

                if (sampleX >= 0 && sampleX < r_texture.width && sampleY >= 0 && sampleY < r_texture.height)
                {
                    Color pixelColor = r_texture.GetPixel(sampleX, sampleY);
                    //float grayScale = (pixelColor.r + pixelColor.g + pixelColor.b) / 3f;
                    float grayScale = pixelColor.grayscale;
                    grayScale = Mathf.RoundToInt(grayScale * 255f);
                    totalGrayScale += grayScale;
                    pixelCount++;
                }
            }
        }

        float averageGrayScale = totalGrayScale / pixelCount;

        // Cleanup: Destroy the temporary Texture2D
        //Destroy(tempTexture);

        return averageGrayScale;
    }

    private float SamplePixelsOfCameraImage(Texture2D r_texture)
    {
        // Calculate the average grayscale value
        float sumGrayscale = 0f;
        Color[] pixels = r_texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            float grayscaleValue = pixels[i].grayscale;
            grayscaleValue = Mathf.RoundToInt(grayscaleValue * 255f);
            sumGrayscale += grayscaleValue;
        }

        float averageGrayscale = sumGrayscale / pixels.Length;

        return averageGrayscale;
    }

    public GazePixelData GetLatestGazePixelData() { return gpd; }
}

public struct GazePixelData
{
    public float foveal_gray_scale_value { get; set; }
    public float parafoveal_gray_scale_value { get; set; }
    public float headpoint_gray_scale_value { get; set; }
    public float image_gray_scale_value { get; set; }
}

using UnityEngine;

public class ColorSchemeManager : MonoBehaviour
{
  public static ColorSchemeManager Instance { get; private set; }

  public ColorScheme prevColorScheme = ColorScheme.Standard;
  public ColorScheme colorScheme = ColorScheme.Stress;

  public Color topColor;
  public Color middleColor;
  public Color bottomColor;
  public bool changeColor = false; //toggle's colour lerping when set to true
  float elapsed = 0f;
  private float timer = 0f;
  //float changeColorSpeed = .1f;
  float t = 0f; //lerps the colours
  
  public AudioSource backgroundMusic;
  public AudioClip stressSoundtrack;
  public AudioClip joySoundtrack;
  public AudioClip sadnessSoundtrack;
  public AudioClip serenitySoundtrack;

  public bool userReady = false;

  public int[] skyOrder;

  private int sceneNumber = 0;
  
  //Scene order: ColorScheme.Stress, ColorScheme.Joy, ColorScheme.Sadness, ColorScheme.Serenity

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    userReady = false;
    ColorSetup();
  }

  void ColorSetup()
  {
    prevColorScheme = ColorScheme.Standard;
    colorScheme = ColorScheme.Standard;
    Color[] colors = Colors.GetColorScheme(colorScheme); //gets the colours of the first scheme you want
    topColor = colors[0];
    middleColor = colors[1];
    bottomColor = colors[2];
  }

  void ColorLerp()
  {
    Color[] prevColors = Colors.GetColorScheme(prevColorScheme);
    Color[] colors = Colors.GetColorScheme(colorScheme);
    topColor = Color.Lerp(prevColors[0], colors[0], t);
    middleColor = Color.Lerp(prevColors[1], colors[1], t);
    bottomColor = Color.Lerp(prevColors[2], colors[2], t);
  }
  
  private void Update()
  {
    //if (Input.GetKeyDown(KeyCode.Keypad1))
    //{
    //  prevColorScheme = colorScheme;
    //  colorScheme = ColorScheme.Stress;
    //  changeColor = true;
    //}

    if (Input.GetKeyDown(KeyCode.Space))
    {
      userReady = true;
    }
    
    if (userReady)
    {
        elapsed += Time.deltaTime;
        //if (elapsed >= changeColorSpeed)
        //{
        //  elapsed = 0;
        if (elapsed is >= 0.0f and <= 60.0f && changeColor == false)
        {
          colourSelection();
        }
        
        if (elapsed is >= 60.0f and <= 120.0f && changeColor == false)
        {
          prevColorScheme = colorScheme;
          colorScheme = ColorScheme.Standard;
          backgroundMusic.Stop();
          changeColor = true;
          timer = 0f;
        }
    
        if (elapsed is >= 120.0f and <= 180.0f && changeColor == false)
        {
          colourSelection();
        }
        
        if (elapsed is >= 180.0f and <= 240.0f && changeColor == false)
        {
          prevColorScheme = colorScheme;
          colorScheme = ColorScheme.Standard;
          backgroundMusic.Stop();
          changeColor = true;
          timer = 0f;
        }
    
        if (elapsed is >= 240.0f and <= 300.0f && changeColor == false)
        {
          colourSelection();
        }
        
        if (elapsed is >= 300.0f and <= 360.0f && changeColor == false)
        {
          prevColorScheme = colorScheme;
          colorScheme = ColorScheme.Standard;
          backgroundMusic.Stop();
          changeColor = true;
          timer = 0f;
        }
        
        if (elapsed is >= 360.0f and <= 420.0f && changeColor == false)
        {
          colourSelection();
        }
        
        if (elapsed is >= 420.0f and <= 480.0f && changeColor == false)
        {
          prevColorScheme = colorScheme;
          colorScheme = ColorScheme.Standard;
          backgroundMusic.Stop();
          changeColor = true;
          timer = 0f;
        }

        timer += Time.deltaTime;
        
        if (changeColor)
        {
          if (t <= 1)
          {
            t += .002f;
            ColorLerp();
            SkyboxManager.Instance.SetSkyboxColor();
          }
          if (backgroundMusic.volume <= 1)
          {
            backgroundMusic.volume += 0.005f;
          }
          if (timer >= 60.0f)
          {
            changeColor = false;
            t = 0;
            timer = 0;
          }
        }
        //}
    }
  }

  public void colourSelection()
  {
    if (skyOrder[sceneNumber] == 1)
    {
      prevColorScheme = colorScheme;
      colorScheme = ColorScheme.Stress;
      backgroundMusic.Stop();
      backgroundMusic.clip = stressSoundtrack;
      backgroundMusic.Play();
      changeColor = true;
      timer = 0f;
    }
    else if (skyOrder[sceneNumber] == 2)
    {
      prevColorScheme = colorScheme;
      colorScheme = ColorScheme.Joy;
      backgroundMusic.Stop();
      backgroundMusic.clip = joySoundtrack;
      backgroundMusic.Play();
      changeColor = true;
      timer = 0f;
    }
    else if (skyOrder[sceneNumber] == 3)
    {
      prevColorScheme = colorScheme;
      colorScheme = ColorScheme.Sadness;
      backgroundMusic.Stop();
      backgroundMusic.clip = sadnessSoundtrack;
      backgroundMusic.Play();
      changeColor = true;
      timer = 0f;
    }
    else
    {
      prevColorScheme = colorScheme;
      colorScheme = ColorScheme.Serenity;
      backgroundMusic.Stop();
      backgroundMusic.clip = serenitySoundtrack;
      backgroundMusic.Play();
      changeColor = true;
      timer = 0f;
    }
    sceneNumber += 1;
  }

}

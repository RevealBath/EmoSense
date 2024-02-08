using System;
using UnityEngine;
using System.Collections.Generic;

public enum ColorScheme { Standard, Stress, Joy, Sadness, Serenity };

public class Colors : MonoBehaviour
{
  public static List<ColorScheme> colorSchemeList = new List<ColorScheme> { ColorScheme.Standard, ColorScheme.Stress, ColorScheme.Joy, ColorScheme.Sadness, ColorScheme.Serenity };
  public static Color[] GetStandardScheme()
  {
    return new Color[] { new Color(1f,1f,1f) , new Color(1f,1f,1f) , new Color(1f,1f,1f) };
  }

  public static Color[] GetStressScheme()
  {
    return new Color[] { new Color(0.91f, 0.439f, 0.725f) , new Color(0.706f, 0.333f, 0.412f) , Color.black };
  }
  
  public static Color[] GetJoyScheme()
  {
    return new Color[] { new Color(0.98f, 0.784f, 0.024f) , new Color(1f, 0.384f, 0f) , Color.green };
  }

  public static Color[] GetSadnessScheme()
  {
    return new Color[] { new Color(0.467f, 0.518f, 0.596f) , new Color(0.341f, 0.439f, 0.6f) , Color.black };
  }
  
  public static Color[] GetSerenityScheme()
  {
    return new Color[] { new Color(0.416f, 0.98f, 0.863f) , new Color(0.384f, 0.745f, 0.255f) , Color.green };
  }

  public static Color[] GetColorScheme(ColorScheme colorScheme)
  {
    switch (colorScheme)
    {
      case ColorScheme.Standard:
        return GetStandardScheme();
      case ColorScheme.Stress:
        return GetStressScheme();
      case ColorScheme.Joy:
        return GetJoyScheme();
      case ColorScheme.Sadness:
        return GetSadnessScheme();
      case ColorScheme.Serenity:
        return GetSerenityScheme();
      default:
        return GetStandardScheme();
    }
  }

}

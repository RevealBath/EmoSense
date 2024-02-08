using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
  public static SkyboxManager Instance { get; private set; }
  Skybox skybox;

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      skybox = gameObject.GetComponent<Skybox>();
    }
    SetSkyboxColor();
  }

  public void SetSkyboxColor()
  {
    Material mat = skybox.material;
    mat.SetColor("_SkyColor1", ColorSchemeManager.Instance.topColor);
    mat.SetColor("_SkyColor2", ColorSchemeManager.Instance.middleColor);
    mat.SetColor("_SkyColor3", ColorSchemeManager.Instance.bottomColor);
  }
}

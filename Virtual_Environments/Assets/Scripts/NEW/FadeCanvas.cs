using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvas : MonoBehaviour
{
    public float fadeDuration = 1.0f;       // Duration of the fade animation
    public float blankDuration = 1.0f;      // Duration to stay blank
    public float fadeInDuration = 1.0f;     // Duration of the fade-in animation
    private Renderer[] renderers;           // Reference to the Renderer components of the child object and its children
    private Graphic[] graphics;             // Reference to the Graphic components of the child object and its children
    private bool fading = false;            // Flag to check if the object is currently fading

    private void Awake()
    {
        // Get all the Renderer and Graphic components from the child object and its children
        renderers = GetComponentsInChildren<Renderer>();
        graphics = GetComponentsInChildren<Graphic>();
    }

    private System.Collections.IEnumerator FadeInOutRoutine()
    {
        fading = true;

        // Fade Out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Set all the Renderer and Graphic components to be completely invisible
        SetAlpha(0f);

        // Wait for the blank duration
        yield return new WaitForSeconds(blankDuration);

        StartCoroutine(FadeInRoutine());
    }

    private System.Collections.IEnumerator FadeInRoutine()
    {
        // Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Set all the Renderer and Graphic components to be completely visible
        SetAlpha(1f);

        fading = false;
    }

    private System.Collections.IEnumerator FadeOutRoutine()
    {
        fading = true;

        // Fade Out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Set all the Renderer and Graphic components to be completely invisible
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }
        }

        if (graphics != null)
        {
            foreach (Graphic graphic in graphics)
            {
                Color color = graphic.color;
                color.a = alpha;
                graphic.color = color;
            }
        }
    }

    public void FadeInSetActive()
    {
        // If not currently fading, start the fade-in process

        gameObject.SetActive(true);  // Activate the GameObject

        StartCoroutine(FadeInRoutine());
        
    }

    public void FadeInOut()
    {
        // If not currently fading, start the fade-out process


         StartCoroutine(FadeInOutRoutine());
        
    }

    public void FadeOutSetUnactive()
    {
        // If not currently fading, start the fade-out process


        StartCoroutine(FadeOutRoutine());            
        
    }
}

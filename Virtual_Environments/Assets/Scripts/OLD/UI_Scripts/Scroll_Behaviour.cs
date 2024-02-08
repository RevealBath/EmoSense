using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll_Behaviour : MonoBehaviour
{
    [SerializeField] RectTransform scrollContent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScrollUp() { scrollContent.position = new Vector3(scrollContent.position.x, scrollContent.position.y - 0.5f, scrollContent.position.z); }

    public void ScrollDown() { scrollContent.position = new Vector3(scrollContent.position.x, scrollContent.position.y+0.5f, scrollContent.position.z); }
}

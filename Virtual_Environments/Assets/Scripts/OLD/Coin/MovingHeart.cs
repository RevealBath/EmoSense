using UnityEngine;

public class MovingHeart : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, 10f * Time.deltaTime, Space.Self);
    }
}

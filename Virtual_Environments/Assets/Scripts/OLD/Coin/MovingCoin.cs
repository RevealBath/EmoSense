using UnityEngine;

public class MovingCoin : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(10f * Time.deltaTime, 0f, 0f, Space.Self);
    }
}

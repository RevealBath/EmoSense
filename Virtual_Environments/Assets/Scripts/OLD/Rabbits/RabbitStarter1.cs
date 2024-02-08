using UnityEngine;

public class RabbitStarter1 : MonoBehaviour
{
    public Animator rabbitRun;
    public bool play = false;
    public float timePassed = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        rabbitRun = GetComponent<Animator>();
        rabbitRun.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (play)
        {
            rabbitRun.enabled = true;
            timePassed += Time.deltaTime;
            if (timePassed >= 15.0f)
            {
                rabbitRun.enabled = false;
            }
        }
    }

    public void PlayPressed()
    {
        play = true;
    }
}

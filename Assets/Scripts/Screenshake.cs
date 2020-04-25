using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Screenshake : MonoBehaviour
{
    private Vector3 shakeOffset;
    private Vector3 shakeAmount;
    private float shakeTime;

    public void Shake(Vector3 amount, float time)
    {
        shakeTime = time;
        shakeAmount = amount;
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTime > 0)
        {
            shakeOffset.x = Random.Range(-shakeAmount.x, shakeAmount.x);
            shakeOffset.y = Random.Range(-shakeAmount.y, shakeAmount.y);

            shakeAmount.x = Mathf.MoveTowards(shakeAmount.x, 0, Time.deltaTime);
            shakeAmount.y = Mathf.MoveTowards(shakeAmount.y, 0, Time.deltaTime);

            transform.position = shakeOffset;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            transform.position = new Vector3(0, 0, 0);
        }
    }
}

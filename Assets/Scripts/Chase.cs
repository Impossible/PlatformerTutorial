using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : MonoBehaviour
{
    public GameObject target;
    public Bounds zone;
    public float speed;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(zone.center, zone.size);
    }

    // Update is called once per frame
    void Update()
    {
        if(!zone.Contains(target.transform.position - transform.position))
        {
            float cameraZ = transform.position.z;
            Vector3 offset = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            offset.z = cameraZ;
            transform.position = offset;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public float minDistance = 0.6f;
    public float maxDistance = 3.0f;

    public float minAngle = -30f;
    public float maxAngle = 60;

    public float smooth = 30.0f;
    Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;
    public float distance;
    public float angle;

    Quaternion rot;

    private Vector3 dadPosition;

    // Use this for initialization
    void Start()
    {
        dollyDir = (transform.localPosition).normalized;
        distance = transform.localPosition.magnitude;
        rot = transform.localRotation;
        dadPosition = transform.parent.position;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        Vector3 r = Vector3.Cross(transform.forward, transform.up);

        Debug.DrawRay(transform.position, r, Color.blue);
        Debug.DrawRay(transform.position, -r, Color.red);

        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit))
        {
            if (hit.transform.tag != "Player" && hit.transform.tag != "PlayerColliders")
            {
                distance = Mathf.Clamp((hit.distance * 0.8f), minDistance, maxDistance);
                //if (Physics.Raycast(transform.position, -r, 5))
                //{

                //}
                //else
                //{
                //    angle = Mathf.Clamp((hit.distance) * 5, minAngle, maxAngle);
                //    Quaternion rotAux = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
                //    rot = Quaternion.RotateTowards(rot, rotAux, 1f);
                //}

                //if (Physics.Raycast(transform.position, r, 5))
                //{

                //}
                //else
                //{
                //    angle = -Mathf.Clamp((hit.distance) * 5, minAngle, maxAngle);
                //    Quaternion rotAux = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
                //    rot = Quaternion.RotateTowards(rot, rotAux, 1f);
                //}

            }
        }
        else
        {
            distance = maxDistance;
            //angle = 0;
            //Quaternion rotAux = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
            //rot = Quaternion.Slerp(rot, rotAux, 0.5f);
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
        transform.localRotation = rot;
    }
}

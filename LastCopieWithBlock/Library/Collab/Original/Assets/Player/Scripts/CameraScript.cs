using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    //Constantes do Y;
    private const float Y_ANGLE_MIN = -50.0f;
    private const float Y_ANGLE_MAX = 50.0f;

    private const float originalMaxDistance = 3.0f;
    private const float originalMinDistance = 0.6f;
    private const float runningMaxDistance = 6.0f;

    //Target
    public Transform lookAt;
    public Transform lookingAt;
    bool lockOn = false;

    //InGame Variables
    private float distance = 10.0f;
    public float mouseX = 0.0f;
    public float mouseY = 0.0f;

    public float moveSpeed = 120.0f;
    [Range(1.0f, 150.0f)]
    public float sensitivity = 4.0f;
    //public float smoothX;
    //public float smoothY;
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    //Cinematic Variables
    public bool lockedCamera = false;

    public Quaternion rot;

    //Original Values;
    Quaternion originalRot;
    Vector3 originalPos;
    float Original_MaxOffsetDistance, Original_MinOffsetDistance;

    private void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalPos = transform.position;
        originalRot = transform.rotation;
        Original_MaxOffsetDistance = transform.GetComponentInChildren<CameraCollision>().maxDistance;
        Original_MinOffsetDistance = transform.GetComponentInChildren<CameraCollision>().minDistance;
        lookingAt = lookAt;
    }

    //private void Update()
    //{

    //}

    private void Update()
    {
        if (!lockedCamera)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            //Attention: Y e X estão trocados

            rotX += mouseY * sensitivity * Time.deltaTime;
            rotY += mouseX * sensitivity * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, Y_ANGLE_MIN, Y_ANGLE_MAX);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            transform.rotation = localRotation;

            if (Input.GetKeyDown(KeyCode.M))
            {
                Zoom(3.5f,3);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Zoom(-3.5f, 3);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                OriginalValues();
                lockedCamera = false;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                RotateCamera(360, 0, 4);
                lockedCamera = false;
            }
        }
        else
        {
            //Vector3 dir = transform.position - lookingAt.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookingAt.position - transform.position);
            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5);
            transform.position = new Vector3(transform.position.x, (int)transform.position.y + 2, transform.position.z);

        }
    }

    private void LateUpdate()
    {
        CameraUpdater();
    }

    void CameraUpdater()
    {
        Transform target = lookAt.transform;
        //Camera move-se até ao objecto target
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

    #region ZOOM
    public void Zoom(float distance, float duration)
    {
        StartCoroutine(Zooming(distance, duration));
    }

    private IEnumerator Zooming(float distance, float duration)
    {
        for (float clock = 0; clock < duration; clock += Time.deltaTime)
        {
            transform.GetComponentInChildren<CameraCollision>().maxDistance += distance / duration * Time.deltaTime;
            yield return null;// new WaitForSeconds(0.1f);
        }

    }
    #endregion

    #region RunningZoom
    public void FixedZoom(float duration)
    {
        if (transform.GetComponentInChildren<CameraCollision>().maxDistance < runningMaxDistance)
            transform.GetComponentInChildren<CameraCollision>().maxDistance += 0.1f;

        transform.GetComponentInChildren<Camera>().fieldOfView += 1f;

        if (transform.GetComponentInChildren<CameraCollision>().maxDistance > runningMaxDistance)
            transform.GetComponentInChildren<CameraCollision>().maxDistance = runningMaxDistance;
    }


    public void FixedZoomOut(float duration)
    {
        if (transform.GetComponentInChildren<CameraCollision>().maxDistance > originalMaxDistance)
            transform.GetComponentInChildren<CameraCollision>().maxDistance -= 0.5f;

        transform.GetComponentInChildren<Camera>().fieldOfView = 80;

        if (transform.GetComponentInChildren<CameraCollision>().maxDistance < originalMaxDistance)
            transform.GetComponentInChildren<CameraCollision>().maxDistance = originalMaxDistance;
    }

    #endregion


    #region Rotate
    public void RotateCamera(float angleX, float angleY, float duration)
    {
        StartCoroutine(RotatingCamera(angleX, angleY, duration));
    }

    private IEnumerator RotatingCamera(float angleX, float angleY, float duration)
    {
        for (float clock = 0; clock < duration; clock += Time.deltaTime)
        {
            rotX += angleY / duration * Time.deltaTime;
            rotY += angleX / duration * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, Y_ANGLE_MIN, Y_ANGLE_MAX);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            transform.rotation = localRotation;

            yield return null;// new WaitForSeconds(0.1f);
        }
    }

    public void RotateAngleShortestWay()
    {
        float ang = Vector3.SignedAngle(lookAt.transform.forward, transform.forward, Vector3.up);
        if (ang > 0)
        {
            ang = 180 - ang;
            RotateCamera(ang, 0, 0.5f);
        }
        else
        {
            ang = 180 + ang;
            RotateCamera(-ang, 0, 0.5f);
        }
    }
    #endregion

    public void OriginalValues()
    {
        lockedCamera = true;
        mouseX = 0;
        mouseY = 0;
        rotX = 0;
        rotY = 0;
        transform.rotation = originalRot;
        transform.GetComponentInChildren<CameraCollision>().maxDistance = Original_MaxOffsetDistance;
        transform.GetComponentInChildren<CameraCollision>().minDistance = Original_MinOffsetDistance;
    }

    public void ChangeTarget(Transform other)
    {
        lockedCamera = true;
        lookingAt = other;
        //lookAt = other;
    }

    public void UnlockCamera()
    {
        lockedCamera = false;
        lookingAt = null;
    }
}

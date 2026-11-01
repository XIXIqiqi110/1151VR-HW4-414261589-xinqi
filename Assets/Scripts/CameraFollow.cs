using UnityEngine;

// 1151VR-HW4: 專業級平滑動態跟隨攝影機 (含撞擊震動效果)
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3.5f, -8.8f);
    public float smoothSpeed = 14f;

    private float shakeAmount = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        if (shakeAmount > 0.01f)
        {
            desiredPosition += Random.insideUnitSphere * shakeAmount;
            shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.deltaTime * 3.5f);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        Vector3 lookTarget = target.position + target.forward * 14f;
        transform.LookAt(lookTarget);
    }

    public void TriggerShake(float amount)
    {
        shakeAmount = Mathf.Max(shakeAmount, amount);
    }
}
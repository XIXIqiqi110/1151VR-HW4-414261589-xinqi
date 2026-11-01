using UnityEngine;

// 1151VR-HW4: 障礙物碰撞處理 (撞到東西遊戲立即停止、觸發零件解體爆炸、播放音效與 Game Over 介面)
public class ObstacleCollision : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        TryTriggerCrash(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        TryTriggerCrash(other.gameObject);
    }

    void TryTriggerCrash(GameObject hitObject)
    {
        if (hasTriggered) return;

        AircraftFlightController aircraft = hitObject.GetComponent<AircraftFlightController>();
        if (aircraft == null) aircraft = hitObject.GetComponentInParent<AircraftFlightController>();

        if (aircraft != null)
        {
            hasTriggered = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCrashSound();
            }

#if UNITY_2023_1_OR_NEWER
            CameraFollow cam = Object.FindFirstObjectByType<CameraFollow>();
#else
            CameraFollow cam = FindObjectOfType<CameraFollow>();
#endif
            if (cam != null) cam.TriggerShake(3.0f);

            aircraft.Die("💥 撞擊立體障礙物！機體解體迫降墜毀！");
        }
    }
}
using UnityEngine;

// 1151VR-HW4: 檢查點光環 (旋轉動畫、穿梭音效與加分)
[RequireComponent(typeof(Collider))]
public class RingCheckpoint : MonoBehaviour
{
    public Transform outerGimbal;
    public int scoreReward = 100;
    private bool hasPassed = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        if (outerGimbal != null)
        {
            outerGimbal.Rotate(0, 0, 75f * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasPassed) return;

        if (other.CompareTag("Player") || other.GetComponent<AircraftFlightController>() != null || other.GetComponentInParent<AircraftFlightController>() != null)
        {
            hasPassed = true;

            // 1. 播放「順利通過障礙物音效」
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPassRingSound();
            }

            // 2. 加分
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreReward);
            }

            // 3. 通過後變為亮綠色提示
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material.color = Color.green;
                r.material.SetColor("_EmissionColor", Color.green * 1.5f);
            }
        }
    }
}
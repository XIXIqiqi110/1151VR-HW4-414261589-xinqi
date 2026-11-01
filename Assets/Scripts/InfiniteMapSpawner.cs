using System.Collections.Generic;
using UnityEngine;

// 1151VR-HW4: 無限隨機地圖生成器 (隨機障礙物、動態光環門、可見 65m 發光能量頂棚、自動回收釋放效能)
public class InfiniteMapSpawner : MonoBehaviour
{
    public Transform playerAircraft;
    public float spawnDistanceAhead = 360f;
    public float despawnDistanceBehind = 60f;

    [Header("材質參考 (由建置精靈自動關聯)")]
    public Material ringNeonMat;
    public Material ringGoldMat;
    public Material hazardRockMat;
    public Material hazardGlowMat;
    public Material runwayMat;
    public Material energyCeilingMat; // 半透明發光能量頂棚材質

    private float nextSpawnZ = 45f;
    private List<GameObject> activeObjects = new List<GameObject>();
    private List<GameObject> runwayTiles = new List<GameObject>();
    private float nextRunwayZ = 0f;
    private int ringIndex = 0;

    void Start()
    {
        if (playerAircraft == null)
        {
#if UNITY_2023_1_OR_NEWER
            AircraftFlightController ac = Object.FindFirstObjectByType<AircraftFlightController>();
#else
            AircraftFlightController ac = FindObjectOfType<AircraftFlightController>();
#endif
            if (ac != null) playerAircraft = ac.transform;
        }

        // 初始化前段跑道地塊 (每片長 150m)
        for (int i = 0; i < 4; i++)
        {
            SpawnRunwayTile();
        }

        // 初始化前段航道光環與隨機障礙物 (預先生成 6 組)
        for (int i = 0; i < 6; i++)
        {
            SpawnNextSegment();
        }
    }

    void Update()
    {
        if (playerAircraft == null) return;

        float playerZ = playerAircraft.position.z;

        // 1. 動態無限生成航道前方光環與隨機障礙物
        while (playerZ + spawnDistanceAhead > nextSpawnZ)
        {
            SpawnNextSegment();
        }

        // 2. 動態無限延伸地面賽道與發光能量頂棚
        while (playerZ + spawnDistanceAhead > nextRunwayZ)
        {
            SpawnRunwayTile();
        }

        // 3. 自動回收玩家後方的物件 (防止記憶體無限增長，保持 60fps 流暢度)
        float cutoffZ = playerZ - despawnDistanceBehind;
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (activeObjects[i] != null && activeObjects[i].transform.position.z < cutoffZ)
            {
                Destroy(activeObjects[i]);
                activeObjects.RemoveAt(i);
            }
        }

        for (int i = runwayTiles.Count - 1; i >= 0; i--)
        {
            if (runwayTiles[i] != null && runwayTiles[i].transform.position.z + 75f < cutoffZ)
            {
                Destroy(runwayTiles[i]);
                runwayTiles.RemoveAt(i);
            }
        }
    }

    void SpawnRunwayTile()
    {
        // 1. 地面跑道地板 (Floor Collider)
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
        tile.name = "Cyber_Runway_Floor_" + nextRunwayZ;
        tile.transform.SetParent(transform);
        tile.transform.position = new Vector3(0, -6, nextRunwayZ + 75f);
        tile.transform.localScale = new Vector3(9, 1, 15); // 90m 寬, 150m 長
        if (runwayMat != null) tile.GetComponent<Renderer>().sharedMaterial = runwayMat;
        tile.AddComponent<ObstacleCollision>();
        runwayTiles.Add(tile);

        // 2. 頂部發光能量天花板 (Ceiling Barrier - 擴展至 65m 高度，清楚可見！)
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Cyber_Energy_Ceiling_" + nextRunwayZ;
        ceiling.transform.SetParent(transform);
        ceiling.transform.position = new Vector3(0, 65f, nextRunwayZ + 75f);
        ceiling.transform.localRotation = Quaternion.Euler(180, 0, 0); // 碰撞面朝下
        ceiling.transform.localScale = new Vector3(9, 1, 15);

        Renderer ceilRen = ceiling.GetComponent<Renderer>();
        if (ceilRen != null)
        {
            ceilRen.enabled = true; // 清楚可見的半透明發光能量網！
            if (energyCeilingMat != null) ceilRen.sharedMaterial = energyCeilingMat;
        }

        ceiling.AddComponent<ObstacleCollision>();
        runwayTiles.Add(ceiling);

        nextRunwayZ += 150f;
    }

    void SpawnNextSegment()
    {
        ringIndex++;
        float z = nextSpawnZ;
        // 航道平滑波浪起伏 (左右 -16 ~ +16, 高低 8 ~ 26)
        float x = Mathf.Sin(ringIndex * 0.55f) * 16f + Random.Range(-2.5f, 2.5f);
        float y = 14f + Mathf.Cos(ringIndex * 0.45f) * 8.5f + Random.Range(-2f, 2f);

        // --- A. 生成雙環旋轉檢查點能量門 ---
        GameObject ring = new GameObject("CheckpointRing_Inf_" + ringIndex);
        ring.transform.SetParent(transform);
        ring.transform.position = new Vector3(x, y, z);

        GameObject innerNeon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerNeon.name = "Inner_Energy_Ring";
        innerNeon.transform.SetParent(ring.transform);
        innerNeon.transform.localPosition = Vector3.zero;
        innerNeon.transform.localRotation = Quaternion.Euler(90, 0, 0);
        innerNeon.transform.localScale = new Vector3(8.5f, 0.28f, 8.5f);
        if (ringNeonMat != null) innerNeon.GetComponent<Renderer>().sharedMaterial = ringNeonMat;
        Destroy(innerNeon.GetComponent<Collider>());

        GameObject outerGimbal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        outerGimbal.name = "Outer_Gimbal_Ring";
        outerGimbal.transform.SetParent(ring.transform);
        outerGimbal.transform.localPosition = Vector3.zero;
        outerGimbal.transform.localRotation = Quaternion.Euler(90, 0, 0);
        outerGimbal.transform.localScale = new Vector3(9.8f, 0.2f, 9.8f);
        if (ringGoldMat != null) outerGimbal.GetComponent<Renderer>().sharedMaterial = ringGoldMat;
        Destroy(outerGimbal.GetComponent<Collider>());

        SphereCollider ringTrigger = ring.AddComponent<SphereCollider>();
        ringTrigger.isTrigger = true;
        ringTrigger.radius = 4.8f;
        RingCheckpoint rc = ring.AddComponent<RingCheckpoint>();
        rc.outerGimbal = outerGimbal.transform;
        activeObjects.Add(ring);

        // --- B. 隨機生成立體障礙物 1：立體岩石水晶石柱 (柱身 + 紅色警示光環) ---
        float sideSign = (Random.value > 0.5f) ? 1f : -1f;
        float pillarX = x + sideSign * Random.Range(9f, 15f);
        float pillarY = y + Random.Range(-4f, 6f);
        float pillarZ = z + Random.Range(10f, 25f);

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Obstacle_Pillar_" + ringIndex;
        pillar.transform.SetParent(transform);
        pillar.transform.position = new Vector3(pillarX, pillarY, pillarZ);
        pillar.transform.localScale = new Vector3(Random.Range(2.8f, 4.5f), 28f, Random.Range(2.8f, 4.5f));
        if (hazardRockMat != null) pillar.GetComponent<Renderer>().sharedMaterial = hazardRockMat;

        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beacon.name = "Danger_Beacon";
        beacon.transform.SetParent(pillar.transform);
        beacon.transform.localPosition = Vector3.zero;
        beacon.transform.localScale = new Vector3(1.3f, 0.2f, 1.3f);
        if (hazardGlowMat != null) beacon.GetComponent<Renderer>().sharedMaterial = hazardGlowMat;
        Destroy(beacon.GetComponent<Collider>());

        pillar.AddComponent<ObstacleCollision>();
        activeObjects.Add(pillar);

        // --- C. 隨機生成立體障礙物 2：懸浮太空小行星隕石 (隨機多面旋轉與不規則縮放) ---
        if (Random.value < 0.75f)
        {
            float asteroidX = x - sideSign * Random.Range(8f, 16f);
            float asteroidY = y + Random.Range(2f, 10f);
            float asteroidZ = z + Random.Range(-10f, 15f);

            GameObject asteroid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            asteroid.name = "Obstacle_Asteroid_" + ringIndex;
            asteroid.transform.SetParent(transform);
            asteroid.transform.position = new Vector3(asteroidX, asteroidY, asteroidZ);
            asteroid.transform.localScale = new Vector3(
                Random.Range(4f, 7f),
                Random.Range(3.5f, 6f),
                Random.Range(4f, 7f)
            );
            asteroid.transform.rotation = Random.rotation;
            if (hazardRockMat != null) asteroid.GetComponent<Renderer>().sharedMaterial = hazardRockMat;
            asteroid.AddComponent<ObstacleCollision>();
            activeObjects.Add(asteroid);
        }

        // --- D. 隨機生成立體障礙物 3：中央浮空障礙橫樑 (考驗鑽越反應) ---
        if (ringIndex > 2 && Random.value < 0.35f)
        {
            float beamZ = z + Random.Range(20f, 32f);
            float beamY = y + (Random.value > 0.5f ? 6.5f : -6.5f);
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Obstacle_CrossBeam_" + ringIndex;
            beam.transform.SetParent(transform);
            beam.transform.position = new Vector3(x, beamY, beamZ);
            beam.transform.localScale = new Vector3(24f, 2f, 2.4f);
            if (hazardRockMat != null) beam.GetComponent<Renderer>().sharedMaterial = hazardRockMat;
            beam.AddComponent<ObstacleCollision>();
            activeObjects.Add(beam);
        }

        nextSpawnZ += Random.Range(50f, 65f);
    }
}
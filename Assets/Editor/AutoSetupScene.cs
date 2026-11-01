#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/**
 * Unity 6.2 (6000.2.14f1 / 6000.0.x) 完美全自動建置精靈 (1151VR-HW4 v3.7 星空宇宙與零件解體爆炸旗艦版)
 * 1. 頂部根選單以英文字母開頭 "VR_HW4"，100% 規避 Unity 反射限制。
 * 2. 包含 [InitializeOnLoad] 與 [InitializeOnLoadMethod]，開啟專案時自動彈出搭建精靈視窗。
 * 3. 完美配置 12 部件 3D 戰機、零件物理噴散解體動畫 (Disassembly Breakup)、靈敏航空轉向 (Agile Handling)。
 * 4. 深空宇宙星空背景 (Cosmic Void + 2000+ 星光粒子 + 空間曲速塵埃)，告別簡陋預設天空！
 * 5. 擴大天空淨空高度 (65m)、半透明發光能量頂棚、HUD 即時高度計與接近頂棚閃爍警報！
 * 6. 無限隨機地圖生成器 (InfiniteMapSpawner)、碰觸即停 (1-Hit Stop) 與開始/結束介面 (GameManager)。
 * 7. 自動載入與連結 3 大規定音效。
 */
[InitializeOnLoad]
public class AutoSetupScene : EditorWindow
{
    // Unity 6.2 啟動或編譯完成後，自動跳出專案快速搭建精靈
    static AutoSetupScene()
    {
        EditorApplication.delayCall += OnStartupCheck;
    }

    [InitializeOnLoadMethod]
    private static void OnStartupCheck()
    {
        if (GameObject.Find("HW4_Flight_World") == null)
        {
            OpenSetupWindow();
        }
    }

    [MenuItem("VR_HW4/★ 一鍵全自動搭建 3D 遊戲世界 (免手動設定)", false, 0)]
    [MenuItem("Tools/VR_HW4 飛行作業/一鍵全自動搭建 3D 遊戲世界", false, 0)]
    [MenuItem("Window/1151VR-HW4 快速搭建精靈", false, 0)]
    [MenuItem("GameObject/★ 1151VR-HW4 一鍵自動搭建", false, 0)]
    public static void OpenSetupWindow()
    {
        AutoSetupScene win = GetWindow<AutoSetupScene>(false, "1151VR-HW4 快速搭建精靈", true);
        win.minSize = new Vector2(490, 520);
        win.maxSize = new Vector2(580, 560);
        win.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(12);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("✈ 1151VR-HW4 3D 飛行遊戲專案 (v3.7 旗艦版)", titleStyle);

        GUILayout.Space(8);
        EditorGUILayout.HelpBox(
            @"Unity 6.2 (6000.2.14f1) 全自動建置精靈 (v3.7 旗艦版)：

✔ 靈敏航空轉向：俯仰 78°/s、偏航 65°/s、橫滾 95°/s，手感敏捷極速響應！
✔ 深空宇宙星空背景：深邃太空色系 + 2000+ 閃爍星光粒子群 + 曲速空間星塵！
✔ 擴大天空與發光頂棚：高度由 25m 大幅擴至 65m，可見半透明青色能量網防護罩！
✔ 即時飛行高度計與頂棚警報：HUD 實時顯示高度，高於 50m 自動觸發紅色閃爍警報！
✔ 撞擊解體爆炸動畫：撞擊瞬間 12 個機身複合部件物理脫離四散旋轉噴發，震撼解體！
✔ 7 重零死角防卡死 1-Hit Stop：任何撞擊立即迫降停機並跳出重來詢問視窗！
✔ 3 種作業規定音效：1.引擎循環 2.撞擊障礙物 3.通關光環", MessageType.Info);

        GUILayout.Space(14);

        GUI.backgroundColor = new Color(0.15f, 0.85f, 0.45f);
        if (GUILayout.Button("【★ 點此一鍵全自動搭建 3D 遊戲世界】", GUILayout.Height(54)))
        {
            BuildCompleteGameScene();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        bool worldExists = GameObject.Find("HW4_Flight_World") != null;
        if (worldExists)
        {
            EditorGUILayout.HelpBox("✔ 3D 遊戲世界已搭建完畢！點擊下方或 Unity 頂部 ▶ Play 即可立即遊玩！", MessageType.None);
            GUILayout.Space(6);
            GUI.backgroundColor = new Color(0.2f, 0.65f, 1f);
            if (GUILayout.Button("【▶ 立即啟動遊戲 (Play)】", GUILayout.Height(40)))
            {
                EditorApplication.isPlaying = true;
            }
            GUI.backgroundColor = Color.white;
        }
    }

    public static void BuildCompleteGameScene()
    {
        // 1. 清理舊場景或世界物件
        GameObject oldWorld = GameObject.Find("HW4_Flight_World");
        if (oldWorld != null) DestroyImmediate(oldWorld);

        GameObject root = new GameObject("HW4_Flight_World");

        // 2. 建立精緻材質 (Standard / URP 相容)
        Shader pbrShader = Shader.Find("Standard");
        if (pbrShader == null) pbrShader = Shader.Find("Universal Render Pipeline/Lit");
        if (pbrShader == null) pbrShader = Shader.Find("Mobile/Diffuse");
        if (pbrShader == null) pbrShader = Shader.Find("Diffuse");

        Material aircraftHullMat = new Material(pbrShader);
        aircraftHullMat.name = "Mat_Aircraft_Hull";
        aircraftHullMat.color = new Color(0.12f, 0.16f, 0.22f); // 深合金鈦黑
        if (aircraftHullMat.HasProperty("_Metallic")) aircraftHullMat.SetFloat("_Metallic", 0.9f);
        if (aircraftHullMat.HasProperty("_Glossiness")) aircraftHullMat.SetFloat("_Glossiness", 0.75f);

        Material aircraftAccentMat = new Material(pbrShader);
        aircraftAccentMat.name = "Mat_Aircraft_Accent";
        aircraftAccentMat.color = new Color(0.02f, 0.65f, 0.88f); // 亮青藍色翼裝
        if (aircraftAccentMat.HasProperty("_Metallic")) aircraftAccentMat.SetFloat("_Metallic", 0.7f);

        Material canopyGlassMat = new Material(pbrShader);
        canopyGlassMat.name = "Mat_Canopy_Glass";
        canopyGlassMat.color = new Color(0.2f, 0.75f, 0.95f, 0.85f);
        if (canopyGlassMat.HasProperty("_Glossiness")) canopyGlassMat.SetFloat("_Glossiness", 0.95f);

        Material ringNeonMat = new Material(pbrShader);
        ringNeonMat.name = "Mat_Ring_Neon";
        ringNeonMat.color = new Color(0.05f, 0.85f, 0.95f);
        ringNeonMat.EnableKeyword("_EMISSION");
        if (ringNeonMat.HasProperty("_EmissionColor")) ringNeonMat.SetColor("_EmissionColor", new Color(0.1f, 0.7f, 0.9f) * 1.5f);

        Material ringGoldMat = new Material(pbrShader);
        ringGoldMat.name = "Mat_Ring_Gold";
        ringGoldMat.color = new Color(0.95f, 0.65f, 0.1f);
        ringGoldMat.EnableKeyword("_EMISSION");
        if (ringGoldMat.HasProperty("_EmissionColor")) ringGoldMat.SetColor("_EmissionColor", new Color(0.8f, 0.5f, 0.05f) * 1.2f);

        Material hazardRockMat = new Material(pbrShader);
        hazardRockMat.name = "Mat_Hazard_Obstacle";
        hazardRockMat.color = new Color(0.32f, 0.34f, 0.38f);

        Material hazardGlowMat = new Material(pbrShader);
        hazardGlowMat.name = "Mat_Hazard_DangerGlow";
        hazardGlowMat.color = new Color(0.95f, 0.15f, 0.15f);
        hazardGlowMat.EnableKeyword("_EMISSION");
        if (hazardGlowMat.HasProperty("_EmissionColor")) hazardGlowMat.SetColor("_EmissionColor", new Color(0.95f, 0.15f, 0.15f) * 2f);

        Material runwayMat = new Material(pbrShader);
        runwayMat.name = "Mat_Cyber_Runway";
        runwayMat.color = new Color(0.06f, 0.08f, 0.12f);

        // 半透明能量防護頂棚材質 (清楚可見，告別莫名其妙撞頂死亡)
        Material energyCeilingMat = new Material(pbrShader);
        energyCeilingMat.name = "Mat_Energy_Ceiling";
        energyCeilingMat.color = new Color(0.08f, 0.55f, 0.95f, 0.45f);
        energyCeilingMat.EnableKeyword("_EMISSION");
        if (energyCeilingMat.HasProperty("_EmissionColor")) energyCeilingMat.SetColor("_EmissionColor", new Color(0.05f, 0.5f, 0.9f) * 0.9f);

        // 3. 建立 12 部件精緻 3D 飛行器
        GameObject aircraft = new GameObject("Aircraft");
        aircraft.transform.SetParent(root.transform);
        aircraft.transform.position = new Vector3(0, 8, 0);
        aircraft.tag = "Player";

        // 視覺模型 Child 物件 (解耦旋轉，避免萬向節鎖死，解體動畫母容器)
        GameObject visualModel = new GameObject("Model_Visual");
        visualModel.transform.SetParent(aircraft.transform);
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;

        // (1) 機身 (Fuselage)
        GameObject fuselage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fuselage.name = "Fuselage";
        fuselage.transform.SetParent(visualModel.transform);
        fuselage.transform.localPosition = new Vector3(0, 0, 0);
        fuselage.transform.localScale = new Vector3(1.4f, 0.85f, 4.8f);
        fuselage.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(fuselage.GetComponent<Collider>());

        // (2) 機鼻錐體 (Nose Cone)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose_Cone";
        nose.transform.SetParent(visualModel.transform);
        nose.transform.localPosition = new Vector3(0, -0.05f, 2.5f);
        nose.transform.localScale = new Vector3(1.1f, 0.7f, 2.2f);
        nose.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(nose.GetComponent<Collider>());

        // (3) 駕駛座艙罩 (Cockpit Canopy)
        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Cockpit_Canopy";
        canopy.transform.SetParent(visualModel.transform);
        canopy.transform.localPosition = new Vector3(0, 0.52f, 0.8f);
        canopy.transform.localScale = new Vector3(0.85f, 0.65f, 2.1f);
        canopy.GetComponent<Renderer>().sharedMaterial = canopyGlassMat;
        DestroyImmediate(canopy.GetComponent<Collider>());

        // (4) 左主翼 (Main Wing Left)
        GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingL.name = "MainWing_Left";
        wingL.transform.SetParent(visualModel.transform);
        wingL.transform.localPosition = new Vector3(-2.2f, 0, -0.4f);
        wingL.transform.localRotation = Quaternion.Euler(0, -18, -3);
        wingL.transform.localScale = new Vector3(3.4f, 0.12f, 1.8f);
        wingL.GetComponent<Renderer>().sharedMaterial = aircraftAccentMat;
        DestroyImmediate(wingL.GetComponent<Collider>());

        // (5) 右主翼 (Main Wing Right)
        GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingR.name = "MainWing_Right";
        wingR.transform.SetParent(visualModel.transform);
        wingR.transform.localPosition = new Vector3(2.2f, 0, -0.4f);
        wingR.transform.localRotation = Quaternion.Euler(0, 18, 3);
        wingR.transform.localScale = new Vector3(3.4f, 0.12f, 1.8f);
        wingR.GetComponent<Renderer>().sharedMaterial = aircraftAccentMat;
        DestroyImmediate(wingR.GetComponent<Collider>());

        // (6) 左右翼梢小翼 (Winglets)
        GameObject wletL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wletL.name = "Winglet_Left";
        wletL.transform.SetParent(visualModel.transform);
        wletL.transform.localPosition = new Vector3(-3.8f, 0.45f, -0.6f);
        wletL.transform.localRotation = Quaternion.Euler(0, 0, 18);
        wletL.transform.localScale = new Vector3(0.08f, 0.9f, 0.9f);
        wletL.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(wletL.GetComponent<Collider>());

        GameObject wletR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wletR.name = "Winglet_Right";
        wletR.transform.SetParent(visualModel.transform);
        wletR.transform.localPosition = new Vector3(3.8f, 0.45f, -0.6f);
        wletR.transform.localRotation = Quaternion.Euler(0, 0, -18);
        wletR.transform.localScale = new Vector3(0.08f, 0.9f, 0.9f);
        wletR.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(wletR.GetComponent<Collider>());

        // (7) 雙渦輪噴射發動機 (Twin Jet Engines)
        GameObject engL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engL.name = "Engine_Left";
        engL.transform.SetParent(visualModel.transform);
        engL.transform.localPosition = new Vector3(-1.45f, -0.1f, -0.9f);
        engL.transform.localRotation = Quaternion.Euler(90, 0, 0);
        engL.transform.localScale = new Vector3(0.7f, 1.3f, 0.7f);
        engL.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(engL.GetComponent<Collider>());

        GameObject engR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engR.name = "Engine_Right";
        engR.transform.SetParent(visualModel.transform);
        engR.transform.localPosition = new Vector3(1.45f, -0.1f, -0.9f);
        engR.transform.localRotation = Quaternion.Euler(90, 0, 0);
        engR.transform.localScale = new Vector3(0.7f, 1.3f, 0.7f);
        engR.GetComponent<Renderer>().sharedMaterial = aircraftHullMat;
        DestroyImmediate(engR.GetComponent<Collider>());

        // (8) 發動機尾噴口發光環 (Engine Nozzles)
        Material nozzleGlowMat = new Material(pbrShader);
        nozzleGlowMat.name = "Mat_Nozzle_Glow";
        nozzleGlowMat.color = new Color(0.2f, 0.85f, 1f);
        nozzleGlowMat.EnableKeyword("_EMISSION");
        if (nozzleGlowMat.HasProperty("_EmissionColor")) nozzleGlowMat.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 1f) * 2.5f);

        GameObject nozzleL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nozzleL.name = "Nozzle_Left";
        nozzleL.transform.SetParent(visualModel.transform);
        nozzleL.transform.localPosition = new Vector3(-1.45f, -0.1f, -2.3f);
        nozzleL.transform.localRotation = Quaternion.Euler(90, 0, 0);
        nozzleL.transform.localScale = new Vector3(0.55f, 0.15f, 0.55f);
        nozzleL.GetComponent<Renderer>().sharedMaterial = nozzleGlowMat;
        DestroyImmediate(nozzleL.GetComponent<Collider>());

        GameObject nozzleR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nozzleR.name = "Nozzle_Right";
        nozzleR.transform.SetParent(visualModel.transform);
        nozzleR.transform.localPosition = new Vector3(1.45f, -0.1f, -2.3f);
        nozzleR.transform.localRotation = Quaternion.Euler(90, 0, 0);
        nozzleR.transform.localScale = new Vector3(0.55f, 0.15f, 0.55f);
        nozzleR.GetComponent<Renderer>().sharedMaterial = nozzleGlowMat;
        DestroyImmediate(nozzleR.GetComponent<Collider>());

        // (9) 雙垂直尾翼 (Twin Vertical Stabilizers)
        GameObject tailL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailL.name = "TailFin_Left";
        tailL.transform.SetParent(visualModel.transform);
        tailL.transform.localPosition = new Vector3(-1.2f, 0.8f, -1.2f);
        tailL.transform.localRotation = Quaternion.Euler(0, 0, -20);
        tailL.transform.localScale = new Vector3(0.08f, 1.5f, 1.2f);
        tailL.GetComponent<Renderer>().sharedMaterial = aircraftAccentMat;
        DestroyImmediate(tailL.GetComponent<Collider>());

        GameObject tailR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailR.name = "TailFin_Right";
        tailR.transform.SetParent(visualModel.transform);
        tailR.transform.localPosition = new Vector3(1.2f, 0.8f, -1.2f);
        tailR.transform.localRotation = Quaternion.Euler(0, 0, 20);
        tailR.transform.localScale = new Vector3(0.08f, 1.5f, 1.2f);
        tailR.GetComponent<Renderer>().sharedMaterial = aircraftAccentMat;
        DestroyImmediate(tailR.GetComponent<Collider>());

        // (10) 雙翼梢導航燈
        GameObject navL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        navL.name = "NavLight_Red";
        navL.transform.SetParent(visualModel.transform);
        navL.transform.localPosition = new Vector3(-3.3f, 0.05f, -0.6f);
        navL.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Material redGlow = new Material(pbrShader);
        redGlow.color = Color.red;
        redGlow.EnableKeyword("_EMISSION");
        if (redGlow.HasProperty("_EmissionColor")) redGlow.SetColor("_EmissionColor", Color.red * 2f);
        navL.GetComponent<Renderer>().sharedMaterial = redGlow;
        DestroyImmediate(navL.GetComponent<Collider>());

        GameObject navR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        navR.name = "NavLight_Green";
        navR.transform.SetParent(visualModel.transform);
        navR.transform.localPosition = new Vector3(3.3f, 0.05f, -0.6f);
        navR.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Material greenGlow = new Material(pbrShader);
        greenGlow.color = Color.green;
        greenGlow.EnableKeyword("_EMISSION");
        if (greenGlow.HasProperty("_EmissionColor")) greenGlow.SetColor("_EmissionColor", Color.green * 2f);
        navR.GetComponent<Renderer>().sharedMaterial = greenGlow;
        DestroyImmediate(navR.GetComponent<Collider>());

        // (11) 雙發動機後燃器火焰粒子
        CreateThrusterParticle(visualModel.transform, new Vector3(-1.45f, -0.1f, -2.4f));
        CreateThrusterParticle(visualModel.transform, new Vector3(1.45f, -0.1f, -2.4f));

        // 4. 掛載飛行器物理元件 (立體包覆全機身，長 7.2m x 寬 7.2m x 高 1.8m，杜絕穿透)
        BoxCollider airCol = aircraft.AddComponent<BoxCollider>();
        airCol.center = new Vector3(0f, 0.1f, 0.2f);
        airCol.size = new Vector3(7.2f, 1.8f, 7.0f);

        Rigidbody rb = aircraft.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 1.0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 鎖定物理打轉！

        AircraftFlightController flightCtrl = aircraft.AddComponent<AircraftFlightController>();
        flightCtrl.visualModel = visualModel.transform;
        flightCtrl.autoFlyOnStart = true;

        // 靈敏航空轉向參數直接在 Inspector 中初始化
        flightCtrl.pitchSensitivity = 78f;
        flightCtrl.yawSensitivity = 65f;
        flightCtrl.rollSensitivity = 95f;
        flightCtrl.maxPitchAngle = 55f;
        flightCtrl.maxRollBankAngle = 58f;
        flightCtrl.maxAltitudeLimit = 65f; // 天空高度由 25m 擴充至 65m
        flightCtrl.minAltitudeLimit = -4.5f;

        // 5. 建立音效管理器 (AudioManager)
        GameObject audioMgrObj = new GameObject("AudioManager");
        audioMgrObj.transform.SetParent(root.transform);
        AudioManager audioMgr = audioMgrObj.AddComponent<AudioManager>();

        AudioSource engineSource = audioMgrObj.AddComponent<AudioSource>();
        engineSource.loop = true;
        engineSource.playOnAwake = false;
        engineSource.spatialBlend = 0f;

        AudioSource sfxSource = audioMgrObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        audioMgr.engineAudioSource = engineSource;
        audioMgr.sfxAudioSource = sfxSource;

        audioMgr.engineClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/1_FlightEngine_Loop.wav");
        if (audioMgr.engineClip == null) audioMgr.engineClip = FindAudioClip("1_FlightEngine_Loop");

        audioMgr.crashClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/2_ObstacleCrash.wav");
        if (audioMgr.crashClip == null) audioMgr.crashClip = FindAudioClip("2_ObstacleCrash");

        audioMgr.passRingClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/3_RingPass_Success.wav");
        if (audioMgr.passRingClip == null) audioMgr.passRingClip = FindAudioClip("3_RingPass_Success");

        flightCtrl.audioManager = audioMgr;

        // 6. 建立【無限隨機地圖生成器】 (InfiniteMapSpawner)
        GameObject spawnerObj = new GameObject("Infinite_Map_Spawner");
        spawnerObj.transform.SetParent(root.transform);
        InfiniteMapSpawner mapSpawner = spawnerObj.AddComponent<InfiniteMapSpawner>();
        mapSpawner.playerAircraft = aircraft.transform;
        mapSpawner.ringNeonMat = ringNeonMat;
        mapSpawner.ringGoldMat = ringGoldMat;
        mapSpawner.hazardRockMat = hazardRockMat;
        mapSpawner.hazardGlowMat = hazardGlowMat;
        mapSpawner.runwayMat = runwayMat;
        mapSpawner.energyCeilingMat = energyCeilingMat;

        // 7. 深空宇宙環境配置 (Cosmic Deep-Space Environment)
        CreateCosmicSpaceEnvironment(root.transform, aircraft.transform);

        // 8. 方向光 (深空科技冷暖交織光影)
#if UNITY_2023_1_OR_NEWER
        Light existingLight = Object.FindFirstObjectByType<Light>();
#else
        Light existingLight = FindObjectOfType<Light>();
#endif
        if (existingLight == null)
        {
            GameObject lightObj = new GameObject("Cosmic_DirectionalLight");
            lightObj.transform.SetParent(root.transform);
            Light sun = lightObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.85f, 0.92f, 1.0f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50, -35, 0);
        }

        // 9. 建立 GameManager (內建開始選單、HUD 儀表板、即時高度顯示與結算畫面)
        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(root.transform);
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.aircraft = flightCtrl;
        gm.autoStart = true;
        gm.currentState = GameState.Playing;

        // 10. 設定 Main Camera (深空星空 SolidColor 清除)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.015f, 0.02f, 0.06f, 1.0f); // 深空宇宙藍黑背景

        CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
        if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
        camFollow.target = aircraft.transform;
        mainCam.transform.position = aircraft.transform.position + new Vector3(0, 3.8f, -9f);

        // 11. 自動儲存場景至 Assets/Scenes/FlightGameScene.unity
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/FlightGameScene.unity");

        Selection.activeGameObject = aircraft;

        EditorUtility.DisplayDialog("搭建完成！",
            @"1151VR-HW4 3D 遊戲世界已在 Unity 6.2 (6000.2.14f1) 中全自動生成完畢！(v3.7 旗艦版)

✔ 靈敏航空轉向：大幅提升轉向反應度與偏航橫滾靈活性！
✔ 深空星空宇宙背景：深邃太空色系 + 2000+ 星光粒子 + 曲速星塵！
✔ 擴大飛行高度空間 (65m) + 半透明發光能量頂棚：告別看不見天花板莫名死亡！
✔ HUD 飛行高度即時儀表：隨時顯示飛行米數，接近頂棚 (>50m) 自動閃爍紅色警報！
✔ 零件物理噴散解體動畫：撞擊障礙物瞬間 12 個部件四散旋轉爆裂！
✔ 7 重零死角防卡死 1-Hit Stop：任何撞擊立即迫降停機並跳出重來詢問視窗！
✔ 3 種作業規定音效已全自動連結完畢
✔ 場景已自動儲存至 Assets/Scenes/FlightGameScene.unity

請直接點擊 Unity 正上方中間的 ▶ (Play) 按鈕享受敏捷暢快的太空穿梭！", "太棒了，立即遊玩！");
    }

    // 建立深空星空宇宙環境 (2000+ 閃爍星光 + 曲速星塵)
    private static void CreateCosmicSpaceEnvironment(Transform parent, Transform targetAircraft)
    {
        // 1. 環境光設置為深藍冷色調
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.16f, 0.28f);

        // 2. 建立宇宙星光粒子群 (Cosmic Starfield)
        GameObject starObj = new GameObject("Cosmic_Starfield");
        starObj.transform.SetParent(parent);
        starObj.transform.position = Vector3.zero;

        ParticleSystem ps = starObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 15f;
        main.startSpeed = 0f;
        main.startSize = 0.45f;
        main.maxParticles = 2500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.85f, 1f, 0.9f),
            new Color(1f, 0.95f, 0.8f, 0.9f)
        );

        var emission = ps.emission;
        emission.rateOverTime = 250;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 280f;

        ParticleSystemRenderer psRenderer = starObj.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            Material starMat = CreateUnlitParticleMaterial("Mat_Cosmic_Stars", new Color(1f, 1f, 1f, 0.95f));
            psRenderer.sharedMaterial = starMat;
        }

        // 3. 建立高速曲速星塵 (Warp Dust)
        GameObject warpObj = new GameObject("Warp_Speed_Dust");
        warpObj.transform.SetParent(targetAircraft);
        warpObj.transform.localPosition = new Vector3(0, 0, 45f);
        warpObj.transform.localRotation = Quaternion.Euler(0, 180, 0);

        ParticleSystem warpPs = warpObj.AddComponent<ParticleSystem>();
        var warpMain = warpPs.main;
        warpMain.loop = true;
        warpMain.startLifetime = 2.0f;
        warpMain.startSpeed = 45f;
        warpMain.startSize = 0.2f;
        warpMain.maxParticles = 400;
        warpMain.simulationSpace = ParticleSystemSimulationSpace.World;
        warpMain.startColor = new Color(0.4f, 0.75f, 1f, 0.65f);

        var warpEmission = warpPs.emission;
        warpEmission.rateOverTime = 80;

        var warpShape = warpPs.shape;
        warpShape.shapeType = ParticleSystemShapeType.Box;
        warpShape.scale = new Vector3(45f, 30f, 10f);

        ParticleSystemRenderer warpRenderer = warpObj.GetComponent<ParticleSystemRenderer>();
        if (warpRenderer != null)
        {
            Material dustMat = CreateUnlitParticleMaterial("Mat_Warp_Dust", new Color(0.35f, 0.75f, 1f, 0.6f));
            warpRenderer.sharedMaterial = dustMat;
        }
    }

    private static Material CreateUnlitParticleMaterial(string matName, Color color)
    {
        Shader pShader = Shader.Find("Particles/Standard Unlit");
        if (pShader == null) pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (pShader == null) pShader = Shader.Find("Mobile/Particles/Additive");
        if (pShader == null) pShader = Shader.Find("Sprites/Default");
        if (pShader == null) pShader = Shader.Find("Unlit/Color");
        if (pShader == null) pShader = Shader.Find("Standard");

        Material pMat = new Material(pShader);
        pMat.name = matName;
        pMat.color = color;
        if (pMat.HasProperty("_TintColor")) pMat.SetColor("_TintColor", color);
        if (pMat.HasProperty("_EmissionColor")) pMat.SetColor("_EmissionColor", color * 1.5f);
        pMat.EnableKeyword("_EMISSION");
        return pMat;
    }

    private static void CreateThrusterParticle(Transform parent, Vector3 localPos)
    {
        GameObject psObj = new GameObject("Thruster_Particle");
        psObj.transform.SetParent(parent);
        psObj.transform.localPosition = localPos;
        psObj.transform.localRotation = Quaternion.Euler(0, 180, 0);

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 0.85f, 1f, 0.9f);
        main.startSize = 0.5f;
        main.startLifetime = 0.35f;
        main.startSpeed = 8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 40;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.2f;

        // 解決 Unity 6 / URP / Built-in 粒子遺失材質變洋紅粉紫方塊 (Magenta Pink Squares)
        ParticleSystemRenderer psRenderer = psObj.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            Material pMat = CreateUnlitParticleMaterial("Mat_Thruster_Particle", new Color(0.15f, 0.85f, 1.0f, 0.85f));
            psRenderer.sharedMaterial = pMat;
        }
    }

    private static AudioClip FindAudioClip(string baseName)
    {
        string[] guids = AssetDatabase.FindAssets(baseName + " t:AudioClip");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return null;
    }
}
#endif

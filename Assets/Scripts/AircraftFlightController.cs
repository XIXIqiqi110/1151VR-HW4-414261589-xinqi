using UnityEngine;

// 1151VR-HW4: 專業級 3D 飛行器控制器 (靈敏航空轉向、零件物理解體爆炸動畫、擴大 65m 空域、零死角 1-Hit Stop)
[RequireComponent(typeof(Rigidbody))]
public class AircraftFlightController : MonoBehaviour
{
    [Header("飛行航速設定")]
    public float normalSpeed = 38f;
    public float boostSpeed = 65f;
    public float brakeSpeed = 20f;
    public float accelerationRate = 4.5f;
    public bool autoFlyOnStart = true;

    [Header("轉向靈敏度 (航空俯仰與偏航，手感敏捷極速響應)")]
    public float pitchSensitivity = 78f;
    public float yawSensitivity = 65f;
    public float rollSensitivity = 95f;
    public float maxPitchAngle = 55f;
    public float maxRollBankAngle = 58f;

    [Header("賽道邊界限制 (高度擴展至 65m，撞擊天花板/地面/出界立即迫降)")]
    public float maxHorizontalDeviation = 38f; // 左右極限邊界 (超出墜毀)
    public float warningDeviation = 22f;        // 偏離警告門檻
    public float minAltitudeLimit = -4.5f;      // 地面跑道高度限制
    public float maxAltitudeLimit = 65f;        // 頂部發光能量頂棚限制 (大幅提升至 65m！)
    public float warningAltitude = 50f;         // 接近天花板警報門檻

    [HideInInspector] public bool isNearBoundary = false;
    [HideInInspector] public float remainingBoundaryDist = 38f;

    // 提供 HUD 讀取的即時高度與接近頂棚狀態
    public float currentAltitude => transform.position.y;
    public float maxAltitude => maxAltitudeLimit;
    public bool isNearCeiling => transform.position.y >= warningAltitude;

    [Header("模型子物件與特效")]
    public Transform visualModel;
    public AudioManager audioManager;

    private Rigidbody rb;
    private Collider[] aircraftColliders;
    private float currentSpeed = 38f;
    private float currentPitch = 0f;
    private float currentYaw = 0f;
    private float currentRoll = 0f;
    private bool isFlying = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        aircraftColliders = GetComponentsInChildren<Collider>();
        rb.useGravity = false;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 1.0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 鎖定物理打轉！

        currentSpeed = normalSpeed;
        currentYaw = transform.eulerAngles.y;

        if (visualModel == null)
        {
            Transform vm = transform.Find("Model_Visual");
            if (vm != null) visualModel = vm;
        }

#if UNITY_2023_1_OR_NEWER
        if (audioManager == null) audioManager = Object.FindFirstObjectByType<AudioManager>();
#else
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
#endif

        if (autoFlyOnStart)
        {
            StartFlying();
        }
    }

    public void StartFlying()
    {
        if (isDead) return;
        isFlying = true;
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        if (currentSpeed < brakeSpeed) currentSpeed = normalSpeed;
        if (audioManager != null) audioManager.StartEngine();
    }

    void Update()
    {
        // 尚未起飛時防呆保障：按下任一鍵或滑鼠立即開飛！
        if (!isFlying && !isDead)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || autoFlyOnStart)
            {
                StartFlying();
            }
            return;
        }

        if (isDead) return;

        // 1. 讀取鍵盤輸入 (同時相容 Axis 與直接按鍵偵測)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;

        bool boost = Input.GetKey(KeyCode.Space);
        bool brake = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 2. 速度平滑調變
        float targetSpeed = normalSpeed;
        if (boost) targetSpeed = boostSpeed;
        else if (brake) targetSpeed = brakeSpeed;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * accelerationRate);

        // 3. 引擎音效隨推力調變
        if (audioManager != null)
        {
            float speedRatio = currentSpeed / normalSpeed;
            audioManager.UpdateEnginePitch(speedRatio);
        }

        // 4. 飛機姿態與敏捷轉向計算 (Bank-to-turn 靈活航空手感)
        currentPitch = Mathf.Clamp(currentPitch - v * pitchSensitivity * Time.deltaTime, -maxPitchAngle, maxPitchAngle);
        currentYaw += h * yawSensitivity * Time.deltaTime;

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // 5. 視覺模型獨立快速橫滾傾斜 (Banking)
        float targetRoll = -h * maxRollBankAngle;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * 10f); // 提升至 10f 敏捷度
        if (visualModel != null)
        {
            visualModel.localRotation = Quaternion.Euler(0f, 0f, currentRoll);
        }

        // 6. 零死角全立體防撞感應 (Fail-Safe 碰撞雷達，徹底杜絕物理卡死在柱子上的問題)
        Collider[] overlaps = Physics.OverlapSphere(transform.position, 2.8f);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider col = overlaps[i];
            if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
            if (col.GetComponent<RingCheckpoint>() != null) continue;

            HandleCollisionImpact(col.gameObject, "💥 撞擊立體障礙物！機體解體迫降墜毀！");
            return;
        }

        // 7. 正前方射線防禦 (SphereCast 防高速穿透)
        RaycastHit forwardHit;
        if (Physics.SphereCast(transform.position, 1.2f, transform.forward, out forwardHit, 2.2f))
        {
            if (forwardHit.collider.gameObject != gameObject && !forwardHit.collider.transform.IsChildOf(transform))
            {
                if (forwardHit.collider.GetComponent<RingCheckpoint>() == null)
                {
                    HandleCollisionImpact(forwardHit.collider.gameObject, "💥 撞擊立體障礙物！機體解體迫降墜毀！");
                    return;
                }
            }
        }

        // 8. 撞擊天花板、地面/地板或飛出場外立即墜毀判定
        float devX = Mathf.Abs(transform.position.x);
        if (devX > warningDeviation)
        {
            isNearBoundary = true;
            remainingBoundaryDist = Mathf.Max(0f, maxHorizontalDeviation - devX);
        }
        else
        {
            isNearBoundary = false;
        }

        if (devX >= maxHorizontalDeviation)
        {
            Die("🚨 飛出場外墜毀！脫離合法航道防護力場邊界！");
            return;
        }
        else if (transform.position.y <= minAltitudeLimit)
        {
            Die("💥 撞擊地面/地板墜毀！高度過低迫降失敗！");
            return;
        }
        else if (transform.position.y >= maxAltitudeLimit)
        {
            Die("⚡ 撞擊頂部能量網墜毀！超出防空空域上限 (65m)！");
            return;
        }
    }

    void FixedUpdate()
    {
        if (!isFlying || isDead) return;

        // 由 PhysX 物理引擎主導速度與位移，杜絕 Transform 穿透與卡死
        Vector3 targetVel = transform.forward * currentSpeed;
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = targetVel;
#else
        rb.velocity = targetVel;
#endif
        rb.MovePosition(rb.position + transform.forward * (currentSpeed * Time.fixedDeltaTime));
    }

    // 碰撞感應四重奏：Enter 與 Stay 雙重把關
    void OnCollisionEnter(Collision collision) { HandleCollisionImpact(collision.gameObject); }
    void OnCollisionStay(Collision collision) { HandleCollisionImpact(collision.gameObject); }
    void OnTriggerEnter(Collider other) { HandleCollisionImpact(other.gameObject); }
    void OnTriggerStay(Collider other) { HandleCollisionImpact(other.gameObject); }

    void HandleCollisionImpact(GameObject hitObj, string fallbackReason = "💥 撞擊立體障礙物！機體解體迫降墜毀！")
    {
        if (isDead) return;
        if (hitObj == gameObject || hitObj.transform.IsChildOf(transform)) return;
        if (hitObj.GetComponent<RingCheckpoint>() != null) return;

        string n = hitObj.name.ToLower();
        if (n.Contains("runway") || n.Contains("floor") || n.Contains("ground"))
        {
            Die("💥 撞擊地面/地板墜毀！高度過低迫降失敗！");
        }
        else if (n.Contains("ceiling") || n.Contains("roof"))
        {
            Die("⚡ 撞擊頂部發光能量網墜毀！超出防空空域上限 (65m)！");
        }
        else
        {
            Die(fallbackReason);
        }
    }

    // 撞擊障礙物瞬間緊急停止並觸發震撼零件解體爆炸動畫 (Disassembly Breakup)
    public void Die(string reason = "💥 撞擊立體障礙物！機體解體迫降墜毀！")
    {
        if (isDead) return;
        isDead = true;
        isFlying = false;

        // 1. 徹底停止物理與前進！
        currentSpeed = 0f;
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#endif
            rb.isKinematic = true;
        }

        // 2. 關閉主機身碰撞體，避免後續微小物理推擠
        if (aircraftColliders != null)
        {
            foreach (var col in aircraftColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        // 3. 觸發震撼的 12 部件物理噴散解體動畫！
        TriggerDisassembly();

        // 4. 播放撞擊音效與停機
        if (audioManager != null)
        {
            audioManager.PlayCrashSound();
            audioManager.StopEngine();
        }

        // 5. 鏡頭強烈震動衝擊感
#if UNITY_2023_1_OR_NEWER
        CameraFollow cam = Object.FindFirstObjectByType<CameraFollow>();
#else
        CameraFollow cam = FindObjectOfType<CameraFollow>();
#endif
        if (cam != null) cam.TriggerShake(3.0f);

        // 6. 通知 GameManager 彈出結算與重來詢問彈窗 (微幅延遲 0.35 秒讓玩家欣賞零件噴散解體)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InvokeGameOver(reason, 0.35f);
        }
    }

    // 零件物理噴散解體動畫 (12 部件四散旋轉爆裂)
    private void TriggerDisassembly()
    {
        if (visualModel == null) return;

        // 停止發動機噴火粒子
        ParticleSystem[] particles = visualModel.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
        {
            if (p != null) p.Stop();
        }

        // 取得所有子零件 (Fuselage, Wings, Engines, Canopy, Fins, Nozzles...)
        Transform[] pieces = visualModel.GetComponentsInChildren<Transform>();
        for (int i = 0; i < pieces.Length; i++)
        {
            Transform piece = pieces[i];
            if (piece == null || piece == visualModel || piece == transform) continue;

            // 脫離母物件，獨立於世界座標
            piece.SetParent(null);

            // 加入碰撞體與剛體，賦予爆裂物理衝量
            BoxCollider col = piece.gameObject.GetComponent<BoxCollider>();
            if (col == null) col = piece.gameObject.AddComponent<BoxCollider>();
            col.enabled = true;

            Rigidbody prb = piece.gameObject.GetComponent<Rigidbody>();
            if (prb == null) prb = piece.gameObject.AddComponent<Rigidbody>();
            prb.mass = 1.8f;
            prb.useGravity = true;
            prb.linearDamping = 0.1f;
            prb.angularDamping = 0.5f;

            // 朝隨機四方 + 機首慣性 + 向上拋射
            Vector3 scatterDir = (Random.insideUnitSphere * 18f) + (transform.forward * 8f) + (Vector3.up * 6f);
#if UNITY_6000_0_OR_NEWER
            prb.linearVelocity = scatterDir;
            prb.angularVelocity = Random.insideUnitSphere * 22f;
#else
            prb.velocity = scatterDir;
            prb.angularVelocity = Random.insideUnitSphere * 22f;
#endif

            // 10 秒後自動回收殘骸
            Destroy(piece.gameObject, 10f);
        }

        // 生成爆炸衝擊波粒子
        SpawnCrashShockwave(transform.position);
    }

    private void SpawnCrashShockwave(Vector3 pos)
    {
        GameObject expObj = new GameObject("Crash_Shockwave_FX");
        expObj.transform.position = pos;

        ParticleSystem ps = expObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 16f;
        main.startSize = 1.2f;
        main.maxParticles = 80;
        main.startColor = new Color(1.0f, 0.65f, 0.2f, 0.95f);

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1.5f;

        ParticleSystemRenderer rend = expObj.GetComponent<ParticleSystemRenderer>();
        if (rend != null)
        {
            Shader s = Shader.Find("Particles/Standard Unlit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (s == null) s = Shader.Find("Sprites/Default");
            if (s != null)
            {
                Material m = new Material(s);
                m.color = new Color(1f, 0.5f, 0.1f);
                rend.sharedMaterial = m;
            }
        }

        Destroy(expObj, 2.5f);
    }
}
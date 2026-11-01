using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    StartMenu,
    Playing,
    GameOver
}

// 1151VR-HW4: 遊戲狀態、開始選單、HUD 儀表板 (含高度計與天花板閃爍警報) 與結算介面 (Unity 6 原生 OnGUI，零外部字型依賴，保證絕不報錯)
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("遊戲狀態")]
    public GameState currentState = GameState.Playing;
    public bool autoStart = true; // 預設即刻巡航開飛，零等待！

    [Header("數值追蹤")]
    public int score = 0;
    public int ringsPassed = 0;
    public float distanceTraveled = 0f;
    public int combo = 0;

    public AircraftFlightController aircraft;
    private float startBannerTimer = 5.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (aircraft == null)
        {
#if UNITY_2023_1_OR_NEWER
            aircraft = Object.FindFirstObjectByType<AircraftFlightController>();
#else
            aircraft = FindObjectOfType<AircraftFlightController>();
#endif
        }

        if (autoStart)
        {
            StartGame();
        }
        else
        {
            currentState = GameState.StartMenu;
        }
    }

    void Update()
    {
        if (startBannerTimer > 0f)
        {
            startBannerTimer -= Time.deltaTime;
        }

        if (currentState == GameState.StartMenu)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
        }
        else if (currentState == GameState.Playing)
        {
            if (aircraft != null)
            {
                distanceTraveled = Mathf.Max(0f, aircraft.transform.position.z);
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                currentState = GameState.StartMenu;
            }
        }
        else if (currentState == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMenu();
            }
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        if (aircraft != null)
        {
            aircraft.StartFlying();
        }
    }

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing) return;
        combo++;
        int earned = amount * Mathf.Max(1, combo);
        score += earned;
        ringsPassed++;
    }

    [Header("結算原因")]
    public string lastCrashReason = "💥 撞擊立體障礙物！機體解體迫降停機！";

    public void InvokeGameOver(string reason, float delay)
    {
        lastCrashReason = reason;
        if (aircraft != null)
        {
            distanceTraveled = Mathf.Max(0f, aircraft.transform.position.z);
        }
        Invoke(nameof(ShowGameOverModal), delay);
    }

    private void ShowGameOverModal()
    {
        currentState = GameState.GameOver;
    }

    public void GameOver(string reason = "💥 撞擊立體障礙物！機體解體迫降停機！")
    {
        if (currentState == GameState.GameOver) return;
        lastCrashReason = reason;
        currentState = GameState.GameOver;

        if (aircraft != null)
        {
            distanceTraveled = Mathf.Max(0f, aircraft.transform.position.z);
            aircraft.Die(reason);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Unity 原生 OnGUI：高畫質 Cyber 介面 (開始畫面、HUD 儀表板、GameOver 結算)
    void OnGUI()
    {
        GUI.depth = -100;
        GUI.skin.label.fontSize = 17;
        GUI.skin.label.fontStyle = FontStyle.Bold;

        if (currentState == GameState.StartMenu)
        {
            DrawStartMenu();
        }
        else if (currentState == GameState.Playing)
        {
            DrawPlayingHUD();
        }
        else if (currentState == GameState.GameOver)
        {
            DrawGameOverModal();
        }
    }

    void DrawStartMenu()
    {
        float w = 550;
        float h = 450;
        Rect boxRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

        GUI.color = Color.white;
        GUI.Box(boxRect, "═══ ★ 1151VR-HW4 3D 飛行穿梭障礙物遊戲 ★ ═══");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUI.color = Color.cyan;
        GUI.Label(new Rect(boxRect.x, boxRect.y + 35, w, 32), "✈ AERO STRIKE: COSMIC FLIGHT (v3.7)", titleStyle);

        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 13;
        subStyle.alignment = TextAnchor.MiddleCenter;
        GUI.color = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(boxRect.x, boxRect.y + 70, w, 22), "Unity 6.2 · 靈敏轉向 · 深空星空 · 65m 頂棚 · 零件解體爆裂", subStyle);

        GUI.color = Color.white;
        Rect infoRect = new Rect(boxRect.x + 35, boxRect.y + 105, w - 70, 215);
        GUI.Box(infoRect, "【 操 控 指 南 & 規 則 】");

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = new Color(0.9f, 0.95f, 1f);

        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 30, infoRect.width - 40, 24), "• 【W / S】 或 【↑ / ↓】 ：極速靈敏俯仰 (俯衝 / 爬升)", infoStyle);
        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 58, infoRect.width - 40, 24), "• 【A / D】 或 【← / →】 ：真實傾斜轉向 (Bank-to-turn 敏捷偏航)", infoStyle);
        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 86, infoRect.width - 40, 24), "• 【Space 空白鍵】 ：後燃器火箭推進加速", infoStyle);
        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 114, infoRect.width - 40, 24), "• 【Shift 鍵】 ：氣動煞車減速", infoStyle);

        GUIStyle ruleStyle = new GUIStyle(GUI.skin.label);
        ruleStyle.fontSize = 14;
        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 145, infoRect.width - 40, 24), "★ 挑戰規則：穿過光環加分與連擊！天空空間擴展至 65m", ruleStyle);
        GUI.Label(new Rect(infoRect.x + 20, infoRect.y + 170, infoRect.width - 40, 24), "★ 注意：撞擊障礙物或頂棚將觸發零件物理噴散解體！", ruleStyle);

        GUI.color = new Color(0.15f, 0.88f, 0.45f);
        if (GUI.Button(new Rect(boxRect.x + 80, boxRect.y + 345, w - 160, 56), "【 ▶ 開始飛行 (按任一鍵或點擊) 】"))
        {
            StartGame();
        }
    }

    void DrawPlayingHUD()
    {
        GUI.color = Color.green;
        GUI.Label(new Rect(30, 20, 220, 30), "狀態: ● 太空巡航中");

        // ★ 核心新功能：實時飛行高度儀表 ★
        float alt = aircraft != null ? aircraft.currentAltitude : 0f;
        float maxAlt = aircraft != null ? aircraft.maxAltitude : 65f;
        GUI.color = (aircraft != null && aircraft.isNearCeiling) ? Color.red : new Color(0.2f, 0.9f, 1f);
        GUI.Label(new Rect(30, 52, 260, 30), $"飛行高度: {alt:F0} m / {maxAlt:F0} m");

        GUI.color = Color.yellow;
        string comboStr = combo > 1 ? $" (連擊 x{combo})!" : "";
        GUI.Label(new Rect(Screen.width / 2f - 130, 20, 280, 30), $"得分: {score}{comboStr}");

        GUI.color = Color.cyan;
        GUI.Label(new Rect(Screen.width - 240, 20, 220, 30), $"穿過光環: {ringsPassed} 個");

        GUI.color = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(Screen.width - 240, 52, 220, 30), $"飛行航程: {(int)distanceTraveled} m");

        // ★ 核心新功能：接近頂部發光能量網警報 (紅黃交替閃爍) ★
        if (aircraft != null && aircraft.isNearCeiling)
        {
            float cw = 580;
            float ch = 38;
            Rect cRect = new Rect((Screen.width - cw) / 2f, 20, cw, ch);
            bool blink = ((int)(Time.time * 4f) % 2) == 0;
            GUI.color = blink ? new Color(0.95f, 0.15f, 0.15f, 0.95f) : new Color(0.95f, 0.65f, 0.05f, 0.95f);
            GUI.Box(cRect, "");

            GUIStyle cWarnStyle = new GUIStyle(GUI.skin.label);
            cWarnStyle.fontSize = 15;
            cWarnStyle.fontStyle = FontStyle.Bold;
            cWarnStyle.alignment = TextAnchor.MiddleCenter;
            cWarnStyle.normal.textColor = Color.white;
            GUI.Label(cRect, $"⚡【頂部能量網警報】高度過高 ({alt:F0}m)！即將觸頂！請速俯衝下潛 [按 W / ↓]！", cWarnStyle);
        }

        // 邊界偏離警報 (限制不可飛出場外太多)
        if (aircraft != null && aircraft.isNearBoundary)
        {
            float bw = 540;
            float bh = 36;
            Rect bRect = new Rect((Screen.width - bw) / 2f, 65, bw, bh);
            GUI.color = new Color(0.85f, 0.1f, 0.1f, 0.9f);
            GUI.Box(bRect, "");
            GUIStyle warnStyle = new GUIStyle(GUI.skin.label);
            warnStyle.fontSize = 14;
            warnStyle.alignment = TextAnchor.MiddleCenter;
            warnStyle.normal.textColor = Color.yellow;
            GUI.Label(bRect, $"⚠️【偏離航道警報】請速向中央折返！距邊界剩 {(int)aircraft.remainingBoundaryDist}m，超出直接墜毀！", warnStyle);
        }

        // 【生效按鍵即時提示列】(提示玩家有哪些鍵有效果，實時高亮反饋)
        float kw = Mathf.Min(Screen.width - 40, 980);
        float kh = 34;
        Rect kRect = new Rect((Screen.width - kw) / 2f, Screen.height - 48, kw, kh);
        GUI.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
        GUI.Box(kRect, "");

        bool isW = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool isS = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool isA = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool isD = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        bool isSpace = Input.GetKey(KeyCode.Space);
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        string wStr = isW ? "【W 俯衝●作用中】" : "[W/↑]俯衝";
        string sStr = isS ? "【S 爬升●作用中】" : "[S/↓]爬升";
        string aStr = isA ? "【A 左滾●作用中】" : "[A/←]左滾";
        string dStr = isD ? "【D 右滾●作用中】" : "[D/→]右滾";
        string spStr = isSpace ? "【Space 加速●噴射中】" : "[Space]加速";
        string shStr = isShift ? "【Shift 煞車●減速中】" : "[Shift]減速";

        GUIStyle keyStyle = new GUIStyle(GUI.skin.label);
        keyStyle.fontSize = 12;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = (isW || isS || isA || isD || isSpace || isShift) ? Color.yellow : new Color(0.75f, 0.9f, 1f);
        GUI.Label(kRect, $"✈ 生效按鍵提示: {wStr} | {sStr} | {aStr} | {dStr} | {spStr} | {shStr} | [R]重新起飛", keyStyle);
    }

    void DrawGameOverModal()
    {
        // 1. 全螢幕半透明深色遮罩，凝聚視覺焦點
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        // 2. 中央結算與詢問卡片
        float w = 560;
        float h = 430;
        Rect boxRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

        GUI.color = Color.white;
        GUI.Box(boxRect, "═══ 💥 飛行任務迫降終止 · 機體解體結算 ═══");

        GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
        alertStyle.fontSize = 17;
        alertStyle.fontStyle = FontStyle.Bold;
        alertStyle.alignment = TextAnchor.MiddleCenter;
        GUI.color = new Color(1f, 0.35f, 0.35f);
        GUI.Label(new Rect(boxRect.x, boxRect.y + 36, w, 32), lastCrashReason, alertStyle);

        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 15;

        GUI.color = Color.yellow;
        GUI.Label(new Rect(boxRect.x + 50, boxRect.y + 78, w - 100, 26), $"★ 最終任務得分: {score} 分", statStyle);

        GUI.color = Color.cyan;
        GUI.Label(new Rect(boxRect.x + 50, boxRect.y + 106, w - 100, 26), $"★ 順利穿過光環: {ringsPassed} 個", statStyle);

        GUI.color = new Color(0.85f, 0.9f, 1f);
        GUI.Label(new Rect(boxRect.x + 50, boxRect.y + 134, w - 100, 26), $"★ 累計飛行航程: {(int)distanceTraveled} 公尺", statStyle);

        // ★ 核心要求：超醒目卡片，明確詢問玩家是否要重來 ★
        Rect promptRect = new Rect(boxRect.x + 30, boxRect.y + 172, w - 60, 90);
        GUI.color = new Color(0.95f, 0.75f, 0.1f, 0.95f);
        GUI.Box(promptRect, "");

        GUIStyle qStyle = new GUIStyle(GUI.skin.label);
        qStyle.fontSize = 18;
        qStyle.fontStyle = FontStyle.Bold;
        qStyle.alignment = TextAnchor.MiddleCenter;
        qStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(promptRect.x, promptRect.y + 12, promptRect.width, 30), "⚠️ 飛行任務結束，是否要重新開始？", qStyle);

        GUIStyle hStyle = new GUIStyle(GUI.skin.label);
        hStyle.fontSize = 13;
        hStyle.alignment = TextAnchor.MiddleCenter;
        hStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(promptRect.x, promptRect.y + 48, promptRect.width, 26), "【按 R 或 Space 空白鍵 或 Enter 鍵立即重新起飛】·【按 M 或 Esc 回主選單】", hStyle);

        GUI.color = new Color(0.15f, 0.88f, 0.45f);
        if (GUI.Button(new Rect(boxRect.x + 30, boxRect.y + 285, 240, 64), "【 🔄 重新開始 (按 R / Space / Enter) 】"))
        {
            RestartGame();
        }

        GUI.color = new Color(0.85f, 0.85f, 0.9f);
        if (GUI.Button(new Rect(boxRect.x + 290, boxRect.y + 285, 240, 64), "【 🏠 回到開始畫面 (按 M / Esc) 】"))
        {
            ReturnToMenu();
        }
    }
}
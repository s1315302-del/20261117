using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed partial class HerbDrawerGame : MonoBehaviour
{
    private const int Columns = 6;
    private const int HerbsPerCustomerStep = 2;
    private const float ChallengeSeconds = 90;
    private const float DrawerOpenSeconds = 2;
    private const float DirectionRepeatDelay = .23f;
    private const float DirectionRepeatInterval = .09f;
    private static readonly string[] HerbNames =
    {
        "枸杞", "人參", "當歸", "黃耆", "甘草", "陳皮",
        "紅棗", "白朮", "茯苓", "川芎", "白芍", "熟地",
        "桂枝", "桔梗", "杜仲", "薏仁", "麥冬", "丹參"
    };

    private enum Phase { Ready, Search, BetweenOrders, Won, Lost }

    private readonly string[] drawers = new string[HerbNames.Length];
    private readonly string[] order = new string[CustomerCount * HerbsPerCustomerStep];
    private readonly bool[] collected = new bool[HerbNames.Length];
    private Phase phase;
    private int cursor;
    private int filled;
    private int orderSize;
    private int opened = -1;
    private float revealUntil;
    private float pullStartedAt;
    private float finishPullAt;
    private float deadline;
    private int heldDirection;
    private float nextDirectionMove;
    private string feedback;
    private Font font;
    private GUIStyle label;
    private Texture2D wood;
    private int served;
    private AudioSource horn;
    private bool ChallengeRunning => phase == Phase.Search || phase == Phase.BetweenOrders;

    private static readonly Color Cream = new Color32(247, 229, 186, 255);
    private static readonly Color Gold = new Color32(234, 175, 86, 255);
    private static readonly Color Muted = new Color32(210, 171, 140, 255);
    private static readonly Color Ink = new Color32(47, 23, 18, 255);
    private static readonly Color Green = new Color32(157, 196, 139, 255);
    private static readonly Rect LeftControl = new Rect(151, 677, 220, 57);
    private static readonly Rect HornControl = new Rect(384, 677, 256, 57);
    private static readonly Rect RightControl = new Rect(653, 677, 220, 57);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
            new GameObject("中藥牆百草尋藥").AddComponent<HerbDrawerGame>();
    }

    private void Awake()
    {
        Array.Copy(HerbNames, drawers, drawers.Length);
        font = Font.CreateDynamicFontFromOSFont(
            new[] { "Microsoft JhengHei", "Noto Sans CJK TC", "PingFang TC", "Arial Unicode MS" }, 28);
        wood = CreateWood();
        BuildShop();
        horn = gameObject.AddComponent<AudioSource>();
        const int sampleRate = 22050;
        var samples = new float[sampleRate / 6];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(Mathf.PI * i / samples.Length);
            samples[i] = envelope * (0.17f * Mathf.Sin(t * 2 * Mathf.PI * 350) +
                                     0.09f * Mathf.Sin(t * 2 * Mathf.PI * 440));
        }
        horn.clip = AudioClip.Create("Horn", samples.Length, 1, sampleRate, false);
        horn.clip.SetData(samples, 0);
        horn.playOnAwake = false;
    }

    private void OnDestroy()
    {
        Destroy(wood);
        ReleaseShop();
        Destroy(font);
        if (horn != null)
            Destroy(horn.clip);
    }

    private void Update()
    {
        bool expired = AdvanceClock();
        UpdateShop();
        if (expired)
            return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.tabKey.wasPressedThisFrame)
                FlipView();
            int direction = (keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.leftArrowKey.isPressed ? 1 : 0);
            UpdateDirection(direction);
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                UseHorn();
        }

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;
        GetCanvas(out Vector2 origin, out float scale);
        if (scale <= 0)
            return;
        Vector2 pointer = mouse.position.ReadValue();
        pointer.y = Screen.height - pointer.y;
        pointer = (pointer - origin) / scale;
        if (new Rect(820, 16, 186, 40).Contains(pointer))
            FlipView();
        else if (LeftControl.Contains(pointer))
            Move(-1);
        else if (RightControl.Contains(pointer))
            Move(1);
        else if (HornControl.Contains(pointer))
            UseHorn();
    }

    private void UpdateDirection(int direction)
    {
        if (phase != Phase.Search || direction == 0)
        {
            heldDirection = 0;
            return;
        }

        float now = Time.unscaledTime;
        if (direction != heldDirection)
        {
            heldDirection = direction;
            Move(direction);
            nextDirectionMove = now + DirectionRepeatDelay;
        }
        else if (now >= nextDirectionMove)
        {
            Move(direction);
            nextDirectionMove = now + DirectionRepeatInterval;
        }
    }

    private bool AdvanceClock()
    {
        if (!ChallengeRunning || Time.unscaledTime < deadline)
            return false;

        faceCabinet = false;
        phase = Phase.Lost;
        feedback = "時間到！已服務 " + served + "／" + CustomerCount + " 位客人，請從第一位重新開始。";
        return true;
    }

    private void StartRound()
    {
        orderSize = (served + 1) * HerbsPerCustomerStep;
        int[] candidates = new int[drawers.Length];
        for (int i = 0; i < candidates.Length; i++)
            candidates[i] = i;
        for (int i = 0; i < orderSize; i++)
        {
            int other = UnityEngine.Random.Range(i, candidates.Length);
            (candidates[i], candidates[other]) = (candidates[other], candidates[i]);
            order[i] = drawers[candidates[i]];
        }

        Array.Clear(collected, 0, collected.Length);
        cursor = 0;
        filled = 0;
        heldDirection = 0;
        opened = -1;
        finishPullAt = 0;
        faceCabinet = true;
        phase = Phase.Search;
        feedback = "第 " + (served + 1) + " 位客人需要 " + orderSize + " 味藥材。";
    }

    private void Move(int step)
    {
        if (phase == Phase.Search)
            cursor = (cursor + step + drawers.Length) % drawers.Length;
    }

    private void UseHorn()
    {
        if (AdvanceClock())
            return;
        if (Time.unscaledTime < finishPullAt)
            return;
        if (phase == Phase.Ready || phase == Phase.Won || phase == Phase.Lost)
        {
            served = 0;
            ResetQueue();
            deadline = Time.unscaledTime + ChallengeSeconds;
            StartRound();
            return;
        }
        if (phase == Phase.BetweenOrders)
        {
            StartRound();
            return;
        }
        if (!faceCabinet && phase == Phase.Search)
        {
            faceCabinet = true;
            return;
        }
        if (phase != Phase.Search || collected[cursor] ||
            (opened >= 0 && Time.unscaledTime < pullStartedAt + PullDuration))
            return;

        horn.Play();
        opened = cursor;
        pullStartedAt = Time.unscaledTime;
        revealUntil = pullStartedAt + DrawerOpenSeconds;
        string herb = drawers[cursor];
        if (Array.IndexOf(order, herb, 0, orderSize) < 0)
        {
            feedback = "打開的是" + herb + "，請找訂單上的藥材。";
            return;
        }

        collected[cursor] = true;
        filled++;
        if (filled == orderSize)
        {
            served++;
            faceCabinet = false;
            finishPullAt = pullStartedAt + HandReturnDuration;
            phase = served == CustomerCount ? Phase.Won : Phase.BetweenOrders;
            feedback = phase == Phase.Won ? "挑戰成功！已在 " + ChallengeSeconds + " 秒內服務完五位客人。"
                : "藥已交給客人。請接待下一位，倒數繼續。";
            return;
        }
        feedback = "已取出" + herb + "。還差 " + (orderSize - filled) + " 味藥材。";
    }

    private static void GetCanvas(out Vector2 origin, out float scale)
    {
        Rect viewport = Screen.safeArea;
        scale = Mathf.Min(viewport.width / 1024f, viewport.height / 740f);
        origin = new Vector2(
            viewport.x + (viewport.width - 1024 * scale) / 2,
            Screen.height - viewport.yMax + (viewport.height - 740 * scale) / 2);
    }

    private void OnGUI()
    {
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { font = font, alignment = TextAnchor.MiddleCenter };
        }

        GetCanvas(out Vector2 origin, out float scale);
        if (scale <= 0)
            return;
        Matrix4x4 original = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(origin, Quaternion.identity,
            new Vector3(scale, scale, 1));

        DrawDrawerLabels(origin, scale);
        Fill(new Rect(18, 12, 988, 48), Ink);
        Write(new Rect(26, 12, 366, 48), "大稻埕  ·  中藥牆百草尋藥", 23, Cream);
        Write(new Rect(415, 12, 240, 48), "已服務 " + served + " / 5 位客人", 21, Gold);
        Control(new Rect(820, 16, 186, 40), faceCabinet ? "轉向櫃檯  [Tab]" : "轉向藥櫃  [Tab]", new Color32(90, 49, 37, 255), true);
        DrawOrder();
        DrawControls();
        GUI.matrix = original;
    }

    private void DrawOrder()
    {
        Fill(new Rect(54, 69, 916, 62), new Color32(67, 38, 28, 255));
        string orderHeading = orderSize == 0 ? "客人訂單" : "訂單 " + filled + "／" + orderSize;
        Write(new Rect(68, 77, 130, 43), orderHeading, 20, Cream);
        for (int i = 0; i < orderSize; i++)
        {
            Rect ticket = new Rect(204 + i * 53, 77, 49, 43);
            int drawerIndex = Array.IndexOf(drawers, order[i]);
            bool done = drawerIndex >= 0 && collected[drawerIndex];
            Color paper = done ? Green : Cream;
            Fill(ticket, paper);
            Write(ticket, order[i], 17, Ink);
        }

        bool timed = ChallengeRunning;
        float remaining = timed ? Mathf.Max(0, deadline - Time.unscaledTime) : 0;
        string timer = phase switch
        {
            Phase.Ready => "總限時 " + ChallengeSeconds + " 秒",
            Phase.Search or Phase.BetweenOrders => Mathf.CeilToInt(remaining) + " 秒",
            Phase.Won => "挑戰成功",
            _ => "時間到"
        };
        Write(new Rect(735, 72, 219, 44), timer, 26, phase == Phase.Won ? Green : Gold);
        if (timed)
        {
            Fill(new Rect(754, 119, 180, 4), Ink);
            Fill(new Rect(754, 119, 180 * remaining / ChallengeSeconds, 4), Gold);
        }
    }

    private void DrawControls()
    {
        string message = phase == Phase.Ready
            ? "在 " + ChallengeSeconds + " 秒內服務五位客人，依序取出 2、4、6、8、10 味藥材。"
            : feedback;
        Fill(new Rect(54, 630, 916, 43), Ink);
        Write(new Rect(54, 630, 916, 43), message, 20, phase == Phase.Won || phase == Phase.BetweenOrders ? Green : Cream);

        bool running = phase == Phase.Search;
        Control(LeftControl, "◀  左方向燈 [←]", new Color32(113, 61, 42, 255), running);
        Control(RightControl, "右方向燈 [→]  ▶", new Color32(113, 61, 42, 255), running);
        string action = phase switch
        {
            Phase.Ready => "開始接單",
            Phase.BetweenOrders => "接待下一位",
            Phase.Won => "再玩一次",
            Phase.Lost => "從第一位重來",
            _ => faceCabinet ? "喇叭・開抽屜 [Space]" : "轉向藥櫃 [Space]"
        };
        Control(HornControl, action, new Color32(153, 53, 39, 255), true);
    }

    private void Control(Rect rect, string text, Color color, bool enabled)
    {
        Fill(rect, enabled ? Gold : Muted);
        Rect inner = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
        Fill(inner, enabled ? color : Ink);
        Write(inner, text, 21, enabled ? Cream : Muted);
    }

    private void Write(Rect rect, string text, int size, Color color)
    {
        label.fontSize = size;
        label.normal.textColor = color;
        GUI.Label(rect, text, label);
    }

    private static void Fill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static Texture2D CreateWood()
    {
        const int width = 192;
        const int height = 80;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float bend = 4 * Mathf.PerlinNoise(x * 0.025f, y * 0.05f);
                float grain = Mathf.PerlinNoise(x * 0.026f, (y + bend) * 0.72f);
                int shade = Mathf.RoundToInt(25 * grain + 8 * Mathf.Sin((y + bend) * 0.48f));
                pixels[y * width + x] = new Color32((byte)(103 + shade), (byte)(54 + shade), (byte)(33 + shade), 255);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

}

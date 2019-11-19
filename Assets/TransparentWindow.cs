using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;

    #region 导入API

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left; //最左坐标
        public int Top; //最上坐标
        public int Right; //最右坐标
        public int Bottom; //最下坐标
    }
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;

    const uint WS_EX_TOPMOST = 0x00000008;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;

    const int SWP_FRAMECHANGED = 0x0020;
    const int SWP_SHOWWINDOW = 0x0040;
    const int LWA_ALPHA = 2;

    private IntPtr HWND_TOPMOST = new IntPtr(-1);

    #endregion

    public IntPtr windowHandle
    {
        get
        {
            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = FindWindow(null, Application.productName);
            }
            return _windowHandle;
        }
    }

    public int screenWidth = 600;
    public int screenHeight = 1000;
    IntPtr _windowHandle = IntPtr.Zero;
    Vector2Int _offset = Vector2Int.zero;

    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        if (Application.isEditor) return;


        MARGINS margins = new MARGINS() { cxLeftWidth = -1 };

        var pos = LoadPos();
        SetWindowPos(windowHandle, (IntPtr)0, pos.x, pos.y, screenWidth, screenHeight, 4);

        // Set properties of the window
        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591%28v=vs.85%29.aspx
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);


        // Extend the window into the client area
        //See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx 
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);
    }

    private void LateUpdate()
    {
        if (Application.isEditor) return;

        Drag();
    }

    private void Drag()
    {
        // 拖拽开始
        if (Input.GetMouseButtonDown(2))
        {
            RECT rect = new RECT();
            GetWindowRect(windowHandle, ref rect);
            _offset.x = -System.Windows.Forms.Cursor.Position.X + rect.Left;
            _offset.y = -System.Windows.Forms.Cursor.Position.Y + rect.Top;
        }

        // 拖拽中
        if (Input.GetMouseButton(2))
        {
            SetWindowPos(windowHandle, HWND_TOPMOST, System.Windows.Forms.Cursor.Position.X + _offset.x,
            System.Windows.Forms.Cursor.Position.Y + _offset.y, 0, 0, 1 | 4);
        }

        // 结束拖拽
        if (Input.GetMouseButtonUp(2))
        {
            SavePos();
        }

        // 归位
        if (Input.GetKeyDown(KeyCode.F9))
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, 1 | 4);
    }

    void SavePos()
    {
        RECT rect = new RECT();
        GetWindowRect(windowHandle, ref rect);
        PlayerPrefs.SetInt(Application.productName + "_WindowRECT_Left", rect.Left);
        PlayerPrefs.SetInt(Application.productName + "_WindowRECT_Top", rect.Top);
    }

    Vector2Int LoadPos()
    {
        if (PlayerPrefs.HasKey(Application.productName + "_WindowRECT_Left"))
        {
            return new Vector2Int(PlayerPrefs.GetInt(Application.productName + "_WindowRECT_Left"),
             PlayerPrefs.GetInt(Application.productName + "_WindowRECT_Top"));
        }
        else
            return new Vector2Int(100, 100);
    }

    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        Graphics.Blit(from, to, m_Material);
    }
}
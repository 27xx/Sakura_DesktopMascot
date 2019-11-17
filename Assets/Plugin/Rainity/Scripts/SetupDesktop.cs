using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public class SetupDesktop : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }


    static IntPtr oldWndProcPtr;
    static IntPtr newWndProcPtr;
    static WndProcDelegate newWndProc;

    public static bool forceInBack = true;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);
    [DllImport("user32.dll")]
    static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);
    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr child, IntPtr parent);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetTopWindow(IntPtr hwnd);
    [DllImport("user32.dll")]
    static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags fuFlags, uint uTimeout, out IntPtr lpdwResult);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);


    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const int GWL_HWNDPARENT = -8;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;

    const int HWND_TOPMOST = -1;
    const int HWND_BOTTOM = 1;
    const int WM_WINDOWPOSCHANGING = 0x46;
    const int WM_WINDOWPOSCHANGED = 0x47;
    const int WM_ACTIVATE = 0x0006;
    const int WM_SHOWWINDOW = 0x0018;
    const int WM_SYSCOMMAND = 0x0112;
    const int WM_SIZE = 0x0005;
    const long WS_CAPTION = 0x00C00000L;
    const long WS_THICKFRAME = 0x00040000L;
    const long WS_MINIMIZEBOX = 0x00020000L;
    const long WS_MAXIMIZEBOX = 0x00010000L;
    const long WS_SYSMENU = 0x00080000L;
    const long WS_EX_DLGMODALFRAME = 0x00000001L;
    const long WS_EX_CLIENTEDGE = 0x00000200L;
    const long WS_EX_STATICEDGE = 0x00020000L;
    const long WS_EX_TOOLWINDOW = 0x00000080L;
    const long WS_EX_APPWINDOW = 0x00040000L;
    const uint WS_EX_TOPMOST = 0x00000008;
    const int SC_MINIMIZE = 0xf020;
    const int SC_MAXIMIZE = 0xf030;


    [Flags()]
    public enum SetWindowPosFlags
    {
        SWP_NOSIZE = 0x1,
        SWP_NOMOVE = 0x2,
        SWP_NOZORDER = 0x4,
        SWP_NOREDRAW = 0x8,
        SWP_NOACTIVATE = 0x10,
        SWP_FRAMECHANGED = 0x20,
        SWP_DRAWFRAME = SWP_FRAMECHANGED,
        SWP_SHOWWINDOW = 0x40,
        SWP_HIDEWINDOW = 0x80,
        SWP_NOCOPYBITS = 0x100,
        SWP_NOOWNERZORDER = 0x200,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_NOSENDCHANGING = 0x400,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000,
    }

    [Flags()]
    public enum ShowWindowFlags
    {
        SW_MAXIMIZE = 3,
        SW_SHOWMAXIMIZED = 3,
        SW_SHOW = 5,
        SW_HIDE = 0
    }

    [Flags]
    public enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT = 0x20
    }

    enum GetWindow_Cmd : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    static IntPtr handle;
    static IntPtr folderView;
    static IntPtr workerW;
    static IntPtr workerWOrig;
    public static int fWidth;
    public static int fHeight;

    static LowLevelKeyboardProc _kbProc = KbHookCallback;
    static IntPtr _kbHookID = IntPtr.Zero;

    public static Vector2 windowOffset;
    public static bool hideFromTaskbar = false;
    public static bool neverHide = true;
    public static bool keepBottomMost = true;
    public static bool borderless = true;
    public static bool behindIcons = false;

    public static bool appQuitting = false;

    // Use this for initialization
    public static void Initialize()
    {
        //Get Windows desktop handle
        workerW = IntPtr.Zero;
        IntPtr progman = FindWindow("Progman", null);

        folderView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
        if (folderView == IntPtr.Zero)
        {
            //If the desktop isn't under Progman, cycle through the WorkerW handles and find the correct one
            do
            {
                workerWOrig = FindWindowEx(WinAPI.desktopWindow, workerWOrig, "WorkerW", null);
                folderView = FindWindowEx(workerWOrig, IntPtr.Zero, "SHELLDLL_DefView", null);
            } while (folderView == IntPtr.Zero && workerWOrig != IntPtr.Zero);
        }
        folderView = FindWindowEx(folderView, IntPtr.Zero, "SysListView32", "FolderView");

        if (behindIcons)
        {
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {

                IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    workerW = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);
        }

        handle = WinAPI.FindWindow(null, UnityEngine.Application.productName);

        if (!UnityEngine.Application.isEditor)
        {
            Rectangle desktopSize = new Rectangle(0, 0, 0, 0);
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                desktopSize.Width += screen.Bounds.Width;
                if (screen.Bounds.Height > desktopSize.Height)
                    desktopSize.Height = screen.Bounds.Height;
            }

            fWidth = desktopSize.Width + (int)windowOffset.x;
            fHeight = desktopSize.Height + (int)windowOffset.y;

            if (borderless)
            {
                long lStyle = WinAPI.GetWindowLong(handle, GWL_STYLE);
                lStyle &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
                SetWindowLongPtr(handle, GWL_STYLE, lStyle);
                long lExStyle = WinAPI.GetWindowLong(handle, GWL_EXSTYLE);
                lExStyle &= ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_APPWINDOW);
                SetWindowLongPtr(handle, GWL_EXSTYLE, lExStyle);
            }

            if (keepBottomMost)
                WinAPI.SetWindowPos(handle, HWND_BOTTOM, (int)windowOffset.x, (int)windowOffset.y, fWidth, fHeight, (int)(SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER));

            if (behindIcons)
            {
                WinAPI.SetWindowPos(handle, HWND_BOTTOM, (int)windowOffset.x, (int)windowOffset.y, fWidth, fHeight, 0);
                SetParent(handle, workerW);
                WinAPI.SetWindowPos(handle, HWND_BOTTOM, (int)windowOffset.x, (int)windowOffset.y, fWidth, fHeight, 0);
            }
            else if (neverHide)
            {
                SetWindowLongPtr(handle, GWL_HWNDPARENT, folderView);
            }

            UnityEngine.Screen.SetResolution(fWidth, fHeight, false);

            // SetWindowLong(handle, GWL_STYLE, (System.IntPtr)(WS_POPUP | WS_VISIBLE));
            // SetWindowLong(handle, GWL_EXSTYLE, (System.IntPtr)(WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT));


            // MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
            // DwmExtendFrameIntoClientArea(handle, ref margins);

        }

        SendMessage(folderView, 0x1000 + 4, 0, IntPtr.Zero);
        _kbHookID = SetHook(_kbProc);
    }

    public static void AddWinProc()
    {
        if (!UnityEngine.Application.isEditor)
        {
            if (keepBottomMost || neverHide || borderless)
            {
                newWndProc = new WndProcDelegate(wndProc);
                newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
                oldWndProcPtr = SetWindowLongPtr(handle, -4, newWndProcPtr);
            }
        }
    }

    private static IntPtr StructToPtr(object obj)
    {
        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
        Marshal.StructureToPtr(obj, ptr, false);
        return ptr;
    }
    static IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_WINDOWPOSCHANGING && forceInBack && !appQuitting)
        {
            winPosChange(hWnd, msg, wParam, lParam);
        }
        return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
    }
    static void winPosChange(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (ShowWindow(handle, (int)ShowWindowFlags.SW_SHOWMAXIMIZED))
        {
            if (borderless)
            {
                long lStyle = WinAPI.GetWindowLong(handle, GWL_STYLE);
                lStyle &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
                SetWindowLongPtr(handle, GWL_STYLE, lStyle);
                long lExStyle = WinAPI.GetWindowLong(handle, GWL_EXSTYLE);
                lExStyle &= ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_APPWINDOW);
                SetWindowLongPtr(handle, GWL_EXSTYLE, lExStyle);
            }

            if (keepBottomMost)
            {
                WINDOWPOS wndPos = WINDOWPOS.FromMessage(hWnd, msg, wParam, lParam);
                wndPos.flags = wndPos.flags | SetWindowPosFlags.SWP_NOZORDER;
                wndPos.UpdateMessage(hWnd, msg, wParam, lParam);
            }
        }
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        int WH_KEYBOARD_LL = 13;
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr KbHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)KeyboardMessages.WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            RainityInput.SetKeyCodeDown((uint)vkCode);
        }
        if (nCode >= 0 && wParam == (IntPtr)KeyboardMessages.WM_KEYUP)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            RainityInput.SetKeyCodeUp((uint)vkCode);
        }

        return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
    }

    public static void UnhookHook()
    {
        WinAPI.ShowWindow(handle, (int)ShowWindowFlags.SW_HIDE);
        UnhookWindowsHookEx(_kbHookID);
    }

    public static void CheckForDesktopInteraction()
    {
        if (WinAPI.GetForegroundWindow() == workerWOrig)
        {
            WinAPI.SetFocus(handle);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hWnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SetWindowPosFlags flags;

        public static WINDOWPOS FromMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
        }

        public void UpdateMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }
    }

    private enum MouseMessages
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205
    }

    private enum KeyboardMessages
    {
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }


    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        UnityEngine.Graphics.Blit(from, to, m_Material);
    }

}
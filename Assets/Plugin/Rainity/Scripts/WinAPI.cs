using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System;

public class WinAPI : MonoBehaviour {

	public static WinAPI instance = null;

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left, Top, Right, Bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public int X
		{
			get { return Left; }
			set { Right -= (Left - value); Left = value; }
		}

		public int Y
		{
			get { return Top; }
			set { Bottom -= (Top - value); Top = value; }
		}

		public int Height
		{
			get { return Bottom - Top; }
			set { Bottom = value + Top; }
		}

		public int Width
		{
			get { return Right - Left; }
			set { Right = value + Left; }
		}

		/*public static bool operator ==(RECT r1, RECT r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(RECT r1, RECT r2)
		{
			return !r1.Equals(r2);
		}

		public bool Equals(RECT r)
		{
			return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
		}*/

		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
		}
	}

	public struct MARGINS
	{
		public int cxLeftWidth;
		public int cxRightWidth;
		public int cyTopHeight;
		public int cyBottomHeight;
	}

	public struct POINTFX
	{
		public int x;
		public int y;
	}

	[DllImport("user32.dll")]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
	[DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
	public static extern IntPtr GetActiveWindow();
	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();
	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(IntPtr hWnd);
	[DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
	public static extern int SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);
	[DllImport("user32.dll")]
	public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	[DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
	public static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);
	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	public static extern long GetWindowLong(IntPtr hwnd, int nIndex);
	[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
	public static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);
	[DllImport("user32.dll")]
	public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool repaint);
	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);
	[DllImport("user32.dll")]
	public static extern bool PrintWindow(IntPtr handle, System.IntPtr hdc, uint flags);
	[DllImport("user32.dll")]
	public static extern int GetDC(IntPtr handle);
	[DllImport("user32.dll")]
	public static extern int ReleaseDC(IntPtr handle, System.IntPtr hdc);
	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();
	//[DllImport("user32.dll")]
	//public static extern IntPtr GetDesktopWindow();
	[DllImport("user32.dll")]
	public static extern int UpdateLayeredWindow(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);
	[DllImport("user32.dll")]
	public static extern bool UpdateWindow(IntPtr hWnd);
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
	[DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
	[DllImport("user32.dll")]
	public static extern IntPtr SetFocus(IntPtr hWnd);
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetCursorPos(out POINTFX lpPoint);

	[DllImport("Dwmapi.dll")]
	public static extern uint DwmExtendFrameIntoClientArea (int hWnd, ref MARGINS margins);

	[DllImport("Gdi32.dll")]
	public static extern bool BitBlt(System.IntPtr hdc, int xDest, int yDest, int width, int height, System.IntPtr hdcSrc, int xSrc, int ySrc, int dwRop);
	[DllImport("Gdi32.dll")]
	public static extern System.IntPtr CreateCompatibleDC(System.IntPtr hdc);
	[DllImport("Gdi32.dll")]
	public static extern int GetDeviceCaps(System.IntPtr hdc, int index);
	[DllImport("Gdi32.dll")]
	public static extern System.IntPtr SelectObject(System.IntPtr hdc, System.IntPtr hgdiobj);
	[DllImport("Gdi32.dll")]
	public static extern int DeleteDC(System.IntPtr hdc);
	[DllImport("Gdi32.dll")]
	public static extern System.IntPtr DeleteObject(System.IntPtr ptr);


	[DllImport("shell32.dll")]
	public static extern System.IntPtr ExtractAssociatedIcon(int hInst, StringBuilder lpIconPath, out ushort lpiIcon);
	[DllImport("shell32.dll")]
	public static extern System.IntPtr ExtractIcon(System.IntPtr hInst, string lpszExeFileName, int nIconIndex);
	[DllImport("shell32.dll", EntryPoint = "#261", CharSet = CharSet.Unicode, PreserveSig = false)]
	public static extern void SHGetUserPicturePath(string username, UInt32 whatever, StringBuilder picpath, int maxLength);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	public static IntPtr activeWindow;
	public static IntPtr foregroundWindow;
	public static IntPtr thisGame;
	public static IntPtr desktopWindow;
	public static RECT windowRect;

	void Awake() {
		if (instance == null) instance = this;
		else if (instance != this) Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		thisGame = GetActiveWindow();
		desktopWindow = GetDesktopWindow();
	}

	void Update() {
		activeWindow = GetActiveWindow();
		foregroundWindow = GetForegroundWindow();
		GetWindowRect(foregroundWindow, out windowRect);
	}
}

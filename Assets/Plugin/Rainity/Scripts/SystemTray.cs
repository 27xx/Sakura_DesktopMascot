using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using UnityEngine;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

public class SystemTray : IDisposable {

	[DllImport("shell32.dll")]
	static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

	public NotifyIcon trayIcon;
	public System.Windows.Forms.ContextMenu trayMenu;

	private List<Action> actions = new List<Action>();

	public SystemTray() {
		
		trayMenu = new System.Windows.Forms.ContextMenu();

		trayIcon = new NotifyIcon();
		trayIcon.Text = UnityEngine.Application.productName;

		if (UnityEngine.Application.isEditor)
			trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
		else {
			ushort uicon;
			StringBuilder strB = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory + "\\" + UnityEngine.Application.productName + ".exe");
			IntPtr handle = ExtractAssociatedIcon(IntPtr.Zero, strB, out uicon);
			Icon ico = Icon.FromHandle(handle);
			trayIcon.Icon = ico;
		}

		trayIcon.ContextMenu = trayMenu;
		trayIcon.Visible = true;
	}

	//Currently does not work
	public void SetIcon(Texture2D icon) {
		using (MemoryStream ms = new MemoryStream(icon.EncodeToPNG())) {
			ms.Seek(0, SeekOrigin.Begin);
			Bitmap bmp = new Bitmap(ms);

			Icon tIcon = Icon.FromHandle(bmp.GetHicon());
			trayIcon.Icon = tIcon;
		}
	}

	public void SetTitle(string title) {
		trayIcon.Text = title;
	}

	public void AddItem(string label, Action function) {
		actions.Add(function);
		trayMenu.MenuItems.Add(label, OnAdd);
	}

	public void AddSeparator() {
		trayMenu.MenuItems.Add("-");
	}

	//Currently does not work
	public void ShowNotification(int duration, string title, string text) {
		trayIcon.Visible = true;
		trayIcon.BalloonTipTitle = title;
		trayIcon.BalloonTipText = text;
		trayIcon.BalloonTipIcon = ToolTipIcon.Info;
		trayIcon.ShowBalloonTip(5000);
	}

	private void OnAdd(object sender, EventArgs e) {
		Action ac = actions[((MenuItem)sender).Index];
		if (ac != null)
			ac();
		else
			UnityEngine.Debug.LogError("System tray's context menu action is null!");
	}

	public void Dispose() {
		trayIcon.Visible = false;
		trayMenu.Dispose();
		trayIcon.Dispose();
	}
}

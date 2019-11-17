using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DemoScript : MonoBehaviour {

	public Renderer iconRenderer;
	public Renderer wallpaperRenderer;
	public Renderer avatarRenderer;

	public Texture2D testIcon;

	public Text weather_cityState;
	public Text weather_temperature;
	public Text weather_condition;
	public Text weather_windSpeed;

	public Text inputStats;

	private SystemTray tray;

	// Use this for initialization
	void Start() {
		//Set file icon texture demonstration
		if (File.Exists(Environment.SystemDirectory + "\\notepad.exe")) {
			iconRenderer.material.mainTexture = Rainity.GetFileIcon(Environment.SystemDirectory + "\\notepad.exe");
		}
		//Set wallpaper image texture demonstration
		wallpaperRenderer.material.mainTexture = Rainity.GetWallpaperImage();
		//Set user avatar texture demonstration
		avatarRenderer.material.mainTexture = Rainity.GetUserAvatar();

		//Create system tray icon (standalone only, editor causes random crashes)
		tray = Rainity.CreateSystemTrayIcon();
		if (tray != null) {
			tray.AddItem("Context menu items with attached functions!", ExitApplication);
			tray.AddSeparator();
			tray.AddItem("Exit", ExitApplication);

			tray.SetTitle("Rainity Demo Application");
		}

		//Get information about the weather and assign text
		WeatherObject weather = Rainity.GetWeatherInformation();
		weather_cityState.text = weather.query.results.channel.location.city + ", " + weather.query.results.channel.location.region;
		weather_temperature.text = weather.query.results.channel.item.condition.temp + "°" + weather.query.results.channel.units.temperature;
		weather_condition.text = weather.query.results.channel.item.condition.text;
		weather_windSpeed.text = "Wind speed: " + weather.query.results.channel.wind.speed + weather.query.results.channel.units.speed;

		RainityFile[] files = Rainity.GetFiles("C:\\Users\\Christian\\Desktop");
		foreach (RainityFile file in files) {
			Debug.Log(file.fileName);
		}
	}

	// Update is called once per frame
	void Update() {
		inputStats.text = "Rainity Input - X:" + RainityInput.mousePosition.x + ", Y:" + RainityInput.mousePosition.y + "\nDefault Input - X:" + Input.mousePosition.x + ", Y: " + Input.mousePosition.y;
	}

	public void ExitApplication() {
		Application.Quit();
	}

	public void SimulateKey(int id) {
		RainityInput.VirtualKeys[] keys = {
			RainityInput.VirtualKeys.MediaPlayPause,
			RainityInput.VirtualKeys.MediaNextTrack,
			RainityInput.VirtualKeys.MediaPrevTrack
		};

		Rainity.SimulateKey((uint)keys[id]);
	}

	//Currently does not work
	public void ShowDemoNotification() {
		if (tray != null) {
			tray.ShowNotification(5000, "Test Notification", "This is a Rainity-powered notification!");
		}
	}
}

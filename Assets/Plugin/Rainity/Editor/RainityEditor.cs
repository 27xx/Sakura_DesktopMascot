using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Rainity))]
[CanEditMultipleObjects]
public class RainityEditor : Editor {

	SerializedProperty windowOffset;
	SerializedProperty hideFromTaskbar;
	SerializedProperty neverHideWindow;
	SerializedProperty keepBottommost;
	SerializedProperty borderless;
	SerializedProperty behindIcons;
	SerializedProperty useRainityInput;

	private void OnEnable() {
		windowOffset = serializedObject.FindProperty("windowOffset");
		hideFromTaskbar = serializedObject.FindProperty("hideFromTaskbar");
		neverHideWindow = serializedObject.FindProperty("neverHideWindow");
		keepBottommost = serializedObject.FindProperty("keepBottomMost");
		borderless = serializedObject.FindProperty("borderless");
		behindIcons = serializedObject.FindProperty("behindIcons");
		useRainityInput = serializedObject.FindProperty("useRainityInput");
	}

	public override void OnInspectorGUI() {
		EditorStyles.label.wordWrap = true;

		serializedObject.Update();
		EditorGUILayout.PropertyField(windowOffset);
		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(hideFromTaskbar);
		EditorGUILayout.PropertyField(neverHideWindow);
		EditorGUILayout.PropertyField(keepBottommost);
		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(borderless);
		EditorGUILayout.PropertyField(behindIcons);
		EditorGUILayout.PropertyField(useRainityInput);
		EditorGUILayout.Separator();

		if (GUILayout.Button("Documentation")) {
			Application.OpenURL("https://christiankosman.com/docs/rainity");
		}
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Tips:");
		EditorGUILayout.LabelField("To make the wallpaper show behind your icons, enable only Borderless and Behind Icons and set the window offset to X:0, Y:0.");
		EditorGUILayout.LabelField("To make the wallpaper overlay your icons but stay under all other windows, enable Never Hide Window, Keep Bottom Most, and Borderless.  You can also set the window offset to the height of your taskbar so it doesn't overlay it.");
		EditorGUILayout.LabelField("If your wallpaper is displayed behind icons, you can enable Use Rainity Input to capture mouse input.");
		EditorGUILayout.LabelField("For a quick and stylish way to quit the wallpaper, use Rainity.CreateSystemTrayIcon() to create an icon in the system tray.  You can then add context menu items that perform actions such as quitting the application.");

		serializedObject.ApplyModifiedProperties();
	}
}

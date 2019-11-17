using UnityEngine;
using UnityEngine.EventSystems;

public class RainityBaseInput : BaseInput
{
	public override string compositionString
	{
		get { return Input.compositionString; }
	}

	public override IMECompositionMode imeCompositionMode
	{
		get { return Input.imeCompositionMode; }
		set { Input.imeCompositionMode = value; }
	}

	public override Vector2 compositionCursorPos
	{
		get { return Input.compositionCursorPos; }
		set { Input.compositionCursorPos = value; }
	}

	public override bool mousePresent
	{
		get { return true; }
	}

	public override bool GetMouseButtonDown(int button)
	{
		return RainityInput.GetMouseButtonDown(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		return RainityInput.GetMouseButtonUp(button);
	}

	public override bool GetMouseButton(int button)
	{
		return RainityInput.GetMouseButton(button);
	}

	public override Vector2 mousePosition
	{
		get { return RainityInput.mousePosition; }
	}

	public override Vector2 mouseScrollDelta
	{
		get { return Input.mouseScrollDelta; }
	}

	public override bool touchSupported
	{
		get { return Input.touchSupported; }
	}

	public override int touchCount
	{
		get { return Input.touchCount; }
	}

	public override Touch GetTouch(int index)
	{
		return Input.GetTouch(index);
	}

	public override float GetAxisRaw(string axisName)
	{
		return Input.GetAxisRaw(axisName);
	}

	public override bool GetButtonDown(string buttonName)
	{
		return RainityInput.GetButtonDown(buttonName);
	}
}
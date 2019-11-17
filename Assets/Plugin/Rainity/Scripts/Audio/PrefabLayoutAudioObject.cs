using Assets.Scripts;
using Assets.Scripts.Extensions;
using Assets.Scripts.ReactiveEffects.Base;
using System.Collections.Generic;
using UnityEngine;


#region Public Enums

public enum PrefabLayoutType
{
    XLinear,
    XZSpread,
    XZCircular,
	XYCircular
}

#endregion

public class PrefabLayoutAudioObject : VisualizationEffectBase
{
    #region Private Member Variables

    private List<GameObject> _gameObjects = new List<GameObject>();

    #endregion

    #region Public Properties

    public GameObject Prefab;
    public PrefabLayoutType LayoutType;
    public float ObjectWidthDepth;
    public float CircularLayoutRadius;
    public Vector3 ObjectRotation;
    public Vector3 RotationOffset;
	public Vector3 ScaleIntensity = new Vector3(0, 1, 0);
	public bool Shuffle;
	public bool Flat;
    public int DesiredColorGroupCount;

    #endregion

    #region Startup / Shutdown

    public override void Start()
    {
        base.Start();
	
        // Instantiate GameObjects
        for (int i = 0; i < LoopbackAudio.SpectrumSize; i++)
        {
            GameObject newGameObject = Instantiate(Prefab);
            newGameObject.transform.parent = gameObject.transform;
            _gameObjects.Add(newGameObject);

            // Try to set various other used scripts
            VisualizationEffectBase[] visualizationEffects = newGameObject.GetComponents<VisualizationEffectBase>();

            if (visualizationEffects != null && visualizationEffects.Length > 0)
            {
                foreach (VisualizationEffectBase visualizationEffect in visualizationEffects)
                {
                    visualizationEffect.AudioVisualizationStrategy = AudioVisualizationStrategy;
                    visualizationEffect.AudioSampleIndex = i;
                }
            }
        }

        performLayout();
    }

    #endregion

    #region Render

    public void Update()
    {
#if DEBUG

        if (Input.GetKeyDown(KeyCode.L))
        {
            performLayout();
        }

#endif
    }

    #endregion

    #region Private Methods

    private void performLayout()
    {
        List<Vector3> layoutPositions = new List<Vector3>();
		List<Vector3> layoutRotations = new List<Vector3>();

		switch (LayoutType)
        {
            case PrefabLayoutType.XLinear:

                float offsetX = -(((LoopbackAudio.SpectrumData.Length) / 2.0f) * ObjectWidthDepth);
                float xStep = ObjectWidthDepth;

                for (int i = 0; i < LoopbackAudio.SpectrumData.Length; i++)
                {
                    layoutPositions.Add(new Vector3(offsetX, 0.0f, 0.0f));

                    offsetX += xStep;
                }

                for (int i = 0; i < layoutPositions.Count; i++)
                {
                    _gameObjects[i].transform.localPosition = layoutPositions[i];
                }

                break;
            case PrefabLayoutType.XZSpread:

                int layoutStepCount = (int)Mathf.Sqrt(LoopbackAudio.SpectrumData.Length);
                float offsetStep = ObjectWidthDepth;
                float xzOffset = -((LoopbackAudio.SpectrumData.Length * offsetStep) / 16.0f) - (offsetStep / 2.0f);
                Vector3 initialOffset = new Vector3(xzOffset, 0.0f, xzOffset) + gameObject.transform.position;
                Vector3 offset = new Vector3(initialOffset.x, initialOffset.y, initialOffset.z);

                for (int i = 0; i < layoutStepCount; i++)
                {
                    offset.x += offsetStep;

                    for (int j = 0; j < layoutStepCount; j++)
                    {
                        offset.z += offsetStep;
                        layoutPositions.Add(new Vector3(offset.x, offset.y, offset.z));
                    }

                    offset.z = initialOffset.z;
                }

                if (Shuffle)
                {
                    layoutPositions.Shuffle();
                }

                for (int i = 0; i < layoutPositions.Count; i++)
                {
                    _gameObjects[i].transform.localPosition = layoutPositions[i];
                }

                break;
            case PrefabLayoutType.XZCircular:

                float angleStep = (360.0f) / LoopbackAudio.SpectrumData.Length;
                float currentAngle = 0.0f;

                for (int i = 0; i < LoopbackAudio.SpectrumData.Length; i++)
                {
                    float angle = ((float)i / (LoopbackAudio.SpectrumSize) * Mathf.PI * 2.0f) - (Mathf.PI / 2.0f);

                    float x = Mathf.Sin(angle) * CircularLayoutRadius;
                    float z = Mathf.Cos(angle) * CircularLayoutRadius;

                    layoutPositions.Add(new Vector3(x + gameObject.transform.position.x, gameObject.transform.position.y, z + gameObject.transform.position.z));

                    currentAngle += angleStep;
                }

                if (Shuffle)
                {
                    layoutPositions.Shuffle();
                }

                for (int i = 0; i < layoutPositions.Count; i++)
                {
                    _gameObjects[i].transform.localPosition = layoutPositions[i];
                    _gameObjects[i].transform.Rotate(ObjectRotation);
                }

                break;
			case PrefabLayoutType.XYCircular:

				float angStep = (360.0f) / LoopbackAudio.SpectrumData.Length;
				float currentAng = 0.0f;

				for (int i = 0; i < LoopbackAudio.SpectrumData.Length; i++) {
					float angle = ((float)i / (LoopbackAudio.SpectrumSize) * Mathf.PI * 2.0f) - (Mathf.PI / 2.0f);

					float x = Mathf.Sin(angle) * CircularLayoutRadius;
					float y = Mathf.Cos(angle) * CircularLayoutRadius;

					layoutPositions.Add(new Vector3(x + gameObject.transform.position.x, y + gameObject.transform.position.y, gameObject.transform.position.z));

					currentAng += angStep;
					layoutRotations.Add(new Vector3(0, 0, -currentAng + 90));
				}

				if (Shuffle) {
					layoutPositions.Shuffle();
				}

				for (int i = 0; i < layoutPositions.Count; i++) {
					_gameObjects[i].transform.localPosition = layoutPositions[i];
					_gameObjects[i].transform.localEulerAngles = layoutRotations[i];
				}

				break;
		}

		for (int i = 0; i < layoutPositions.Count; i++) {
			if(_gameObjects[i].transform.GetChild(0) && Flat)
				_gameObjects[i].transform.GetChild(0).transform.localPosition = new Vector3(0, 0.125f, 0);
			var objScaleComp = _gameObjects[i].GetComponent<Assets.Scripts.ReactiveEffects.ObjectScaleReactiveEffect>();
			if(objScaleComp)
				objScaleComp.ScaleIntensity = ScaleIntensity;
		}

		gameObject.transform.Rotate(RotationOffset);
    }

    #endregion
}
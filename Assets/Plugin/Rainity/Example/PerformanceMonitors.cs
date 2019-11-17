using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceMonitors : MonoBehaviour {

	public Text cpuText;
	public Image cpuImage;

	public Text ramText;
	public Image ramImage;

	public Text vramText;
	public Image vramImage;

	public Text diskText;
	public Image diskImage;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		MemoryInformation memInfo = Rainity.GetMemoryInformation();
		ramText.text = Mathf.Round(memInfo.ramUsed / memInfo.ramTotal * 100).ToString() + "%";
		ramImage.fillAmount = memInfo.ramUsed / memInfo.ramTotal;
		vramText.text = Mathf.Round(memInfo.vRamUsed / memInfo.vRamTotal * 100).ToString() + "%";
		vramImage.fillAmount = memInfo.vRamUsed / memInfo.vRamTotal;

		DiskInformation diskInfo = Rainity.GetDiskInformation("C:\\");
		diskText.text = Mathf.Round((float)diskInfo.bytesFree / (float)diskInfo.bytesTotal * 100).ToString() + "%";
		diskImage.fillAmount = (float)diskInfo.bytesFree / (float)diskInfo.bytesTotal;

		cpuText.text = Mathf.Round(Rainity.GetCPUUsagePercent()).ToString() + "%";
		cpuImage.fillAmount = Rainity.GetCPUUsagePercent()/100;
	}
}

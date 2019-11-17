using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherObject {
	public Weather_Query query;
}

[System.Serializable]
public class Weather_Query {
	public int count;
	public string created;
	public string lang;
	public Weather_Results results;
}

[System.Serializable]
public class Weather_Results {
	public Weather_Channel channel;
}

[System.Serializable]
public class Weather_Channel {
	public Weather_Units units;
	public string title;
	public string link;
	public string description;
	public string language;
	public string lastBuildDate;
	public string ttl;
	public Weather_Location location;
	public Weather_Wind wind;
	public Weather_Atmosphere atmosphere;
	public Weather_Astronomy astronomy;
	public Weather_Item item;
}

[System.Serializable]
public class Weather_Units {
	public string distance;
	public string pressure;
	public string speed;
	public string temperature;
}

[System.Serializable]
public class Weather_Location
{
	public string city;
	public string country;
	public string region;
}

[System.Serializable]
public class Weather_Wind
{
	public string chill;
	public string direction;
	public string speed;
}

[System.Serializable]
public class Weather_Atmosphere
{
	public string humidity;
	public string pressure;
	public string rising;
	public string visibility;
}

[System.Serializable]
public class Weather_Astronomy
{
	public string sunrise;
	public string sunset;
}

[System.Serializable]
public class Weather_Image
{
	public string title;
	public string width;
	public string height;
	public string link;
	public string url;
}

[System.Serializable]
public class Weather_Item
{
	public string title;
	public string lat;
	public string longi;
	public string link;
	public string pubDate;
	public Weather_Condition condition;
	public Weather_Forecast[] forecast;
	public string description;
}

[System.Serializable]
public class Weather_Condition
{
	public string code;
	public string date;
	public string temp;
	public string text;
}

[System.Serializable]
public class Weather_Forecast
{
	public string code;
	public string date;
	public string day;
	public string high;
	public string low;
	public string text;
}
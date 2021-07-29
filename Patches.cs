using HarmonyLib;

namespace HeatMapEnhanced
{
	public class Patches
	{
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Enable")]
		public class TemperatureToColorPatch
		{
			static bool effect = false;
			static SimDebugView.ColorThreshold[] colorThreshold;
			
			public static void Prefix()
			{
				if(!effect){
					SimDebugView.ColorThreshold[] tmp = SimDebugView.Instance.temperatureThresholds;
					colorThreshold = new SimDebugView.ColorThreshold[tmp.Length];
					for (int i = 0; i<tmp.Length ; i++){
						colorThreshold[i] = tmp[i];
					}
					effect = true;
				}
				float lowerBound = 273 + 125;
				string a = "modified: ";
				string aa = "original: ";
				string aaa = "factor: ";
				float maxTemp = 273 + 500;
				float step = (maxTemp - lowerBound) / (colorThreshold.Length - 2);
					for(int i = 1; i < colorThreshold.Length - 1; i++){
						float currentVal = colorThreshold[i].value;					
						float b = lowerBound + step * (i-1);
						SimDebugView.Instance.temperatureThresholds[i].value = b;
						Debug.Log(a+b.ToString());
						Debug.Log(aa+colorThreshold[i].value.ToString());
						Debug.Log(aaa+(currentVal / maxTemp).ToString());
					}
			}

			public static void Postfix()
			{
			}
		}
		
		
	}
}

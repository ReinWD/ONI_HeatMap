using HarmonyLib;

namespace HeatMapEnhanced
{
	public class Patches
	{
		[HarmonyPatch(typeof(SimDebugView))]
		[HarmonyPatch("TemperatureToColor")]
		public class TemperatureToColorPatch
		{
			static bool effect = false;
			public static void Prefix()
			{
				if(!effect){
					float lowerBound = 273;
					SimDebugView.ColorThreshold[] colorThreshold = SimDebugView.Instance.temperatureThresholds;
					string a = "colorThreshold: "+ colorThreshold;
					Debug.Log(a);
					
					for(int i = 0; i < colorThreshold.Length; i++){

					}
				}
				Debug.Log("I execute before SimDebugView.TemperatureToColor!");
			}

			public static void Postfix()
			{
				Debug.Log("I execute after SimDebugView.TemperatureToColor!");
			}
		}
		
		
	}
}

using HarmonyLib;

namespace reinwd.HeatMap
{
	public class HeatMapPatch
	{
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Enable")]
		public class OnEnable{
			static bool effect = false;
			static SimDebugView.ColorThreshold[] colorThreshold;
			static float lowerBound = 273 + 125;
			static float maxTemp = 273 + 500;
			static bool modified = false;

			public static void Prefix()
			{
				if(!effect){
					SimDebugView.ColorThreshold[] tmp = SimDebugView.Instance.temperatureThresholds;
					colorThreshold = new SimDebugView.ColorThreshold[tmp.Length];
					for (int i = 0; i<tmp.Length ; i++){
						colorThreshold[i] = tmp[i];
					}
					lowerBound = tmp[1].value;
					lowerBound = tmp[tmp.Length-2].value;
					effect = true;
				}
				string a = "modified: ";
				string aa = "original: ";
				string aaa = "factor: ";
				float step = (maxTemp - lowerBound) / (colorThreshold.Length - 2);

				HeatMap.showUI();
				for(int i = 1; i < colorThreshold.Length - 1; i++){
					if(modified){
						float currentVal = colorThreshold[i].value;					
						float b = lowerBound + step * (i-1);
						SimDebugView.Instance.temperatureThresholds[i].value = b;
						Debug.Log(a+b.ToString());
						Debug.Log(aa+colorThreshold[i].value.ToString());
						Debug.Log(aaa+(currentVal / maxTemp).ToString());
					}else{
						SimDebugView.Instance.temperatureThresholds[i] = tmp[i]
					}
				}
			}

		
		}

		public static void showUI(){
			if(!uiInited){
				initUI();
			}
		}

		public static void hideUI(){}

		public static void initUI(){
			
		}
		
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Disable")]
		public class OnDisable{
			public static void Prefix()
			{
				HeatMap.hideUI();
			}
		}
		
	}
}

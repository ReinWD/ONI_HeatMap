using HarmonyLib;

namespace reinwd.HeatMap
{
	public class HeatMapPatch
	{
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Enable")]
		public class OnEnable{
			static bool initialized = false;

			public static void Prefix()
			{
				HeatMapControl.Instance.showUI();
				updateTempInfo();
			}	
		}
		[HarmonyPatch(typeof(OverlayScreen))]
		[HarmonyPatch("Refresh")]
		public class OnRefresh{
			static bool initialized = false;

			public static void Prefix()
			{
				updateTempInfo();
			}	
		}

	public static void updateTempInfo(){
					var lowerBound = HeatMapControl.lowerBound;
					var maxTemp = HeatMapControl.upperBound;
					var modified = HeatMapControl.modified;
					var colorThreshold = HeatMapControl.colorThreshold;
					string a = "modified: ";
					string aa = "original: ";
					string aaa = "factor: ";
					float step = (maxTemp - lowerBound) / (colorThreshold.Length - 2);

					for(int i = 1; i < colorThreshold.Length - 1; i++){
						if(modified){
							float currentVal = colorThreshold[i].value;					
							float b = lowerBound + step * (i-1);
							SimDebugView.Instance.temperatureThresholds[i].value = b;
							Debug.Log(a+b.ToString());
							Debug.Log(aa+colorThreshold[i].value.ToString());
							Debug.Log(aaa+(currentVal / maxTemp).ToString());
						}else{
							SimDebugView.Instance.temperatureThresholds[i] = colorThreshold[i];
						}
					}
			}	
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Disable")]
		public class OnDisable{
			public static void Prefix()
			{
				HeatMapControl.Instance.hideUI();
			}
		}
		
	}
}

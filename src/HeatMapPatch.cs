using HarmonyLib;

namespace reinwd.HeatMap
{
	public class HeatMapPatch
	{
		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("Enable")]
		public class OnEnable{
			public static void Prefix()
			{
				HeatMapControl.Instance.showUI();
				HeatMapControl.updateTempInfo();
			}	
		}
		[HarmonyPatch(typeof(OverlayScreen))]
		[HarmonyPatch("Refresh")]	
		public class OnRefresh{
			public static void Prefix()
			{
				HeatMapControl.updateTempInfo();
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

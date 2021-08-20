using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace reinwd.HeatMap{
    public class HeatMapControl{
        public static HeatMapControl Instance = new HeatMapControl();

        /// <summary>
		/// The margin around the scrollable area to avoid stomping on the scrollbar.
		/// </summary>
		private static readonly RectOffset ELEMENT_MARGIN = new RectOffset(2, 2, 2, 2);

		/// <summary>
		/// The margin around the text in the title bar.
		/// </summary>
		private static readonly RectOffset TITLE_MARGIN = new RectOffset(5, 5, 3, 3);

		/// <summary>
		/// The indent of the categories, and the items in each category.
		/// </summary>
		internal const int INDENT = 24;

		/// <summary>
		/// The size of the panel (UI sizes are hard coded in prefabs).
		/// </summary>
		internal static readonly Vector2 PANEL_SIZE = new Vector2(260.0f, 320.0f);

		/// <summary>
		/// The margin between the scroll pane and the window.
		/// </summary>
		private static readonly RectOffset OUTER_MARGIN = new RectOffset(6, 10, 6, 14);

		/// <summary>
		/// The size of checkboxes and images in this control.
		/// </summary>
		internal static readonly Vector2 ROW_SIZE = new Vector2(24.0f, 24.0f);

		/// <summary>
		/// The spacing between each row.
		/// </summary>
		internal const int ROW_SPACING = 5;


        public bool uiInited = false;

       	/// <summary>
		/// The root panel of the whole control.
		/// </summary>
		public GameObject RootPanel { get; set;}

        private int HandleSize = 30;

        private GameObject upperSlider;
        private GameObject lowerSlider;

        private PLabel upperLabel;
        private PLabel lowerLabel;

		private GameObject resetButton;
		private PCheckBox linkCheckBox;
		private GameObject linkCheckBoxObj;
		private bool linked {get; set;}

        public static float lowerBound = 273 + 125;
		public static float upperBound = 273 + 500;
		public static bool modified = false;
		public static SimDebugView.ColorThreshold[] colorThreshold;

		
		public static void updateTempInfo(){
			var lowerBound = HeatMapControl.lowerBound;
			var maxTemp = HeatMapControl.upperBound;
			var modified = HeatMapControl.modified;
			var colorThreshold = HeatMapControl.colorThreshold;

			float lowest = colorThreshold[0].value;
			float highest = colorThreshold[colorThreshold.Length - 1].value;

			float step1 = (lowerBound - lowest) / (4);
			float step2 = (maxTemp - lowerBound) / (colorThreshold.Length - 5);


			//default to be coldest blue to green
			for(int i = 0; i < 3; i++){
				if(modified){
					float currentVal = colorThreshold[i].value;					
					float b = lowest + step1 * (i);
					SimDebugView.Instance.temperatureThresholds[i].value = b;
				}else{
					SimDebugView.Instance.temperatureThresholds[i] = colorThreshold[i];
				}
			}
			//green to orange
			for(int i = 3; i < colorThreshold.Length - 1; i++){	
				if(modified){
					float currentVal = colorThreshold[i].value;					
					float b = lowerBound + step2 * (i-3);
					SimDebugView.Instance.temperatureThresholds[i].value = b;
				}else{
					SimDebugView.Instance.temperatureThresholds[i] = colorThreshold[i];
				}
			}
		}	

		public void showUI(){
			if(!uiInited ){
				initUI();
			}
			if(RootPanel == null){
				Debug.Log("Root Panel null!");
				initUI();
			}
			
            RootPanel.SetParent(ToolMenu.Instance.gameObject);
            RootPanel.transform.SetAsFirstSibling();
            RootPanel.SetActive(true);
		}

		public void hideUI(){
            if(uiInited){
                RootPanel.SetActive(false);
            }
        }

		public void initUI(){

            //Initialize temperature infomation
			SimDebugView.ColorThreshold[] tmp = SimDebugView.Instance.temperatureThresholds;
			HeatMapControl.colorThreshold = new SimDebugView.ColorThreshold[tmp.Length];
			for (int i = 0; i<tmp.Length ; i++){
				HeatMapControl.colorThreshold[i] = tmp[i];
			}
			HeatMapControl.lowerBound = tmp[1].value;
			HeatMapControl.lowerBound = tmp[tmp.Length-2].value;	
           	
			this.lowerLabel = new PLabel("lowerLabel"){
            		Text = HeatMapStrings.UI_LOWERBOUND,
            		TextAlignment = TextAnchor.MiddleLeft,FlexSize = Vector2.right, Margin = ELEMENT_MARGIN,
					DynamicSize = true,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
            };
			this.upperLabel = new PLabel("upperLabel"){
            	Text = HeatMapStrings.UI_UPPERBOUND,
            	TextAlignment = TextAnchor.MiddleLeft,FlexSize = Vector2.right, Margin = ELEMENT_MARGIN,
				Sprite = PUITuning.Images.BoxBorder, SpriteMode = Image.Type.Sliced,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
            };
			// Scroll to select elements
			var sp = new PScrollPane("Scroll") {
				ScrollHorizontal = false, 
				ScrollVertical = true,
				AlwaysShowVertical = false, 
				TrackSize = 8.0f,
				FlexSize = Vector2.one
            };
            var insidePanel = new PPanel("bars"){
					Spacing = ROW_SPACING, Margin = new RectOffset(0, 0, 0, 0),
					Alignment = TextAnchor.UpperLeft,
            };
         	this.linkCheckBox = new PCheckBox("linkCheckBox"){
					Text = HeatMapStrings.UI_KEEP_TEMP_DIST,
					InitialState = PCheckBox.STATE_UNCHECKED,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
					OnChecked = OnCheck,
					CheckSize = ROW_SIZE,
					SpriteSize = ROW_SIZE,
			}.AddOnRealize(cbx => this.linkCheckBoxObj = cbx);
            
			insidePanel.AddChild( 
				this.lowerLabel
			)
			.AddChild(
                new PSliderSingle("lowerSlider"){
           		    InitialValue = lowerBound,
           		    OnValueChanged = onLowerChanged,
           		    MaxValue = colorThreshold[colorThreshold.Length-1].value,
           		    MinValue = colorThreshold[0].value,
           		    HandleSize = this.HandleSize,
           		    PreferredLength = PANEL_SIZE.x - 50,
					ToolTip = "{0} K"
           		}.AddOnRealize(slider => this.lowerSlider = slider)
            ).AddChild(
				this.upperLabel
			).AddChild(
            	new PSliderSingle("upperSlider"){
            	    InitialValue = upperBound,
            	    OnValueChanged = onUpperChanged,
            	    MaxValue = colorThreshold[colorThreshold.Length-1].value,
            	    MinValue = colorThreshold[0].value,
            	    HandleSize = this.HandleSize,
            	    PreferredLength = PANEL_SIZE.x - 40,
					ToolTip = "{0} K"
            	}.AddOnRealize(slider =>this.upperSlider = slider)
            ).AddChild(
				this.linkCheckBox
			).AddChild(	
				new PButton("resetButton"){
					Text = HeatMapStrings.UI_RESET,
					OnClick = OnReset,
				}.AddOnRealize(btn =>this.resetButton = btn)
			);
            sp.Child = insidePanel;
			// Title bar
			var title = new PLabel("Title") {
				TextAlignment = TextAnchor.MiddleCenter, Text = HeatMapStrings.UI_TITLE
                , FlexSize = Vector2.right, Margin = TITLE_MARGIN,
			}.SetKleiPinkColor();
			// 1px black border on the rest of the dialog for contrast
			RootPanel = new PRelativePanel("Border") {
				BackImage = PUITuning.Images.BoxBorder, ImageMode = Image.Type.Sliced,
				DynamicSize = false, BackColor = PUITuning.Colors.BackgroundLight
			}.AddChild(sp).AddChild(title).SetMargin(sp, OUTER_MARGIN).
				SetLeftEdge(title, fraction: 0.0f).SetRightEdge(title, fraction: 1.0f).
				SetLeftEdge(sp, fraction: 0.0f).SetRightEdge(sp, fraction: 1.0f).
				SetTopEdge(title, fraction: 1.0f).SetBottomEdge(sp, fraction: 0.0f).
				SetTopEdge(sp, below: title).Build();
			RootPanel.SetMinUISize(PANEL_SIZE);
			RootPanel.AddComponent<Canvas>();
			RootPanel.AddComponent<TypeSelectScreen>();
			RootPanel.SetActive(false);
			
            uiInited = true;
		}

		private string getLowerBound(){
			return lowerBound.ToString();
		}

		private string getUpperBound(){
			return upperBound.ToString();
		}

        private void onUpperChanged(GameObject source, float newValue){
            modified = true;
		
			var orig = upperBound;
			var step = upperBound - lowerBound;
            upperBound = newValue;
            upperLabel.Text = newValue.ToString();

			if(isLinked()){
 				lowerBound = newValue - step;
				PSliderSingle.SetCurrentValue(lowerSlider, lowerBound);
			}
            OverlayScreen.Instance.Refresh();
        }

        private void onLowerChanged(GameObject source, float newValue){
            modified = true;
			var orig = lowerBound;
			var step = upperBound - lowerBound;
            lowerBound = newValue;
			
			if(isLinked()){
 				upperBound = newValue + step;
				PSliderSingle.SetCurrentValue(upperSlider, upperBound);
			}
            OverlayScreen.Instance.Refresh();
        }

		private bool isLinked(){
			return this.linked;
		}

        private void OnReset(GameObject source){
			lowerBound = colorThreshold[1].value;
			lowerLabel.Text = lowerBound.ToString();
			PSliderSingle.SetCurrentValue(lowerSlider, lowerBound);

     		upperBound = colorThreshold[colorThreshold.Length-1].value;
			upperLabel.Text = upperBound.ToString();
			PSliderSingle.SetCurrentValue(upperSlider, upperBound);
			modified = false;
			updateTempInfo();
		}

		
        private void OnCheck(GameObject source, int state){
			this.linked = state == PCheckBox.STATE_UNCHECKED;
			PCheckBox.SetCheckState(linkCheckBoxObj, this.linked? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

		/// <summary>
		/// The screen type used for a type select control.
		/// </summary>
		private sealed class TypeSelectScreen : KScreen {
			public TypeSelectScreen() {
				activateOnSpawn = true;
				ConsumeMouseScroll = true;
			}
		}

		
    }
}
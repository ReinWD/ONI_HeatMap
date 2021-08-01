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
		internal const int ROW_SPACING = 2;


        public bool uiInited = false;

       	/// <summary>
		/// The root panel of the whole control.
		/// </summary>
		public GameObject RootPanel { get; set;}

		/// <summary>
		/// The "all items" checkbox.
		/// </summary>
		private GameObject allItems;

        private int HandleSize = 30;

        private GameObject upperSlider;
        private GameObject lowerSlider;

        private PLabel upperLabel;
        private PLabel lowerLabel;

		private GameObject resetButton;
		private PCheckBox linkCheckBox;
		private GameObject linkCheckBoxObj;

        public static float lowerBound = 273 + 125;
		public static float upperBound = 273 + 500;
		public static bool modified = false;
		public static SimDebugView.ColorThreshold[] colorThreshold;

		public void showUI(){
			if(!uiInited){
				initUI();
			}
            RootPanel.SetParent(ToolMenu.Instance.gameObject);
            RootPanel.transform.SetAsFirstSibling();
            RootPanel.SetActive(true);
			Debug.Log("HeatMapControl：open panel");
		}

		public void hideUI(){
            if(uiInited){
                RootPanel.SetActive(false);
			    Debug.Log("HeatMapControl：close Panel");
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
            		Text = lowerBound.ToString(),
            		TextAlignment = TextAnchor.MiddleLeft,FlexSize = Vector2.right, Margin = ELEMENT_MARGIN,
					DynamicSize = true,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
            };
			this.upperLabel = new PLabel("upperLabel"){
            	Text = upperBound.ToString(),
            	TextAlignment = TextAnchor.MiddleLeft,FlexSize = Vector2.right, Margin = ELEMENT_MARGIN,
				Sprite = PUITuning.Images.BoxBorder, SpriteMode = Image.Type.Sliced,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
            };
			// Scroll to select elements
			var sp = new PScrollPane("Scroll") {
				ScrollHorizontal = false, ScrollVertical = true,
				AlwaysShowVertical = true, TrackSize = 8.0f, FlexSize = Vector2.one
            };
            var insidePanel = new PPanel("bars"){
					Spacing = ROW_SPACING, Margin = new RectOffset(INDENT, 0, 0, 0),
					Alignment = TextAnchor.UpperLeft,
            };
         	this.linkCheckBox = new PCheckBox("linkCheckBox"){
					Text = "保持温度差",
					InitialState = PCheckBox.STATE_UNCHECKED,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
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
           		    PreferredLength = PANEL_SIZE.x - 40,
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
            	}.AddOnRealize(slider =>this.upperSlider = slider)
            ).AddChild(	
				new PButton("resetButton"){
					Text = "重置",
					OnClick = OnReset,
				}.AddOnRealize(btn =>this.resetButton = btn)
			).AddChild(
				this.linkCheckBox
			);
            sp.Child = insidePanel;
			// Title bar
			var title = new PLabel("Title") {
				TextAlignment = TextAnchor.MiddleCenter, Text = "温度控制"
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

        private void onUpperChanged(GameObject source, float newValue){
            modified = true;
		
			var orig = upperBound;
            upperBound = newValue;
            upperLabel.Text = newValue.ToString();

			if(isLinked()){
 				lowerBound = newValue + (orig - lowerBound);
				PSliderSingle.SetCurrentValue(lowerSlider, lowerBound);
            	lowerLabel.Text = newValue.ToString();
			}
            OverlayScreen.Instance.Refresh();
        }

        private void onLowerChanged(GameObject source, float newValue){
            modified = true;
			var orig = upperBound;
            lowerBound = newValue;
            lowerLabel.Text = newValue.ToString();

			if(isLinked()){
 				upperBound = newValue + (orig - upperBound);
				PSliderSingle.SetCurrentValue(upperSlider, upperBound);
            	upperLabel.Text = newValue.ToString();
			}
            OverlayScreen.Instance.Refresh();
        }

		private bool isLinked(){
			return PCheckBox.GetCheckState(linkCheckBoxObj) == PCheckBox.STATE_CHECKED;
		}

        private void OnReset(GameObject source){
			lowerBound = colorThreshold[1].value;
			lowerLabel.Text = lowerBound.ToString();
			PSliderSingle.SetCurrentValue(lowerSlider, lowerBound);

     		upperBound = colorThreshold[colorThreshold.Length-1].value;
			upperLabel.Text = upperBound.ToString();
			PSliderSingle.SetCurrentValue(upperSlider, upperBound);
			modified = false;
			
		}

		
        private void OnCheck(GameObject source, int state){

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
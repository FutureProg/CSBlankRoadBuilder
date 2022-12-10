using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util;
using BlankRoadBuilder.Util.MarkingStyles;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class FillerOptions : OptionsPanelBase
{
	public override string TabName { get; } = "Custom Fillers";

	public FillerOptions(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
	{
		foreach (var option in MarkingStyleUtil.CustomFillerMarkings)
		{
			AddFillerOption(option.Key, option.Value);
		}
	}

	private void AddFillerOption(LaneType key, SavedFillerOption value)
	{
		var button = _panel.AddUIComponent<SlickButton>();
		button.size = new Vector2(36, 36);
		button.relativePosition = new Vector2(_panel.width - 42, yPos);
		button.text = "Reset";
		button.tooltip = "Resets this filler to the vanilla setting";
		button.SetIcon("I_Undo.png");

		var topLabel = AddLabel($"{key} Lane");
		topLabel.textScale = 1.15F;
		topLabel.textColor = new Color32(138, 189, 235, 255);

		yPos += Margin;

		var xPos = LeftMargin;
		var enumVales = getEnumValues();

		var dropDown = UIDropDowns.AddDropDown(_panel, xPos, yPos, 150);
		dropDown.items = enumVales.Values.ToArray();
		dropDown.selectedIndex = enumVales.Keys.ToList().IndexOf((int)value.MarkingType);
		xPos += 150 + 2 * Margin;

		var dashSlider = AddSlider("Dash Length", xPos, 0.1F, 3F, 0.1F, value.DashLength, "m");
		dashSlider.eventValueChanged += (s, v) => value.DashLength = v;
		xPos += 160 + 2 * Margin;

		var dashSpace = AddSlider("Dash Space", xPos, 0.1F, 3F, 0.1F, value.DashSpace, "m");
		dashSpace.eventValueChanged += (s, v) => value.DashSpace = v;

		xPos = LeftMargin;
		yPos += 40;

		var color = _panel.AddUIComponent<UISprite>();

		color.relativePosition = new Vector2(xPos, yPos);
		color.size = new Vector2(16, 16);
		ChangeColor(color, value);

		xPos += 24;

		var rl = UILabels.AddLabel(_panel, xPos + 3, yPos + 3, "R", -1, 0.8F);
		xPos += 25;

		var r = UITextFields.AddTinyTextField(_panel, xPos, yPos, 120);
		r.text = value.R.ToString();
		r.eventTextChanged += (s, v) => { value.R = int.TryParse(v, out var i) ? Byte(i) : value.R; ChangeColor(color, value); };
		xPos += r.width + Margin;

		var gl = UILabels.AddLabel(_panel, xPos + 3, yPos + 3, "G", -1, 0.8F);
		xPos += 25;

		var g = UITextFields.AddTinyTextField(_panel, xPos, yPos, 120);
		g.text = value.G.ToString();
		g.eventTextChanged += (s, v) => { value.G = int.TryParse(v, out var i) ? Byte(i) : value.G; ChangeColor(color, value); };
		xPos += g.width + Margin;

		var bl = UILabels.AddLabel(_panel, xPos + 3, yPos + 3, "B", -1, 0.8F);
		xPos += 25;

		var b = UITextFields.AddTinyTextField(_panel, xPos, yPos, 120);
		b.text = value.B.ToString();
		b.eventTextChanged += (s, v) => { value.B = int.TryParse(v, out var i) ? Byte(i) : value.B; ChangeColor(color, value); };
		xPos += b.width + Margin;

		var al = UILabels.AddLabel(_panel, xPos + 3, yPos + 3, "A", -1, 0.8F);
		xPos += 25;

		var a = UITextFields.AddTinyTextField(_panel, xPos, yPos, 120);
		a.text = value.A.ToString();
		a.eventTextChanged += (s, v) => value.A = int.TryParse(v, out var i) ? Byte(i) : value.A;
		xPos += a.width + Margin;

		yPos += a.height + Margin;

		UISpacers.AddOptionsSpacer(_panel, LeftMargin, yPos + 6, 700);

		yPos += 2.5F * Margin;

		SetVisibility();

		static Dictionary<int, string> getEnumValues() => Enum.GetValues(typeof(MarkingFillerType)).Cast<MarkingFillerType>().ToDictionary(x => (int)x, x => x.ToString().FormatWords());

		button.eventClick += (s, e) => ResetFiller();

		dropDown.eventSelectedIndexChanged += (s, v) => { value.MarkingType = (MarkingFillerType)v; SetVisibility(); };

		void ResetFiller()
		{
			var vanilla = MarkingStylesTemplates.Vanilla_Fillers();

			if (vanilla.ContainsKey(value.LaneType))
				value.Set(vanilla[value.LaneType]);
			else
				value.Set(new Util.MarkingStyleUtil.FillerInfo());

			dropDown.selectedIndex = enumVales.Keys.ToList().IndexOf((int)value.MarkingType);
			dashSlider.value = value.DashLength;
			dashSpace.value = value.DashSpace;
			r.text = value.R.ToString();
			g.text = value.G.ToString();
			b.text = value.B.ToString();
			a.text = value.A.ToString();
		}

		void SetVisibility()
		{
			dashSlider.parent.isVisible = value.MarkingType == MarkingFillerType.Dashed;
			dashSpace.parent.isVisible = value.MarkingType == MarkingFillerType.Dashed;
			color.isVisible = r.isVisible = rl.isVisible = g.isVisible = gl.isVisible = b.isVisible = bl.isVisible = a.isVisible = al.isVisible = value.MarkingType == MarkingFillerType.Filled || value.MarkingType == MarkingFillerType.Dashed;
		}
	}

	private int Byte(int i) => Math.Max(0, Math.Min(255, i));

	private void ChangeColor(UISprite color, SavedFillerOption value)
	{
		var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		var texture = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false, linear: false);

		texture.SetPixel(0, 0, new Color(value.R / 255F, value.G / 255F, value.B / 255F));
		texture.Apply(false);

		atlas.name = Guid.NewGuid().ToString();
		atlas.material = UnityEngine.Object.Instantiate(UIView.GetAView().defaultAtlas.material);
		atlas.material.mainTexture = texture;
		atlas.AddSprite(new UITextureAtlas.SpriteInfo
		{
			name = "normal",
			texture = texture,
			region = new Rect(0f, 0f, 1f, 1f)
		});

		color.atlas = atlas;
		color.spriteName = "normal";
	}

	protected UISlider AddSlider(string labelKey, float xPos, float minValue, float maxValue, float step, float initialValue, string? measurementUnit = null)
	{
		var newSlider = UISliders.AddPlainSlider(_panel, xPos, yPos - 8F, "", minValue, maxValue, step, initialValue, 160);

		newSlider.parent.Find<UILabel>("Label").textScale = 0.8F;
		newSlider.parent.Find<UILabel>("Label").width *= 2;
		newSlider.parent.Find<UILabel>("Label").text = $"{labelKey} ( {initialValue:0.#####}{measurementUnit} )";

		newSlider.relativePosition = new Vector2(newSlider.relativePosition.x, newSlider.relativePosition.y - 12F);

		newSlider.eventValueChanged += (s, v) =>
		{
			s.parent.Find<UILabel>("Label").text = $"{labelKey} ( {v:0.#####}{measurementUnit} )";
		};

		return newSlider;
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class TrackChangesMarkups
{
	private RevisionType m_markupType;

	private WTextBody m_changedValue;

	private PointF m_position;

	private LayoutedWidget m_ltWidget;

	private float m_emptySpace;

	private WordDocument m_wordDocument;

	private float m_ballonYPosition;

	private bool m_isAligned;

	internal WordDocument Document => m_wordDocument;

	internal float BallonYPosition
	{
		get
		{
			return m_ballonYPosition;
		}
		set
		{
			m_ballonYPosition = value;
		}
	}

	internal RevisionType TypeOfMarkup
	{
		get
		{
			return m_markupType;
		}
		set
		{
			m_markupType = value;
		}
	}

	internal WTextBody ChangedValue
	{
		get
		{
			if (m_changedValue == null)
			{
				WSection wSection = new WSection(m_wordDocument);
				m_changedValue = wSection.Body;
			}
			return m_changedValue;
		}
		set
		{
			m_changedValue = value;
		}
	}

	internal PointF Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal LayoutedWidget LtWidget
	{
		get
		{
			return m_ltWidget;
		}
		set
		{
			m_ltWidget = value;
		}
	}

	internal float EmptySpace
	{
		get
		{
			return m_emptySpace;
		}
		set
		{
			m_emptySpace = value;
		}
	}

	internal bool IsAligned
	{
		get
		{
			return m_isAligned;
		}
		set
		{
			m_isAligned = value;
		}
	}

	internal TrackChangesMarkups(WordDocument wordDocument)
	{
		m_wordDocument = wordDocument;
	}

	internal string GetBalloonValueForMarkupType()
	{
		if (TypeOfMarkup != RevisionType.Deletions)
		{
			if (TypeOfMarkup != RevisionType.Formatting)
			{
				return "";
			}
			return "Formatted";
		}
		return "Deleted";
	}

	internal void DisplayBalloonValueCFormat(FontScriptType scriptType, Dictionary<int, object> newpropertyhash, WCharacterFormat characterformat, ref Dictionary<int, string> hierarchyOrder)
	{
		bool isTextureRead = false;
		bool isBackcolorRead = false;
		bool isForecolorRead = false;
		string shadingFormattingText = "";
		string[] shadingFormattings = new string[3];
		using Dictionary<int, object>.Enumerator enumerator = newpropertyhash.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Key)
			{
			case 1:
				if (!hierarchyOrder.ContainsKey(11) && characterformat.TextColor != Color.Empty)
				{
					hierarchyOrder.Add(11, "Font color:" + GetColorName(characterformat.TextColor));
				}
				break;
			case 2:
				if (!hierarchyOrder.ContainsKey(0))
				{
					hierarchyOrder.Add(0, characterformat.GetFontNameToRender(scriptType));
				}
				break;
			case 3:
				if (!hierarchyOrder.ContainsKey(2))
				{
					hierarchyOrder.Add(2, characterformat.FontSize.ToString(CultureInfo.InvariantCulture) + " pt");
				}
				break;
			case 4:
				if (!hierarchyOrder.ContainsKey(4))
				{
					hierarchyOrder.Add(4, "Bold");
				}
				break;
			case 5:
				if (!hierarchyOrder.ContainsKey(5))
				{
					hierarchyOrder.Add(5, "Italic");
				}
				break;
			case 6:
				if (!hierarchyOrder.ContainsKey(12))
				{
					hierarchyOrder.Add(12, "Strikethrough");
				}
				break;
			case 7:
				if (!hierarchyOrder.ContainsKey(10))
				{
					hierarchyOrder.Add(10, "Underline");
				}
				break;
			case 10:
				if (characterformat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SubScript && !hierarchyOrder.ContainsKey(15))
				{
					hierarchyOrder.Add(15, "Subscript");
				}
				if (characterformat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SuperScript && !hierarchyOrder.ContainsKey(14))
				{
					hierarchyOrder.Add(14, "Superscript");
				}
				break;
			case 14:
				if (!hierarchyOrder.ContainsKey(13))
				{
					hierarchyOrder.Add(13, "Double strikethrough");
				}
				break;
			case 59:
				if (!hierarchyOrder.ContainsKey(7))
				{
					hierarchyOrder.Add(7, "Bold");
				}
				break;
			case 60:
				if (!hierarchyOrder.ContainsKey(8))
				{
					hierarchyOrder.Add(8, "Italic");
				}
				break;
			case 61:
				if (!hierarchyOrder.ContainsKey(1))
				{
					hierarchyOrder.Add(1, characterformat.GetFontNameToRender(scriptType));
				}
				break;
			case 62:
				if (!hierarchyOrder.ContainsKey(3))
				{
					hierarchyOrder.Add(3, characterformat.FontSize.ToString(CultureInfo.InvariantCulture) + " pt");
				}
				break;
			case 63:
				if (!hierarchyOrder.ContainsKey(22))
				{
					hierarchyOrder.Add(22, "Highlight");
				}
				break;
			case 71:
				if (!hierarchyOrder.ContainsKey(23))
				{
					hierarchyOrder.Add(23, "Text Outline");
				}
				break;
			case 77:
			{
				isForecolorRead = true;
				string colorName2 = GetColorName(characterformat.ForeColor);
				ForecolorFormatting(colorName2, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isBackcolorRead);
				break;
			}
			case 90:
			{
				string colorName = GetColorName(characterformat.UnderlineColor);
				if (!hierarchyOrder.ContainsKey(10))
				{
					hierarchyOrder.Add(10, "Underline color:" + colorName);
				}
				break;
			}
			case 120:
				if (!hierarchyOrder.ContainsKey(28))
				{
					hierarchyOrder.Add(28, "Contextual Alternates");
				}
				break;
			case 121:
				if (characterformat.Ligatures != 0)
				{
					string text3 = "Ligatures: ";
					switch (characterformat.Ligatures)
					{
					case LigatureType.Standard:
						text3 += "Standard";
						break;
					case LigatureType.Contextual:
						text3 += "Contextual";
						break;
					case LigatureType.StandardContextual:
						text3 += "Standard + Contextual";
						break;
					case LigatureType.Historical:
						text3 += "Historical";
						break;
					case LigatureType.StandardHistorical:
						text3 += "Standard + Historical";
						break;
					case LigatureType.ContextualHistorical:
						text3 += "Contextual + Historical";
						break;
					case LigatureType.StandardContextualHistorical:
						text3 += "Standard + Contextual + Historical";
						break;
					case LigatureType.Discretional:
						text3 += "Discretional";
						break;
					case LigatureType.StandardDiscretional:
						text3 += "Standard + Discretional";
						break;
					case LigatureType.ContextualDiscretional:
						text3 += "Contextual + Discretional";
						break;
					case LigatureType.StandardContextualDiscretional:
						text3 += "Standard + Contextual +Discretional";
						break;
					case LigatureType.HistoricalDiscretional:
						text3 += "Historical + Discretional";
						break;
					case LigatureType.StandardHistoricalDiscretional:
						text3 += "Standard + Historical + Discretional";
						break;
					case LigatureType.ContextualHistoricalDiscretional:
						text3 += "Contextual + Historical + Discretional";
						break;
					case LigatureType.All:
						text3 += "All";
						break;
					}
					if (!hierarchyOrder.ContainsKey(25))
					{
						hierarchyOrder.Add(25, text3);
					}
				}
				break;
			case 122:
				if (characterformat.NumberForm != 0)
				{
					string text2 = "Number Forms: ";
					switch (characterformat.NumberForm)
					{
					case NumberFormType.Lining:
						text2 += "Lining";
						break;
					case NumberFormType.OldStyle:
						text2 += "Oldstyle";
						break;
					}
					if (!hierarchyOrder.ContainsKey(26))
					{
						hierarchyOrder.Add(26, text2);
					}
				}
				break;
			case 123:
				if (characterformat.NumberSpacing != 0)
				{
					string text = "Number Spacing: ";
					switch (characterformat.NumberSpacing)
					{
					case NumberSpacingType.Proportional:
						text += "Proportional";
						break;
					case NumberSpacingType.Tabular:
						text += "Tabular";
						break;
					}
					if (!hierarchyOrder.ContainsKey(27))
					{
						hierarchyOrder.Add(27, text);
					}
				}
				break;
			case 124:
			{
				string value2 = "Stylistic Set " + (int)characterformat.StylisticSet;
				if (!hierarchyOrder.ContainsKey(29))
				{
					hierarchyOrder.Add(29, value2);
				}
				break;
			}
			case 125:
				if (!hierarchyOrder.ContainsKey(21))
				{
					hierarchyOrder.Add(21, "Kern at " + characterformat.Kern.ToString(CultureInfo.InvariantCulture) + " pt");
				}
				break;
			case 9:
			{
				isBackcolorRead = true;
				string colorName3 = GetColorName(characterformat.TextBackgroundColor);
				BackgroundColorFormatting(colorName3, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isForecolorRead);
				break;
			}
			case 78:
			{
				isTextureRead = true;
				string textureStyleText = GetTextureStyleText(characterformat.TextureStyle);
				TextureStyleFormatting(textureStyleText, hierarchyOrder, shadingFormattings, shadingFormattingText, isBackcolorRead, isForecolorRead);
				break;
			}
			case 17:
				if (characterformat.Position < 0f)
				{
					if (!hierarchyOrder.ContainsKey(21))
					{
						hierarchyOrder.Add(21, "Lowered by " + Math.Abs(characterformat.Position).ToString(CultureInfo.InvariantCulture) + " pt");
					}
				}
				else if (!hierarchyOrder.ContainsKey(21))
				{
					hierarchyOrder.Add(21, "Raised by " + characterformat.Position.ToString(CultureInfo.InvariantCulture) + "pt");
				}
				break;
			case 18:
				if (characterformat.CharacterSpacing < 0f)
				{
					hierarchyOrder.Add(20, "Condensed  by " + Math.Abs(characterformat.CharacterSpacing).ToString(CultureInfo.InvariantCulture) + " pt");
				}
				else
				{
					hierarchyOrder.Add(20, "Expanded  by " + characterformat.CharacterSpacing.ToString(CultureInfo.InvariantCulture) + " pt");
				}
				break;
			case 53:
				if (!hierarchyOrder.ContainsKey(18))
				{
					hierarchyOrder.Add(18, "Hidden");
				}
				break;
			case 50:
				if (!hierarchyOrder.ContainsKey(24))
				{
					hierarchyOrder.Add(24, "Shadow");
				}
				break;
			case 51:
				if (!hierarchyOrder.ContainsKey(32))
				{
					hierarchyOrder.Add(32, "Emboss");
				}
				break;
			case 52:
				if (!hierarchyOrder.ContainsKey(33))
				{
					hierarchyOrder.Add(33, "Engrave");
				}
				break;
			case 54:
				if (!hierarchyOrder.ContainsKey(17))
				{
					hierarchyOrder.Add(17, "All caps");
				}
				break;
			case 55:
				if (!hierarchyOrder.ContainsKey(16))
				{
					hierarchyOrder.Add(16, "Small caps");
				}
				break;
			case 58:
				if (!hierarchyOrder.ContainsKey(34))
				{
					hierarchyOrder.Add(34, "Right- to- left");
				}
				break;
			case 73:
				if (!hierarchyOrder.ContainsKey(35))
				{
					hierarchyOrder.Add(35, GetDisplayNameOfLocale(characterformat.LocaleIdASCII));
				}
				break;
			case 75:
			{
				string value = "(Complex) " + GetDisplayNameOfLocale(characterformat.LocaleIdASCII);
				if (!hierarchyOrder.ContainsKey(36))
				{
					hierarchyOrder.Add(36, value);
				}
				break;
			}
			case 76:
				if (characterformat.NoProof && !hierarchyOrder.ContainsKey(37))
				{
					hierarchyOrder.Add(37, "Do not check spelling and grammar");
				}
				break;
			case 79:
				if (characterformat.EmphasisType == EmphasisType.NoEmphasis)
				{
					break;
				}
				switch (characterformat.EmphasisType)
				{
				case EmphasisType.Circle:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Strikethrough");
					}
					break;
				case EmphasisType.Comma:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Comma");
					}
					break;
				case EmphasisType.Dot:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Dot");
					}
					break;
				case EmphasisType.UnderDot:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Outline");
					}
					break;
				}
				break;
			case 127:
				if (!hierarchyOrder.ContainsKey(19))
				{
					hierarchyOrder.Add(19, "Character scale: " + characterformat.Scaling.ToString(CultureInfo.InvariantCulture) + "%");
				}
				break;
			case 80:
				if (characterformat.TextEffect == TextEffect.None)
				{
					break;
				}
				switch (characterformat.TextEffect)
				{
				case TextEffect.MarchingBlackAnts:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Marching Black Ants");
					}
					break;
				case TextEffect.MarchingRedAnts:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Marching Red Ants");
					}
					break;
				case TextEffect.BlinkingBackground:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Blinking Background");
					}
					break;
				case TextEffect.LasVegasLights:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Las Vegas Lights");
					}
					break;
				case TextEffect.Shimmer:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Shimmer");
					}
					break;
				case TextEffect.SparkleText:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Sparkle Text");
					}
					break;
				}
				break;
			}
		}
	}

	internal void DisplayBalloonValueforRemovedCFormat(Dictionary<int, object> newpropertyhash, WCharacterFormat characterformat, ref Dictionary<int, string> hierarchyOrder)
	{
		string[] shadingFormattings = new string[3];
		bool isTextureRead = false;
		bool isBackcolorRead = false;
		bool isForecolorRead = false;
		string shadingFormattingText = "";
		using Dictionary<int, object>.Enumerator enumerator = newpropertyhash.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Key)
			{
			case 4:
				if (!hierarchyOrder.ContainsKey(4))
				{
					hierarchyOrder.Add(4, "Not Bold");
				}
				break;
			case 5:
				if (!hierarchyOrder.ContainsKey(5))
				{
					hierarchyOrder.Add(5, "Not Italic");
				}
				break;
			case 6:
				if (!hierarchyOrder.ContainsKey(12))
				{
					hierarchyOrder.Add(12, "Not Strikethrough");
				}
				break;
			case 7:
				if (!hierarchyOrder.ContainsKey(10))
				{
					hierarchyOrder.Add(10, "No underline");
				}
				break;
			case 10:
				if (characterformat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None && !hierarchyOrder.ContainsKey(15))
				{
					hierarchyOrder.Add(15, "Not Superscript/ Subscript");
				}
				break;
			case 14:
				if (!hierarchyOrder.ContainsKey(13))
				{
					hierarchyOrder.Add(13, "Not Double strikethrough");
				}
				break;
			case 59:
				if (!hierarchyOrder.ContainsKey(7))
				{
					hierarchyOrder.Add(7, "Complex: Not Bold");
				}
				break;
			case 60:
				if (!hierarchyOrder.ContainsKey(8))
				{
					hierarchyOrder.Add(8, "Not Italic");
				}
				break;
			case 63:
				if (!hierarchyOrder.ContainsKey(22))
				{
					hierarchyOrder.Add(22, "Not Highlight");
				}
				break;
			case 120:
				if (!hierarchyOrder.ContainsKey(28))
				{
					hierarchyOrder.Add(28, "Not Contextual Alternates");
				}
				break;
			case 77:
			{
				isForecolorRead = true;
				string colorName2 = GetColorName(characterformat.ForeColor);
				ForecolorFormatting(colorName2, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isBackcolorRead);
				break;
			}
			case 9:
			{
				isBackcolorRead = true;
				string colorName = GetColorName(characterformat.TextBackgroundColor);
				BackgroundColorFormatting(colorName, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isForecolorRead);
				break;
			}
			case 78:
			{
				isTextureRead = true;
				string textureStyleText = GetTextureStyleText(characterformat.TextureStyle);
				TextureStyleFormatting(textureStyleText, hierarchyOrder, shadingFormattings, shadingFormattingText, isBackcolorRead, isForecolorRead);
				break;
			}
			case 17:
				if (!hierarchyOrder.ContainsKey(21))
				{
					hierarchyOrder.Add(21, "Not Raised by / Lowered by");
				}
				break;
			case 18:
				if (!hierarchyOrder.ContainsKey(20))
				{
					hierarchyOrder.Add(20, "Not Expanded by/ Condensed by");
				}
				break;
			case 53:
				if (!hierarchyOrder.ContainsKey(18))
				{
					hierarchyOrder.Add(18, "Not Hidden");
				}
				break;
			case 50:
				if (!hierarchyOrder.ContainsKey(24))
				{
					hierarchyOrder.Add(24, "Not Shadow");
				}
				break;
			case 51:
				if (!hierarchyOrder.ContainsKey(32))
				{
					hierarchyOrder.Add(32, "Not Emboss");
				}
				break;
			case 52:
				if (!hierarchyOrder.ContainsKey(33))
				{
					hierarchyOrder.Add(33, "Not Engrave");
				}
				break;
			case 54:
				if (!hierarchyOrder.ContainsKey(17))
				{
					hierarchyOrder.Add(17, "Not All caps");
				}
				break;
			case 55:
				if (!hierarchyOrder.ContainsKey(16))
				{
					hierarchyOrder.Add(16, "Not Small caps");
				}
				break;
			case 58:
				if (!hierarchyOrder.ContainsKey(34))
				{
					hierarchyOrder.Add(34, "Left-to-right");
				}
				break;
			case 76:
				if (!characterformat.NoProof && !hierarchyOrder.ContainsKey(37))
				{
					hierarchyOrder.Add(37, "Check spelling and grammar");
				}
				break;
			case 123:
			{
				string text3 = "Number Spacing: ";
				switch (characterformat.NumberSpacing)
				{
				case NumberSpacingType.Proportional:
					text3 += "Proportional";
					break;
				case NumberSpacingType.Tabular:
					text3 += "Tabular";
					break;
				case NumberSpacingType.Default:
					text3 += "Default";
					break;
				}
				if (!hierarchyOrder.ContainsKey(27))
				{
					hierarchyOrder.Add(27, text3);
				}
				break;
			}
			case 121:
			{
				string text2 = "Ligatures: ";
				switch (characterformat.Ligatures)
				{
				case LigatureType.None:
					text2 += "None";
					break;
				case LigatureType.Standard:
					text2 += "Standard";
					break;
				case LigatureType.Contextual:
					text2 += "Contextual";
					break;
				case LigatureType.StandardContextual:
					text2 += "Standard + Contextual";
					break;
				case LigatureType.Historical:
					text2 += "Historical";
					break;
				case LigatureType.StandardHistorical:
					text2 += "Standard + Historical";
					break;
				case LigatureType.ContextualHistorical:
					text2 += "Contextual + Historical";
					break;
				case LigatureType.StandardContextualHistorical:
					text2 += "Standard + Contextual + Historical";
					break;
				case LigatureType.Discretional:
					text2 += "Discretional";
					break;
				case LigatureType.StandardDiscretional:
					text2 += "Standard + Discretional";
					break;
				case LigatureType.ContextualDiscretional:
					text2 += "Contextual + Discretional";
					break;
				case LigatureType.StandardContextualDiscretional:
					text2 += "Standard + Contextual +Discretional";
					break;
				case LigatureType.HistoricalDiscretional:
					text2 += "Historical + Discretional";
					break;
				case LigatureType.StandardHistoricalDiscretional:
					text2 += "Standard + Historical + Discretional";
					break;
				case LigatureType.ContextualHistoricalDiscretional:
					text2 += "Contextual + Historical + Discretional";
					break;
				case LigatureType.All:
					text2 += "All";
					break;
				}
				if (!hierarchyOrder.ContainsKey(25))
				{
					hierarchyOrder.Add(25, text2);
				}
				break;
			}
			case 79:
				switch (characterformat.EmphasisType)
				{
				case EmphasisType.NoEmphasis:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "No emphasis mark");
					}
					break;
				case EmphasisType.Circle:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Strikethrough");
					}
					break;
				case EmphasisType.Comma:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Comma");
					}
					break;
				case EmphasisType.Dot:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Dot");
					}
					break;
				case EmphasisType.UnderDot:
					if (!hierarchyOrder.ContainsKey(38))
					{
						hierarchyOrder.Add(38, "Outline");
					}
					break;
				}
				break;
			case 124:
				if (characterformat.StylisticSet != 0)
				{
					string value = "Stylistic Set: " + (int)characterformat.StylisticSet;
					if (!hierarchyOrder.ContainsKey(29))
					{
						hierarchyOrder.Add(29, value);
					}
				}
				break;
			case 122:
			{
				string text = "Number Forms: ";
				switch (characterformat.NumberForm)
				{
				case NumberFormType.Default:
					text += "Default";
					break;
				case NumberFormType.Lining:
					text += "Lining";
					break;
				case NumberFormType.OldStyle:
					text += "Oldstyle";
					break;
				}
				if (!hierarchyOrder.ContainsKey(26))
				{
					hierarchyOrder.Add(26, text);
				}
				break;
			}
			case 71:
				if (!hierarchyOrder.ContainsKey(23))
				{
					hierarchyOrder.Add(23, "Text Outline");
				}
				break;
			case 80:
				switch (characterformat.TextEffect)
				{
				case TextEffect.None:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "None");
					}
					break;
				case TextEffect.MarchingBlackAnts:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Marching Black Ants");
					}
					break;
				case TextEffect.MarchingRedAnts:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Marching Red Ants");
					}
					break;
				case TextEffect.BlinkingBackground:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Blinking Background");
					}
					break;
				case TextEffect.LasVegasLights:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Las Vegas Lights");
					}
					break;
				case TextEffect.Shimmer:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Shimmer");
					}
					break;
				case TextEffect.SparkleText:
					if (!hierarchyOrder.ContainsKey(39))
					{
						hierarchyOrder.Add(39, "Sparkle Text");
					}
					break;
				}
				break;
			}
		}
	}

	private string GetDisplayNameOfLocale(short localeIdASCII)
	{
		string text = "";
		if (Enum.IsDefined(typeof(LocaleIDs), (int)localeIdASCII))
		{
			LocaleIDs localeIDs = (LocaleIDs)localeIdASCII;
			text = localeIDs.ToString().Replace("_", "-");
		}
		if (!string.IsNullOrEmpty(text))
		{
			return new CultureInfo(text).DisplayName;
		}
		return "";
	}

	private string GetTextureStyleText(TextureStyle textureStyle)
	{
		float num = 0f;
		return textureStyle switch
		{
			TextureStyle.Texture5Percent => "5%", 
			TextureStyle.Texture10Percent => "10%", 
			TextureStyle.Texture12Pt5Percent => 12.5f.ToString(CultureInfo.InvariantCulture) + "%", 
			TextureStyle.Texture15Percent => "15%", 
			TextureStyle.Texture20Percent => "20%", 
			TextureStyle.Texture25Percent => "25%", 
			TextureStyle.Texture30Percent => "30%", 
			TextureStyle.Texture35Percent => "35%", 
			TextureStyle.Texture37Pt5Percent => 37.5f.ToString(CultureInfo.InvariantCulture) + "%", 
			TextureStyle.Texture40Percent => "40%", 
			TextureStyle.Texture45Percent => "45%", 
			TextureStyle.Texture50Percent => "50%", 
			TextureStyle.Texture55Percent => "55%", 
			TextureStyle.Texture60Percent => "60%", 
			TextureStyle.Texture62Pt5Percent => 62.5f.ToString(CultureInfo.InvariantCulture) + "%", 
			TextureStyle.Texture65Percent => "65%", 
			TextureStyle.Texture70Percent => "70%", 
			TextureStyle.Texture75Percent => "75%", 
			TextureStyle.Texture80Percent => "80%", 
			TextureStyle.Texture85Percent => "85%", 
			TextureStyle.Texture87Pt5Percent => 87.5f.ToString(CultureInfo.InvariantCulture) + "%", 
			TextureStyle.Texture90Percent => "90%", 
			TextureStyle.Texture95Percent => "95%", 
			TextureStyle.TextureCross => "Lt Grid", 
			TextureStyle.TextureDarkCross => "Dk Grid", 
			TextureStyle.TextureDarkDiagonalCross => "Dk Trellis", 
			TextureStyle.TextureDarkDiagonalDown => "Dk Dwn Diagonal", 
			TextureStyle.TextureDarkDiagonalUp => "Dk Up Diagonal", 
			TextureStyle.TextureDarkHorizontal => "DK Horizontal", 
			TextureStyle.TextureDarkVertical => "Dk Vertical", 
			TextureStyle.TextureDiagonalCross => "Lt Trellis", 
			TextureStyle.TextureDiagonalDown => "Lt Dwn Diagonal", 
			TextureStyle.TextureDiagonalUp => "Lt Up Diagonal", 
			TextureStyle.TextureHorizontal => "Lt Horizontal", 
			TextureStyle.TextureSolid => "Solid (100%)", 
			TextureStyle.TextureVertical => "Lt Vertical", 
			_ => "Clear", 
		};
	}

	private string GenerateShadingFormattingText(string textureStyle, string foreColor, string textBgColor)
	{
		string result = "";
		if (textureStyle != null)
		{
			if (textureStyle.Equals("Clear", StringComparison.OrdinalIgnoreCase))
			{
				result = ((!string.IsNullOrEmpty(foreColor) || string.IsNullOrEmpty(textBgColor)) ? ("Pattern:" + textureStyle) : ("Pattern: " + textureStyle + "(" + textBgColor + ")"));
			}
			else if (string.IsNullOrEmpty(foreColor) && string.IsNullOrEmpty(textBgColor))
			{
				result = "Pattern: " + textureStyle;
			}
			else if (!string.IsNullOrEmpty(foreColor) && !string.IsNullOrEmpty(textBgColor))
			{
				result = "Pattern: " + textureStyle + "(" + foreColor + "Foreground," + textBgColor + "Background)";
			}
			else if (string.IsNullOrEmpty(foreColor) && !string.IsNullOrEmpty(textBgColor))
			{
				result = "Pattern: " + textureStyle + "( Auto Foreground," + textBgColor + "Background)";
			}
			else if (!string.IsNullOrEmpty(foreColor) && string.IsNullOrEmpty(textBgColor))
			{
				result = "Pattern: " + textureStyle + "(" + foreColor + "Foreground,Auto Background)";
			}
		}
		return result;
	}

	private string GetColorName(Color colorValue)
	{
		switch (colorValue.Name.ToLower())
		{
		case "ffc00000":
			return "Dark Red";
		case "ffff0000":
			return "Red";
		case "ffffc000":
			return "Orange";
		case "ffffff00":
			return "Yellow";
		case "ff92d050":
			return "Light Green";
		case "ff00b050":
			return "Green";
		case "ff00b0f0":
			return "Light Blue";
		case "ff0070c0":
			return "Blue";
		case "ff002060":
			return "Dark Blue";
		case "ff7030a0":
			return "Purple";
		default:
			if (!(colorValue != Color.Empty))
			{
				return "";
			}
			return "Custom Color(RGB(" + colorValue.R + "," + colorValue.G + "," + colorValue.B + "))";
		}
	}

	internal void DisplayBalloonValueForPFormat(Dictionary<int, object> newpropertyhash, WParagraphFormat paragraphFormat, ref Dictionary<int, string> hierarchyOrder)
	{
		string[] shadingFormattings = new string[3];
		string shadingFormattingText = "";
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool isTextureRead = false;
		bool isBackcolorRead = false;
		bool isForecolorRead = false;
		using Dictionary<int, object>.Enumerator enumerator = newpropertyhash.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Key)
			{
			case 0:
				GenerateAlignmentFormattingText(paragraphFormat, hierarchyOrder);
				break;
			case 6:
				if (!hierarchyOrder.ContainsKey(9))
				{
					hierarchyOrder.Add(9, "Keep lines together");
				}
				break;
			case 10:
				if (!hierarchyOrder.ContainsKey(10))
				{
					hierarchyOrder.Add(10, "Keep with next");
				}
				break;
			case 11:
				if (!hierarchyOrder.ContainsKey(11))
				{
					hierarchyOrder.Add(11, "No widow/orphan control");
				}
				break;
			case 12:
				if (!hierarchyOrder.ContainsKey(12))
				{
					hierarchyOrder.Add(12, "Page break before");
				}
				break;
			case 32:
			{
				isForecolorRead = true;
				string colorName = GetColorName(paragraphFormat.ForeColor);
				ForecolorFormatting(colorName, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isBackcolorRead);
				break;
			}
			case 21:
			{
				isBackcolorRead = true;
				string colorName2 = GetColorName(paragraphFormat.BackColor);
				BackgroundColorFormatting(colorName2, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isForecolorRead);
				break;
			}
			case 33:
			{
				isTextureRead = true;
				string textureStyleText = GetTextureStyleText(paragraphFormat.TextureStyle);
				TextureStyleFormatting(textureStyleText, hierarchyOrder, shadingFormattings, shadingFormattingText, isBackcolorRead, isForecolorRead);
				break;
			}
			case 31:
				if (!hierarchyOrder.ContainsKey(16))
				{
					hierarchyOrder.Add(16, "Right- to- left");
				}
				break;
			case 81:
				if (!paragraphFormat.AutoSpaceDE && !hierarchyOrder.ContainsKey(17))
				{
					hierarchyOrder.Add(17, "Don't adjust space between Latin and Asian text");
				}
				break;
			case 82:
				if (!paragraphFormat.AutoSpaceDN && !hierarchyOrder.ContainsKey(18))
				{
					hierarchyOrder.Add(18, "Don't adjust space between Asian text and numbers");
				}
				break;
			case 92:
				if (!hierarchyOrder.ContainsKey(19))
				{
					hierarchyOrder.Add(19, "Don't add space between paragraphs of the same style");
				}
				break;
			case 78:
				if (paragraphFormat.SuppressAutoHyphens && !hierarchyOrder.ContainsKey(20))
				{
					hierarchyOrder.Add(20, "Don't hyphenate");
				}
				break;
			case 75:
				if (!hierarchyOrder.ContainsKey(22))
				{
					hierarchyOrder.Add(22, "Don't swap indents on facing pages");
				}
				break;
			case 34:
				if (paragraphFormat.BaseLineAlignment != BaseLineAlignment.Auto && !hierarchyOrder.ContainsKey(23))
				{
					hierarchyOrder.Add(23, "Font Alignment: " + paragraphFormat.BaseLineAlignment);
				}
				break;
			case 37:
				switch (paragraphFormat.TextboxTightWrap)
				{
				case TextboxTightWrapOptions.AllLines:
					if (!hierarchyOrder.ContainsKey(24))
					{
						hierarchyOrder.Add(24, "Textbox Tight Wrap: Heading will be collapsed by default on open");
					}
					break;
				case TextboxTightWrapOptions.FirstAndLastLine:
					if (!hierarchyOrder.ContainsKey(25))
					{
						hierarchyOrder.Add(25, "Textbox Tight Wrap: All");
					}
					break;
				case TextboxTightWrapOptions.FirstLineOnly:
					if (!hierarchyOrder.ContainsKey(26))
					{
						hierarchyOrder.Add(26, "Textbox Tight Wrap: First and last lines");
					}
					break;
				case TextboxTightWrapOptions.LastLineOnly:
					if (!hierarchyOrder.ContainsKey(27))
					{
						hierarchyOrder.Add(27, "Textbox Tight Wrap: First line only");
					}
					break;
				}
				break;
			case 38:
				if (!hierarchyOrder.ContainsKey(28))
				{
					hierarchyOrder.Add(28, "Suppress line numbers");
				}
				break;
			case 41:
				if (!hierarchyOrder.ContainsKey(29))
				{
					hierarchyOrder.Add(29, "Don\ufffdt allow hanging punctuation");
				}
				break;
			case 42:
				if (!hierarchyOrder.ContainsKey(30))
				{
					hierarchyOrder.Add(30, "Compress initial punctuation");
				}
				break;
			case 30:
			{
				if (paragraphFormat.Tabs.Count <= 0)
				{
					break;
				}
				string text = "Tab stops: ";
				for (int i = 0; i < paragraphFormat.Tabs.Count; i++)
				{
					Tab tab = paragraphFormat.Tabs[i];
					text = text + tab.Position.ToString(CultureInfo.InvariantCulture) + " pt, " + GetTabAlignmentText(tab.Justification);
					if (tab.TabLeader != 0)
					{
						text += ", Leader: ...";
					}
					if (i != paragraphFormat.Tabs.Count - 1)
					{
						text += " + ";
					}
				}
				if (!hierarchyOrder.ContainsKey(31))
				{
					hierarchyOrder.Add(31, text);
				}
				break;
			}
			case 2:
			case 3:
			case 5:
			case 85:
			case 86:
			case 87:
				if (!flag)
				{
					if (!hierarchyOrder.ContainsKey(32))
					{
						hierarchyOrder.Add(32, "Indent: " + GenerateIndentsFormattingText(newpropertyhash, paragraphFormat));
					}
					flag = true;
				}
				break;
			case 8:
			case 9:
			case 54:
			case 55:
			case 90:
			case 91:
				if (!flag2)
				{
					if (!hierarchyOrder.ContainsKey(33))
					{
						hierarchyOrder.Add(33, "Space " + GetSpacingChangesText(newpropertyhash, paragraphFormat));
					}
					flag2 = true;
				}
				break;
			case 52:
			case 53:
				if (!hierarchyOrder.ContainsKey(34))
				{
					hierarchyOrder.Add(34, "Line Spacing: " + ((paragraphFormat.LineSpacingRule == LineSpacingRule.AtLeast) ? "At least" : paragraphFormat.LineSpacingRule.ToString()) + " " + paragraphFormat.LineSpacing.ToString(CultureInfo.InvariantCulture) + " pt");
				}
				break;
			case 56:
				if (!hierarchyOrder.ContainsKey(35))
				{
					hierarchyOrder.Add(35, "Outline: " + paragraphFormat.OutlineLevel);
				}
				break;
			case 20:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 66:
			case 67:
			case 93:
			case 94:
				if (!flag4)
				{
					string borderChangesText = GetBorderChangesText(paragraphFormat);
					if (borderChangesText != "" && !hierarchyOrder.ContainsKey(36))
					{
						hierarchyOrder.Add(36, "Border: " + borderChangesText);
					}
					flag4 = true;
				}
				break;
			case 39:
			case 71:
			case 72:
			case 73:
			case 74:
			case 76:
			case 77:
			case 83:
			case 84:
			case 88:
				if (!flag3)
				{
					if (!hierarchyOrder.ContainsKey(115))
					{
						hierarchyOrder.Add(115, GetFrameFormattingText(newpropertyhash, paragraphFormat));
					}
					flag3 = true;
				}
				break;
			}
		}
	}

	private void GenerateAlignmentFormattingText(WParagraphFormat paragraphFormat, Dictionary<int, string> hierarchyOrder)
	{
		switch (paragraphFormat.LogicalJustification)
		{
		case HorizontalAlignment.Center:
			if (!hierarchyOrder.ContainsKey(112))
			{
				hierarchyOrder.Add(112, "Centered");
			}
			break;
		case HorizontalAlignment.Left:
			if (!hierarchyOrder.ContainsKey(137))
			{
				hierarchyOrder.Add(137, "Left");
			}
			break;
		case HorizontalAlignment.Right:
			if (!hierarchyOrder.ContainsKey(2))
			{
				hierarchyOrder.Add(2, "Right");
			}
			break;
		case HorizontalAlignment.Justify:
			if (!hierarchyOrder.ContainsKey(3))
			{
				hierarchyOrder.Add(3, "Justified");
			}
			break;
		case HorizontalAlignment.Distribute:
			if (!hierarchyOrder.ContainsKey(4))
			{
				hierarchyOrder.Add(4, "Distributed");
			}
			break;
		case HorizontalAlignment.JustifyMedium:
			if (!hierarchyOrder.ContainsKey(5))
			{
				hierarchyOrder.Add(5, "Justify Medium");
			}
			break;
		case HorizontalAlignment.JustifyHigh:
			if (!hierarchyOrder.ContainsKey(6))
			{
				hierarchyOrder.Add(6, "Justify High");
			}
			break;
		case HorizontalAlignment.JustifyLow:
			if (!hierarchyOrder.ContainsKey(7))
			{
				hierarchyOrder.Add(7, "Justify Low");
			}
			break;
		case HorizontalAlignment.ThaiJustify:
			if (!hierarchyOrder.ContainsKey(8))
			{
				hierarchyOrder.Add(8, "Thai Distributed Justification");
			}
			break;
		case (HorizontalAlignment)6:
			break;
		}
	}

	internal void DisplayBalloonValueForRemovedPFormat(Dictionary<int, object> newpropertyhash, WParagraphFormat paragraphFormat, ref Dictionary<int, string> hierarchyOrder)
	{
		string[] shadingFormattings = new string[3];
		string shadingFormattingText = "";
		bool flag = false;
		bool flag2 = false;
		bool isTextureRead = false;
		bool isBackcolorRead = false;
		bool isForecolorRead = false;
		using Dictionary<int, object>.Enumerator enumerator = newpropertyhash.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Key)
			{
			case 0:
				GenerateAlignmentFormattingText(paragraphFormat, hierarchyOrder);
				break;
			case 6:
				if (!hierarchyOrder.ContainsKey(39))
				{
					hierarchyOrder.Add(39, "Don't Keep lines together");
				}
				break;
			case 10:
				if (!hierarchyOrder.ContainsKey(39))
				{
					hierarchyOrder.Add(39, "Don't Keep with next");
				}
				break;
			case 11:
				if (!hierarchyOrder.ContainsKey(102))
				{
					hierarchyOrder.Add(102, "Widow/orphan control");
				}
				break;
			case 12:
				if (!hierarchyOrder.ContainsKey(103))
				{
					hierarchyOrder.Add(103, "No Page break before");
				}
				break;
			case 31:
				if (!hierarchyOrder.ContainsKey(104))
				{
					hierarchyOrder.Add(104, "Left-to-right");
				}
				break;
			case 81:
				if (!paragraphFormat.AutoSpaceDE && !hierarchyOrder.ContainsKey(105))
				{
					hierarchyOrder.Add(105, "Adjust space between Latin and Asian text");
				}
				break;
			case 82:
				if (!paragraphFormat.AutoSpaceDN && !hierarchyOrder.ContainsKey(106))
				{
					hierarchyOrder.Add(106, "Adjust space between Asian text and numbers");
				}
				break;
			case 92:
				if (!hierarchyOrder.ContainsKey(107))
				{
					hierarchyOrder.Add(107, "Add space between paragraphs of the same style");
				}
				break;
			case 78:
				if (!paragraphFormat.SuppressAutoHyphens && !hierarchyOrder.ContainsKey(109))
				{
					hierarchyOrder.Add(109, "Hyphenate");
				}
				break;
			case 75:
				if (!hierarchyOrder.ContainsKey(110))
				{
					hierarchyOrder.Add(110, "Not Don't swap indents on facing pages");
				}
				break;
			case 38:
				if (!hierarchyOrder.ContainsKey(111))
				{
					hierarchyOrder.Add(111, "Don't Suppress line numbers");
				}
				break;
			case 41:
				if (!hierarchyOrder.ContainsKey(112))
				{
					hierarchyOrder.Add(112, "Allow hanging punctuation");
				}
				break;
			case 42:
				if (!hierarchyOrder.ContainsKey(113))
				{
					hierarchyOrder.Add(113, "Don't Compress initial punctuation");
				}
				break;
			case 30:
			{
				string text = "Tab stops:Not at ";
				for (int i = 0; i < paragraphFormat.Tabs.Count; i++)
				{
					Tab tab = paragraphFormat.Tabs[i];
					text = text + tab.Position.ToString(CultureInfo.InvariantCulture) + " pt ";
				}
				if (!hierarchyOrder.ContainsKey(114))
				{
					hierarchyOrder.Add(114, text);
				}
				break;
			}
			case 32:
			{
				isForecolorRead = true;
				string colorName2 = GetColorName(paragraphFormat.ForeColor);
				ForecolorFormatting(colorName2, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isBackcolorRead);
				break;
			}
			case 21:
			{
				isBackcolorRead = true;
				string colorName = GetColorName(paragraphFormat.BackColor);
				BackgroundColorFormatting(colorName, hierarchyOrder, shadingFormattings, shadingFormattingText, isTextureRead, isForecolorRead);
				break;
			}
			case 33:
			{
				isTextureRead = true;
				string textureStyleText = GetTextureStyleText(paragraphFormat.TextureStyle);
				TextureStyleFormatting(textureStyleText, hierarchyOrder, shadingFormattings, shadingFormattingText, isBackcolorRead, isForecolorRead);
				break;
			}
			case 37:
				switch (paragraphFormat.TextboxTightWrap)
				{
				case TextboxTightWrapOptions.AllLines:
					if (!hierarchyOrder.ContainsKey(24))
					{
						hierarchyOrder.Add(24, "Textbox Tight Wrap: Heading will be collapsed by default on open");
					}
					break;
				case TextboxTightWrapOptions.FirstAndLastLine:
					if (!hierarchyOrder.ContainsKey(25))
					{
						hierarchyOrder.Add(25, "Textbox Tight Wrap: All");
					}
					break;
				case TextboxTightWrapOptions.FirstLineOnly:
					if (!hierarchyOrder.ContainsKey(26))
					{
						hierarchyOrder.Add(26, "Textbox Tight Wrap: First and last lines");
					}
					break;
				case TextboxTightWrapOptions.LastLineOnly:
					if (!hierarchyOrder.ContainsKey(27))
					{
						hierarchyOrder.Add(27, "Textbox Tight Wrap: First line only");
					}
					break;
				case TextboxTightWrapOptions.None:
					if (!hierarchyOrder.ContainsKey(127))
					{
						hierarchyOrder.Add(127, "Textbox Tight Wrap: None");
					}
					break;
				}
				break;
			case 56:
				if (!hierarchyOrder.ContainsKey(35))
				{
					hierarchyOrder.Add(35, "Outline: " + paragraphFormat.OutlineLevel);
				}
				break;
			case 2:
			case 3:
			case 5:
			case 85:
			case 86:
			case 87:
				if (!flag)
				{
					if (!hierarchyOrder.ContainsKey(32))
					{
						hierarchyOrder.Add(32, "Indent: " + GenerateIndentsFormattingText(newpropertyhash, paragraphFormat));
					}
					flag = true;
				}
				break;
			case 39:
			case 71:
			case 72:
			case 73:
			case 74:
			case 76:
			case 77:
			case 83:
			case 84:
			case 88:
				if (!flag2)
				{
					if (!hierarchyOrder.ContainsKey(115))
					{
						hierarchyOrder.Add(115, GetFrameFormattingText(newpropertyhash, paragraphFormat));
					}
					flag2 = true;
				}
				break;
			}
		}
	}

	private void TextureStyleFormatting(string trackChangesTextureName, Dictionary<int, string> hierarchyOrder, string[] shadingFormattings, string shadingFormattingText, bool isBackcolorRead, bool isForecolorRead)
	{
		shadingFormattings[0] = trackChangesTextureName;
		if (isForecolorRead && isBackcolorRead)
		{
			shadingFormattingText = GenerateShadingFormattingText(shadingFormattings[0], shadingFormattings[1], shadingFormattings[2]);
		}
		if (!string.IsNullOrEmpty(shadingFormattingText) && !hierarchyOrder.ContainsKey(15))
		{
			hierarchyOrder.Add(15, shadingFormattingText);
		}
	}

	private void BackgroundColorFormatting(string trackChangesBackColorName, Dictionary<int, string> hierarchyOrder, string[] shadingFormattings, string shadingFormattingText, bool isTextureRead, bool isForecolorRead)
	{
		shadingFormattings[2] = trackChangesBackColorName;
		if (isTextureRead && isForecolorRead)
		{
			shadingFormattingText = GenerateShadingFormattingText(shadingFormattings[0], shadingFormattings[1], shadingFormattings[2]);
		}
		if (!string.IsNullOrEmpty(shadingFormattingText) && !hierarchyOrder.ContainsKey(14))
		{
			hierarchyOrder.Add(14, shadingFormattingText);
		}
	}

	private void ForecolorFormatting(string trackChangesForeColorName, Dictionary<int, string> hierarchyOrder, string[] shadingFormattings, string shadingFormattingText, bool isTextureRead, bool isBackcolorRead)
	{
		shadingFormattings[1] = trackChangesForeColorName;
		if (isTextureRead && isBackcolorRead)
		{
			shadingFormattingText = GenerateShadingFormattingText(shadingFormattings[0], shadingFormattings[1], shadingFormattings[2]);
		}
		if (!string.IsNullOrEmpty(shadingFormattingText) && !hierarchyOrder.ContainsKey(13))
		{
			hierarchyOrder.Add(13, shadingFormattingText);
		}
	}

	private string GetBorderChangesText(WParagraphFormat paragraphFormat)
	{
		List<string> list = new List<string>();
		string text = "";
		if (paragraphFormat.Borders.Top.LineWidth > 0f)
		{
			list.Add("Top:(" + paragraphFormat.Borders.Top.BorderType.ToString() + "," + GetColorName(paragraphFormat.Borders.Top.Color) + ", " + paragraphFormat.Borders.Top.LineWidth.ToString(CultureInfo.InvariantCulture) + " pt Line Width)");
		}
		if (paragraphFormat.Borders.Bottom.LineWidth > 0f)
		{
			list.Add("Bottom:(" + paragraphFormat.Borders.Bottom.BorderType.ToString() + ", " + GetColorName(paragraphFormat.Borders.Top.Color) + ", " + paragraphFormat.Borders.Bottom.LineWidth.ToString(CultureInfo.InvariantCulture) + " pt Line Width)");
		}
		if (paragraphFormat.Borders.Left.LineWidth > 0f)
		{
			list.Add("Left:(" + paragraphFormat.Borders.Left.BorderType.ToString() + ", " + GetColorName(paragraphFormat.Borders.Top.Color) + ", " + paragraphFormat.Borders.Left.LineWidth.ToString(CultureInfo.InvariantCulture) + " pt Line Width)");
		}
		if (paragraphFormat.Borders.Right.LineWidth > 0f)
		{
			list.Add("Right:(" + paragraphFormat.Borders.Right.BorderType.ToString() + ", " + GetColorName(paragraphFormat.Borders.Top.Color) + ", " + paragraphFormat.Borders.Right.LineWidth.ToString(CultureInfo.InvariantCulture) + " pt Line Width)");
		}
		if (paragraphFormat.Borders.Vertical.LineWidth > 0f)
		{
			list.Add("Bar:(" + paragraphFormat.Borders.Vertical.BorderType.ToString() + ", " + GetColorName(paragraphFormat.Borders.Top.Color) + ", " + paragraphFormat.Borders.Vertical.LineWidth.ToString(CultureInfo.InvariantCulture) + " pt Line Width)");
		}
		if (list.Count > 1)
		{
			for (int i = 0; i < list.Count - 1; i++)
			{
				text = text + list[i] + ", ";
			}
			return text;
		}
		if (list.Count != 0)
		{
			return list[0];
		}
		return "";
	}

	private string GetSpacingChangesText(Dictionary<int, object> newpropertyhash, WParagraphFormat paragraphFormat)
	{
		List<string> list = new List<string>();
		if (newpropertyhash.ContainsKey(54))
		{
			list.Add("Before Auto: " + paragraphFormat.SpaceBeforeAuto + " pt");
		}
		else if (newpropertyhash.ContainsKey(8))
		{
			list.Add("Before: " + paragraphFormat.BeforeSpacing.ToString(CultureInfo.InvariantCulture) + " pt");
		}
		else if (newpropertyhash.ContainsKey(90))
		{
			list.Add("Before: " + paragraphFormat.BeforeLines.ToString(CultureInfo.InvariantCulture) + " pt");
		}
		if (newpropertyhash.ContainsKey(55))
		{
			list.Add("Auto After: " + paragraphFormat.SpaceAfterAuto + " pt");
		}
		else if (newpropertyhash.ContainsKey(9))
		{
			list.Add("After: " + paragraphFormat.AfterSpacing.ToString(CultureInfo.InvariantCulture) + " pt");
		}
		else if (newpropertyhash.ContainsKey(91))
		{
			list.Add("After: " + paragraphFormat.AfterLines.ToString(CultureInfo.InvariantCulture) + " pt");
		}
		if (list.Count < 2)
		{
			return list[0];
		}
		return list[0] + "," + list[1];
	}

	private string GetFrameFormattingText(Dictionary<int, object> newpropertyhash, WParagraphFormat paragraphFormat)
	{
		string text = "";
		string[] array = new string[7];
		if (newpropertyhash.ContainsKey(71) || newpropertyhash.ContainsKey(72) || newpropertyhash.ContainsKey(73) || newpropertyhash.ContainsKey(74) || newpropertyhash.ContainsKey(83) || newpropertyhash.ContainsKey(84))
		{
			array[0] = "Position: ";
			if (newpropertyhash.ContainsKey(71) || newpropertyhash.ContainsKey(72))
			{
				ref string reference = ref array[0];
				reference = reference + "Horizontal: " + GetFrameXFormattedText(paragraphFormat.FrameX) + ", Relative to:" + ((FrameHorzAnchor)paragraphFormat.FrameHorizontalPos).ToString() + ", Vertical: " + GetFrameYFormattedText(paragraphFormat.FrameY) + ", Relative to:" + (FrameVertAnchor)paragraphFormat.FrameVerticalPos;
			}
			if (newpropertyhash.ContainsKey(83))
			{
				array[1] = "Horizontal: " + paragraphFormat.FrameHorizontalDistanceFromText + " pt";
			}
			if (newpropertyhash.ContainsKey(84))
			{
				array[2] = "Vertical: " + paragraphFormat.FrameVerticalDistanceFromText + " pt";
			}
		}
		if (newpropertyhash.ContainsKey(76) && paragraphFormat.FrameWidth != 0f)
		{
			array[3] = "Width: Exactly " + paragraphFormat.FrameWidth + " pt";
		}
		if (newpropertyhash.ContainsKey(77))
		{
			bool flag = ((ushort)Math.Round(paragraphFormat.FrameHeight * 20f) & 0x8000) != 0;
			string text2 = "";
			text2 = ((!flag) ? "Exact " : ((!(flag & (paragraphFormat.FrameHeight == 0f))) ? "At least " : ""));
			array[4] = ((!string.IsNullOrEmpty(text2)) ? ("Height: " + text2 + paragraphFormat.FrameHeight.ToString(CultureInfo.InvariantCulture) + " pt") : "");
		}
		if (newpropertyhash.ContainsKey(88))
		{
			array[5] = GetFrameWrappingFormmattedText(paragraphFormat.WrapFrameAround);
		}
		if (newpropertyhash.ContainsKey(39))
		{
			array[6] = (paragraphFormat.LockFrameAnchor ? "Lock Anchor" : "");
		}
		for (int i = 0; i < array.Length; i++)
		{
			text += ((i == array.Length - 1) ? ((!string.IsNullOrEmpty(array[i])) ? (array[i] + ", ") : "") : array[i]);
		}
		return text;
	}

	private string GetFrameWrappingFormmattedText(FrameWrapMode wrapFrameAround)
	{
		string result = "";
		switch (wrapFrameAround)
		{
		case FrameWrapMode.NotBeside:
			result = "No Wrapping";
			break;
		default:
			result = "";
			break;
		case FrameWrapMode.Around:
		case FrameWrapMode.Tight:
		case FrameWrapMode.Through:
			break;
		}
		return result;
	}

	private string GetFrameYFormattedText(float frameY)
	{
		if (frameY != -4f && frameY != -12f && frameY != -8f && frameY != -16f && frameY != -20f && frameY != 0f)
		{
			return frameY.ToString(CultureInfo.InvariantCulture);
		}
		return ((FrameVerticalPosition)frameY).ToString();
	}

	private string GetFrameXFormattedText(float frameX)
	{
		if (frameX != -8f && frameX != -4f && frameX != -12f && frameX != -16f && frameX != 0f)
		{
			return frameX.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		return ((PageNumberAlignment)frameX).ToString();
	}

	private string GenerateIndentsFormattingText(Dictionary<int, object> newpropertyhash, WParagraphFormat paragraphFormat)
	{
		string[] array = new string[6];
		string text = "";
		if (newpropertyhash.ContainsKey(2) && newpropertyhash.ContainsKey(5) && paragraphFormat.FirstLineIndent < 0f)
		{
			array[0] = "Before: " + (-1f * paragraphFormat.FirstLineIndent + paragraphFormat.LeftIndent).ToString(CultureInfo.InvariantCulture) + " pt";
		}
		else if (newpropertyhash.ContainsKey(2))
		{
			array[0] = "Left: " + paragraphFormat.LeftIndent.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		if (newpropertyhash.ContainsKey(5))
		{
			array[1] = ((paragraphFormat.FirstLineIndent < 0f) ? "Hanging " : ("First line: " + paragraphFormat.FirstLineIndent.ToString(CultureInfo.InvariantCulture) + " pt"));
		}
		if (newpropertyhash.ContainsKey(3))
		{
			array[2] = "Right: " + paragraphFormat.RightIndent.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		if (newpropertyhash.ContainsKey(85))
		{
			array[3] = "Left " + paragraphFormat.LeftIndentChars.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		if (newpropertyhash.ContainsKey(86))
		{
			array[4] = "First line:" + paragraphFormat.FirstLineIndentChars.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		if (newpropertyhash.ContainsKey(87))
		{
			array[5] = "Right " + paragraphFormat.RightIndentChars.ToString(CultureInfo.InvariantCulture) + " pt";
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]))
			{
				text += ((i == array.Length - 1) ? array[i] : (array[i] + ","));
			}
		}
		return text;
	}

	private string GetTabAlignmentText(DocGen.DocIO.DLS.TabJustification justification)
	{
		switch (justification)
		{
		case DocGen.DocIO.DLS.TabJustification.Left:
		case DocGen.DocIO.DLS.TabJustification.Centered:
		case DocGen.DocIO.DLS.TabJustification.Right:
		case DocGen.DocIO.DLS.TabJustification.Bar:
			justification.ToString();
			break;
		}
		return "";
	}

	internal string DisplayBalloonValueForListFormat(Dictionary<int, object> newpropertyhash, WListFormat listFormat)
	{
		string[] array = new string[7];
		WListLevel currentListLevel = listFormat.CurrentListLevel;
		ListStyle currentListStyle = listFormat.CurrentListStyle;
		string text = "";
		if (listFormat.ListType == ListType.Numbered && !currentListStyle.IsHybrid)
		{
			array[0] = "Numbered";
		}
		else if (listFormat.ListType == ListType.Numbered)
		{
			array[0] = "Outline Numbered";
		}
		else if (listFormat.ListType == ListType.Bulleted)
		{
			array[0] = "Bulleted";
		}
		array[1] = "Level: " + (listFormat.ListLevelNumber + 1);
		if (listFormat.ListType == ListType.Numbered)
		{
			array[2] = GetListPatternFormattedText(currentListLevel.PatternType);
			array[3] = "Start at: " + (currentListLevel.StartAt + 1);
		}
		array[4] = "Alignment: " + currentListLevel.NumberAlignment;
		array[5] = "Aliged at: " + (currentListLevel.TextPosition + currentListLevel.NumberPosition).ToString(CultureInfo.InvariantCulture) + " pt";
		array[6] = "Indent at: " + currentListLevel.TextPosition.ToString(CultureInfo.InvariantCulture) + " pt";
		for (int i = 0; i < array.Length; i++)
		{
			text += ((i != array.Length - 1) ? ((!string.IsNullOrEmpty(array[i])) ? (array[i] + "+ ") : "") : array[i]);
		}
		return "List Paragraph, " + text;
	}

	private string GetListPatternFormattedText(ListPatternType patternType)
	{
		string text = "";
		switch (patternType)
		{
		case ListPatternType.Arabic:
			text = "1, 2, 3, ...";
			break;
		case ListPatternType.UpRoman:
			text = "I, II, III, ...";
			break;
		case ListPatternType.LowRoman:
			text = "i, ii, iii, ...";
			break;
		case ListPatternType.UpLetter:
			text = "A, B, C, ...";
			break;
		case ListPatternType.LowLetter:
			text = "a, b, c, ...";
			break;
		case ListPatternType.Ordinal:
			text += "1st, 2nd, 3rd, ...";
			break;
		case ListPatternType.OrdinalText:
			text = " First, Second, Third, ...";
			break;
		case ListPatternType.LeadingZero:
			text = "01, 02, 03, ...";
			break;
		case ListPatternType.Number:
			text = "One, Two, Three, ...";
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		return "Numbering Style: " + text;
	}

	internal void AppendInDeletionBalloon(WTextRange textRange)
	{
		ChangedValue.LastParagraph.AppendText(textRange.Text).ApplyCharacterFormat(textRange.CharacterFormat);
		(ChangedValue.LastParagraph.Items.LastItem as WTextRange).CharacterFormat.FontSize = 10f;
		(ChangedValue.LastParagraph.Items.LastItem as WTextRange).CharacterFormat.IsDeleteRevision = false;
	}

	internal string ConvertDictionaryValuesToString(Dictionary<int, string> hierarchyOrder)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		int num = hierarchyOrder.Count;
		for (int i = 0; i < 150; i++)
		{
			if (!hierarchyOrder.ContainsKey(i))
			{
				continue;
			}
			if (!flag && i >= 4 && i <= 6)
			{
				stringBuilder.Append("Font: ");
				flag = true;
			}
			if (!flag2 && (i == 7 || i == 8 || i == 1 || i == 3))
			{
				if (!string.IsNullOrEmpty(stringBuilder.ToString()))
				{
					stringBuilder.Append("Complex Script Font: ");
				}
				else
				{
					stringBuilder.Append("Complex Script Font: ");
				}
				flag2 = true;
			}
			string value = "";
			hierarchyOrder.TryGetValue(i, out value);
			if (!string.IsNullOrEmpty(value))
			{
				if (num != 1)
				{
					stringBuilder.Append(" " + value + ",");
					num--;
				}
				else
				{
					stringBuilder.Append(" " + value);
				}
			}
		}
		return stringBuilder.ToString();
	}
}

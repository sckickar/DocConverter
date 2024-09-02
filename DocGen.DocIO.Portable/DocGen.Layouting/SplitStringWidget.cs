using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class SplitStringWidget : ISplitLeafWidget, ILeafWidget, IWidget
{
	private IStringWidget m_strWidget;

	internal int m_prevWidgetIndex = -1;

	internal int StartIndex;

	internal int Length;

	private byte m_bFlags;

	internal bool IsTrailSpacesWrapped
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public string SplittedText
	{
		get
		{
			string text = null;
			if (StartIndex == int.MinValue && Length == int.MinValue)
			{
				return null;
			}
			if (StartIndex == -1 && Length == -1)
			{
				return string.Empty;
			}
			if (StartIndex != -1 && Length == -1)
			{
				return (m_strWidget as WTextRange).Text.Substring(StartIndex);
			}
			if (Length < 0)
			{
				return string.Empty;
			}
			if (StartIndex != -1 && StartIndex + Length <= (m_strWidget as WTextRange).Text.Length)
			{
				return (m_strWidget as WTextRange).Text.Substring(StartIndex, Length);
			}
			return string.Empty;
		}
	}

	public IStringWidget RealStringWidget => m_strWidget;

	public ILayoutInfo LayoutInfo => m_strWidget.LayoutInfo;

	public SplitStringWidget(IStringWidget strWidget, int startIndex, int length)
	{
		m_strWidget = strWidget;
		StartIndex = startIndex;
		Length = length;
	}

	public string GetText()
	{
		return SplittedText;
	}

	public void InitLayoutInfo(IWidget widget)
	{
	}

	void IWidget.InitLayoutInfo()
	{
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	public ISplitLeafWidget[] SplitBySize(DrawingContext dc, SizeF offset, float clientWidth, float clientActiveAreaWidth, ref bool isLastWordFit, bool isTabStopInterSectingfloattingItem, bool isSplitByCharacter, bool isFirstItemInLine, ref int countForConsecutivelimit, Layouter layouter, ref bool isHyphenated)
	{
		return SplitBySize(dc, offset.Width, m_strWidget, this, clientWidth, clientActiveAreaWidth, ref isLastWordFit, isTabStopInterSectingfloattingItem, isSplitByCharacter, isFirstItemInLine, ref countForConsecutivelimit, layouter, ref isHyphenated);
	}

	public SizeF Measure(DrawingContext dc)
	{
		return m_strWidget.Measure(dc, GetText());
	}

	public static ISplitLeafWidget[] SplitBySize(DrawingContext dc, double offset, IStringWidget strWidget, SplitStringWidget splitStringWidget, float clientWidth, float clientActiveAreaWidth, ref bool isLastWordFit, bool isTabStopInterSectingfloattingItem, bool isSplitByCharacter, bool isFirstItemInLine, ref int countForConsecutivelimit, Layouter layouter, ref bool isHyphenated)
	{
		StringFormat stringFormat = new StringFormat();
		stringFormat.FormatFlags &= ~StringFormatFlags.LineLimit;
		stringFormat.Alignment = StringAlignment.Near;
		stringFormat.LineAlignment = StringAlignment.Near;
		stringFormat.Trimming = StringTrimming.Word;
		stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip;
		string text = ((splitStringWidget != null) ? splitStringWidget.SplittedText : (strWidget as WTextRange).Text);
		if ((strWidget is WField) ? (strWidget as WField).CharacterFormat.AllCaps : (strWidget as WTextRange).CharacterFormat.AllCaps)
		{
			text = text.ToUpper();
		}
		WCharacterFormat wCharacterFormat = ((strWidget is WField) ? (strWidget as WField).CharacterFormat : (strWidget as WTextRange).CharacterFormat);
		FontScriptType scriptType = ((strWidget is WField) ? (strWidget as WField).ScriptType : (strWidget as WTextRange).ScriptType);
		DocGen.Drawing.Font font = dc.GetFont(scriptType, wCharacterFormat, text);
		DocGen.Drawing.Font defaultFont = dc.GetDefaultFont(scriptType, font, wCharacterFormat);
		bool isTrailSpacesWrapped = false;
		WParagraph ownerParagraph = (strWidget as ParagraphItem).OwnerParagraph;
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		if (ownerParagraph != null && ownerParagraph.Document != null)
		{
			num = ownerParagraph.Document.DOP.ConsecHypLim;
			flag = ownerParagraph.Document.DOP.AutoHyphen;
			flag2 = ownerParagraph.Document.DOP.HyphCapitals;
		}
		if (!flag2)
		{
			bool num2;
			if (!(strWidget is WTextRange))
			{
				num2 = string.Equals(text.ToUpper(), text);
			}
			else
			{
				if ((strWidget as WTextRange).CharacterFormat.AllCaps)
				{
					goto IL_017b;
				}
				num2 = (strWidget as WTextRange).CharacterFormat.SmallCaps;
			}
			if (num2)
			{
				goto IL_017b;
			}
		}
		goto IL_017e;
		IL_017b:
		flag = false;
		goto IL_017e;
		IL_017e:
		if (num != 0 && countForConsecutivelimit >= num)
		{
			flag = false;
		}
		if (Hyphenator.Dictionaries.Count == 0)
		{
			flag = false;
		}
		StringSplitter stringSplitter = new StringSplitter();
		StringSplitResult stringSplitResult = stringSplitter.Split(text, font, defaultFont, stringFormat, new SizeF((float)offset, float.MaxValue), wCharacterFormat, ref isLastWordFit, isTabStopInterSectingfloattingItem, ref isTrailSpacesWrapped, flag, strWidget, ref isHyphenated);
		stringSplitter.Close();
		if (stringSplitResult.Lines.Length != 0)
		{
			ISplitLeafWidget[] array = new ISplitLeafWidget[2];
			int length = stringSplitResult.Lines[0].Line.Length;
			bool flag3 = false;
			if (flag && stringSplitResult.Lines[0].Line.EndsWith("-"))
			{
				countForConsecutivelimit++;
				string text2 = "";
				if (splitStringWidget != null)
				{
					if ((splitStringWidget.RealStringWidget as WTextRange).Text[splitStringWidget.StartIndex + (length - 1)] != '-')
					{
						text2 = (splitStringWidget.RealStringWidget as WTextRange).Text;
						text2 = text2.Insert(splitStringWidget.StartIndex + (length - 1), "-");
						(splitStringWidget.RealStringWidget as WTextRange).Text = text2;
						flag3 = true;
						isHyphenated = true;
					}
				}
				else if (length - 1 < (strWidget as WTextRange).Text.Length && (strWidget as WTextRange).Text[length - 1] != '-')
				{
					text2 = (strWidget as WTextRange).Text;
					text2 = text2.Insert(length - 1, "-");
					(strWidget as WTextRange).Text = text2;
					flag3 = true;
					isHyphenated = true;
				}
			}
			else
			{
				countForConsecutivelimit = 0;
			}
			if (ownerParagraph.ParagraphFormat.Bidi && !wCharacterFormat.Bidi && length > 0 && stringSplitResult.Remainder != null && stringSplitResult.Remainder != string.Empty && stringSplitResult.Remainder[0] == ControlChar.SpaceChar && strWidget is WTextRange && ((strWidget as WTextRange).CharacterRange == CharacterRangeType.RTL || stringSplitResult.Lines[0].Line[length - 1] == ControlChar.SpaceChar))
			{
				stringSplitResult.Remainder.ToCharArray();
				string text3 = new string(stringSplitResult.Lines[0].Line.ToCharArray());
				bool flag4 = true;
				bool flag5 = false;
				for (int num3 = text3.Length - 1; num3 >= 0; num3--)
				{
					if (text3[num3] != ControlChar.SpaceChar)
					{
						flag4 = false;
						text3 = text3.Remove(num3);
					}
					else
					{
						if (!flag4)
						{
							break;
						}
						text3 = text3.Remove(num3);
					}
				}
				string text4 = text.Substring(text3.Length);
				for (int i = 0; i < text4.Length; i++)
				{
					if (TextSplitter.IsRTLChar(text4[i]))
					{
						flag5 = true;
					}
					else if (text4[i] != ControlChar.SpaceChar)
					{
						break;
					}
				}
				if (text3 != string.Empty && text4 != string.Empty && flag5)
				{
					stringSplitResult.Lines[0].Line = text3;
					stringSplitResult.Remainder = text4;
					length = stringSplitResult.Lines[0].Line.Length;
				}
			}
			array[0] = new SplitStringWidget(strWidget, splitStringWidget?.StartIndex ?? 0, stringSplitResult.Lines[0].Line.Length);
			(array[0] as SplitStringWidget).IsTrailSpacesWrapped = isTrailSpacesWrapped;
			string text5 = string.Empty;
			int num4 = (splitStringWidget?.StartIndex ?? 0) + length;
			if (stringSplitResult.Remainder == null && stringSplitResult.Lines.Length > 1)
			{
				for (int j = 1; j < stringSplitResult.Count; j++)
				{
					text5 = ((!(stringSplitResult.Lines[j].Line == ControlChar.Space)) ? (text5 + ControlChar.LineFeed + stringSplitResult.Lines[j].Line) : (text5 + ControlChar.LineFeed));
				}
			}
			else
			{
				int startIndex = ((flag && flag3) ? (length - 1) : length);
				text5 = (flag ? text.Substring(startIndex) : stringSplitResult.Remainder);
			}
			if (text5 == ControlChar.LineFeed || text5 == ControlChar.ParagraphBreak)
			{
				text5 = ControlChar.Space;
			}
			if (text5 != null)
			{
				if (StartsWithExt(text5, ControlChar.LineFeed) || StartsWithExt(text5, ControlChar.ParagraphBreak))
				{
					text5 = text5.Remove(0, 1);
					if (stringSplitResult.Lines[0].Line != " ")
					{
						num4++;
					}
				}
				else if (StartsWithExt(text5, ControlChar.Space))
				{
					int length2 = text5.Length;
					text5 = text5.TrimStart(ControlChar.SpaceChar);
					num4 += length2 - text5.Length;
				}
				if (text5 != string.Empty)
				{
					array[1] = new SplitStringWidget(strWidget, num4, text5.Length);
				}
			}
			else
			{
				array[1] = new SplitStringWidget(strWidget, num4, -1);
			}
			return array;
		}
		if ((strWidget as WTextRange).Text != null && (strWidget as WTextRange).Text != string.Empty)
		{
			countForConsecutivelimit = 0;
			if ((double)dc.MeasureTextRange(strWidget as WTextRange, stringSplitResult.Remainder).Width > offset && (offset != 0.0 || !layouter.AtLeastOneChildFitted))
			{
				return SplitByOffset(dc, offset, splitStringWidget?.StartIndex ?? 0, strWidget, null, clientWidth, clientActiveAreaWidth, stringSplitResult.Remainder, isSplitByCharacter, isFirstItemInLine);
			}
		}
		else
		{
			countForConsecutivelimit = 0;
		}
		return null;
	}

	public static ISplitLeafWidget[] SplitByOffset(DrawingContext dc, double offset, int startIndex, IStringWidget strWidget, StringSplitInfo splitInfo, float clientWidth, float clientActiveAreaWidth, string textToSplit, bool isSpliByCharacter, bool isFirstItemInLine)
	{
		if (splitInfo == null)
		{
			splitInfo = new StringSplitInfo(0, textToSplit.Length - 1);
		}
		int num = strWidget.OffsetToIndex(dc, offset, splitInfo.GetSubstring(textToSplit), clientWidth, clientActiveAreaWidth, isSpliByCharacter);
		if (num > -1 && num < splitInfo.Length - 1)
		{
			ISplitLeafWidget[] array = new ISplitLeafWidget[2];
			array[0] = new SplitStringWidget(strWidget, startIndex, splitInfo.GetSplitFirstPart(num + 1).LastPos + 1);
			array[1] = new SplitStringWidget(strWidget, (array[0] as SplitStringWidget).StartIndex + (array[0] as SplitStringWidget).Length, -1);
			return array;
		}
		if (num > -1 && splitInfo.Length - 1 == num && isFirstItemInLine)
		{
			ISplitLeafWidget[] array2 = new ISplitLeafWidget[2]
			{
				new SplitStringWidget(strWidget, startIndex, 1),
				null
			};
			if (splitInfo.Length > 1)
			{
				array2[1] = new SplitStringWidget(strWidget, startIndex + 1, -1);
			}
			return array2;
		}
		return null;
	}

	private static bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}
}

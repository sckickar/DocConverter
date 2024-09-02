using System.Collections.Generic;
using System.Globalization;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.OfficeChart;

internal abstract class RendererBase
{
	private IRange _cell;

	private StringFormat _stringFormat;

	private WorkbookImpl _workBookImpl;

	private List<IOfficeFont> _richTextFont;

	private List<string> _drawString;

	private const string NewLineKey = "\n";

	private float _leftWidth;

	private float _rightWidth;

	private RectangleF _cellRect;

	internal abstract bool IsHfRtfProcess { get; set; }

	internal abstract RectangleF HfImageBounds { get; set; }

	internal void SetWidth(float leftWidth, float rightWidth)
	{
		_leftWidth = leftWidth;
		_rightWidth = rightWidth;
	}

	internal RendererBase(IRange cell, StringFormat stringFormat, List<IOfficeFont> fonts, List<string> drawString, WorkbookImpl workbook)
	{
		_cell = cell;
		_stringFormat = stringFormat;
		_workBookImpl = workbook;
		_richTextFont = fonts;
		_drawString = drawString;
	}

	internal void DrawRTFText(RectangleF cellRect, RectangleF adjacentRect, bool isShape, bool isWrapText, bool isHorizontalTextOverflow, bool isVerticalTextOverflow, bool isChartShape, bool isHeaderFooter)
	{
		_cellRect = cellRect;
		if (_cell != null && (_cell.VerticalAlignment == OfficeVAlign.VAlignJustify || ((_cell as RangeImpl).ExtendedFormat != null && (_cell as RangeImpl).ExtendedFormat.Rotation == 255)))
		{
			_stringFormat.Trimming = StringTrimming.Word;
		}
		List<LineInfoImpl> list = new List<LineInfoImpl>();
		List<TextInfoImpl> textInfoCollection = new List<TextInfoImpl>();
		float usedWidth = 0f;
		float usedHeight = 0f;
		float maxHeight = 0f;
		float maxAscent = 0f;
		for (int i = 0; i < _drawString.Count; i++)
		{
			string text = _drawString[i];
			IOfficeFont officeFont = _richTextFont[i];
			if (text != "\n" && officeFont is FontImpl && (officeFont as FontImpl).HasParagrapAlign)
			{
				if ((officeFont as FontImpl).ParaAlign == Excel2007CommentHAlign.l)
				{
					_stringFormat.Alignment = StringAlignment.Near;
				}
				if ((officeFont as FontImpl).ParaAlign == Excel2007CommentHAlign.r)
				{
					_stringFormat.Alignment = StringAlignment.Far;
				}
				if ((officeFont as FontImpl).ParaAlign == Excel2007CommentHAlign.ctr)
				{
					_stringFormat.Alignment = StringAlignment.Center;
				}
			}
			else if (text != "\n" && officeFont is FontWrapper && (officeFont as FontWrapper).Font.HasParagrapAlign)
			{
				if ((officeFont as FontWrapper).Font.ParaAlign == Excel2007CommentHAlign.l)
				{
					_stringFormat.Alignment = StringAlignment.Near;
				}
				if ((officeFont as FontWrapper).Font.ParaAlign == Excel2007CommentHAlign.r)
				{
					_stringFormat.Alignment = StringAlignment.Far;
				}
				if ((officeFont as FontWrapper).Font.ParaAlign == Excel2007CommentHAlign.ctr)
				{
					_stringFormat.Alignment = StringAlignment.Center;
				}
			}
			bool num = CheckUnicode(text);
			string text2 = officeFont.FontName;
			if (num)
			{
				if (officeFont is FontImpl)
				{
					text2 = SwitchFonts(text, (officeFont as FontImpl).CharSet, text2);
				}
				if (officeFont is FontWrapper)
				{
					text2 = SwitchFonts(text, (byte)(officeFont as FontWrapper).CharSet, text2);
				}
			}
			Font systemFont = GetSystemFont(officeFont, text2);
			SizeF size = MeasureString(text, systemFont);
			float ascent = FindAscent(text, systemFont);
			if (text.Contains("\n"))
			{
				if (_stringFormat.Trimming != StringTrimming.Word)
				{
					if (!text.Equals("\n"))
					{
						_drawString[i] = text.Replace("\n", string.Empty);
						i--;
					}
					continue;
				}
				if (text.Equals("\n"))
				{
					if (size.Height > maxHeight)
					{
						maxHeight = size.Height;
					}
					LayoutNewLine(adjacentRect, ref usedHeight, list, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, isLastLine: true, string.Empty, isShape, isHeaderFooter);
					if (i + 1 == _drawString.Count && size.Height > maxHeight)
					{
						maxHeight = size.Height;
					}
					continue;
				}
				_drawString.RemoveAt(i);
				_richTextFont.RemoveAt(i);
				int num2 = 0;
				int num3 = 0;
				while (text.IndexOf("\n", num2) > -1)
				{
					int num4 = text.IndexOf("\n", num2);
					string text3 = text.Substring(num2, (num4 - num2 == 0) ? 1 : (num4 - num2));
					num2 += text3.Length;
					_drawString.Insert(i + num3, text3);
					_richTextFont.Insert(i + num3++, officeFont);
				}
				if (num2 < text.Length)
				{
					string item = text.Substring(num2, text.Length - num2);
					_drawString.Insert(i + num3, item);
					_richTextFont.Insert(i + num3, officeFont);
				}
				i--;
				continue;
			}
			if (IsHfRtfProcess && text.Contains("HeaderFooterImage"))
			{
				string[] array = text.Split(new char[1] { ':' });
				size.Width = float.Parse(array[1], CultureInfo.InvariantCulture.NumberFormat);
				ascent = (size.Height = float.Parse(array[2], CultureInfo.InvariantCulture.NumberFormat));
			}
			if (size.Width <= adjacentRect.Width - usedWidth || _stringFormat.Trimming != StringTrimming.Word || (IsHfRtfProcess && text.Contains("HeaderFooterImage")) || !isWrapText)
			{
				TextInfoImpl textInfoImpl = LayoutText(adjacentRect, ref usedWidth, usedHeight, officeFont, size, ascent, ref maxHeight, ref maxAscent, text, text2, isShape, isHeaderFooter);
				textInfoImpl.Length = text.Length;
				textInfoCollection.Add(textInfoImpl);
			}
			else if (text.TrimStart(' ').TrimEnd(' ').Contains(" ") || text.TrimStart(' ').TrimEnd(' ').Contains("-") || text.EndsWith(" "))
			{
				int currPosition = 0;
				while (currPosition < text.Length)
				{
					int num6 = GetSpaceIndexBeforeText(text, currPosition);
					int num7 = text.IndexOf('-', currPosition);
					if (num7 >= 0 && (num7 < num6 || num6 == -1))
					{
						num6 = num7;
					}
					string text4 = ((num6 < 0) ? text.Substring(currPosition, text.Length - currPosition).TrimEnd(' ') : text.Substring(currPosition, num6 + 1 - currPosition));
					size = MeasureString(text4, systemFont);
					ascent = FindAscent(text4, systemFont);
					if (size.Width <= adjacentRect.Width - usedWidth || _workBookImpl.IsNullOrWhiteSpace(text4))
					{
						LayoutSplittedText(adjacentRect, usedHeight, textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, officeFont, text, ref size, ascent, ref currPosition, num6, text2);
						continue;
					}
					if (num6 > 0)
					{
						string text5 = text.Substring(currPosition, num6 + 1 - currPosition).TrimEnd(' ');
						if (text5 == string.Empty)
						{
							currPosition = num6 + 1;
							continue;
						}
						SizeF sizeF = MeasureString(text5, systemFont);
						ascent = FindAscent(text5, systemFont);
						if (sizeF.Width <= adjacentRect.Width - usedWidth)
						{
							LayoutSplittedText(adjacentRect, usedHeight, textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, officeFont, text, ref size, ascent, ref currPosition, num6, text2);
							continue;
						}
					}
					if (size.Width <= adjacentRect.Width && adjacentRect.Width - usedWidth > 0f)
					{
						LayoutNewLine(adjacentRect, ref usedHeight, list, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, isLastLine: false, text4, isShape, isHeaderFooter);
						LayoutSplittedText(adjacentRect, usedHeight, textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, officeFont, text, ref size, ascent, ref currPosition, num6, text2);
					}
					else
					{
						SplitByCharacter(adjacentRect, ref usedHeight, list, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, text4, officeFont, ref size, ascent, currPosition, text, text2, isShape, isHeaderFooter);
						currPosition += text4.Length;
					}
				}
			}
			else
			{
				SplitByCharacter(adjacentRect, ref usedHeight, list, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, text, officeFont, ref size, ascent, 0, text, text2, isShape, isHeaderFooter);
			}
			systemFont.Dispose();
			systemFont = null;
		}
		LayoutNewLine(adjacentRect, ref usedHeight, list, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, isLastLine: true, string.Empty, isShape, isHeaderFooter);
		float num8 = 0f;
		if (_cell != null && _cell.VerticalAlignment == OfficeVAlign.VAlignJustify)
		{
			if (list.Count > 1 && adjacentRect.Height > usedHeight)
			{
				num8 = (adjacentRect.Height - usedHeight) / (float)(list.Count - 1);
				for (int j = 0; j < list.Count; j++)
				{
					if (j == 0)
					{
						continue;
					}
					foreach (TextInfoImpl item2 in list[j].TextInfoCollection)
					{
						item2.Y += num8 * (float)j;
					}
				}
				num8 = 0f;
			}
		}
		else
		{
			if (_cell != null)
			{
				double num9 = _workBookImpl.GetCellScaledWidthHeight(_cell.Worksheet)[1];
				double scaledHeight = _workBookImpl.GetScaledHeight(_cell.CellStyle.Font.FontName, _cell.CellStyle.Font.Size, _cell.Worksheet);
				double num10 = (double)adjacentRect.Height / num9;
				RowStorage rowStorage = (_cell as RangeImpl).RowStorage;
				if (rowStorage != null && !rowStorage.IsBadFontHeight)
				{
					if (rowStorage.IsSpaceAboveRow)
					{
						num10 -= 0.5;
					}
					if (rowStorage.IsSpaceBelowRow)
					{
						num10 -= 0.5;
					}
				}
				num10 *= scaledHeight;
				usedHeight = UpdateTextHeight((float)num10, list, isShape: false);
			}
			else if (isShape && isWrapText && !isVerticalTextOverflow)
			{
				usedHeight = UpdateTextHeight(adjacentRect.Height, list, isShape: true);
			}
			switch (_stringFormat.LineAlignment)
			{
			case StringAlignment.Center:
				num8 = (adjacentRect.Height - usedHeight) / 2f;
				break;
			case StringAlignment.Far:
				num8 = adjacentRect.Height - usedHeight;
				break;
			}
		}
		usedWidth = list[list.Count - 1].Width;
		for (int k = 0; k < list.Count; k++)
		{
			if (list[k].Width > usedWidth)
			{
				usedWidth = list[k].Width;
			}
		}
		if (!isChartShape)
		{
			InitializeStringFormat();
		}
		float width = list[0].Width;
		RectangleF rectangleF = default(RectangleF);
		if (_stringFormat.Alignment == StringAlignment.Center && _stringFormat.Trimming != StringTrimming.Word && (_leftWidth != 0f || _rightWidth != 0f))
		{
			DrawTextTemplate(adjacentRect, list, num8);
		}
		else if (isShape && isHorizontalTextOverflow && !isWrapText)
		{
			rectangleF = new RectangleF(adjacentRect.X, adjacentRect.Y, adjacentRect.Width, adjacentRect.Height);
			DrawTextTemplate(rectangleF, list, num8);
		}
		else if (isShape && isVerticalTextOverflow && isWrapText && usedHeight > adjacentRect.Height)
		{
			foreach (LineInfoImpl item3 in list)
			{
				foreach (TextInfoImpl item4 in item3.TextInfoCollection)
				{
					item4.Y += num8;
					DrawString(item4, _stringFormat);
				}
			}
		}
		else if (isShape && !isWrapText && !isHorizontalTextOverflow && !isVerticalTextOverflow && adjacentRect.Width <= usedWidth)
		{
			DrawTextTemplate(adjacentRect, list, num8);
		}
		else if (_stringFormat.Trimming == StringTrimming.Word || adjacentRect.Width >= width)
		{
			if (adjacentRect.Height >= usedHeight)
			{
				foreach (LineInfoImpl item5 in list)
				{
					foreach (TextInfoImpl item6 in item5.TextInfoCollection)
					{
						item6.Y += num8;
						if (IsHfRtfProcess && item6.Text.Contains("HeaderFooterImage"))
						{
							HfImageBounds = item6.Bounds;
						}
						else
						{
							DrawString(item6, _stringFormat);
						}
					}
				}
			}
			else
			{
				if (isShape && list.Count > 0 && list.Count == 1)
				{
					foreach (LineInfoImpl item7 in list)
					{
						foreach (TextInfoImpl item8 in item7.TextInfoCollection)
						{
							if (item8.Font.Underline != 0)
							{
								adjacentRect.Height += 0.5f;
							}
						}
					}
				}
				DrawTextTemplate(adjacentRect, list, num8);
			}
		}
		else
		{
			switch (_stringFormat.LineAlignment)
			{
			case StringAlignment.Near:
				adjacentRect.Width += cellRect.X - adjacentRect.X;
				break;
			case StringAlignment.Far:
				adjacentRect.Width += adjacentRect.X - cellRect.X;
				adjacentRect.X = cellRect.X;
				break;
			}
			DrawTextTemplate(adjacentRect, list, num8);
		}
		_leftWidth = 0f;
		_rightWidth = 0f;
		if (_richTextFont != null)
		{
			_richTextFont.Clear();
			_richTextFont = null;
		}
		if (_drawString != null)
		{
			_drawString.Clear();
			_drawString = null;
		}
		if (list != null)
		{
			foreach (LineInfoImpl item9 in list)
			{
				item9.Dispose();
			}
			list.Clear();
			list = null;
		}
		if (textInfoCollection == null)
		{
			return;
		}
		foreach (TextInfoImpl item10 in textInfoCollection)
		{
			item10.Dispose();
		}
		textInfoCollection.Clear();
		textInfoCollection = null;
	}

	internal float UpdateTextHeight(float height, List<LineInfoImpl> lineInfoCollection, bool isShape)
	{
		float num = 0f;
		int num2 = 0;
		float result = 0f;
		if (isShape)
		{
			for (num2 = 0; num2 < lineInfoCollection.Count; num2++)
			{
				num += lineInfoCollection[num2].Height;
				if (height + 4f < num)
				{
					break;
				}
				result = num;
			}
			if (num2 == 0)
			{
				result = lineInfoCollection[0].Height;
				num2 = 1;
			}
			if (num2 < lineInfoCollection.Count)
			{
				int num3 = num2;
				while (num3 < lineInfoCollection.Count)
				{
					lineInfoCollection.RemoveAt(num3);
				}
			}
		}
		else
		{
			for (num2 = 0; num2 < lineInfoCollection.Count; num2++)
			{
				num += lineInfoCollection[num2].Height;
				if (height <= num)
				{
					result = num;
					num2++;
					break;
				}
				result = num;
			}
			if (num2 < lineInfoCollection.Count)
			{
				int num4 = num2;
				while (num4 < lineInfoCollection.Count)
				{
					lineInfoCollection.RemoveAt(num4);
				}
			}
		}
		return result;
	}

	internal abstract bool CheckUnicode(string text);

	internal abstract string SwitchFonts(string text, byte charSet, string fontName);

	internal abstract void InitializeStringFormat();

	internal abstract void DrawString(TextInfoImpl textInfo, StringFormat stringFormat);

	internal abstract SizeF MeasureString(string text, Font systemFont);

	internal abstract string CheckPdfFont(Font sysFont, string testString);

	internal virtual float FindAscent(string text, Font font)
	{
		float num = 0f;
		return (float)font.GetAscent(text);
	}

	internal void LayoutNewLine(RectangleF shapeBounds, ref float usedHeight, List<LineInfoImpl> lineInfoCollection, ref List<TextInfoImpl> textInfoCollection, ref float usedWidth, ref float maxAscent, ref float maxHeight, bool isLastLine, string text, bool isShape, bool isHeaderFooter)
	{
		LineInfoImpl lineInfoImpl = new LineInfoImpl();
		lineInfoImpl.TextInfoCollection = textInfoCollection;
		lineInfoCollection.Add(lineInfoImpl);
		LayoutXYPosition(usedWidth, maxAscent, shapeBounds.Width, textInfoCollection, isLastLine, ref maxHeight, ref usedHeight, isShape, isHeaderFooter);
		if (textInfoCollection.Count == 0)
		{
			lineInfoImpl.Height = maxHeight;
		}
		textInfoCollection = new List<TextInfoImpl>();
		usedHeight += maxHeight;
		usedWidth = 0f;
		maxHeight = 0f;
		maxAscent = 0f;
		CheckPreviousElement(lineInfoCollection, ref textInfoCollection, text, ref usedWidth, usedHeight, shapeBounds, isShape, isHeaderFooter);
	}

	private void LayoutXYPosition(float usedWidth, float maxAscent, float shapeWidth, List<TextInfoImpl> textInfoCollection, bool isLastLine, ref float maxHeight, ref float usedHeight, bool isShape, bool isHeaderFooter)
	{
		float x = 0f;
		float y = 0f;
		if (IsJustify())
		{
			if (textInfoCollection.Count > 0)
			{
				if (!isLastLine)
				{
					LineJustify(ref usedWidth, maxAscent, shapeWidth, textInfoCollection);
				}
				else
				{
					UpdateXYPosition(maxAscent, textInfoCollection, x, y, isShape, isHeaderFooter, ref maxHeight);
				}
			}
			return;
		}
		switch (_stringFormat.Alignment)
		{
		case StringAlignment.Near:
			UpdateXYPosition(maxAscent, textInfoCollection, x, y, isShape, isHeaderFooter, ref maxHeight);
			break;
		case StringAlignment.Center:
			usedWidth -= RemoveWhiteSpaces(textInfoCollection);
			x = (IsHfRtfProcess ? ((shapeWidth - usedWidth) / 2f) : ((_cell == null || _cell.HorizontalAlignment != OfficeHAlign.HAlignCenterAcrossSelection) ? (_leftWidth + (_cellRect.Width - usedWidth) / 2f) : ((shapeWidth - usedWidth) / 2f)));
			UpdateXYPosition(maxAscent, textInfoCollection, x, y, isShape, isHeaderFooter, ref maxHeight);
			break;
		case StringAlignment.Far:
			usedWidth -= RemoveWhiteSpaces(textInfoCollection);
			x = shapeWidth - usedWidth;
			UpdateXYPosition(maxAscent, textInfoCollection, x, y, isShape, isHeaderFooter, ref maxHeight);
			break;
		}
	}

	internal abstract bool IsJustify();

	private void CheckPreviousElement(List<LineInfoImpl> lineInfoCollection, ref List<TextInfoImpl> currTextInfoCollection, string text, ref float usedWidth, float usedHeight, RectangleF shapeBounds, bool isShape, bool isHeaderFooter)
	{
		if (lineInfoCollection.Count <= 0 || !(text != string.Empty) || text.StartsWith(" "))
		{
			return;
		}
		LineInfoImpl lineInfoImpl = lineInfoCollection[lineInfoCollection.Count - 1];
		List<TextInfoImpl> textInfoCollection = lineInfoImpl.TextInfoCollection;
		string text2 = lineInfoImpl.Text;
		if (text2 == string.Empty || !text2.Contains(" ") || text2.EndsWith(" "))
		{
			return;
		}
		int num = 0;
		float num2 = 0f;
		for (int num3 = textInfoCollection.Count - 1; num3 >= 0; num3--)
		{
			TextInfoImpl textInfoImpl = textInfoCollection[num3];
			string text3 = textInfoImpl.Text;
			if (text3 != string.Empty && !text3.Contains(" ") && !text3.EndsWith("-"))
			{
				num++;
			}
			else
			{
				if (text3.TrimEnd(' ').Contains(" ") && !text3.EndsWith(" "))
				{
					text3 = text3.TrimEnd(' ');
					int num4 = text3.LastIndexOf(' ') + 1;
					TextInfoImpl textInfoImpl2 = new TextInfoImpl(textInfoImpl.GetOriginalText());
					textInfoImpl.CopyTo(textInfoImpl2);
					textInfoImpl2.Position = num4;
					textInfoImpl2.Length = text3.Length - num4;
					textInfoImpl.Length = num4;
					textInfoImpl.Width = MeasureString(textInfoImpl.Text, GetSystemFont(textInfoImpl)).Width;
					textInfoImpl2.X = shapeBounds.Left + usedWidth;
					textInfoImpl2.Y = shapeBounds.Top + usedHeight;
					SizeF sizeF = MeasureString(textInfoImpl2.Text, GetSystemFont(textInfoImpl2));
					float num5 = usedWidth;
					float num6 = (textInfoImpl2.Width = sizeF.Width);
					usedWidth = num5 + (num2 = num6);
					textInfoImpl2.Height = sizeF.Height;
					currTextInfoCollection.Add(textInfoImpl2);
					break;
				}
				if (!text3.Equals(string.Empty))
				{
					break;
				}
				num++;
			}
		}
		int num7 = textInfoCollection.Count - num;
		while (textInfoCollection.Count != num7)
		{
			TextInfoImpl textInfoImpl3 = textInfoCollection[num7];
			textInfoCollection.RemoveAt(num7);
			currTextInfoCollection.Add(textInfoImpl3);
			textInfoImpl3.X = shapeBounds.Left + usedWidth;
			textInfoImpl3.Y = shapeBounds.Top + usedHeight + AlignSuperOrSubScript(textInfoImpl3.Text, textInfoImpl3.Font, isShape, isHeaderFooter);
			usedWidth += textInfoImpl3.Width;
			num2 += textInfoImpl3.Width;
		}
		if ((double)num2 == 0.0)
		{
			return;
		}
		switch (_stringFormat.Alignment)
		{
		case StringAlignment.Center:
			num2 += RemoveWhiteSpaces(textInfoCollection);
			UpdateXPosition(textInfoCollection, num2 / 2f);
			return;
		case StringAlignment.Far:
			num2 += RemoveWhiteSpaces(textInfoCollection);
			UpdateXPosition(textInfoCollection, num2);
			return;
		}
		if (_cell == null || _cell.HorizontalAlignment != OfficeHAlign.HAlignJustify)
		{
			return;
		}
		string text4 = string.Empty;
		float num8 = 0f - RemoveWhiteSpaces(textInfoCollection);
		foreach (TextInfoImpl item in textInfoCollection)
		{
			if (item.Text != null)
			{
				text4 += item.Text;
			}
			num8 += item.Width;
		}
		int num9 = text4.TrimEnd(' ').Length - text4.Replace(" ", string.Empty).Length;
		for (int i = 0; i < textInfoCollection.Count; i++)
		{
			UpdateLineJustifyPosition(textInfoCollection, (shapeBounds.Width - num8) / (float)num9, i);
		}
	}

	internal abstract void DrawTextTemplate(RectangleF bounds, List<LineInfoImpl> lineInfoCollection, float y);

	private float AlignSuperOrSubScript(string text, IOfficeFont font, bool isShape, bool isHeaderFooter)
	{
		float num = 0f;
		float num2 = 1.35f;
		if (font.Superscript)
		{
			Font font2 = _workBookImpl.GetFont(font);
			float num3 = FindAscent(text, font2);
			int num4 = (int)font2.GetHeight();
			num = ((!(isShape || isHeaderFooter)) ? (0f - (num + num3 + ((float)num4 / num2 - (float)num4))) : (0f - (num + num3 + ((float)num4 / num2 - (float)num4)) / 1.5f));
		}
		else if (font.Subscript)
		{
			Font font3 = _workBookImpl.GetFont(font);
			float num5 = FindAscent(text, font3);
			int num4 = (int)font3.GetHeight();
			num = (num + num5 + ((float)num4 / num2 - (float)num4)) / 3f;
		}
		return num;
	}

	internal Color NormalizeColor(Color color)
	{
		if (color.A == 0)
		{
			return Color.FromArgb(255, color.R, color.G, color.B);
		}
		return color;
	}

	private TextInfoImpl LayoutText(RectangleF adjacentRect, ref float usedWidth, float usedHeight, IOfficeFont font, SizeF size, float ascent, ref float maxHeight, ref float maxAscent, string text, string unicodeFont, bool isShape, bool isHeaderFooter)
	{
		float x = adjacentRect.Left + usedWidth;
		float y = adjacentRect.Top + usedHeight + AlignSuperOrSubScript(text, font, isShape, isHeaderFooter);
		float width = size.Width;
		float height = size.Height;
		usedWidth += size.Width;
		if (size.Height > maxHeight)
		{
			maxHeight = size.Height;
		}
		if (ascent > maxAscent)
		{
			maxAscent = ascent;
		}
		return new TextInfoImpl(text)
		{
			Bounds = new RectangleF(x, y, width, height),
			Ascent = ascent,
			Font = font,
			UnicodeFont = unicodeFont
		};
	}

	private int GetSpaceIndexBeforeText(string text, int startIndex)
	{
		int num = text.IndexOf(' ', startIndex);
		if (num < 0 || num == text.Length)
		{
			return num;
		}
		while (text[num] == ' ')
		{
			num++;
			if (num == text.Length)
			{
				break;
			}
		}
		return num - 1;
	}

	private void LayoutSplittedText(RectangleF shapeBounds, float usedHeight, List<TextInfoImpl> textInfoCollection, ref float usedWidth, ref float maxAscent, ref float maxHeight, IOfficeFont font, string text, ref SizeF size, float ascent, ref int currPosition, int index, string unicodeFont)
	{
		TextInfoImpl textInfoImpl = LayoutText(shapeBounds, ref usedWidth, usedHeight, font, size, ascent, ref maxHeight, ref maxAscent, text, unicodeFont, isShape: false, isHeaderFooter: false);
		textInfoImpl.Position = currPosition;
		textInfoCollection.Add(textInfoImpl);
		if (index > -1)
		{
			textInfoImpl.Length = index + 1 - currPosition;
			currPosition = index + 1;
		}
		else
		{
			textInfoImpl.Length = text.Length - currPosition;
			currPosition = text.Length;
		}
	}

	private void SplitByCharacter(RectangleF shapeBounds, ref float usedHeight, List<LineInfoImpl> lineInfoCollection, ref List<TextInfoImpl> textInfoCollection, ref float usedWidth, ref float maxAscent, ref float maxHeight, string text, IOfficeFont font, ref SizeF size, float ascent, int currPosition, string originalText, string unicodeFont, bool isShape, bool isHeaderFooter)
	{
		int num = GetTextIndexAfterSpace(text.TrimEnd(' '), 0);
		int num2 = 1;
		float width = 0f;
		string text2 = string.Empty;
		if ((double)usedWidth > 0.0 && textInfoCollection.Count > 1 && size.Width <= shapeBounds.Width)
		{
			LayoutNewLine(shapeBounds, ref usedHeight, lineInfoCollection, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, isLastLine: false, text, isShape, isHeaderFooter);
		}
		while (num + num2 <= text.Length)
		{
			text2 = text.Substring(num, num2);
			size = MeasureString(text2, GetSystemFont(font, unicodeFont));
			if (size.Width > shapeBounds.Width - usedWidth)
			{
				if (text2.Length > 1)
				{
					size.Width = width;
				}
				TextInfoImpl textInfoImpl = LayoutText(shapeBounds, ref usedWidth, usedHeight, font, size, ascent, ref maxHeight, ref maxAscent, originalText, unicodeFont, isShape, isHeaderFooter);
				textInfoImpl.Position = num + currPosition;
				if (shapeBounds.Width - usedWidth < 0f)
				{
					textInfoImpl.Length = num2;
					textInfoImpl.Width = MeasureString(text2, GetSystemFont(textInfoImpl)).Width;
					ascent = FindAscent(text, GetSystemFont(textInfoImpl));
				}
				else if (usedWidth == 0f)
				{
					textInfoImpl.Length = num2;
				}
				else
				{
					textInfoImpl.Length = num2 - 1;
				}
				textInfoCollection.Add(textInfoImpl);
				num = ((text2.Length != 1 || textInfoCollection.Count != 1) ? (num + (num2 - 1)) : (num + num2));
				num2 = 1;
				LayoutNewLine(shapeBounds, ref usedHeight, lineInfoCollection, ref textInfoCollection, ref usedWidth, ref maxAscent, ref maxHeight, isLastLine: false, text2[text2.Length - 1].ToString(), isShape, isHeaderFooter);
				if (textInfoCollection.Count == 1)
				{
					textInfoCollection = new List<TextInfoImpl>();
					usedWidth = 0f;
					num = GetTextIndexAfterSpace(text.TrimEnd(' '), 0);
				}
				text2 = string.Empty;
			}
			else
			{
				width = size.Width;
				num2++;
			}
		}
		if (text2 != string.Empty)
		{
			TextInfoImpl textInfoImpl2 = LayoutText(shapeBounds, ref usedWidth, usedHeight, font, size, ascent, ref maxHeight, ref maxAscent, originalText, unicodeFont, isShape, isHeaderFooter);
			textInfoImpl2.Position = num + currPosition;
			textInfoImpl2.Length = num2 - 1;
			textInfoCollection.Add(textInfoImpl2);
		}
	}

	private int GetTextIndexAfterSpace(string text, int startIndex)
	{
		int num = text.IndexOf(' ', startIndex) + 1;
		if (num == 0 || num == text.Length)
		{
			return num;
		}
		while (text[num] == ' ')
		{
			num++;
			if (num == text.Length)
			{
				break;
			}
		}
		return num;
	}

	private void UpdateXYPosition(float maxAscent, IEnumerable<TextInfoImpl> textInfoCollection, float x, float y, bool isShape, bool isHeaderFooter, ref float maxHeight)
	{
		foreach (TextInfoImpl item in textInfoCollection)
		{
			item.X += x;
			if (maxAscent == item.Ascent && item.Font.Superscript && (isShape || isHeaderFooter))
			{
				item.Font.Superscript = false;
				Font systemFont = GetSystemFont(item.Font, item.Font.FontName);
				float num = FindAscent(item.Text, systemFont);
				if (isHeaderFooter)
				{
					SizeF sizeF = MeasureString(item.Text, systemFont);
					if (maxHeight < sizeF.Height)
					{
						maxHeight = sizeF.Height;
					}
				}
				item.Font.Superscript = true;
				item.Y += num - item.Ascent + y;
			}
			else if (maxAscent == item.Ascent && item.Font.Subscript && isHeaderFooter)
			{
				item.Font.Subscript = false;
				Font systemFont2 = GetSystemFont(item.Font, item.Font.FontName);
				if (isHeaderFooter)
				{
					SizeF sizeF2 = MeasureString(item.Text, systemFont2);
					if (maxHeight < sizeF2.Height)
					{
						maxHeight = sizeF2.Height;
					}
				}
				float num2 = FindAscent(item.Text, systemFont2);
				item.Font.Subscript = true;
				item.Y += num2 - item.Ascent + y - 1f;
			}
			else
			{
				item.Y += maxAscent - item.Ascent + y;
			}
		}
	}

	private void LineJustify(ref float usedWidth, float maxAscent, float shapeWidth, List<TextInfoImpl> textInfoCollection)
	{
		List<TextInfoImpl> list = new List<TextInfoImpl>();
		float num = textInfoCollection[0].Bounds.X;
		string text = string.Empty;
		foreach (TextInfoImpl item in textInfoCollection)
		{
			string originalText = item.GetOriginalText();
			int num2 = item.Position;
			int num3 = item.Position;
			if (originalText.Substring(num3, originalText.Length - num3).Contains(" "))
			{
				while (num2 < item.Length + item.Position)
				{
					int startIndex = num2;
					num2 = GetTextIndexAfterSpace(originalText, num2);
					int i;
					for (i = originalText.IndexOf('-', startIndex) + 1; i > 0 && i < originalText.Length && originalText[i].ToString() == " "; i++)
					{
					}
					if (i > 0 && (i < num2 || num2 == 0))
					{
						num2 = i;
					}
					if (num2 == 0 && num3 == originalText.Length)
					{
						break;
					}
					if (num2 == 0)
					{
						num2 = originalText.Length;
					}
					if (num2 <= originalText.Length)
					{
						if (originalText.Substring(num3, num2 - num3).TrimEnd(' ').StartsWith(" ") && !string.IsNullOrEmpty(originalText))
						{
							num3 += originalText.Length - originalText.TrimStart(' ').Length;
							continue;
						}
						TextInfoImpl textInfoImpl = new TextInfoImpl(item.GetOriginalText());
						string text2 = originalText.Substring(num3, num2 - num3);
						textInfoImpl.Position = num3;
						textInfoImpl.Length = num2 - num3;
						num3 += textInfoImpl.Length;
						SizeF sizeF = MeasureString(text2, GetSystemFont(item));
						textInfoImpl.Bounds = new RectangleF(num, item.Bounds.Y, sizeF.Width, sizeF.Height);
						item.CopyTo(textInfoImpl);
						list.Add(textInfoImpl);
						text += text2;
						num += sizeF.Width;
					}
				}
			}
			else
			{
				text += item.Text;
				list.Add(item);
				num += item.Width;
			}
		}
		textInfoCollection.Clear();
		textInfoCollection.AddRange(list);
		usedWidth -= RemoveWhiteSpaces(textInfoCollection);
		int num4 = text.TrimEnd(' ').Length - text.Replace(" ", "").Length;
		float x = (shapeWidth - usedWidth) / (float)num4;
		for (int j = 0; j < textInfoCollection.Count; j++)
		{
			TextInfoImpl textInfoImpl2 = textInfoCollection[j];
			textInfoImpl2.Y += maxAscent - textInfoImpl2.Ascent;
			UpdateLineJustifyPosition(textInfoCollection, x, j);
		}
	}

	private float RemoveWhiteSpaces(List<TextInfoImpl> textInfoCollection)
	{
		float result = 0f;
		if (textInfoCollection.Count == 0)
		{
			return result;
		}
		TextInfoImpl textInfoImpl = textInfoCollection[textInfoCollection.Count - 1];
		string text = string.Empty;
		for (int num = textInfoImpl.Text.Length - 1; num >= 0; num--)
		{
			char c = textInfoImpl.Text[num];
			if (c != ' ')
			{
				break;
			}
			text += c;
		}
		if (!text.Equals(string.Empty))
		{
			result = MeasureString(text, GetSystemFont(textInfoImpl)).Width;
		}
		return result;
	}

	private static void UpdateLineJustifyPosition(List<TextInfoImpl> textInfoCollection, float x, int index)
	{
		if (index != 0)
		{
			if (textInfoCollection[index - 1].Text.EndsWith(" "))
			{
				textInfoCollection[index].X = textInfoCollection[index - 1].Bounds.Right + x;
			}
			else
			{
				textInfoCollection[index].X = textInfoCollection[index - 1].Bounds.Right;
			}
		}
	}

	private void UpdateXPosition(IEnumerable<TextInfoImpl> textInfoCollection, float x)
	{
		foreach (TextInfoImpl item in textInfoCollection)
		{
			item.X += x;
		}
	}

	internal Font GetSystemFont(TextInfoImpl textInfo)
	{
		Font systemFont = _workBookImpl.GetSystemFont(textInfo.Font, string.IsNullOrEmpty(textInfo.UnicodeFont) ? textInfo.Font.FontName : textInfo.UnicodeFont);
		if (systemFont.Name != textInfo.Font.FontName && systemFont.Name == "Microsoft Sans Serif")
		{
			systemFont = _workBookImpl.GetSystemFont(textInfo.Font, _workBookImpl.StandardFont);
		}
		return systemFont;
	}

	internal Font GetSystemFont(IOfficeFont font, string fontName)
	{
		Font systemFont = _workBookImpl.GetSystemFont(font, fontName);
		if (systemFont.Name != font.FontName && systemFont.Name == "Microsoft Sans Serif")
		{
			systemFont = _workBookImpl.GetSystemFont(font, _workBookImpl.StandardFont);
		}
		return systemFont;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class AutoFitManager : IDisposable
{
	private class StyleWithText
	{
		internal string fontName;

		internal double size;

		internal FontStyle style;

		internal List<string> strValues;

		internal StyleWithText()
		{
			strValues = new List<string>();
		}
	}

	private const double DEF_AUTO_FILTER_WIDTH = 1.363;

	private const string DROPDOWNSYMBOL = "AA";

	private RangeImpl m_rangeImpl;

	private WorksheetImpl m_worksheet;

	private WorkbookImpl m_book;

	private int m_row;

	private int m_column;

	private int m_lastRow;

	private int m_lastColumn;

	internal IWorksheet Worksheet => m_worksheet;

	internal AutoFitManager(int row, int column, int lastRow, int lastColumn, RangeImpl rangeImpl)
	{
		m_row = row;
		m_column = column;
		m_lastRow = lastRow;
		m_lastColumn = lastColumn;
		m_rangeImpl = rangeImpl;
		m_worksheet = rangeImpl.Worksheet as WorksheetImpl;
		m_book = rangeImpl.Workbook;
	}

	internal AutoFitManager()
	{
	}

	internal void MeasureToFitColumn()
	{
		int num = 14;
		int row = m_row;
		int lastRow = m_lastRow;
		int column = m_column;
		int lastColumn = m_lastColumn;
		Dictionary<int, IList<object>> dictionary = new Dictionary<int, IList<object>>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		RectangleF rectF = new RectangleF(0f, 0f, 1800f, 100f);
		WorksheetImpl worksheet = m_worksheet;
		IMigrantRange migrantRange = m_worksheet.MigrantRange;
		for (int i = row; i <= lastRow; i++)
		{
			for (int j = column; j <= lastColumn; j++)
			{
				ushort num2 = (ushort)m_worksheet.GetXFIndex(i, j);
				int numberFormatIndex = m_book.GetExtFormat(num2).NumberFormatIndex;
				int fontIndex = m_book.InnerExtFormats[num2].FontIndex;
				_ = m_book.InnerFonts[fontIndex];
				FormatImpl formatImpl = m_book.InnerFormats[numberFormatIndex];
				IStyle style = m_worksheet.InnerGetCellStyle(j, i, num2, m_rangeImpl[i, j] as RangeImpl);
				int num3 = 0;
				if (RangeImpl.IsMergedCell(worksheet.MergeCells, i, j, isRow: false, ref num3) && num3 == 0)
				{
					continue;
				}
				if (!dictionary2.ContainsKey(j))
				{
					dictionary2.Add(j, 0);
				}
				string displayText = m_rangeImpl.GetDisplayText(i, j, formatImpl);
				if (displayText == null || displayText == "")
				{
					continue;
				}
				migrantRange.ResetRowColumn(i, j);
				bool flag = migrantRange.CellStyle.WrapText || migrantRange.WrapText;
				if ((style.Rotation == 0 || style.Rotation == 255) && !flag)
				{
					IList<object> list = (dictionary.ContainsKey(j) ? dictionary[j] : null);
					if (list == null)
					{
						list = new List<object>();
						dictionary.Add(j, list);
					}
					if (style.HorizontalAlignment == OfficeHAlign.HAlignCenterAcrossSelection)
					{
						IRange range = m_rangeImpl[i, j++];
						if (j == range.Column + 1)
						{
						}
					}
				}
				else if (flag)
				{
					int columnWidthInPixels = m_worksheet.GetColumnWidthInPixels(j);
					string[] array = displayText.Split(' ', '\n');
					int num4 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						string text = array[k].ToString();
						if (text.Length > 0)
						{
							if (MeasureCharacterRanges(style, text, num, rectF) < columnWidthInPixels)
							{
								for (int l = k + 1; l < array.Length; l++)
								{
									k = l;
									text = string.Format("{0}{1}{2}", text, " ", array[l]);
									int num5 = MeasureCharacterRanges(style, text, num, rectF);
									if (num5 < columnWidthInPixels)
									{
										if (num5 > num4)
										{
											num4 = num5;
											l = array.Length;
										}
									}
									else
									{
										k = l - 1;
										l = array.Length;
									}
								}
							}
							else
							{
								num4 = columnWidthInPixels;
								k = array.Length;
							}
						}
						dictionary2[j] = num4;
					}
				}
				else
				{
					int num6 = MeasureCharacterRanges(style, displayText, num, rectF);
					if ((dictionary2.ContainsKey(j) ? dictionary2[j] : 0) < num6)
					{
						dictionary2[j] = num6;
					}
				}
			}
		}
		IDictionaryEnumerator dictionaryEnumerator = dictionary.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			IList<object> list2 = (IList<object>)dictionaryEnumerator.Value;
			int key = (int)dictionaryEnumerator.Key;
			int num7 = 0;
			for (int m = 0; m < list2.Count; m++)
			{
				StyleWithText styleWithText = (StyleWithText)list2[m];
				int num8 = MeasureCharacterRanges(styleWithText, num, rectF);
				if (num7 < num8)
				{
					num7 = num8;
				}
			}
			int num9 = (dictionary2.ContainsKey(key) ? dictionary2[key] : 0);
			if (num7 > num9)
			{
				dictionary2[key] = num7;
			}
		}
		IDictionaryEnumerator dictionaryEnumerator2 = dictionary2.GetEnumerator();
		while (dictionaryEnumerator2.MoveNext())
		{
			int num10 = (int)dictionaryEnumerator2.Value;
			if (num10 != 0)
			{
				worksheet.SetColumnWidthInPixels((int)dictionaryEnumerator2.Key, num10);
			}
		}
	}

	private Font CreateFont(string fontName, float size, FontStyle fontStyle)
	{
		return new Font
		{
			Name = fontName,
			Size = size,
			Bold = true,
			Italic = true
		};
	}

	private int MeasureCharacterRanges(StyleWithText styleWithText, int paramNum, RectangleF rectF)
	{
		int num = 0;
		using Font font = CreateFont(styleWithText.fontName, (float)styleWithText.size, styleWithText.style);
		for (int i = 0; i < styleWithText.strValues.Count; i++)
		{
			string text = styleWithText.strValues[i];
			int num2 = (int)((double)MeasureString(text, font, rectF, isAutoFitRow: false).Width + 0.05) + paramNum;
			if (num2 > 100)
			{
				num2++;
			}
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	private RectangleF MeasureString(string text, Font font, RectangleF rectF, bool isAutoFitRow)
	{
		SizeF empty = SizeF.Empty;
		empty = Measure(text, font.Name, font.Bold, font.Italic, font.Size);
		RectangleF result = default(RectangleF);
		result.Height = empty.Height;
		result.Width = empty.Width;
		return result;
	}

	internal SizeF Measure(string strValue, string fontName, bool bold, bool italic, float fontSize)
	{
		return default(SizeF);
	}

	private int MeasureCharacterRanges(IStyle style, string strText, int num, RectangleF rectF)
	{
		FontStyle fontStyle = FontStyle.Regular;
		double num2 = 10.0;
		string fontName = "Arial";
		IOfficeFont font = style.Font;
		if (font != null)
		{
			if (font.Bold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (font.Italic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (font.Strikethrough)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (font.Underline != 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			fontName = font.FontName;
			num2 = font.Size;
		}
		Font font2;
		try
		{
			font2 = CreateFont(fontName, (float)num2, fontStyle);
		}
		catch
		{
			font2 = CreateFont(fontName, (float)num2, fontStyle);
		}
		if (style.Rotation == 90)
		{
			return (int)((double)GetFontHeight(font2) * 1.1 + 0.5) + 6;
		}
		RectangleF rectangleF = MeasureString(strText, font2, rectF, isAutoFitRow: false);
		if (style.Rotation == 0 || style.Rotation == 255)
		{
			int num3 = (int)((double)rectangleF.Width + 0.5) + num;
			if (num3 > 100)
			{
				num3++;
			}
			return num3;
		}
		int num4 = (int)((double)rectangleF.Width + 0.5) + num;
		int num5 = (int)((double)GetFontHeight(font2) * 1.1 + 0.5);
		double num6 = Math.PI * (double)Math.Abs(style.Rotation) / 180.0;
		font2.Dispose();
		font2 = null;
		return (int)((double)num4 * Math.Cos(num6) + (double)num5 * Math.Sin(num6) + 6.5);
	}

	private static void SortTextToFit(IList<object> list, FontImpl fontImpl, string strText, bool AutoFilter)
	{
		FontStyle fontStyle = FontStyle.Regular;
		double num = 10.0;
		string text = "Arial";
		if (fontImpl != null)
		{
			if (((IOfficeFont)fontImpl).Bold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (((IOfficeFont)fontImpl).Italic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (((IOfficeFont)fontImpl).Strikethrough)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (((IOfficeFont)fontImpl).Underline != 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			text = ((IOfficeFont)fontImpl).FontName;
			num = ((IOfficeFont)fontImpl).Size;
		}
		for (int i = 0; i < list.Count; i++)
		{
			StyleWithText styleWithText = (StyleWithText)list[i];
			if (!(styleWithText.fontName == text) || styleWithText.size != num || styleWithText.style != fontStyle)
			{
				continue;
			}
			for (int j = 0; j < styleWithText.strValues.Count; j++)
			{
				if (styleWithText.strValues[j].Length < strText.Length)
				{
					styleWithText.strValues.Insert(j, strText);
					if (styleWithText.strValues.Count > 5)
					{
						styleWithText.strValues.RemoveAt(5);
					}
					return;
				}
			}
			if (styleWithText.strValues.Count < 5)
			{
				styleWithText.strValues.Add(strText);
			}
			return;
		}
		StyleWithText styleWithText2 = new StyleWithText();
		styleWithText2.fontName = text;
		if (AutoFilter)
		{
			styleWithText2.size = num + 1.363;
		}
		else
		{
			styleWithText2.size = num;
		}
		styleWithText2.style = fontStyle;
		styleWithText2.strValues.Add(strText);
		list.Add(styleWithText2);
	}

	internal int CalculateWrappedCell(ExtendedFormatImpl format, string stringValue, int columnWidth, ApplicationImpl applicationImpl)
	{
		int num = 0;
		int num2 = 19;
		if (stringValue == null || stringValue == "")
		{
			return 0;
		}
		IOfficeFont font = format.Font;
		int num3 = (int)((double)(stringValue.Length / 406) * font.Size + (double)(2 * Convert.ToInt32(font.Bold || font.Italic)));
		num = ((num3 < columnWidth) ? columnWidth : num3);
		return MeasureCell(format, stringValue, num, num2, isString: true);
	}

	private int MeasureCell(ExtendedFormatImpl format, string stringValue, float columnWidth, int num, bool isString)
	{
		IOfficeFont font = format.Font;
		double size = font.Size;
		FontStyle fontStyle = FontStyle.Regular;
		if (stringValue[stringValue.Length - 1] == '\n')
		{
			stringValue += "a";
		}
		if (font.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (font.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (font.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (font.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		if (isString && font.FontName == "Times New Roman")
		{
			stringValue = ModifySepicalChar(stringValue);
		}
		using Font font2 = CreateFont(font.FontName, (float)size, fontStyle);
		float x = 0f;
		float y = 0f;
		float height = 600f;
		if (!format.WrapText)
		{
			columnWidth = 600f;
		}
		else if (columnWidth < 100f)
		{
			OfficeHAlign horizontalAlignment = format.HorizontalAlignment;
			if (horizontalAlignment == OfficeHAlign.HAlignLeft || horizontalAlignment == OfficeHAlign.HAlignRight)
			{
				columnWidth -= 1f;
			}
		}
		else
		{
			columnWidth -= 2f;
		}
		RectangleF rectF = new RectangleF(x, y, columnWidth, height);
		RectangleF rectangleF = MeasureString(stringValue, font2, rectF, isAutoFitRow: true);
		int num2 = (int)((double)rectangleF.Height * 1.1 + 0.5);
		if (font.Size >= 20.0 || num2 > 100)
		{
			num2++;
		}
		if (format.WrapText)
		{
			if (size >= 10.0)
			{
				return num2;
			}
			int num3 = CalculateFontHeight(font);
			float num4 = rectangleF.Height;
			if (num4 > 100f)
			{
				num4 += 1f;
			}
			int num5 = (int)Math.Ceiling((double)num4 * 1.0 / (double)num3);
			if (num4 > 100f)
			{
				num5 = (int)((double)num4 * 1.0 / (double)num3) + 1;
			}
			if (num5 == 1)
			{
				return CalculateFontHeightFromGraphics(font);
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < num5; i++)
			{
				stringBuilder.Append("\n0");
			}
			return MeasureFontSize(format, stringBuilder.ToString(), columnWidth);
		}
		int num6 = Math.Abs(format.Rotation);
		if (num6 == 90)
		{
			return (int)((double)rectangleF.Width + 0.5) + num;
		}
		int num7 = (int)((double)rectangleF.Width + 0.5) + num;
		int num8 = (int)((double)GetFontHeight(font2) * 1.1 + 0.5);
		return (int)((double)num7 * Math.Sin(Math.PI * (double)num6 / 180.0) + (double)num8 * Math.Cos(Math.PI * (double)num6 / 180.0) + 6.5);
	}

	private int CalculateFontHeightFromGraphics(IOfficeFont font)
	{
		double size = font.Size;
		FontStyle fontStyle = FontStyle.Regular;
		if (font.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (font.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (font.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (font.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		using Font font2 = CreateFont(font.FontName, (float)size, fontStyle);
		int num = (int)((double)GetFontHeight(font2) * 1.1 + 0.5);
		if (font.Size >= 20.0 || num > 100 || (font.Size == 12.0 && font.Bold))
		{
			num++;
		}
		if (font.Size == 8.0)
		{
			num += 2;
		}
		else if (font.Size < 10.0)
		{
			num++;
		}
		return num;
	}

	private int CalculateFontHeight(IOfficeFont font)
	{
		double size = font.Size;
		FontStyle fontStyle = FontStyle.Regular;
		if (font.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (font.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (font.Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (font.Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		using Font font2 = CreateFont(font.FontName, (float)size, fontStyle);
		return (int)Math.Ceiling(GetFontHeight(font2));
	}

	private float GetFontHeight(Font font)
	{
		return 11f;
	}

	private int MeasureFontSize(ExtendedFormatImpl extendedFromat, string stringValue, float columnWidth)
	{
		if (stringValue == "")
		{
			return 0;
		}
		double num = extendedFromat.Font.Size;
		string fontName;
		if (((fontName = extendedFromat.Font.FontName) == null || fontName != "Calibri") && num < 10.0)
		{
			num = (int)(num * 1.1 + 0.5);
			if (num > 10.0)
			{
				num = 10.0;
			}
		}
		using Font font = CreateFont(extendedFromat.Font.FontName, (float)num, FontStyle.Regular);
		float x = 0f;
		float y = 0f;
		float height = 600f;
		if (!extendedFromat.WrapText)
		{
			columnWidth = 600f;
		}
		RectangleF rectF = new RectangleF(x, y, columnWidth, height);
		return (int)((double)MeasureString(stringValue, font, rectF, isAutoFitRow: true).Height * 1.1 + 0.5);
	}

	private string ModifySepicalChar(string stringValue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = stringValue.ToCharArray();
		for (int i = 0; i < stringValue.Length; i++)
		{
			switch (array[i])
			{
			case ' ':
				if (i != 0)
				{
					char c = array[i - 1];
					if (c == '%' || c == '&')
					{
						stringBuilder.Append(array[i]);
					}
				}
				stringBuilder.Append(array[i]);
				break;
			case '/':
				stringBuilder.Append('W');
				break;
			default:
				stringBuilder.Append(array[i]);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		m_worksheet = null;
		m_rangeImpl = null;
		m_book = null;
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.Office;

internal class StringLayouter
{
	private string m_text;

	private Font m_font;

	private TrueTypeFontStringFormat m_format;

	private SizeF m_size;

	private RectangleF m_rect;

	private float m_pageHeight;

	private StringTokenizer m_reader;

	private bool m_isTabReplaced;

	private int m_tabOccuranceCount;

	internal StringLayoutResult Layout(string text, Font font, TrueTypeFontStringFormat format, RectangleF rect, float pageHeight)
	{
		Initialize(text, font, format, rect, pageHeight);
		StringLayoutResult result = DoLayout();
		Clear();
		return result;
	}

	internal StringLayoutResult Layout(string text, Font font, TrueTypeFontStringFormat format, SizeF size)
	{
		Initialize(text, font, format, size);
		StringLayoutResult result = DoLayout();
		Clear();
		return result;
	}

	internal StringLayoutResult Layout(string text, Font font, TrueTypeFontStringFormat format, SizeF size, out float boundWidth)
	{
		Initialize(text, font, format, size);
		StringLayoutResult result = DoLayout(out boundWidth);
		Clear();
		return result;
	}

	private void Initialize(string text, Font font, TrueTypeFontStringFormat format, RectangleF rect, float pageHeight)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_text = text;
		m_font = font;
		m_format = format;
		m_size = rect.Size;
		m_rect = rect;
		m_pageHeight = pageHeight;
		m_reader = new StringTokenizer(text);
	}

	private void Initialize(string text, Font font, TrueTypeFontStringFormat format, SizeF size)
	{
		Initialize(text, font, format, new RectangleF(PointF.Empty, size), 0f);
	}

	private StringLayoutResult DoLayout()
	{
		StringLayoutResult result = new StringLayoutResult();
		StringLayoutResult stringLayoutResult = new StringLayoutResult();
		List<LineInfo> lines = new List<LineInfo>();
		string text = m_reader.PeekLine();
		float lineIndent = GetLineIndent(firstLine: true);
		while (text != null)
		{
			if (m_format != null && m_format.ComplexScript && m_font is TrueTypeFont && (m_font as TrueTypeFont).InternalFont is UnicodeTrueTypeFont && (m_font as TrueTypeFont).TtfReader.isOTFFont())
			{
				TrueTypeFont trueTypeFont = m_font as TrueTypeFont;
				Dictionary<ScriptTags, int> dictionary = new Dictionary<ScriptTags, int>();
				LanguageUtil languageUtil = new LanguageUtil();
				for (int i = 0; i < text.Length; i++)
				{
					ScriptTags language = languageUtil.GetLanguage(text[i]);
					if (language != 0)
					{
						if (dictionary.ContainsKey(language))
						{
							dictionary[language]++;
						}
						else
						{
							dictionary.Add(language, 1);
						}
					}
				}
				ScriptTags[] array = new ScriptTags[dictionary.Count];
				dictionary.Keys.CopyTo(array, 0);
				bool flag = false;
				ScriptTags[] array2 = array;
				foreach (ScriptTags item in array2)
				{
					if (trueTypeFont.TtfReader.supportedScriptTags.Contains(item))
					{
						flag = true;
						break;
					}
				}
				stringLayoutResult = ((!flag) ? LayoutLine(text, lineIndent) : LayoutLine(text, lineIndent, array));
			}
			else
			{
				stringLayoutResult = LayoutLine(text, lineIndent);
			}
			if (!stringLayoutResult.Empty)
			{
				int numInserted = 0;
				if (!CopyToResult(result, stringLayoutResult, lines, out numInserted))
				{
					m_reader.Read(numInserted);
					break;
				}
			}
			if (stringLayoutResult.Remainder != null && stringLayoutResult.Remainder.Length > 0)
			{
				break;
			}
			m_reader.ReadLine();
			text = m_reader.PeekLine();
			lineIndent = GetLineIndent(firstLine: false);
		}
		FinalizeResult(result, lines);
		return result;
	}

	private StringLayoutResult DoLayout(out float boundWidth)
	{
		StringLayoutResult result = new StringLayoutResult();
		StringLayoutResult stringLayoutResult = new StringLayoutResult();
		List<LineInfo> lines = new List<LineInfo>();
		float boundWidth2 = 0f;
		string text = m_reader.PeekLine();
		float lineIndent = GetLineIndent(firstLine: true);
		while (text != null)
		{
			if (m_format != null && m_format.ComplexScript && m_font is TrueTypeFont && (m_font as TrueTypeFont).InternalFont is UnicodeTrueTypeFont && (m_font as TrueTypeFont).TtfReader.isOTFFont())
			{
				TrueTypeFont trueTypeFont = m_font as TrueTypeFont;
				Dictionary<ScriptTags, int> dictionary = new Dictionary<ScriptTags, int>();
				LanguageUtil languageUtil = new LanguageUtil();
				for (int i = 0; i < text.Length; i++)
				{
					ScriptTags language = languageUtil.GetLanguage(text[i]);
					if (language != 0)
					{
						if (dictionary.ContainsKey(language))
						{
							dictionary[language]++;
						}
						else
						{
							dictionary.Add(language, 1);
						}
					}
				}
				ScriptTags[] array = new ScriptTags[dictionary.Count];
				dictionary.Keys.CopyTo(array, 0);
				bool flag = false;
				ScriptTags[] array2 = array;
				foreach (ScriptTags item in array2)
				{
					if (trueTypeFont.TtfReader.supportedScriptTags.Contains(item))
					{
						flag = true;
						break;
					}
				}
				stringLayoutResult = ((!flag) ? LayoutLine(text, lineIndent, out boundWidth2) : LayoutLine(text, lineIndent, array));
			}
			else
			{
				stringLayoutResult = LayoutLine(text, lineIndent, out boundWidth2);
			}
			if (!stringLayoutResult.Empty)
			{
				int numInserted = 0;
				if (!CopyToResult(result, stringLayoutResult, lines, out numInserted))
				{
					m_reader.Read(numInserted);
					break;
				}
			}
			if (stringLayoutResult.Remainder != null && stringLayoutResult.Remainder.Length > 0)
			{
				break;
			}
			m_reader.ReadLine();
			text = m_reader.PeekLine();
			lineIndent = GetLineIndent(firstLine: false);
		}
		boundWidth = boundWidth2;
		FinalizeResult(result, lines);
		return result;
	}

	private bool CopyToResult(StringLayoutResult result, StringLayoutResult lineResult, List<LineInfo> lines, out int numInserted)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (lineResult == null)
		{
			throw new ArgumentNullException("lineResult");
		}
		if (lines == null)
		{
			throw new ArgumentNullException("lines");
		}
		bool result2 = true;
		bool flag = m_format != null && !m_format.LineLimit;
		float num = result.ActualSize.Height;
		float num2 = m_size.Height;
		if (m_pageHeight > 0f && num2 + m_rect.Y > m_pageHeight)
		{
			num2 = m_rect.Y - m_pageHeight;
			num2 = Math.Max(num2, 0f - num2);
		}
		numInserted = 0;
		if (lineResult.Lines != null)
		{
			int i = 0;
			for (int num3 = lineResult.Lines.Length; i < num3; i++)
			{
				float num4 = num + lineResult.LineHeight;
				if (num4 <= num2 || num2 <= 0f || flag)
				{
					LineInfo lineInfo = lineResult.Lines[i];
					if (!m_isTabReplaced)
					{
						numInserted += lineInfo.Text.Length;
					}
					else
					{
						int num5 = m_tabOccuranceCount * 3;
						numInserted += lineInfo.Text.Length - num5;
						m_isTabReplaced = false;
					}
					lineInfo = TrimLine(lineInfo, lines.Count == 0);
					lines.Add(lineInfo);
					SizeF actualSize = result.ActualSize;
					actualSize.Width = Math.Max(actualSize.Width, lineInfo.Width);
					result.m_actualSize = actualSize;
					if (num4 >= num2 && num2 > 0f && flag)
					{
						if (m_format == null || !m_format.NoClip)
						{
							float num6 = num4 - num2;
							float num7 = lineResult.LineHeight - num6;
							num += num7;
						}
						else
						{
							num = num4;
						}
						result2 = false;
						break;
					}
					num = num4;
					continue;
				}
				result2 = false;
				break;
			}
		}
		if (num != result.ActualSize.Height)
		{
			SizeF actualSize2 = result.ActualSize;
			actualSize2.Height = num;
			result.m_actualSize = actualSize2;
		}
		return result2;
	}

	private void FinalizeResult(StringLayoutResult result, List<LineInfo> lines)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (lines == null)
		{
			throw new ArgumentNullException("lines");
		}
		result.m_lines = lines.ToArray();
		result.m_lineHeight = GetLineHeight();
		if (!m_reader.EOF)
		{
			result.m_remainder = m_reader.ReadToEnd();
		}
		lines.Clear();
	}

	private void Clear()
	{
		m_font = null;
		m_format = null;
		m_reader.Close();
		m_reader = null;
		m_text = null;
	}

	private float GetLineHeight()
	{
		float result = m_font.Height;
		if (m_format != null && m_format.LineSpacing != 0f)
		{
			result = m_format.LineSpacing + m_font.Height;
		}
		return result;
	}

	private StringLayoutResult LayoutLine(string line, float lineIndent, ScriptTags[] tags)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (line.Contains("\t"))
		{
			m_tabOccuranceCount = 0;
			int startIndex = 0;
			string text = "\t";
			while ((startIndex = line.IndexOf(text, startIndex)) != -1)
			{
				startIndex += text.Length;
				m_tabOccuranceCount++;
			}
			line = line.Replace("\t", "    ");
			m_isTabReplaced = true;
		}
		StringLayoutResult stringLayoutResult = new StringLayoutResult();
		stringLayoutResult.m_lineHeight = GetLineHeight();
		List<LineInfo> list = new List<LineInfo>();
		float width = m_size.Width;
		OtfGlyphInfoList glyphList;
		float num = GetLineWidth(line, out glyphList, tags) + lineIndent;
		LineType lineType = LineType.FirstParagraphLine;
		bool readWord = true;
		if (width <= 0f || Math.Round(num, 2) <= Math.Round(width, 2))
		{
			AddToLineResult(stringLayoutResult, list, line, num, LineType.NewLineBreak | lineType, glyphList);
		}
		else if (glyphList != null)
		{
			List<OtfGlyphInfo> list2 = new List<OtfGlyphInfo>();
			OtfGlyphTokenizer otfGlyphTokenizer = new OtfGlyphTokenizer(glyphList);
			string text2 = null;
			OtfGlyphInfo[] array = otfGlyphTokenizer.ReadWord(out text2);
			StringTokenizer stringTokenizer = new StringTokenizer(line);
			string text3 = stringTokenizer.PeekWord();
			float num2 = lineIndent;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			float num3 = 0f;
			float outWordSpace = 0f;
			float outCharSpace = 0f;
			int position = 0;
			bool flag = false;
			bool flag2 = false;
			while (array.Length != 0)
			{
				num3 = otfGlyphTokenizer.GetLineWidth(array, m_font as TrueTypeFont, m_format, stringBuilder.ToString() + text2, out outWordSpace, out outCharSpace);
				if (num2 + num3 + outWordSpace + outCharSpace > width)
				{
					if (text3.Length > 0 && stringBuilder2.Length == 0)
					{
						if (text3.Length == 1)
						{
							stringBuilder2.Append(text3);
							break;
						}
						int num4 = 0;
						stringTokenizer.Position = position;
						text3 = stringTokenizer.Peek().ToString();
						OtfGlyphInfo[] array2 = array;
						foreach (OtfGlyphInfo otfGlyphInfo in array2)
						{
							float num5 = 0f;
							float lineWidth = otfGlyphTokenizer.GetLineWidth(otfGlyphInfo, m_font as TrueTypeFont, m_format);
							if (m_format != null && m_format.CharacterSpacing != 0f)
							{
								if (otfGlyphInfo.Characters != null && otfGlyphInfo.Characters.Length != 0)
								{
									char[] characters = otfGlyphInfo.Characters;
									for (int j = 0; j < characters.Length; j++)
									{
										_ = characters[j];
										num5 += m_format.CharacterSpacing;
									}
								}
								else
								{
									num5 += m_format.CharacterSpacing;
								}
							}
							if (num2 + outWordSpace + outCharSpace + num5 + lineWidth > width)
							{
								if (text3.Length == 1 && stringTokenizer.Position != 0 && stringBuilder2.Length == 0)
								{
									otfGlyphTokenizer.m_position -= array.Length;
									otfGlyphTokenizer.m_position++;
									stringTokenizer.Position++;
									stringBuilder.Append(otfGlyphInfo.Characters);
									stringBuilder2.Append(text3);
									flag = true;
								}
								else
								{
									otfGlyphTokenizer.m_position -= array.Length;
									otfGlyphTokenizer.m_position += num4;
									stringTokenizer.Position = position;
									if (!flag)
									{
										flag2 = true;
									}
								}
								break;
							}
							num4++;
							num2 += lineWidth;
							outCharSpace += num5;
							list2.Add(otfGlyphInfo);
							if (otfGlyphInfo.Characters != null)
							{
								stringBuilder.Append(otfGlyphInfo.Characters);
							}
							stringBuilder2.Append(text3);
							if (otfGlyphInfo.Characters.Length > 1)
							{
								for (int k = 1; k < otfGlyphInfo.Characters.Length; k++)
								{
									stringTokenizer.Read();
									text3 = stringTokenizer.Peek().ToString();
									stringBuilder2.Append(text3);
								}
							}
							stringTokenizer.Read();
							text3 = stringTokenizer.Peek().ToString();
							position = stringTokenizer.Position;
							flag = true;
						}
						AddToLineResult(stringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.LayoutBreak | lineType, new OtfGlyphInfoList(list2));
						lineType = LineType.None;
						list2 = new List<OtfGlyphInfo>();
						num2 = 0f;
						outWordSpace = 0f;
						outCharSpace = 0f;
						stringBuilder = new StringBuilder();
						stringBuilder2 = new StringBuilder();
					}
					else
					{
						otfGlyphTokenizer.m_position -= array.Length;
						stringTokenizer.Position = position;
						AddToLineResult(stringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.LayoutBreak | lineType, new OtfGlyphInfoList(list2));
						lineType = LineType.None;
						list2 = new List<OtfGlyphInfo>();
						num2 = 0f;
						outWordSpace = 0f;
						outCharSpace = 0f;
						stringBuilder = new StringBuilder();
						stringBuilder2 = new StringBuilder();
					}
				}
				else
				{
					OtfGlyphInfo[] array2 = array;
					foreach (OtfGlyphInfo item in array2)
					{
						list2.Add(item);
					}
					num2 += num3;
					stringBuilder.Append(text2);
					stringBuilder2.Append(text3);
				}
				text2 = null;
				text3 = null;
				array = otfGlyphTokenizer.ReadWord(out text2);
				position = stringTokenizer.Position;
				if (flag)
				{
					text3 = stringTokenizer.ReadWord();
					stringTokenizer.Position--;
					flag = false;
				}
				else if (flag2 && stringTokenizer.Position == 0)
				{
					text3 = stringTokenizer.PeekWord();
					flag2 = false;
				}
				else
				{
					stringTokenizer.ReadWord();
					text3 = stringTokenizer.PeekWord();
				}
			}
			if (list2.Count > 0)
			{
				num2 += num3;
				AddToLineResult(stringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.NewLineBreak | LineType.LastParagraphLine, new OtfGlyphInfoList(list2));
			}
		}
		else
		{
			LayoutLine(line, num, lineIndent, stringLayoutResult, width, lineType, readWord, list);
		}
		stringLayoutResult.m_lines = list.ToArray();
		list.Clear();
		return stringLayoutResult;
	}

	private void LayoutLine(string line, float lineIndent, StringLayoutResult lineResult, float maxWidth, LineType lineType, List<LineInfo> lines, ScriptTags[] tags)
	{
		StringTokenizer stringTokenizer = new StringTokenizer(line);
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		float num = lineIndent;
		string text = stringTokenizer.PeekWord();
		OtfGlyphInfoList glyphList = null;
		float num2 = 0f;
		while (text != null)
		{
			stringBuilder.Append(text);
			num2 = GetLineWidth(stringBuilder.ToString(), out glyphList, tags) + num;
			if (num2 > maxWidth)
			{
				stringBuilder = stringBuilder2;
				float lineWidth = num2;
				string line2 = stringBuilder2.ToString();
				OtfGlyphInfoList glyphList2 = glyphList;
				for (int i = 0; i < text.Length; i++)
				{
					stringBuilder.Append(text[i]);
					glyphList = null;
					num2 = GetLineWidth(stringBuilder.ToString(), out glyphList, tags) + num;
					if (num2 > maxWidth)
					{
						AddToLineResult(lineResult, lines, line2, lineWidth, LineType.LayoutBreak | lineType, glyphList2);
						stringBuilder = new StringBuilder();
						stringBuilder2 = new StringBuilder();
						num = 0f;
						num2 = 0f;
						lineType = LineType.None;
						text = text.Substring(i, text.Length - i);
						break;
					}
					lineWidth = num2;
					line2 = stringBuilder.ToString();
					glyphList2 = glyphList;
				}
			}
			else
			{
				stringBuilder2.Append(text);
				stringTokenizer.ReadWord();
				text = stringTokenizer.PeekWord();
			}
		}
		if (stringBuilder2.Length > 0)
		{
			AddToLineResult(lineResult, lines, stringBuilder2.ToString(), num2, LineType.NewLineBreak | LineType.LastParagraphLine, glyphList);
		}
	}

	private void LayoutLine(string line, float lineWidth, float lineIndent, StringLayoutResult lineResult, float maxWidth, LineType lineType, bool readWord, List<LineInfo> lines)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		lineWidth = lineIndent;
		float num = lineIndent;
		StringTokenizer stringTokenizer = new StringTokenizer(line);
		string text = stringTokenizer.PeekWord();
		if (text.Length != stringTokenizer.Length && text == " ")
		{
			stringBuilder2.Append(text);
			stringBuilder.Append(text);
			stringTokenizer.Position++;
			text = stringTokenizer.PeekWord();
		}
		while (text != null)
		{
			stringBuilder2.Append(text);
			float num2 = GetLineWidth(stringBuilder2.ToString()) + num;
			if (stringBuilder2.ToString() == " ")
			{
				stringBuilder2.Length = 0;
				num2 = 0f;
			}
			if (num2 > maxWidth)
			{
				TextAlignment textAlignment = ((m_format != null) ? m_format.Alignment : TextAlignment.Left);
				if (GetWrapType() == WordWrapType.None)
				{
					if (textAlignment != 0)
					{
						break;
					}
					stringBuilder2 = stringBuilder;
					string text2 = text;
					foreach (char value in text2)
					{
						stringBuilder2.Append(value);
						num2 = GetLineWidth(stringBuilder2.ToString()) + num;
						if (num2 > maxWidth)
						{
							stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
							break;
						}
					}
					break;
				}
				if (stringBuilder2.Length == text.Length)
				{
					if (GetWrapType() == WordWrapType.WordOnly)
					{
						lineResult.m_remainder = line.Substring(stringTokenizer.Position);
						break;
					}
					if (stringBuilder2.Length == 1)
					{
						stringBuilder.Append(text);
						break;
					}
					readWord = false;
					stringBuilder2.Length = 0;
					text = stringTokenizer.Peek().ToString();
				}
				else if (GetWrapType() != WordWrapType.Character || !readWord)
				{
					string text3 = stringBuilder.ToString();
					if (text3 != " ")
					{
						AddToLineResult(lineResult, lines, text3, lineWidth, LineType.LayoutBreak | lineType, null);
					}
					stringBuilder2.Length = 0;
					stringBuilder.Length = 0;
					lineWidth = 0f;
					num = 0f;
					num2 = 0f;
					lineType = LineType.None;
					text = (readWord ? text : stringTokenizer.PeekWord());
					readWord = true;
				}
				else
				{
					readWord = false;
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder.ToString());
					text = stringTokenizer.Peek().ToString();
				}
			}
			else
			{
				stringBuilder.Append(text);
				lineWidth = num2;
				if (readWord)
				{
					stringTokenizer.ReadWord();
					text = stringTokenizer.PeekWord();
				}
				else
				{
					stringTokenizer.Read();
					text = stringTokenizer.Peek().ToString();
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			string line2 = stringBuilder.ToString();
			AddToLineResult(lineResult, lines, line2, lineWidth, LineType.NewLineBreak | LineType.LastParagraphLine, null);
		}
		stringTokenizer.Close();
	}

	private StringLayoutResult LayoutLine(string line, float lineIndent)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (line.Contains("\t"))
		{
			m_tabOccuranceCount = 0;
			int startIndex = 0;
			string text = "\t";
			while ((startIndex = line.IndexOf(text, startIndex)) != -1)
			{
				startIndex += text.Length;
				m_tabOccuranceCount++;
			}
			line = line.Replace("\t", "    ");
			m_isTabReplaced = true;
		}
		StringLayoutResult stringLayoutResult = new StringLayoutResult();
		stringLayoutResult.m_lineHeight = GetLineHeight();
		List<LineInfo> list = new List<LineInfo>();
		float width = m_size.Width;
		float num = GetLineWidth(line) + lineIndent;
		LineType lineType = LineType.FirstParagraphLine;
		bool readWord = true;
		if (width <= 0f || Math.Round(num, 2) <= Math.Round(width, 2))
		{
			AddToLineResult(stringLayoutResult, list, line, num, LineType.NewLineBreak | lineType, null);
		}
		else
		{
			LayoutLine(line, num, lineIndent, stringLayoutResult, width, lineType, readWord, list);
		}
		stringLayoutResult.m_lines = list.ToArray();
		list.Clear();
		return stringLayoutResult;
	}

	private StringLayoutResult LayoutLine(string line, float lineIndent, out float boundWidth)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (line.Contains("\t"))
		{
			m_tabOccuranceCount = 0;
			int startIndex = 0;
			string text = "\t";
			while ((startIndex = line.IndexOf(text, startIndex)) != -1)
			{
				startIndex += text.Length;
				m_tabOccuranceCount++;
			}
			line = line.Replace("\t", "    ");
			m_isTabReplaced = true;
		}
		StringLayoutResult stringLayoutResult = new StringLayoutResult();
		stringLayoutResult.m_lineHeight = GetLineHeight();
		List<LineInfo> list = new List<LineInfo>();
		float width = m_size.Width;
		float num = GetLineWidth(line, out boundWidth) + lineIndent;
		LineType lineType = LineType.FirstParagraphLine;
		bool readWord = true;
		if (width <= 0f || Math.Round(num, 2) <= Math.Round(width, 2))
		{
			AddToLineResult(stringLayoutResult, list, line, num, LineType.NewLineBreak | lineType, null);
		}
		else
		{
			LayoutLine(line, num, lineIndent, stringLayoutResult, width, lineType, readWord, list);
		}
		stringLayoutResult.m_lines = list.ToArray();
		list.Clear();
		return stringLayoutResult;
	}

	private void AddToLineResult(StringLayoutResult lineResult, List<LineInfo> lines, string line, float lineWidth, LineType breakType, OtfGlyphInfoList glyphList)
	{
		if (lineResult == null)
		{
			throw new ArgumentNullException("lineResult");
		}
		if (lines == null)
		{
			throw new ArgumentNullException("lines");
		}
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		LineInfo lineInfo = new LineInfo();
		if (glyphList != null)
		{
			lineInfo.OpenTypeGlyphList = new OtfGlyphInfoList(glyphList.Glyphs);
		}
		lineInfo.Text = line;
		lineInfo.Width = lineWidth;
		lineInfo.LineType = breakType;
		lines.Add(lineInfo);
		SizeF actualSize = lineResult.ActualSize;
		actualSize.Height += GetLineHeight();
		actualSize.Width = Math.Max(actualSize.Width, lineWidth);
		lineResult.m_actualSize = actualSize;
	}

	private LineInfo TrimLine(LineInfo info, bool firstLine)
	{
		string text = info.Text;
		float num = info.Width;
		bool num2 = (info.LineType & LineType.FirstParagraphLine) == 0;
		bool flag = m_format == null || !m_format.RightToLeft;
		char[] spaces = StringTokenizer.Spaces;
		OtfGlyphTokenizer otfGlyphTokenizer = new OtfGlyphTokenizer();
		if (num2)
		{
			text = (flag ? text.TrimStart(spaces) : text.TrimEnd(spaces));
			if (info.OpenTypeGlyphList != null)
			{
				info.OpenTypeGlyphList = (flag ? otfGlyphTokenizer.TrimStartSpaces(info.OpenTypeGlyphList) : otfGlyphTokenizer.TrimEndSpaces(info.OpenTypeGlyphList));
			}
		}
		if (m_format == null || !m_format.MeasureTrailingSpaces)
		{
			if ((info.LineType & LineType.FirstParagraphLine) > LineType.None && StringTokenizer.IsWhitespace(text))
			{
				text = new string(' ', 1);
				if (m_font.Underline)
				{
					text = string.Empty;
				}
			}
			else
			{
				text = (flag ? text.TrimEnd(spaces) : text.TrimStart(spaces));
				if (info.OpenTypeGlyphList != null)
				{
					info.OpenTypeGlyphList = (flag ? otfGlyphTokenizer.TrimEndSpaces(info.OpenTypeGlyphList) : otfGlyphTokenizer.TrimStartSpaces(info.OpenTypeGlyphList));
				}
			}
		}
		if (text.Length != info.Text.Length)
		{
			if (info.OpenTypeGlyphList != null)
			{
				num = otfGlyphTokenizer.GetLineWidth(info.OpenTypeGlyphList.Glyphs.ToArray(), m_font as TrueTypeFont, m_format, text, out var outWordSpace, out var outCharSpace);
				num += outWordSpace;
				num += outCharSpace;
			}
			else
			{
				num = GetLineWidth(text);
			}
			if ((info.LineType & LineType.FirstParagraphLine) > LineType.None)
			{
				num += GetLineIndent(firstLine);
			}
		}
		info.Text = text;
		info.Width = num;
		return info;
	}

	private float GetLineWidth(string line)
	{
		return m_font.GetLineWidth(line, m_format);
	}

	private float GetLineWidth(string line, out float boundWidth)
	{
		return m_font.GetLineWidth(line, m_format, out boundWidth);
	}

	private float GetLineWidth(string line, out OtfGlyphInfoList glyphList, ScriptTags[] tags)
	{
		glyphList = null;
		float num = 0f;
		if (m_font is TrueTypeFont)
		{
			TrueTypeFont trueTypeFont = m_font as TrueTypeFont;
			if (trueTypeFont.Unicode)
			{
				return trueTypeFont.GetLineWidth(line, m_format, out glyphList, tags);
			}
			return m_font.GetLineWidth(line, m_format);
		}
		return m_font.GetLineWidth(line, m_format);
	}

	private float GetLineIndent(bool firstLine)
	{
		float result = 0f;
		if (m_format != null)
		{
			result = (firstLine ? m_format.FirstLineIndent : m_format.ParagraphIndent);
			result = ((m_size.Width > 0f) ? Math.Min(m_size.Width, result) : result);
		}
		return result;
	}

	private WordWrapType GetWrapType()
	{
		return WordWrapType.Word;
	}
}

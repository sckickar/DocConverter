using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics.Fonts;

namespace DocGen.Pdf.Graphics;

public class PdfStringLayouter
{
	private string m_text;

	private PdfFont m_font;

	private PdfStringFormat m_format;

	private SizeF m_size;

	private RectangleF m_rect;

	private float m_pageHeight;

	private StringTokenizer m_reader;

	private bool m_isTabReplaced;

	private int m_tabOccuranceCount;

	internal PdfStringLayoutResult Layout(string text, PdfFont font, PdfStringFormat format, RectangleF rect, float pageHeight)
	{
		Initialize(text, font, format, rect, pageHeight);
		PdfStringLayoutResult result = DoLayout();
		Clear();
		return result;
	}

	public PdfStringLayoutResult Layout(string text, PdfFont font, PdfStringFormat format, SizeF size)
	{
		Initialize(text, font, format, size);
		PdfStringLayoutResult result = DoLayout();
		Clear();
		return result;
	}

	private void Initialize(string text, PdfFont font, PdfStringFormat format, RectangleF rect, float pageHeight)
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

	private void Initialize(string text, PdfFont font, PdfStringFormat format, SizeF size)
	{
		Initialize(text, font, format, new RectangleF(PointF.Empty, size), 0f);
	}

	private PdfStringLayoutResult DoLayout()
	{
		PdfStringLayoutResult result = new PdfStringLayoutResult();
		PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayoutResult();
		List<LineInfo> lines = new List<LineInfo>();
		string text = m_reader.PeekLine();
		float lineIndent = GetLineIndent(firstLine: true);
		while (text != null)
		{
			if (m_format != null && m_format.ComplexScript && m_font is PdfTrueTypeFont && (m_font as PdfTrueTypeFont).InternalFont is UnicodeTrueTypeFont && (m_font as PdfTrueTypeFont).TtfReader.isOTFFont())
			{
				PdfTrueTypeFont pdfTrueTypeFont = m_font as PdfTrueTypeFont;
				Dictionary<ScriptTags, int> dictionary = new Dictionary<ScriptTags, int>();
				LanguageUtil languageUtil = new LanguageUtil();
				for (int i = 0; i < text.Length; i++)
				{
					ScriptTags language = languageUtil.GetLanguage(text[i]);
					ScriptTags scriptTags = language;
					if (scriptTags != 0)
					{
						scriptTags = languageUtil.GetGlyphTag(text[i]);
					}
					if (language != 0 && !ScriptTags.Common.Equals(scriptTags) && !ScriptTags.Unknown.Equals(scriptTags) && !ScriptTags.Inherited.Equals(scriptTags))
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
					if (pdfTrueTypeFont.TtfReader.supportedScriptTags.Contains(item))
					{
						flag = true;
						break;
					}
				}
				pdfStringLayoutResult = ((!flag) ? LayoutLine(text, lineIndent) : LayoutLine(text, lineIndent, array));
			}
			else
			{
				pdfStringLayoutResult = LayoutLine(text, lineIndent);
			}
			int numInserted = 0;
			if (!pdfStringLayoutResult.Empty && !CopyToResult(result, pdfStringLayoutResult, lines, out numInserted))
			{
				m_reader.Read(numInserted);
				break;
			}
			if (pdfStringLayoutResult.Remainder != null && pdfStringLayoutResult.Remainder.Length > 0)
			{
				if (numInserted > 0 && m_reader.Position == 0)
				{
					m_reader.Position = numInserted;
				}
				break;
			}
			m_reader.ReadLine();
			text = m_reader.PeekLine();
			lineIndent = GetLineIndent(firstLine: false);
		}
		FinalizeResult(result, lines);
		return result;
	}

	private bool CopyToResult(PdfStringLayoutResult result, PdfStringLayoutResult lineResult, List<LineInfo> lines, out int numInserted)
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
			if (num2 <= 0f || m_pageHeight - m_rect.Y < 0f)
			{
				result2 = false;
				num2 = 0.01f;
			}
		}
		numInserted = 0;
		if (lineResult.Lines != null)
		{
			int i = 0;
			for (int num3 = lineResult.Lines.Length; i < num3; i++)
			{
				float num4 = num + lineResult.LineHeight;
				if (i == lineResult.Lines.Length - 1 && m_format != null && !m_format.m_isList && m_format.LineSpacing < 0f)
				{
					num4 -= m_format.LineSpacing;
				}
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

	private void FinalizeResult(PdfStringLayoutResult result, List<LineInfo> lines)
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
		if (lines.Count != 0 && lines[lines.Count - 1].m_lineType == (LineType.NewLineBreak | LineType.FirstParagraphLine) && m_reader.Position == 0 && m_reader.Length >= 2)
		{
			m_reader.Position = 2;
		}
		if (!m_reader.EOF)
		{
			result.m_remainder = m_reader.ReadToEnd();
		}
		else if (m_reader.Length > 0 && lines.Count == 0 && m_reader.EOF)
		{
			m_reader.Position = 0;
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

	private PdfStringLayoutResult LayoutLine(string line, float lineIndent, ScriptTags[] tags)
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
		PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayoutResult();
		pdfStringLayoutResult.m_lineHeight = GetLineHeight();
		List<LineInfo> list = new List<LineInfo>();
		float width = m_size.Width;
		OtfGlyphInfoList glyphList;
		float num = GetLineWidth(line, out glyphList, tags) + lineIndent;
		LineType lineType = LineType.FirstParagraphLine;
		bool readWord = true;
		if (width <= 0f || Math.Round(num, 2) <= Math.Round(width, 2))
		{
			AddToLineResult(pdfStringLayoutResult, list, line, num, LineType.NewLineBreak | lineType, glyphList);
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
				num3 = otfGlyphTokenizer.GetLineWidth(array, m_font as PdfTrueTypeFont, m_format, stringBuilder.ToString() + text2, out outWordSpace, out outCharSpace);
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
							float lineWidth = otfGlyphTokenizer.GetLineWidth(otfGlyphInfo, m_font as PdfTrueTypeFont, m_format);
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
						AddToLineResult(pdfStringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.LayoutBreak | lineType, new OtfGlyphInfoList(list2));
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
						AddToLineResult(pdfStringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.LayoutBreak | lineType, new OtfGlyphInfoList(list2));
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
				AddToLineResult(pdfStringLayoutResult, list, stringBuilder2.ToString(), num2 + outWordSpace + outCharSpace, LineType.NewLineBreak | LineType.LastParagraphLine, new OtfGlyphInfoList(list2));
			}
		}
		else
		{
			LayoutLine(line, num, lineIndent, pdfStringLayoutResult, width, lineType, readWord, list);
		}
		if (m_format != null && m_format.TextDirection == PdfTextDirection.RightToLeft && list.Count > 1)
		{
			m_format.isCustomRendering = true;
			UpdateLineInfoBidiLevels(line, list);
		}
		pdfStringLayoutResult.m_lines = list.ToArray();
		list.Clear();
		return pdfStringLayoutResult;
	}

	private void LayoutLine(string line, float lineIndent, PdfStringLayoutResult lineResult, float maxWidth, LineType lineType, List<LineInfo> lines, ScriptTags[] tags)
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

	private void LayoutLine(string line, float lineWidth, float lineIndent, PdfStringLayoutResult lineResult, float maxWidth, LineType lineType, bool readWord, List<LineInfo> lines)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		lineWidth = lineIndent;
		float num = lineIndent;
		StringTokenizer stringTokenizer = new StringTokenizer(line);
		string text = stringTokenizer.PeekWord();
		bool flag = false;
		if (text.Length != stringTokenizer.Length && text == " ")
		{
			stringBuilder2.Append(text);
			stringBuilder.Append(text);
			stringTokenizer.Position++;
			text = stringTokenizer.PeekWord();
			bool flag2 = m_format != null && m_format.m_paragraphIndent == 0f;
			flag = GetWrapType() == PdfWordWrapType.Word && flag2;
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
				if (GetWrapType() == PdfWordWrapType.None)
				{
					stringBuilder2 = stringBuilder;
					string text2 = text;
					foreach (char value in text2)
					{
						stringBuilder2.Append(value);
						num2 = GetLineWidth(stringBuilder2.ToString()) + num;
						if (num2 > maxWidth)
						{
							if (m_format == null || !m_format.NoClip)
							{
								stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
							}
							break;
						}
						lineWidth = num2;
					}
					break;
				}
				if (stringBuilder2.Length == text.Length)
				{
					if (GetWrapType() == PdfWordWrapType.WordOnly)
					{
						lineResult.m_remainder = line.Substring(stringTokenizer.Position);
						break;
					}
					if (stringBuilder2.Length == 1)
					{
						if (num2 < maxWidth)
						{
							stringBuilder.Append(text);
						}
						break;
					}
					readWord = false;
					stringBuilder2.Length = 0;
					text = stringTokenizer.Peek().ToString();
				}
				else if (GetWrapType() != PdfWordWrapType.Character || !readWord)
				{
					string text3 = stringBuilder.ToString();
					if (GetWrapType() == PdfWordWrapType.DiscretionaryHyphen && ((!text3.EndsWith(" ") && (text3.Contains("-") || text3.Contains("\ufffd"))) || (text3.EndsWith(" ") && BrokeHyphenContent(maxWidth, lineWidth, num, stringTokenizer))))
					{
						if (text3.EndsWith(" "))
						{
							text3 += (text = (readWord ? text : stringTokenizer.PeekWord()));
						}
						int posHyphen = -1;
						ReadHyphenLocation(text3, maxWidth, num, out posHyphen);
						if (posHyphen > 0)
						{
							text3 = text3.Substring(0, posHyphen);
							lineWidth = GetLineWidth(text3.ToString()) + num;
							AddToLineResult(lineResult, lines, text3, lineWidth, LineType.LayoutBreak | lineType, null);
							stringTokenizer.Position -= stringBuilder.Length - text3.Length;
							stringBuilder2.Length = 0;
							stringBuilder.Length = 0;
							lineWidth = 0f;
							num = 0f;
							num2 = 0f;
							lineType = LineType.None;
							text = (readWord ? text : stringTokenizer.PeekWord());
							readWord = true;
						}
					}
					else
					{
						if (text3 != " ")
						{
							AddToLineResult(lineResult, lines, text3, lineWidth, LineType.LayoutBreak | lineType, null);
						}
						if (lines.Count == 0 && flag && text != " " && text.Length > 1)
						{
							readWord = false;
							stringBuilder2.Length = 0;
							stringBuilder2.Append(stringBuilder.ToString());
							text = stringTokenizer.Peek().ToString();
							continue;
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
				else if (!stringTokenizer.EOF)
				{
					stringTokenizer.Read();
					text = stringTokenizer.Peek().ToString();
				}
				else
				{
					text = null;
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

	private bool BrokeHyphenContent(float maxWidth, float lineWidth, float curIndent, StringTokenizer reader)
	{
		string text = reader.PeekWord();
		if (text.Contains("-") || text.Contains("\ufffd"))
		{
			int nextHyphen = -1;
			float maxWidth2 = maxWidth - lineWidth;
			ReadNearHyphen(text, maxWidth2, curIndent, out nextHyphen);
			if (nextHyphen == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void ReadNearHyphen(string ln, float maxWidth, float curIndent, out int nextHyphen)
	{
		string text = ln;
		nextHyphen = GetFirstHyphenIndex(text);
		if (nextHyphen > 0)
		{
			nextHyphen++;
			text = text.Substring(0, nextHyphen);
		}
		if (GetLineWidth(text.ToString()) + curIndent > maxWidth)
		{
			nextHyphen = 0;
		}
		else
		{
			nextHyphen = text.Length;
		}
	}

	private int GetLastHyphenIndex(string text)
	{
		int result = -1;
		char[] array = text.ToCharArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (array[num] == '-' || array[num] == '\u00ad')
			{
				result = num;
				break;
			}
		}
		return result;
	}

	private int GetFirstHyphenIndex(string text)
	{
		int result = -1;
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '-' || array[i] == '\u00ad')
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void ReadHyphenLocation(string ln, float maxWidth, float curIndent, out int posHyphen)
	{
		string text = ln;
		posHyphen = GetLastHyphenIndex(text);
		if (posHyphen > -1)
		{
			posHyphen++;
			text = text.Substring(0, posHyphen);
		}
		if (GetLineWidth(text.ToString()) + curIndent > maxWidth)
		{
			text = text.Remove(text.Length - 1, 1);
			ReadHyphenLocation(text, maxWidth, curIndent, out posHyphen);
		}
		else
		{
			posHyphen = text.Length;
		}
	}

	private PdfStringLayoutResult LayoutLine(string line, float lineIndent)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		PdfStandardFont pdfStandardFont = m_font as PdfStandardFont;
		if ((line.Contains("\t") && pdfStandardFont != null && pdfStandardFont.fontEncoding == null) || (line.Contains("\t") && pdfStandardFont == null))
		{
			m_tabOccuranceCount = 0;
			int num = 0;
			string text = "\t";
			while (line.Length > num && (num = line.IndexOf(text, num)) != -1)
			{
				num += text.Length;
				m_tabOccuranceCount++;
			}
			line = line.Replace("\t", "    ");
			m_isTabReplaced = true;
		}
		PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayoutResult();
		pdfStringLayoutResult.m_lineHeight = GetLineHeight();
		List<LineInfo> list = new List<LineInfo>();
		float width = m_size.Width;
		float num2 = GetLineWidth(line) + lineIndent;
		LineType lineType = LineType.FirstParagraphLine;
		bool readWord = true;
		if (width <= 0f || Math.Round(num2, 2) <= Math.Round(width, 2))
		{
			AddToLineResult(pdfStringLayoutResult, list, line, num2, LineType.NewLineBreak | lineType, null);
		}
		else
		{
			LayoutLine(line, num2, lineIndent, pdfStringLayoutResult, width, lineType, readWord, list);
		}
		if (m_format != null && m_format.TextDirection == PdfTextDirection.RightToLeft && list.Count > 1)
		{
			m_format.isCustomRendering = true;
			UpdateLineInfoBidiLevels(line, list);
		}
		pdfStringLayoutResult.m_lines = list.ToArray();
		list.Clear();
		return pdfStringLayoutResult;
	}

	private void AddToLineResult(PdfStringLayoutResult lineResult, List<LineInfo> lines, string line, float lineWidth, LineType breakType, OtfGlyphInfoList glyphList)
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
		if (m_format != null && m_format.MeasureTiltingSpace && m_font is PdfTrueTypeFont)
		{
			double num = 11.0;
			PdfTrueTypeFont pdfTrueTypeFont = m_font as PdfTrueTypeFont;
			if (pdfTrueTypeFont.Italic && !pdfTrueTypeFont.TtfReader.Metrics.IsItalic)
			{
				m_format.TiltingSpace = (float)((double)pdfTrueTypeFont.Height * Math.Tan(num / 180.0 * Math.PI));
			}
			lineWidth += m_format.TiltingSpace;
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
			else if (text != info.Text)
			{
				info.TrimCount = info.Text.Length - text.Length;
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
				num = otfGlyphTokenizer.GetLineWidth(info.OpenTypeGlyphList.Glyphs.ToArray(), m_font as PdfTrueTypeFont, m_format, text, out var outWordSpace, out var outCharSpace);
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

	private float GetLineWidth(string line, out OtfGlyphInfoList glyphList, ScriptTags[] tags)
	{
		glyphList = null;
		float num = 0f;
		if (m_font is PdfTrueTypeFont)
		{
			PdfTrueTypeFont pdfTrueTypeFont = m_font as PdfTrueTypeFont;
			if (pdfTrueTypeFont.Unicode)
			{
				return pdfTrueTypeFont.GetLineWidth(line, m_format, out glyphList, tags);
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
			result = (firstLine ? m_format.FirstLineIndent : m_format.m_paragraphIndent);
			result = ((m_size.Width > 0f) ? Math.Min(m_size.Width, result) : result);
		}
		return result;
	}

	private PdfWordWrapType GetWrapType()
	{
		if (m_format == null)
		{
			return PdfWordWrapType.Word;
		}
		return m_format.WordWrap;
	}

	private void UpdateLineInfoBidiLevels(string line, List<LineInfo> lines)
	{
		string empty = string.Empty;
		empty = new ArabicShapeRenderer().Shape(line.ToCharArray(), 0);
		if (line.Length == empty.Length)
		{
			Bidi bidi = new Bidi();
			bidi.m_isVisualOrder = false;
			bidi.GetLogicalToVisualString(line, isRTL: true);
			byte[] indexLevels = bidi.IndexLevels;
			int num = 0;
			for (int i = 0; i < lines.Count; i++)
			{
				LineInfo lineInfo = lines[i];
				byte[] array = new byte[lineInfo.Text.Length];
				Array.Copy(indexLevels, num, array, 0, array.Length);
				num += array.Length;
				lineInfo.BidiLevels = array;
			}
		}
	}
}

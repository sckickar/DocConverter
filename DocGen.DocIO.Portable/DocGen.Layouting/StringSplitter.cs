using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class StringSplitter
{
	private DrawingContext m_dc;

	private Hyphenator hyphen;

	public StringSplitter()
	{
		m_dc = new DrawingContext();
	}

	public StringSplitResult Split(string text, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, StringFormat format, SizeF size, WCharacterFormat charFormat, ref bool isLastWordFit, bool isTabStopInterSectingfloattingItem, ref bool isTrailSpacesWrapped, bool isAutoHyphenated, IStringWidget strWidget, ref bool isHyphenated)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		return DoSplit(ref isLastWordFit, text, font, defaultFont, charFormat, format, size, isTabStopInterSectingfloattingItem, ref isTrailSpacesWrapped, isAutoHyphenated, strWidget, ref isHyphenated);
	}

	internal void Close()
	{
		m_dc.Close();
	}

	private StringSplitResult DoSplit(ref bool isLastWordFit, string text, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat, StringFormat format, SizeF size, bool isTabStopInterSectingfloattingItem, ref bool isTrailSpacesWrapped, bool isAutoHyphenated, IStringWidget strWidget, ref bool isHyphenated)
	{
		StringSplitResult result = new StringSplitResult();
		StringSplitResult stringSplitResult = new StringSplitResult();
		List<TextLineInfo> lines = new List<TextLineInfo>();
		StringParser stringParser = new StringParser(text);
		string text2 = stringParser.PeekLine();
		float lineIndent = GetLineIndent(firstLine: true, format, size);
		while (text2 != null)
		{
			stringSplitResult = SplitLine(text2, lineIndent, ref isLastWordFit, font, defaultFont, charFormat, format, size, isTabStopInterSectingfloattingItem, ref isTrailSpacesWrapped, isAutoHyphenated, strWidget as IEntity, ref isHyphenated);
			if (stringParser.Length == text2.Length)
			{
				return stringSplitResult;
			}
			if (!stringSplitResult.Empty)
			{
				int numInserted = 0;
				if (!CopyToResult(result, stringSplitResult, lines, out numInserted, font, defaultFont, charFormat, format, size))
				{
					stringParser.Read(numInserted);
					break;
				}
				if (stringParser.Length == stringParser.Position)
				{
					break;
				}
			}
			if (stringSplitResult.Remainder != null && stringSplitResult.Remainder.Length > 0)
			{
				break;
			}
			stringParser.ReadLine();
			text2 = stringParser.PeekLine();
			lineIndent = GetLineIndent(firstLine: false, format, size);
		}
		SaveResult(result, lines, stringParser, font, text);
		return result;
	}

	private bool CopyToResult(StringSplitResult result, StringSplitResult lineResult, List<TextLineInfo> lines, out int numInserted, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat, StringFormat format, SizeF textSize)
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
		bool flag = format != null && format.FormatFlags != StringFormatFlags.LineLimit;
		float num = result.ActualSize.Height;
		float height = textSize.Height;
		numInserted = 0;
		if (lineResult.Lines != null)
		{
			int i = 0;
			for (int num2 = lineResult.Lines.Length; i < num2; i++)
			{
				float num3 = num + lineResult.LineHeight;
				if (num3 <= height || height <= 0f || flag)
				{
					TextLineInfo info = lineResult.Lines[i];
					numInserted += info.Line.Length;
					info = TrimLine(info, lines.Count == 0, font, defaultFont, charFormat, format, textSize);
					lines.Add(info);
					SizeF actualSize = result.ActualSize;
					actualSize.Width = Math.Max(actualSize.Width, info.Width);
					result.ActualSize = actualSize;
					if (num3 >= height && height > 0f && flag)
					{
						if (format.FormatFlags != StringFormatFlags.NoClip)
						{
							float num4 = num3 - height;
							float num5 = lineResult.LineHeight - num4;
							num += num5;
						}
						else
						{
							num = num3;
						}
						result2 = false;
						break;
					}
					num = num3;
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
			result.ActualSize = actualSize2;
		}
		return result2;
	}

	private void SaveResult(StringSplitResult result, List<TextLineInfo> lines, StringParser reader, DocGen.Drawing.Font m_font, string m_text)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (lines == null)
		{
			throw new ArgumentNullException("lines");
		}
		result.Lines = lines.ToArray();
		result.LineHeight = GetLineHeight(m_font, FontScriptType.English);
		if (!reader.EOF)
		{
			int num = 0;
			if (lines.Count > 0)
			{
				num = lines[0].Line.Length;
			}
			result.Remainder = m_text.Substring(num, m_text.Length - num).TrimStart(StringParser.Spaces);
		}
		lines.Clear();
	}

	private float GetLineHeight(DocGen.Drawing.Font m_font, FontScriptType scriptType)
	{
		return WordDocument.RenderHelper.GetFontHeight(m_font, scriptType);
	}

	private StringSplitResult SplitLine(string line, float lineIndent, ref bool isLastWordFit, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat, StringFormat format, SizeF size, bool isTabStopInterSectingfloattingItem, ref bool isTrailSpacesWrapped, bool isAutoHyphenated, IEntity strWidget, ref bool isHyphenated)
	{
		List<string> list = null;
		IEntity previousSibling = (strWidget as Entity).PreviousSibling;
		bool flag = false;
		if (previousSibling is Shape)
		{
			flag = (previousSibling as Shape).IsHorizontalRule;
		}
		else if (previousSibling is WPicture && (previousSibling as WPicture).IsShape)
		{
			flag = (previousSibling as WPicture).PictureShape.IsHorizontalRule;
		}
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		line = line.Replace(ControlChar.Tab, "    ");
		StringSplitResult stringSplitResult = new StringSplitResult();
		stringSplitResult.LineHeight = GetLineHeight(font, FontScriptType.English);
		List<TextLineInfo> list2 = new List<TextLineInfo>();
		float width = size.Width;
		float num = GetLineWidth(line, font, defaultFont, charFormat) + lineIndent;
		TextLineType textLineType = TextLineType.FirstParagraphLine;
		bool flag2 = true;
		bool flag3 = false;
		if ((width <= 0f || num <= width) && width > 0f)
		{
			AddToLineResult(stringSplitResult, list2, line, num, TextLineType.NewLineBreak | textLineType, font);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			num = lineIndent;
			StringParser stringParser = new StringParser(line);
			string text = stringParser.PeekWord(strWidget);
			if (text.Length != stringParser.Length && text == " ")
			{
				stringParser.Position++;
				text = " " + stringParser.PeekWord(strWidget);
			}
			while (text != null)
			{
				stringBuilder2.Append(text);
				bool flag4 = false;
				float num2 = GetLineWidth(stringBuilder2.ToString(), font, defaultFont, charFormat);
				if (num2 > width && stringBuilder2.ToString().TrimEnd(ControlChar.SpaceChar) == line.TrimEnd(ControlChar.SpaceChar) && GetLineWidth(stringBuilder2.ToString().TrimEnd(ControlChar.SpaceChar), font, defaultFont, charFormat) <= width)
				{
					num2 = width;
					text = text.TrimEnd(ControlChar.SpaceChar);
					isTrailSpacesWrapped = true;
				}
				if (num2 > width)
				{
					if (GetWrapType(format) == StringTrimming.None)
					{
						break;
					}
					if (stringBuilder2.Length == text.Length)
					{
						int num3 = line.Split((char[]?)null).Length;
						if (GetWrapType(format) == StringTrimming.Word || num3 == 1)
						{
							if (!charFormat.OwnerBase.Document.DOP.HyphCapitals && text.ToUpper().Equals(text, StringComparison.Ordinal))
							{
								isAutoHyphenated = false;
							}
							if (isAutoHyphenated && text.Contains("-"))
							{
								isAutoHyphenated = false;
							}
							if (isAutoHyphenated && text.Trim() != "")
							{
								string languageCode = ((LocaleIDs)charFormat.LocaleIdASCII).ToString().Replace('_', '-');
								GetDictionary(charFormat, languageCode);
								if (hyphen == null)
								{
									stringSplitResult.Remainder = line.Substring(stringParser.Position);
									if (text.StartsWith(" "))
									{
										stringBuilder.Append(line.Substring(0, stringParser.Position));
									}
									break;
								}
								if (text.ToUpper().Equals(text, StringComparison.Ordinal))
								{
									flag4 = true;
									text = text.ToLower();
								}
								list = new List<string>(hyphen.HyphenateText(text).Split('='));
								if (text.ToLower().Equals(text, StringComparison.Ordinal) && flag4)
								{
									text = text.ToUpper();
								}
								string fittedWord = "";
								string remainingWord = "";
								SplitAsPerAutoHyphenation(width, list, ref fittedWord, ref remainingWord, font, defaultFont, charFormat);
								list.Clear();
								list = null;
								if (fittedWord != "")
								{
									stringBuilder.Append(fittedWord + "-");
									stringSplitResult.Remainder = remainingWord;
									flag3 = true;
									isHyphenated = true;
									break;
								}
							}
							stringSplitResult.Remainder = line.Substring(stringParser.Position);
							if (text.StartsWith(" "))
							{
								stringBuilder.Append(line.Substring(0, stringParser.Position));
							}
							break;
						}
						flag2 = false;
						stringBuilder2.Length = 0;
						text = stringParser.Peek().ToString();
						continue;
					}
					if (GetWrapType(format) != StringTrimming.Character || !flag2)
					{
						if (!charFormat.OwnerBase.Document.DOP.HyphCapitals && text.ToUpper().Equals(text, StringComparison.Ordinal))
						{
							isAutoHyphenated = false;
						}
						if (isAutoHyphenated && text.Contains("-"))
						{
							isAutoHyphenated = false;
						}
						if (!isAutoHyphenated || !(text.Trim() != ""))
						{
							break;
						}
						string languageCode2 = ((LocaleIDs)charFormat.LocaleIdASCII).ToString().Replace('_', '-');
						GetDictionary(charFormat, languageCode2);
						if (hyphen != null)
						{
							if (text.ToUpper().Equals(text, StringComparison.Ordinal))
							{
								flag4 = true;
								text = text.ToLower();
							}
							list = new List<string>(hyphen.HyphenateText(text).Split('='));
							if (text.ToLower().Equals(text, StringComparison.Ordinal) && flag4)
							{
								text = text.ToUpper();
							}
							string line2 = stringBuilder2.ToString().Substring(0, stringBuilder2.ToString().LastIndexOf(text));
							float lineWidth = GetLineWidth(line2, font, defaultFont, charFormat);
							float maxWidth = width - lineWidth;
							string fittedWord2 = "";
							string remainingWord2 = "";
							SplitAsPerAutoHyphenation(maxWidth, list, ref fittedWord2, ref remainingWord2, font, defaultFont, charFormat);
							if (fittedWord2 != "")
							{
								stringBuilder.Append(fittedWord2 + "-");
								stringSplitResult.Remainder = remainingWord2;
								flag3 = true;
								isHyphenated = true;
							}
						}
						break;
					}
					flag2 = false;
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder.ToString());
					text = stringParser.Peek().ToString();
				}
				else
				{
					stringBuilder.Append(text);
					num = num2;
					if (flag2)
					{
						stringParser.ReadWord(strWidget);
						text = stringParser.PeekWord(strWidget);
					}
					else
					{
						stringParser.Read();
						text = stringParser.Peek().ToString();
					}
				}
			}
			if (stringBuilder.Length > 0 || (stringBuilder2.ToString().TrimEnd(ControlChar.SpaceChar) == string.Empty && !flag) || isTabStopInterSectingfloattingItem)
			{
				string line3 = stringBuilder.ToString();
				AddToLineResult(stringSplitResult, list2, line3, num, TextLineType.NewLineBreak | TextLineType.LastParagraphLine, font);
				stringSplitResult.Remainder = (flag3 ? stringSplitResult.Remainder : stringParser.ReadToEnd());
			}
			stringParser.Close();
		}
		stringSplitResult.Lines = list2.ToArray();
		list2.Clear();
		return stringSplitResult;
	}

	private void GetDictionary(WCharacterFormat charFormat, string languageCode)
	{
		if (Hyphenator.LoadedHyphenators.ContainsKey(languageCode))
		{
			hyphen = Hyphenator.LoadedHyphenators[languageCode];
			return;
		}
		if (Hyphenator.Dictionaries.ContainsKey(languageCode))
		{
			Stream stream = Hyphenator.Dictionaries[languageCode];
			hyphen = ((stream != null) ? new Hyphenator(stream) : null);
			Hyphenator.LoadedHyphenators.Add(languageCode, hyphen);
			return;
		}
		languageCode = charFormat.Document.Hyphenator.GetAlternateForMissedLanguageCode(languageCode);
		if (Hyphenator.LoadedHyphenators.ContainsKey(languageCode))
		{
			hyphen = Hyphenator.LoadedHyphenators[languageCode];
		}
		else if (Hyphenator.Dictionaries.ContainsKey(languageCode))
		{
			Stream stream2 = Hyphenator.Dictionaries[languageCode];
			hyphen = ((stream2 != null) ? new Hyphenator(stream2) : null);
			Hyphenator.LoadedHyphenators.Add(languageCode, hyphen);
		}
	}

	private void SplitAsPerAutoHyphenation(float maxWidth, List<string> hyphenatedWords, ref string fittedWord, ref string remainingWord, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat)
	{
		int num = 0;
		float lineWidth = GetLineWidth(hyphenatedWords[num] + "-", font, defaultFont, charFormat);
		while (lineWidth <= maxWidth && num + 1 < hyphenatedWords.Count)
		{
			fittedWord += hyphenatedWords[num];
			num++;
			lineWidth = GetLineWidth(fittedWord + hyphenatedWords[num] + "-", font, defaultFont, charFormat);
		}
		for (int i = num; i < hyphenatedWords.Count; i++)
		{
			remainingWord += hyphenatedWords[i];
		}
	}

	private void AddToLineResult(StringSplitResult lineResult, List<TextLineInfo> lines, string line, float lineWidth, TextLineType breakType, DocGen.Drawing.Font font)
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
		TextLineInfo item = default(TextLineInfo);
		item.Line = line;
		item.Width = lineWidth;
		item.LineType = breakType;
		item.Length = line.Length;
		lines.Add(item);
		SizeF actualSize = lineResult.ActualSize;
		actualSize.Height += GetLineHeight(font, FontScriptType.English);
		actualSize.Width = Math.Max(actualSize.Width, lineWidth);
		lineResult.ActualSize = actualSize;
	}

	private TextLineInfo TrimLine(TextLineInfo info, bool firstLine, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat, StringFormat format, SizeF size)
	{
		string text = info.Line;
		float num = info.Width;
		bool num2 = (info.LineType & TextLineType.FirstParagraphLine) == 0;
		bool flag = format == null || format.FormatFlags != StringFormatFlags.DirectionRightToLeft;
		char[] spaces = StringParser.Spaces;
		if (num2)
		{
			text = (flag ? text.TrimStart(spaces) : text.TrimEnd(spaces));
		}
		if (format == null || format.FormatFlags != StringFormatFlags.MeasureTrailingSpaces)
		{
			text = (((info.LineType & TextLineType.FirstParagraphLine) <= TextLineType.None || !StringParser.IsWhitespace(text)) ? (flag ? text.TrimEnd(spaces) : text.TrimStart(spaces)) : new string(' ', 1));
		}
		if (text.Length != info.Line.Length)
		{
			num = GetLineWidth(text, font, defaultFont, charFormat);
			if ((info.LineType & TextLineType.FirstParagraphLine) > TextLineType.None)
			{
				num += GetLineIndent(firstLine, format, size);
			}
		}
		info.Line = text;
		info.Width = num;
		return info;
	}

	private float GetLineWidth(string line, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, WCharacterFormat charFormat)
	{
		FontScriptType scriptType = ((charFormat != null && charFormat.OwnerBase is WTextRange) ? (charFormat.OwnerBase as WTextRange).ScriptType : FontScriptType.English);
		if (defaultFont.Name != font.Name && m_dc.IsUnicodeText(line))
		{
			return m_dc.MeasureString(line, font, defaultFont, null, charFormat, scriptType).Width;
		}
		return m_dc.MeasureString(line, font, null, charFormat, isMeasureFromTabList: false, scriptType).Width;
	}

	private float GetLineIndent(bool firstLine, StringFormat format, SizeF size)
	{
		float num = 0f;
		if (format != null)
		{
			num = ((size.Width > 0f) ? Math.Min(size.Width, num) : num);
		}
		return num;
	}

	private StringTrimming GetWrapType(StringFormat format)
	{
		return format?.Trimming ?? StringTrimming.Word;
	}
}

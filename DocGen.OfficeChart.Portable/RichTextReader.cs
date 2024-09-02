using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;

internal class RichTextReader
{
	private string m_defaultCodePage;

	private const string FontCharSet = "charset";

	internal const string Seperator = ";";

	internal const string ControlStart = "\\";

	internal const string ParaStart = "pard";

	internal const string FontIndex = "f";

	internal const string FontSize = "fs";

	internal const string Bold = "b";

	internal const string Italic = "i";

	internal const string Tab = "tab";

	internal const string ForegroundColor = "cf";

	internal const string Para = "par\r\n";

	internal const string Para2 = "par\n";

	internal const string Para3 = "par";

	internal const string UnderLine = "ul";

	internal const string ItalicsUnderline = "iul";

	internal const string StopUnderLine = "ulnone";

	internal const string Strike = "strike";

	internal const string Subscript = "sub";

	internal const string NoSuperSub = "nosupersub";

	internal const string Superscript = "super";

	internal const string DestinationMark = "*";

	internal const string Red = "red";

	internal const string Green = "green";

	internal const string Blue = "blue";

	internal const string ParagraphCenter = "qc";

	internal const string ParagraphJustify = "qj";

	internal const string ParagraphLeft = "ql";

	internal const string ParagraphRight = "qr";

	internal const string Language = "lang";

	internal const string openBraces = "{";

	internal const string closeBraces = "}";

	internal const string singleQuote = "'";

	private const string unicode = "u";

	private IApplication m_application;

	private Dictionary<int, Color> m_colorDict = new Dictionary<int, Color>();

	private string[] m_complete;

	private Dictionary<int, IOfficeFont> m_fontDict = new Dictionary<int, IOfficeFont>();

	private int m_index;

	private WorkbookImpl m_book;

	private int m_iFontIndex;

	private string m_rtfText;

	private WorksheetImpl m_sheet;

	private IRange m_range;

	private RichTextString m_rtf;

	private int currentStrIndex;

	internal string DefaultCodePage
	{
		get
		{
			_ = m_defaultCodePage;
			return m_defaultCodePage;
		}
		set
		{
			m_defaultCodePage = value;
		}
	}

	protected virtual FontImpl DefaultFont => (FontImpl)m_book.InnerFonts[m_iFontIndex];

	public RichTextReader(IWorksheet parentSheet)
	{
		m_sheet = parentSheet as WorksheetImpl;
		m_book = parentSheet.Workbook as WorkbookImpl;
		m_application = (parentSheet as WorksheetImpl).Application;
	}

	private Color FindColor(string findColor)
	{
		string[] array = findColor.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		if (array.Length == 3)
		{
			b = byte.Parse(array[0].TrimStart("red".ToCharArray()));
			b2 = byte.Parse(array[1].TrimStart("green".ToCharArray()));
			b3 = byte.Parse(array[2].TrimStart("blue".ToCharArray()));
			return Color.FromArgb(255, b, b2, b3);
		}
		return Color.Empty;
	}

	private void Parse()
	{
		m_complete = m_rtfText.Split(new char[2] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
		m_index = 0;
		while (m_index < m_complete.Length)
		{
			m_complete[m_index].Split(new char[1] { ' ' }, StringSplitOptions.None);
			m_index++;
			ParseFontTable();
			ParseColorTable();
			ParseContent();
		}
		m_rtf.TextObject.RtfText = m_rtfText;
	}

	private void ParseColorTable()
	{
		while (!m_complete[m_index].Contains("\\par") && !("{" + m_complete[m_index]).StartsWith(RtfTextWriter.DEF_TAGS[2]))
		{
			m_index++;
		}
		m_colorDict.Add(0, Color.Black);
		int num = m_colorDict.Count;
		string text = "{" + m_complete[m_index];
		if (!text.StartsWith(RtfTextWriter.DEF_TAGS[2]))
		{
			return;
		}
		string[] array = text.Split(";".ToCharArray());
		for (int i = 1; i < array.Length; i++)
		{
			Color color = FindColor(array[i]);
			if (color != Color.Empty)
			{
				m_colorDict.Add(num, color);
				num++;
			}
		}
		m_index++;
	}

	private void ParseContent()
	{
		if (m_complete[m_index].Contains("\\*"))
		{
			m_index++;
		}
		ParseControlWord();
	}

	private void ParseControlWord()
	{
		int num = 0;
		Color rGBColor = Color.Empty;
		IOfficeFont officeFont = m_fontDict[0];
		IOfficeFont officeFont2 = officeFont;
		IOfficeFont officeFont3 = officeFont2;
		while (m_index < m_complete.Length)
		{
			bool flag = false;
			string[] array = m_complete[m_index].Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (num < array.Length)
			{
				string[] array2 = array[num].Split(new char[1] { ' ' }, StringSplitOptions.None);
				int num2 = ParseNumber(array2[0]);
				string text = ((num2 != int.MinValue) ? array2[0].Split(num2.ToString().ToCharArray(), StringSplitOptions.None)[0] : array2[0]);
				int num3 = ((m_rtf.Text != null) ? m_rtf.Text.Length : 0);
				if ((array2[0].Contains(Environment.NewLine) || array2[0].Contains("\n")) && (!array2[0].EndsWith(Environment.NewLine) || !array2[0].EndsWith("\n")))
				{
					string[] array3 = array[num].Split(new string[2]
					{
						Environment.NewLine,
						"\n"
					}, StringSplitOptions.None);
					if (array3.Length > 1)
					{
						if (officeFont2 != officeFont)
						{
							officeFont = m_book.CreateFont(officeFont2);
						}
						AppendRTF("\n" + array3[1]);
						flag = true;
					}
					else if (array3.Length != 0 && !array3[0].Contains("par"))
					{
						AppendRTF(array3[0]);
					}
				}
				else if (array2.Length > 1 && !text.StartsWith("'") && !array2[0].StartsWith("tab") && !array2[1].StartsWith(" ") && !array2[0].StartsWith("par\r\n") && !array2[0].StartsWith("par\n") && !array2[0].StartsWith("par") && !array2[0].StartsWith("qc") && !array2[0].StartsWith("qj") && !array2[0].StartsWith("ql") && !array2[0].StartsWith("qr") && !array2[0].StartsWith("b"))
				{
					AppendRTF(array2[1]);
					flag = true;
				}
				int num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
				int num5 = num;
				switch (text)
				{
				case "pard":
					officeFont = m_book.CreateFont(officeFont2);
					(officeFont as FontImpl).ParaAlign = Excel2007CommentHAlign.l;
					if (array2.Length > 1)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont2 = officeFont;
					break;
				case "qc":
					officeFont = m_book.CreateFont(officeFont2);
					(officeFont as FontImpl).ParaAlign = Excel2007CommentHAlign.ctr;
					(officeFont as FontImpl).HasParagrapAlign = true;
					if (array2.Length > 1)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont2 = officeFont;
					break;
				case "ql":
				case "qj":
					officeFont = m_book.CreateFont(officeFont2);
					(officeFont as FontImpl).ParaAlign = Excel2007CommentHAlign.l;
					(officeFont as FontImpl).HasParagrapAlign = true;
					if (array2.Length > 1)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont2 = officeFont;
					break;
				case "qr":
					officeFont = m_book.CreateFont(officeFont2);
					(officeFont as FontImpl).ParaAlign = Excel2007CommentHAlign.r;
					(officeFont as FontImpl).HasParagrapAlign = true;
					if (array2.Length > 1)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont2 = officeFont;
					break;
				case "f":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.FontName = m_fontDict[num2].FontName;
					officeFont.RGBColor = rGBColor;
					if (array2.Length > 1 && !flag)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont2 = officeFont;
					if (array2.Length > 1 && !flag)
					{
						array2[1] = array2[1].Replace("'7B", "{");
						array2[1] = array2[1].Replace("'7D", "}");
						AppendRTF(array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					break;
				case "fs":
					officeFont = m_book.CreateFont(officeFont2);
					if (array2.Length > 1 && !flag)
					{
						m_rtf.SetText(m_rtf.Text + array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont.Size = (double)num2 / 2.0;
					officeFont2 = officeFont;
					break;
				case "tab":
					AppendRTF("\t");
					if (array2.Length > 1)
					{
						AppendRTF(array2[1].Replace(Environment.NewLine, ""));
					}
					break;
				case "cf":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.RGBColor = m_colorDict[num2];
					rGBColor = officeFont.RGBColor;
					officeFont2 = officeFont;
					break;
				case "par\n":
				case "par\r\n":
					if (!flag)
					{
						officeFont = m_book.CreateFont(officeFont2);
						string[] array4 = array2[0].Split(text.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						string empty = string.Empty;
						empty = ((num == array.Length - 1) ? m_rtf.Text : (m_rtf.Text + "\n"));
						m_rtf.SetText(empty + ((array4.Length != 0) ? array4[0] : ""));
						if (array2.Length > 1)
						{
							AppendRTF(" " + array2[1]);
						}
						if (m_range != null && !m_range.WrapText)
						{
							m_range.WrapText = true;
						}
					}
					break;
				case "ul":
					officeFont = m_book.CreateFont(officeFont2);
					if (array2.Length > 1 && array2[1].StartsWith(" "))
					{
						AppendRTF(array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					officeFont.Underline = OfficeUnderline.Single;
					officeFont2 = officeFont;
					break;
				case "ulnone":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.Underline = OfficeUnderline.None;
					officeFont2 = officeFont;
					break;
				case "strike":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.Strikethrough = !officeFont.Strikethrough;
					officeFont2 = officeFont;
					break;
				case "sub":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.Subscript = !officeFont.Subscript;
					officeFont2 = officeFont;
					break;
				case "nosupersub":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.Subscript = !officeFont.Subscript && officeFont.Subscript;
					officeFont.Superscript = !officeFont.Superscript && officeFont.Superscript;
					officeFont2 = officeFont;
					break;
				case "super":
					officeFont = m_book.CreateFont(officeFont2);
					officeFont.Superscript = !officeFont.Superscript;
					officeFont2 = officeFont;
					break;
				case "*":
					num++;
					break;
				case "b":
					officeFont = m_book.CreateFont(officeFont2);
					if (array2.Length > 1)
					{
						AppendRTF(array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					if (num2 != 0)
					{
						officeFont.Bold = true;
					}
					else
					{
						officeFont.Bold = false;
					}
					officeFont2 = officeFont;
					break;
				case "i":
				case "iul":
					officeFont = m_book.CreateFont(officeFont2);
					if (array2.Length > 1 && array2[1].StartsWith(" "))
					{
						AppendRTF(array2[1]);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					if (num2 != 0)
					{
						officeFont.Italic = true;
					}
					else
					{
						officeFont.Italic = false;
					}
					if (text == "iul")
					{
						officeFont.Underline = OfficeUnderline.Single;
					}
					officeFont2 = officeFont;
					break;
				case "lang":
					_ = array2[0];
					break;
				case "'":
					if (array2[0].Length == 3)
					{
						num5 = SetExtendedCharacter(array2, officeFont3 as FontWrapper, array, num);
						break;
					}
					array[num] = array[num].Replace("'7B", "{");
					array[num] = array[num].Replace("'7D", "}");
					num5 = SetExtendedCharacter(array2, officeFont3 as FontWrapper, array, num);
					num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					break;
				case "u":
				{
					string text2 = array[num].Replace("u", "");
					text2 = text2.Replace("?", "");
					if (text2.Contains(" "))
					{
						AppendRTF((char)Convert.ToInt32(text2) + " ");
					}
					else
					{
						AppendRTF(((char)Convert.ToInt32(text2)).ToString());
					}
					break;
				}
				default:
					if (officeFont2 != officeFont)
					{
						officeFont = m_book.CreateFont(officeFont2);
					}
					if (text.StartsWith("'"))
					{
						if (array2[0].Length == 3)
						{
							num5 = SetExtendedCharacter(array2, officeFont3 as FontWrapper, array, num);
							break;
						}
						array2[0] = array2[0].Replace("'7B", "{");
						array2[0] = array2[0].Replace("'7D", "}");
						num5 = SetExtendedCharacter(array2, officeFont3 as FontWrapper, array, num);
						num4 = ((m_rtf.Text != null) ? (m_rtf.Text.Length - 1) : 0);
					}
					break;
				}
				if (array2.Length > 1 && num3 <= num4)
				{
					m_rtf.SetFont(num3, num4, officeFont);
				}
				else if (array2.Length == 1 && num3 > num4 && num4 >= 0)
				{
					m_rtf.SetFont(num3, num3, officeFont);
				}
				num = ((num5 == num || num5 == 0) ? (num + 1) : (num5 + 1));
			}
			else
			{
				if (array.Length == 0)
				{
					string text3 = m_complete[m_index];
					text3 = "{" + text3.Substring(2, text3.Length - 2) + "}";
					AppendRTF(text3);
				}
				m_index++;
				num = 0;
			}
		}
	}

	private void AppendRTF(string rtfString)
	{
		rtfString = rtfString.Replace("'..", "_");
		m_rtf.SetText(m_rtf.Text + rtfString);
	}

	private int SetExtendedCharacter(string[] rtfString, FontWrapper font, string[] rtfArray, int currentIndex)
	{
		string empty = string.Empty;
		string text = string.Empty;
		int num = 0;
		currentStrIndex = currentIndex;
		while (num < rtfString.Length)
		{
			if (rtfString[num] == string.Empty)
			{
				empty = " " + rtfString[num++];
			}
			else
			{
				if (rtfString[num].Length > 3)
				{
					text = rtfString[num].Substring(3);
					rtfString[num] = rtfString[num].Substring(0, 3);
				}
				empty = GetExtendedCharacter(rtfString[num++], font, rtfArray);
				if (text != string.Empty)
				{
					empty += text;
				}
				text = string.Empty;
			}
			m_rtf.SetText(m_rtf.Text + empty);
		}
		return currentStrIndex;
	}

	private void ParseFontTable()
	{
		m_index++;
		int num = ((m_fontDict.Count > 0) ? (m_fontDict.Count + 1) : 0);
		while (m_complete[m_index].StartsWith("\\f") || m_complete[m_index].StartsWith("\\*"))
		{
			IOfficeFont officeFont = m_book.CreateFont();
			string[] array = m_complete[m_index].Split(new string[1] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[^1].Split(new char[1] { ' ' });
			if (array[0].Contains("charset"))
			{
				if (array[0].StartsWith("af"))
				{
					array[0] = array[0].Replace("afcharset", " ");
					(officeFont as FontWrapper).CharSet = Convert.ToInt16(array[0]);
				}
				else if (array[0].StartsWith("f"))
				{
					array[0] = array[0].Replace("fcharset", " ");
					(officeFont as FontWrapper).CharSet = Convert.ToInt16(array[0]);
				}
			}
			string text = m_complete[m_index];
			int num2 = text.IndexOf(' ') + 1;
			int num3 = text.LastIndexOf(";"[0]) - num2;
			if (num3 != -1)
			{
				officeFont.FontName = text.Substring(num2, num3);
			}
			m_fontDict.Add(num, officeFont);
			num++;
			m_index++;
		}
		if (num == 0)
		{
			m_fontDict.Add(m_iFontIndex, DefaultFont);
		}
	}

	private int ParseNumber(string numText)
	{
		string[] array = numText.Split("\\".ToCharArray());
		string text = "";
		if (array.Length != 0)
		{
			char[] array2 = array[0].ToCharArray();
			foreach (char c in array2)
			{
				int result = 0;
				if (int.TryParse(c.ToString(), out result))
				{
					text += result;
				}
			}
		}
		if (text != null && text != string.Empty)
		{
			return int.Parse(text);
		}
		return int.MinValue;
	}

	public void SetRTF(int row, int column, string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("RTF Text");
		}
		m_rtfText = text;
		m_range = m_sheet[row, column];
		m_rtf = m_range.RichText as RichTextString;
		Parse();
	}

	public void SetRTF(object shape, string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("RTF Text");
		}
		m_rtfText = text;
		m_rtf = CreateRichTextString() as RichTextString;
		Parse();
		if (shape.GetType().Name == "TextBoxShapeImpl")
		{
			(shape as ITextBoxShape).RichText = m_rtf;
		}
	}

	protected IRichTextString CreateRichTextString()
	{
		return new RangeRichTextString(m_sheet.Application, m_sheet, -1, -1);
	}

	private string GetExtendedCharacter(string token, FontWrapper font, string[] rtfArray)
	{
		int num = 0;
		string text = "";
		int length = token.Length;
		_ = string.Empty;
		_ = string.Empty;
		string text2 = null;
		Encoding.GetEncoding(GetCodePage(font.CharSet));
		if (token.Length >= 3)
		{
			text2 = token.Substring(1, 2);
		}
		if (!token.StartsWith("'"))
		{
			text = " " + token;
		}
		else if (token == "'.." || token == "'.")
		{
			text = "_";
		}
		else if (text2 != "3f")
		{
			if (!IsSingleByte(font.CharSet))
			{
				if (currentStrIndex != rtfArray.Length - 1)
				{
					currentStrIndex++;
				}
				string text3 = rtfArray[currentStrIndex];
				string[] array = SeperateToken(text3);
				_ = array[0];
				_ = array[1];
				text3 = text3.Trim();
				text3 = text3.Substring(1);
				text2 = text3 + text2;
				num = int.Parse(text2, NumberStyles.HexNumber);
			}
			if (text2 != null)
			{
				num = int.Parse(text2, NumberStyles.HexNumber);
			}
			Windows1252Encoding windows1252Encoding = new Windows1252Encoding();
			byte[] bytes = BitConverter.GetBytes((short)num);
			text = windows1252Encoding.GetString(bytes, 0, bytes.Length);
			text = text.Replace("\0", "");
		}
		if (length > 3)
		{
			text += token.Substring(3, length - 3);
		}
		return text;
	}

	private string[] SeperateToken(string token)
	{
		string[] array = new string[2];
		for (int i = 0; i < token.Length; i++)
		{
			char c = token[i];
			if (char.IsDigit(c))
			{
				array[1] += c;
			}
			else
			{
				array[0] += c;
			}
		}
		return array;
	}

	private bool IsSingleByte(int charset)
	{
		switch (charset)
		{
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 128:
		case 129:
		case 130:
		case 134:
		case 136:
			return false;
		default:
			return true;
		}
	}

	private string GetCodePage(int fontCharSet)
	{
		switch (fontCharSet)
		{
		case 0:
		case 1:
			return "Windows-1252";
		case 77:
			return "macintosh";
		case 78:
			return "x-mac-japanese";
		case 79:
			return "x-mac-korean";
		case 80:
			return "x-mac-chinesesimp";
		case 81:
		case 82:
			return "x-mac-chinesetrad";
		case 83:
			return "x-mac-hebrew";
		case 84:
			return "x-mac-arabic";
		case 85:
			return "x-mac-greek";
		case 86:
			return "x-mac-turkish";
		case 87:
			return "x-mac-thai";
		case 88:
			return "x-mac-ce";
		case 89:
			return "x-mac-cyrillic";
		case 128:
			return "shift_jis";
		case 129:
			return "ks_c_5601-1987";
		case 130:
			return "Johab";
		case 134:
			return "gb2312";
		case 136:
			return "big5";
		case 161:
			return "windows-1253";
		case 162:
			return "windows-1254";
		case 163:
			return "windows-1258";
		case 177:
			return "windows-1255";
		case 178:
		case 179:
		case 180:
		case 181:
			return "windows-1256";
		case 186:
			return "windows-1257";
		case 204:
			return "windows-1251";
		case 222:
			return "windows-874";
		case 238:
			return "windows-1250";
		case 254:
			return "IBM437";
		case 255:
			return "ibm850";
		default:
			return DefaultCodePage;
		}
	}

	private bool IsSupportedCodePage(int codePage)
	{
		if (codePage <= 20269)
		{
			switch (codePage)
			{
			case 37:
			case 437:
			case 500:
			case 708:
			case 720:
			case 737:
			case 775:
			case 850:
			case 852:
			case 855:
			case 857:
			case 858:
			case 860:
			case 861:
			case 862:
			case 863:
			case 864:
			case 865:
			case 866:
			case 869:
			case 870:
			case 874:
			case 875:
			case 932:
			case 936:
			case 949:
			case 950:
			case 1026:
			case 1029:
			case 1047:
			case 1140:
			case 1141:
			case 1142:
			case 1143:
			case 1144:
			case 1145:
			case 1146:
			case 1147:
			case 1148:
			case 1149:
			case 1200:
			case 1201:
			case 1250:
			case 1251:
			case 1252:
			case 1253:
			case 1254:
			case 1255:
			case 1256:
			case 1257:
			case 1258:
			case 1361:
			case 10000:
			case 10001:
			case 10002:
			case 10003:
			case 10004:
			case 10005:
			case 10006:
			case 10007:
			case 10008:
			case 10010:
			case 10017:
			case 10021:
			case 10029:
			case 10079:
			case 10081:
			case 10082:
			case 12000:
			case 12001:
			case 20000:
			case 20001:
			case 20002:
			case 20003:
			case 20004:
			case 20005:
			case 20105:
			case 20106:
			case 20107:
			case 20108:
			case 20127:
			case 20261:
			case 20269:
				break;
			default:
				goto IL_04fe;
			}
		}
		else
		{
			switch (codePage)
			{
			case 20273:
			case 20277:
			case 20278:
			case 20280:
			case 20284:
			case 20285:
			case 20290:
			case 20297:
			case 20420:
			case 20423:
			case 20424:
			case 20833:
			case 20838:
			case 20866:
			case 20871:
			case 20880:
			case 20905:
			case 20924:
			case 20932:
			case 20936:
			case 20949:
			case 21025:
			case 21866:
			case 28591:
			case 28592:
			case 28593:
			case 28594:
			case 28595:
			case 28596:
			case 28597:
			case 28598:
			case 28599:
			case 28603:
			case 28605:
			case 29001:
			case 38598:
			case 50220:
			case 50221:
			case 50222:
			case 50225:
			case 50227:
			case 51932:
			case 51936:
			case 51949:
			case 52936:
			case 54936:
			case 57002:
			case 57003:
			case 57004:
			case 57005:
			case 57006:
			case 57007:
			case 57008:
			case 57009:
			case 57010:
			case 57011:
			case 65000:
			case 65001:
				break;
			default:
				goto IL_04fe;
			}
		}
		return true;
		IL_04fe:
		return false;
	}

	private string GetSupportedCodePage(int codePage)
	{
		return codePage switch
		{
			37 => "IBM037", 
			437 => "IBM437", 
			500 => "IBM500", 
			708 => "ASMO-708", 
			720 => "DOS-720", 
			737 => "ibm737", 
			775 => "ibm775", 
			850 => "ibm850", 
			852 => "ibm852", 
			855 => "IBM855", 
			857 => "ibm857", 
			858 => "IBM00858", 
			860 => "IBM860", 
			861 => "ibm861", 
			862 => "DOS-862", 
			863 => "IBM863", 
			864 => "IBM864", 
			865 => "IBM865", 
			866 => "cp866", 
			869 => "ibm869", 
			870 => "IBM870", 
			874 => "windows-874", 
			875 => "cp875", 
			932 => "shift_jis", 
			936 => "gb2312", 
			949 => "ks_c_5601-1987", 
			950 => "big5", 
			1026 => "IBM1026", 
			1047 => "IBM01047", 
			1140 => "IBM01140", 
			1141 => "IBM01141", 
			1142 => "IBM01142", 
			1143 => "IBM01143", 
			1144 => "IBM01144", 
			1145 => "IBM01145", 
			1146 => "IBM01146", 
			1147 => "IBM01147", 
			1148 => "IBM01148", 
			1149 => "IBM01149", 
			1200 => "utf-16", 
			1201 => "unicodeFFFE", 
			1250 => "windows-1250", 
			1251 => "windows-1251", 
			1252 => "windows-1252", 
			1253 => "windows-1253", 
			1254 => "windows-1254", 
			1255 => "windows-1255", 
			1256 => "windows-1256", 
			1257 => "windows-1257", 
			1258 => "windows-1258", 
			1361 => "Johab", 
			10000 => "macintosh", 
			10001 => "x-mac-japanese", 
			10002 => "x-mac-chinesetrad", 
			10003 => "x-mac-korean", 
			10004 => "x-mac-arabic", 
			10005 => "x-mac-hebrew", 
			10006 => "x-mac-greek", 
			10007 => "x-mac-cyrillic", 
			10008 => "x-mac-chinesesimp", 
			10010 => "x-mac-romanian", 
			10017 => "x-mac-ukrainian", 
			10021 => "x-mac-thai", 
			10029 => "x-mac-ce", 
			10079 => "x-mac-icelandic", 
			10081 => "x-mac-turkish", 
			10082 => "x-mac-croatian", 
			12000 => "utf-32", 
			12001 => "utf-32BE", 
			20000 => "x-Chinese_CNS", 
			20001 => "x-cp20001", 
			20002 => "x_Chinese-Eten", 
			20003 => "x-cp20003", 
			20004 => "x-cp20004", 
			20005 => "x-cp20005", 
			20105 => "x-IA5", 
			20106 => "x-IA5-German", 
			20107 => "x-IA5-Swedish", 
			20108 => "x-IA5-Norwegian", 
			20127 => "us-ascii", 
			20261 => "x-cp20261", 
			20269 => "x-cp20269", 
			20273 => "IBM273", 
			20277 => "IBM277", 
			20278 => "IBM278", 
			20280 => "IBM280", 
			20284 => "IBM284", 
			20285 => "IBM285", 
			20290 => "IBM290", 
			20297 => "IBM297", 
			20420 => "IBM420", 
			20423 => "IBM423", 
			20424 => "IBM424", 
			20833 => "x-EBCDIC-KoreanExtended", 
			20838 => "IBM-Thai", 
			20866 => "koi8-r", 
			20871 => "IBM871", 
			20880 => "IBM880", 
			20905 => "IBM905", 
			20924 => "IBM00924", 
			20932 => "EUC-JP", 
			20936 => "x-cp20936", 
			20949 => "x-cp20949", 
			21025 => "cp1025", 
			21866 => "koi8-u", 
			28591 => "iso-8859-1", 
			28592 => "iso-8859-2", 
			28593 => "iso-8859-3", 
			28594 => "iso-8859-4", 
			28595 => "iso-8859-5", 
			28596 => "iso-8859-6", 
			28597 => "iso-8859-7", 
			28598 => "iso-8859-8", 
			28599 => "iso-8859-9", 
			28603 => "iso-8859-13", 
			28605 => "iso-8859-15", 
			29001 => "x-Europa", 
			38598 => "iso-8859-8-i", 
			50220 => "iso-2022-jp", 
			50221 => "csISO2022JP", 
			50222 => "iso-2022-jp", 
			50225 => "iso-2022-kr", 
			50227 => "x-cp50227", 
			51932 => "euc-jp", 
			51936 => "EUC-CN", 
			51949 => "euc-kr", 
			52936 => "hz-gb-2312", 
			54936 => "GB18030", 
			57002 => "x-iscii-de", 
			57003 => "x-iscii-be", 
			57004 => "x-iscii-ta", 
			57005 => "x-iscii-te", 
			57006 => "x-iscii-as", 
			57007 => "x-iscii-or", 
			57008 => "x-iscii-ka", 
			57009 => "x-iscii-ma", 
			57010 => "x-iscii-gu", 
			57011 => "x-iscii-pa", 
			65000 => "utf-7", 
			65001 => "utf-8", 
			_ => "Windows-1252", 
		};
	}
}

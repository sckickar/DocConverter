using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class RtfTextWriter : TextWriter
{
	private const string DEF_FONT = "{{\\f{0}\\fnil\\fcharset{1} {2};}}";

	private const string DEF_FONT_ATTRIBUTE = "\\f{0}\\fs{1}";

	private const string DEF_COLOR_FORMAT = "\\red{0}\\green{1}\\blue{2};";

	private static readonly string[] UnderlineTags = new string[19]
	{
		"\\ul", "\\ul0", "\\uld", "\\uldash", "\\uldashd", "\\uldashdd", "\\uldb", "\\ulhwave", "\\ulldash", "\\ulnone",
		"\\ulth", "\\ulthd", "\\ulthdash", "\\ulthdashd", "\\ulthdashdd", "\\ulthldash", "\\ululdbwave", "\\ulw", "\\ulwave"
	};

	private static readonly string[] StrikeThroughTags = new string[4] { "\\strike1", "\\strike0", "\\striked1", "\\striked0" };

	internal static readonly string[] DEF_TAGS = new string[18]
	{
		"{\\fonttbl", "}", "{\\colortbl", "}", "\\b", "\\b0", "\\i", "\\i0", "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033", "}",
		"{", "}", "\\par", "\\cf{0}", "\\cb{0}", "\\sub", "\\super", "\\nosupersub"
	};

	private List<Color> m_arrColors = new List<Color>();

	private Dictionary<Font, int> m_hashFonts = new Dictionary<Font, int>();

	private Dictionary<Color, int> m_hashColorTable = new Dictionary<Color, int>();

	private bool m_bEnableFormatting;

	private TextWriter m_innerWriter;

	private bool m_bTabsPending;

	private bool m_bEscape;

	private static readonly char[] newLine = "\\line\r\n".ToCharArray();

	public bool Escape
	{
		get
		{
			return m_bEscape;
		}
		set
		{
			m_bEscape = value;
		}
	}

	public override Encoding Encoding => m_innerWriter.Encoding;

	public RtfTextWriter()
		: this(new StringWriter(), enableFormatting: true)
	{
	}

	public RtfTextWriter(bool enableFormatting)
		: this(new StringWriter(), enableFormatting)
	{
	}

	public RtfTextWriter(TextWriter underlyingWriter)
		: this(underlyingWriter, enableFormatting: true)
	{
	}

	public RtfTextWriter(TextWriter underlyingWriter, bool enableFormatting)
	{
		m_innerWriter = underlyingWriter;
		m_bEnableFormatting = enableFormatting;
	}

	protected virtual void OutputTabs()
	{
		if (m_bTabsPending)
		{
			m_bTabsPending = false;
		}
	}

	protected string GetImageRTF(string rtf)
	{
		int num = rtf.IndexOf("{\\pict");
		int num2 = rtf.IndexOf("}", num);
		return rtf.Substring(num, num2 - num + 1);
	}

	private void WriteFontInTable(Font font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (m_bEnableFormatting)
		{
			if (!m_hashFonts.ContainsKey(font))
			{
				throw new ApplicationException("Collection does not contain font");
			}
			_ = m_hashFonts[font];
			Escape = false;
			Escape = true;
		}
	}

	private void WriteFontAttribute(int iFontId, int iFontSize)
	{
		if (m_bEnableFormatting)
		{
			Escape = false;
			m_innerWriter.Write($"\\f{iFontId}\\fs{iFontSize * 2}");
			Escape = true;
		}
	}

	private void WriteColorInTable(Color value)
	{
		if (m_bEnableFormatting)
		{
			Escape = false;
			Write($"\\red{value.R}\\green{value.G}\\blue{value.B};");
			Escape = true;
		}
	}

	private void WriteChar(char value)
	{
		if (m_bEscape)
		{
			switch (value)
			{
			case '{':
				m_innerWriter.Write('\\');
				m_innerWriter.Write('{');
				break;
			case '}':
				m_innerWriter.Write('\\');
				m_innerWriter.Write('}');
				break;
			case '\\':
				m_innerWriter.Write('\\');
				m_innerWriter.Write('\\');
				break;
			default:
			{
				TextWriter innerWriter = m_innerWriter;
				int num = value;
				innerWriter.Write("\\u" + num + "*");
				break;
			}
			}
		}
		else
		{
			m_innerWriter.Write(value);
		}
	}

	private void WriteString(string value)
	{
		if (value == null || value.Length == 0)
		{
			return;
		}
		if (m_bEscape)
		{
			int i = 0;
			for (int length = value.Length; i < length; i++)
			{
				Write(value[i]);
			}
		}
		else
		{
			m_innerWriter.Write(value);
		}
	}

	private void WriteImageString(string value)
	{
		if (value != null && value.Length != 0)
		{
			m_innerWriter.Write(value);
		}
	}

	private void WriteString(string value, string image, string align)
	{
		if (value == null || value.Length == 0)
		{
			return;
		}
		if (m_bEscape)
		{
			int i = 0;
			for (int length = value.Length; i < length; i++)
			{
				if (value[i] == '&' && value[i + 1] == 'G' && image != null)
				{
					WriteImageString(GetImageRTF(image));
					i += 2;
					if (i == length)
					{
						break;
					}
				}
				Write(value[i]);
			}
		}
		else
		{
			m_innerWriter.Write(value);
		}
	}

	private void WriteNewLine()
	{
		if (m_bEscape)
		{
			m_innerWriter.Write(newLine);
		}
		else
		{
			m_innerWriter.WriteLine();
		}
	}

	private void WriteNewLine(string value)
	{
		if (m_bEscape)
		{
			Write(value);
			m_innerWriter.Write(newLine);
		}
		else
		{
			m_innerWriter.WriteLine(value);
		}
	}

	public override string ToString()
	{
		return m_innerWriter.ToString();
	}

	public override void Write(bool value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(char value)
	{
		OutputTabs();
		WriteChar(value);
	}

	public override void Write(char[] buffer)
	{
		OutputTabs();
		m_innerWriter.Write(buffer);
	}

	public override void Write(double value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(int value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(long value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(object value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(float value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(string s)
	{
		OutputTabs();
		WriteString(s);
	}

	internal void Write(string value, string image, string align)
	{
		OutputTabs();
		WriteString(value, image, align);
	}

	[CLSCompliant(false)]
	public override void Write(uint value)
	{
		OutputTabs();
		m_innerWriter.Write(value);
	}

	public override void Write(char[] buffer, int index, int count)
	{
		OutputTabs();
		m_innerWriter.Write(buffer, index, count);
	}

	public override void WriteLine()
	{
		OutputTabs();
		WriteNewLine();
		m_bTabsPending = true;
	}

	public override void WriteLine(bool value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(char value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(char[] buffer)
	{
		OutputTabs();
		m_innerWriter.WriteLine(buffer);
		m_bTabsPending = true;
	}

	public override void WriteLine(double value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(int value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(long value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(object value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(float value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(string s)
	{
		OutputTabs();
		WriteNewLine(s);
		m_bTabsPending = true;
	}

	[CLSCompliant(false)]
	public override void WriteLine(uint value)
	{
		OutputTabs();
		m_innerWriter.WriteLine(value);
		m_bTabsPending = true;
	}

	public override void WriteLine(string format, params object[] arg)
	{
		OutputTabs();
		m_innerWriter.WriteLine(format, arg);
		m_bTabsPending = true;
	}

	public override void WriteLine(char[] buffer, int index, int count)
	{
		OutputTabs();
		m_innerWriter.WriteLine(buffer, index, count);
		m_bTabsPending = true;
	}

	public int AddFont(Font font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (m_hashFonts.ContainsKey(font))
		{
			return m_hashFonts[font];
		}
		int num = m_hashFonts.Count + 1;
		m_hashFonts.Add(font, num);
		return num;
	}

	public int AddColor(Color color)
	{
		if (!m_hashColorTable.ContainsKey(color))
		{
			m_hashColorTable.Add(color, m_hashColorTable.Count + 1);
			m_arrColors.Add(color);
		}
		return m_hashColorTable[color];
	}

	public void WriteFontTable()
	{
		if (m_hashFonts.Count == 0)
		{
			return;
		}
		WriteTag(RtfTags.FontTableBegin);
		foreach (Font key in m_hashFonts.Keys)
		{
			WriteFontInTable(key);
		}
		WriteTag(RtfTags.FontTableEnd);
	}

	public void WriteColorTable()
	{
		if (m_hashColorTable.Count != 0)
		{
			WriteTag(RtfTags.ColorTableStart);
			int i = 0;
			for (int count = m_arrColors.Count; i < count; i++)
			{
				Color value = m_arrColors[i];
				WriteColorInTable(value);
			}
			WriteTag(RtfTags.ColorTableEnd);
		}
	}

	public void WriteText(Font font, string strText)
	{
		WriteText(font, ColorExtension.Empty, ColorExtension.Empty, strText);
	}

	public void WriteText(Font font, Color foreColor, string strText)
	{
		WriteText(font, foreColor, ColorExtension.Empty, strText);
	}

	public void WriteText(Font font, Color foreColor, Color backColor, string strText)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (strText == null || strText.Length == 0)
		{
			return;
		}
		WriteTag(RtfTags.GroupStart);
		WriteFont(font);
		if (foreColor != ColorExtension.Empty)
		{
			WriteForeColorAttribute(foreColor);
		}
		if (backColor != ColorExtension.Empty)
		{
			WriteBackColorAttribute(backColor);
		}
		int num = 0;
		int length = strText.Length;
		bool flag = true;
		while (num < length)
		{
			int num2 = strText.IndexOf(NewLine, num);
			if (num2 == -1)
			{
				num2 = strText.Length;
				flag = false;
			}
			string value = strText.Substring(num, num2 - num);
			Write(value);
			if (flag)
			{
				WriteTag(RtfTags.EndLine);
			}
			num = num2 + NewLine.Length;
		}
		WriteTag(RtfTags.GroupEnd);
	}

	public void WriteText(IOfficeFont font, string strText)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (strText == null || strText.Length == 0)
		{
			return;
		}
		WriteTag(RtfTags.GroupStart);
		WriteFont(font);
		int num = 0;
		int length = strText.Length;
		bool flag = true;
		while (num < length)
		{
			int num2 = strText.IndexOf(NewLine, num);
			if (num2 == -1)
			{
				num2 = strText.Length;
				flag = false;
			}
			string value = strText.Substring(num, num2 - num);
			Write(value);
			if (flag)
			{
				WriteTag(RtfTags.EndLine);
			}
			num = num2 + NewLine.Length;
		}
		WriteTag(RtfTags.GroupEnd);
	}

	internal void WriteImageText(IOfficeFont font, string strText, string image, string align)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (strText == null || strText.Length == 0)
		{
			return;
		}
		WriteTag(RtfTags.GroupStart);
		WriteFont(font);
		int num = 0;
		int length = strText.Length;
		bool flag = true;
		while (num < length)
		{
			int num2 = strText.IndexOf(NewLine, num);
			if (num2 == -1)
			{
				num2 = strText.Length;
				flag = false;
			}
			string value = strText.Substring(num, num2 - num);
			Write(value, image, align);
			if (flag)
			{
				WriteTag(RtfTags.EndLine);
			}
			num = num2 + NewLine.Length;
		}
		WriteTag(RtfTags.GroupEnd);
	}

	public void WriteFontAttribute(Font font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (!m_hashFonts.ContainsKey(font))
		{
			throw new ArgumentException("Unknown font");
		}
		int iFontId = m_hashFonts[font];
		int iFontSize = (int)font.Size;
		WriteFontAttribute(iFontId, iFontSize);
	}

	public void WriteFont(Font font)
	{
		WriteFontAttribute(font);
		WriteFontItalicBoldStriked(font);
		if (font.Underline)
		{
			WriteUnderlineAttribute();
		}
	}

	public void WriteFont(IOfficeFont font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		FontImpl font2;
		if (font is FontImpl)
		{
			font2 = (FontImpl)font;
		}
		else
		{
			if (!(font is FontWrapper))
			{
				throw new InvalidCastException("Wrong type of font");
			}
			font2 = ((FontWrapper)font).Wrapped;
		}
		Font font3 = font.GenerateNativeFont();
		WriteFontAttribute(font3);
		WriteFontItalicBoldStriked(font3);
		WriteUnderline(font2);
		WriteSubSuperScript(font2);
		WriteForeColorAttribute(font.RGBColor);
	}

	public void WriteSubSuperScript(FontImpl font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (font.Subscript)
		{
			WriteTag(RtfTags.SubScript);
		}
		else if (font.Superscript)
		{
			WriteTag(RtfTags.SuperScript);
		}
	}

	public void WriteFontItalicBoldStriked(Font font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (font.Italic)
		{
			WriteTag(RtfTags.ItalicOn);
		}
		if (font.Bold)
		{
			WriteTag(RtfTags.BoldOn);
		}
		if (font.Strikeout)
		{
			WriteStrikeThrough(StrikeThroughStyle.SingleOn);
		}
	}

	public void WriteUnderline(FontImpl font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		switch (font.Underline)
		{
		case OfficeUnderline.Double:
		case OfficeUnderline.DoubleAccounting:
			WriteUnderlineAttribute(UnderlineStyle.Double);
			break;
		case OfficeUnderline.Single:
		case OfficeUnderline.SingleAccounting:
			WriteUnderlineAttribute(UnderlineStyle.Continuous);
			break;
		}
	}

	public void WriteUnderlineAttribute()
	{
		if (m_bEnableFormatting)
		{
			WriteUnderlineAttribute(UnderlineStyle.Continuous);
		}
	}

	public void WriteUnderlineAttribute(UnderlineStyle style)
	{
		if (style < UnderlineStyle.Continuous || (int)style >= UnderlineTags.Length)
		{
			throw new ArgumentOutOfRangeException("style");
		}
		Escape = false;
		Write(UnderlineTags[(int)style]);
		Escape = true;
	}

	public void WriteStrikeThrough(StrikeThroughStyle style)
	{
		if (style < StrikeThroughStyle.SingleOn || (int)style >= StrikeThroughTags.Length)
		{
			throw new ArgumentOutOfRangeException("style");
		}
		Escape = false;
		Write(StrikeThroughTags[(int)style]);
		Escape = true;
	}

	public void WriteBackColorAttribute(Color color)
	{
		if (!m_hashColorTable.ContainsKey(color))
		{
			throw new ArgumentOutOfRangeException("color", "Unknown color");
		}
		int num = m_hashColorTable[color];
		WriteTag(RtfTags.BackColor, num);
	}

	public void WriteForeColorAttribute(Color color)
	{
		if (!m_hashColorTable.ContainsKey(color))
		{
			throw new ArgumentOutOfRangeException("color", "Unknown color");
		}
		int num = m_hashColorTable[color];
		WriteTag(RtfTags.ForeColor, num);
	}

	public void WriteLineNoTabs(string s)
	{
		m_innerWriter.WriteLine(s);
	}

	public void WriteTag(RtfTags tag)
	{
		if (m_bEnableFormatting)
		{
			if (tag < RtfTags.FontTableBegin || (int)tag >= DEF_TAGS.Length)
			{
				throw new ArgumentOutOfRangeException("tag");
			}
			Escape = false;
			m_innerWriter.Write(DEF_TAGS[(int)tag]);
			Escape = true;
		}
	}

	public void WriteTag(RtfTags tag, params object[] arrParams)
	{
		if (m_bEnableFormatting)
		{
			if (tag < RtfTags.FontTableBegin || (int)tag >= DEF_TAGS.Length)
			{
				throw new ArgumentOutOfRangeException("tag");
			}
			Escape = false;
			m_innerWriter.Write(string.Format(DEF_TAGS[(int)tag], arrParams));
			Escape = true;
		}
	}

	internal void WriteAlignment(string alignment)
	{
		string arg = "ql";
		switch (alignment)
		{
		case "Center":
			arg = "qc";
			break;
		case "Left":
			arg = "ql";
			break;
		case "Right":
			arg = "qr";
			break;
		}
		Escape = false;
		m_innerWriter.Write($"\\pard\\{arg}");
		Escape = true;
	}
}

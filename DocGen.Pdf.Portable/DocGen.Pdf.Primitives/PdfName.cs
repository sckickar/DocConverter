using System;
using System.Text;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class PdfName : IPdfPrimitive
{
	internal const string StringStartMark = "/";

	public static string Delimiters = "()<>[]{}/%}";

	private static readonly char[] m_replacements = new char[4] { ' ', '\t', '\n', '\r' };

	private string m_value = string.Empty;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value != m_value)
			{
				string value2 = value;
				if (value != null && value.Length > 0)
				{
					value2 = ((value.Substring(0, 1) == "/") ? value.Substring(1) : value);
					m_value = NormalizeValue(value2);
				}
				else
				{
					m_value = value2;
				}
			}
		}
	}

	public ObjectStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			m_status = value;
		}
	}

	public bool IsSaving
	{
		get
		{
			return m_isSaving;
		}
		set
		{
			m_isSaving = value;
		}
	}

	public int ObjectCollectionIndex
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public int Position
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

	public IPdfPrimitive ClonedObject => null;

	public PdfName()
	{
	}

	public PdfName(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.IndexOfAny(m_replacements) != -1)
		{
			m_value = NormalizeValue(value);
		}
		else
		{
			m_value = value;
		}
	}

	public PdfName(Enum value)
		: this(value.ToString())
	{
	}

	private static string NormalizeValue(string value)
	{
		string text = value;
		char[] replacements = m_replacements;
		foreach (char symbol in replacements)
		{
			text = NormalizeValue(text, symbol);
		}
		return text;
	}

	private static string NormalizeValue(string value, char symbol)
	{
		return value.Replace(symbol.ToString(), $"#{(int)symbol:X}");
	}

	public static string EscapeString(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (str == string.Empty)
		{
			return str;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int length = str.Length; i < length; i++)
		{
			char c = str[i];
			Delimiters.IndexOf(c);
			switch (c)
			{
			case '\r':
				stringBuilder.Append("\\r");
				break;
			case '\n':
				stringBuilder.Append("\n");
				break;
			case '(':
			case ')':
			case '\\':
				stringBuilder.Append(c);
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	internal static string EncodeName(string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = value.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = (char)(array[i] & 0xFFu);
			switch (c)
			{
			case ' ':
			case '#':
			case '%':
			case '(':
			case ')':
			case '/':
			case '<':
			case '>':
			case '[':
			case ']':
			case '{':
			case '}':
				stringBuilder.Append('#');
				stringBuilder.Append(Convert.ToString(c, 16).ToUpper());
				continue;
			}
			if (c > '~' || c < ' ')
			{
				stringBuilder.Append('#');
				if (c < '\u0010')
				{
					stringBuilder.Append('0');
				}
				stringBuilder.Append(Convert.ToString(c, 16).ToUpper());
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	internal static string DecodeName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int length = name.Length;
		for (int i = 0; i < length; i++)
		{
			char c = name[i];
			if (c == '#')
			{
				c = (char)((ConvertToHex(name[i + 1]) << 4) + ConvertToHex(name[i + 2]));
				i += 2;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private static int ConvertToHex(int hex)
	{
		if (hex >= 48 && hex <= 57)
		{
			return hex - 48;
		}
		if (hex >= 65 && hex <= 70)
		{
			return hex - 65 + 10;
		}
		if (hex >= 97 && hex <= 102)
		{
			return hex - 97 + 10;
		}
		return -1;
	}

	public static explicit operator PdfName(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return new PdfName(str);
	}

	public override string ToString()
	{
		return "/" + EscapeString(Value);
	}

	public override bool Equals(object obj)
	{
		PdfName pdfName = obj as PdfName;
		if (pdfName == (object)null)
		{
			return false;
		}
		return pdfName.Value == Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(PdfName name1, object name2)
	{
		if ((object)name1 == name2)
		{
			return true;
		}
		if ((object)name1 == null || name2 == null)
		{
			return false;
		}
		PdfName pdfName = name2 as PdfName;
		if (pdfName == null)
		{
			return false;
		}
		return name1.Value == pdfName.Value;
	}

	public static bool operator !=(PdfName name1, object name2)
	{
		return !(name1 == name2);
	}

	public static bool operator ==(PdfName name1, PdfName name2)
	{
		bool flag = false;
		if ((object)name1 == name2)
		{
			return true;
		}
		if ((object)name1 == null || (object)name2 == null)
		{
			return false;
		}
		return name1.Value == name2.Value;
	}

	public static bool operator !=(PdfName name1, PdfName name2)
	{
		return !(name1 == name2);
	}

	public void Save(IPdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.Write(ToString());
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		return new PdfName
		{
			Value = m_value
		};
	}
}

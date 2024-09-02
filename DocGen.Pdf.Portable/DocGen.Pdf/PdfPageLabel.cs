using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPageLabel : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private int m_startIndex = -1;

	public PdfNumberStyle NumberStyle
	{
		get
		{
			PdfNumberStyle result = PdfNumberStyle.None;
			PdfName pdfName = m_dictionary["S"] as PdfName;
			if (pdfName != null)
			{
				result = FromStringToStyle(pdfName.Value);
			}
			return result;
		}
		set
		{
			string text = FromStyleToString(value);
			if (text == null || text == string.Empty)
			{
				m_dictionary.Remove("S");
			}
			else
			{
				m_dictionary.SetName("S", text);
			}
		}
	}

	public string Prefix
	{
		get
		{
			string result = null;
			if (m_dictionary["P"] is PdfString pdfString)
			{
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				m_dictionary.Remove("P");
			}
			else
			{
				m_dictionary.SetString("P", value);
			}
		}
	}

	public int StartNumber
	{
		get
		{
			int result = -1;
			if (m_dictionary["St"] is PdfNumber pdfNumber)
			{
				result = pdfNumber.IntValue;
			}
			return result;
		}
		set
		{
			if (value < 0)
			{
				m_dictionary.Remove("St");
			}
			else
			{
				m_dictionary.SetNumber("St", value);
			}
		}
	}

	public int StartPageIndex
	{
		get
		{
			return m_startIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(" Start index not less than zero");
			}
			m_startIndex = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPageLabel()
	{
		m_dictionary.SetProperty("Type", new PdfName("PageLabel"));
		m_dictionary.SetProperty("S", new PdfName("D"));
	}

	private static string FromStyleToString(PdfNumberStyle style)
	{
		string result = null;
		switch (style)
		{
		case PdfNumberStyle.Numeric:
			result = "D";
			break;
		case PdfNumberStyle.UpperLatin:
			result = "A";
			break;
		case PdfNumberStyle.LowerLatin:
			result = "a";
			break;
		case PdfNumberStyle.UpperRoman:
			result = "R";
			break;
		case PdfNumberStyle.LowerRoman:
			result = "r";
			break;
		default:
			throw new ArgumentException("Unsupported style.", "style");
		case PdfNumberStyle.None:
			break;
		}
		return result;
	}

	private static PdfNumberStyle FromStringToStyle(string name)
	{
		PdfNumberStyle result = PdfNumberStyle.None;
		if (name != null && name != string.Empty)
		{
			result = name switch
			{
				"D" => PdfNumberStyle.Numeric, 
				"A" => PdfNumberStyle.UpperLatin, 
				"a" => PdfNumberStyle.LowerLatin, 
				"R" => PdfNumberStyle.UpperRoman, 
				"r" => PdfNumberStyle.LowerRoman, 
				_ => throw new ArgumentException("Unsupported style name.", "name"), 
			};
		}
		return result;
	}
}

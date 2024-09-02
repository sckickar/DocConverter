using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

public class SyntaxAnalyzerResult : AnalyzerResult
{
	private List<PdfException> m_errors;

	private bool m_isCorrupted;

	public List<PdfException> Errors
	{
		get
		{
			return m_errors;
		}
		internal set
		{
			m_errors = value;
		}
	}

	public bool IsCorrupted
	{
		get
		{
			return m_isCorrupted;
		}
		internal set
		{
			m_isCorrupted = value;
		}
	}

	internal SyntaxAnalyzerResult()
	{
	}
}

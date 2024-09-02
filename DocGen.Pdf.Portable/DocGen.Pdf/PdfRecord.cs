using System.Diagnostics;

namespace DocGen.Pdf;

[DebuggerDisplay("({OperatorName}, operands={Operands.Length})")]
internal class PdfRecord
{
	private string m_operatorName;

	private string[] m_operands;

	private byte[] m_inlineImageBytes;

	internal string OperatorName
	{
		get
		{
			return m_operatorName;
		}
		set
		{
			m_operatorName = value;
		}
	}

	internal string[] Operands
	{
		get
		{
			return m_operands;
		}
		set
		{
			m_operands = value;
		}
	}

	internal byte[] InlineImageBytes
	{
		get
		{
			return m_inlineImageBytes;
		}
		set
		{
			m_inlineImageBytes = value;
		}
	}

	public PdfRecord(string name, string[] operands)
	{
		m_operatorName = name;
		m_operands = operands;
	}

	internal PdfRecord(string name, byte[] imageData)
	{
		m_operatorName = name;
		m_inlineImageBytes = imageData;
	}
}

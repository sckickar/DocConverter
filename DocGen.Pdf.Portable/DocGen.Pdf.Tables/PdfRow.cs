namespace DocGen.Pdf.Tables;

public class PdfRow
{
	private object[] m_values;

	public object[] Values
	{
		get
		{
			return m_values;
		}
		set
		{
			m_values = value;
		}
	}

	internal PdfRow()
	{
	}

	internal PdfRow(object[] values)
		: this()
	{
		m_values = values;
	}
}

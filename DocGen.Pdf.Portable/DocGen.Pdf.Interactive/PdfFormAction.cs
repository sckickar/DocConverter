namespace DocGen.Pdf.Interactive;

public class PdfFormAction : PdfAction
{
	private bool m_include;

	private PdfFieldCollection m_fields;

	public virtual bool Include
	{
		get
		{
			return m_include;
		}
		set
		{
			m_include = value;
		}
	}

	public PdfFieldCollection Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new PdfFieldCollection();
				base.Dictionary.SetProperty("Fields", m_fields);
			}
			return m_fields;
		}
	}
}

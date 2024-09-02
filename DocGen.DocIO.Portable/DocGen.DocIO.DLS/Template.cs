namespace DocGen.DocIO.DLS;

public class Template
{
	private SttbfAssoc m_assocStrings;

	public string Path
	{
		get
		{
			return m_assocStrings.AttachedTemplate;
		}
		set
		{
			m_assocStrings.AttachedTemplate = value;
		}
	}

	internal Template(SttbfAssoc assocStrings)
	{
		m_assocStrings = assocStrings;
	}
}

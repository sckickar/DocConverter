namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class Month : DateBase
{
	private string m_possesiveForm;

	private string m_textual;

	internal string PossesiveForm
	{
		get
		{
			return m_possesiveForm;
		}
		set
		{
			m_possesiveForm = value;
		}
	}

	internal string Textual
	{
		get
		{
			return m_textual;
		}
		set
		{
			m_textual = value;
		}
	}
}

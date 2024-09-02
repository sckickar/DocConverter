namespace DocGen.Pdf.Security;

internal abstract class ECX9Params
{
	private ECX9Field m_parameters;

	public ECX9Field Parameters
	{
		get
		{
			if (m_parameters == null)
			{
				m_parameters = DefineParameters();
			}
			return m_parameters;
		}
	}

	protected abstract ECX9Field DefineParameters();
}

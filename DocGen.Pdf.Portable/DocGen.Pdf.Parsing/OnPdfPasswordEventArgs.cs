namespace DocGen.Pdf.Parsing;

public class OnPdfPasswordEventArgs
{
	private string m_userpassword;

	public string UserPassword
	{
		get
		{
			return m_userpassword;
		}
		set
		{
			m_userpassword = value;
		}
	}

	internal OnPdfPasswordEventArgs()
	{
		UserPassword = string.Empty;
	}
}

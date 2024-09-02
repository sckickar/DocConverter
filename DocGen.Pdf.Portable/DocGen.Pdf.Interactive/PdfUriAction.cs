using System;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfUriAction : PdfAction
{
	private string m_uri = string.Empty;

	public string Uri
	{
		get
		{
			return m_uri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (m_uri != value)
			{
				m_uri = value;
				if (PdfString.IsUnicode(value))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(m_uri);
					string @string = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
					m_uri = @string;
				}
				base.Dictionary.SetString("URI", m_uri);
			}
		}
	}

	public PdfUriAction()
	{
	}

	public PdfUriAction(string uri)
	{
		Uri = uri;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("S", new PdfName("URI"));
	}
}

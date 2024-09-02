using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfJavaScriptAction : PdfAction
{
	private string m_javaScript = string.Empty;

	public string JavaScript
	{
		get
		{
			return m_javaScript;
		}
		set
		{
			if (m_javaScript != value)
			{
				m_javaScript = value;
				base.Dictionary.SetString("JS", m_javaScript);
			}
		}
	}

	public PdfJavaScriptAction(string javaScript)
	{
		if (javaScript == null)
		{
			throw new ArgumentNullException("javaScript");
		}
		JavaScript = javaScript;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("S", new PdfName("JavaScript"));
		base.Dictionary.SetProperty("JS", new PdfString(m_javaScript));
	}
}

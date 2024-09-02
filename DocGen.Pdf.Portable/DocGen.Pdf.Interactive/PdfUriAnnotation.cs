using System;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfUriAnnotation : PdfActionLinkAnnotation
{
	private PdfUriAction m_uriAction = new PdfUriAction();

	public string Uri
	{
		get
		{
			return m_uriAction.Uri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Uri");
			}
			if (m_uriAction.Uri != value)
			{
				m_uriAction.Uri = value;
			}
			NotifyPropertyChanged("Uri");
		}
	}

	public override PdfAction Action
	{
		get
		{
			return base.Action;
		}
		set
		{
			base.Action = value;
			m_uriAction.Next = value;
			NotifyPropertyChanged("Action");
		}
	}

	public PdfUriAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfUriAnnotation(RectangleF rectangle, string uri)
		: base(rectangle)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("Uri");
		}
		Uri = uri;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Link"));
		base.Dictionary.SetProperty("A", m_uriAction);
	}
}

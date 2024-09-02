using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSubmitAction : PdfFormAction
{
	private string m_fileName = string.Empty;

	private PdfSubmitFormFlags m_flags;

	private HttpMethod m_httpMethod = HttpMethod.Post;

	private bool m_canonicalDateTimeFormat;

	private bool m_submitCoordinates;

	private bool m_includeNoValueFields;

	private bool m_includeIncrementalUpdates;

	private bool m_includeAnnotations;

	private bool m_excludeNonUserAnnotations;

	private bool m_embedForm;

	private SubmitDataFormat m_dataFormat = SubmitDataFormat.Fdf;

	public string Url => m_fileName;

	public HttpMethod HttpMethod
	{
		get
		{
			return m_httpMethod;
		}
		set
		{
			if (m_httpMethod != value)
			{
				m_httpMethod = value;
				if (m_httpMethod == HttpMethod.Get)
				{
					m_flags |= PdfSubmitFormFlags.GetMethod;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.GetMethod;
				}
			}
		}
	}

	public bool CanonicalDateTimeFormat
	{
		get
		{
			return m_canonicalDateTimeFormat;
		}
		set
		{
			if (m_canonicalDateTimeFormat != value)
			{
				m_canonicalDateTimeFormat = value;
				if (m_canonicalDateTimeFormat)
				{
					m_flags |= PdfSubmitFormFlags.CanonicalFormat;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.CanonicalFormat;
				}
			}
		}
	}

	public bool SubmitCoordinates
	{
		get
		{
			return m_submitCoordinates;
		}
		set
		{
			if (m_submitCoordinates != value)
			{
				m_submitCoordinates = value;
				if (m_submitCoordinates)
				{
					m_flags |= PdfSubmitFormFlags.SubmitCoordinates;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.SubmitCoordinates;
				}
			}
		}
	}

	public bool IncludeNoValueFields
	{
		get
		{
			return m_includeNoValueFields;
		}
		set
		{
			if (m_includeNoValueFields != value)
			{
				m_includeNoValueFields = value;
				if (m_includeNoValueFields)
				{
					m_flags |= PdfSubmitFormFlags.IncludeNoValueFields;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.IncludeNoValueFields;
				}
			}
		}
	}

	public bool IncludeIncrementalUpdates
	{
		get
		{
			return m_includeIncrementalUpdates;
		}
		set
		{
			if (m_includeIncrementalUpdates != value)
			{
				m_includeIncrementalUpdates = value;
				if (m_includeIncrementalUpdates)
				{
					m_flags |= PdfSubmitFormFlags.IncludeAppendSaves;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.IncludeAppendSaves;
				}
			}
		}
	}

	public bool IncludeAnnotations
	{
		get
		{
			return m_includeAnnotations;
		}
		set
		{
			if (m_includeAnnotations != value)
			{
				m_includeAnnotations = value;
				if (m_includeAnnotations)
				{
					m_flags |= PdfSubmitFormFlags.IncludeAnnotations;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.IncludeAnnotations;
				}
			}
		}
	}

	public bool ExcludeNonUserAnnotations
	{
		get
		{
			return m_excludeNonUserAnnotations;
		}
		set
		{
			if (m_excludeNonUserAnnotations != value)
			{
				m_excludeNonUserAnnotations = value;
				if (m_excludeNonUserAnnotations)
				{
					m_flags |= PdfSubmitFormFlags.ExclNonUserAnnots;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.ExclNonUserAnnots;
				}
			}
		}
	}

	public bool EmbedForm
	{
		get
		{
			return m_embedForm;
		}
		set
		{
			if (m_embedForm != value)
			{
				m_embedForm = value;
				if (m_embedForm)
				{
					m_flags |= PdfSubmitFormFlags.EmbedForm;
				}
				else
				{
					m_flags &= ~PdfSubmitFormFlags.EmbedForm;
				}
			}
		}
	}

	public SubmitDataFormat DataFormat
	{
		get
		{
			return m_dataFormat;
		}
		set
		{
			if (m_dataFormat != value)
			{
				m_dataFormat = value;
				switch (m_dataFormat)
				{
				case SubmitDataFormat.Pdf:
					m_flags |= PdfSubmitFormFlags.SubmitPdf;
					m_flags &= (PdfSubmitFormFlags)(-1);
					break;
				case SubmitDataFormat.Xfdf:
					m_flags |= PdfSubmitFormFlags.Xfdf;
					m_flags &= (PdfSubmitFormFlags)(-1);
					break;
				case SubmitDataFormat.Html:
					m_flags |= PdfSubmitFormFlags.ExportFormat;
					m_flags &= (PdfSubmitFormFlags)(-1);
					break;
				case SubmitDataFormat.Fdf:
					m_flags &= (PdfSubmitFormFlags)(-1);
					break;
				}
			}
		}
	}

	public override bool Include
	{
		get
		{
			return base.Include;
		}
		set
		{
			if (base.Include != value)
			{
				base.Include = value;
				if (base.Include)
				{
					m_flags &= ~PdfSubmitFormFlags.IncludeExclude;
				}
				else
				{
					m_flags |= PdfSubmitFormFlags.IncludeExclude;
				}
			}
		}
	}

	public PdfSubmitAction(string url)
	{
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		if (url.Length <= 0)
		{
			throw new ArgumentException("The URL can't be an empty string.", "url");
		}
		m_fileName = url;
		base.Dictionary.SetProperty("F", new PdfString(m_fileName));
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.BeginSave += Dictionary_BeginSave;
		base.Dictionary.SetProperty("S", new PdfName("SubmitForm"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		base.Dictionary.SetProperty("Flags", new PdfNumber((int)m_flags));
	}
}

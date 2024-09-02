using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfEmbeddedFileSpecification : PdfFileSpecificationBase
{
	private string m_description = string.Empty;

	private EmbeddedFile m_embeddedFile;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfPortfolioAttributes m_portfolioAttributes;

	private PdfAttachmentRelationship m_relationShip;

	public override string FileName
	{
		get
		{
			return m_embeddedFile.FileName;
		}
		set
		{
			m_embeddedFile.FileName = value;
		}
	}

	public byte[] Data
	{
		get
		{
			return m_embeddedFile.Data;
		}
		set
		{
			m_embeddedFile.Data = value;
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			if (m_description != value)
			{
				m_description = value;
				base.Dictionary.SetString("Desc", m_description);
			}
		}
	}

	public string MimeType
	{
		get
		{
			return m_embeddedFile.MimeType;
		}
		set
		{
			m_embeddedFile.MimeType = value;
		}
	}

	public DateTime CreationDate
	{
		get
		{
			return m_embeddedFile.Params.CreationDate;
		}
		set
		{
			m_embeddedFile.Params.CreationDate = value;
		}
	}

	public DateTime ModificationDate
	{
		get
		{
			return m_embeddedFile.Params.ModificationDate;
		}
		set
		{
			m_embeddedFile.Params.ModificationDate = value;
		}
	}

	public PdfPortfolioAttributes PortfolioAttributes
	{
		get
		{
			return m_portfolioAttributes;
		}
		set
		{
			m_portfolioAttributes = value;
			base.Dictionary.SetProperty("CI", m_portfolioAttributes);
		}
	}

	internal EmbeddedFile EmbeddedFile => m_embeddedFile;

	public PdfAttachmentRelationship Relationship
	{
		get
		{
			return m_relationShip;
		}
		set
		{
			m_relationShip = value;
			base.Dictionary.SetProperty("AFRelationship", new PdfName(m_relationShip));
		}
	}

	public PdfEmbeddedFileSpecification(string fileName)
		: base(fileName)
	{
		m_embeddedFile = new EmbeddedFile(fileName);
		Description = fileName;
	}

	public PdfEmbeddedFileSpecification(string fileName, byte[] data)
		: base(fileName)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_embeddedFile = new EmbeddedFile(fileName, data);
		Description = fileName;
	}

	public PdfEmbeddedFileSpecification(string fileName, Stream stream)
		: base(fileName)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_embeddedFile = new EmbeddedFile(fileName, stream);
		Description = fileName;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("EF", m_dictionary);
	}

	protected override void Save()
	{
		m_dictionary["F"] = new PdfReferenceHolder(m_embeddedFile);
		PdfString primitive = new PdfString(FormatFileName(FileName, flag: false));
		base.Dictionary.SetProperty("F", primitive);
		base.Dictionary.SetProperty("UF", primitive);
	}
}

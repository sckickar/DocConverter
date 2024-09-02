using System;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class UrlFileSpecification : PdfFileSpecificationBase
{
	private string m_fileName = string.Empty;

	public override string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("FileName can't be empty");
			}
			if (m_fileName != value)
			{
				m_fileName = value;
			}
		}
	}

	public UrlFileSpecification(string fileName)
		: base(fileName)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("FS", new PdfName("URL"));
	}

	protected override void Save()
	{
		base.Dictionary.SetProperty("F", new PdfString(FileName));
	}
}

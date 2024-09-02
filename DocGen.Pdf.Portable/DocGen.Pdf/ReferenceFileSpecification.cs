using System;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class ReferenceFileSpecification : PdfFileSpecificationBase
{
	private string m_fileName = string.Empty;

	private PdfFilePathType m_path;

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
			m_fileName = value;
		}
	}

	public ReferenceFileSpecification(string fileName, PdfFilePathType path)
		: base(fileName)
	{
		m_path = path;
		FileName = fileName;
	}

	internal ReferenceFileSpecification(string fileName)
		: base(fileName)
	{
		m_fileName = fileName;
	}

	protected override void Save()
	{
		bool flag = m_path == PdfFilePathType.Relative;
		string value = FormatFileName(FileName, flag);
		base.Dictionary.SetProperty("UF", new PdfString(value));
	}
}

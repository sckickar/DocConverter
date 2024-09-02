using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLaunchAction : PdfAction
{
	private ReferenceFileSpecification m_fileSpecification;

	private PdfFilePathType m_pathType = PdfFilePathType.Absolute;

	public string FileName
	{
		get
		{
			return m_fileSpecification.FileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("File name can not be empty");
			}
			if (m_fileSpecification.FileName != value)
			{
				m_fileSpecification.FileName = value;
			}
		}
	}

	public PdfLaunchAction(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_fileSpecification = new ReferenceFileSpecification(fileName, m_pathType);
	}

	public PdfLaunchAction(string fileName, PdfFilePathType path)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_pathType = path;
		m_fileSpecification = new ReferenceFileSpecification(fileName, m_pathType);
	}

	internal PdfLaunchAction(string fileName, bool loaded)
	{
		if (loaded)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			m_fileSpecification = new ReferenceFileSpecification(fileName);
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.BeginSave += Dictionary_BeginSave;
		base.Dictionary.SetProperty("S", new PdfName("Launch"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		base.Dictionary.SetProperty("F", new PdfReferenceHolder(m_fileSpecification));
	}
}

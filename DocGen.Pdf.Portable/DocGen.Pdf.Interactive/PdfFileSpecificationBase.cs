using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfFileSpecificationBase : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	public abstract string FileName { get; set; }

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfFileSpecificationBase(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		Initialize();
	}

	protected virtual void Initialize()
	{
		m_dictionary.SetProperty("Type", new PdfName("Filespec"));
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	protected abstract void Save();

	protected string FormatFileName(string fileName, bool flag)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (fileName.Length == 0)
		{
			throw new ArgumentException("fileName - string can not be empty");
		}
		string text = fileName.Replace("\\", "/");
		if (text.Substring(0, 2) == "\\")
		{
			text = text.Remove(1, 1);
		}
		if (text.Substring(0, 1) != "/" && !flag)
		{
			text = text;
		}
		return text;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}
}

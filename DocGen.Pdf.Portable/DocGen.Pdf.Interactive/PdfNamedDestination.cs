using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfNamedDestination : IPdfWrapper
{
	private PdfDestination m_destination;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfCrossTable m_crossTable = new PdfCrossTable();

	public virtual PdfDestination Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("The destination value can't be null");
			}
			if (value != m_destination)
			{
				m_destination = value;
				Dictionary.SetProperty("D", m_destination);
			}
		}
	}

	public virtual string Title
	{
		get
		{
			PdfString pdfString = Dictionary["Title"] as PdfString;
			string result = null;
			if (pdfString != null)
			{
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("The title can't be null");
			}
			Dictionary.SetString("Title", value);
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	internal PdfCrossTable CrossTable => m_crossTable;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfNamedDestination(string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("The title can't be null");
		}
		Title = title;
		Initialize();
	}

	internal PdfNamedDestination(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		m_dictionary = dictionary;
		m_crossTable = crossTable;
	}

	internal void Initialize()
	{
		Dictionary.BeginSave += Dictionary_BeginSave;
		Dictionary.SetProperty("S", new PdfName("GoTo"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Dictionary.SetProperty("D", m_destination);
	}
}

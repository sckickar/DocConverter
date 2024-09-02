using System;

namespace DocGen.Pdf;

public class PdfFileStructure
{
	private PdfVersion m_version;

	private PdfCrossReferenceType m_crossReferenceType;

	private PdfFileFormat m_fileformat;

	private bool m_incrementalUpdate;

	private bool m_taggedPdf;

	internal bool m_fileID;

	public PdfVersion Version
	{
		get
		{
			return m_version;
		}
		set
		{
			m_version = value;
			if (m_version <= PdfVersion.Version1_3)
			{
				m_crossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
			}
		}
	}

	public bool IncrementalUpdate
	{
		get
		{
			return m_incrementalUpdate;
		}
		set
		{
			m_incrementalUpdate = value;
		}
	}

	public bool EnableTrailerId
	{
		get
		{
			return m_fileID;
		}
		set
		{
			m_fileID = value;
		}
	}

	public PdfCrossReferenceType CrossReferenceType
	{
		get
		{
			return m_crossReferenceType;
		}
		set
		{
			m_crossReferenceType = value;
		}
	}

	internal PdfFileFormat FileFormat
	{
		get
		{
			return m_fileformat;
		}
		set
		{
			m_fileformat = value;
		}
	}

	public bool TaggedPdf
	{
		get
		{
			return m_taggedPdf;
		}
		internal set
		{
			if (m_taggedPdf != value)
			{
				m_taggedPdf = value;
			}
			OnTaggedPdfChanged(new EventArgs());
		}
	}

	internal event EventHandler TaggedPdfChanged;

	public PdfFileStructure()
	{
		m_version = PdfVersion.Version1_5;
		m_crossReferenceType = PdfCrossReferenceType.CrossReferenceStream;
		m_fileformat = PdfFileFormat.Plain;
		m_incrementalUpdate = true;
		m_taggedPdf = false;
	}

	protected void OnTaggedPdfChanged(EventArgs e)
	{
		this.TaggedPdfChanged?.Invoke(this, e);
	}
}

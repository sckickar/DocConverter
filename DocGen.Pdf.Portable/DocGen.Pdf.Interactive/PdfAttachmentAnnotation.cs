using System;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAttachmentAnnotation : PdfFileAnnotation
{
	private PdfAttachmentIcon m_attachmentIcon;

	private PdfEmbeddedFileSpecification m_fileSpecification;

	public PdfAttachmentIcon Icon
	{
		get
		{
			return m_attachmentIcon;
		}
		set
		{
			m_attachmentIcon = value;
			base.Dictionary.SetName("Name", m_attachmentIcon.ToString());
		}
	}

	public override string FileName
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
				throw new ArgumentException("FileName can't be empty");
			}
			if (m_fileSpecification.FileName != value)
			{
				m_fileSpecification.FileName = value;
			}
		}
	}

	public PdfPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory != null)
			{
				return m_reviewHistory;
			}
			return m_reviewHistory = new PdfPopupAnnotationCollection(this, isReview: true);
		}
	}

	public PdfPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments != null)
			{
				return m_comments;
			}
			return m_comments = new PdfPopupAnnotationCollection(this, isReview: false);
		}
	}

	public PdfAttachmentAnnotation(RectangleF rectangle, string fileName, byte[] data)
		: base(rectangle)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_fileSpecification = new PdfEmbeddedFileSpecification(fileName, data);
	}

	public PdfAttachmentAnnotation(RectangleF rectangle, string fileName, Stream stream)
		: base(rectangle)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_fileSpecification = new PdfEmbeddedFileSpecification(fileName, stream);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("FileAttachment"));
	}

	protected override void Save()
	{
		base.Save();
		base.Dictionary.SetProperty("FS", new PdfReferenceHolder(m_fileSpecification));
	}
}

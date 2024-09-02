using System;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSoundAnnotation : PdfFileAnnotation
{
	private PdfSoundIcon m_icon;

	private PdfSound m_sound;

	public PdfSoundIcon Icon
	{
		get
		{
			return m_icon;
		}
		set
		{
			if (m_icon != value)
			{
				m_icon = value;
				base.Dictionary.SetName("Name", m_icon.ToString());
			}
			NotifyPropertyChanged("Icon");
		}
	}

	public PdfSound Sound
	{
		get
		{
			return m_sound;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Sound");
			}
			if (value != m_sound)
			{
				m_sound = value;
			}
			NotifyPropertyChanged("Sound");
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

	public override string FileName
	{
		get
		{
			return m_sound.FileName;
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
			if (m_sound.FileName != value)
			{
				m_sound.FileName = value;
			}
			NotifyPropertyChanged("FileName");
		}
	}

	public PdfSoundAnnotation(RectangleF rectangle, Stream data)
		: base(rectangle)
	{
		if (data == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_sound = new PdfSound(data);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Sound"));
	}

	protected override void Save()
	{
		base.Save();
		base.Dictionary.SetProperty("Sound", new PdfReferenceHolder(m_sound));
	}
}

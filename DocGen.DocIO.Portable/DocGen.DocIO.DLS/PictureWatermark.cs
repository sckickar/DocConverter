using System;
using System.IO;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class PictureWatermark : Watermark
{
	private WPicture m_picture;

	private ImageRecord m_imageRecord;

	private byte m_bFlags = 1;

	private int m_originalPib = -1;

	public float Scaling
	{
		get
		{
			return m_picture.HeightScale;
		}
		set
		{
			WPicture picture = m_picture;
			float heightScale = (m_picture.WidthScale = value);
			picture.HeightScale = heightScale;
		}
	}

	public bool Washout
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal Image Picture
	{
		get
		{
			if (m_picture.Document == null && m_imageRecord != null)
			{
				return GetImage(m_imageRecord.ImageBytes);
			}
			return m_picture.GetImage(m_picture.ImageBytes, isImageFromScratch: false);
		}
		set
		{
			if (m_picture.Document == null || !m_picture.Document.IsOpening)
			{
				m_picture.Title = string.Empty;
			}
			m_originalPib = -1;
			if (m_picture.Document != null)
			{
				m_picture.LoadImage(value);
				return;
			}
			byte[] imageData = value.ImageData;
			m_imageRecord = new ImageRecord(null, imageData);
			if (value.IsMetafile)
			{
				m_imageRecord.IsMetafile = true;
			}
		}
	}

	internal WPicture WordPicture
	{
		get
		{
			return m_picture;
		}
		set
		{
			if (m_picture != null)
			{
				m_picture.SetOwner(null, null);
			}
			m_picture = value;
			if (m_picture != null)
			{
				m_picture.SetOwner(this);
			}
		}
	}

	internal int OriginalPib
	{
		get
		{
			return m_originalPib;
		}
		set
		{
			m_originalPib = value;
		}
	}

	public void LoadPicture(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("image bytes is empty");
		}
		MemoryStream stream = new MemoryStream(bytes);
		Picture = new Image(stream);
	}

	public PictureWatermark()
		: base(WatermarkType.PictureWatermark)
	{
		m_picture = new WPicture(null);
		m_picture.SetOwner(this);
		m_picture.HorizontalAlignment = ShapeHorizontalAlignment.Center;
		m_picture.VerticalAlignment = ShapeVerticalAlignment.Center;
		m_picture.SetTextWrappingStyleValue(TextWrappingStyle.Behind);
		m_picture.IsBelowText = true;
	}

	internal PictureWatermark(Image image, bool washout)
		: this()
	{
		Picture = image;
		Washout = washout;
	}

	internal PictureWatermark(WordDocument doc)
		: base(doc, WatermarkType.PictureWatermark)
	{
		m_picture = new WPicture(doc);
		m_picture.SetOwner(this);
	}

	internal override void Close()
	{
		if (m_picture != null)
		{
			m_picture.Close();
			m_picture = null;
		}
		if (m_imageRecord != null)
		{
			m_imageRecord.Close();
			m_imageRecord = null;
		}
		base.Close();
	}

	internal void UpdateImage()
	{
		if (m_imageRecord != null)
		{
			m_picture.LoadImage(m_imageRecord.ImageBytes, m_imageRecord.IsMetafile);
			m_imageRecord.Close();
			m_imageRecord = null;
		}
	}

	private Image GetImage(byte[] imageBytes)
	{
		Image result = null;
		if (imageBytes != null)
		{
			try
			{
				result = Image.FromStream(new MemoryStream(imageBytes));
				imageBytes = null;
			}
			catch
			{
				throw new ArgumentException("Argument is not image byte array");
			}
		}
		return result;
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("image", m_picture);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (!Washout)
		{
			writer.WriteValue("PictureWashout", Washout);
		}
		if (m_originalPib != -1)
		{
			writer.WriteValue("PicturePib", m_originalPib);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("PictureWashout"))
		{
			Washout = reader.ReadBoolean("PictureWashout");
		}
		if (reader.HasAttribute("PicturePib"))
		{
			m_originalPib = reader.ReadInt("PicturePib");
		}
	}

	protected override object CloneImpl()
	{
		PictureWatermark pictureWatermark = (PictureWatermark)base.CloneImpl();
		pictureWatermark.WordPicture = (WPicture)WordPicture.Clone();
		pictureWatermark.WordPicture.SetOwner(pictureWatermark);
		return pictureWatermark;
	}
}

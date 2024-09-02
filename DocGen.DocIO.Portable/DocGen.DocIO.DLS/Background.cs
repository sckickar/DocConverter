using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class Background : XDLSSerializableBase
{
	private BackgroundType m_effectType;

	private Color m_color = Color.White;

	private Color m_backColor = Color.White;

	private ImageRecord m_imageRecord;

	private BackgroundGradient m_gradient = new BackgroundGradient();

	private BackgroundFillType m_fillType;

	private EscherClass m_escher;

	private Stream m_patternFill;

	private byte[] m_patternImage;

	public BackgroundType Type
	{
		get
		{
			return m_effectType;
		}
		set
		{
			m_effectType = value;
			if (!base.Document.IsOpening)
			{
				base.Document.Settings.DisplayBackgrounds = true;
			}
		}
	}

	public byte[] Picture
	{
		get
		{
			if (m_imageRecord == null)
			{
				return null;
			}
			return m_imageRecord.ImageBytes;
		}
		set
		{
			if (m_imageRecord != null)
			{
				m_imageRecord.OccurenceCount--;
			}
			m_fillType = BackgroundFillType.msofillPicture;
			m_effectType = BackgroundType.Picture;
			LoadImage(value);
			if (!base.Document.IsOpening)
			{
				base.Document.Settings.DisplayBackgrounds = true;
			}
		}
	}

	internal DocGen.DocIO.DLS.Entities.Image Image => GetImageValue();

	public Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_effectType = BackgroundType.Color;
			m_color = value;
			if (!base.Document.IsOpening)
			{
				base.Document.Settings.DisplayBackgrounds = true;
			}
		}
	}

	public BackgroundGradient Gradient
	{
		get
		{
			return m_gradient;
		}
		set
		{
			m_gradient = value;
			m_effectType = BackgroundType.Gradient;
			m_gradient.SetOwner(base.Document);
			if (!base.Document.IsOpening)
			{
				base.Document.Settings.DisplayBackgrounds = true;
			}
		}
	}

	internal ImageRecord ImageRecord
	{
		get
		{
			return m_imageRecord;
		}
		set
		{
			if (m_imageRecord != null)
			{
				m_imageRecord.OccurenceCount--;
			}
			m_imageRecord = value;
			m_imageRecord.OccurenceCount++;
		}
	}

	internal byte[] ImageBytes
	{
		get
		{
			if (m_imageRecord == null)
			{
				return null;
			}
			return m_imageRecord.ImageBytes;
		}
		set
		{
			if (m_imageRecord != null)
			{
				m_imageRecord.OccurenceCount--;
			}
			LoadImage(value);
		}
	}

	internal BackgroundFillType FillType
	{
		get
		{
			return m_fillType;
		}
		set
		{
			m_fillType = value;
		}
	}

	internal Color PictureBackColor => m_backColor;

	internal Stream PatternFill
	{
		get
		{
			return m_patternFill;
		}
		set
		{
			m_effectType = BackgroundType.Texture;
			m_patternFill = value;
		}
	}

	internal byte[] PatternImageBytes
	{
		get
		{
			return m_patternImage;
		}
		set
		{
			m_patternImage = value;
		}
	}

	internal Background(WordDocument doc, BackgroundType type)
		: base(doc, null)
	{
		m_effectType = type;
	}

	internal Background(WordDocument doc)
		: base(doc, null)
	{
		m_escher = doc.Escher;
		GetBackgroundData(m_escher.BackgroundContainer, isDocBackground: true);
	}

	internal Background(WordDocument doc, MsofbtSpContainer container)
		: base(doc, null)
	{
		m_escher = doc.Escher;
		GetBackgroundData(container, isDocBackground: false);
	}

	internal Background Clone()
	{
		Background background = new Background(base.Document, Type);
		if (m_imageRecord != null)
		{
			background.m_imageRecord = new ImageRecord(base.Document, m_imageRecord);
		}
		if (m_gradient != null)
		{
			background.Gradient = Gradient.Clone();
		}
		background.Color = Color;
		return background;
	}

	internal void UpdateImageRecord(WordDocument doc)
	{
		if (m_imageRecord != null)
		{
			ImageRecord imageRecord = m_imageRecord;
			if (imageRecord.IsMetafile)
			{
				m_imageRecord = doc.Images.LoadMetaFileImage(imageRecord.m_imageBytes, isCompressed: true);
			}
			else
			{
				m_imageRecord = doc.Images.LoadImage(imageRecord.ImageBytes);
			}
			m_imageRecord.Size = imageRecord.Size;
			m_imageRecord.ImageFormat = imageRecord.ImageFormat;
			m_imageRecord.Length = imageRecord.Length;
			imageRecord.Close();
			imageRecord = null;
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Type"))
		{
			m_effectType = (BackgroundType)(object)reader.ReadEnum("Type", typeof(BackgroundType));
		}
		if (reader.HasAttribute("FillColor"))
		{
			m_color = reader.ReadColor("FillColor");
		}
		if (reader.HasAttribute("FillBackgroundColor"))
		{
			m_backColor = reader.ReadColor("FillBackgroundColor");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Type", m_effectType);
		if (m_imageRecord != null && m_imageRecord.IsMetafile)
		{
			writer.WriteValue("IsMetafile", m_imageRecord.IsMetafile);
		}
		if (m_color != Color.White)
		{
			writer.WriteValue("FillColor", m_color);
		}
		if (m_backColor != Color.White)
		{
			writer.WriteValue("FillBackgroundColor", m_backColor);
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (ImageBytes != null)
		{
			writer.WriteChildBinaryElement("image", ImageBytes);
		}
		else
		{
			(writer as XDLSWriter).WriteImage(Image);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "image")
		{
			LoadImage(reader.ReadChildBinaryElement());
		}
		return result;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("gradient", m_gradient);
	}

	internal override void Close()
	{
		base.Close();
		if (m_imageRecord != null)
		{
			m_imageRecord.Close();
			m_imageRecord = null;
		}
		if (m_gradient != null)
		{
			m_gradient.Close();
			m_gradient = null;
		}
		if (m_escher != null)
		{
			m_escher.Close();
			m_escher = null;
		}
		if (m_patternFill != null)
		{
			m_patternFill.Close();
			m_patternFill = null;
		}
		if (m_patternImage != null)
		{
			m_patternImage = null;
		}
	}

	internal void SetBackgroundColor(Color color)
	{
		m_color = color;
	}

	internal void SetPatternFillValue(Stream stream)
	{
		m_patternFill = stream;
	}

	private void GetBackgroundData(MsofbtSpContainer container, bool isDocBackground)
	{
		if (container == null || !container.HasFillEffect())
		{
			return;
		}
		m_fillType = container.GetBackgroundFillType();
		m_effectType = container.GetBackgroundType();
		if (m_effectType == BackgroundType.NoBackground && isDocBackground)
		{
			m_effectType = BackgroundType.Color;
		}
		if (m_effectType == BackgroundType.NoBackground)
		{
			return;
		}
		switch (m_effectType)
		{
		case BackgroundType.Color:
			m_color = container.GetBackgroundColor(isPictureBackground: false);
			if (m_effectType == BackgroundType.Color && m_color == Color.White)
			{
				m_effectType = BackgroundType.NoBackground;
			}
			break;
		case BackgroundType.Picture:
		case BackgroundType.Texture:
			m_imageRecord = container.GetBackgroundImage(m_escher);
			m_backColor = container.GetBackgroundColor(isPictureBackground: true);
			break;
		case BackgroundType.Gradient:
			m_gradient = new BackgroundGradient(base.Document, container);
			m_gradient.SetOwner(base.Document);
			break;
		}
	}

	private DocGen.DocIO.DLS.Entities.Image GetImageValue()
	{
		DocGen.DocIO.DLS.Entities.Image result = null;
		if (ImageBytes != null)
		{
			try
			{
				result = DocGen.DocIO.DLS.Entities.Image.FromStream(new MemoryStream(ImageBytes));
			}
			catch
			{
				throw new ArgumentException("Argument is not image byte array");
			}
		}
		return result;
	}

	private DocGen.DocIO.DLS.Entities.Image GetImage(byte[] imageBytes)
	{
		DocGen.DocIO.DLS.Entities.Image result = null;
		if (imageBytes != null)
		{
			try
			{
				result = DocGen.DocIO.DLS.Entities.Image.FromStream(new MemoryStream(imageBytes));
				imageBytes = null;
			}
			catch
			{
				throw new ArgumentException("Argument is not image byte array");
			}
		}
		return result;
	}

	private void LoadImage(byte[] imageBytes)
	{
		if (imageBytes == null)
		{
			throw new ArgumentNullException("image");
		}
		if (GetImage(imageBytes).IsMetafile)
		{
			m_imageRecord = base.Document.Images.LoadMetaFileImage(imageBytes, isCompressed: false);
		}
		else
		{
			m_imageRecord = base.Document.Images.LoadImage(imageBytes);
		}
	}

	internal bool Compare(Background background)
	{
		if (Type != background.Type || FillType != background.FillType || Color != background.Color || PictureBackColor != background.PictureBackColor)
		{
			return false;
		}
		if ((Gradient == null && background.Gradient != null) || (Gradient != null && background.Gradient == null) || (PatternFill == null && background.PatternFill != null) || (PatternFill != null && background.PatternFill == null) || (PatternImageBytes == null && background.PatternImageBytes != null) || (PatternImageBytes != null && background.PatternImageBytes == null) || (ImageRecord == null && background.ImageRecord != null) || (ImageRecord != null && background.ImageRecord == null))
		{
			return false;
		}
		if (Gradient != null && !Gradient.Compare(background.Gradient))
		{
			return false;
		}
		if (PatternFill != null && !Comparison.CompareStream(PatternFill, background.PatternFill))
		{
			return false;
		}
		if (PatternImageBytes != null && !Comparison.CompareBytes(PatternImageBytes, background.PatternImageBytes))
		{
			return false;
		}
		if (ImageRecord != null && ImageRecord.comparedImageName != background.ImageRecord.comparedImageName)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)Type + ";");
		stringBuilder.Append((int)FillType + ";");
		stringBuilder.Append(Color.ToArgb() + ";");
		stringBuilder.Append(PictureBackColor.ToArgb() + ";");
		if (ImageRecord != null)
		{
			stringBuilder.Append(ImageRecord.comparedImageName);
		}
		if (Gradient != null)
		{
			stringBuilder.Append(Gradient.GetAsString());
		}
		if (PatternFill != null)
		{
			stringBuilder.Append(PatternFill.Length.ToString());
		}
		if (PatternImageBytes != null)
		{
			stringBuilder.Append(base.Document.Comparison.ConvertBytesAsHash(PatternImageBytes));
		}
		return stringBuilder;
	}
}

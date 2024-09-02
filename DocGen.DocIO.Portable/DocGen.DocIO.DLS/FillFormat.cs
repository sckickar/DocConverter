using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class FillFormat
{
	private Color m_BackColor;

	private Color m_ForeColor;

	private Color m_recolorTarget;

	private PatternType m_Pattern = PatternType.Mixed;

	private TextureAlignment m_TextureAlignment;

	private double m_TextureHorizontalScale;

	private double m_TextureOffsetX;

	private double m_TextureOffsetY;

	private double m_TextureVerticalScale;

	private float m_Transparency;

	private FillType m_FillType;

	private ImageRecord m_ImageRecord;

	private FlipOrientation m_FlipOrientation;

	private TileRectangle m_SourceRectangle;

	private TileRectangle m_FillRectangle;

	private GradientFill m_GradientFill;

	private BlipCompressionType m_compressionMode;

	private BlipFormat m_blipFormat;

	private string m_alternateHRef;

	private float m_angle;

	private FillAspect m_fillAspect;

	private float m_focus;

	private float m_focusPositionX;

	private float m_focusPositionY;

	private float m_positionX;

	private float m_positionY;

	private float m_focusSizeX;

	private float m_focusSizeY;

	private float m_secondaryOpacity;

	private byte m_flagA;

	private List<DictionaryEntry> m_fillSchemeColor;

	private ShapeBase m_shape;

	private ChildShape m_childShape;

	private float m_contrast;

	private byte m_bFlags = 11;

	internal const byte IsDefaultFillKey = 0;

	internal const byte IsDefaultFillColorKey = 1;

	private WPicture m_picture;

	internal GradientFill GradientFill
	{
		get
		{
			if (m_GradientFill == null)
			{
				m_GradientFill = new GradientFill();
			}
			return m_GradientFill;
		}
		set
		{
			m_GradientFill = value;
		}
	}

	internal TileRectangle FillRectangle
	{
		get
		{
			if (m_FillRectangle == null)
			{
				m_FillRectangle = new TileRectangle();
			}
			return m_FillRectangle;
		}
		set
		{
			m_FillRectangle = value;
		}
	}

	internal TileRectangle SourceRectangle
	{
		get
		{
			if (m_SourceRectangle == null)
			{
				m_SourceRectangle = new TileRectangle();
			}
			return m_SourceRectangle;
		}
		set
		{
			m_SourceRectangle = value;
		}
	}

	internal FlipOrientation FlipOrientation
	{
		get
		{
			return m_FlipOrientation;
		}
		set
		{
			m_FlipOrientation = value;
		}
	}

	internal ImageRecord ImageRecord
	{
		get
		{
			return m_ImageRecord;
		}
		set
		{
			m_ImageRecord = value;
		}
	}

	internal bool IsDefaultFill
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

	internal bool IsDefaultFillColor
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public bool Fill
	{
		get
		{
			return (m_flagA & 1) != 0;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsFillStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsFillStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsFillStyleInline = true;
			}
			m_flagA = (byte)((m_flagA & 0xFEu) | (value ? 1u : 0u));
			FillFormatChanged();
		}
	}

	public Color Color
	{
		get
		{
			return m_BackColor;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsFillStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsFillStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsFillStyleInline = true;
			}
			m_BackColor = value;
			FillFormatChanged();
		}
	}

	internal Color ReColorTarget
	{
		get
		{
			return m_recolorTarget;
		}
		set
		{
			m_recolorTarget = value;
		}
	}

	internal Color ForeColor
	{
		get
		{
			return m_ForeColor;
		}
		set
		{
			m_ForeColor = value;
		}
	}

	internal PatternType Pattern
	{
		get
		{
			return m_Pattern;
		}
		set
		{
			m_Pattern = value;
		}
	}

	internal bool RotateWithObject
	{
		get
		{
			return (m_flagA & 2) >> 1 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal TextureAlignment TextureAlignment
	{
		get
		{
			return m_TextureAlignment;
		}
		set
		{
			m_TextureAlignment = value;
		}
	}

	internal double TextureHorizontalScale
	{
		get
		{
			return m_TextureHorizontalScale;
		}
		set
		{
			m_TextureHorizontalScale = value;
		}
	}

	internal double TextureOffsetX
	{
		get
		{
			return m_TextureOffsetX;
		}
		set
		{
			m_TextureOffsetX = value;
		}
	}

	internal double TextureOffsetY
	{
		get
		{
			return m_TextureOffsetY;
		}
		set
		{
			m_TextureOffsetY = value;
		}
	}

	internal bool TextureTile
	{
		get
		{
			return (m_flagA & 4) >> 2 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal double TextureVerticalScale
	{
		get
		{
			return m_TextureVerticalScale;
		}
		set
		{
			m_TextureVerticalScale = value;
		}
	}

	public float Transparency
	{
		get
		{
			return m_Transparency;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsFillStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsFillStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsFillStyleInline = true;
			}
			m_Transparency = value;
			FillFormatChanged();
		}
	}

	internal float Contrast
	{
		get
		{
			return m_contrast;
		}
		set
		{
			m_contrast = value;
		}
	}

	internal FillType FillType
	{
		get
		{
			return m_FillType;
		}
		set
		{
			m_FillType = value;
		}
	}

	internal BlipCompressionType BlipCompressionMode
	{
		get
		{
			return m_compressionMode;
		}
		set
		{
			m_compressionMode = value;
		}
	}

	internal BlipFormat BlipFormat
	{
		get
		{
			if (m_blipFormat == null)
			{
				m_blipFormat = new BlipFormat(m_shape);
			}
			return m_blipFormat;
		}
		set
		{
			m_blipFormat = value;
		}
	}

	internal bool AlignWithShape
	{
		get
		{
			return (m_flagA & 8) >> 3 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool DetectMouseClick
	{
		get
		{
			return (m_flagA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool ReColor
	{
		get
		{
			return (m_flagA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal string AlternateHRef
	{
		get
		{
			return m_alternateHRef;
		}
		set
		{
			m_alternateHRef = value;
		}
	}

	internal float Angle
	{
		get
		{
			return m_angle;
		}
		set
		{
			m_angle = value;
		}
	}

	internal FillAspect Aspect
	{
		get
		{
			return m_fillAspect;
		}
		set
		{
			m_fillAspect = value;
		}
	}

	internal float Focus
	{
		get
		{
			return m_focus;
		}
		set
		{
			m_focus = value;
		}
	}

	internal float FocusPositionX
	{
		get
		{
			return m_focusPositionX;
		}
		set
		{
			m_focusPositionX = value;
		}
	}

	internal float FocusPositionY
	{
		get
		{
			return m_focusPositionY;
		}
		set
		{
			m_focusPositionY = value;
		}
	}

	internal float PositionX
	{
		get
		{
			return m_positionX;
		}
		set
		{
			m_positionX = value;
		}
	}

	internal float PositionY
	{
		get
		{
			return m_positionY;
		}
		set
		{
			m_positionY = value;
		}
	}

	internal float FocusSizeX
	{
		get
		{
			return m_focusSizeX;
		}
		set
		{
			m_focusSizeX = value;
		}
	}

	internal float FocusSizeY
	{
		get
		{
			return m_focusSizeY;
		}
		set
		{
			m_focusSizeY = value;
		}
	}

	internal float SecondaryOpacity
	{
		get
		{
			return m_secondaryOpacity;
		}
		set
		{
			m_secondaryOpacity = value;
		}
	}

	internal bool Visible
	{
		get
		{
			return (m_flagA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal List<DictionaryEntry> FillSchemeColorTransforms
	{
		get
		{
			if (m_fillSchemeColor == null)
			{
				m_fillSchemeColor = new List<DictionaryEntry>();
			}
			return m_fillSchemeColor;
		}
		set
		{
			m_fillSchemeColor = value;
		}
	}

	internal bool IsGrpFill
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public FillFormat(Shape shape)
		: this((ShapeBase)shape)
	{
	}

	internal FillFormat(ShapeBase shape)
	{
		m_shape = shape;
		m_flagA = 1;
		Visible = true;
		m_FillType = FillType.FillSolid;
		m_BackColor = Color.White;
		FillFormatChanged();
	}

	internal FillFormat(ChildShape shape)
	{
		m_childShape = shape;
		Visible = true;
		if (m_childShape.DocxProps.ContainsKey("gradFill"))
		{
			m_childShape.DocxProps.Remove("gradFill");
		}
		if (m_childShape.DocxProps.ContainsKey("blipFill"))
		{
			m_childShape.DocxProps.Remove("blipFill");
		}
		if (m_childShape.DocxProps.ContainsKey("pattFill"))
		{
			m_childShape.DocxProps.Remove("pattFill");
		}
		if (m_childShape.Docx2007Props.ContainsKey("fill"))
		{
			m_childShape.Docx2007Props.Remove("fill");
		}
	}

	internal FillFormat(WPicture picture)
	{
		m_picture = picture;
	}

	internal FillFormat(InlineShapeObject inlineShapeObject)
	{
	}

	private void FillFormatChanged()
	{
		if (m_shape is Shape)
		{
			if ((m_shape as Shape).DocxProps.ContainsKey("gradFill"))
			{
				(m_shape as Shape).DocxProps.Remove("gradFill");
			}
			if ((m_shape as Shape).DocxProps.ContainsKey("blipFill"))
			{
				(m_shape as Shape).DocxProps.Remove("blipFill");
			}
			if ((m_shape as Shape).DocxProps.ContainsKey("pattFill"))
			{
				(m_shape as Shape).DocxProps.Remove("pattFill");
			}
			if ((m_shape as Shape).Docx2007Props.ContainsKey("fill"))
			{
				(m_shape as Shape).Docx2007Props.Remove("fill");
			}
		}
		else if (m_shape is GroupShape)
		{
			if ((m_shape as GroupShape).DocxProps.ContainsKey("gradFill"))
			{
				(m_shape as GroupShape).DocxProps.Remove("gradFill");
			}
			if ((m_shape as GroupShape).DocxProps.ContainsKey("blipFill"))
			{
				(m_shape as GroupShape).DocxProps.Remove("blipFill");
			}
			if ((m_shape as GroupShape).DocxProps.ContainsKey("pattFill"))
			{
				(m_shape as GroupShape).DocxProps.Remove("pattFill");
			}
		}
	}

	internal FillFormat Clone()
	{
		FillFormat fillFormat = (FillFormat)MemberwiseClone();
		if (m_ImageRecord != null)
		{
			ImageRecord imageRecord = new ImageRecord((m_picture != null) ? m_picture.Document : ((m_childShape != null) ? m_childShape.Document : m_shape.Document), m_ImageRecord);
			fillFormat.m_ImageRecord = imageRecord;
		}
		if (FillRectangle != null)
		{
			fillFormat.FillRectangle = FillRectangle.Clone();
		}
		if (SourceRectangle != null)
		{
			fillFormat.SourceRectangle = SourceRectangle.Clone();
		}
		if (BlipFormat != null)
		{
			fillFormat.BlipFormat = BlipFormat.Clone();
		}
		if (GradientFill != null && GradientFill.GradientStops != null && GradientFill.GradientStops.Count > 0)
		{
			fillFormat.GradientFill = GradientFill.Clone();
		}
		return fillFormat;
	}

	internal void Close()
	{
		if (m_GradientFill != null)
		{
			m_GradientFill.Close();
			m_GradientFill = null;
		}
		if (m_FillRectangle != null)
		{
			m_FillRectangle = null;
		}
		if (m_SourceRectangle != null)
		{
			m_SourceRectangle = null;
		}
		if (m_ImageRecord != null)
		{
			m_ImageRecord.Close();
			m_ImageRecord = null;
		}
		if (m_blipFormat != null)
		{
			m_blipFormat.Close();
			m_blipFormat = null;
		}
		if (m_fillSchemeColor != null)
		{
			m_fillSchemeColor.Clear();
			m_fillSchemeColor = null;
		}
		m_shape = null;
		m_picture = null;
	}

	internal bool Compare(FillFormat fillFormat)
	{
		if (IsDefaultFill != fillFormat.IsDefaultFill || IsDefaultFillColor != fillFormat.IsDefaultFillColor || Fill != fillFormat.Fill || RotateWithObject != fillFormat.RotateWithObject || TextureTile != fillFormat.TextureTile || AlignWithShape != fillFormat.AlignWithShape || DetectMouseClick != fillFormat.DetectMouseClick || ReColor != fillFormat.ReColor || Visible != fillFormat.Visible || FlipOrientation != fillFormat.FlipOrientation || Pattern != fillFormat.Pattern || TextureAlignment != fillFormat.TextureAlignment || TextureHorizontalScale != fillFormat.TextureHorizontalScale || TextureOffsetX != fillFormat.TextureOffsetX || TextureOffsetY != fillFormat.TextureOffsetY || TextureVerticalScale != fillFormat.TextureVerticalScale || Transparency != fillFormat.Transparency || Contrast != fillFormat.Contrast || FillType != fillFormat.FillType || BlipCompressionMode != fillFormat.BlipCompressionMode || AlternateHRef != fillFormat.AlternateHRef || Angle != fillFormat.Angle || Aspect != fillFormat.Aspect || Focus != fillFormat.Focus || FocusPositionX != fillFormat.FocusPositionX || FocusPositionY != fillFormat.FocusPositionY || PositionX != fillFormat.PositionX || PositionY != fillFormat.PositionY || FocusSizeX != fillFormat.FocusSizeX || FocusSizeY != fillFormat.FocusSizeY || SecondaryOpacity != fillFormat.SecondaryOpacity || Color.ToArgb() != fillFormat.Color.ToArgb() || ForeColor.ToArgb() != fillFormat.ForeColor.ToArgb() || ReColorTarget.ToArgb() != fillFormat.ReColorTarget.ToArgb())
		{
			return false;
		}
		if ((FillRectangle != null && fillFormat.FillRectangle == null) || (FillRectangle == null && fillFormat.FillRectangle != null) || (GradientFill != null && fillFormat.GradientFill == null) || (GradientFill == null && fillFormat.GradientFill != null) || (BlipFormat != null && fillFormat.BlipFormat == null) || (BlipFormat == null && fillFormat.BlipFormat != null) || (FillSchemeColorTransforms != null && fillFormat.FillSchemeColorTransforms == null) || (FillSchemeColorTransforms == null && fillFormat.FillSchemeColorTransforms != null))
		{
			return false;
		}
		if (FillRectangle != null && fillFormat.FillRectangle != null && !FillRectangle.Compare(fillFormat.FillRectangle))
		{
			return false;
		}
		if (GradientFill != null && fillFormat.GradientFill != null && !GradientFill.Compare(fillFormat.GradientFill))
		{
			return false;
		}
		if (BlipFormat != null && fillFormat.BlipFormat != null && !BlipFormat.Compare(fillFormat.BlipFormat))
		{
			return false;
		}
		if (FillSchemeColorTransforms != null && fillFormat.FillSchemeColorTransforms != null)
		{
			if (FillSchemeColorTransforms.Count != fillFormat.FillSchemeColorTransforms.Count)
			{
				return false;
			}
			for (int i = 0; i < FillSchemeColorTransforms.Count; i++)
			{
				if (FillSchemeColorTransforms[i].Key != fillFormat.FillSchemeColorTransforms[i].Key || FillSchemeColorTransforms[i].Value != fillFormat.FillSchemeColorTransforms[i].Value)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = (IsDefaultFill ? "1" : "0");
		stringBuilder.Append(value);
		value = (IsDefaultFillColor ? "1" : "0");
		stringBuilder.Append(value);
		value = (Fill ? "1" : "0");
		stringBuilder.Append(value);
		value = (RotateWithObject ? "1" : "0");
		stringBuilder.Append(value);
		value = (TextureTile ? "1" : "0");
		stringBuilder.Append(value);
		value = (AlignWithShape ? "1" : "0");
		stringBuilder.Append(value);
		value = (DetectMouseClick ? "1" : "0");
		stringBuilder.Append(value);
		value = (ReColor ? "1" : "0");
		stringBuilder.Append(value);
		value = (Visible ? "1" : "0");
		stringBuilder.Append(value);
		stringBuilder.Append((int)FlipOrientation + ",");
		stringBuilder.Append((int)Pattern + ",");
		stringBuilder.Append((int)TextureAlignment + ",");
		stringBuilder.Append(TextureHorizontalScale + ",");
		stringBuilder.Append(TextureOffsetX + ",");
		stringBuilder.Append(TextureOffsetY + ",");
		stringBuilder.Append(TextureVerticalScale + ",");
		stringBuilder.Append(Transparency + ",");
		stringBuilder.Append(Contrast + ",");
		stringBuilder.Append((int)FillType + ",");
		stringBuilder.Append((int)BlipCompressionMode + ",");
		stringBuilder.Append(AlternateHRef + ",");
		stringBuilder.Append(Angle + ",");
		stringBuilder.Append((int)Aspect + ",");
		stringBuilder.Append(Focus + ",");
		stringBuilder.Append(FocusPositionX + ",");
		stringBuilder.Append(FocusPositionY + ",");
		stringBuilder.Append(PositionX + ",");
		stringBuilder.Append(PositionY + ",");
		stringBuilder.Append(FocusSizeX + ",");
		stringBuilder.Append(FocusSizeY + ",");
		stringBuilder.Append(SecondaryOpacity + ",");
		stringBuilder.Append(Color.ToArgb() + ";");
		stringBuilder.Append(ForeColor.ToArgb() + ";");
		stringBuilder.Append(ReColorTarget.ToArgb() + ";");
		if (FillRectangle != null)
		{
			stringBuilder.Append(FillRectangle.GetAsString());
		}
		if (GradientFill != null)
		{
			stringBuilder.Append(GradientFill.GetAsString());
		}
		if (BlipFormat != null)
		{
			stringBuilder.Append(BlipFormat.GetAsString());
		}
		foreach (DictionaryEntry fillSchemeColorTransform in FillSchemeColorTransforms)
		{
			stringBuilder.Append(fillSchemeColorTransform.Value?.ToString() + ";");
		}
		return stringBuilder;
	}
}

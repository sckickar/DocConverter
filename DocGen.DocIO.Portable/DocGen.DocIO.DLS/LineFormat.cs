using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class LineFormat
{
	private Color m_BackColor;

	private LineEndLength m_BeginArrowheadLength;

	private ArrowheadStyle m_BeginArrowheadStyle;

	private LineEndWidth m_BeginArrowheadWidth;

	private LineDashing m_DashStyle;

	private LineEndLength m_EndArrowheadLength;

	private ArrowheadStyle m_EndArrowheadStyle;

	private LineEndWidth m_EndArrowheadWidth;

	private bool m_InsetPen;

	private LineStyle m_Style;

	private float m_Transparency;

	internal float m_Weight;

	internal bool m_Line;

	private DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap m_LineCap;

	private GradientFill m_GradientFill;

	private LineFormatType m_LineFormatType;

	private DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin m_LineJoin;

	private PatternType m_Pattern = PatternType.Mixed;

	private Color m_ForeColor;

	private ImageRecord m_ImageRecord;

	private string m_miterJoinLimit;

	private List<DictionaryEntry> m_lineSchemeColor;

	internal ushort m_flag;

	private ChildShape m_childShape;

	private Entity m_shape;

	internal Dictionary<string, Stream> m_docxProps;

	internal byte m_flagA;

	internal Dictionary<int, object> m_propertiesHash;

	internal const byte LineJoinKey = 0;

	internal const byte LineCapKey = 1;

	internal const byte BeginArrowheadLengthKey = 2;

	internal const byte BeginArrowheadStyleKey = 3;

	internal const byte BeginArrowheadWidthKey = 4;

	internal const byte DashStyleKey = 5;

	internal const byte EndArrowheadLengthKey = 6;

	internal const byte EndArrowheadStyleKey = 7;

	internal const byte EndArrowheadWidthKey = 8;

	internal const byte InsertPenKey = 9;

	internal const byte StyleKey = 10;

	internal const byte LineWeightKey = 11;

	internal const byte ColorKey = 12;

	internal const byte LineKey = 14;

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

	internal Color ForeColor
	{
		get
		{
			return m_ForeColor;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xDFFFu) | 0x2000u);
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

	internal DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin LineJoin
	{
		get
		{
			if (HasKeyValue(0))
			{
				return (DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin)PropertiesHash[0];
			}
			return m_LineJoin;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFEu) | 1u);
			m_LineJoin = value;
			SetKeyValue(0, value);
		}
	}

	internal LineFormatType LineFormatType
	{
		get
		{
			return m_LineFormatType;
		}
		set
		{
			m_LineFormatType = value;
		}
	}

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

	internal DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap LineCap
	{
		get
		{
			return m_LineCap;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFDu) | 2u);
			m_LineCap = value;
		}
	}

	public bool Line
	{
		get
		{
			if (HasKeyValue(14))
			{
				return (bool)PropertiesHash[14];
			}
			return m_Line;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_flag = (ushort)((m_flag & 0xDFFEu) | 0x4000u);
			m_Line = value;
			SetKeyValue(14, value);
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
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
			m_flag = (ushort)((m_flag & 0xEFFFu) | 0x1000u);
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_BackColor = value;
		}
	}

	internal LineEndLength BeginArrowheadLength
	{
		get
		{
			return m_BeginArrowheadLength;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFBu) | 4u);
			m_BeginArrowheadLength = value;
		}
	}

	public ArrowheadStyle BeginArrowheadStyle
	{
		get
		{
			return m_BeginArrowheadStyle;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFF7u) | 8u);
			m_BeginArrowheadStyle = value;
		}
	}

	internal LineEndWidth BeginArrowheadWidth
	{
		get
		{
			return m_BeginArrowheadWidth;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFEFu) | 0x10u);
			m_BeginArrowheadWidth = value;
		}
	}

	public LineDashing DashStyle
	{
		get
		{
			return m_DashStyle;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_flag = (ushort)((m_flag & 0xFFDFu) | 0x20u);
			m_DashStyle = value;
		}
	}

	internal LineEndLength EndArrowheadLength
	{
		get
		{
			return m_EndArrowheadLength;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFBFu) | 0x40u);
			m_EndArrowheadLength = value;
		}
	}

	public ArrowheadStyle EndArrowheadStyle
	{
		get
		{
			return m_EndArrowheadStyle;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFF7Fu) | 0x80u);
			m_EndArrowheadStyle = value;
		}
	}

	internal LineEndWidth EndArrowheadWidth
	{
		get
		{
			return m_EndArrowheadWidth;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFEFFu) | 0x100u);
			m_EndArrowheadWidth = value;
		}
	}

	internal bool InsetPen
	{
		get
		{
			return m_InsetPen;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFDFFu) | 0x200u);
			m_InsetPen = value;
		}
	}

	public LineStyle Style
	{
		get
		{
			return m_Style;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFBFFu) | 0x400u);
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_Style = value;
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
			m_Transparency = value;
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_Transparency = value;
		}
	}

	public float Weight
	{
		get
		{
			if (HasKeyValue(11))
			{
				return (float)PropertiesHash[11];
			}
			return m_Weight;
		}
		set
		{
			if (m_shape != null && m_shape.Document != null && !m_shape.Document.IsOpening)
			{
				if (m_shape is Shape)
				{
					(m_shape as Shape).IsLineStyleInline = true;
				}
				else if (m_shape is GroupShape)
				{
					(m_shape as GroupShape).IsLineStyleInline = true;
				}
			}
			else if (m_childShape != null && m_childShape.Document != null && !m_childShape.Document.IsOpening)
			{
				m_childShape.IsLineStyleInline = true;
			}
			m_flag = (ushort)((m_flag & 0xF7FFu) | 0x800u);
			m_Weight = value;
			SetKeyValue(11, value);
		}
	}

	internal bool Is2007StrokeDefined
	{
		get
		{
			return (m_flagA & 1) != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsInlineLineWeightNull
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

	internal string MiterJoinLimit
	{
		get
		{
			return m_miterJoinLimit;
		}
		set
		{
			m_miterJoinLimit = value;
		}
	}

	internal List<DictionaryEntry> LineSchemeColorTransforms
	{
		get
		{
			if (m_lineSchemeColor == null)
			{
				m_lineSchemeColor = new List<DictionaryEntry>();
			}
			return m_lineSchemeColor;
		}
		set
		{
			m_lineSchemeColor = value;
		}
	}

	public LineFormat(Shape shape)
		: this((ShapeBase)shape)
	{
	}

	internal LineFormat(ShapeBase shape)
	{
		m_shape = shape;
		m_BackColor = Color.Black;
		m_DashStyle = LineDashing.Solid;
		m_Line = true;
		m_Style = LineStyle.Single;
		m_Transparency = 0f;
		m_Weight = 1f;
		m_LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Miter;
		m_LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat;
		m_BeginArrowheadLength = LineEndLength.MediumLenArrow;
		m_BeginArrowheadWidth = LineEndWidth.MediumWidthArrow;
		m_EndArrowheadLength = LineEndLength.MediumLenArrow;
		m_EndArrowheadWidth = LineEndWidth.MediumWidthArrow;
		LineFormatChanged();
	}

	internal LineFormat(ChildShape shape)
	{
		m_childShape = shape;
		m_BackColor = Color.Black;
		m_DashStyle = LineDashing.Solid;
		m_Line = true;
		m_Style = LineStyle.Single;
		m_Transparency = 0f;
		m_Weight = 1f;
		m_LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Miter;
		m_LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat;
		m_BeginArrowheadLength = LineEndLength.MediumLenArrow;
		m_BeginArrowheadWidth = LineEndWidth.MediumWidthArrow;
		m_EndArrowheadLength = LineEndLength.MediumLenArrow;
		m_EndArrowheadWidth = LineEndWidth.MediumWidthArrow;
		LineFormatChanged();
	}

	private void LineFormatChanged()
	{
		if (DocxProps.ContainsKey("gradFill"))
		{
			DocxProps.Remove("gradFill");
		}
		if (DocxProps.ContainsKey("pattFill"))
		{
			DocxProps.Remove("pattFill");
		}
		if (m_shape is Shape)
		{
			if (m_shape is Shape && (m_shape as Shape).Docx2007Props.ContainsKey("stroke"))
			{
				(m_shape as Shape).Docx2007Props.Remove("stroke");
			}
			m_Line = true;
		}
	}

	internal bool HasKeyValue(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_GradientFill != null)
		{
			m_GradientFill.Close();
			m_GradientFill = null;
		}
		if (m_ImageRecord != null)
		{
			m_ImageRecord.Close();
			m_ImageRecord = null;
		}
		if (m_lineSchemeColor != null)
		{
			m_lineSchemeColor.Clear();
			m_lineSchemeColor = null;
		}
		if (m_docxProps == null)
		{
			return;
		}
		foreach (Stream value in m_docxProps.Values)
		{
			value.Close();
		}
		m_docxProps.Clear();
		m_docxProps = null;
	}

	internal bool HasKey(int propertyKey)
	{
		return (m_flag & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	internal LineFormat Clone()
	{
		LineFormat lineFormat = (LineFormat)MemberwiseClone();
		if (GradientFill != null && GradientFill.GradientStops != null && GradientFill.GradientStops.Count > 0)
		{
			lineFormat.GradientFill = GradientFill.Clone();
		}
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			((m_shape != null) ? m_shape.Document : ((m_childShape != null) ? m_childShape.Document : null))?.CloneProperties(DocxProps, ref lineFormat.m_docxProps);
		}
		return lineFormat;
	}

	internal bool Compare(LineFormat lineFormat)
	{
		if (Line != lineFormat.Line || InsetPen != lineFormat.InsetPen || Is2007StrokeDefined != lineFormat.Is2007StrokeDefined || Pattern != lineFormat.Pattern || LineJoin != lineFormat.LineJoin || LineFormatType != lineFormat.LineFormatType || LineCap != lineFormat.LineCap || BeginArrowheadLength != lineFormat.BeginArrowheadLength || BeginArrowheadStyle != lineFormat.BeginArrowheadStyle || BeginArrowheadWidth != lineFormat.BeginArrowheadWidth || DashStyle != lineFormat.DashStyle || EndArrowheadLength != lineFormat.EndArrowheadLength || EndArrowheadStyle != lineFormat.EndArrowheadStyle || EndArrowheadWidth != lineFormat.EndArrowheadWidth || Style != lineFormat.Style || Transparency != lineFormat.Transparency || Weight != lineFormat.Weight || MiterJoinLimit != lineFormat.MiterJoinLimit || ForeColor.ToArgb() != lineFormat.ForeColor.ToArgb() || Color.ToArgb() != lineFormat.Color.ToArgb())
		{
			return false;
		}
		if ((GradientFill == null && lineFormat.GradientFill != null) || (GradientFill != null && lineFormat.GradientFill == null) || (ImageRecord == null && lineFormat.ImageRecord != null) || (ImageRecord != null && lineFormat.ImageRecord == null) || (LineSchemeColorTransforms == null && lineFormat.LineSchemeColorTransforms != null) || (LineSchemeColorTransforms != null && lineFormat.LineSchemeColorTransforms == null))
		{
			return false;
		}
		if (GradientFill != null && !GradientFill.Compare(lineFormat.GradientFill))
		{
			return false;
		}
		if (ImageRecord != null && ImageRecord.comparedImageName != lineFormat.ImageRecord.comparedImageName)
		{
			return false;
		}
		if (LineSchemeColorTransforms != null && lineFormat.LineSchemeColorTransforms != null)
		{
			if (LineSchemeColorTransforms.Count != lineFormat.LineSchemeColorTransforms.Count)
			{
				return false;
			}
			for (int i = 0; i < LineSchemeColorTransforms.Count; i++)
			{
				if (LineSchemeColorTransforms[i].Key != lineFormat.LineSchemeColorTransforms[i].Key || LineSchemeColorTransforms[i].Value != lineFormat.LineSchemeColorTransforms[i].Value)
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
		string text = (Line ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (InsetPen ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Is2007StrokeDefined ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)Pattern + ";");
		stringBuilder.Append((int)LineJoin + ";");
		stringBuilder.Append((int)LineFormatType + ";");
		stringBuilder.Append((int)LineCap + ";");
		stringBuilder.Append((int)BeginArrowheadLength + ";");
		stringBuilder.Append((int)BeginArrowheadStyle + ";");
		stringBuilder.Append((int)BeginArrowheadWidth + ";");
		stringBuilder.Append((int)DashStyle + ";");
		stringBuilder.Append((int)EndArrowheadLength + ";");
		stringBuilder.Append((int)EndArrowheadStyle + ";");
		stringBuilder.Append((int)EndArrowheadWidth + ";");
		stringBuilder.Append((int)Style + ";");
		stringBuilder.Append(Transparency + ";");
		stringBuilder.Append(Weight + ";");
		stringBuilder.Append(MiterJoinLimit + ";");
		stringBuilder.Append(ForeColor.ToArgb() + ";");
		stringBuilder.Append(Color.ToArgb() + ";");
		if (GradientFill != null)
		{
			stringBuilder.Append(GradientFill.GetAsString());
		}
		if (ImageRecord != null)
		{
			stringBuilder.Append(ImageRecord.comparedImageName + ";");
		}
		if (LineSchemeColorTransforms != null)
		{
			foreach (DictionaryEntry lineSchemeColorTransform in LineSchemeColorTransforms)
			{
				stringBuilder.Append(lineSchemeColorTransform.Value?.ToString() + ";");
			}
		}
		return stringBuilder;
	}
}

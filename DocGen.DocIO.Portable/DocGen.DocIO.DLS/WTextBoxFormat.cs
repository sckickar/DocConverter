using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class WTextBoxFormat : FormatBase
{
	internal const float DEF_LINE_WIDTH = 0.75f;

	internal const byte LineWidthKey = 11;

	private HorizontalOrigin m_horizRelation;

	private VerticalOrigin m_vertRelation;

	private WidthOrigin m_widthRelation;

	private HeightOrigin m_heightRelation;

	private float m_width;

	private float m_height;

	private Color m_fillColor;

	private Color m_lineColor;

	private TextBoxLineStyle m_lineStyle;

	private TextWrappingStyle m_wrapStyle;

	private float m_wrapDistanceBottom;

	private float m_wrapDistanceLeft = 9f;

	private float m_wrapDistanceRight = 9f;

	private float m_wrapDistanceTop;

	private float m_horPosition;

	private float m_verPosition;

	private int m_spid;

	internal float m_txbxLineWidth;

	private LineDashing m_lineDashing;

	private TextWrappingType m_wrappingType;

	private WrapMode m_wrapMode;

	private float m_txID;

	private byte m_bFlags = 72;

	private ShapeHorizontalAlignment m_horizAlignment;

	private ShapeVerticalAlignment m_verticalAlignment;

	private VerticalAlignment m_textVerticalAlignment;

	private InternalMargin m_intMargin;

	private Background m_background;

	private int m_orderIndex = int.MaxValue;

	private List<string> m_styleProps;

	private float m_widthRelPercent;

	private float m_heightRelPercent;

	private float m_horRelPercent = float.MinValue;

	private float m_verRelPercent = float.MinValue;

	private TextDirection m_textDirection;

	private Color m_textThemeColor;

	internal short WrapCollectionIndex = -1;

	private WrapPolygon m_wrapPolygon;

	private List<Stream> m_docxProps;

	private string m_name;

	private float m_rotation;

	private byte m_bflag;

	private string m_path;

	private float m_coordinateXOrigin;

	private float m_coordinateYOrigin;

	private string m_coordinateSize;

	private List<Path2D> m_vmlPathPoints;

	internal bool m_isVMLPathUpdated;

	internal bool IsWrappingBoundsAdded
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public WidthOrigin WidthOrigin
	{
		get
		{
			return m_widthRelation;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.WidthOrigin = value;
			}
			m_widthRelation = value;
		}
	}

	public HeightOrigin HeightOrigin
	{
		get
		{
			return m_heightRelation;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.HeightOrigin = value;
			}
			m_heightRelation = value;
		}
	}

	public HorizontalOrigin HorizontalOrigin
	{
		get
		{
			return m_horizRelation;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.HorizontalOrigin = value;
			}
			m_horizRelation = value;
		}
	}

	public VerticalOrigin VerticalOrigin
	{
		get
		{
			return m_vertRelation;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.VerticalOrigin = value;
			}
			m_vertRelation = value;
		}
	}

	public TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return m_wrapStyle;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.WrapFormat.TextWrappingStyle = value;
			}
			m_wrapStyle = value;
			if (m_wrapStyle == TextWrappingStyle.Behind)
			{
				m_bFlags = (byte)((m_bFlags & 0xFEu) | 1u);
			}
			else
			{
				m_bFlags = (byte)((m_bFlags & 0xFEu) | 0u);
			}
			if (m_wrapStyle == TextWrappingStyle.Inline)
			{
				m_bFlags = (byte)((m_bFlags & 0xBFu) | 0x40u);
			}
		}
	}

	internal float WrapDistanceBottom
	{
		get
		{
			if (m_wrapDistanceBottom < 0f || m_wrapDistanceBottom > 1584f)
			{
				return 0f;
			}
			return m_wrapDistanceBottom;
		}
		set
		{
			m_wrapDistanceBottom = value;
		}
	}

	internal float WrapDistanceLeft
	{
		get
		{
			if (m_wrapDistanceLeft < 0f || m_wrapDistanceLeft > 1584f)
			{
				return 0f;
			}
			return m_wrapDistanceLeft;
		}
		set
		{
			m_wrapDistanceLeft = value;
		}
	}

	internal float WrapDistanceRight
	{
		get
		{
			if (m_wrapDistanceRight < 0f || m_wrapDistanceRight > 1584f)
			{
				return 0f;
			}
			return m_wrapDistanceRight;
		}
		set
		{
			m_wrapDistanceRight = value;
		}
	}

	internal float WrapDistanceTop
	{
		get
		{
			if (m_wrapDistanceTop < 0f || m_wrapDistanceTop > 1584f)
			{
				return 0f;
			}
			return m_wrapDistanceTop;
		}
		set
		{
			m_wrapDistanceTop = value;
		}
	}

	public Color FillColor
	{
		get
		{
			if (m_background != null)
			{
				return m_background.Color;
			}
			return Color.White;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.FillFormat.Color = value;
			}
			FillEfects.Color = value;
			FillEfects.Type = BackgroundType.Color;
		}
	}

	public TextBoxLineStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				switch (value)
				{
				case TextBoxLineStyle.Double:
					(base.OwnerBase as WTextBox).Shape.LineFormat.Style = DocGen.DocIO.DLS.LineStyle.ThinThin;
					break;
				case TextBoxLineStyle.Simple:
					(base.OwnerBase as WTextBox).Shape.LineFormat.Style = DocGen.DocIO.DLS.LineStyle.Single;
					break;
				case TextBoxLineStyle.ThickThin:
					(base.OwnerBase as WTextBox).Shape.LineFormat.Style = DocGen.DocIO.DLS.LineStyle.ThickThin;
					break;
				case TextBoxLineStyle.ThinThick:
					(base.OwnerBase as WTextBox).Shape.LineFormat.Style = DocGen.DocIO.DLS.LineStyle.ThinThick;
					break;
				case TextBoxLineStyle.Triple:
					(base.OwnerBase as WTextBox).Shape.LineFormat.Style = DocGen.DocIO.DLS.LineStyle.ThickBetweenThin;
					break;
				}
			}
			m_lineStyle = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.Width = value;
			}
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.Height = value;
			}
			m_height = value;
		}
	}

	public Color LineColor
	{
		get
		{
			return m_lineColor;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.LineFormat.Color = value;
			}
			m_lineColor = value;
		}
	}

	public bool NoLine
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.LineFormat.Line = !value;
			}
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal WrapMode WrappingMode
	{
		get
		{
			return m_wrapMode;
		}
		set
		{
			m_wrapMode = value;
		}
	}

	public float HorizontalPosition
	{
		get
		{
			return m_horPosition;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.HorizontalPosition = value;
			}
			m_horPosition = value;
		}
	}

	internal bool IsBelowText
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			if (value && TextWrappingStyle == TextWrappingStyle.InFrontOfText)
			{
				m_wrapStyle = TextWrappingStyle.Behind;
			}
			else if (!value && TextWrappingStyle == TextWrappingStyle.Behind)
			{
				m_wrapStyle = TextWrappingStyle.InFrontOfText;
			}
		}
	}

	public float VerticalPosition
	{
		get
		{
			return m_verPosition;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.VerticalPosition = value;
			}
			m_verPosition = value;
		}
	}

	public TextWrappingType TextWrappingType
	{
		get
		{
			return m_wrappingType;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.WrapFormat.TextWrappingType = value;
			}
			m_wrappingType = value;
		}
	}

	internal int TextBoxShapeID
	{
		get
		{
			return m_spid;
		}
		set
		{
			m_spid = value;
		}
	}

	public float LineWidth
	{
		get
		{
			if (HasKeyValue(11))
			{
				return (float)base.PropertiesHash[11];
			}
			return m_txbxLineWidth;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.LineFormat.Weight = value;
			}
			m_txbxLineWidth = value;
			SetKeyValue(11, value);
		}
	}

	public LineDashing LineDashing
	{
		get
		{
			return m_lineDashing;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.LineFormat.DashStyle = value;
			}
			m_lineDashing = value;
		}
	}

	public ShapeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horizAlignment;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.HorizontalAlignment = value;
			}
			m_horizAlignment = value;
		}
	}

	public ShapeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.VerticalAlignment = value;
			}
			m_verticalAlignment = value;
		}
	}

	public VerticalAlignment TextVerticalAlignment
	{
		get
		{
			return m_textVerticalAlignment;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.TextVerticalAlignment = value;
			}
			m_textVerticalAlignment = value;
		}
	}

	internal float TextBoxIdentificator
	{
		get
		{
			return m_txID;
		}
		set
		{
			m_txID = value;
		}
	}

	internal bool IsHeaderTextBox
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

	public InternalMargin InternalMargin
	{
		get
		{
			if (m_intMargin == null)
			{
				m_intMargin = new InternalMargin();
				m_intMargin.SetOwner(base.OwnerBase);
			}
			return m_intMargin;
		}
	}

	public float Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.Rotation = value;
			}
			m_rotation = value;
		}
	}

	public bool FlipHorizontal
	{
		get
		{
			return (m_bflag & 0x10) >> 4 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.FlipHorizontal = value;
			}
			m_bflag = (byte)((m_bflag & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	public bool FlipVertical
	{
		get
		{
			return (m_bflag & 0x20) >> 5 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.FlipVertical = value;
			}
			m_bflag = (byte)((m_bflag & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public Background FillEfects
	{
		get
		{
			if (m_background == null)
			{
				m_background = new Background(base.Document, BackgroundType.NoBackground);
			}
			return m_background;
		}
	}

	internal bool AllowInCell
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal int OrderIndex
	{
		get
		{
			if (m_orderIndex == int.MaxValue && base.Document != null && !base.Document.IsOpening && base.Document.Escher != null)
			{
				int shapeOrderIndex = base.Document.Escher.GetShapeOrderIndex(TextBoxShapeID);
				if (shapeOrderIndex != -1)
				{
					m_orderIndex = shapeOrderIndex;
				}
			}
			return m_orderIndex;
		}
		set
		{
			m_orderIndex = value;
		}
	}

	internal List<string> DocxStyleProps
	{
		get
		{
			if (m_styleProps == null)
			{
				m_styleProps = new List<string>();
			}
			return m_styleProps;
		}
	}

	internal bool HasDocxProps => m_styleProps != null;

	public bool AutoFit
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				if (value)
				{
					(base.OwnerBase as WTextBox).Shape.TextFrame.ShapeAutoFit = true;
					(base.OwnerBase as WTextBox).Shape.TextFrame.NoAutoFit = false;
					(base.OwnerBase as WTextBox).Shape.TextFrame.NormalAutoFit = false;
				}
				else
				{
					(base.OwnerBase as WTextBox).Shape.TextFrame.NoAutoFit = true;
					(base.OwnerBase as WTextBox).Shape.TextFrame.ShapeAutoFit = false;
					(base.OwnerBase as WTextBox).Shape.TextFrame.NormalAutoFit = false;
				}
			}
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal float WidthRelativePercent
	{
		get
		{
			return m_widthRelPercent;
		}
		set
		{
			m_widthRelPercent = value;
		}
	}

	internal float HeightRelativePercent
	{
		get
		{
			return m_heightRelPercent;
		}
		set
		{
			m_heightRelPercent = value;
		}
	}

	internal float HorizontalRelativePercent
	{
		get
		{
			return m_horRelPercent;
		}
		set
		{
			m_horRelPercent = value;
		}
	}

	internal float VerticalRelativePercent
	{
		get
		{
			return m_verRelPercent;
		}
		set
		{
			m_verRelPercent = value;
		}
	}

	public TextDirection TextDirection
	{
		get
		{
			return m_textDirection;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.TextDirection = value;
			}
			m_textDirection = value;
		}
	}

	internal Color TextThemeColor
	{
		get
		{
			return m_textThemeColor;
		}
		set
		{
			m_textThemeColor = value;
		}
	}

	public bool AllowOverlap
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			if (TextWrappingStyle != 0)
			{
				if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
				{
					(base.OwnerBase as WTextBox).Shape.WrapFormat.AllowOverlap = value;
				}
				m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
			}
		}
	}

	internal WrapPolygon WrapPolygon
	{
		get
		{
			if (m_wrapPolygon == null)
			{
				m_wrapPolygon = new WrapPolygon();
				m_wrapPolygon.Edited = false;
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
			}
			return m_wrapPolygon;
		}
		set
		{
			m_wrapPolygon = value;
		}
	}

	internal List<Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new List<Stream>();
			}
			return m_docxProps;
		}
	}

	internal string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	internal string CoordinateSize
	{
		get
		{
			return m_coordinateSize;
		}
		set
		{
			m_coordinateSize = value;
		}
	}

	internal float CoordinateXOrigin
	{
		get
		{
			return m_coordinateXOrigin;
		}
		set
		{
			m_coordinateXOrigin = value;
		}
	}

	internal float CoordinateYOrigin
	{
		get
		{
			return m_coordinateYOrigin;
		}
		set
		{
			m_coordinateYOrigin = value;
		}
	}

	internal List<Path2D> VMLPathPoints
	{
		get
		{
			return m_vmlPathPoints;
		}
		set
		{
			m_vmlPathPoints = value;
		}
	}

	public WTextBoxFormat(WordDocument doc)
		: base(doc, isTextBox: true)
	{
		m_wrapStyle = TextWrappingStyle.InFrontOfText;
		m_fillColor = Color.White;
		m_lineColor = Color.Black;
		m_lineStyle = TextBoxLineStyle.Simple;
		m_horizRelation = HorizontalOrigin.Column;
		m_vertRelation = VerticalOrigin.Paragraph;
		m_txbxLineWidth = 0.75f;
		m_lineDashing = LineDashing.Solid;
		m_wrapMode = WrapMode.Square;
		m_horizAlignment = ShapeHorizontalAlignment.None;
		m_verticalAlignment = ShapeVerticalAlignment.None;
		m_textVerticalAlignment = DocGen.DocIO.DLS.VerticalAlignment.Top;
		m_background = new Background(doc, BackgroundType.NoBackground);
	}

	public override void ClearFormatting()
	{
		SetTextWrappingStyleValue(TextWrappingStyle.InFrontOfText);
		FillColor = Color.White;
		LineColor = Color.Black;
		LineStyle = TextBoxLineStyle.Simple;
		HorizontalOrigin = HorizontalOrigin.Column;
		VerticalOrigin = VerticalOrigin.Paragraph;
		LineWidth = 0.75f;
		LineDashing = LineDashing.Solid;
		WrappingMode = WrapMode.Square;
		HorizontalAlignment = ShapeHorizontalAlignment.None;
		VerticalAlignment = ShapeVerticalAlignment.None;
		TextVerticalAlignment = DocGen.DocIO.DLS.VerticalAlignment.Top;
		m_background = new Background(base.Document, BackgroundType.NoBackground);
		if (DocxProps != null && DocxProps.Count > 0)
		{
			DocxProps.Clear();
		}
		if (DocxStyleProps != null && DocxStyleProps.Count > 0)
		{
			DocxStyleProps.Clear();
		}
		if (base.OwnerBase != null && base.OwnerBase is WTextBox)
		{
			(base.OwnerBase as WTextBox).IsShape = false;
		}
	}

	protected override object GetDefValue(int key)
	{
		return null;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("FillColor"))
		{
			FillColor = reader.ReadColor("FillColor");
		}
		if (reader.HasAttribute("Height"))
		{
			Height = reader.ReadFloat("Height");
		}
		if (reader.HasAttribute("HorizontalOrigin"))
		{
			HorizontalOrigin = (HorizontalOrigin)(object)reader.ReadEnum("HorizontalOrigin", typeof(HorizontalOrigin));
		}
		if (reader.HasAttribute("LineStyle"))
		{
			LineStyle = (TextBoxLineStyle)(object)reader.ReadEnum("LineStyle", typeof(TextBoxLineStyle));
		}
		if (reader.HasAttribute("WrappingStyle"))
		{
			SetTextWrappingStyleValue((TextWrappingStyle)(object)reader.ReadEnum("WrappingStyle", typeof(TextWrappingStyle)));
		}
		if (reader.HasAttribute("VerticalOrigin"))
		{
			VerticalOrigin = (VerticalOrigin)(object)reader.ReadEnum("VerticalOrigin", typeof(VerticalOrigin));
		}
		if (reader.HasAttribute("Width"))
		{
			Width = reader.ReadFloat("Width");
		}
		if (reader.HasAttribute("LineColor"))
		{
			LineColor = reader.ReadColor("LineColor");
		}
		if (reader.HasAttribute("HorizontalPosition"))
		{
			HorizontalPosition = reader.ReadFloat("HorizontalPosition");
		}
		if (reader.HasAttribute("LineDashing"))
		{
			LineDashing = (LineDashing)(object)reader.ReadEnum("LineDashing", typeof(LineDashing));
		}
		if (reader.HasAttribute("LineWidth"))
		{
			LineWidth = reader.ReadFloat("LineWidth");
		}
		if (reader.HasAttribute("VerticalPosition"))
		{
			VerticalPosition = reader.ReadFloat("VerticalPosition");
		}
		if (reader.HasAttribute("WrappingMode"))
		{
			WrappingMode = (WrapMode)(object)reader.ReadEnum("WrappingMode", typeof(WrapMode));
		}
		if (reader.HasAttribute("WrappingType"))
		{
			TextWrappingType = (TextWrappingType)(object)reader.ReadEnum("WrappingType", typeof(TextWrappingType));
		}
		if (reader.HasAttribute("IsBelowText"))
		{
			IsBelowText = reader.ReadBoolean("IsBelowText");
		}
		if (reader.HasAttribute("NoLine"))
		{
			NoLine = reader.ReadBoolean("NoLine");
		}
		if (reader.HasAttribute("NoFill"))
		{
			FillColor = Color.Empty;
		}
		if (reader.HasAttribute("HorizontalAlignment"))
		{
			HorizontalAlignment = (ShapeHorizontalAlignment)(object)reader.ReadEnum("HorizontalAlignment", typeof(ShapeHorizontalAlignment));
		}
		if (reader.HasAttribute("VerticalAlignment"))
		{
			VerticalAlignment = (ShapeVerticalAlignment)(object)reader.ReadEnum("VerticalAlignment", typeof(ShapeVerticalAlignment));
		}
		if (reader.HasAttribute("ShapeID"))
		{
			TextBoxShapeID = reader.ReadInt("ShapeID");
		}
		if (reader.HasAttribute("IsHeader"))
		{
			IsHeaderTextBox = reader.ReadBoolean("IsHeader");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (FillColor == Color.Empty)
		{
			writer.WriteValue("NoFill", value: true);
		}
		else if (FillColor != Color.White)
		{
			writer.WriteValue("FillColor", FillColor);
		}
		if (Height != 0f)
		{
			writer.WriteValue("Height", Height);
		}
		if (HorizontalOrigin != HorizontalOrigin.Column)
		{
			writer.WriteValue("HorizontalOrigin", HorizontalOrigin);
		}
		if (LineStyle != 0)
		{
			writer.WriteValue("LineStyle", LineStyle);
		}
		if (TextWrappingStyle != TextWrappingStyle.Square)
		{
			writer.WriteValue("WrappingStyle", TextWrappingStyle);
		}
		if (VerticalOrigin != VerticalOrigin.Paragraph)
		{
			writer.WriteValue("VerticalOrigin", VerticalOrigin);
		}
		if (Width != 0f)
		{
			writer.WriteValue("Width", Width);
		}
		if (LineColor != Color.Black)
		{
			writer.WriteValue("LineColor", LineColor);
		}
		if (HorizontalPosition != 0f)
		{
			writer.WriteValue("HorizontalPosition", HorizontalPosition);
		}
		if (LineDashing != 0)
		{
			writer.WriteValue("LineDashing", LineDashing);
		}
		if (LineWidth != 0.75f)
		{
			writer.WriteValue("LineWidth", LineWidth);
		}
		if (VerticalPosition != 0f)
		{
			writer.WriteValue("VerticalPosition", VerticalPosition);
		}
		if (WrappingMode != WrapMode.None)
		{
			writer.WriteValue("WrappingMode", WrappingMode);
		}
		if (TextWrappingType != 0)
		{
			writer.WriteValue("WrappingType", TextWrappingType);
		}
		if (IsBelowText)
		{
			writer.WriteValue("IsBelowText", IsBelowText);
		}
		if (NoLine)
		{
			writer.WriteValue("NoLine", NoLine);
		}
		if (HorizontalAlignment != 0)
		{
			writer.WriteValue("HorizontalAlignment", HorizontalAlignment);
		}
		if (VerticalAlignment != 0)
		{
			writer.WriteValue("VerticalAlignment", VerticalAlignment);
		}
		if (TextBoxShapeID != 0)
		{
			writer.WriteValue("ShapeID", TextBoxShapeID);
		}
		if (IsHeaderTextBox)
		{
			writer.WriteValue("IsHeader", IsHeaderTextBox);
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
		base[propKey] = value;
	}

	internal override void Close()
	{
		base.Close();
		if (m_background != null)
		{
			m_background.Close();
			m_background = null;
		}
		if (m_styleProps != null)
		{
			m_styleProps.Clear();
			m_styleProps = null;
		}
		if (m_docxProps == null)
		{
			return;
		}
		foreach (Stream docxProp in m_docxProps)
		{
			docxProp.Close();
		}
		m_docxProps.Clear();
		m_docxProps = null;
	}

	public WTextBoxFormat Clone()
	{
		WTextBoxFormat wTextBoxFormat = (WTextBoxFormat)MemberwiseClone();
		if (WrapPolygon != null)
		{
			wTextBoxFormat.WrapPolygon = WrapPolygon.Clone();
		}
		if (m_intMargin != null)
		{
			wTextBoxFormat.m_intMargin = m_intMargin.Clone();
		}
		if (m_background != null)
		{
			wTextBoxFormat.m_background = m_background.Clone();
		}
		return wTextBoxFormat;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (m_background != null)
		{
			m_background.UpdateImageRecord(doc);
		}
	}

	internal bool Compare(WTextBoxFormat textboxFormat)
	{
		if (NoLine != textboxFormat.NoLine || IsBelowText != textboxFormat.IsBelowText || IsHeaderTextBox != textboxFormat.IsHeaderTextBox || FlipHorizontal != textboxFormat.FlipHorizontal || FlipVertical != textboxFormat.FlipVertical || AllowInCell != textboxFormat.AllowInCell || HasDocxProps != textboxFormat.HasDocxProps || AutoFit != textboxFormat.AutoFit || AllowOverlap != textboxFormat.AllowOverlap || WidthOrigin != textboxFormat.WidthOrigin || HeightOrigin != textboxFormat.HeightOrigin || HorizontalOrigin != textboxFormat.HorizontalOrigin || VerticalOrigin != textboxFormat.VerticalOrigin || TextWrappingStyle != textboxFormat.TextWrappingStyle || WrapDistanceBottom != textboxFormat.WrapDistanceBottom || WrapDistanceLeft != textboxFormat.WrapDistanceLeft || WrapDistanceRight != textboxFormat.WrapDistanceRight || WrapDistanceTop != textboxFormat.WrapDistanceTop || LineStyle != textboxFormat.LineStyle || Width != textboxFormat.Width || Height != textboxFormat.Height || WrappingMode != textboxFormat.WrappingMode || HorizontalPosition != textboxFormat.HorizontalPosition || VerticalPosition != textboxFormat.VerticalPosition || TextWrappingType != textboxFormat.TextWrappingType || LineWidth != textboxFormat.LineWidth || LineDashing != textboxFormat.LineDashing || HorizontalAlignment != textboxFormat.HorizontalAlignment || VerticalAlignment != textboxFormat.VerticalAlignment || TextVerticalAlignment != textboxFormat.TextVerticalAlignment || Rotation != textboxFormat.Rotation || OrderIndex != textboxFormat.OrderIndex || WidthRelativePercent != textboxFormat.WidthRelativePercent || HeightRelativePercent != textboxFormat.HeightRelativePercent || HorizontalRelativePercent != textboxFormat.HorizontalRelativePercent || VerticalRelativePercent != textboxFormat.VerticalRelativePercent || TextDirection != textboxFormat.TextDirection || CoordinateXOrigin != textboxFormat.CoordinateXOrigin || CoordinateYOrigin != textboxFormat.CoordinateYOrigin || Path != textboxFormat.Path || CoordinateSize != textboxFormat.CoordinateSize || FillColor.ToArgb() != textboxFormat.FillColor.ToArgb() || LineColor.ToArgb() != textboxFormat.LineColor.ToArgb() || TextThemeColor.ToArgb() != textboxFormat.TextThemeColor.ToArgb())
		{
			return false;
		}
		if ((WrapPolygon == null && textboxFormat.WrapPolygon != null) || (WrapPolygon != null && textboxFormat.WrapPolygon == null) || (InternalMargin == null && textboxFormat.InternalMargin != null) || (InternalMargin != null && textboxFormat.InternalMargin == null) || (FillEfects == null && textboxFormat.FillEfects != null) || (FillEfects != null && textboxFormat.FillEfects == null) || (VMLPathPoints == null && textboxFormat.VMLPathPoints != null) || (VMLPathPoints != null && textboxFormat.VMLPathPoints == null) || (DocxStyleProps == null && textboxFormat.DocxStyleProps != null) || (DocxStyleProps != null && textboxFormat.DocxStyleProps == null))
		{
			return false;
		}
		if (WrapPolygon != null && !WrapPolygon.Compare(textboxFormat.WrapPolygon))
		{
			return false;
		}
		if (InternalMargin != null && !InternalMargin.Compare(textboxFormat.InternalMargin))
		{
			return false;
		}
		if (FillEfects != null && !FillEfects.Compare(textboxFormat.FillEfects))
		{
			return false;
		}
		if (VMLPathPoints != null && textboxFormat.VMLPathPoints.Count == VMLPathPoints.Count)
		{
			for (int i = 0; i < VMLPathPoints.Count; i++)
			{
				if (!VMLPathPoints[i].Compare(textboxFormat.VMLPathPoints[i]))
				{
					return false;
				}
			}
			if (DocxStyleProps != null && textboxFormat.DocxStyleProps.Count == DocxStyleProps.Count)
			{
				for (int j = 0; j < DocxStyleProps.Count; j++)
				{
					if (DocxStyleProps[j] != textboxFormat.DocxStyleProps[j])
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (NoLine ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsBelowText ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsHeaderTextBox ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipHorizontal ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipVertical ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AllowInCell ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasDocxProps ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AutoFit ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AllowOverlap ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(WidthOrigin.ToString() + ";");
		stringBuilder.Append(HeightOrigin.ToString() + ";");
		stringBuilder.Append(HorizontalOrigin.ToString() + ";");
		stringBuilder.Append(VerticalOrigin.ToString() + ";");
		stringBuilder.Append(TextWrappingStyle.ToString() + ";");
		stringBuilder.Append(WrapDistanceBottom + ";");
		stringBuilder.Append(WrapDistanceLeft + ";");
		stringBuilder.Append(WrapDistanceRight + ";");
		stringBuilder.Append(WrapDistanceTop + ";");
		stringBuilder.Append(LineStyle.ToString() + ";");
		stringBuilder.Append(Width + ";");
		stringBuilder.Append(Height + ";");
		stringBuilder.Append(WrappingMode.ToString() + ";");
		stringBuilder.Append(HorizontalPosition + ";");
		stringBuilder.Append(VerticalPosition + ";");
		stringBuilder.Append(TextWrappingType.ToString() + ";");
		stringBuilder.Append(LineWidth + ";");
		stringBuilder.Append(LineDashing.ToString() + ";");
		stringBuilder.Append(HorizontalAlignment.ToString() + ";");
		stringBuilder.Append(VerticalAlignment.ToString() + ";");
		stringBuilder.Append(TextVerticalAlignment.ToString() + ";");
		stringBuilder.Append(Rotation + ";");
		stringBuilder.Append(OrderIndex + ";");
		stringBuilder.Append(WidthRelativePercent + ";");
		stringBuilder.Append(HeightRelativePercent + ";");
		stringBuilder.Append(HorizontalRelativePercent + ";");
		stringBuilder.Append(VerticalRelativePercent + ";");
		stringBuilder.Append(TextDirection.ToString() + ";");
		stringBuilder.Append(CoordinateXOrigin + ";");
		stringBuilder.Append(CoordinateYOrigin + ";");
		stringBuilder.Append(Path + ";");
		stringBuilder.Append(CoordinateSize + ";");
		stringBuilder.Append(FillColor.ToArgb() + ";");
		stringBuilder.Append(LineColor.ToArgb() + ";");
		stringBuilder.Append(TextThemeColor.ToArgb() + ";");
		if (WrapPolygon != null)
		{
			stringBuilder.Append(WrapPolygon.GetAsString());
		}
		if (InternalMargin != null)
		{
			stringBuilder.Append(InternalMargin.GetAsString());
		}
		if (FillEfects != null)
		{
			stringBuilder.Append(FillEfects.GetAsString());
		}
		if (VMLPathPoints != null)
		{
			foreach (Path2D vMLPathPoint in VMLPathPoints)
			{
				stringBuilder.Append(vMLPathPoint.GetAsString());
			}
		}
		if (DocxStyleProps != null)
		{
			foreach (string docxStyleProp in DocxStyleProps)
			{
				stringBuilder.Append(docxStyleProp + ";");
			}
		}
		return stringBuilder;
	}

	internal void UpdateFillEffects(MsofbtSpContainer container, WordDocument doc)
	{
		m_background = new Background(doc, container);
	}

	internal void SetTextWrappingStyleValue(TextWrappingStyle textWrappingStyle)
	{
		m_wrapStyle = textWrappingStyle;
	}
}

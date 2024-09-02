using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;

namespace DocGen.Drawing;

internal class ShapeImplExt
{
	private string m_textlink;

	private bool m_fLocksText;

	private bool m_fPublished;

	private ClientAnchor m_clientAnchor;

	private ExcelAutoShapeType m_shapeType;

	private AutoShapeType m_autoShapeType;

	private ShapeDrawingType m_shapeDrawingType;

	private int m_shapeID;

	private TextFrame m_textframe;

	private Dictionary<string, Stream> m_preservedElements;

	private string m_decription;

	private string m_name;

	private bool m_isHidden;

	private bool m_flipVertical;

	private bool m_flipHorizontal;

	private string m_title;

	private WorksheetImpl m_worksheet;

	private WorksheetBaseImpl m_parentSheet;

	private AnchorType m_anchorType;

	private string m_macro;

	private string m_text;

	private bool m_lockText;

	private bool m_published;

	private bool m_isCreated;

	private double m_shapeRotation;

	private ShapeFillImpl m_fill;

	private ShapeLineFormatImpl m_line;

	private RelationCollection m_relations;

	private PreservationLogger m_logger;

	private Dictionary<string, string> m_shapeGuide;

	private bool m_bCustomGeometry;

	private List<Path2D> m_pathList;

	private Rectangle m_coordinates = Rectangle.Empty;

	internal Rectangle Coordinates
	{
		get
		{
			return m_coordinates;
		}
		set
		{
			m_coordinates = value;
		}
	}

	internal double Rotation
	{
		get
		{
			return m_shapeRotation;
		}
		set
		{
			m_shapeRotation = value;
		}
	}

	internal WorksheetImpl Worksheet
	{
		get
		{
			return m_worksheet;
		}
		set
		{
			m_worksheet = value;
		}
	}

	internal string Description
	{
		get
		{
			return m_decription;
		}
		set
		{
			m_decription = value;
		}
	}

	internal WorksheetBaseImpl ParentSheet
	{
		get
		{
			return m_parentSheet;
		}
		set
		{
			m_parentSheet = value;
		}
	}

	internal Dictionary<string, Stream> PreservedElements
	{
		get
		{
			if (m_preservedElements == null)
			{
				m_preservedElements = new Dictionary<string, Stream>();
			}
			return m_preservedElements;
		}
	}

	internal string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal bool IsCreated
	{
		get
		{
			return m_isCreated;
		}
		set
		{
			m_isCreated = value;
		}
	}

	public TextFrame TextFrame
	{
		get
		{
			if (m_textframe == null)
			{
				m_textframe = new TextFrame(this);
			}
			return m_textframe;
		}
	}

	public ClientAnchor ClientAnchor
	{
		get
		{
			if (m_clientAnchor == null)
			{
				m_clientAnchor = new ClientAnchor(m_worksheet);
			}
			return m_clientAnchor;
		}
	}

	public ShapeFillImpl Fill
	{
		get
		{
			if (m_fill == null)
			{
				if (m_worksheet != null)
				{
					m_fill = new ShapeFillImpl(m_worksheet.AppImplementation, m_worksheet, OfficeFillType.SolidColor, m_logger);
				}
				else if (m_parentSheet != null)
				{
					m_fill = new ShapeFillImpl(m_parentSheet.AppImplementation, m_parentSheet, OfficeFillType.SolidColor, m_logger);
				}
			}
			return m_fill;
		}
	}

	public ShapeLineFormatImpl Line
	{
		get
		{
			if (m_line == null)
			{
				if (m_worksheet != null)
				{
					m_line = new ShapeLineFormatImpl(m_worksheet.AppImplementation, m_worksheet, m_logger);
				}
				else if (m_parentSheet != null)
				{
					m_line = new ShapeLineFormatImpl(m_parentSheet.AppImplementation, m_parentSheet, m_logger);
				}
			}
			return m_line;
		}
	}

	public int ShapeID
	{
		get
		{
			return m_shapeID;
		}
		set
		{
			m_shapeID = value;
		}
	}

	public ExcelAutoShapeType ShapeType
	{
		get
		{
			return m_shapeType;
		}
		set
		{
			m_shapeType = value;
		}
	}

	public AutoShapeType AutoShapeType => m_autoShapeType;

	public string Macro
	{
		get
		{
			return m_macro;
		}
		set
		{
			m_macro = value;
		}
	}

	public string TextLink
	{
		get
		{
			return m_textlink;
		}
		set
		{
			m_textlink = value;
		}
	}

	public bool LocksText
	{
		get
		{
			return m_lockText;
		}
		set
		{
			m_lockText = value;
		}
	}

	public bool Published
	{
		get
		{
			return m_published;
		}
		set
		{
			m_published = value;
		}
	}

	public AnchorType AnchorType
	{
		get
		{
			return m_anchorType;
		}
		set
		{
			m_anchorType = value;
		}
	}

	public string Name
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

	public bool IsHidden
	{
		get
		{
			return m_isHidden;
		}
		set
		{
			m_isHidden = value;
		}
	}

	public RelationCollection Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new RelationCollection();
			}
			return m_relations;
		}
	}

	public PreservationLogger Logger => m_logger;

	public bool FlipVertical
	{
		get
		{
			return m_flipVertical;
		}
		set
		{
			m_flipVertical = value;
		}
	}

	public bool FlipHorizontal
	{
		get
		{
			return m_flipHorizontal;
		}
		set
		{
			m_flipHorizontal = value;
		}
	}

	internal Dictionary<string, string> ShapeGuide
	{
		get
		{
			if (m_shapeGuide == null)
			{
				return new Dictionary<string, string>();
			}
			return m_shapeGuide;
		}
		set
		{
			m_shapeGuide = value;
		}
	}

	internal bool IsCustomGeometry
	{
		get
		{
			return m_bCustomGeometry;
		}
		set
		{
			m_bCustomGeometry = value;
		}
	}

	internal List<Path2D> Path2DList
	{
		get
		{
			if (m_pathList == null)
			{
				return new List<Path2D>();
			}
			return m_pathList;
		}
		set
		{
			m_pathList = value;
		}
	}

	public ShapeImplExt(AutoShapeType autoShapeType, WorksheetImpl worksheetImpl)
	{
		m_worksheet = worksheetImpl;
		m_autoShapeType = autoShapeType;
		m_anchorType = AnchorType.TwoCell;
		m_logger = new PreservationLogger();
		CreateShapeType(autoShapeType);
	}

	internal ShapeImplExt(AutoShapeType autoShapeType, WorksheetBaseImpl worksheetBaseImpl)
	{
		m_parentSheet = worksheetBaseImpl;
		m_autoShapeType = autoShapeType;
		m_anchorType = AnchorType.TwoCell;
		m_logger = new PreservationLogger();
		CreateShapeType(autoShapeType);
	}

	private void CreateShapeType(AutoShapeType autoShapeType)
	{
		if ((uint)(autoShapeType - 224) <= 1u || (uint)(autoShapeType - 227) <= 7u)
		{
			m_shapeType = ExcelAutoShapeType.cxnSp;
		}
		else
		{
			m_shapeType = ExcelAutoShapeType.sp;
		}
	}

	internal ShapeImplExt Clone(ShapeImpl parent)
	{
		ShapeImplExt shapeImplExt = (ShapeImplExt)MemberwiseClone();
		if (m_fill != null)
		{
			shapeImplExt.m_fill = m_fill.Clone(parent);
		}
		if (m_line != null)
		{
			shapeImplExt.m_line = m_line.Clone(parent);
		}
		if (m_relations != null)
		{
			shapeImplExt.m_relations = m_relations.Clone();
		}
		if (m_textframe != null)
		{
			shapeImplExt.m_textframe = m_textframe.Clone(shapeImplExt);
		}
		return shapeImplExt;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class ShapeImpl : CommonObject, IShape, IParentApplication, IDisposable, ICloneParent, INamedObject
{
	protected static readonly Color DEF_FORE_COLOR = ColorExtension.White;

	protected static readonly Color DEF_BACK_COLOR = ColorExtension.White;

	private const int DEF_APLPHA_KNOWN_COLORS = 8;

	protected const int DEF_SIZETEXTTOFITSHAPE_FALSE_VALUE = 524296;

	protected const int DEF_SIZETEXTTOFITSHAPE_TRUE_VALUE = 655370;

	protected const int DEF_NOFILLHITTEST_VALUE = 1048592;

	public const int DEF_FULL_COLUMN_OFFSET = 1024;

	public const int DEF_FULL_ROW_OFFSET = 256;

	private const int DEF_LINE_WEIGHT = 9525;

	public const double DEF_TRANSPARENCY_MULL = 655.0;

	public const double DEF_TRANSPARENCY_MULL_100 = 65500.0;

	internal const double LineWieghtMultiplier = 12700.0;

	internal const double MAX_SHAPE_WIDTH_HEIGHT = 225408.0;

	internal const double MIN_SHAPE_WIDTH_HEIGHT = 0.0;

	private static readonly Type[] DEF_PARENT_TYPES = new Type[2]
	{
		typeof(ShapesCollection),
		typeof(WorkbookImpl)
	};

	private static readonly MsoOptions[] FillOptions = new MsoOptions[4]
	{
		MsoOptions.FillType,
		MsoOptions.ForeColor,
		MsoOptions.BackColor,
		MsoOptions.NoFillHitTest
	};

	private static readonly MsoOptions[] LineOptions = new MsoOptions[16]
	{
		MsoOptions.NoLineDrawDash,
		MsoOptions.LineStyle,
		MsoOptions.LineWeight,
		MsoOptions.LineDashStyle,
		MsoOptions.ContainRoundDot,
		MsoOptions.LineTransparency,
		MsoOptions.LineColor,
		MsoOptions.LineBackColor,
		MsoOptions.ContainLinePattern,
		MsoOptions.LinePattern,
		MsoOptions.LineStartArrow,
		MsoOptions.LineEndArrow,
		MsoOptions.StartArrowLength,
		MsoOptions.EndArrowLength,
		MsoOptions.StartArrowWidth,
		MsoOptions.EndArrowWidth
	};

	private double m_startX;

	private double m_startY;

	private double m_toX;

	private double m_toY;

	private double m_chartShapeX;

	private double m_chartShapeY;

	private double m_chartShapeWidth;

	private double m_chartShapeHeight;

	protected bool m_bSupportOptions;

	private bool m_validComment = true;

	private bool m_isLeftValueSet;

	private string m_strName = string.Empty;

	private string m_onAction = string.Empty;

	private string m_strAlternativeText = string.Empty;

	private MsoBase m_record;

	private WorkbookImpl m_book;

	private OfficeShapeType m_shapeType;

	[CLSCompliant(false)]
	protected MsofbtSp m_shape;

	private MsofbtClientAnchor m_clientAnchor;

	private string m_presetGeometry;

	protected ShapeCollectionBase m_shapes;

	private OBJRecord m_object;

	[CLSCompliant(false)]
	protected MsofbtOPT m_options;

	private RectangleF m_rectAbsolute;

	private ShapeFillImpl m_fill;

	private ShapeLineFormatImpl m_lineFormat;

	protected bool m_bUpdateLineFill = true;

	private Stream m_xmlDataStream;

	private Stream m_xmlTypeStream;

	private Relation m_imageRelation;

	private string m_strImageRelationId;

	private bool m_bUpdatePositions = true;

	private bool m_bVmlShape;

	private int m_iShapeId;

	private string m_macroName;

	private Ptg[] m_macroTokens;

	private bool m_shapeVisibility = true;

	private ShadowImpl m_shadow;

	private ThreeDFormatImpl m_3D;

	private bool m_enableAlternateContent;

	private List<ShapeImpl> m_childShapes;

	private MsofbtChildAnchor m_childAnchor;

	private MsoUnknown m_unknown;

	private Dictionary<string, string> m_styleProperties;

	private string m_preserveStyleString;

	private bool m_isHyperlink;

	private int m_shapeRotation;

	private Stream m_formulaMacroStream;

	private bool m_bHasBorder;

	internal List<Stream> preservedShapeStreams;

	internal List<Stream> preservedCnxnShapeStreams;

	internal List<Stream> preservedInnerCnxnShapeStreams;

	internal List<Stream> preservedPictureStreams;

	internal Stream m_graphicFrame;

	private bool m_bIsAbsoluteAnchor;

	private bool m_lockWithSheet = true;

	private bool m_printWithSheet = true;

	private Stream m_streamExtLst;

	internal Dictionary<string, Stream> m_preservedElements;

	private bool m_isCustomGeom;

	private Stream m_styleStream;

	private ShapeFrame m_shapeFrame;

	private ShapeFrame m_groupFrame;

	private bool m_isGroupFill;

	private bool m_isGroupLine;

	private bool m_isSlicer;

	private bool m_hasExtent;

	private bool m_bAutoSize;

	internal bool IsEquationShape;

	internal List<string> preserveStreamOrder;

	public virtual int Height
	{
		get
		{
			return (int)m_rectAbsolute.Height;
		}
		set
		{
			SetHeight(value);
		}
	}

	internal double HeightDouble
	{
		get
		{
			return m_rectAbsolute.Height;
		}
		set
		{
			SetHeight(value);
		}
	}

	public virtual int Id
	{
		get
		{
			if (m_record is MsofbtSpContainer msofbtSpContainer && msofbtSpContainer.ItemsList[0] is MsofbtSp msofbtSp)
			{
				return msofbtSp.ShapeId;
			}
			return 0;
		}
	}

	public IThreeDFormat ThreeD
	{
		get
		{
			if (m_3D == null)
			{
				m_3D = new ThreeDFormatImpl(base.Application, this);
			}
			return m_3D;
		}
	}

	internal bool ValidComment
	{
		get
		{
			return m_validComment;
		}
		set
		{
			m_validComment = value;
		}
	}

	public IShadow Shadow
	{
		get
		{
			if (m_shadow == null)
			{
				m_shadow = new ShadowImpl(base.Application, this);
			}
			return m_shadow;
		}
	}

	internal bool IsLeftValueSet
	{
		get
		{
			return m_isLeftValueSet;
		}
		set
		{
			m_isLeftValueSet = value;
		}
	}

	internal bool HasExtent
	{
		get
		{
			return m_hasExtent;
		}
		set
		{
			m_hasExtent = value;
		}
	}

	public virtual int Left
	{
		get
		{
			return (int)m_rectAbsolute.X;
		}
		set
		{
			if (!m_book.IsWorkbookOpening)
			{
				IsLeftValueSet = true;
			}
			SetLeftPosition(value);
		}
	}

	internal double LeftDouble
	{
		get
		{
			return m_rectAbsolute.X;
		}
		set
		{
			SetLeftPosition(value);
		}
	}

	internal bool EnableAlternateContent
	{
		get
		{
			return m_enableAlternateContent;
		}
		set
		{
			m_enableAlternateContent = value;
		}
	}

	public virtual string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public virtual int Top
	{
		get
		{
			return (int)m_rectAbsolute.Y;
		}
		set
		{
			SetTopPosition(value);
		}
	}

	internal double TopDouble
	{
		get
		{
			return m_rectAbsolute.Y;
		}
		set
		{
			SetTopPosition(value);
		}
	}

	public virtual int Width
	{
		get
		{
			return (int)m_rectAbsolute.Width;
		}
		set
		{
			SetWidth(value);
		}
	}

	internal double WidthDouble
	{
		get
		{
			return m_rectAbsolute.Width;
		}
		set
		{
			SetWidth(value);
		}
	}

	public OfficeShapeType ShapeType
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

	public bool IsShapeVisible
	{
		get
		{
			return m_shapeVisibility;
		}
		set
		{
			m_shapeVisibility = value;
			if (!m_book.IsWorkbookOpening && this is GroupShapeImpl)
			{
				SetInnerShapes(value, "IsShapeVisible");
			}
		}
	}

	public virtual string AlternativeText
	{
		get
		{
			return m_strAlternativeText;
		}
		set
		{
			m_strAlternativeText = value;
		}
	}

	public virtual bool IsMoveWithCell
	{
		get
		{
			return ClientAnchor.IsMoveWithCell;
		}
		set
		{
			ClientAnchor.IsMoveWithCell = value;
			if (!m_book.IsWorkbookOpening && this is GroupShapeImpl)
			{
				SetInnerShapes(value, "IsMoveWithCell");
			}
		}
	}

	public virtual bool IsSizeWithCell
	{
		get
		{
			return ClientAnchor.IsSizeWithCell;
		}
		set
		{
			ClientAnchor.IsSizeWithCell = value;
			if (!m_book.IsWorkbookOpening && this is GroupShapeImpl)
			{
				SetInnerShapes(value, "IsSizeWithCell");
			}
		}
	}

	public virtual IOfficeFill Fill
	{
		get
		{
			if (!m_bSupportOptions)
			{
				throw new NotSupportedException("This shape doesn't support fill properties.");
			}
			if (!m_bUpdateLineFill)
			{
				ParseLineFill(m_options);
			}
			else if (m_fill == null)
			{
				m_fill = new ShapeFillImpl(base.Application, this);
				if (ShapeType == OfficeShapeType.CheckBox)
				{
					m_fill.Visible = false;
				}
				if (VmlShape && ShapeType == OfficeShapeType.TextBox)
				{
					m_fill.Visible = false;
				}
			}
			return m_fill;
		}
	}

	public virtual IShapeLineFormat Line
	{
		get
		{
			if (!m_bSupportOptions)
			{
				throw new NotSupportedException("This shape doesn't support line properties.");
			}
			if (!m_bUpdateLineFill)
			{
				ParseLineFill(m_options);
			}
			else if (m_lineFormat == null)
			{
				m_lineFormat = new ShapeLineFormatImpl(base.Application, this);
			}
			return m_lineFormat;
		}
	}

	internal string PresetGeometry
	{
		get
		{
			return m_presetGeometry;
		}
		set
		{
			m_presetGeometry = value;
		}
	}

	internal string MacroName
	{
		get
		{
			return m_macroName;
		}
		set
		{
			m_macroName = value;
		}
	}

	public bool AutoSize
	{
		get
		{
			return m_bAutoSize;
		}
		set
		{
			m_bAutoSize = value;
		}
	}

	public Stream XmlDataStream
	{
		get
		{
			return m_xmlDataStream;
		}
		set
		{
			m_xmlDataStream = value;
		}
	}

	public Stream XmlTypeStream
	{
		get
		{
			return m_xmlTypeStream;
		}
		set
		{
			m_xmlTypeStream = value;
		}
	}

	public bool VmlShape
	{
		get
		{
			return m_bVmlShape;
		}
		set
		{
			m_bVmlShape = value;
		}
	}

	public string OnAction
	{
		get
		{
			return m_onAction;
		}
		set
		{
			m_onAction = value;
		}
	}

	public string ImageRelationId
	{
		get
		{
			return m_strImageRelationId;
		}
		set
		{
			m_strImageRelationId = value;
		}
	}

	public Relation ImageRelation
	{
		get
		{
			return m_imageRelation;
		}
		set
		{
			m_imageRelation = value;
		}
	}

	public virtual int ShapeRotation
	{
		get
		{
			return m_shapeRotation;
		}
		set
		{
			if (value > 3600 && value < -3600)
			{
				throw new ArgumentException("The rotation value should be between -3600 and 3600");
			}
			m_shapeRotation = value;
			m_shapeFrame.Rotation = m_shapeRotation * 60000;
		}
	}

	public virtual ITextFrame TextFrame
	{
		get
		{
			throw new NotImplementedException("This property doesn't support in this class");
		}
	}

	internal Stream FormulaMacroStream
	{
		get
		{
			return m_formulaMacroStream;
		}
		set
		{
			m_formulaMacroStream = value;
		}
	}

	internal Stream StyleStream
	{
		get
		{
			return m_styleStream;
		}
		set
		{
			m_styleStream = value;
		}
	}

	internal double ChartShapeX
	{
		get
		{
			return m_chartShapeX;
		}
		set
		{
			m_chartShapeX = value;
		}
	}

	internal double ChartShapeY
	{
		get
		{
			return m_chartShapeY;
		}
		set
		{
			m_chartShapeY = value;
		}
	}

	internal double ChartShapeWidth
	{
		get
		{
			return m_chartShapeWidth;
		}
		set
		{
			m_chartShapeWidth = value;
		}
	}

	internal double ChartShapeHeight
	{
		get
		{
			return m_chartShapeHeight;
		}
		set
		{
			m_chartShapeHeight = value;
		}
	}

	internal double StartX
	{
		get
		{
			return m_startX;
		}
		set
		{
			m_startX = value;
		}
	}

	internal double StartY
	{
		get
		{
			return m_startY;
		}
		set
		{
			m_startY = value;
		}
	}

	internal double ToX
	{
		get
		{
			return m_toX;
		}
		set
		{
			m_toX = value;
		}
	}

	internal double ToY
	{
		get
		{
			return m_toY;
		}
		set
		{
			m_toY = value;
		}
	}

	internal bool LockWithSheet
	{
		get
		{
			return m_lockWithSheet;
		}
		set
		{
			m_lockWithSheet = value;
		}
	}

	internal bool PrintWithSheet
	{
		get
		{
			return m_printWithSheet;
		}
		set
		{
			m_printWithSheet = value;
		}
	}

	internal Stream GraphicFrameStream
	{
		get
		{
			return m_graphicFrame;
		}
		set
		{
			m_graphicFrame = value;
		}
	}

	internal bool HasBorder
	{
		get
		{
			return m_bHasBorder;
		}
		set
		{
			m_bHasBorder = value;
		}
	}

	public IWorkbook Workbook => m_book;

	public WorkbookImpl ParentWorkbook => m_book;

	public ShapeCollectionBase ParentShapes => m_shapes;

	public WorksheetBaseImpl Worksheet => m_shapes.WorksheetBase;

	[CLSCompliant(false)]
	public OBJRecord Obj => m_object;

	[CLSCompliant(false)]
	public MsofbtClientAnchor ClientAnchor => m_clientAnchor;

	public virtual int TopRow
	{
		get
		{
			return ClientAnchor.TopRow + 1;
		}
		set
		{
			ClientAnchor.TopRow = value - 1;
			if (m_bUpdatePositions)
			{
				OnTopRowChanged();
			}
		}
	}

	public virtual int LeftColumn
	{
		get
		{
			return ClientAnchor.LeftColumn + 1;
		}
		set
		{
			ClientAnchor.LeftColumn = value - 1;
			if (!m_bUpdatePositions)
			{
				return;
			}
			OnLeftColumnChange();
			if (m_shapes.Worksheet != null)
			{
				if (IsLeftValueSet && Left > 0)
				{
					LeftColumnOffset = ConvertPixelsIntoWidthOffset(Left, m_shapes.Worksheet.GetColumnWidthInPixels(value));
				}
				else
				{
					LeftColumnOffset = 0;
				}
				EvaluateLeftPosition();
			}
		}
	}

	public virtual int BottomRow
	{
		get
		{
			return ClientAnchor.BottomRow + 1;
		}
		set
		{
			ClientAnchor.BottomRow = value - 1;
			if (m_bUpdatePositions)
			{
				UpdateHeight();
			}
		}
	}

	public virtual int RightColumn
	{
		get
		{
			return ClientAnchor.RightColumn + 1;
		}
		set
		{
			ClientAnchor.RightColumn = value - 1;
			if (m_bUpdatePositions)
			{
				RightColumnOffset = 0;
			}
			UpdateWidth();
		}
	}

	public virtual int TopRowOffset
	{
		get
		{
			return ClientAnchor.TopOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("TopRowOffset");
			}
			ClientAnchor.TopOffset = value;
			OnTopRowChanged();
		}
	}

	public virtual int LeftColumnOffset
	{
		get
		{
			return ClientAnchor.LeftOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("LeftColumnOffset");
			}
			ClientAnchor.LeftOffset = value;
			OnLeftColumnChange();
		}
	}

	public virtual int BottomRowOffset
	{
		get
		{
			return ClientAnchor.BottomOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("BottomRowOffset");
			}
			ClientAnchor.BottomOffset = value;
			UpdateHeight();
		}
	}

	public virtual int RightColumnOffset
	{
		get
		{
			return ClientAnchor.RightOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("RightColumnOffset");
			}
			ClientAnchor.RightOffset = value;
			UpdateWidth();
		}
	}

	[CLSCompliant(false)]
	public uint OldObjId
	{
		get
		{
			if (m_object != null)
			{
				return (m_object.RecordsList[0] as ftCmo).ID;
			}
			return 0u;
		}
		set
		{
			if (m_object != null)
			{
				(m_object.RecordsList[0] as ftCmo).ID = (ushort)value;
			}
		}
	}

	[CLSCompliant(false)]
	public MsoBase Record => m_record;

	[CLSCompliant(false)]
	public MsofbtSp InnerSpRecord => m_shape;

	public bool IsShortVersion
	{
		get
		{
			return m_clientAnchor.IsShortVersion;
		}
		set
		{
			m_clientAnchor.IsShortVersion = value;
		}
	}

	public int ShapeCount
	{
		get
		{
			if (!(m_record is MsofbtSpgrContainer msofbtSpgrContainer))
			{
				return 1;
			}
			return msofbtSpgrContainer.ItemsList.Count;
		}
	}

	public bool UpdatePositions
	{
		get
		{
			return m_bUpdatePositions;
		}
		set
		{
			m_bUpdatePositions = value;
		}
	}

	public virtual int Instance
	{
		get
		{
			if (m_shape == null)
			{
				return -1;
			}
			return m_shape.Instance;
		}
	}

	public bool HasFill
	{
		get
		{
			if (m_fill != null)
			{
				return m_fill.Visible;
			}
			return false;
		}
		internal set
		{
			if (!value)
			{
				m_fill = null;
			}
			else if (m_fill == null && value)
			{
				m_fill = new ShapeFillImpl(base.Application, base.Parent);
			}
		}
	}

	internal bool IsGroupFill
	{
		get
		{
			return m_isGroupFill;
		}
		set
		{
			m_isGroupFill = value;
		}
	}

	internal bool IsGroupLine
	{
		get
		{
			return m_isGroupLine;
		}
		set
		{
			m_isGroupLine = value;
		}
	}

	public bool HasLineFormat
	{
		get
		{
			if (m_lineFormat != null)
			{
				return m_lineFormat.Visible;
			}
			return false;
		}
		internal set
		{
			if (!value)
			{
				m_lineFormat = null;
			}
		}
	}

	public int ShapeId
	{
		get
		{
			return m_iShapeId;
		}
		set
		{
			m_iShapeId = value;
		}
	}

	[CLSCompliant(false)]
	public MsofbtSp ShapeRecord
	{
		get
		{
			if (m_shape == null)
			{
				m_shape = (MsofbtSp)MsoFactory.GetRecord(MsoRecords.msofbtSp);
			}
			return m_shape;
		}
	}

	internal bool IsActiveX => ((ftPioGrbit)Obj.FindSubRecord(TObjSubRecordType.ftPioGrbit))?.IsActiveX ?? false;

	internal Dictionary<string, string> StyleProperties
	{
		get
		{
			if (m_styleProperties == null)
			{
				return new Dictionary<string, string>();
			}
			return m_styleProperties;
		}
		set
		{
			m_styleProperties = value;
		}
	}

	internal string PreserveStyleString
	{
		get
		{
			return m_preserveStyleString;
		}
		set
		{
			m_preserveStyleString = value;
		}
	}

	internal bool IsHyperlink
	{
		get
		{
			return m_isHyperlink;
		}
		set
		{
			m_isHyperlink = value;
		}
	}

	internal bool IsAbsoluteAnchor
	{
		get
		{
			return m_bIsAbsoluteAnchor;
		}
		set
		{
			m_bIsAbsoluteAnchor = value;
		}
	}

	internal Stream NvPrExtLstStream
	{
		get
		{
			return m_streamExtLst;
		}
		set
		{
			m_streamExtLst = value;
		}
	}

	internal bool IsSlicer
	{
		get
		{
			return m_isSlicer;
		}
		set
		{
			m_isSlicer = value;
		}
	}

	internal GroupShapeImpl Group => base.Parent as GroupShapeImpl;

	internal bool IsGroup => m_shapeType == OfficeShapeType.Group;

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

	internal bool IsCustomGeometry
	{
		get
		{
			return m_isCustomGeom;
		}
		set
		{
			m_isCustomGeom = value;
		}
	}

	internal ShapeFrame ShapeFrame
	{
		get
		{
			return m_shapeFrame;
		}
		set
		{
			m_shapeFrame = value;
		}
	}

	internal ShapeFrame GroupFrame
	{
		get
		{
			return m_groupFrame;
		}
		set
		{
			m_groupFrame = value;
		}
	}

	private MsofbtOPT ShapeOptions
	{
		get
		{
			if (m_options == null)
			{
				m_options = CreateDefaultOptions();
			}
			return m_options;
		}
	}

	internal List<ShapeImpl> ChildShapes
	{
		get
		{
			if (m_childShapes == null)
			{
				m_childShapes = new List<ShapeImpl>();
			}
			return m_childShapes;
		}
	}

	internal MsofbtChildAnchor ChildAnchor => m_childAnchor;

	internal MsoUnknown UnKnown => m_unknown;

	[CLSCompliant(false)]
	public static void SerializeForte(IFopteOptionWrapper options, MsoOptions id, byte[] arr)
	{
		SerializeForte(options, id, arr, null, isValid: false);
	}

	[CLSCompliant(false)]
	public static void SerializeForte(IFopteOptionWrapper options, MsoOptions id, byte[] arr, byte[] addData, bool isValid)
	{
		if (arr == null)
		{
			throw new ArgumentNullException("arr");
		}
		SerializeForte(options, id, BitConverter.ToInt32(arr, 0), addData, isValid);
	}

	[CLSCompliant(false)]
	public static void SerializeForte(IFopteOptionWrapper options, MsoOptions id, int value)
	{
		SerializeForte(options, id, value, null, isValid: false);
	}

	[CLSCompliant(false)]
	public static void SerializeForte(IFopteOptionWrapper options, MsoOptions id, int value, byte[] addData, bool isValid)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = id;
		fOPTE.UInt32Value = (uint)value;
		fOPTE.IsValid = isValid;
		fOPTE.IsComplex = false;
		if (addData != null)
		{
			fOPTE.IsComplex = true;
			fOPTE.AdditionalData = addData;
			fOPTE.UInt32Value = (uint)addData.Length;
		}
		options.AddOptionSorted(fOPTE);
	}

	public ShapeImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_bSupportOptions = true;
		SetParents();
		m_bHasBorder = true;
		if (m_shapes.Worksheet == null)
		{
			m_bUpdatePositions = false;
		}
		m_shapeFrame = new ShapeFrame(this);
		m_clientAnchor = (MsofbtClientAnchor)MsoFactory.GetRecord(MsoRecords.msofbtClientAnchor);
	}

	public ShapeImpl(IApplication application, object parent, ShapeImpl instance)
		: this(application, parent)
	{
		m_bIsDisposed = instance.m_bIsDisposed;
		m_bSupportOptions = instance.m_bSupportOptions;
		m_rectAbsolute = instance.m_rectAbsolute;
		m_shapeType = instance.m_shapeType;
		m_strAlternativeText = instance.m_strAlternativeText;
		m_strName = instance.m_strName;
		MsoBase record = instance.m_record;
		if (record != null)
		{
			m_record = (MsoBase)CloneUtils.CloneMsoBase(record, null);
		}
		else if (m_shape != null)
		{
			m_shape = (MsofbtSp)CloneUtils.CloneMsoBase(instance.m_shape, null);
		}
		UpdateRecord(instance.m_clientAnchor);
		m_object = (OBJRecord)CloneUtils.CloneCloneable(instance.m_object);
	}

	[CLSCompliant(false)]
	public ShapeImpl(IApplication application, object parent, MsoBase[] records, int index)
		: this(application, parent)
	{
		m_record = (MsofbtSpContainer)records[index];
		ParseRecord();
	}

	[CLSCompliant(false)]
	public ShapeImpl(IApplication application, object parent, MsofbtSpContainer container)
		: this(application, parent, container, OfficeParseOptions.Default)
	{
	}

	[CLSCompliant(false)]
	public ShapeImpl(IApplication application, object parent, MsofbtSpContainer container, OfficeParseOptions options)
		: this(application, parent)
	{
		m_record = container;
		ParseRecord(options);
		m_bSupportOptions = true;
	}

	[CLSCompliant(false)]
	public ShapeImpl(IApplication application, object parent, MsoBase shapeRecord)
		: this(application, parent, shapeRecord, OfficeParseOptions.Default)
	{
	}

	[CLSCompliant(false)]
	public ShapeImpl(IApplication application, object parent, MsoBase shapeRecord, OfficeParseOptions options)
		: this(application, parent)
	{
		m_record = shapeRecord;
	}

	protected virtual void CreateDefaultFillLineFormats()
	{
	}

	private void ParseRecord()
	{
		ParseRecord(OfficeParseOptions.Default);
	}

	private void ParseRecord(OfficeParseOptions options)
	{
		m_shapeType = OfficeShapeType.Unknown;
		MsofbtSpContainer msofbtSpContainer = m_record as MsofbtSpContainer;
		CreateDefaultFillLineFormats();
		if (msofbtSpContainer == null)
		{
			return;
		}
		List<MsoBase> itemsList = msofbtSpContainer.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			MsoBase msoBase = itemsList[i];
			switch (msoBase.MsoRecordType)
			{
			case MsoRecords.msofbtOPT:
				if (m_bUpdateLineFill)
				{
					ParseOptions((MsofbtOPT)msoBase);
				}
				else
				{
					ExtractNecessaryOptions(m_options = msoBase as MsofbtOPT);
				}
				break;
			case MsoRecords.msofbtSp:
				ParseShape((MsofbtSp)msoBase);
				break;
			case MsoRecords.msofbtClientAnchor:
				ParseClientAnchor((MsofbtClientAnchor)msoBase);
				break;
			case MsoRecords.msofbtSpgr:
				ParseShapeGroup((MsofbtSpgr)msoBase);
				break;
			case MsoRecords.msofbtSpgrContainer:
				ParseShapeGroupContainer((MsofbtSpgrContainer)msoBase);
				break;
			case MsoRecords.msofbtChildAnchor:
				ParseChildAnchor((MsofbtChildAnchor)msoBase);
				break;
			case MsoRecords.msofbtClientData:
				ParseClientData((MsofbtClientData)msoBase, options);
				break;
			default:
				if (msoBase.MsoRecordType == MsoRecords.msofbtClientTextbox)
				{
					ParseOtherRecords(msoBase, options);
				}
				else
				{
					ParseUnKnown((MsoUnknown)msoBase);
				}
				break;
			}
		}
		if (Id != 0)
		{
			m_iShapeId = Id;
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseClientData(MsofbtClientData clientData, OfficeParseOptions options)
	{
		m_object = clientData.ObjectRecord;
		List<ObjSubRecord> recordsList = m_object.RecordsList;
		m_book.CurrentObjectId = Math.Max(m_book.CurrentObjectId, (recordsList[0] as ftCmo).ID);
		int i = 1;
		for (int count = recordsList.Count; i < count; i++)
		{
			ObjSubRecord objSubRecord = recordsList[i];
			if (objSubRecord.Type == TObjSubRecordType.ftMacro)
			{
				ftMacro ftMacro = (ftMacro)objSubRecord;
				m_macroTokens = ftMacro.Tokens;
				break;
			}
		}
		int j = 0;
		for (int count2 = recordsList.Count; j < count2; j++)
		{
			ObjSubRecord objSubRecord2 = recordsList[j];
			if (objSubRecord2.Type == TObjSubRecordType.ftCmo)
			{
				ftCmo ftCmo = (ftCmo)objSubRecord2;
				m_printWithSheet = ftCmo.Printable;
				break;
			}
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseOtherRecords(MsoBase subRecord, OfficeParseOptions options)
	{
	}

	private void ParseOptions(MsofbtOPT options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		m_options = options;
		MsofbtOPT.FOPTE[] properties = options.Properties;
		int i = 0;
		for (int num = properties.Length; i < num; i++)
		{
			ParseOption(properties[i]);
		}
	}

	private bool ParseFill(MsofbtOPT.FOPTE option)
	{
		if (m_fill == null && Array.IndexOf(FillOptions, option.Id) >= 0)
		{
			m_fill = new ShapeFillImpl(base.Application, this);
		}
		if (m_fill == null)
		{
			return false;
		}
		return m_fill.ParseOption(option);
	}

	private bool ParseLineFormat(MsofbtOPT.FOPTE option)
	{
		if (m_lineFormat == null && Array.IndexOf(LineOptions, option.Id) >= 0)
		{
			m_lineFormat = new ShapeLineFormatImpl(base.Application, this);
		}
		if (m_lineFormat == null)
		{
			return false;
		}
		return m_lineFormat.ParseOption(option);
	}

	[CLSCompliant(false)]
	protected virtual bool ParseOption(MsofbtOPT.FOPTE option)
	{
		if (ParseFill(option))
		{
			return true;
		}
		if (ParseLineFormat(option))
		{
			return true;
		}
		switch (option.Id)
		{
		case MsoOptions.ShapeName:
			m_strName = ParseName(option);
			return true;
		case MsoOptions.SizeTextToFitShape:
			m_bAutoSize = option.UInt32Value == 655370;
			return true;
		case MsoOptions.AlternativeText:
			m_strAlternativeText = ParseName(option);
			break;
		}
		return false;
	}

	[CLSCompliant(false)]
	protected virtual void ParseShape(MsofbtSp shapeRecord)
	{
		if (shapeRecord == null)
		{
			throw new ArgumentNullException("shapeRecord");
		}
		m_shape = (MsofbtSp)shapeRecord.Clone();
	}

	[CLSCompliant(false)]
	public virtual void ParseClientAnchor(MsofbtClientAnchor clientAnchor)
	{
		if (clientAnchor == null)
		{
			throw new ArgumentNullException("clientAnchor");
		}
		m_clientAnchor = clientAnchor;
		if (m_shapes.Worksheet != null)
		{
			int maxColumnCount = m_shapes.Workbook.MaxColumnCount;
			if (m_clientAnchor.RightColumn >= maxColumnCount && m_clientAnchor.LeftColumn >= maxColumnCount)
			{
				int rightColumn = (clientAnchor.LeftColumn = 0);
				clientAnchor.RightColumn = rightColumn;
			}
			else if (m_clientAnchor.RightColumn >= maxColumnCount)
			{
				clientAnchor.RightColumn = clientAnchor.LeftColumn;
			}
			else if (m_clientAnchor.LeftColumn >= maxColumnCount)
			{
				clientAnchor.RightColumn = 255;
				clientAnchor.LeftColumn = 0;
			}
			int maxRowCount = m_shapes.Workbook.MaxRowCount;
			if (m_clientAnchor.BottomRow >= maxRowCount)
			{
				m_clientAnchor.BottomRow = maxRowCount;
			}
		}
		if (!m_clientAnchor.IsShortVersion)
		{
			EvaluateTopLeftPosition();
			UpdateHeight();
			UpdateWidth();
		}
	}

	protected virtual void SetParents()
	{
		m_shapes = FindParent(typeof(ShapeCollectionBase), bSubTypes: true) as ShapeCollectionBase;
		if (m_shapes == null)
		{
			throw new ArgumentNullException("Can't find parent collection.");
		}
		m_book = m_shapes.Workbook;
	}

	internal void ChangeParent(object parent)
	{
		SetParent(parent);
	}

	protected void AttachEvents()
	{
		if (m_shapes.WorksheetBase is WorksheetImpl worksheetImpl)
		{
			(m_book.Styles["Normal"].Font as FontWrapper).AfterChangeEvent += NormalFont_OnAfterChange;
			worksheetImpl.ColumnWidthChanged += Worksheet_ColumnWidthChanged;
			worksheetImpl.RowHeightChanged += Worksheet_RowHeightChanged;
		}
	}

	protected void DetachEvents()
	{
		if (m_shapes.WorksheetBase is WorksheetImpl worksheetImpl)
		{
			if (m_book.Styles != null && m_book.Styles.Contains("Normal"))
			{
				(m_book.Styles["Normal"].Font as FontWrapper).AfterChangeEvent -= NormalFont_OnAfterChange;
			}
			worksheetImpl.ColumnWidthChanged -= Worksheet_ColumnWidthChanged;
			worksheetImpl.RowHeightChanged -= Worksheet_RowHeightChanged;
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseShapeGroup(MsofbtSpgr shapeGroup)
	{
	}

	[CLSCompliant(false)]
	protected virtual void ParseShapeGroupContainer(MsofbtSpgrContainer subRecord)
	{
	}

	[CLSCompliant(false)]
	protected virtual void ParseChildAnchor(MsofbtChildAnchor childAnchor)
	{
		if (childAnchor == null)
		{
			throw new ArgumentNullException("childAnchor");
		}
		m_childAnchor = childAnchor;
	}

	protected virtual void ParseUnKnown(MsoUnknown UnKnown)
	{
		if (UnKnown == null)
		{
			throw new ArgumentNullException("MsOUnknown");
		}
		m_unknown = UnKnown;
	}

	[CLSCompliant(false)]
	protected Color GetColorValue(MsofbtOPT.FOPTE option)
	{
		byte[] bytes = BitConverter.GetBytes(option.UInt32Value);
		if (bytes[3] == 8)
		{
			OfficeKnownColors color = (OfficeKnownColors)bytes[0];
			return m_book.GetPaletteColor(color);
		}
		return Color.FromArgb(0, bytes[0], bytes[1], bytes[2]);
	}

	private byte GetByte(MsofbtOPT.FOPTE option, int iByteIndex)
	{
		return BitConverter.GetBytes(option.UInt32Value)[iByteIndex];
	}

	[CLSCompliant(false)]
	protected string ParseName(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		byte[] additionalData = option.AdditionalData;
		string text = null;
		if (additionalData == null)
		{
			return null;
		}
		if (additionalData.Length != 0)
		{
			text = Encoding.Unicode.GetString(additionalData, 0, additionalData.Length);
			return text.Substring(0, text.Length - 1);
		}
		return text = string.Empty;
	}

	private void ExtractNecessaryOptions(MsofbtOPT options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		IList<MsofbtOPT.FOPTE> propertyList = options.PropertyList;
		int i = 0;
		for (int count = propertyList.Count; i < count; i++)
		{
			MsofbtOPT.FOPTE option = propertyList[i];
			ExtractNecessaryOption(option);
		}
	}

	[CLSCompliant(false)]
	protected virtual bool ExtractNecessaryOption(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		switch (option.Id)
		{
		case MsoOptions.ShapeName:
			m_strName = ParseName(option);
			return true;
		case MsoOptions.SizeTextToFitShape:
			m_bAutoSize = option.UInt32Value == 655370;
			return true;
		case MsoOptions.AlternativeText:
			m_strAlternativeText = ParseName(option);
			return true;
		default:
			return false;
		}
	}

	public void Remove()
	{
		OnDelete();
		m_shapes.Remove(this);
	}

	public void Scale(int scaleWidth, int scaleHeight)
	{
		if (scaleWidth < 0)
		{
			throw new ArgumentOutOfRangeException("scaleWidth");
		}
		if (scaleHeight < 0)
		{
			throw new ArgumentOutOfRangeException("scaleHeight");
		}
		Width = (int)((double)(Width * scaleWidth) / 100.0);
		Height = (int)((double)(Height * scaleHeight) / 100.0);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		if (m_bIsDisposed)
		{
			if (m_streamExtLst != null)
			{
				m_streamExtLst = null;
			}
			if (m_graphicFrame != null)
			{
				m_graphicFrame = null;
			}
			if (preservedShapeStreams != null)
			{
				preservedShapeStreams.Clear();
				preservedShapeStreams = null;
			}
			if (preservedCnxnShapeStreams != null)
			{
				preservedCnxnShapeStreams.Clear();
				preservedCnxnShapeStreams = null;
			}
			if (preservedInnerCnxnShapeStreams != null)
			{
				preservedInnerCnxnShapeStreams.Clear();
				preservedInnerCnxnShapeStreams = null;
			}
			if (preservedPictureStreams != null)
			{
				preservedPictureStreams.Clear();
				preservedPictureStreams = null;
			}
			if (m_formulaMacroStream != null)
			{
				m_formulaMacroStream = null;
			}
			if (m_xmlDataStream != null)
			{
				m_xmlDataStream = null;
			}
			if (m_xmlTypeStream != null)
			{
				m_xmlTypeStream = null;
			}
			m_record = null;
			m_shape = null;
			m_clientAnchor = null;
			m_object = null;
			m_options = null;
			m_childAnchor = null;
			m_unknown = null;
			if (m_fill != null)
			{
				m_fill.Dispose();
				m_fill = null;
			}
			if (m_lineFormat != null)
			{
				m_lineFormat.Dispose();
				m_lineFormat = null;
			}
			if (m_shadow != null)
			{
				m_shadow.Dispose();
				m_shadow = null;
			}
			if (m_3D != null)
			{
				m_3D.Dispose();
				m_3D = null;
			}
			if (m_styleProperties != null)
			{
				m_styleProperties.Clear();
				m_fill = null;
			}
			if (m_childShapes != null)
			{
				m_childShapes.Clear();
			}
			if (m_shapeFrame != null)
			{
				m_shapeFrame = null;
			}
			if (m_groupFrame != null)
			{
				m_groupFrame = null;
			}
		}
	}

	[CLSCompliant(false)]
	public void Serialize(MsofbtSpgrContainer spgrContainer)
	{
		if (ChildShapes.Count > 0)
		{
			SerializeShape(spgrContainer, isGroupShape: true);
		}
		else
		{
			SerializeShape(spgrContainer);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(MsofbtSpgrContainer spgrContainer, bool isGroupShape)
	{
		SerializeShape(spgrContainer, isGroupShape);
	}

	[CLSCompliant(false)]
	protected virtual void SerializeShape(MsofbtSpgrContainer spgrContainer)
	{
		if (m_record != null)
		{
			spgrContainer.AddItem(m_record);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeShape(MsofbtSpgrContainer spgrContainer, bool isGroupShape)
	{
		List<ShapeImpl> childShapes = ChildShapes;
		if (childShapes.Count > 0)
		{
			MsofbtSpgrContainer msofbtSpgrContainer = (MsofbtSpgrContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpgrContainer);
			spgrContainer.AddItem(msofbtSpgrContainer);
			{
				foreach (ShapeImpl item in childShapes)
				{
					item.Serialize(msofbtSpgrContainer, isGroupShape: true);
				}
				return;
			}
		}
		if (m_record != null)
		{
			spgrContainer.AddItem(m_record);
		}
	}

	private void SerializeMsoOptions(MsofbtSpContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (m_options == null)
		{
			m_options = CreateDefaultOptions();
		}
		if (m_shapeType == OfficeShapeType.Unknown)
		{
			return;
		}
		if (m_bUpdateLineFill)
		{
			m_options = SerializeMsoOptions(m_options);
		}
		List<MsoBase> itemsList = container.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			if (itemsList[i] is MsofbtOPT)
			{
				itemsList[i] = m_options;
				break;
			}
		}
	}

	[CLSCompliant(false)]
	protected MsofbtOPT SerializeMsoOptions(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (m_fill != null)
		{
			UpdateFillFopte(opt);
			opt = (MsofbtOPT)m_fill.Serialize(opt);
		}
		if (m_bSupportOptions && m_lineFormat != null)
		{
			m_lineFormat.Serialize(opt);
		}
		SerializeCommentShadow(opt);
		return opt;
	}

	private void SerializeTransparency(MsofbtOPT opt, int value)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int num = 100 - value;
		SerializeForte(opt, MsoOptions.Transparency, (int)((double)num * 655.0));
	}

	[CLSCompliant(false)]
	protected virtual MsofbtOPT SerializeOptions(MsoBase parent)
	{
		if (m_options == null)
		{
			m_options = CreateDefaultOptions();
		}
		return m_options;
	}

	[CLSCompliant(false)]
	protected void SerializeSizeTextToFit(MsofbtOPT options)
	{
		int value = (m_bAutoSize ? 655370 : 524296);
		SerializeOptionSorted(options, MsoOptions.SizeTextToFitShape, (uint)value);
	}

	[CLSCompliant(false)]
	protected void SerializeHitTest(MsofbtOPT options)
	{
		SerializeOptionSorted(options, MsoOptions.NoFillHitTest, 1048592u);
	}

	[CLSCompliant(false)]
	protected void SerializeOption(MsofbtOPT options, MsoOptions id, uint value)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = id;
		fOPTE.UInt32Value = value;
		fOPTE.IsValid = false;
		fOPTE.IsComplex = false;
		options.AddOptionsOrReplace(fOPTE);
	}

	[CLSCompliant(false)]
	protected MsofbtOPT.FOPTE SerializeOption(MsofbtOPT options, MsoOptions id, int value)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = id;
		fOPTE.Int32Value = value;
		fOPTE.IsValid = false;
		fOPTE.IsComplex = false;
		options.AddOptionsOrReplace(fOPTE);
		return fOPTE;
	}

	[CLSCompliant(false)]
	protected void SerializeOptionSorted(MsofbtOPT options, MsoOptions id, uint value)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = id;
		fOPTE.UInt32Value = value;
		fOPTE.IsValid = false;
		fOPTE.IsComplex = false;
		options.AddOptionSorted(fOPTE);
	}

	[CLSCompliant(false)]
	protected void SerializeShapeVisibility(MsofbtOPT options)
	{
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.CommentShowAlways;
		byte[] array;
		if (options.IndexOf(MsoOptions.CommentShowAlways) == options.PropertyList.Count)
		{
			array = new byte[4] { 0, 0, 2, 0 };
		}
		else
		{
			array = new byte[4];
			Array.Copy(options.PropertyList[options.IndexOf(MsoOptions.CommentShowAlways)].MainData, 2, array, 0, 4);
		}
		if (IsShapeVisible)
		{
			array[0] = 0;
		}
		else
		{
			array[0] = 2;
		}
		fOPTE.UInt32Value = BitConverter.ToUInt32(array, 0);
		options.AddOptionSorted(fOPTE);
	}

	[CLSCompliant(false)]
	protected void SerializeShapeName(MsofbtOPT options)
	{
		SerializeName(options, MsoOptions.ShapeName, m_strName);
	}

	[CLSCompliant(false)]
	protected void SerializeName(MsofbtOPT options, MsoOptions optionId, string name)
	{
		if (name != null)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			int length = name.Length;
			string text = name;
			if (length > 0 && name[length - 1] != 0)
			{
				text += "\0";
			}
			byte[] bytes = Encoding.Unicode.GetBytes(text);
			MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
			fOPTE.Id = optionId;
			fOPTE.UInt32Value = (uint)bytes.Length;
			fOPTE.IsValid = true;
			fOPTE.IsComplex = true;
			fOPTE.AdditionalData = bytes;
			options.AddOptionSorted(fOPTE);
		}
	}

	[CLSCompliant(false)]
	protected virtual MsofbtOPT CreateDefaultOptions()
	{
		return (MsofbtOPT)MsoFactory.GetRecord(MsoRecords.msofbtOPT);
	}

	private void UpdateFillFopte(MsofbtOPT option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("opt");
		}
		for (int i = 384; i <= 412; i++)
		{
			option.RemoveOption(i);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeCommentShadow(MsofbtOPT option)
	{
	}

	internal double GetBorderThickness()
	{
		return (Line as ShapeLineFormatImpl).Weight;
	}

	internal Color GetBorderColor()
	{
		Color empty = ColorExtension.Empty;
		TextBoxShapeImpl textBoxShapeImpl = this as TextBoxShapeImpl;
		if (this is AutoShapeImpl autoShapeImpl && ((!autoShapeImpl.Line.Visible && !autoShapeImpl.ShapeExt.IsCreated) || (autoShapeImpl.ShapeExt.IsCreated && autoShapeImpl.ShapeExt.Logger.GetPreservedItem(PreservedFlag.Line) && !autoShapeImpl.Line.Visible)))
		{
			return empty;
		}
		if (textBoxShapeImpl != null && !textBoxShapeImpl.Line.Visible)
		{
			return empty;
		}
		return GetDefaultColor(PreservedFlag.Line, "lnRef");
	}

	internal Color GetFillColor()
	{
		Color empty = ColorExtension.Empty;
		TextBoxShapeImpl textBoxShapeImpl = this as TextBoxShapeImpl;
		if (this is AutoShapeImpl autoShapeImpl && !autoShapeImpl.Fill.Visible)
		{
			return empty;
		}
		if (textBoxShapeImpl != null && !textBoxShapeImpl.Fill.Visible)
		{
			return empty;
		}
		return GetDefaultColor(PreservedFlag.Fill, "fillRef");
	}

	internal Color GetDefaultColor(PreservedFlag flag, string elementTag)
	{
		Color color = ColorExtension.Empty;
		bool flag2 = false;
		TextBoxShapeImpl textBoxShapeImpl = this as TextBoxShapeImpl;
		AutoShapeImpl autoShapeImpl = this as AutoShapeImpl;
		if (autoShapeImpl != null)
		{
			color = (autoShapeImpl.ShapeExt.IsCreated ? (flag switch
			{
				PreservedFlag.Fill => Color.FromArgb(0, 68, 114, 196), 
				PreservedFlag.Line => Color.FromArgb(0, 47, 82, 143), 
				_ => ColorExtension.Black, 
			}) : (flag switch
			{
				PreservedFlag.Fill => Color.FromArgb(0, 91, 155, 213), 
				PreservedFlag.Line => Color.FromArgb(0, 65, 113, 156), 
				_ => ColorExtension.White, 
			}));
		}
		else if (textBoxShapeImpl != null)
		{
			switch (flag)
			{
			case PreservedFlag.Fill:
				if ((textBoxShapeImpl.Fill as ShapeFillImpl).ForeColorObject.Value == -1)
				{
					color = ColorExtension.White;
					flag2 = true;
				}
				else
				{
					color = textBoxShapeImpl.Fill.ForeColor;
					flag2 = true;
				}
				break;
			case PreservedFlag.Line:
				_ = textBoxShapeImpl.Line;
				color = textBoxShapeImpl.Line.ForeColor;
				break;
			default:
				color = ColorExtension.Black;
				break;
			}
		}
		if (textBoxShapeImpl != null)
		{
			switch (flag)
			{
			case PreservedFlag.Line:
			{
				ShapeLineFormatImpl shapeLineFormatImpl = textBoxShapeImpl.Line as ShapeLineFormatImpl;
				if (textBoxShapeImpl.StyleStream == null || (textBoxShapeImpl.IsLineProperties && shapeLineFormatImpl.IsNoFill) || shapeLineFormatImpl.IsSolidFill)
				{
					break;
				}
				XmlReader xmlReader2 = UtilityMethods.CreateReader(textBoxShapeImpl.StyleStream, elementTag);
				if (xmlReader2.MoveToAttribute("idx") && int.Parse(xmlReader2.Value) != 0)
				{
					xmlReader2.Read();
					if (ParentWorkbook.DataHolder.Parser.m_dicThemeColors != null)
					{
						color = ChartParserCommon.ReadColor(xmlReader2, out var _, out var _, out var _, ParentWorkbook.DataHolder.Parser);
					}
				}
				break;
			}
			case PreservedFlag.Fill:
			{
				if (textBoxShapeImpl.StyleStream == null || textBoxShapeImpl.IsFill || textBoxShapeImpl.IsNoFill)
				{
					break;
				}
				XmlReader xmlReader3 = UtilityMethods.CreateReader(textBoxShapeImpl.StyleStream, elementTag);
				if (xmlReader3.MoveToAttribute("idx") && int.Parse(xmlReader3.Value) != 0)
				{
					xmlReader3.Read();
					if (ParentWorkbook.DataHolder.Parser.m_dicThemeColors != null)
					{
						color = ChartParserCommon.ReadColor(xmlReader3, out var _, out var _, out var _, ParentWorkbook.DataHolder.Parser);
					}
				}
				break;
			}
			default:
				if (!flag2 && textBoxShapeImpl.StyleStream != null)
				{
					XmlReader xmlReader = UtilityMethods.CreateReader(textBoxShapeImpl.StyleStream, elementTag);
					xmlReader.Read();
					if (ParentWorkbook.DataHolder.Parser.m_dicThemeColors != null)
					{
						color = ChartParserCommon.ReadColor(xmlReader, out var _, out var _, out var _, ParentWorkbook.DataHolder.Parser);
					}
				}
				break;
			}
		}
		else if (autoShapeImpl != null)
		{
			if (autoShapeImpl.ShapeExt.Logger.GetPreservedItem(flag) || (flag == PreservedFlag.Fill && autoShapeImpl.IsGroupFill))
			{
				switch (flag)
				{
				case PreservedFlag.Fill:
					return autoShapeImpl.Fill.ForeColor;
				case PreservedFlag.Line:
					return autoShapeImpl.Line.ForeColor;
				}
			}
			else
			{
				string streamTag = "Style";
				if (flag == PreservedFlag.Fill && autoShapeImpl.ShapeExt.PreservedElements.ContainsKey("Fill"))
				{
					streamTag = "Fill";
					elementTag = "solidFill";
				}
				if (flag == PreservedFlag.Line && autoShapeImpl.ShapeExt.PreservedElements.ContainsKey("Line"))
				{
					streamTag = "Line";
					elementTag = "solidFill";
				}
				GetStyleColor(autoShapeImpl, flag, streamTag, elementTag, ref color);
			}
		}
		return color;
	}

	internal void GetStyleColor(AutoShapeImpl autoShape, PreservedFlag flag, string streamTag, string elementTag, ref Color color)
	{
		Stream value = null;
		if (autoShape.ShapeExt.PreservedElements.TryGetValue(streamTag, out value))
		{
			if (value == null || value.Length <= 0)
			{
				return;
			}
			value.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(value, elementTag);
			if (flag == PreservedFlag.Line && xmlReader.NodeType == XmlNodeType.None)
			{
				GetStyleColor(autoShape, flag, "Style", "lnRef", ref color);
				return;
			}
			xmlReader.Read();
			bool flag2 = false;
			if (xmlReader.LocalName == "srgbClr")
			{
				flag2 = true;
			}
			if (ParentWorkbook.DataHolder != null)
			{
				int transparecy;
				int tint;
				int shade;
				Color color2 = ChartParserCommon.ReadColor(xmlReader, out transparecy, out tint, out shade, ParentWorkbook.DataHolder.Parser);
				if (ParentWorkbook.DataHolder.Parser.m_dicThemeColors != null || !CheckIfColorEmpty(color2))
				{
					color = color2;
				}
				else if (ParentWorkbook.DataHolder.Parser.m_dicThemeColors == null && CheckIfColorEmpty(color2) && flag2)
				{
					color = color2;
				}
			}
		}
		else if (!autoShape.ShapeExt.IsCreated && flag == PreservedFlag.RichText)
		{
			color = ColorExtension.Black;
		}
	}

	private bool CheckIfColorEmpty(Color color)
	{
		if (color.A == 0 && color.B == 0 && color.G == 0 && color.R == 0)
		{
			return true;
		}
		return false;
	}

	public virtual void GenerateDefaultName()
	{
		Name = CollectionBaseEx<IShape>.GenerateDefaultName(m_shapes, "Shape ");
	}

	protected virtual void OnDelete()
	{
	}

	[CLSCompliant(false)]
	protected void SetObject(OBJRecord value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_object = value;
	}

	public virtual IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollections)
	{
		ShapeImpl shapeImpl = (ShapeImpl)MemberwiseClone();
		shapeImpl.SetParent(parent);
		shapeImpl.SetParents();
		shapeImpl.CopyFrom(this, hashNewNames, dicFontIndexes);
		if (m_shapeFrame != null)
		{
			shapeImpl.ShapeFrame = m_shapeFrame.Clone(shapeImpl);
		}
		if (m_groupFrame != null)
		{
			shapeImpl.GroupFrame = m_groupFrame.Clone(shapeImpl);
		}
		shapeImpl.CloneLineFill(this);
		if (shapeImpl.ShapeType == OfficeShapeType.AutoShape)
		{
			AutoShapeImpl obj = shapeImpl as AutoShapeImpl;
			obj.ShapeExt = (this as AutoShapeImpl).ShapeExt.Clone(shapeImpl);
			obj.ShapeExt.ShapeID = CollectionBaseEx<IShape>.GenerateID(m_shapes);
		}
		if (addToCollections)
		{
			shapeImpl.m_shapes.AddShape(shapeImpl);
		}
		shapeImpl.AttachEvents();
		shapeImpl.OldObjId = 0u;
		return shapeImpl;
	}

	public object Clone(object parent)
	{
		return Clone(parent, null, null, addToCollections: true);
	}

	public virtual void CopyFrom(ShapeImpl shape, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
	{
		MsoBase record = shape.m_record;
		if (record != null)
		{
			m_record = (MsoBase)record.Clone();
		}
		UpdateRecord(shape.ClientAnchor);
		m_iShapeId = shape.m_iShapeId;
	}

	public bool CanInsertRowColumn(int iIndex, int iCount, bool bRow, int iMaxIndex)
	{
		if (!IsMoveWithCell && !IsSizeWithCell)
		{
			return true;
		}
		if (IsIndexLess(iIndex, bRow))
		{
			if (IsMoveWithCell)
			{
				if (GetLowerBound(bRow) + iCount >= 0)
				{
					return GetUpperBound(bRow) + iCount <= iMaxIndex;
				}
				return false;
			}
		}
		else if (IsIndexMiddle(iIndex, bRow) && IsSizeWithCell)
		{
			return GetUpperBound(bRow) + iCount <= iMaxIndex;
		}
		return true;
	}

	private int GetLowerBound(bool bRow)
	{
		if (!bRow)
		{
			return LeftColumn;
		}
		return TopRow;
	}

	private int GetUpperBound(bool bRow)
	{
		if (!bRow)
		{
			return RightColumn;
		}
		return BottomRow;
	}

	public void RemoveRowColumn(int iIndex, int iCount, bool bRow)
	{
		int num = (bRow ? BottomRow : RightColumn);
		bool flag = iIndex <= num;
		if (!IsMoveWithCell && !IsSizeWithCell)
		{
			UpdateNotSizeNotMoveShape(bRow, iIndex, -iCount);
			flag = false;
		}
		if (flag)
		{
			int countAbove = GetCountAbove(iIndex, iCount, bRow);
			if (countAbove > 0)
			{
				UpdateAboveRowColumnIndexes(-countAbove, bRow);
			}
			iCount -= countAbove;
			flag = iCount > 0;
		}
		if (flag && IndicatesFirst(iIndex, iCount, bRow))
		{
			UpdateFirstRowColumnIndexes(bRow, -1);
			iCount--;
			flag = iCount > 0;
		}
		if (flag)
		{
			int countInside = GetCountInside(iIndex, iCount, bRow);
			if (countInside > 0)
			{
				UpdateInsideRowColumnIndexes(-countInside, bRow);
			}
			iCount -= countInside;
			flag = iCount > 0;
		}
		if (flag)
		{
			UpdateLastRowColumnIndex(bRow);
		}
	}

	public void InsertRowColumn(int iIndex, int iCount, bool bRow)
	{
		if (!IsSizeWithCell && !IsMoveWithCell)
		{
			UpdateNotSizeNotMoveShape(bRow, iIndex, iCount);
		}
		else if (IsIndexLess(iIndex, bRow))
		{
			if (IsMoveWithCell)
			{
				IncreaseAndUpdateAll(iCount, bRow);
			}
		}
		else if (IsIndexMiddle(iIndex, bRow) && IsSizeWithCell)
		{
			IncreaseAndUpdateEnd(iCount, bRow);
		}
	}

	public virtual void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
	}

	public void SetName(string strShapeName)
	{
		if (strShapeName == null)
		{
			throw new ArgumentNullException("strShapeName");
		}
		if (strShapeName.Length == 0)
		{
			throw new ArgumentException("strShapeName - string cannot be empty.");
		}
		m_strName = strShapeName;
	}

	public virtual void RegisterInSubCollection()
	{
	}

	public virtual bool CanCopyShapesOnRangeCopy(Rectangle sourceRec, Rectangle destRec, out Rectangle newPosition)
	{
		int leftColumn = LeftColumn;
		int topRow = TopRow;
		bool flag = (leftColumn == RightColumn && (leftColumn > sourceRec.Right || leftColumn < sourceRec.Left)) || (topRow == BottomRow && (topRow > sourceRec.Bottom || topRow < sourceRec.Top));
		bool flag2 = IsMoveWithCell && !flag;
		newPosition = new Rectangle(0, 0, 0, 0);
		if (flag2)
		{
			newPosition.Y = topRow - sourceRec.Top + destRec.Top;
			flag2 = sourceRec.Top - 1 <= topRow && newPosition.Top > 0;
		}
		if (flag2)
		{
			newPosition.X = leftColumn - sourceRec.Left + destRec.Left;
			flag2 = sourceRec.Left - 1 <= leftColumn && newPosition.Left > 0;
		}
		if (flag2)
		{
			int num = destRec.Bottom - (sourceRec.Bottom - BottomRow);
			flag2 = sourceRec.Bottom + 1 >= BottomRow && num <= m_book.MaxRowCount;
			newPosition.Height = num - newPosition.Y;
		}
		if (flag2)
		{
			int num2 = destRec.Right - (sourceRec.Right - RightColumn);
			flag2 = sourceRec.Right + 1 >= RightColumn && num2 <= m_book.MaxColumnCount;
			newPosition.Width = num2 - newPosition.X;
		}
		return flag2;
	}

	public virtual ShapeImpl CopyMoveShapeOnRangeCopyMove(WorksheetImpl sheet, Rectangle destRec, bool bIsCopy)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		ShapeImpl shapeImpl = this;
		ShapesCollection shapesCollection = (ShapesCollection)sheet.Shapes;
		if (bIsCopy)
		{
			shapeImpl = (ShapeImpl)shapeImpl.Clone(shapesCollection, null, null, addToCollections: true);
		}
		else if (sheet != Worksheet)
		{
			((ShapesCollection)Worksheet.Shapes).Remove(this);
			shapesCollection.AddShape(shapeImpl);
		}
		int height = Height;
		int width = Width;
		int num = ConvertHeightOffsetIntoPixels((int)ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.TopRow, bRaiseEvents: false), MeasureUnits.Point), shapeImpl.ClientAnchor.TopOffset, bIsOffsetInPixels: false);
		shapeImpl.ClientAnchor.TopRow = destRec.Top - 1;
		if ((double)num < ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.TopRow, bRaiseEvents: false), MeasureUnits.Point) && (!sheet.CopyShapesForSorting || shapeImpl.IsSizeWithCell))
		{
			shapeImpl.ClientAnchor.TopOffset = ConvertPixelsIntoHeightOffset(num, (int)ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.TopRow, bRaiseEvents: false), MeasureUnits.Point));
		}
		int num2 = ConvertWidthOffsetIntoPixels(m_shapes.Worksheet.GetColumnWidthInPixels(shapeImpl.LeftColumn), shapeImpl.LeftColumnOffset, bIsInPixels: false);
		shapeImpl.ClientAnchor.LeftColumn = destRec.Left - 1;
		if (num2 < sheet.GetColumnWidthInPixels(shapeImpl.LeftColumn) && (!sheet.CopyShapesForSorting || shapeImpl.IsSizeWithCell))
		{
			shapeImpl.ClientAnchor.LeftOffset = ConvertPixelsIntoWidthOffset(num2, sheet.GetColumnWidthInPixels(shapeImpl.LeftColumn));
		}
		num = ConvertHeightOffsetIntoPixels((int)ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.BottomRow, bRaiseEvents: false), MeasureUnits.Point), shapeImpl.ClientAnchor.BottomOffset, bIsOffsetInPixels: false);
		num2 = ConvertWidthOffsetIntoPixels(m_shapes.Worksheet.GetColumnWidthInPixels(shapeImpl.RightColumn), shapeImpl.RightColumnOffset, bIsInPixels: false);
		if (IsSizeWithCell)
		{
			shapeImpl.ClientAnchor.BottomRow = destRec.Bottom - 1;
			shapeImpl.ClientAnchor.RightColumn = destRec.Right - 1;
			shapeImpl.UpdateWidth();
			shapeImpl.UpdateHeight();
		}
		else
		{
			shapeImpl.Height = height;
			shapeImpl.Width = width;
			shapeImpl.UpdateBottomRow();
			shapeImpl.UpdateRightColumn();
		}
		if ((double)num < ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.BottomRow, bRaiseEvents: false), MeasureUnits.Point) && (!sheet.CopyShapesForSorting || shapeImpl.IsSizeWithCell))
		{
			shapeImpl.ClientAnchor.BottomOffset = ConvertPixelsIntoHeightOffset(num, (int)ApplicationImpl.ConvertToPixels((float)sheet.InnerGetRowHeight(shapeImpl.BottomRow, bRaiseEvents: false), MeasureUnits.Point));
		}
		if (num2 < sheet.GetColumnWidthInPixels(shapeImpl.RightColumn) && (!sheet.CopyShapesForSorting || shapeImpl.IsSizeWithCell))
		{
			shapeImpl.ClientAnchor.RightOffset = ConvertPixelsIntoWidthOffset(num2, sheet.GetColumnWidthInPixels(shapeImpl.RightColumn));
		}
		shapeImpl.Name = CollectionBaseEx<IShape>.GenerateDefaultName(m_shapes, shapeImpl.Name);
		return shapeImpl;
	}

	public void CopyFillOptions(ShapeImpl sourceShape, IDictionary dicFontIndexes)
	{
		if (sourceShape.m_lineFormat != null)
		{
			m_lineFormat = sourceShape.m_lineFormat.Clone(this);
		}
		if (sourceShape.m_fill != null)
		{
			m_fill = sourceShape.m_fill.Clone(this);
		}
	}

	public void PrepareForSerialization()
	{
		if (OldObjId == 0)
		{
			m_book.CurrentObjectId++;
			OldObjId = (uint)m_book.CurrentObjectId;
		}
		OnPrepareForSerialization();
		ShapeRecord.ShapeId = m_iShapeId;
	}

	protected virtual void OnPrepareForSerialization()
	{
		if (m_record is MsofbtSpContainer container)
		{
			SerializeMsoOptions(container);
		}
		if (m_object != null)
		{
			UpdateMacroInfo();
		}
	}

	private void UpdateMacroInfo()
	{
		List<ObjSubRecord> recordsList = m_object.RecordsList;
		ftMacro ftMacro = null;
		int i = 0;
		for (int count = recordsList.Count; i < count; i++)
		{
			ObjSubRecord objSubRecord = recordsList[i];
			if (objSubRecord.Type == TObjSubRecordType.ftMacro)
			{
				ftMacro = (ftMacro)objSubRecord;
				if (m_macroTokens == null)
				{
					recordsList.RemoveAt(i);
				}
				break;
			}
		}
		if (ftMacro == null && m_macroTokens != null)
		{
			ftMacro = new ftMacro();
			recordsList.Insert(recordsList.Count - 2, ftMacro);
		}
		if (m_macroTokens != null)
		{
			ftMacro.Tokens = m_macroTokens;
		}
	}

	internal void SetInstance(int instance)
	{
		ShapeRecord.Instance = instance;
	}

	public void SetOption(MsoOptions option, int value)
	{
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = option;
		fOPTE.Int32Value = value;
		ShapeOptions.AddOptionsOrReplace(fOPTE);
	}

	public void UpdateNamedRangeIndexes(int[] arrNewIndex)
	{
		if (m_macroTokens != null)
		{
			m_book.FormulaUtil.UpdateNameIndex(m_macroTokens, arrNewIndex);
		}
	}

	public void UpdateNamedRangeIndexes(IDictionary<int, int> dicNewIndex)
	{
		if (m_macroTokens != null)
		{
			m_book.FormulaUtil.UpdateNameIndex(m_macroTokens, dicNewIndex);
		}
	}

	private void UpdateLeftColumn()
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			ClientAnchor.LeftColumn = (int)m_rectAbsolute.Left;
			ClientAnchor.LeftOffset = 0;
			return;
		}
		int i = 0;
		int num;
		for (num = 0; (float)i <= m_rectAbsolute.Left; i += worksheet.GetColumnWidthInPixels(num))
		{
			num++;
			if (num > m_book.MaxColumnCount)
			{
				ClientAnchor.LeftColumn = m_book.MaxColumnCount - 1;
				ClientAnchor.LeftOffset = 1024;
				return;
			}
		}
		int columnWidthInPixels = worksheet.GetColumnWidthInPixels(num);
		i -= columnWidthInPixels;
		int iPixels = (int)m_rectAbsolute.Left - i;
		ClientAnchor.LeftColumn = num - 1;
		ClientAnchor.LeftOffset = ConvertPixelsIntoWidthOffset(iPixels, columnWidthInPixels);
	}

	internal void ClearShapeOffset(bool clear)
	{
		if (clear)
		{
			ClientAnchor.LeftOffset = 0;
			ClientAnchor.RightOffset = 0;
			ClientAnchor.BottomOffset = 0;
			ClientAnchor.TopOffset = 0;
		}
	}

	protected internal void UpdateRightColumn(int iCount)
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			m_rectAbsolute.X = ClientAnchor.LeftColumn;
			m_rectAbsolute.Y = m_rectAbsolute.Top;
			ClientAnchor.RightColumn = (int)(m_rectAbsolute.Left + m_rectAbsolute.Width);
			ClientAnchor.RightOffset = 0;
			return;
		}
		int num = Width;
		int num2 = LeftColumn;
		int num3 = LeftColumnOffset;
		while (num >= 0)
		{
			if (num2 > m_book.MaxColumnCount)
			{
				RightColumn = m_book.MaxColumnCount;
				RightColumnOffset = 1024;
				break;
			}
			int columnWidthInPixels = worksheet.GetColumnWidthInPixels(num2 + iCount);
			columnWidthInPixels = ((columnWidthInPixels > worksheet.GetColumnWidthInPixels(num2)) ? columnWidthInPixels : worksheet.GetColumnWidthInPixels(num2));
			int num4 = columnWidthInPixels - OffsetInPixels(num2, num3, isXOffset: true);
			if (num4 < 0)
			{
				num4 = 0;
			}
			if (num4 < 0)
			{
				throw new ArgumentOutOfRangeException("Calculated value can't be less than zero, error in coordinates update.");
			}
			if (num4 > num)
			{
				RightColumn = num2;
				RightColumnOffset = num3 + PixelsInOffset(num2, num, isXSize: true);
				break;
			}
			num -= num4;
			num2++;
			num3 = 0;
		}
	}

	protected internal void UpdateRightColumn()
	{
		UpdateRightColumn(0);
	}

	private void UpdateTopRow()
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			ClientAnchor.TopRow = (int)m_rectAbsolute.Y;
			ClientAnchor.TopOffset = 0;
			return;
		}
		int i = 0;
		int num;
		for (num = 0; (float)i <= m_rectAbsolute.Top; i += (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(num, bRaiseEvents: false), MeasureUnits.Point))
		{
			num++;
		}
		int num2 = (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(num, bRaiseEvents: false), MeasureUnits.Point);
		i -= num2;
		int iPixels = (int)(m_rectAbsolute.Top - (float)i);
		ClientAnchor.TopRow = num - 1;
		if (ClientAnchor.TopRow > 1048575)
		{
			ClientAnchor.TopRow = 1048575;
		}
		ClientAnchor.TopOffset = ConvertPixelsIntoHeightOffset(iPixels, num2);
	}

	protected internal void UpdateBottomRow()
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			m_rectAbsolute.X = m_rectAbsolute.Left;
			m_rectAbsolute.Y = ClientAnchor.TopRow;
			ClientAnchor.BottomRow = (int)(m_rectAbsolute.Top + m_rectAbsolute.Height);
			ClientAnchor.BottomOffset = 0;
			return;
		}
		int num = Height;
		int num2 = TopRow;
		int num3 = TopRowOffset;
		int maxRowCount = m_book.MaxRowCount;
		while (num >= 0)
		{
			if (num2 > maxRowCount)
			{
				BottomRow = maxRowCount;
				BottomRowOffset = 256;
				break;
			}
			int num4 = (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(num2, bRaiseEvents: false), MeasureUnits.Point);
			int num5 = num4 - OffsetInPixels((double)num4, num3, isXOffset: false);
			if (num5 < 0)
			{
				throw new ArgumentOutOfRangeException("Calculated value can't be less than zero, error in coordinates update.");
			}
			if (num5 > num)
			{
				BottomRow = num2;
				BottomRowOffset = num3 + PixelsInOffset((double)num4, num, isXSize: false);
				break;
			}
			num -= num5;
			num2++;
			num3 = 0;
		}
	}

	internal void UpdateAnchorPoints()
	{
		EvaluateTopPosition();
		EvaluateLeftPosition();
	}

	protected internal void UpdateWidth()
	{
		m_rectAbsolute.Width = GetWidth(LeftColumn, LeftColumnOffset, RightColumn, RightColumnOffset, bIsOffsetInPixels: false);
		m_shapeFrame.OffsetCX = (long)ApplicationImpl.ConvertFromPixel(m_rectAbsolute.Width, MeasureUnits.EMU);
	}

	protected internal void UpdateHeight()
	{
		m_rectAbsolute.Height = (int)GetHeight(TopRow, TopRowOffset, BottomRow, BottomRowOffset, bIsOffsetInPixels: false);
		m_shapeFrame.OffsetCY = (long)ApplicationImpl.ConvertFromPixel(m_rectAbsolute.Height, MeasureUnits.EMU);
	}

	internal int OffsetInPixels(int iRowColumn, int iOffset, bool isXOffset)
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			return 0;
		}
		if (isXOffset)
		{
			int columnWidthInPixels = worksheet.GetColumnWidthInPixels(iRowColumn);
			return OffsetInPixels((double)columnWidthInPixels, iOffset, isXOffset);
		}
		double num = ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(iRowColumn, bRaiseEvents: false), MeasureUnits.Point);
		return OffsetInPixels(num, iOffset, isXOffset);
	}

	internal int OffsetInPixels(double iWidthHeight, int iOffset, bool isXOffset)
	{
		if (m_shapes.Worksheet == null)
		{
			return 0;
		}
		double a = ((!isXOffset) ? ((double)iOffset * iWidthHeight / 256.0) : ((double)iOffset * iWidthHeight / 1024.0));
		return (int)Math.Round(a);
	}

	internal int PixelsInOffset(int iCurRowColumn, int iPixels, bool isXSize)
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			return 0;
		}
		if (iPixels < 0)
		{
			throw new ArgumentOutOfRangeException("IPixels", "Can't be less than zero.");
		}
		if (isXSize)
		{
			int columnWidthInPixels = worksheet.GetColumnWidthInPixels(iCurRowColumn);
			return PixelsInOffset((double)columnWidthInPixels, iPixels, isXSize);
		}
		int num = (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(iCurRowColumn, bRaiseEvents: false), MeasureUnits.Point);
		return PixelsInOffset((double)num, iPixels, isXSize);
	}

	internal int PixelsInOffset(double iWidthHeight, int iPixels, bool isXSize)
	{
		if (m_shapes.Worksheet == null)
		{
			return 0;
		}
		if (iPixels < 0)
		{
			throw new ArgumentOutOfRangeException("IPixels", "Can't be less than zero.");
		}
		if (isXSize)
		{
			return (int)((iWidthHeight != 0.0) ? ((double)(iPixels * 1024) / iWidthHeight) : iWidthHeight);
		}
		return (int)((iWidthHeight != 0.0) ? ((double)(iPixels * 256) / iWidthHeight) : 256.0);
	}

	internal int GetWidth(int iColumn1, int iOffset1, int iColumn2, int iOffset2, bool bIsOffsetInPixels)
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			return iColumn2 - iColumn1;
		}
		if (iColumn1 < 1 || iColumn1 > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumn1");
		}
		if (iOffset2 == 0)
		{
			iColumn2--;
			iOffset2 = 1024;
		}
		if (iColumn1 > iColumn2)
		{
			return 0;
		}
		if (iColumn2 < 1 || iColumn2 > m_book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumn2");
		}
		if (iOffset1 < 0)
		{
			throw new ArgumentOutOfRangeException("iOffset1");
		}
		if (iOffset2 < 0)
		{
			throw new ArgumentOutOfRangeException("iOffset2");
		}
		if (iColumn1 == iColumn2 && iOffset1 > iOffset2)
		{
			return 0;
		}
		int columnWidthInPixels = worksheet.GetColumnWidthInPixels(iColumn1);
		int columnWidthInPixels2 = worksheet.GetColumnWidthInPixels(iColumn2);
		int num = ConvertWidthOffsetIntoPixels(columnWidthInPixels, Math.Min(iOffset1, 1024), bIsOffsetInPixels);
		int num2 = ConvertWidthOffsetIntoPixels(columnWidthInPixels2, Math.Min(iOffset2, 1024), bIsOffsetInPixels) - num;
		for (int i = iColumn1; i < iColumn2; i++)
		{
			num2 += worksheet.GetColumnWidthInPixels(i);
		}
		return num2;
	}

	internal float GetHeight(int iRow1, int iOffset1, int iRow2, int iOffset2, bool bIsOffsetInPixels)
	{
		WorksheetImpl worksheet = m_shapes.Worksheet;
		if (worksheet == null)
		{
			return iRow2 - iRow1;
		}
		if (iRow1 < 1 || iRow1 > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRow1");
		}
		if (iRow2 < 1 || iRow2 > m_book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRow2");
		}
		if (iRow2 < iRow1)
		{
			return 0f;
		}
		if (iRow1 == iRow2 && iOffset1 > iOffset2)
		{
			return 0f;
		}
		int iRowHeight = (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(iRow1, bRaiseEvents: false), MeasureUnits.Point);
		int iRowHeight2 = (int)ApplicationImpl.ConvertToPixels((float)worksheet.InnerGetRowHeight(iRow2, bRaiseEvents: false), MeasureUnits.Point);
		int num = ConvertHeightOffsetIntoPixels(iRowHeight, iOffset1, bIsOffsetInPixels);
		return (float)(ConvertHeightOffsetIntoPixels(iRowHeight2, iOffset2, bIsOffsetInPixels) - num) + (float)(worksheet.RowHeightHelper.GetTotal(iRow2 - 1) - worksheet.RowHeightHelper.GetTotal(iRow1 - 1));
	}

	internal static int ConvertWidthOffsetIntoPixels(int iColWidth, int iOffset, bool bIsInPixels)
	{
		int result = iOffset;
		if (!bIsInPixels)
		{
			result = (int)Math.Round((double)(iOffset * iColWidth) / 1024.0);
		}
		return result;
	}

	internal static int ConvertHeightOffsetIntoPixels(int iRowHeight, int iOffset, bool bIsOffsetInPixels)
	{
		int result = iOffset;
		if (!bIsOffsetInPixels)
		{
			result = (int)Math.Round((double)(iOffset * iRowHeight) / 256.0);
		}
		return result;
	}

	internal static int ConvertPixelsIntoWidthOffset(int iPixels, int iColWidth)
	{
		if (iColWidth == 0)
		{
			return 0;
		}
		return iPixels * 1024 / iColWidth;
	}

	internal static int ConvertPixelsIntoHeightOffset(int iPixels, int iRowHeight)
	{
		if (iPixels < 0 || iPixels > iRowHeight)
		{
			throw new ArgumentOutOfRangeException("iPixels");
		}
		return iPixels * 256 / iRowHeight;
	}

	public void EvaluateTopLeftPosition()
	{
		EvaluateLeftPosition();
		EvaluateTopPosition();
	}

	private void EvaluateLeftPosition()
	{
		m_rectAbsolute.X = GetWidth(1, 0, LeftColumn, LeftColumnOffset, bIsOffsetInPixels: false);
		m_shapeFrame.OffsetX = (long)ApplicationImpl.ConvertFromPixel(m_rectAbsolute.X, MeasureUnits.EMU);
	}

	private void EvaluateRightPosition()
	{
		m_rectAbsolute.Y = (int)GetHeight(1, 0, TopRow, TopRowOffset, bIsOffsetInPixels: false);
	}

	private void EvaluateTopPosition()
	{
		m_rectAbsolute.Y = (int)GetHeight(1, 0, TopRow, TopRowOffset, bIsOffsetInPixels: false);
		m_shapeFrame.OffsetY = (long)ApplicationImpl.ConvertFromPixel(m_rectAbsolute.Y, MeasureUnits.EMU);
	}

	[CLSCompliant(false)]
	protected void SetClientAnchor(MsofbtClientAnchor anchor)
	{
		if (anchor == null)
		{
			throw new ArgumentOutOfRangeException("anchor");
		}
		m_clientAnchor = anchor;
	}

	private void OnLeftColumnChange()
	{
		if (IsSizeWithCell)
		{
			UpdateWidth();
		}
		else
		{
			UpdateRightColumn();
		}
	}

	private void OnTopRowChanged()
	{
		EvaluateTopPosition();
		if (IsSizeWithCell)
		{
			UpdateHeight();
		}
		else
		{
			UpdateBottomRow();
		}
	}

	private bool IsIndexLess(int iRowColumnIndex, bool bIsRow)
	{
		if (!bIsRow)
		{
			return iRowColumnIndex <= LeftColumn;
		}
		return iRowColumnIndex <= TopRow;
	}

	private bool IsIndexMiddle(int iRowColumnIndex, bool bIsRow)
	{
		if (!bIsRow)
		{
			return iRowColumnIndex <= RightColumn;
		}
		return iRowColumnIndex <= BottomRow;
	}

	private bool IsIndexLast(int iRowColumnIndex, bool bIsRow)
	{
		throw new NotImplementedException();
	}

	private void IncreaseAndUpdateAll(int iCount, bool bIsRow)
	{
		if (bIsRow)
		{
			if (ClientAnchor.TopRow + iCount >= 0)
			{
				ClientAnchor.TopRow += iCount;
			}
			else
			{
				ClientAnchor.TopOffset = 0;
			}
			ClientAnchor.BottomRow += iCount;
			EvaluateTopPosition();
		}
		else
		{
			if (ClientAnchor.LeftColumn + iCount >= 0)
			{
				ClientAnchor.LeftColumn += iCount;
			}
			else
			{
				ClientAnchor.LeftOffset = 0;
			}
			ClientAnchor.RightColumn += iCount;
			EvaluateLeftPosition();
		}
	}

	private void IncreaseAndUpdateEnd(int iCount, bool bIsRow)
	{
		if (bIsRow)
		{
			ClientAnchor.BottomRow += iCount;
			UpdateHeight();
		}
		else
		{
			ClientAnchor.RightColumn += iCount;
			UpdateWidth();
		}
	}

	private int GetCountAbove(int iIndex, int iCount, bool bIsRow)
	{
		int num = (bIsRow ? TopRow : LeftColumn);
		if (iIndex < num)
		{
			return Math.Min(iCount, num - iIndex);
		}
		return 0;
	}

	private int GetCountInside(int iIndex, int iCount, bool bIsRow)
	{
		int num = (bIsRow ? (BottomRow - 1) : (RightColumn - 1));
		if (iIndex > num)
		{
			return 0;
		}
		int val = (bIsRow ? (TopRow + 1) : (LeftColumn + 1));
		int val2 = iIndex + iCount - 1;
		int num2 = Math.Max(val, iIndex) - Math.Min(num, val2) + 1;
		if (num2 <= 0)
		{
			return 0;
		}
		return num2;
	}

	private bool IndicatesFirst(int iIndex, int iCount, bool bIsRow)
	{
		int num = (bIsRow ? TopRow : LeftColumn);
		int num2 = (bIsRow ? BottomRow : RightColumn);
		if (num == num2)
		{
			return false;
		}
		num -= iIndex;
		if (num >= 0)
		{
			return num < iCount;
		}
		return false;
	}

	private void UpdateAboveRowColumnIndexes(int iCount, bool bIsRow)
	{
		if (bIsRow)
		{
			ClientAnchor.TopRow += iCount;
			ClientAnchor.BottomRow += iCount;
			EvaluateTopPosition();
		}
		else
		{
			ClientAnchor.LeftColumn += iCount;
			ClientAnchor.RightColumn += iCount;
			EvaluateLeftPosition();
		}
	}

	private void UpdateFirstRowColumnIndexes(bool bIsRow, int iCount)
	{
		if (bIsRow)
		{
			ClientAnchor.TopOffset = 0;
			ClientAnchor.BottomRow += iCount;
			int width = Width;
			EvaluateTopPosition();
			if (!IsSizeWithCell)
			{
				Width = width;
			}
		}
		else
		{
			ClientAnchor.LeftOffset = 0;
			ClientAnchor.RightColumn += iCount;
			int height = Height;
			EvaluateLeftPosition();
			if (!IsSizeWithCell)
			{
				Height = height;
			}
		}
		if (!IsSizeWithCell)
		{
			UpdateNotSizeNotMoveShape(bIsRow, 0, iCount);
		}
	}

	protected virtual void UpdateNotSizeNotMoveShape(bool bRow, int iIndex, int iCount)
	{
		if (bRow)
		{
			UpdateTopRow();
			UpdateBottomRow();
		}
		else
		{
			UpdateLeftColumn();
			UpdateRightColumn();
		}
	}

	private void UpdateInsideRowColumnIndexes(int iCount, bool bRow)
	{
		if (IsSizeWithCell)
		{
			if (bRow)
			{
				ClientAnchor.BottomRow += iCount;
				EvaluateTopPosition();
			}
			else
			{
				ClientAnchor.RightColumn += iCount;
				EvaluateRightPosition();
			}
		}
		else if (!(Worksheet as WorksheetImpl).CopyShapesForSorting)
		{
			UpdateNotSizeNotMoveShape(bRow, 0, iCount);
		}
	}

	private void UpdateLastRowColumnIndex(bool bRow)
	{
		if (IsSizeWithCell)
		{
			if (bRow)
			{
				ClientAnchor.BottomOffset = 0;
				EvaluateTopPosition();
			}
			else
			{
				ClientAnchor.RightOffset = 0;
				EvaluateLeftPosition();
			}
		}
		else if (!(Worksheet as WorksheetImpl).CopyShapesForSorting)
		{
			UpdateNotSizeNotMoveShape(bRow, 0, 1);
		}
	}

	private void UpdateRecord(MsofbtClientAnchor anchor)
	{
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		if (!(m_record is MsofbtSpContainer msofbtSpContainer))
		{
			m_clientAnchor = (MsofbtClientAnchor)anchor.Clone();
			return;
		}
		IList itemsList = msofbtSpContainer.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			MsoBase mso = itemsList[i] as MsoBase;
			UpdateMso(mso);
		}
	}

	private void ParseLineFill(MsofbtOPT options)
	{
		m_fill = new ShapeFillImpl(base.Application, this);
		m_fill.Visible = false;
		m_lineFormat = new ShapeLineFormatImpl(base.Application, this);
		m_lineFormat.Visible = false;
		m_bUpdateLineFill = true;
		if (options != null)
		{
			ParseOptions(options);
		}
	}

	[CLSCompliant(false)]
	protected virtual bool UpdateMso(MsoBase mso)
	{
		if (mso == null)
		{
			throw new ArgumentNullException("mso");
		}
		if (mso is MsofbtClientAnchor)
		{
			m_clientAnchor = mso as MsofbtClientAnchor;
			return true;
		}
		if (mso is MsofbtClientData)
		{
			m_object = (mso as MsofbtClientData).ObjectRecord;
			return true;
		}
		if (mso is MsofbtOPT)
		{
			m_options = mso as MsofbtOPT;
			return true;
		}
		if (mso is MsofbtSp)
		{
			m_shape = mso as MsofbtSp;
			return true;
		}
		return false;
	}

	protected void CloneLineFill(ShapeImpl sourceShape)
	{
		if (sourceShape == null)
		{
			throw new ArgumentNullException("sourceShape");
		}
		if (m_bUpdateLineFill)
		{
			if (sourceShape.m_fill != null)
			{
				m_fill = sourceShape.m_fill.Clone(this);
			}
			if (sourceShape.m_lineFormat != null)
			{
				m_lineFormat = sourceShape.m_lineFormat.Clone(this);
			}
		}
	}

	private void CodeName_Changed(object sender, ValueChangedEventArgs e)
	{
		string[] array = OnAction.Split('.');
		if (array.Length > 1)
		{
			string[] array2 = array[0].Split('!');
			if (array2.Length > 1)
			{
				array[0] = array2[1];
			}
			if (array[0] == e.oldValue.ToString())
			{
				array[0] = e.newValue.ToString();
			}
			OnAction = array[0] + "." + array[1];
		}
	}

	private void Worksheet_ColumnWidthChanged(object sender, ValueChangedEventArgs e)
	{
		bool flag = m_clientAnchor == null || (m_clientAnchor.LeftOffset == m_clientAnchor.RightOffset && m_clientAnchor.RightOffset == m_clientAnchor.TopOffset && m_clientAnchor.TopOffset == m_clientAnchor.BottomOffset && m_clientAnchor.TopOffset == 0 && m_clientAnchor.LeftColumn == m_clientAnchor.RightColumn && m_clientAnchor.TopRow == m_clientAnchor.BottomRow);
		if (m_book.IsWorkbookOpening || flag)
		{
			return;
		}
		int num = (int)e.oldValue;
		if (num > RightColumn)
		{
			return;
		}
		if (num < LeftColumn)
		{
			if (IsMoveWithCell)
			{
				EvaluateLeftPosition();
				return;
			}
			UpdateLeftColumn();
			UpdateRightColumn();
		}
		else if (num == LeftColumn)
		{
			if (IsSizeWithCell)
			{
				UpdateLeftColumn();
				UpdateWidth();
			}
			else
			{
				UpdateLeftColumn();
				UpdateRightColumn();
			}
		}
		else if (num == RightColumn)
		{
			if (IsSizeWithCell)
			{
				LeaveRelativeBottomRightCorner();
			}
			else
			{
				UpdateRightColumn();
			}
		}
		else if (IsSizeWithCell)
		{
			UpdateWidth();
		}
		else
		{
			UpdateRightColumn();
		}
	}

	private void LeaveRelativeBottomRightCorner()
	{
		if (!m_book.IsWorkbookOpening)
		{
			WorksheetImpl worksheet = m_shapes.Worksheet;
			if (worksheet != null)
			{
				int width = GetWidth(LeftColumn, LeftColumnOffset, RightColumn, RightColumnOffset, bIsOffsetInPixels: false);
				int columnWidthInPixels = worksheet.GetColumnWidthInPixels(RightColumn);
				int num = RightColumnOffset + ConvertPixelsIntoWidthOffset(Math.Min(Width - width, columnWidthInPixels), columnWidthInPixels);
				RightColumnOffset = ((num >= 0) ? num : 0);
				UpdateWidth();
			}
		}
	}

	private void NormalFont_OnAfterChange(object sender, EventArgs e)
	{
		if (!m_book.IsWorkbookOpening)
		{
			if (!IsMoveWithCell)
			{
				UpdateLeftColumn();
				UpdateTopRow();
			}
			else
			{
				EvaluateTopLeftPosition();
			}
			if (!IsSizeWithCell)
			{
				UpdateRightColumn();
				UpdateBottomRow();
			}
			else
			{
				UpdateWidth();
				UpdateHeight();
			}
		}
	}

	private void Worksheet_RowHeightChanged(object sender, ValueChangedEventArgs e)
	{
		int num = (int)e.oldValue;
		if (num > BottomRow)
		{
			return;
		}
		if (num < TopRow)
		{
			if (IsMoveWithCell)
			{
				EvaluateTopPosition();
				return;
			}
			UpdateTopRow();
			UpdateBottomRow();
		}
		else if (num == TopRow)
		{
			if (IsSizeWithCell)
			{
				UpdateTopRow();
				UpdateHeight();
			}
			else
			{
				UpdateTopRow();
				UpdateBottomRow();
			}
		}
		else if (num == BottomRow)
		{
			if (IsSizeWithCell)
			{
				LeaveRelativeBottomRightCorner();
			}
			else
			{
				UpdateBottomRow();
			}
		}
		else if (IsSizeWithCell)
		{
			UpdateHeight();
		}
		else
		{
			UpdateBottomRow();
		}
	}

	internal void CheckLeftOffset()
	{
		int leftOffset = ClientAnchor.LeftOffset;
		int num = ClientAnchor.LeftColumn + 1;
		int columnWidthInPixels = m_shapes.Worksheet.GetColumnWidthInPixels(num);
		int num2 = OffsetInPixels(num, leftOffset, isXOffset: true);
		if (columnWidthInPixels < num2)
		{
			ClientAnchor.LeftColumn++;
			ClientAnchor.LeftOffset = num2 - num;
		}
	}

	internal void UpdateGroupFrame(bool isAll)
	{
		if (Group != null)
		{
			UpdateGroupPositions(out var left, out var top, out var width, out var height);
			int rotation = ShapeFrame.Rotation;
			if (!isAll || !IsGroup)
			{
				RectangleF shapeRect = new RectangleF((float)ApplicationImpl.ConvertToPixels(left, MeasureUnits.EMU), (float)ApplicationImpl.ConvertToPixels(top, MeasureUnits.EMU), (float)ApplicationImpl.ConvertToPixels(width, MeasureUnits.EMU), (float)ApplicationImpl.ConvertToPixels(height, MeasureUnits.EMU));
				RectangleF updatedRectangle = GetUpdatedRectangle(this, shapeRect);
				shapeRect.X = updatedRectangle.X;
				shapeRect.Y = updatedRectangle.Y;
				left = (long)ApplicationImpl.ConvertFromPixel(shapeRect.Left, MeasureUnits.EMU);
				top = (long)ApplicationImpl.ConvertFromPixel(shapeRect.Top, MeasureUnits.EMU);
				rotation = GetShapeRotation() * 60000;
			}
			m_groupFrame.SetAnchor(rotation, left, top, width, height);
		}
	}

	internal void UpdateGroupFrame()
	{
		if (Group != null)
		{
			UpdateGroupPositions(out var left, out var top, out var width, out var height);
			m_groupFrame.SetAnchor(ShapeFrame.Rotation, left, top, width, height);
		}
	}

	private void UpdateGroupPositions(out long left, out long top, out long width, out long height)
	{
		m_groupFrame = new ShapeFrame(this);
		ShapeFrame shapeFrame = Group.GroupFrame ?? Group.ShapeFrame;
		double num = Math.Round((double)shapeFrame.OffsetCX / (double)shapeFrame.ChOffsetCX, 4);
		double num2 = Math.Round((double)shapeFrame.OffsetCY / (double)shapeFrame.ChOffsetCY, 4);
		left = (long)Math.Round((double)shapeFrame.OffsetX + (double)(ShapeFrame.OffsetX - shapeFrame.ChOffsetX) * num);
		top = (long)Math.Round((double)shapeFrame.OffsetY + (double)(ShapeFrame.OffsetY - shapeFrame.ChOffsetY) * num2);
		width = ShapeFrame.OffsetCX;
		height = ShapeFrame.OffsetCY;
		int num3 = ((ShapeFrame.Rotation != -1) ? Math.Abs(ShapeFrame.Rotation % 21600000) : 0);
		if ((num3 >= 2700000 && num3 <= 8099999) || (num3 >= 13500000 && num3 <= 18899999))
		{
			double num4 = num;
			num = num2;
			num2 = num4;
			left = (long)((double)left - (double)width * (num - num2) / 2.0);
			top = (long)((double)top - (double)height * (num2 - num) / 2.0);
		}
		width = (long)Math.Round((double)width * num);
		height = (long)Math.Round((double)height * num2);
		m_groupFrame.SetChildAnchor(ShapeFrame.ChOffsetX, ShapeFrame.ChOffsetY, ShapeFrame.ChOffsetCX, ShapeFrame.ChOffsetCY);
	}

	private RectangleF GetChildShapePositionToDraw(RectangleF groupShapeBounds, float groupShapeRotation, RectangleF childShapeBounds)
	{
		double num = groupShapeBounds.X + groupShapeBounds.Width / 2f;
		double num2 = groupShapeBounds.Y + groupShapeBounds.Height / 2f;
		if (groupShapeRotation > 360f)
		{
			groupShapeRotation %= 360f;
		}
		double num3 = (double)groupShapeRotation * Math.PI / 180.0;
		double num4 = Math.Sin(num3);
		double num5 = Math.Cos(num3);
		double num6 = childShapeBounds.X + childShapeBounds.Width / 2f;
		double num7 = childShapeBounds.Y + childShapeBounds.Height / 2f;
		double num8 = num + ((double)childShapeBounds.X - num) * num5 - ((double)childShapeBounds.Y - num2) * num4;
		double num9 = num2 + ((double)childShapeBounds.X - num) * num4 + ((double)childShapeBounds.Y - num2) * num5;
		double num10 = num + (num6 - num) * num5 - (num7 - num2) * num4;
		double num11 = num2 + (num6 - num) * num4 + (num7 - num2) * num5;
		double num12 = (double)(360f - groupShapeRotation) * Math.PI / 180.0;
		num4 = Math.Sin(num12);
		num5 = Math.Cos(num12);
		double num13 = num10 + (num8 - num10) * num5 - (num9 - num11) * num4;
		double num14 = num11 + (num8 - num10) * num4 + (num9 - num11) * num5;
		return new RectangleF((float)num13, (float)num14, childShapeBounds.Width, childShapeBounds.Height);
	}

	internal int GetShapeRotation()
	{
		int num = ShapeRotation;
		for (GroupShapeImpl group = Group; group != null; group = group.Group)
		{
			int num2 = group.ShapeRotation;
			if (group.FlipVertical ^ group.FlipHorizontal)
			{
				num2 = 360 - num2;
			}
			num = (num + num2) % 360;
			if (group.FlipVertical ^ group.FlipHorizontal)
			{
				num = 360 - num;
			}
		}
		return num;
	}

	private RectangleF GetUpdatedRectangle(ShapeImpl shape, RectangleF shapeRect)
	{
		GroupShapeImpl group = shape.Group;
		_ = shape.ShapeRotation;
		while (group != null)
		{
			float x;
			float y;
			float width;
			float height;
			if (group.GroupFrame != null)
			{
				x = (float)ApplicationImpl.ConvertToPixels(group.GroupFrame.OffsetX, MeasureUnits.EMU);
				y = (float)ApplicationImpl.ConvertToPixels(group.GroupFrame.OffsetY, MeasureUnits.EMU);
				width = (float)ApplicationImpl.ConvertToPixels(group.GroupFrame.OffsetCX, MeasureUnits.EMU);
				height = (float)ApplicationImpl.ConvertToPixels(group.GroupFrame.OffsetCY, MeasureUnits.EMU);
			}
			else
			{
				x = (float)ApplicationImpl.ConvertToPixels(group.ShapeFrame.OffsetX, MeasureUnits.EMU);
				y = (float)ApplicationImpl.ConvertToPixels(group.ShapeFrame.OffsetY, MeasureUnits.EMU);
				width = (float)ApplicationImpl.ConvertToPixels(group.ShapeFrame.OffsetCX, MeasureUnits.EMU);
				height = (float)ApplicationImpl.ConvertToPixels(group.ShapeFrame.OffsetCY, MeasureUnits.EMU);
			}
			RectangleF rectangleF = new RectangleF(x, y, width, height);
			shapeRect = GetChildShapePositionToDraw(rectangleF, (group.FlipVertical ^ group.FlipHorizontal) ? (360 - group.ShapeRotation) : group.ShapeRotation, shapeRect);
			PointF[] points = new PointF[4]
			{
				shapeRect.Location,
				new PointF(shapeRect.X + shapeRect.Width, shapeRect.Y),
				new PointF(shapeRect.Right, shapeRect.Bottom),
				new PointF(shapeRect.X, shapeRect.Y + shapeRect.Height)
			};
			GetTransformMatrix(rectangleF, group.FlipVertical, group.FlipHorizontal).TransformPoints(points);
			shapeRect = CreateRect(points);
			_ = group.ShapeRotation;
			group = group.Group;
		}
		return shapeRect;
	}

	private static RectangleF CreateRect(PointF[] points)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		int num5 = points.Length;
		for (int i = 0; i < num5; i++)
		{
			float x = points[i].X;
			float y = points[i].Y;
			if (x < num)
			{
				num = x;
			}
			if (x > num3)
			{
				num3 = x;
			}
			if (y < num2)
			{
				num2 = y;
			}
			if (y > num4)
			{
				num4 = y;
			}
		}
		return new RectangleF(num, num2, num3 - num, num4 - num2);
	}

	private Matrix GetTransformMatrix(RectangleF bounds, bool flipV, bool flipH)
	{
		Matrix matrix = new Matrix();
		PointF pointF = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
		Matrix target = new Matrix(1f, 0f, 0f, -1f, 0f, 0f);
		Matrix target2 = new Matrix(-1f, 0f, 0f, 1f, 0f, 0f);
		if (flipV)
		{
			MatrixMultiply(matrix, target, MatrixOrder.Append);
			MatrixTranslate(matrix, 0f, pointF.Y * 2f, MatrixOrder.Append);
		}
		if (flipH)
		{
			MatrixMultiply(matrix, target2, MatrixOrder.Append);
			MatrixTranslate(matrix, pointF.X * 2f, 0f, MatrixOrder.Append);
		}
		return matrix;
	}

	private void MatrixTranslate(Matrix matrix, float x, float y, MatrixOrder matrixOrder)
	{
	}

	private void MatrixMultiply(Matrix matrix, Matrix target, MatrixOrder matrixOrder)
	{
	}

	private Matrix GetTransformMatrix(RectangleF bounds, float ang, bool flipV, bool flipH)
	{
		Matrix transformMatrix = GetTransformMatrix(bounds, flipV, flipH);
		new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
		return transformMatrix;
	}

	internal RectangleF UpdateShapeBounds(RectangleF rect, int rotation)
	{
		if ((rotation > 44 && rotation < 135) || (rotation > 224 && rotation < 315))
		{
			PointF[] array = new PointF[3]
			{
				new PointF(rect.X, rect.Y),
				new PointF(rect.Right, rect.Y),
				new PointF(rect.X, rect.Bottom)
			};
			GetTransformMatrix(rect, -90f, flipV: false, flipH: false).TransformPoints(array);
			rect = new RectangleF(array[1].X, array[1].Y, rect.Height, rect.Width);
		}
		return rect;
	}

	internal void SetPostion(long offsetX, long offsetY, long offsetCX, long offsetCY)
	{
		LeftDouble = ((offsetX >= 0) ? ApplicationImpl.ConvertToPixels(offsetX, MeasureUnits.EMU) : 0.0);
		TopDouble = ((offsetY >= 0) ? ApplicationImpl.ConvertToPixels(offsetY, MeasureUnits.EMU) : 0.0);
		WidthDouble = ((offsetCX >= 0) ? ApplicationImpl.ConvertToPixels(offsetCX, MeasureUnits.EMU) : 0.0);
		HeightDouble = ((offsetCY >= 0) ? ApplicationImpl.ConvertToPixels(offsetCY, MeasureUnits.EMU) : 0.0);
	}

	private void SetLeftPosition(double value)
	{
		if ((double)m_rectAbsolute.X != value)
		{
			m_rectAbsolute.X = (float)value;
			m_shapeFrame.OffsetX = (long)ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU);
			UpdateLeftColumn();
			UpdateRightColumn();
		}
	}

	private void SetTopPosition(double value)
	{
		if ((double)m_rectAbsolute.Y != value)
		{
			m_rectAbsolute.Y = (float)value;
			m_shapeFrame.OffsetY = (long)ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU);
			UpdateTopRow();
			UpdateBottomRow();
		}
	}

	private void SetWidth(double value)
	{
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException("Width");
		}
		if ((double)m_rectAbsolute.Width != value)
		{
			m_rectAbsolute.Width = (float)value;
			m_shapeFrame.OffsetCX = (long)ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU);
			UpdateRightColumn();
		}
	}

	private void SetHeight(double value)
	{
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException("Height");
		}
		if ((double)m_rectAbsolute.Height != value)
		{
			m_rectAbsolute.Height = (float)value;
			m_shapeFrame.OffsetCY = (long)ApplicationImpl.ConvertFromPixel(value, MeasureUnits.EMU);
			UpdateBottomRow();
		}
	}

	internal void SetInnerShapes(object value, string property)
	{
		IShape[] items = (this as GroupShapeImpl).Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = items[i] as ShapeImpl;
			switch (property)
			{
			case "IsShapeVisible":
				shapeImpl.IsShapeVisible = (bool)value;
				break;
			case "IsMoveWithCell":
				shapeImpl.IsMoveWithCell = (bool)value;
				break;
			case "IsSizeWithCell":
				shapeImpl.IsSizeWithCell = (bool)value;
				break;
			}
		}
	}
}

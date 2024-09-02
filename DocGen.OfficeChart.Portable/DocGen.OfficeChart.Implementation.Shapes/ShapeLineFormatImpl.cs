using System;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class ShapeLineFormatImpl : CommonObject, IShapeLineFormat
{
	private const double DEF_LINE_WEIGHT_MULL = 12700.0;

	private const int DEF_LINE_MAX_WEIGHT = 1584;

	private const int DEF_PARSE_ARR_LENGTH = 5088;

	private static byte[] m_parsePattArray;

	private double m_weight = 0.75;

	private Color m_foreColor = ColorExtension.Black;

	private Color m_backColor = ColorExtension.White;

	private WorkbookImpl m_book;

	private OfficeShapeArrowStyle m_beginArrowStyle;

	private OfficeShapeArrowStyle m_endArrowStyle;

	private OfficeShapeArrowLength m_beginArrowLength = OfficeShapeArrowLength.ArrowHeadMedium;

	private OfficeShapeArrowLength m_endArrowLength = OfficeShapeArrowLength.ArrowHeadMedium;

	private OfficeShapeArrowWidth m_beginArrowWidth = OfficeShapeArrowWidth.ArrowHeadMedium;

	private OfficeShapeArrowWidth m_endArrowWidth = OfficeShapeArrowWidth.ArrowHeadMedium;

	private OfficeShapeDashLineStyle m_dashStyle;

	private OfficeShapeLineStyle m_style = OfficeShapeLineStyle.Line_Single;

	private double m_transparency;

	private bool m_visible;

	private OfficeGradientPattern m_pattern = OfficeGradientPattern.Pat_5_Percent;

	private bool m_bContainPattern;

	private bool m_bRound;

	private PreservationLogger m_logger;

	private bool m_isNoFill;

	private bool m_isSolidFill;

	private int m_DefaultLineStyleIndex = -1;

	public double Weight
	{
		get
		{
			return m_weight;
		}
		set
		{
			if (value < 0.0 || value > 1584.0)
			{
				throw new ArgumentOutOfRangeException("Weight");
			}
			m_weight = value;
			Visible = true;
		}
	}

	public Color ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			m_foreColor = value;
			Visible = true;
		}
	}

	public Color BackColor
	{
		get
		{
			return m_backColor;
		}
		set
		{
			m_backColor = value;
			Visible = true;
		}
	}

	public OfficeKnownColors ForeColorIndex
	{
		get
		{
			return m_book.GetNearestColor(m_foreColor);
		}
		set
		{
			ForeColor = m_book.GetPaletteColor(value);
		}
	}

	public OfficeKnownColors BackColorIndex
	{
		get
		{
			return m_book.GetNearestColor(m_backColor);
		}
		set
		{
			BackColor = m_book.GetPaletteColor(value);
		}
	}

	public OfficeShapeArrowStyle BeginArrowHeadStyle
	{
		get
		{
			return m_beginArrowStyle;
		}
		set
		{
			m_beginArrowStyle = value;
			Visible = true;
		}
	}

	public OfficeShapeArrowStyle EndArrowHeadStyle
	{
		get
		{
			return m_endArrowStyle;
		}
		set
		{
			m_endArrowStyle = value;
			Visible = true;
		}
	}

	public OfficeShapeArrowLength BeginArrowheadLength
	{
		get
		{
			return m_beginArrowLength;
		}
		set
		{
			m_beginArrowLength = value;
			Visible = true;
		}
	}

	public OfficeShapeArrowLength EndArrowheadLength
	{
		get
		{
			return m_endArrowLength;
		}
		set
		{
			m_endArrowLength = value;
			Visible = true;
		}
	}

	public OfficeShapeArrowWidth BeginArrowheadWidth
	{
		get
		{
			return m_beginArrowWidth;
		}
		set
		{
			m_beginArrowWidth = value;
			Visible = true;
		}
	}

	public OfficeShapeArrowWidth EndArrowheadWidth
	{
		get
		{
			return m_endArrowWidth;
		}
		set
		{
			m_endArrowWidth = value;
			Visible = true;
		}
	}

	public OfficeShapeDashLineStyle DashStyle
	{
		get
		{
			return m_dashStyle;
		}
		set
		{
			m_dashStyle = value;
			Visible = true;
		}
	}

	public OfficeShapeLineStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
			Visible = true;
		}
	}

	public double Transparency
	{
		get
		{
			return m_transparency;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			m_transparency = value;
			Visible = true;
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
			m_logger.SetFlag(PreservedFlag.Line);
		}
	}

	public OfficeGradientPattern Pattern
	{
		get
		{
			if (!m_bContainPattern)
			{
				throw new NotSupportedException("Doesn't checked patterned style.");
			}
			return m_pattern;
		}
		set
		{
			m_pattern = value;
			HasPattern = true;
			Visible = true;
		}
	}

	public bool HasPattern
	{
		get
		{
			return m_bContainPattern;
		}
		set
		{
			if (HasPattern != value)
			{
				m_bContainPattern = value;
				Visible = true;
			}
		}
	}

	internal bool IsNoFill
	{
		get
		{
			return m_isNoFill;
		}
		set
		{
			m_isNoFill = value;
		}
	}

	internal bool IsSolidFill
	{
		get
		{
			return m_isSolidFill;
		}
		set
		{
			m_isSolidFill = value;
		}
	}

	internal int DefaultLineStyleIndex
	{
		get
		{
			return m_DefaultLineStyleIndex;
		}
		set
		{
			m_DefaultLineStyleIndex = value;
		}
	}

	public WorkbookImpl Workbook => m_book;

	public bool IsRound
	{
		get
		{
			return m_bRound;
		}
		set
		{
			m_bRound = value;
		}
	}

	static ShapeLineFormatImpl()
	{
		m_parsePattArray = new byte[5088];
		int num = 0;
		ResourceHandler resourceHandler = new ResourceHandler();
		for (int i = 1; i < 49; i++)
		{
			byte[] array = resourceHandler.PatternArray["Patt" + i];
			m_parsePattArray[num] = (byte)array.Length;
			num++;
			array.CopyTo(m_parsePattArray, num);
			num += array.Length;
		}
	}

	public ShapeLineFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		m_logger = new PreservationLogger();
	}

	internal ShapeLineFormatImpl(IApplication application, object parent, PreservationLogger logger)
		: base(application, parent)
	{
		FindParents();
		m_logger = logger;
	}

	private void FindParents()
	{
		m_book = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		if (m_book == null)
		{
			throw new ApplicationException("Cann't find parent object.");
		}
	}

	internal static double ParseTransparency(uint value)
	{
		return (65500.0 - (double)value) / 65500.0;
	}

	internal static void SerializeTransparency(IFopteOptionWrapper opt, MsoOptions id, double value)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int value2 = (int)((100.0 - value * 100.0) * 655.0);
		ShapeImpl.SerializeForte(opt, id, value2);
	}

	internal static void SerializeColor(IFopteOptionWrapper opt, ChartColor color, WorkbookImpl book, MsoOptions id)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		bool flag = true;
		byte[] arr;
		if (color.ColorType == ColorType.Indexed)
		{
			arr = new byte[4]
			{
				(byte)color.Value,
				0,
				0,
				8
			};
		}
		else
		{
			Color rGB = color.GetRGB(book);
			arr = new byte[4] { rGB.R, rGB.G, rGB.B, 2 };
			if (rGB.A == 0 && rGB.R == 0 && rGB.G == 0 && rGB.B == 0)
			{
				flag = false;
			}
		}
		if (flag)
		{
			ShapeImpl.SerializeForte(opt, id, arr);
		}
	}

	[CLSCompliant(false)]
	public bool ParseOption(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		if (ParseArrowsPropertys(option))
		{
			return true;
		}
		switch (option.Id)
		{
		case MsoOptions.NoLineDrawDash:
			ParseVisible(option.MainData);
			return true;
		case MsoOptions.LineStyle:
			m_style = (OfficeShapeLineStyle)(option.UInt32Value + 1);
			return true;
		case MsoOptions.LineWeight:
			m_weight = (double)option.UInt32Value / 12700.0;
			return true;
		case MsoOptions.LineDashStyle:
			m_dashStyle = (OfficeShapeDashLineStyle)option.UInt32Value;
			return true;
		case MsoOptions.ContainRoundDot:
			m_dashStyle = OfficeShapeDashLineStyle.Dotted_Round;
			return true;
		case MsoOptions.LineTransparency:
			m_transparency = ParseTransparency(option.UInt32Value);
			return true;
		case MsoOptions.LineColor:
			m_foreColor = ShapeFillImpl.ParseColor(m_book, option.MainData);
			return true;
		case MsoOptions.LineBackColor:
			m_backColor = ShapeFillImpl.ParseColor(m_book, option.MainData);
			return true;
		case MsoOptions.ContainLinePattern:
			m_bContainPattern = true;
			return true;
		case MsoOptions.LinePattern:
			m_pattern = ParsePattern(option);
			return true;
		default:
			return false;
		}
	}

	private bool ParseArrowsPropertys(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		switch (option.Id)
		{
		case MsoOptions.LineStartArrow:
			m_beginArrowStyle = (OfficeShapeArrowStyle)option.UInt32Value;
			return true;
		case MsoOptions.LineEndArrow:
			m_endArrowStyle = (OfficeShapeArrowStyle)option.UInt32Value;
			return true;
		case MsoOptions.StartArrowLength:
			m_beginArrowLength = (OfficeShapeArrowLength)option.UInt32Value;
			return true;
		case MsoOptions.EndArrowLength:
			m_endArrowLength = (OfficeShapeArrowLength)option.UInt32Value;
			return true;
		case MsoOptions.StartArrowWidth:
			m_beginArrowWidth = (OfficeShapeArrowWidth)option.UInt32Value;
			return true;
		case MsoOptions.EndArrowWidth:
			m_endArrowWidth = (OfficeShapeArrowWidth)option.UInt32Value;
			return true;
		default:
			return false;
		}
	}

	private OfficeGradientPattern ParsePattern(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		byte[] array;
		if (option.AdditionalData == null || option.AdditionalData.Length == 0)
		{
			if (option.UInt32Value == 0)
			{
				return OfficeGradientPattern.Pat_5_Percent;
			}
			MsofbtBSE picture = m_book.ShapesData.GetPicture((int)option.UInt32Value);
			MemoryStream memoryStream = new MemoryStream(picture.Length);
			picture.InfillInternalData(memoryStream, 0, null, null);
			array = new byte[picture.Length - 36];
			memoryStream.Position = 36L;
			memoryStream.Read(array, 0, array.Length);
		}
		else
		{
			array = option.AdditionalData;
		}
		return GetPattern(array);
	}

	private void ParseVisible(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte b = data[4];
		byte b2 = data[2];
		if (((b2 < 8 || b2 > 16) && data[3] == 0 && data[5] == 0 && ((b >= 8 && b <= 16) || b == 24)) || (b == 24 && b2 == 16))
		{
			m_visible = false;
		}
		else
		{
			m_visible = true;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (m_visible)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.LineWeight, (int)(m_weight * 12700.0));
		}
		SerializeVisible(opt);
		if (m_visible)
		{
			SerializeColor(opt, m_foreColor, m_book, MsoOptions.LineColor);
			SerializeColor(opt, m_backColor, m_book, MsoOptions.LineBackColor);
			SerializeArrowProperties(opt);
			SerializeDashStyle(opt);
			SerializeLineStyle(opt);
			SerializeTransparency(opt, MsoOptions.LineTransparency, m_transparency);
			SerializePattern(opt);
		}
	}

	private void SerializeArrowProperties(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.LineStartArrow, (int)m_beginArrowStyle);
		ShapeImpl.SerializeForte(opt, MsoOptions.LineEndArrow, (int)m_endArrowStyle);
		ShapeImpl.SerializeForte(opt, MsoOptions.StartArrowLength, (int)m_beginArrowLength);
		ShapeImpl.SerializeForte(opt, MsoOptions.EndArrowLength, (int)m_endArrowLength);
		ShapeImpl.SerializeForte(opt, MsoOptions.StartArrowWidth, (int)m_beginArrowWidth);
		ShapeImpl.SerializeForte(opt, MsoOptions.EndArrowWidth, (int)m_endArrowWidth);
	}

	private void SerializeDashStyle(MsofbtOPT opt)
	{
		OfficeShapeDashLineStyle officeShapeDashLineStyle = m_dashStyle;
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (officeShapeDashLineStyle == OfficeShapeDashLineStyle.Dotted_Round)
		{
			officeShapeDashLineStyle = OfficeShapeDashLineStyle.Dotted;
			ShapeImpl.SerializeForte(opt, MsoOptions.ContainRoundDot, 0);
		}
		else
		{
			opt.RemoveOption(471);
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.LineDashStyle, (int)officeShapeDashLineStyle);
	}

	private void SerializeLineStyle(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int style = (int)m_style;
		ShapeImpl.SerializeForte(opt, MsoOptions.LineStyle, style - 1);
	}

	private void SerializeVisible(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		byte[] array = new byte[4] { 8, 0, 8, 0 };
		if (!m_visible)
		{
			array[0] = 0;
			array[2] = 24;
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.NoLineDrawDash, array);
	}

	private void SerializePattern(MsofbtOPT opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int pattern = (int)m_pattern;
		string text = pattern.ToString();
		byte[] resData = ShapeFillImpl.GetResData("Patt" + text);
		if (m_bContainPattern)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.LinePattern, 0, resData, isValid: true);
			ShapeImpl.SerializeForte(opt, MsoOptions.ContainLinePattern, 1);
		}
		else
		{
			opt.RemoveOption(453);
			opt.RemoveOption(452);
		}
	}

	private OfficeGradientPattern GetPattern(byte[] arr)
	{
		if (arr == null)
		{
			throw new ArgumentNullException("arr");
		}
		int num = 1;
		int num2 = 0;
		int num3 = arr.Length;
		while (num2 < m_parsePattArray.Length)
		{
			int i = 0;
			num2++;
			if (m_parsePattArray[num2 - 1] == num3)
			{
				for (; i < num3 && arr[i] == m_parsePattArray[num2 + i]; i++)
				{
				}
				if (i == num3)
				{
					return (OfficeGradientPattern)num;
				}
			}
			num2 += m_parsePattArray[num2 - 1];
			num++;
		}
		return OfficeGradientPattern.Pat_5_Percent;
	}

	public ShapeLineFormatImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ShapeLineFormatImpl obj = (ShapeLineFormatImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		return obj;
	}
}

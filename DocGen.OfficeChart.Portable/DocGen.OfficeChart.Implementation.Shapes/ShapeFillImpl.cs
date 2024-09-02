using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class ShapeFillImpl : CommonObject, IOfficeFill, IInternalFill, IGradient
{
	private const int DEF_SHAD_STYLE_VERTICAL = 166;

	private const int DEF_SHAD_STYLE_VERTICAL2007 = 90;

	private const int DEF_SHAD_STYLE_DIAGONAL_UP = 121;

	private const int DEF_SHAD_STYLE_DIAGONAL_DOWN = 211;

	internal const int DEF_COLOR_CONSTANT = 8;

	private static readonly byte[] DEF_VARIANT_FIRST_ARR;

	private static readonly byte[] DEF_VARIANT_THIRD_ARR;

	private static readonly byte[] DEF_VARIANT_FOURTH_ARR;

	private const int DEF_ONE_COLOR_STYLE_VALUE = 1073741835;

	private static readonly byte[] DEF_VARIANT_CENTER_ADD_DATA;

	private static readonly byte[] DEF_VARIANT_CORNER_ADD_DATA;

	public const string DEF_PATTERN_PREFIX = "Patt";

	internal const string DEF_TEXTURE_PREFIX = "Text";

	private const string DEF_GRAD_PREFIX = "Grad";

	private const string DEF_PATTERN_ENUM_PREFIX = "Pat_";

	private const byte DEF_NOT_VISIBLE_VALUE = 16;

	private static readonly byte[] DEF_BITMAP_INDEX;

	public static readonly Color DEF_COMENT_PARSE_COLOR;

	public const int DEF_COMMENT_COLOR_INDEX = 80;

	private const int DEF_CORNER_STYLE = 5;

	private const int DEF_CENTER_STYLE = 6;

	private const int DEF_OFFSET = 25;

	internal const int MaxValue = 100000;

	internal const int HorizontalAngle = 5400000;

	internal const int VerticalAngle = 0;

	internal const int DiagonalUpAngle = 2700000;

	internal const int DiagonalDownAngle = 18900000;

	internal static Rectangle RectangleFromCenter;

	internal static Rectangle[] RectanglesCorner;

	private static Dictionary<string, byte[]> m_dicResources;

	internal OfficeFillType m_fillType;

	private OfficeGradientStyle m_gradStyle;

	private OfficeGradientVariants m_gradVariant = OfficeGradientVariants.ShadingVariants_2;

	private double m_transparencyTo;

	private double m_transparencyFrom;

	private OfficeGradientColor m_gradientColor = OfficeGradientColor.TwoColor;

	private OfficeGradientPattern m_gradPattern = OfficeGradientPattern.Pat_5_Percent;

	internal OfficeTexture m_gradTexture = OfficeTexture.Papyrus;

	private WorkbookImpl m_book;

	private ChartColor m_backColor = new ChartColor(ColorExtension.Black);

	private ChartColor m_foreColor = new ChartColor(ColorExtension.Gray);

	private OfficeGradientPreset m_presetGrad = OfficeGradientPreset.Grad_Early_Sunset;

	protected Image m_picture;

	private string m_strPictureName;

	private bool m_bVisible = true;

	private int m_imageIndex = -1;

	private double m_gradDegree = 0.2;

	private MsofbtOPT.FOPTE m_parsePictureData;

	protected bool m_bIsShapeFill = true;

	private bool m_bTile;

	private Rectangle m_fillrect;

	private Rectangle m_srcRect;

	private float m_amt;

	private GradientStops m_preseredGradient;

	private bool m_bSupportedGradient = true;

	private float m_textureVerticalScale;

	private float m_textureHorizontalScale;

	private float m_textureOffsetX;

	private float m_textureOffsetY;

	private string m_alignment;

	private string m_tileFlipping;

	private static Assembly s_asem;

	private static byte[] m_arrPreset;

	private static Dictionary<OfficeGradientPreset, byte[]> s_dicPresetStops;

	private PreservationLogger m_logger = new PreservationLogger();

	public GradientStops GradientStops
	{
		get
		{
			GradientStops gradientStops = null;
			if (m_fillType == OfficeFillType.Gradient && m_bSupportedGradient)
			{
				if (m_gradientColor == OfficeGradientColor.Preset)
				{
					gradientStops = GetPresetGradientStops(m_presetGrad);
				}
				else if (m_gradientColor == OfficeGradientColor.OneColor)
				{
					gradientStops = new GradientStops();
					GradientStopImpl item = new GradientStopImpl(ForeColorObject, 0, 100000);
					gradientStops.Add(item);
					byte b = (byte)(m_gradDegree * 255.0);
					int num = 127;
					int shade = ((b <= num) ? (b * 100000 / 255) : (-1));
					int tint = ((b > num) ? ((255 - b) * 100000 / 255) : (-1));
					item = new GradientStopImpl(ForeColorObject, 100000, 100000, tint, shade);
					gradientStops.Add(item);
				}
				else if (m_gradientColor == OfficeGradientColor.TwoColor)
				{
					gradientStops = new GradientStops();
					GradientStopImpl item2 = new GradientStopImpl(ForeColorObject, 0, 100000);
					gradientStops.Add(item2);
					item2 = new GradientStopImpl(BackColorObject, 100000, 100000);
					gradientStops.Add(item2);
				}
				if (IsInverted(m_gradStyle, m_gradVariant))
				{
					gradientStops.InvertGradientStops();
				}
				if (IsDoubled(m_gradStyle, m_gradVariant))
				{
					gradientStops.DoubleGradientStops();
				}
				gradientStops.Angle = GradientAngle(m_gradStyle);
				gradientStops.FillToRect = GradientFillToRect(m_gradStyle, m_gradVariant);
				gradientStops.GradientType = GetGradientType(m_gradStyle);
			}
			return gradientStops;
		}
	}

	public bool Tile
	{
		get
		{
			return m_bTile;
		}
		set
		{
			m_bTile = value;
		}
	}

	public GradientStops PreservedGradient
	{
		get
		{
			return m_preseredGradient;
		}
		set
		{
			m_preseredGradient = value;
		}
	}

	public Rectangle FillRect
	{
		get
		{
			return m_fillrect;
		}
		set
		{
			m_fillrect = value;
		}
	}

	public Rectangle SourceRect
	{
		get
		{
			return m_srcRect;
		}
		set
		{
			m_srcRect = value;
		}
	}

	internal MsofbtOPT.FOPTE ParsePictureData => m_parsePictureData;

	public bool IsGradientSupported
	{
		get
		{
			return m_bSupportedGradient;
		}
		set
		{
			m_bSupportedGradient = value;
		}
	}

	public OfficeFillType FillType
	{
		get
		{
			return m_fillType;
		}
		set
		{
			if (FillType != value || base.Parent is ChartWallOrFloorImpl)
			{
				switch (value)
				{
				case OfficeFillType.Picture:
					throw new ArgumentException("For set picture type use UserPicture method.");
				case OfficeFillType.Texture:
					m_gradTexture = OfficeTexture.Papyrus;
					break;
				}
				if (value == OfficeFillType.Gradient)
				{
					m_gradVariant = OfficeGradientVariants.ShadingVariants_1;
				}
				m_fillType = value;
				ChangeVisible();
			}
		}
	}

	public OfficeGradientStyle GradientStyle
	{
		get
		{
			ValidateGradientType();
			return m_gradStyle;
		}
		set
		{
			ValidateGradientType();
			m_gradStyle = value;
			ChangeVisible();
		}
	}

	public OfficeGradientVariants GradientVariant
	{
		get
		{
			ValidateGradientType();
			return m_gradVariant;
		}
		set
		{
			ValidateGradientType();
			bool flag = value == OfficeGradientVariants.ShadingVariants_3 || value == OfficeGradientVariants.ShadingVariants_4;
			if (m_gradStyle == OfficeGradientStyle.FromCenter && flag)
			{
				throw new NotSupportedException("This variant doesn't support center shading style.");
			}
			m_gradVariant = value;
			ChangeVisible();
		}
	}

	public virtual double TransparencyTo
	{
		get
		{
			ValidateGradientType();
			return m_transparencyTo;
		}
		set
		{
			ValidateGradientType();
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("TransparencyTo");
			}
			m_transparencyTo = value;
			ChangeVisible();
		}
	}

	public virtual double TransparencyFrom
	{
		get
		{
			return m_transparencyFrom;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("TransparencyFrom");
			}
			m_transparencyFrom = value;
			ChangeVisible();
		}
	}

	public float TransparencyColor
	{
		get
		{
			return m_amt;
		}
		set
		{
			if (value >= 0f && 1f >= value)
			{
				m_amt = value;
				return;
			}
			throw new ArgumentException("The specified value is out of range");
		}
	}

	public double Transparency
	{
		get
		{
			ValidateSolidType();
			return m_transparencyFrom;
		}
		set
		{
			ValidateSolidType();
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Transparency is out of range");
			}
			m_logger.SetFlag(PreservedFlag.Fill);
			m_transparencyFrom = value;
		}
	}

	public OfficeGradientColor GradientColorType
	{
		get
		{
			ValidateGradientType();
			return m_gradientColor;
		}
		set
		{
			ValidateGradientType();
			m_gradientColor = value;
			ChangeVisible();
		}
	}

	public OfficeGradientPattern Pattern
	{
		get
		{
			ValidatePatternType();
			return m_gradPattern;
		}
		set
		{
			m_fillType = OfficeFillType.Pattern;
			m_gradPattern = value;
			ChangeVisible();
		}
	}

	public OfficeTexture Texture
	{
		get
		{
			ValidateTextureType();
			return m_gradTexture;
		}
		set
		{
			if (m_gradTexture == OfficeTexture.User_Defined)
			{
				throw new ArgumentException("This method support only preset textured");
			}
			m_fillType = OfficeFillType.Texture;
			m_gradTexture = value;
			ChangeVisible();
		}
	}

	public OfficeKnownColors BackColorIndex
	{
		get
		{
			return BackColorObject.GetIndexed(m_book);
		}
		set
		{
			BackColorObject.SetIndexed(value);
		}
	}

	public OfficeKnownColors ForeColorIndex
	{
		get
		{
			return ForeColorObject.GetIndexed(m_book);
		}
		set
		{
			ForeColorObject.SetIndexed(value);
		}
	}

	public virtual Color BackColor
	{
		get
		{
			return BackColorObject.GetRGB(m_book);
		}
		set
		{
			BackColorObject.SetRGB(value, m_book);
		}
	}

	public virtual Color ForeColor
	{
		get
		{
			return ForeColorObject.GetRGB(m_book);
		}
		set
		{
			ForeColorObject.SetRGB(value, m_book);
		}
	}

	public virtual ChartColor BackColorObject => m_backColor;

	public virtual ChartColor ForeColorObject => m_foreColor;

	public OfficeGradientPreset PresetGradientType
	{
		get
		{
			ValidateGradientType();
			if (m_gradientColor != OfficeGradientColor.Preset)
			{
				throw new NotSupportedException("This property supported only if checked preset color type.");
			}
			return m_presetGrad;
		}
		set
		{
			ValidateGradientType();
			m_gradientColor = OfficeGradientColor.Preset;
			m_presetGrad = value;
			ChangeVisible();
		}
	}

	public Image Picture
	{
		get
		{
			ValidatePictureProperties();
			return m_picture;
		}
	}

	public string PictureName
	{
		get
		{
			ValidatePictureProperties();
			return m_strPictureName;
		}
	}

	public virtual bool Visible
	{
		get
		{
			return m_bVisible;
		}
		set
		{
			if (Visible != value)
			{
				m_bVisible = value;
			}
		}
	}

	public double GradientDegree
	{
		get
		{
			ValidateGradientType();
			if (m_gradientColor != 0)
			{
				throw new NotSupportedException("This property supports only if checked one color gradient");
			}
			return m_gradDegree;
		}
		set
		{
			ValidateGradientType();
			if (m_gradientColor != 0)
			{
				throw new NotSupportedException("This property supports only if checked one color gradient");
			}
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Gradient degree is out of range.");
			}
			m_gradDegree = value;
			ChangeVisible();
		}
	}

	public float TextureVerticalScale
	{
		get
		{
			return m_textureVerticalScale;
		}
		set
		{
			if (value < 21475f)
			{
				m_textureVerticalScale = value;
				return;
			}
			throw new ArgumentException("The specified value is out of range");
		}
	}

	public float TextureHorizontalScale
	{
		get
		{
			return m_textureHorizontalScale;
		}
		set
		{
			if (value < 21475f)
			{
				m_textureHorizontalScale = value;
				return;
			}
			throw new ArgumentException("The specified value is out of range");
		}
	}

	public float TextureOffsetX
	{
		get
		{
			return m_textureOffsetX;
		}
		set
		{
			if (value < 169056f)
			{
				m_textureOffsetX = value;
				return;
			}
			throw new ArgumentException("The specified value is out of range");
		}
	}

	public float TextureOffsetY
	{
		get
		{
			return m_textureOffsetY;
		}
		set
		{
			if (value < 169056f)
			{
				m_textureOffsetY = value;
				return;
			}
			throw new ArgumentException("The specified value is out of range");
		}
	}

	public string Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	public string TileFlipping
	{
		get
		{
			return m_tileFlipping;
		}
		set
		{
			m_tileFlipping = value;
		}
	}

	static ShapeFillImpl()
	{
		DEF_VARIANT_FIRST_ARR = new byte[4] { 100, 0, 0, 0 };
		DEF_VARIANT_THIRD_ARR = new byte[4] { 206, 255, 255, 255 };
		DEF_VARIANT_FOURTH_ARR = new byte[4] { 50, 0, 0, 0 };
		DEF_VARIANT_CENTER_ADD_DATA = new byte[4] { 0, 128, 0, 0 };
		DEF_VARIANT_CORNER_ADD_DATA = new byte[4] { 0, 0, 1, 0 };
		DEF_BITMAP_INDEX = new byte[4] { 128, 122, 31, 240 };
		DEF_COMENT_PARSE_COLOR = Color.FromArgb(255, 255, 255, 222);
		RectangleFromCenter = Rectangle.FromLTRB(50000, 50000, 50000, 50000);
		RectanglesCorner = new Rectangle[4]
		{
			Rectangle.FromLTRB(0, 0, 100000, 100000),
			Rectangle.FromLTRB(100000, 0, 0, 100000),
			Rectangle.FromLTRB(0, 100000, 100000, 0),
			Rectangle.FromLTRB(100000, 100000, 0, 0)
		};
		m_dicResources = new Dictionary<string, byte[]>();
		s_asem = typeof(ShapeFillImpl).GetTypeInfo().Assembly;
		m_arrPreset = new byte[1320];
		int num = 0;
		ResourceHandler resourceHandler = new ResourceHandler();
		for (int i = 1; i <= 24; i++)
		{
			byte[] array = resourceHandler.TexturePresetGradient["Grad" + i];
			int num2 = array.Length;
			m_arrPreset[num] = (byte)num2;
			num++;
			array.CopyTo(m_arrPreset, num);
			num += num2;
		}
	}

	public static byte[] GetResData(string strID)
	{
		if (!m_dicResources.TryGetValue(strID, out var value))
		{
			return new ResourceHandler().PatternArray[strID];
		}
		return value;
	}

	internal static Color ParseColor(WorkbookImpl book, byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (value[5] == 8)
		{
			return (value[2] == 80) ? DEF_COMENT_PARSE_COLOR : book.GetPaletteColor((OfficeKnownColors)value[2]);
		}
		return Color.FromArgb(255, value[2], value[3], value[4]);
	}

	internal static void ParseColor(WorkbookImpl book, byte[] value, ChartColor color)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (color == null)
		{
			throw new ArgumentNullException("color");
		}
		if (value[5] == 8)
		{
			if (value[2] == 80)
			{
				color.SetIndexed((OfficeKnownColors)80);
			}
			else
			{
				color.SetIndexed((OfficeKnownColors)value[2]);
			}
		}
		else if (value[5] == 16)
		{
			color.SetRGB(Color.FromArgb(0, 0, 0, 0));
		}
		else
		{
			color.SetRGB(Color.FromArgb(255, value[2], value[3], value[4]), book);
		}
	}

	public static GradientStops GetPresetGradientStops(OfficeGradientPreset preset)
	{
		return new GradientStops(GetPresetGradientStopsData(preset));
	}

	public static byte[] GetPresetGradientStopsData(OfficeGradientPreset preset)
	{
		if (s_dicPresetStops == null)
		{
			FillPresetsGradientStops();
		}
		return s_dicPresetStops[preset];
	}

	public ShapeFillImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		m_foreColor = new ChartColor(ColorExtension.White);
		m_backColor = new ChartColor(ColorExtension.Empty);
		m_backColor.AfterChange += ChangeVisible;
		m_foreColor.AfterChange += ChangeVisible;
	}

	internal ShapeFillImpl(IApplication application, object parent, OfficeFillType fillType, PreservationLogger logger)
		: this(application, parent)
	{
		m_fillType = fillType;
		m_logger = logger;
	}

	public ShapeFillImpl(IApplication application, object parent, OfficeFillType fillType)
		: this(application, parent)
	{
		m_fillType = fillType;
	}

	private void FindParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ApplicationException("Cann't find parent book");
		}
	}

	public void UserPicture(Image im, string name)
	{
		if (name == null || name.Length == 0)
		{
			throw new ArgumentException("name canot be null or empty.");
		}
		if (im == null)
		{
			throw new ArgumentNullException("im");
		}
		m_fillType = OfficeFillType.Picture;
		m_picture = im;
		m_strPictureName = name;
		ChangeVisible();
		m_imageIndex = SetPictureToBse(im, name);
	}

	public void UserTexture(Image im, string name)
	{
		if (name == null || name.Length == 0)
		{
			throw new ArgumentException("name canot be null or empty.");
		}
		if (im == null)
		{
			throw new ArgumentNullException("im");
		}
		m_fillType = OfficeFillType.Texture;
		m_gradTexture = OfficeTexture.User_Defined;
		m_picture = im;
		m_strPictureName = name;
		ChangeVisible();
		m_imageIndex = SetPictureToBse(im, name);
	}

	public void Patterned(OfficeGradientPattern pattern)
	{
		Pattern = pattern;
		ChangeVisible();
	}

	public void PresetGradient(OfficeGradientPreset grad)
	{
		PresetGradient(grad, OfficeGradientStyle.Horizontal);
	}

	public void PresetGradient(OfficeGradientPreset grad, OfficeGradientStyle shadStyle)
	{
		PresetGradient(grad, shadStyle, OfficeGradientVariants.ShadingVariants_1);
	}

	public void PresetGradient(OfficeGradientPreset grad, OfficeGradientStyle shadStyle, OfficeGradientVariants shadVar)
	{
		if (shadStyle == OfficeGradientStyle.FromCenter && shadVar > OfficeGradientVariants.ShadingVariants_2)
		{
			throw new ArgumentException("From centr style support only var_1 or var_2");
		}
		m_fillType = OfficeFillType.Gradient;
		m_gradientColor = OfficeGradientColor.Preset;
		m_presetGrad = grad;
		m_gradStyle = shadStyle;
		m_gradVariant = shadVar;
		m_bSupportedGradient = true;
		ChangeVisible();
	}

	public void PresetTextured(OfficeTexture texture)
	{
		Texture = texture;
		ChangeVisible();
	}

	public void TwoColorGradient()
	{
		TwoColorGradient(OfficeGradientStyle.Horizontal);
	}

	public void TwoColorGradient(OfficeGradientStyle style)
	{
		TwoColorGradient(style, OfficeGradientVariants.ShadingVariants_1);
	}

	public void TwoColorGradient(OfficeGradientStyle style, OfficeGradientVariants variant)
	{
		if (style == OfficeGradientStyle.FromCenter && variant > OfficeGradientVariants.ShadingVariants_2)
		{
			throw new ArgumentException("From centr style support only var_1 or var_2");
		}
		m_fillType = OfficeFillType.Gradient;
		m_gradientColor = OfficeGradientColor.TwoColor;
		m_gradStyle = style;
		m_gradVariant = variant;
		m_bSupportedGradient = true;
		ChangeVisible();
	}

	public void OneColorGradient()
	{
		OneColorGradient(OfficeGradientStyle.Horizontal);
	}

	public void OneColorGradient(OfficeGradientStyle style)
	{
		OneColorGradient(style, OfficeGradientVariants.ShadingVariants_1);
	}

	public void OneColorGradient(OfficeGradientStyle style, OfficeGradientVariants variant)
	{
		if (style == OfficeGradientStyle.FromCenter && variant > OfficeGradientVariants.ShadingVariants_2)
		{
			throw new ArgumentException("From centr style support only var_1 or var_2");
		}
		m_fillType = OfficeFillType.Gradient;
		m_gradientColor = OfficeGradientColor.OneColor;
		m_gradStyle = style;
		m_gradVariant = variant;
		m_bSupportedGradient = true;
		ChangeVisible();
	}

	public void Solid()
	{
		m_fillType = OfficeFillType.SolidColor;
		ChangeVisible();
	}

	public int CompareTo(IGradient twin)
	{
		if (twin == null)
		{
			return 1;
		}
		int num = ((m_gradStyle != twin.GradientStyle) ? 1 : 0);
		if (num != 0)
		{
			return num;
		}
		num = ((m_gradVariant != twin.GradientVariant) ? 1 : 0);
		if (num != 0)
		{
			return num;
		}
		num = ((!(m_backColor == twin.BackColorObject)) ? 1 : 0);
		if (num != 0)
		{
			return num;
		}
		return (!(m_foreColor == twin.ForeColorObject)) ? 1 : 0;
	}

	[CLSCompliant(false)]
	public bool ParseOption(MsofbtOPT.FOPTE option)
	{
		switch (option.Id)
		{
		case MsoOptions.FillType:
			ParseFillType(option.UInt32Value);
			return true;
		case MsoOptions.BackColor:
			ParseColor(m_book, option.MainData, BackColorObject);
			m_gradDegree = ParseGradientDegree(option.MainData);
			return true;
		case MsoOptions.ForeColor:
			ParseColor(m_book, option.MainData, ForeColorObject);
			return true;
		case MsoOptions.ShadStyle:
			ParseShadingStyle(option.MainData);
			return true;
		case MsoOptions.ShadVariant:
			ParseShadingVariant(option.MainData[2]);
			return true;
		case MsoOptions.PattTextName:
		{
			byte[] additionalData = option.AdditionalData;
			if (!option.IsValid && additionalData != null && additionalData.Length != 0)
			{
				ParsePattTextName(additionalData);
			}
			return true;
		}
		case MsoOptions.PatternTexture:
			if (m_fillType == OfficeFillType.Picture || m_fillType == OfficeFillType.Texture)
			{
				m_parsePictureData = option;
			}
			return true;
		case MsoOptions.GradientColorType:
			ParseGradientColor(option.UInt32Value);
			return true;
		case MsoOptions.PresetGradientData:
			ParsePresetGradient(option.AdditionalData);
			return true;
		case MsoOptions.ShadingStyleCorner_1:
			if (m_gradStyle == OfficeGradientStyle.FromCorner && option.MainData[4] == 1)
			{
				m_gradVariant = OfficeGradientVariants.ShadingVariants_2;
			}
			return true;
		case MsoOptions.ShadingStyleCorner_2:
			ParseCornerVariants(option.MainData[4]);
			return true;
		case MsoOptions.NoFillHitTest:
			ParseVisible(option.MainData);
			return true;
		case MsoOptions.GradientTransparency:
			m_transparencyTo = ShapeLineFormatImpl.ParseTransparency(option.UInt32Value);
			return true;
		case MsoOptions.Transparency:
			m_transparencyFrom = ShapeLineFormatImpl.ParseTransparency(option.UInt32Value);
			return true;
		default:
			return false;
		}
	}

	private void ParseFillType(uint value)
	{
		m_fillType = (OfficeFillType)value;
		if (value == 5)
		{
			m_fillType = OfficeFillType.Gradient;
			m_gradStyle = OfficeGradientStyle.FromCorner;
			m_gradVariant = OfficeGradientVariants.ShadingVariants_1;
		}
		if (value == 6)
		{
			m_fillType = OfficeFillType.Gradient;
			m_gradStyle = OfficeGradientStyle.FromCenter;
		}
	}

	private void ParseShadingStyle(byte[] arr)
	{
		if (arr == null)
		{
			throw new ArgumentNullException("arr");
		}
		if (m_gradStyle != OfficeGradientStyle.FromCenter && m_gradStyle != OfficeGradientStyle.FromCorner)
		{
			switch (arr[4])
			{
			case 90:
			case 166:
				m_gradStyle = OfficeGradientStyle.Vertical;
				break;
			case 121:
				m_gradStyle = OfficeGradientStyle.DiagonalUp;
				break;
			case 211:
				m_gradStyle = OfficeGradientStyle.DiagonalDown;
				m_gradVariant = OfficeGradientVariants.ShadingVariants_1;
				break;
			}
		}
	}

	private void ParseShadingVariant(byte value)
	{
		if (m_gradStyle == OfficeGradientStyle.FromCorner)
		{
			return;
		}
		if (m_gradStyle == OfficeGradientStyle.FromCenter)
		{
			m_gradVariant = ((value == 100) ? OfficeGradientVariants.ShadingVariants_1 : OfficeGradientVariants.ShadingVariants_2);
			return;
		}
		switch (value)
		{
		case 100:
			m_gradVariant = ((m_gradStyle != OfficeGradientStyle.DiagonalDown) ? OfficeGradientVariants.ShadingVariants_1 : OfficeGradientVariants.ShadingVariants_2);
			break;
		case 50:
			m_gradVariant = ((m_gradStyle == OfficeGradientStyle.Horizontal) ? OfficeGradientVariants.ShadingVariants_3 : OfficeGradientVariants.ShadingVariants_4);
			break;
		case 206:
			m_gradVariant = ((m_gradStyle == OfficeGradientStyle.Horizontal) ? OfficeGradientVariants.ShadingVariants_4 : OfficeGradientVariants.ShadingVariants_3);
			break;
		default:
			m_gradVariant = ((m_gradStyle == OfficeGradientStyle.DiagonalDown) ? OfficeGradientVariants.ShadingVariants_1 : OfficeGradientVariants.ShadingVariants_2);
			break;
		}
	}

	private void ParsePattTextName(byte[] addData)
	{
		bool flag = m_fillType == OfficeFillType.Pattern;
		bool flag2 = m_fillType == OfficeFillType.Picture;
		bool flag3 = m_fillType == OfficeFillType.Texture;
		if (!flag && !flag3 && !flag2)
		{
			return;
		}
		if (addData == null)
		{
			throw new ArgumentNullException("addData");
		}
		string text = "";
		int i = 0;
		for (int num = addData.Length - 2; i < num; i += 2)
		{
			string text2 = text;
			char c = (char)addData[i];
			text = text2 + c;
		}
		if (flag2)
		{
			ParsePictureOrUserDefinedTexture(text, bIsPicture: true);
			return;
		}
		if (flag3 && text[0] >= '0' && text[0] <= '9')
		{
			ParsePictureOrUserDefinedTexture(text, bIsPicture: false);
			return;
		}
		string text3 = text.Replace(' ', '_');
		if (flag)
		{
			text3 = "Pat_" + text3;
			try
			{
				m_gradPattern = (OfficeGradientPattern)Enum.Parse(typeof(OfficeGradientPattern), text3, ignoreCase: true);
				return;
			}
			catch
			{
				m_gradPattern = OfficeGradientPattern.Pat_5_Percent;
				return;
			}
		}
		try
		{
			m_gradTexture = (OfficeTexture)Enum.Parse(typeof(OfficeTexture), text3, ignoreCase: true);
		}
		catch
		{
			ParsePictureOrUserDefinedTexture(text, bIsPicture: false);
		}
	}

	private void ParseGradientColor(uint value)
	{
		switch (value)
		{
		case 0u:
			m_gradientColor = OfficeGradientColor.Preset;
			break;
		case 1073741835u:
			m_gradientColor = OfficeGradientColor.OneColor;
			break;
		default:
			m_gradientColor = OfficeGradientColor.TwoColor;
			break;
		}
	}

	private void ParsePresetGradient(byte[] value)
	{
		if (value == null || m_fillType != OfficeFillType.Gradient)
		{
			return;
		}
		int i = 1;
		int num = 0;
		int num2 = value.Length;
		bool flag = false;
		for (; i < 25; i++)
		{
			if (flag)
			{
				break;
			}
			int num3 = m_arrPreset[num];
			num++;
			if (num3 == num2)
			{
				for (int j = 0; j < num3 && value[j] == m_arrPreset[num + j]; j++)
				{
					if (num3 - j == 1)
					{
						flag = true;
					}
				}
			}
			num += num3;
		}
		if (flag)
		{
			m_presetGrad = (OfficeGradientPreset)(i - 1);
		}
	}

	private void ParsePictureOrUserDefinedTexture(string strName, bool bIsPicture)
	{
		if (strName == null || strName.Length == 0)
		{
			throw new ArgumentException("strName canot be null or empty.");
		}
		m_strPictureName = strName;
		ParsePictureOrUserDefinedTexture(bIsPicture);
	}

	protected void ParsePictureOrUserDefinedTexture(bool bIsPicture)
	{
	}

	public static void UpdateBitMapHederToStream(MemoryStream ms, byte[] arr)
	{
		if (ms == null)
		{
			throw new ArgumentNullException("ms");
		}
		if (arr == null)
		{
			throw new ArgumentNullException("arr");
		}
		if (BiffRecordRaw.CompareArrays(arr, 0, DEF_BITMAP_INDEX, 0, DEF_BITMAP_INDEX.Length))
		{
			int iFullSize = arr.Length + 14 - 25;
			uint uiSize = BitConverter.ToUInt32(arr, 25);
			uint dibColorCount = BitConverter.ToUInt32(arr, 57);
			MsoBitmapPicture.AddBitMapHeaderToStream(ms, iFullSize, uiSize, dibColorCount);
		}
	}

	private void ParseVisible(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte b = (byte)(data[2] & 0x10u);
		if (b > 0)
		{
			m_bVisible = true;
			return;
		}
		b = (byte)(data[4] & 0x10u);
		if (data[3] == 0 && data[5] == 0 && b > 0)
		{
			m_bVisible = false;
		}
		else
		{
			m_bVisible = true;
		}
	}

	[CLSCompliant(false)]
	public IFopteOptionWrapper Serialize(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		opt = SerializeFillType(opt);
		opt = SerializeTransparency(opt);
		opt = SerializeVisible(opt);
		ShapeLineFormatImpl.SerializeColor(opt, ForeColorObject, m_book, MsoOptions.ForeColor);
		ShapeLineFormatImpl.SerializeColor(opt, BackColorObject, m_book, MsoOptions.BackColor);
		switch (m_fillType)
		{
		case OfficeFillType.Gradient:
			return SerializeGradient(opt);
		case OfficeFillType.Pattern:
		case OfficeFillType.Texture:
			return SerializePatternTexture(opt);
		case OfficeFillType.Picture:
			return SerializePicture(opt);
		case OfficeFillType.SolidColor:
			return SerializeSolidColor(opt);
		case OfficeFillType.UnknownGradient:
			return opt;
		default:
			throw new ApplicationException("Unknown fill type");
		}
	}

	private IFopteOptionWrapper SerializeGradient(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		opt = SerializeShadVariant(opt);
		opt = SerializeGradientStyle(opt);
		if (m_gradStyle != 0)
		{
			opt = SerializeShadStyle(opt);
		}
		if (m_gradientColor == OfficeGradientColor.Preset)
		{
			opt = SerializeGradientPreset(opt);
		}
		return opt;
	}

	private IFopteOptionWrapper SerializePatternTexture(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		bool flag = m_fillType == OfficeFillType.Pattern;
		if (!flag && m_gradTexture == OfficeTexture.User_Defined)
		{
			return SerializePicture(opt);
		}
		int num;
		string text2;
		string text;
		if (flag)
		{
			num = (int)m_gradPattern;
			text = m_gradPattern.ToString();
			text2 = "Patt";
		}
		else
		{
			num = (int)m_gradTexture;
			text = m_gradTexture.ToString();
			text2 = "Text";
		}
		byte[] resData = GetResData(text2 + num);
		ShapeImpl.SerializeForte(opt, MsoOptions.PatternTexture, 0, resData, isValid: true);
		if (flag)
		{
			text = text.Substring("Pat_".Length);
			text = text.Replace("_Percent", "%");
		}
		text = text.Replace('_', ' ');
		byte[] addData = ConvertNameToByteArray(text);
		ShapeImpl.SerializeForte(opt, MsoOptions.PattTextName, 0, addData, isValid: true);
		return opt;
	}

	private IFopteOptionWrapper SerializePicture(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		opt = SetPicture(opt);
		if (!string.IsNullOrEmpty(m_strPictureName))
		{
			byte[] addData = ConvertNameToByteArray(m_strPictureName);
			ShapeImpl.SerializeForte(opt, MsoOptions.PattTextName, 0, addData, isValid: true);
		}
		return opt;
	}

	private IFopteOptionWrapper SerializeSolidColor(IFopteOptionWrapper opt)
	{
		return opt;
	}

	private IFopteOptionWrapper SerializeFillType(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int value = (int)m_fillType;
		if (m_fillType == OfficeFillType.Gradient && m_gradStyle == OfficeGradientStyle.FromCorner)
		{
			value = 5;
		}
		if (m_fillType == OfficeFillType.Gradient && m_gradStyle == OfficeGradientStyle.FromCenter)
		{
			value = 6;
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.FillType, value);
		return opt;
	}

	private IFopteOptionWrapper SerializeShadStyle(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		byte[] array = new byte[4] { 0, 0, 0, 255 };
		switch (m_gradStyle)
		{
		case OfficeGradientStyle.Vertical:
			array[2] = 166;
			break;
		case OfficeGradientStyle.DiagonalUp:
			array[2] = 121;
			break;
		case OfficeGradientStyle.DiagonalDown:
			array[2] = 211;
			break;
		default:
			return opt;
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadStyle, array);
		return opt;
	}

	private IFopteOptionWrapper SerializeShadVariant(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (m_gradStyle == OfficeGradientStyle.FromCorner)
		{
			return SerializeShadVariantCorner(opt);
		}
		if (m_gradStyle == OfficeGradientStyle.FromCenter)
		{
			return SerializeShadVariantCenter(opt);
		}
		if (m_gradVariant < OfficeGradientVariants.ShadingVariants_3)
		{
			if (!((m_gradStyle == OfficeGradientStyle.DiagonalDown) ? (m_gradVariant != OfficeGradientVariants.ShadingVariants_2) : (m_gradVariant == OfficeGradientVariants.ShadingVariants_2)))
			{
				ShapeImpl.SerializeForte(opt, MsoOptions.ShadVariant, DEF_VARIANT_FIRST_ARR);
			}
			return opt;
		}
		if ((m_gradStyle == OfficeGradientStyle.Horizontal) ? (m_gradVariant != OfficeGradientVariants.ShadingVariants_3) : (m_gradVariant == OfficeGradientVariants.ShadingVariants_3))
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadVariant, DEF_VARIANT_THIRD_ARR);
		}
		else
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadVariant, DEF_VARIANT_FOURTH_ARR);
		}
		return opt;
	}

	private IFopteOptionWrapper SerializeShadVariantCenter(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_1)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadVariant, DEF_VARIANT_FIRST_ARR);
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_1, DEF_VARIANT_CENTER_ADD_DATA);
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_2, DEF_VARIANT_CENTER_ADD_DATA);
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_3, DEF_VARIANT_CENTER_ADD_DATA);
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_4, DEF_VARIANT_CENTER_ADD_DATA);
		return opt;
	}

	private IFopteOptionWrapper SerializeShadVariantCorner(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.ShadVariant, DEF_VARIANT_FIRST_ARR);
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_1)
		{
			return opt;
		}
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_2 || m_gradVariant == OfficeGradientVariants.ShadingVariants_4)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_1, DEF_VARIANT_CORNER_ADD_DATA);
		}
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_3 || m_gradVariant == OfficeGradientVariants.ShadingVariants_4)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_2, DEF_VARIANT_CORNER_ADD_DATA);
		}
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_2 || m_gradVariant == OfficeGradientVariants.ShadingVariants_4)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_3, DEF_VARIANT_CORNER_ADD_DATA);
		}
		if (m_gradVariant == OfficeGradientVariants.ShadingVariants_3 || m_gradVariant == OfficeGradientVariants.ShadingVariants_4)
		{
			ShapeImpl.SerializeForte(opt, MsoOptions.ShadingStyleCorner_4, DEF_VARIANT_CORNER_ADD_DATA);
		}
		return opt;
	}

	private IFopteOptionWrapper SerializeGradientStyle(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		if (m_gradientColor != 0)
		{
			return opt;
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.GradientColorType, 1073741835);
		SerializeGradientDegree(opt);
		return opt;
	}

	private IFopteOptionWrapper SerializeGradientPreset(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		int presetGrad = (int)m_presetGrad;
		byte[] resData = GetResData("Grad" + presetGrad);
		ShapeImpl.SerializeForte(opt, MsoOptions.PresetGradientData, 0, resData, isValid: true);
		ShapeImpl.SerializeForte(opt, MsoOptions.GradientColorType, 0);
		return opt;
	}

	private IFopteOptionWrapper SerializeVisible(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		byte[] array = new byte[4] { 0, 0, 31, 0 };
		if (m_bVisible)
		{
			array[0] = 28;
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.NoFillHitTest, array);
		return opt;
	}

	private IFopteOptionWrapper SerializeGradientDegree(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		byte[] array = new byte[4] { 240, 1, 0, 16 };
		double num = m_gradDegree;
		if (num >= 0.5)
		{
			array[1] = 2;
			num = 1.0 - num;
		}
		array[2] = (byte)(num * 255.0 * 2.0);
		ShapeImpl.SerializeForte(opt, MsoOptions.BackColor, array);
		return opt;
	}

	private void ValidateGradientType()
	{
		if (m_fillType != OfficeFillType.Gradient)
		{
			throw new NotSupportedException("This property can be set only when Gradient Style is selected.");
		}
	}

	private void ValidatePictureProperties()
	{
		bool flag = m_fillType == OfficeFillType.Texture && m_gradTexture == OfficeTexture.User_Defined;
		if (m_fillType != OfficeFillType.Picture && !flag)
		{
			throw new NotSupportedException("This property support only if defined user texture of picture");
		}
	}

	private void ValidatePatternType()
	{
		if (m_fillType != OfficeFillType.Pattern)
		{
			throw new NotSupportedException("This property suports only if chacked pattern style.");
		}
	}

	private void ValidateTextureType()
	{
		if (m_fillType != OfficeFillType.Texture)
		{
			throw new NotSupportedException("This property suports only if chacked texture style.");
		}
	}

	private void ValidateSolidType()
	{
		if (m_fillType != 0)
		{
			throw new NotSupportedException("This property supports only if Checked Solid style.");
		}
	}

	private byte[] ConvertNameToByteArray(string strName)
	{
		if (strName == null || strName.Length == 0)
		{
			throw new ArgumentException("strName canot be null or empty.");
		}
		int length = strName.Length;
		byte[] array = new byte[length * 2 + 2];
		for (int i = 0; i < length; i++)
		{
			char c = strName[i];
			if (char.IsUpper(c) && i > 0)
			{
				c = char.ToLower(c);
			}
			array[2 * i] = (byte)c;
			array[2 * i + 1] = 0;
		}
		array[2 * length] = 0;
		array[2 * length + 1] = 0;
		return array;
	}

	private double ParseGradientDegree(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		double num = m_gradDegree;
		if (value[5] == 16)
		{
			num = (double)(int)value[4] * 0.5 / 255.0;
			if (value[3] == 2)
			{
				num = 1.0 - num;
			}
		}
		return num;
	}

	private void ParseCornerVariants(byte value)
	{
		if (m_gradStyle == OfficeGradientStyle.FromCorner && value == 1)
		{
			m_gradVariant = ((m_gradVariant == OfficeGradientVariants.ShadingVariants_1) ? OfficeGradientVariants.ShadingVariants_3 : OfficeGradientVariants.ShadingVariants_4);
		}
	}

	private static void FillPresetsGradientStops()
	{
		s_dicPresetStops = new Dictionary<OfficeGradientPreset, byte[]>();
		OfficeGradientPreset[] array = new OfficeGradientPreset[24]
		{
			OfficeGradientPreset.Grad_Early_Sunset,
			OfficeGradientPreset.Grad_Late_Sunset,
			OfficeGradientPreset.Grad_Nightfall,
			OfficeGradientPreset.Grad_Daybreak,
			OfficeGradientPreset.Grad_Horizon,
			OfficeGradientPreset.Grad_Desert,
			OfficeGradientPreset.Grad_Ocean,
			OfficeGradientPreset.Grad_Calm_Water,
			OfficeGradientPreset.Grad_Fire,
			OfficeGradientPreset.Grad_Fog,
			OfficeGradientPreset.Grad_Moss,
			OfficeGradientPreset.Grad_Peacock,
			OfficeGradientPreset.Grad_Wheat,
			OfficeGradientPreset.Grad_Parchment,
			OfficeGradientPreset.Grad_Mahogany,
			OfficeGradientPreset.Grad_Rainbow,
			OfficeGradientPreset.Grad_RainbowII,
			OfficeGradientPreset.Grad_Gold,
			OfficeGradientPreset.Grad_GoldII,
			OfficeGradientPreset.Grad_Brass,
			OfficeGradientPreset.Grad_Chrome,
			OfficeGradientPreset.Grad_ChromeII,
			OfficeGradientPreset.Grad_Silver,
			OfficeGradientPreset.Grad_Sapphire
		};
		ResourceHandler resourceHandler = new ResourceHandler();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			OfficeGradientPreset key = array[i];
			byte[] value = resourceHandler.Gradients[key.ToString()];
			s_dicPresetStops.Add(key, value);
		}
	}

	[CLSCompliant(false)]
	protected virtual IFopteOptionWrapper SetPicture(IFopteOptionWrapper opt)
	{
		if (opt == null)
		{
			throw new ArgumentNullException("opt");
		}
		ShapeImpl.SerializeForte(opt, MsoOptions.PatternTexture, m_imageIndex, null, isValid: true);
		return opt;
	}

	protected virtual int SetPictureToBse(Image im, string strName)
	{
		if (im == null)
		{
			throw new ArgumentNullException("im");
		}
		WorkbookShapeDataImpl shapesData = m_book.ShapesData;
		if (m_imageIndex >= 0)
		{
			MsofbtBSE picture = shapesData.GetPicture(m_imageIndex);
			if (picture != null && picture.RefCount <= 1)
			{
				shapesData.RemovePicture((uint)(m_imageIndex - 1), removeImage: true);
			}
		}
		return shapesData.AddPicture(im, ExcelImageFormat.Original, strName);
	}

	[CLSCompliant(false)]
	protected virtual IFopteOptionWrapper SerializeTransparency(IFopteOptionWrapper opt)
	{
		ShapeLineFormatImpl.SerializeTransparency(opt, MsoOptions.GradientTransparency, m_transparencyTo);
		ShapeLineFormatImpl.SerializeTransparency(opt, MsoOptions.Transparency, m_transparencyFrom);
		return opt;
	}

	internal virtual void ChangeVisible()
	{
		Visible = true;
	}

	public virtual ShapeFillImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ShapeFillImpl obj = (ShapeFillImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		return obj;
	}

	public void CopyFrom(ShapeFillImpl fill)
	{
		m_fillType = fill.m_fillType;
		m_gradStyle = fill.m_gradStyle;
		m_gradVariant = fill.m_gradVariant;
		m_transparencyTo = fill.m_transparencyTo;
		m_transparencyFrom = fill.m_transparencyFrom;
		m_gradientColor = fill.m_gradientColor;
		m_gradPattern = fill.m_gradPattern;
		m_gradTexture = fill.m_gradTexture;
		m_book = fill.m_book;
		m_backColor.CopyFrom(fill.m_backColor, callEvent: false);
		m_foreColor.CopyFrom(fill.m_foreColor, callEvent: false);
		m_presetGrad = fill.m_presetGrad;
		m_strPictureName = fill.m_strPictureName;
		m_bVisible = fill.m_bVisible;
		m_imageIndex = fill.m_imageIndex;
		m_gradDegree = fill.m_gradDegree;
		m_parsePictureData = (MsofbtOPT.FOPTE)fill.m_parsePictureData.Clone();
		m_bIsShapeFill = fill.m_bIsShapeFill;
	}

	public static bool IsInverted(OfficeGradientStyle gradientStyle, OfficeGradientVariants variant)
	{
		bool result = false;
		switch (gradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
		case OfficeGradientStyle.Vertical:
		case OfficeGradientStyle.DiagonalUp:
		case OfficeGradientStyle.FromCenter:
			result = StandardInverted(variant);
			break;
		case OfficeGradientStyle.DiagonalDown:
			result = DiagonalDownInverted(variant);
			break;
		case OfficeGradientStyle.FromCorner:
			result = false;
			break;
		}
		return result;
	}

	private static bool DiagonalDownInverted(OfficeGradientVariants variant)
	{
		if (variant != OfficeGradientVariants.ShadingVariants_1 && variant != OfficeGradientVariants.ShadingVariants_4)
		{
			return false;
		}
		return true;
	}

	private static bool StandardInverted(OfficeGradientVariants variant)
	{
		if (variant != OfficeGradientVariants.ShadingVariants_2 && variant != OfficeGradientVariants.ShadingVariants_4)
		{
			return false;
		}
		return true;
	}

	public static bool IsDoubled(OfficeGradientStyle gradientStyle, OfficeGradientVariants variant)
	{
		bool result = false;
		switch (gradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
		case OfficeGradientStyle.Vertical:
		case OfficeGradientStyle.DiagonalUp:
		case OfficeGradientStyle.DiagonalDown:
		case OfficeGradientStyle.FromCenter:
			result = StandardDoubled(variant);
			break;
		case OfficeGradientStyle.FromCorner:
			result = false;
			break;
		}
		return result;
	}

	private static bool StandardDoubled(OfficeGradientVariants variant)
	{
		if (variant != OfficeGradientVariants.ShadingVariants_3 && variant != OfficeGradientVariants.ShadingVariants_4)
		{
			return false;
		}
		return true;
	}

	private static int GradientAngle(OfficeGradientStyle gradientStyle)
	{
		int result = -1;
		switch (gradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
			result = 5400000;
			break;
		case OfficeGradientStyle.Vertical:
			result = 0;
			break;
		case OfficeGradientStyle.DiagonalUp:
			result = 2700000;
			break;
		case OfficeGradientStyle.DiagonalDown:
			result = 18900000;
			break;
		}
		return result;
	}

	private static Rectangle GradientFillToRect(OfficeGradientStyle gradientStyle, OfficeGradientVariants variant)
	{
		Rectangle result = Rectangle.Empty;
		switch (gradientStyle)
		{
		case OfficeGradientStyle.FromCorner:
			result = RectanglesCorner[(int)variant];
			break;
		case OfficeGradientStyle.FromCenter:
			result = RectangleFromCenter;
			break;
		}
		return result;
	}

	private static GradientType GetGradientType(OfficeGradientStyle gradStyle)
	{
		switch (gradStyle)
		{
		case OfficeGradientStyle.Horizontal:
		case OfficeGradientStyle.Vertical:
		case OfficeGradientStyle.DiagonalUp:
		case OfficeGradientStyle.DiagonalDown:
			return GradientType.Liniar;
		case OfficeGradientStyle.FromCorner:
		case OfficeGradientStyle.FromCenter:
			return GradientType.Rect;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	internal void SetInnerShapes(object value, string property)
	{
		IShape[] items = (base.Parent as GroupShapeImpl).Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = items[i] as ShapeImpl;
			if (shapeImpl is ChartShapeImpl)
			{
				continue;
			}
			TextBoxShapeImpl textBoxShapeImpl = shapeImpl as TextBoxShapeImpl;
			if (!m_book.IsWorkbookOpening || textBoxShapeImpl == null || textBoxShapeImpl.IsGroupFill)
			{
				switch (property)
				{
				case "BackColor":
					shapeImpl.Fill.BackColor = (Color)value;
					break;
				case "BackColorIndex":
					shapeImpl.Fill.BackColorIndex = (OfficeKnownColors)value;
					break;
				case "FillType":
					shapeImpl.Fill.FillType = (OfficeFillType)value;
					break;
				case "ForeColor":
					shapeImpl.Fill.ForeColor = (Color)value;
					break;
				case "ForeColorIndex":
					shapeImpl.Fill.ForeColorIndex = (OfficeKnownColors)value;
					break;
				case "GradientColorType":
					shapeImpl.Fill.GradientColorType = (OfficeGradientColor)value;
					break;
				case "GradientDegree":
					shapeImpl.Fill.GradientDegree = (double)value;
					break;
				case "GradientStyle":
					shapeImpl.Fill.GradientStyle = (OfficeGradientStyle)value;
					break;
				case "GradientVariant":
					shapeImpl.Fill.GradientVariant = (OfficeGradientVariants)value;
					break;
				case "Pattern":
					shapeImpl.Fill.Pattern = (OfficeGradientPattern)value;
					break;
				case "PresetGradientType":
					shapeImpl.Fill.PresetGradientType = (OfficeGradientPreset)value;
					break;
				case "Texture":
					shapeImpl.Fill.Texture = (OfficeTexture)value;
					break;
				case "TextureHorizontalScale":
					shapeImpl.Fill.TextureHorizontalScale = (float)value;
					break;
				case "TextureOffsetX":
					shapeImpl.Fill.TextureOffsetX = (float)value;
					break;
				case "TextureOffsetY":
					shapeImpl.Fill.TextureOffsetY = (float)value;
					break;
				case "TextureVerticalScale":
					shapeImpl.Fill.TextureVerticalScale = (float)value;
					break;
				case "Transparency":
					shapeImpl.Fill.Transparency = (double)value;
					break;
				case "TransparencyColor":
					shapeImpl.Fill.TransparencyColor = (float)value;
					break;
				case "TransparencyFrom":
					shapeImpl.Fill.TransparencyFrom = (double)value;
					break;
				case "TransparencyTo":
					shapeImpl.Fill.TransparencyTo = (double)value;
					break;
				case "Visible":
					shapeImpl.Fill.Visible = (bool)value;
					break;
				case "ForeColorObject":
					(shapeImpl.Fill as ShapeFillImpl).SetForeColorObject((ChartColor)value);
					break;
				case "BackColorObject":
					(shapeImpl.Fill as ShapeFillImpl).SetBackColorObject((ChartColor)value);
					break;
				}
			}
		}
	}

	internal void SetInnerShapesFillVisible()
	{
		IShape[] items = (base.Parent as GroupShapeImpl).Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = items[i] as ShapeImpl;
			if (!(shapeImpl is ChartShapeImpl))
			{
				TextBoxShapeImpl textBoxShapeImpl = shapeImpl as TextBoxShapeImpl;
				AutoShapeImpl autoShapeImpl = shapeImpl as AutoShapeImpl;
				if (m_book.IsWorkbookOpening && textBoxShapeImpl != null && textBoxShapeImpl.IsGroupFill)
				{
					textBoxShapeImpl.Fill.Visible = false;
				}
				if (m_book.IsWorkbookOpening && autoShapeImpl != null && autoShapeImpl.IsGroupFill)
				{
					autoShapeImpl.Fill.Visible = false;
				}
			}
		}
	}

	private void SetBackColorObject(ChartColor value)
	{
		m_backColor = value;
	}

	private void SetForeColorObject(ChartColor value)
	{
		m_foreColor = value;
	}

	internal void Clear()
	{
		if (m_backColor != null)
		{
			m_backColor.Dispose();
		}
		if (m_foreColor != null)
		{
			m_foreColor.Dispose();
		}
		if (m_picture != null)
		{
			m_picture.Close();
		}
		if (m_preseredGradient != null)
		{
			m_preseredGradient.Dispose();
		}
		m_backColor = null;
		m_foreColor = null;
		m_picture = null;
		m_parsePictureData = null;
	}
}

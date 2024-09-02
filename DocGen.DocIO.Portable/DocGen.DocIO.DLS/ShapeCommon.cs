using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public abstract class ShapeCommon : ParagraphItem
{
	private long m_ID;

	private string m_Name;

	private string m_AlternateText;

	private string m_Title;

	private float m_Height;

	private float m_Width;

	private float m_widthScale = 100f;

	private float m_heightScale = 100f;

	internal Dictionary<string, Stream> m_docxProps = new Dictionary<string, Stream>();

	private Dictionary<int, object> m_propertiesHash;

	internal const byte WidthScaleKey = 0;

	private string m_path;

	private float m_coordinateXOrigin;

	private float m_coordinateYOrigin;

	private string m_coordinateSize;

	internal long ShapeID
	{
		get
		{
			return m_ID;
		}
		set
		{
			m_ID = value;
		}
	}

	public float Height
	{
		get
		{
			return m_Height;
		}
		set
		{
			if (this is WChart)
			{
				(this as WChart).OfficeChart.Height = value;
			}
			m_Height = value;
		}
	}

	public float Width
	{
		get
		{
			return m_Width;
		}
		set
		{
			if (this is WChart)
			{
				(this as WChart).OfficeChart.Width = value;
			}
			m_Width = value;
		}
	}

	public float HeightScale
	{
		get
		{
			return m_heightScale;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException("Scale factor must be greater than 0");
			}
			m_heightScale = value;
		}
	}

	public float WidthScale
	{
		get
		{
			if (this is Shape && (this as Shape).IsHorizontalRule && HasKey(0))
			{
				return (float)PropertiesHash[0];
			}
			return m_widthScale;
		}
		set
		{
			if (this is Shape && !(this as Shape).IsHorizontalRule && value <= 0f)
			{
				throw new ArgumentOutOfRangeException("Scale factor must be greater than 0");
			}
			m_widthScale = value;
			if (this is Shape && (this as Shape).IsHorizontalRule)
			{
				SetKeyValue(0, value);
			}
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

	public string AlternativeText
	{
		get
		{
			return m_AlternateText;
		}
		set
		{
			m_AlternateText = value;
		}
	}

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			m_Title = value;
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
		set
		{
			m_docxProps = value;
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

	internal void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	protected override object CloneImpl()
	{
		ShapeCommon shapeCommon = (ShapeCommon)base.CloneImpl();
		shapeCommon.ShapeID = 0L;
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			shapeCommon.m_doc.CloneProperties(DocxProps, ref shapeCommon.m_docxProps);
		}
		if (m_propertiesHash != null && m_propertiesHash.Count > 0)
		{
			m_propertiesHash = new Dictionary<int, object>();
			foreach (KeyValuePair<int, object> item in m_propertiesHash)
			{
				shapeCommon.PropertiesHash.Add(item.Key, item.Value);
			}
		}
		return shapeCommon;
	}

	internal ShapeCommon(WordDocument doc)
		: base(doc)
	{
	}

	internal RectangleF GetBoundsToLayoutShapeTextBody(AutoShapeType autoShapeType, Dictionary<string, string> shapeGuide, RectangleF bounds)
	{
		Dictionary<string, float> dictionary = WordDocument.RenderHelper.ParseShapeFormula(autoShapeType, shapeGuide, bounds);
		switch (autoShapeType)
		{
		case AutoShapeType.Oval:
		case AutoShapeType.Donut:
		case AutoShapeType.BlockArc:
		case AutoShapeType.Arc:
		case AutoShapeType.CircularArrow:
		case AutoShapeType.FlowChartConnector:
		case AutoShapeType.FlowChartSequentialAccessStorage:
		case AutoShapeType.DoubleWave:
		case AutoShapeType.CloudCallout:
		case AutoShapeType.Chord:
		case AutoShapeType.Cloud:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.DoubleBracket:
		case AutoShapeType.DoubleBrace:
		case AutoShapeType.FlowChartAlternateProcess:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.BentUpArrow:
			return new RectangleF(0f, dictionary["y2"], dictionary["x4"], bounds.Height);
		case AutoShapeType.Bevel:
			return new RectangleF(dictionary["x1"], dictionary["x1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.Can:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, dictionary["y3"]);
		case AutoShapeType.L_Shape:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.FlowChartDelay:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.Cube:
			return new RectangleF(0f, dictionary["y1"], dictionary["x4"], bounds.Height);
		case AutoShapeType.Decagon:
			return new RectangleF(dictionary["x1"], dictionary["y2"], dictionary["x4"], dictionary["y3"]);
		case AutoShapeType.DiagonalStripe:
			return new RectangleF(0f, 0f, dictionary["x3"], dictionary["y3"]);
		case AutoShapeType.Diamond:
		case AutoShapeType.FlowChartDecision:
		case AutoShapeType.FlowChartCollate:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FlowChartDisplay:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.Dodecagon:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x4"], dictionary["y4"]);
		case AutoShapeType.DownArrow:
			return new RectangleF(dictionary["x1"], 0f, dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.DownArrowCallout:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y2"]);
		case AutoShapeType.FlowChartDocument:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y1"]);
		case AutoShapeType.FlowChartExtract:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 2f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartData:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x5"], bounds.Height);
		case AutoShapeType.FlowChartInternalStorage:
			return new RectangleF(bounds.Width / 8f, bounds.Height / 8f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartMagneticDisk:
			return new RectangleF(0f, bounds.Height / 3f, bounds.Width, dictionary["y3"]);
		case AutoShapeType.FlowChartDirectAccessStorage:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartManualInput:
		case AutoShapeType.FlowChartCard:
			return new RectangleF(0f, bounds.Height / 5f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartManualOperation:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x3"], bounds.Height);
		case AutoShapeType.FlowChartMerge:
			return new RectangleF(bounds.Width / 4f, 0f, dictionary["x2"], bounds.Height / 2f);
		case AutoShapeType.FlowChartMultiDocument:
			return new RectangleF(0f, dictionary["y2"], dictionary["x5"], dictionary["y8"]);
		case AutoShapeType.FlowChartOffPageConnector:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y1"]);
		case AutoShapeType.FlowChartStoredData:
			return new RectangleF(bounds.Width / 6f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.Parallelogram:
		case AutoShapeType.Hexagon:
		case AutoShapeType.Cross:
		case AutoShapeType.SmileyFace:
		case AutoShapeType.NoSymbol:
		case AutoShapeType.FlowChartTerminator:
		case AutoShapeType.FlowChartSummingJunction:
		case AutoShapeType.FlowChartOr:
		case AutoShapeType.Star16Point:
		case AutoShapeType.Star24Point:
		case AutoShapeType.Star32Point:
		case AutoShapeType.Wave:
		case AutoShapeType.OvalCallout:
		case AutoShapeType.SnipSameSideCornerRectangle:
		case AutoShapeType.Teardrop:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FlowChartPredefinedProcess:
			return new RectangleF(bounds.Width / 8f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.FlowChartPreparation:
			return new RectangleF(bounds.Width / 5f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.UTurnArrow:
		case AutoShapeType.FlowChartProcess:
		case AutoShapeType.RectangularCallout:
		case AutoShapeType.StraightConnector:
			return new RectangleF(0f, 0f, bounds.Width, bounds.Height);
		case AutoShapeType.FlowChartPunchedTape:
			return new RectangleF(0f, bounds.Height / 5f, bounds.Width, dictionary["ib"]);
		case AutoShapeType.FlowChartSort:
			return new RectangleF(bounds.Width / 4f, bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.FoldedCorner:
			return new RectangleF(0f, 0f, bounds.Width, dictionary["y2"]);
		case AutoShapeType.Frame:
			return new RectangleF(dictionary["x1"], dictionary["x1"], dictionary["x4"], dictionary["y4"]);
		case AutoShapeType.Heart:
			return new RectangleF(dictionary["il"], bounds.Height / 4f, dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.Heptagon:
			return new RectangleF(dictionary["x2"], dictionary["y1"], dictionary["x5"], dictionary["ib"]);
		case AutoShapeType.Pentagon:
		case AutoShapeType.RoundSingleCornerRectangle:
			return new RectangleF(0f, 0f, dictionary["ir"], bounds.Height);
		case AutoShapeType.HorizontalScroll:
			return new RectangleF(dictionary["ch"], dictionary["ch"], dictionary["x4"], dictionary["y6"]);
		case AutoShapeType.Explosion1:
			return new RectangleF(dictionary["x5"], dictionary["y3"], dictionary["x21"], dictionary["y9"]);
		case AutoShapeType.Explosion2:
			return new RectangleF(dictionary["x5"], dictionary["y3"], dictionary["x19"], dictionary["y17"]);
		case AutoShapeType.LeftArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], bounds.Width, dictionary["y2"]);
		case AutoShapeType.LeftArrowCallout:
			return new RectangleF(dictionary["x2"], 0f, bounds.Width, bounds.Height);
		case AutoShapeType.LeftBracket:
		case AutoShapeType.LeftBrace:
			return new RectangleF(dictionary["il"], dictionary["it"], bounds.Width, dictionary["ib"]);
		case AutoShapeType.LeftRightArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x4"], dictionary["y2"]);
		case AutoShapeType.LeftRightArrowCallout:
			return new RectangleF(dictionary["x2"], 0f, dictionary["x3"], bounds.Height);
		case AutoShapeType.LeftRightUpArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["ir"], dictionary["y5"]);
		case AutoShapeType.LeftUpArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["x4"], dictionary["y5"]);
		case AutoShapeType.LightningBolt:
			return new RectangleF(dictionary["x4"], dictionary["y4"], dictionary["x9"], dictionary["y10"]);
		case AutoShapeType.MathDivision:
			return new RectangleF(dictionary["x1"], dictionary["y3"], dictionary["x3"], dictionary["y4"]);
		case AutoShapeType.UpArrow:
		case AutoShapeType.MathEqual:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], bounds.Height);
		case AutoShapeType.UpDownArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], dictionary["y4"]);
		case AutoShapeType.MathMinus:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.MathMultiply:
			return new RectangleF(dictionary["xA"], dictionary["yB"], dictionary["xE"], dictionary["yH"]);
		case AutoShapeType.MathNotEqual:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x8"], dictionary["y4"]);
		case AutoShapeType.MathPlus:
			return new RectangleF(dictionary["x1"], dictionary["y2"], dictionary["x4"], dictionary["y3"]);
		case AutoShapeType.Moon:
			return new RectangleF(dictionary["g12w"], dictionary["g15h"], dictionary["g0w"], dictionary["g16h"]);
		case AutoShapeType.Trapezoid:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.NotchedRightArrow:
			return new RectangleF(dictionary["x1"], dictionary["y1"], dictionary["x3"], dictionary["y2"]);
		case AutoShapeType.RoundedRectangle:
		case AutoShapeType.Octagon:
		case AutoShapeType.Plaque:
		case AutoShapeType.RoundedRectangularCallout:
		case AutoShapeType.SnipDiagonalCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RegularPentagon:
			return new RectangleF(dictionary["x2"], dictionary["it"], dictionary["x3"], dictionary["y2"]);
		case AutoShapeType.Pie:
			return new RectangleF(dictionary["il"], dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.QuadArrow:
			return new RectangleF(dictionary["il"], dictionary["y3"], dictionary["ir"], dictionary["y4"]);
		case AutoShapeType.QuadArrowCallout:
			return new RectangleF(dictionary["x2"], dictionary["y2"], dictionary["x7"], dictionary["y7"]);
		case AutoShapeType.DownRibbon:
			return new RectangleF(dictionary["x2"], dictionary["y2"], dictionary["x9"], bounds.Height);
		case AutoShapeType.UpRibbon:
			return new RectangleF(dictionary["x2"], 0f, dictionary["x9"], dictionary["y2"]);
		case AutoShapeType.RightArrow:
			return new RectangleF(0f, dictionary["y1"], dictionary["x2"], dictionary["y2"]);
		case AutoShapeType.RightArrowCallout:
			return new RectangleF(0f, 0f, dictionary["x2"], bounds.Height);
		case AutoShapeType.RightBracket:
		case AutoShapeType.RightBrace:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RoundDiagonalCornerRectangle:
			return new RectangleF(dictionary["dx"], dictionary["dx"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RoundSameSideCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["tdx"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.RightTriangle:
			return new RectangleF(bounds.Width / 12f, dictionary["it"], dictionary["ir"], dictionary["ib"]);
		case AutoShapeType.SnipSingleCornerRectangle:
			return new RectangleF(0f, dictionary["it"], dictionary["ir"], bounds.Height);
		case AutoShapeType.SnipAndRoundSingleCornerRectangle:
			return new RectangleF(dictionary["il"], dictionary["il"], dictionary["ir"], bounds.Height);
		case AutoShapeType.Star10Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy2"], dictionary["sx5"], dictionary["sy3"]);
		case AutoShapeType.Star12Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy2"], dictionary["sx5"], dictionary["sy5"]);
		case AutoShapeType.Star4Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx2"], dictionary["sy2"]);
		case AutoShapeType.Star5Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy3"]);
		case AutoShapeType.Star6Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy2"]);
		case AutoShapeType.Star7Point:
			return new RectangleF(dictionary["sx2"], dictionary["sy1"], dictionary["sx5"], dictionary["sy3"]);
		case AutoShapeType.Star8Point:
			return new RectangleF(dictionary["sx1"], dictionary["sy1"], dictionary["sx4"], dictionary["sy4"]);
		case AutoShapeType.StripedRightArrow:
			return new RectangleF(dictionary["x4"], dictionary["y1"], dictionary["x6"], dictionary["y2"]);
		case AutoShapeType.Sun:
			return new RectangleF(dictionary["x9"], dictionary["y9"], dictionary["x8"], dictionary["y8"]);
		case AutoShapeType.IsoscelesTriangle:
			return new RectangleF(dictionary["x1"], bounds.Height / 2f, dictionary["x3"], bounds.Height);
		case AutoShapeType.UpArrowCallout:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, bounds.Height);
		case AutoShapeType.UpDownArrowCallout:
			return new RectangleF(0f, dictionary["y2"], bounds.Width, dictionary["y3"]);
		case AutoShapeType.VerticalScroll:
			return new RectangleF(dictionary["ch"], dictionary["ch"], dictionary["x6"], dictionary["y4"]);
		case AutoShapeType.CurvedUpRibbon:
			return new RectangleF(dictionary["x2"], dictionary["y6"], dictionary["x5"], dictionary["rh"]);
		case AutoShapeType.CurvedDownRibbon:
			return new RectangleF(dictionary["x2"], dictionary["q1"], dictionary["x5"], dictionary["y6"]);
		case AutoShapeType.Chevron:
			return new RectangleF(dictionary["il"], 0f, dictionary["ir"], bounds.Height);
		default:
			return new RectangleF(0f, 0f, bounds.Width, bounds.Height);
		}
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_docxProps != null)
		{
			foreach (Stream value in m_docxProps.Values)
			{
				value.Close();
			}
			m_docxProps.Clear();
			m_docxProps = null;
		}
		base.Close();
	}
}

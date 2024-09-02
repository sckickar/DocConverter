using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DocGen.Compression;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartParserCommon
{
	private const string NullString = "null";

	private const int DefaultShadowSize = 100;

	private const int DefaultBlurValue = 0;

	private const int DefaultAngleValue = 0;

	private const int DefaultDistanceValue = 0;

	private static Dictionary<KeyValuePair<string, string>, OfficeChartLinePattern> s_dicLinePatterns;

	private static WorkbookImpl m_book;

	static ChartParserCommon()
	{
		s_dicLinePatterns = new Dictionary<KeyValuePair<string, string>, OfficeChartLinePattern>();
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("solid", string.Empty), OfficeChartLinePattern.Solid);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("lgDash", string.Empty), OfficeChartLinePattern.LongDash);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("sysDash", string.Empty), OfficeChartLinePattern.Dot);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("sysDot", string.Empty), OfficeChartLinePattern.CircleDot);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("dash", string.Empty), OfficeChartLinePattern.Dash);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("lgDashDot", string.Empty), OfficeChartLinePattern.LongDashDot);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("dashDot", string.Empty), OfficeChartLinePattern.DashDot);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("lgDashDotDot", string.Empty), OfficeChartLinePattern.LongDashDotDot);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("solid", "pct75"), OfficeChartLinePattern.DarkGray);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("solid", "pct50"), OfficeChartLinePattern.MediumGray);
		s_dicLinePatterns.Add(new KeyValuePair<string, string>("solid", "pct25"), OfficeChartLinePattern.LightGray);
	}

	public static void SetWorkbook(WorkbookImpl book)
	{
		m_book = book;
	}

	public static void ParseTextArea(XmlReader reader, IInternalOfficeChartTextArea textArea, FileDataHolder holder, RelationCollection relations)
	{
		ParseTextArea(reader, textArea, holder, relations, null);
	}

	public static void ParseTextArea(XmlReader reader, IInternalOfficeChartTextArea textArea, FileDataHolder holder, RelationCollection relations, float? defaultFontSize)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				ParseTextAreaTag(reader, textArea, relations, holder, defaultFontSize);
			}
		}
		reader.Read();
	}

	public static void ParseTextAreaTag(XmlReader reader, IInternalOfficeChartTextArea textArea, RelationCollection relations, FileDataHolder holder, float? defaultFontSize)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			switch (reader.LocalName)
			{
			case "tx":
				textArea.Text = string.Empty;
				if (textArea is ChartDataLabelsImpl)
				{
					bool showTextProperties = (textArea as ChartDataLabelsImpl).ShowTextProperties;
					ParseTextAreaText(reader, textArea, holder.Parser, defaultFontSize);
					if (!showTextProperties)
					{
						(textArea as ChartDataLabelsImpl).ShowTextProperties = false;
					}
				}
				else
				{
					ParseTextAreaText(reader, textArea, holder.Parser, defaultFontSize);
				}
				break;
			case "layout":
				(textArea as ChartTextAreaImpl).Layout = new ChartLayoutImpl(holder.Workbook.Application, textArea as ChartTextAreaImpl, textArea.Parent);
				ParseChartLayout(reader, (textArea as ChartTextAreaImpl).Layout);
				break;
			case "spPr":
			{
				IOfficeChartFrameFormat frameFormat = textArea.FrameFormat;
				IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(frameFormat.Border as ChartBorderImpl, frameFormat.Interior as ChartInteriorImpl, frameFormat.Fill as IInternalFill, frameFormat.Shadow as ShadowImpl, frameFormat.ThreeD as ThreeDFormatImpl);
				ParseShapeProperties(reader, objectGetter, holder, relations);
				break;
			}
			case "txPr":
				if (!((ChartTextAreaImpl)textArea).IsTextParsed)
				{
					((ChartTextAreaImpl)textArea).ParagraphType = ChartParagraphType.CustomDefault;
					ParseDefaultTextFormatting(reader, textArea, holder.Parser, defaultFontSize);
				}
				else
				{
					reader.Skip();
				}
				break;
			case "overlay":
				if (textArea is ChartTextAreaImpl chartTextAreaImpl)
				{
					chartTextAreaImpl.Overlay = ParseBoolValueTag(reader);
				}
				break;
			case "unitsLabel":
				reader.Read();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		else
		{
			reader.Skip();
		}
	}

	internal static void ParseDefaultTextFormatting(XmlReader reader, IInternalOfficeChartTextArea textFormatting, Excel2007Parser parser, double? defaultFontSize)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textFormatting == null)
		{
			throw new ArgumentNullException("textFormatting");
		}
		if (reader.LocalName != "txPr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement && reader.LocalName != "txPr" && reader.LocalName != "defRPr")
		{
			if (reader.LocalName == "bodyPr")
			{
				if (reader.MoveToAttribute("rot"))
				{
					textFormatting.TextRotationAngle = XmlConvertExtension.ToInt32(reader.Value) / 60000;
				}
				reader.Skip();
			}
			else
			{
				reader.Read();
			}
		}
		if (defaultFontSize.HasValue)
		{
			textFormatting.Size = defaultFontSize.Value;
		}
		if (reader.LocalName == "defRPr")
		{
			ParseParagraphRunProperites(reader, textFormatting, parser, null);
			while (reader.LocalName != "txPr")
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	public static string ParseValueTag(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string result = ((!reader.MoveToAttribute("val")) ? AssignValTag(reader.LocalName) : reader.Value);
		reader.Read();
		return result;
	}

	private static string AssignValTag(string localName)
	{
		switch (localName)
		{
		case "baseTimeUnit":
		case "majorTimeUnit":
		case "minorTimeUnit":
			return "days";
		case "depthPercent":
		case "hPercent":
			return "100";
		case "manualLayout":
		case "hMode":
		case "xMode":
		case "yMode":
		case "wMode":
			return "factor";
		case "overlap":
		case "rotX":
		case "rotY":
		case "thickness":
		case "idx":
			return "0";
		case "orientation":
			return "minMax";
		case "errDir":
			return null;
		case "barDir":
			return "col";
		case "grouping":
			return "clustered";
		case "shape":
			return "box";
		case "sizeRepresents":
			return "area";
		case "crossBetween":
			return "between";
		case "crosses":
			return "autoZero";
		case "errBarType":
			return "both";
		case "errValType":
			return "fixedVal";
		case "gapWidth":
			return "150";
		case "lblAlgn":
			return "ctr";
		case "layoutTarget":
			return "outer";
		case "legendPos":
			return "r";
		case "symbol":
			return "none";
		case "ofPieType":
			return "pie";
		case "perspective":
			return "30";
		case "radarStyle":
			return "standard";
		case "rAngAx":
			return "true";
		case "scatterStyle":
			return "marker";
		case "order":
			return "2";
		case "splitType":
			return "auto";
		case "tickLblPos":
			return "nextTo";
		case "trendlineType":
			return "linear";
		case "prstDash":
			return null;
		default:
			throw new XmlException();
		}
	}

	public static bool ParseBoolValueTag(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return XmlConvertExtension.ToBoolean(ParseValueTag(reader));
	}

	public static int ParseIntValueTag(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return XmlConvertExtension.ToInt32(ParseValueTag(reader));
	}

	public static double ParseDoubleValueTag(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return XmlConvertExtension.ToDouble(ParseValueTag(reader));
	}

	public static void ParseLineProperties(XmlReader reader, ChartBorderImpl border, Excel2007Parser parser)
	{
		ParseLineProperties(reader, border, bRoundCorners: false, parser);
	}

	public static void ParsePatternFill(XmlReader reader, IOfficeFill fill, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "pattFill")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string text = (reader.MoveToAttribute("prst") ? reader.Value : null);
		OfficeGradientPattern pattern = (OfficeGradientPattern)((text != null) ? ((Excel2007GradientPattern)Enum.Parse(typeof(Excel2007GradientPattern), text, ignoreCase: false)) : Excel2007GradientPattern.dashDnDiag);
		fill.Pattern = pattern;
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					int transparecy;
					int tint;
					int shade;
					if (!(localName == "fgClr"))
					{
						if (localName == "bgClr")
						{
							reader.Read();
							Color backColor = ReadColor(reader, out transparecy, out tint, out shade, parser);
							fill.BackColor = backColor;
							reader.Read();
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						reader.Read();
						Color backColor = ReadColor(reader, out transparecy, out tint, out shade, parser);
						fill.ForeColor = backColor;
						reader.Read();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	public static void ParseSolidFill(XmlReader reader, ChartInteriorImpl interior, Excel2007Parser parser, out int Alpha)
	{
		if (interior == null)
		{
			throw new ArgumentNullException("interior");
		}
		ParseSolidFill(reader, parser, interior.ForegroundColorObject, out Alpha);
		interior.UseAutomaticFormat = false;
	}

	public static void ParseSolidFill(XmlReader reader, ChartInteriorImpl interior, Excel2007Parser parser)
	{
		if (interior == null)
		{
			throw new ArgumentNullException("interior");
		}
		ParseSolidFill(reader, parser, interior.ForegroundColorObject, out var _);
	}

	public static void ParseSolidFill(XmlReader reader, Excel2007Parser parser, ChartColor color)
	{
		int Alpha = 100000;
		ParseSolidFill(reader, parser, color, out Alpha);
	}

	public static void ParseSolidFill(XmlReader reader, Excel2007Parser parser, IInternalFill fill)
	{
		int Alpha = 100000;
		ParseSolidFill(reader, parser, fill.ForeColorObject, out Alpha);
		fill.Transparency = 1f - (float)Alpha / 100000f;
	}

	public static void ParseSolidFill(XmlReader reader, Excel2007Parser parser, ChartColor color, out int Alpha)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "solidFill")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		Alpha = 100000;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					int tint;
					int shade;
					switch (reader.LocalName)
					{
					case "srgbClr":
						color.SetRGB(ParseSRgbColor(reader, out Alpha, out tint, out shade, parser));
						break;
					case "schemeClr":
						color.SetRGB(ParseSchemeColor(reader, out Alpha, parser));
						break;
					case "sysClr":
						color.SetRGB(ReadColor(reader, out Alpha, out tint, out shade, parser));
						break;
					case "prstClr":
						color.SetRGB(ParsePresetColor(reader, out Alpha, parser));
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Read();
				}
			}
		}
		reader.Read();
	}

	public static Color ParseSRgbColor(XmlReader reader, Excel2007Parser parser)
	{
		int alpha;
		int tint;
		int shade;
		return ParseSRgbColor(reader, out alpha, out tint, out shade, parser);
	}

	public static Color ParseSRgbColor(XmlReader reader, out int alpha, out int tint, out int shade, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "srgbClr")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		Color empty = ColorExtension.Empty;
		alpha = 100000;
		tint = -1;
		shade = -1;
		if (reader.MoveToAttribute("val"))
		{
			string value = reader.Value;
			empty = (string.IsNullOrEmpty(value) ? Color.Empty : ColorExtension.FromArgb(int.Parse(value, NumberStyles.HexNumber, null)));
			reader.MoveToElement();
			reader.Read();
			if (!isEmptyElement)
			{
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.LocalName)
						{
						case "alpha":
							alpha = ParseIntValueTag(reader);
							break;
						case "gamma":
							reader.Skip();
							break;
						case "invGamma":
							reader.Skip();
							break;
						case "tint":
							tint = ParseIntValueTag(reader);
							break;
						case "shade":
							shade = ParseIntValueTag(reader);
							break;
						default:
							empty = ParseColorUpdater(reader, empty, parser, out alpha);
							break;
						}
					}
					else
					{
						reader.Skip();
					}
				}
				reader.Read();
			}
			return empty;
		}
		throw new XmlException();
	}

	public static Color ParseSchemeColor(XmlReader reader, Excel2007Parser parser)
	{
		int alpha;
		return ParseSchemeColor(reader, out alpha, parser);
	}

	public static Color ParseSchemeColor(XmlReader reader, out int alpha, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException();
		}
		if (reader.LocalName != "schemeClr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		alpha = 100000;
		string colorName = null;
		if (reader.MoveToAttribute("val"))
		{
			colorName = reader.Value;
		}
		Color result = parser.GetThemeColor(colorName);
		reader.MoveToElement();
		if (!isEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					result = ParseColorUpdater(reader, result, parser, out alpha);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		return result;
	}

	public static Color ParsePresetColor(XmlReader reader, out int alpha, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException();
		}
		if (reader.LocalName != "prstClr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		alpha = 100000;
		string name = null;
		if (reader.MoveToAttribute("val"))
		{
			name = reader.Value;
		}
		Color result = ColorExtension.FromName(name);
		reader.MoveToElement();
		if (!isEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					result = ParseColorUpdater(reader, result, parser, out alpha);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		return result;
	}

	public static Color ParseSystemColor(XmlReader reader, out int alpha, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException();
		}
		if (reader.LocalName != "sysClr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		alpha = 100000;
		Color result = ((!reader.MoveToAttribute("lastClr")) ? ColorExtension.Empty : ColorExtension.FromArgb(int.Parse(reader.Value, NumberStyles.HexNumber, null)));
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					result = ParseColorUpdater(reader, result, parser, out alpha);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		return result;
	}

	private static Color ParseColorUpdater(XmlReader reader, Color result, Excel2007Parser parser, out int alpha)
	{
		alpha = 100000;
		double dHue;
		double dLuminance;
		double dSaturation;
		switch (reader.LocalName)
		{
		case "lumMod":
		{
			int num2 = ParseIntValueTag(reader);
			Excel2007Parser.ConvertRGBtoHLS(result, out dHue, out dLuminance, out dSaturation);
			dLuminance *= (double)num2 / 100000.0;
			result = Excel2007Parser.ConvertHLSToRGB(dHue, dLuminance, dSaturation);
			break;
		}
		case "lumOff":
		{
			int num3 = ParseIntValueTag(reader);
			Excel2007Parser.ConvertRGBtoHLS(result, out dHue, out dLuminance, out dSaturation);
			dLuminance += (double)(255 * num3) / 100000.0;
			result = Excel2007Parser.ConvertHLSToRGB(dHue, dLuminance, dSaturation);
			break;
		}
		case "satMod":
		{
			int num4 = ParseIntValueTag(reader);
			Excel2007Parser.ConvertRGBtoHLS(result, out dHue, out dLuminance, out dSaturation);
			dSaturation *= (double)num4 / 100000.0;
			result = Excel2007Parser.ConvertHLSToRGB(dHue, dLuminance, dSaturation);
			break;
		}
		case "tint":
		{
			double num = ParseDoubleValueTag(reader);
			if (num > 100.0)
			{
				num /= 100000.0;
			}
			result = Excel2007Parser.ConvertColorByTintBlip(result, num);
			break;
		}
		case "shade":
		{
			double shade = (double)ParseIntValueTag(reader) / 100000.0;
			result = parser.ConvertColorByShadeBlip(result, shade);
			break;
		}
		case "alpha":
			alpha = ParseIntValueTag(reader);
			break;
		default:
			reader.Skip();
			break;
		}
		return result;
	}

	private static void ParseLineProperties(XmlReader reader, ChartBorderImpl border, bool bRoundCorners, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (border == null)
		{
			throw new ArgumentNullException("border");
		}
		if (reader.LocalName != "ln")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		bool flag = border.IsAutoLineColor;
		if (reader.MoveToAttribute("w"))
		{
			int lineWeight = (int)((double)int.Parse(reader.Value) / 12700.0) - 1;
			border.LineWeight = (OfficeChartLineWeight)lineWeight;
			border.LineWeightString = reader.Value;
			if (flag && !border.IsAutoLineColor)
			{
				border.IsAutoLineColor = flag;
				flag = false;
			}
		}
		else
		{
			border.LineWeight = OfficeChartLineWeight.Hairline;
		}
		if (reader.MoveToAttribute("cap"))
		{
			string value = reader.Value;
			LineCap lineCap = LineCap.Custom;
			switch (value)
			{
			case "sq":
				lineCap = LineCap.Square;
				break;
			case "rnd":
				lineCap = LineCap.Round;
				break;
			case "flat":
				lineCap = LineCap.Flat;
				break;
			}
			if (lineCap != LineCap.Custom)
			{
				border.CapStyle = lineCap;
			}
		}
		if (reader.MoveToAttribute("cmpd"))
		{
			Excel2007ShapeLineStyle lineStyle = (Excel2007ShapeLineStyle)Enum.Parse(typeof(Excel2007ShapeLineStyle), reader.Value, ignoreCase: false);
			border.LineStyle = lineStyle;
		}
		int Alpha = 100000;
		bool flag2 = false;
		string text = null;
		if (!isEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "noFill":
						border.LinePattern = OfficeChartLinePattern.None;
						border.HasLineProperties = true;
						reader.Read();
						break;
					case "round":
						border.JoinType = Excel2007BorderJoinType.Round;
						reader.Read();
						break;
					case "miter":
						border.JoinType = Excel2007BorderJoinType.Mitter;
						reader.Read();
						break;
					case "bevel":
						border.JoinType = Excel2007BorderJoinType.Bevel;
						reader.Read();
						break;
					case "solidFill":
					{
						border.Color.AfterChange += border.ClearAutoColor;
						bool isEmptyElement2 = reader.IsEmptyElement;
						ParseSolidFill(reader, parser, border.Color, out Alpha);
						border.Transparency = 1f - (float)Alpha / 100000f;
						border.Color.AfterChange -= border.ClearAutoColor;
						border.AutoFormat = false;
						if (!flag)
						{
							border.TryAndClearAutoColor();
						}
						if (border.IsAutoLineColor && !isEmptyElement2 && border.FindParent(typeof(ChartDataPointImpl)) is ChartDataPointImpl { IsDefault: false })
						{
							border.IsAutoLineColor = false;
						}
						flag2 = true;
						break;
					}
					case "prstDash":
						text = ParseValueTag(reader);
						break;
					case "pattFill":
						reader.Skip();
						break;
					case "gradFill":
					{
						border.AutoFormat = false;
						GradientStops gradientStops = ParseGradientFill(reader, parser);
						ConvertGradientStopsToProperties(gradientStops, border.Fill);
						border.Fill.PreservedGradient = gradientStops;
						border.HasLineProperties = true;
						if (!flag)
						{
							border.TryAndClearAutoColor();
						}
						break;
					}
					case "headEnd":
						ParseArrowSettings(reader, border, isHead: true);
						break;
					case "tailEnd":
						ParseArrowSettings(reader, border, isHead: false);
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (flag2 && string.IsNullOrEmpty(text))
		{
			border.LinePattern = OfficeChartLinePattern.Solid;
		}
		else if (border.LinePattern != OfficeChartLinePattern.None && text != null)
		{
			KeyValuePair<string, string> key = new KeyValuePair<string, string>(text, string.Empty);
			if (s_dicLinePatterns.TryGetValue(key, out var value2))
			{
				border.LinePattern = value2;
			}
		}
		if (!isEmptyElement)
		{
			border.HasLineProperties = true;
		}
		reader.Read();
	}

	private static void ParseArrowSettings(XmlReader reader, IOfficeChartBorder border, bool isHead)
	{
		if (reader.HasAttributes)
		{
			if (reader.MoveToAttribute("type"))
			{
				if (isHead)
				{
					border.BeginArrowType = GetArrowType(reader.Value);
					border.BeginArrowSize = OfficeArrowSize.ArrowLSize5;
				}
				else
				{
					border.EndArrowType = GetArrowType(reader.Value);
					border.EndArrowSize = OfficeArrowSize.ArrowLSize5;
				}
			}
			if (reader.MoveToAttribute("w"))
			{
				if (isHead)
				{
					(border as ChartBorderImpl).m_beginArrowwidth = reader.Value;
				}
				else
				{
					(border as ChartBorderImpl).m_endArrowWidth = reader.Value;
				}
			}
			if (reader.MoveToAttribute("len"))
			{
				if (isHead)
				{
					(border as ChartBorderImpl).m_beginArrowLg = reader.Value;
					if ((border as ChartBorderImpl).m_beginArrowwidth != null && (border as ChartBorderImpl).m_beginArrowLg != null)
					{
						border.BeginArrowSize = GetArrowSize((border as ChartBorderImpl).m_beginArrowLg, (border as ChartBorderImpl).m_beginArrowwidth);
					}
				}
				else
				{
					(border as ChartBorderImpl).m_endArrowLg = reader.Value;
					if ((border as ChartBorderImpl).m_endArrowWidth != null && (border as ChartBorderImpl).m_endArrowLg != null)
					{
						border.EndArrowSize = GetArrowSize((border as ChartBorderImpl).m_endArrowLg, (border as ChartBorderImpl).m_endArrowWidth);
					}
				}
			}
		}
		reader.Skip();
	}

	private static OfficeArrowType GetArrowType(string value)
	{
		return value switch
		{
			"arrow" => OfficeArrowType.OpenArrow, 
			"diamond" => OfficeArrowType.DiamondArrow, 
			"none" => OfficeArrowType.None, 
			"oval" => OfficeArrowType.OvalArrow, 
			"stealth" => OfficeArrowType.StealthArrow, 
			"triangle" => OfficeArrowType.Arrow, 
			_ => OfficeArrowType.None, 
		};
	}

	private static OfficeArrowSize GetArrowSize(string length, string width)
	{
		if (length == "sm" && width == "sm")
		{
			return OfficeArrowSize.ArrowLSize1;
		}
		if (length == "sm" && width == "med")
		{
			return OfficeArrowSize.ArrowLSize2;
		}
		if (length == "sm" && width == "lg")
		{
			return OfficeArrowSize.ArrowLSize3;
		}
		if (length == "med" && width == "sm")
		{
			return OfficeArrowSize.ArrowLSize4;
		}
		if (length == "med" && width == "med")
		{
			return OfficeArrowSize.ArrowLSize5;
		}
		if (length == "med" && width == "lg")
		{
			return OfficeArrowSize.ArrowLSize6;
		}
		if (length == "lg" && width == "sm")
		{
			return OfficeArrowSize.ArrowLSize7;
		}
		if (length == "lg" && width == "med")
		{
			return OfficeArrowSize.ArrowLSize8;
		}
		if (length == "lg" && width == "lg")
		{
			return OfficeArrowSize.ArrowLSize9;
		}
		return OfficeArrowSize.ArrowLSize1;
	}

	public static void ParsePictureFill(XmlReader reader, IOfficeFill fill, RelationCollection relations, FileDataHolder holder)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		if (reader.LocalName != "blipFill")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "blip":
					if (reader.MoveToAttribute("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
					{
						string value = reader.Value;
						CommonObject commonObject = fill as CommonObject;
						ChartImpl chartImpl = null;
						if (commonObject != null)
						{
							chartImpl = commonObject.FindParent(typeof(ChartImpl)) as ChartImpl;
						}
						if (chartImpl != null && chartImpl.RelationPreservedStreamCollection.Count > 0 && chartImpl.RelationPreservedStreamCollection.ContainsKey(value))
						{
							Image im = ApplicationImpl.CreateImage(chartImpl.RelationPreservedStreamCollection[value]);
							fill.UserPicture(im, "image");
						}
					}
					reader.Read();
					while (reader.LocalName != "srcRect" && reader.LocalName != "tile" && reader.LocalName != "stretch")
					{
						if (reader.LocalName == "alphaModFix")
						{
							int num = (reader.MoveToAttribute("amt") ? int.Parse(reader.Value) : 0);
							(fill as IInternalFill).TransparencyColor = 1f - (float)num / 100000f;
							reader.Read();
						}
						reader.Read();
					}
					break;
				case "tile":
					if (reader.NodeType != XmlNodeType.EndElement)
					{
						if (reader.MoveToAttribute("tx"))
						{
							(fill as IInternalFill).TextureOffsetX = float.Parse(reader.Value) / 12700f;
						}
						if (reader.MoveToAttribute("ty"))
						{
							(fill as IInternalFill).TextureOffsetY = float.Parse(reader.Value) / 12700f;
						}
						if (reader.MoveToAttribute("sx"))
						{
							(fill as IInternalFill).TextureHorizontalScale = float.Parse(reader.Value) / 100000f;
						}
						if (reader.MoveToAttribute("sy"))
						{
							(fill as IInternalFill).TextureVerticalScale = float.Parse(reader.Value) / 100000f;
						}
						if (reader.MoveToAttribute("flip"))
						{
							(fill as IInternalFill).TileFlipping = reader.Value;
						}
						if (reader.MoveToAttribute("algn"))
						{
							(fill as IInternalFill).Tile = true;
							(fill as IInternalFill).Alignment = reader.Value;
						}
					}
					reader.Read();
					break;
				case "stretch":
				{
					reader.Read();
					_ = reader.LocalName == "fillRect";
					int left2 = (reader.MoveToAttribute("l") ? int.Parse(reader.Value) : 0);
					int top2 = (reader.MoveToAttribute("t") ? int.Parse(reader.Value) : 0);
					int right2 = (reader.MoveToAttribute("r") ? int.Parse(reader.Value) : 0);
					int bottom2 = (reader.MoveToAttribute("b") ? int.Parse(reader.Value) : 0);
					(fill as ShapeFillImpl).FillRect = Rectangle.FromLTRB(left2, top2, right2, bottom2);
					reader.Read();
					reader.Read();
					break;
				}
				case "srcRect":
				{
					int left = (reader.MoveToAttribute("l") ? int.Parse(reader.Value) : 0);
					int top = (reader.MoveToAttribute("t") ? int.Parse(reader.Value) : 0);
					int right = (reader.MoveToAttribute("r") ? int.Parse(reader.Value) : 0);
					int bottom = (reader.MoveToAttribute("b") ? int.Parse(reader.Value) : 0);
					(fill as ShapeFillImpl).SourceRect = Rectangle.FromLTRB(left, top, right, bottom);
					reader.Read();
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private static void ParseTextAreaText(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, float? defaultFontSize)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "tx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "rich":
				{
					List<ChartAlrunsRecord.TRuns> tRuns = null;
					if (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).ChartAlRuns != null)
					{
						tRuns = new List<ChartAlrunsRecord.TRuns>((textArea as ChartTextAreaImpl).ChartAlRuns.Runs);
					}
					else if (textArea is ChartDataLabelsImpl && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns != null)
					{
						tRuns = new List<ChartAlrunsRecord.TRuns>((textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns.Runs);
					}
					ParseRichText(reader, textArea, parser, defaultFontSize, tRuns);
					break;
				}
				case "strRef":
					ParseStringReference(reader, textArea);
					break;
				case "txData":
				{
					string formula = null;
					textArea.Text = ParseFormulaOrValue(reader, out formula);
					if (formula != null && parser.Workbook != null)
					{
						IRange range = ChartParser.GetRange(parser.Workbook, formula);
						if (range != null && !(range is ExternalRange))
						{
							textArea.Text = range.Text;
						}
					}
					reader.Read();
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	internal static string ParseFormulaOrValue(XmlReader reader, out string formula)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "txData")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string result = null;
		formula = null;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "v"))
					{
						if (localName == "f")
						{
							formula = reader.ReadElementContentAsString();
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						result = reader.ReadElementContentAsString();
					}
				}
				else
				{
					reader.Read();
				}
			}
		}
		else
		{
			reader.Skip();
		}
		return result;
	}

	public static void ParseChartLayout(XmlReader reader, IOfficeChartLayout layout)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (layout == null)
		{
			throw new ArgumentNullException("layout");
		}
		if (reader.LocalName != "layout")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			IOfficeChartManualLayout officeChartManualLayout = null;
			if (reader.IsStartElement() && reader.LocalName == "manualLayout")
			{
				officeChartManualLayout = layout.ManualLayout;
			}
			if (officeChartManualLayout == null)
			{
				return;
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "manualLayout")
					{
						ParseManualLayout(reader, officeChartManualLayout);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
			reader.Read();
		}
		else
		{
			reader.Skip();
		}
	}

	internal static void ParseChartTitleElement(Stream titleAreaStream, IInternalOfficeChartTextArea textArea, FileDataHolder holder, RelationCollection relations, float fontSize)
	{
		titleAreaStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(titleAreaStream);
		xmlReader.Read();
		if ((xmlReader.LocalName == "title" || xmlReader.LocalName == "units") && !xmlReader.IsEmptyElement)
		{
			xmlReader.Read();
			while (xmlReader.NodeType != XmlNodeType.EndElement)
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					string localName = xmlReader.LocalName;
					if (!(localName == "txPr"))
					{
						if (localName == "unitsLabel")
						{
							xmlReader.Read();
						}
						else
						{
							xmlReader.Skip();
						}
					}
					else
					{
						((ChartTextAreaImpl)textArea).ParagraphType = ChartParagraphType.CustomDefault;
						SetWorkbook(holder.Workbook);
						ParseDefaultTextFormatting(xmlReader, textArea, holder.Parser, fontSize);
					}
				}
				else
				{
					xmlReader.Skip();
				}
			}
		}
		titleAreaStream.Position = 0L;
		XmlReader xmlReader2 = UtilityMethods.CreateReader(titleAreaStream);
		xmlReader2.Read();
		if ((xmlReader2.LocalName == "title" || xmlReader2.LocalName == "units") && !xmlReader2.IsEmptyElement)
		{
			SetWorkbook(holder.Workbook);
			ParseTextArea(xmlReader2, textArea, holder, relations, fontSize);
		}
		xmlReader2.Dispose();
		xmlReader.Dispose();
		titleAreaStream.Dispose();
	}

	private static void ParseManualLayout(XmlReader reader, IOfficeChartManualLayout manualLayout)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (manualLayout == null)
		{
			throw new ArgumentNullException("manualLayout");
		}
		if (reader.LocalName != "manualLayout")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "layoutTarget":
						manualLayout.LayoutTarget = (LayoutTargets)Enum.Parse(typeof(LayoutTargets), ParseValueTag(reader), ignoreCase: true);
						break;
					case "xMode":
						manualLayout.LeftMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), ParseValueTag(reader), ignoreCase: true);
						break;
					case "yMode":
						manualLayout.TopMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), ParseValueTag(reader), ignoreCase: true);
						break;
					case "x":
						manualLayout.Left = ParseDoubleValueTag(reader);
						break;
					case "y":
						manualLayout.Top = ParseDoubleValueTag(reader);
						break;
					case "dX":
						reader.Skip();
						break;
					case "dY":
						reader.Skip();
						break;
					case "wMode":
						manualLayout.WidthMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), ParseValueTag(reader), ignoreCase: true);
						break;
					case "hMode":
						manualLayout.HeightMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), ParseValueTag(reader), ignoreCase: true);
						break;
					case "w":
						manualLayout.Width = ParseDoubleValueTag(reader);
						break;
					case "h":
						manualLayout.Height = ParseDoubleValueTag(reader);
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
			reader.Read();
		}
		else
		{
			reader.Skip();
		}
	}

	private static void ParseStringReference(XmlReader reader, IInternalOfficeChartTextArea textArea)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "strRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ChartTextAreaImpl chartTextAreaImpl = textArea as ChartTextAreaImpl;
		ChartDataLabelsImpl chartDataLabelsImpl = textArea as ChartDataLabelsImpl;
		string text = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "f"))
				{
					if (localName == "strCache")
					{
						if (chartTextAreaImpl != null)
						{
							chartTextAreaImpl.StringCache = ParseDirectlyEnteredValues(reader);
						}
						else if (chartDataLabelsImpl != null)
						{
							chartDataLabelsImpl.StringCache = ParseDirectlyEnteredValues(reader);
							string[] stringCache = chartDataLabelsImpl.StringCache;
							foreach (string text2 in stringCache)
							{
								text = text + " " + text2;
							}
							textArea.Text = text;
						}
						if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "strCache")
						{
							reader.Read();
						}
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					if (textArea is ChartTextAreaImpl)
					{
						chartTextAreaImpl.IsFormula = true;
					}
					else if (textArea is ChartDataLabelsImpl)
					{
						chartDataLabelsImpl.IsFormula = true;
					}
					textArea.Text = reader.ReadElementContentAsString();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private static string[] ParseDirectlyEnteredValues(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		List<string> list = new List<string>();
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			return list.ToArray();
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "pt" && !reader.IsEmptyElement)
			{
				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "v")
					{
						list.Add(reader.ReadElementContentAsString());
					}
					else
					{
						reader.Skip();
					}
				}
			}
			else
			{
				reader.Skip();
			}
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "pt")
			{
				reader.Read();
			}
		}
		return list.ToArray();
	}

	private static void ParseRichText(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, float? defaultFontSize, List<ChartAlrunsRecord.TRuns> tRuns)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "rich")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		bool flag = true;
		IInternalOfficeChartTextArea internalOfficeChartTextArea = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "bodyPr":
					ParseBodyProperties(reader, textArea);
					break;
				case "lstStyle":
					ParseListStyles(reader, textArea);
					break;
				case "p":
					if (!flag)
					{
						textArea.Text += "\n";
					}
					else
					{
						flag = false;
					}
					if (tRuns != null && tRuns.Count > 0)
					{
						tRuns[tRuns.Count - 1].HasNewParagarphStart = true;
					}
					if (internalOfficeChartTextArea == null && textArea is ChartTextAreaImpl)
					{
						internalOfficeChartTextArea = (IInternalOfficeChartTextArea)(textArea as ChartTextAreaImpl).Clone(textArea.Parent);
					}
					tRuns = ParseParagraphs(reader, textArea, parser, defaultFontSize, tRuns, internalOfficeChartTextArea);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		if (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).DefaultParagarphProperties.Count > 1)
		{
			CopyDefaultTextAreaSettings(textArea, (textArea as ChartTextAreaImpl).DefaultParagarphProperties[0]);
		}
		reader.Read();
	}

	private static void ParseBodyProperties(XmlReader reader, IOfficeChartTextArea textArea)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "bodyPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("rot"))
		{
			int num = XmlConvertExtension.ToInt32(reader.Value);
			textArea.TextRotationAngle = num / 60000;
			if (reader.MoveToAttribute("vert"))
			{
				if (textArea is ChartTextAreaImpl)
				{
					(textArea as ChartTextAreaImpl).TextRotation = (Excel2007TextRotation)Enum.Parse(typeof(Excel2007TextRotation), reader.Value, ignoreCase: false);
				}
				else if (textArea is ChartDataLabelsImpl)
				{
					(textArea as ChartDataLabelsImpl).TextRotation = (Excel2007TextRotation)Enum.Parse(typeof(Excel2007TextRotation), reader.Value, ignoreCase: false);
				}
			}
			reader.MoveToElement();
		}
		reader.Skip();
	}

	private static void ParseListStyles(XmlReader reader, IOfficeChartTextArea textArea)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "lstStyle")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Skip();
	}

	private static List<ChartAlrunsRecord.TRuns> ParseParagraphs(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, float? defaultFontSize, List<ChartAlrunsRecord.TRuns> tRuns, IInternalOfficeChartTextArea defaultTextArea)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "p")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		TextSettings defaultSettings = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "pPr":
					defaultSettings = ParseParagraphProperties(reader, parser, defaultFontSize);
					break;
				case "r":
					CopyDefaultTextAreaSettings(textArea, defaultTextArea);
					tRuns = ParseParagraphRun(reader, textArea, parser, defaultSettings, tRuns);
					break;
				case "fld":
					tRuns = ParseFldElement(reader, textArea, parser, defaultSettings, tRuns, defaultFontSize);
					break;
				case "br":
					textArea.Text += "\n";
					reader.Skip();
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		if (defaultTextArea != null)
		{
			IInternalOfficeChartTextArea internalOfficeChartTextArea = (IInternalOfficeChartTextArea)(defaultTextArea as ChartTextAreaImpl).Clone(defaultTextArea.Parent);
			CopyDefaultSettings(internalOfficeChartTextArea, defaultSettings, parser.Workbook);
			CopyDefaultTextAreaSettings(internalOfficeChartTextArea, textArea);
			(internalOfficeChartTextArea as ChartTextAreaImpl).ChartAlRuns.Runs = (textArea as ChartTextAreaImpl).ChartAlRuns.Runs;
			(textArea as ChartTextAreaImpl).DefaultParagarphProperties.Add(internalOfficeChartTextArea);
		}
		return tRuns;
	}

	private static List<ChartAlrunsRecord.TRuns> ParseFldElement(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, TextSettings defaultSettings, List<ChartAlrunsRecord.TRuns> tRuns, float? defaultFontSize)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "fld")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "rPr":
				ParseParagraphRunProperites(reader, textArea, parser, defaultSettings);
				break;
			case "pPr":
				defaultSettings = ParseParagraphProperties(reader, parser, defaultFontSize);
				break;
			case "t":
			{
				ushort firstChar = 0;
				string text = reader.ReadElementContentAsString();
				if (textArea.Text == "Chart Title" && text != null)
				{
					textArea.Text = text;
				}
				else
				{
					if (textArea.Text != null)
					{
						firstChar = (ushort)textArea.Text.Length;
					}
					textArea.Text += text;
				}
				if (tRuns != null)
				{
					tRuns.Add(new ChartAlrunsRecord.TRuns(firstChar, (ushort)textArea.Font.Index));
					if (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).ChartAlRuns != null)
					{
						(textArea as ChartTextAreaImpl).ChartAlRuns.Runs = tRuns.ToArray();
					}
					else if (textArea is ChartDataLabelsImpl && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns != null)
					{
						(textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns.Runs = tRuns.ToArray();
					}
				}
				if (textArea.Text.Contains("\r"))
				{
					textArea.Text = textArea.Text.Remove(textArea.Text.IndexOf('\r'), 1);
				}
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		reader.Read();
		return tRuns;
	}

	private static TextSettings ParseParagraphProperties(XmlReader reader, Excel2007Parser parser, float? defaultFontSize)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		TextSettings result = null;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "defRPr")
					{
						result = ParseDefaultParagraphProperties(reader, parser, defaultFontSize, parser.Workbook);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		return result;
	}

	internal static TextSettings ParseDefaultParagraphProperties(XmlReader reader, Excel2007Parser parser, WorkbookImpl book)
	{
		return ParseDefaultParagraphProperties(reader, parser, null, book);
	}

	internal static TextSettings ParseDefaultParagraphProperties(XmlReader reader, Excel2007Parser parser, float? defaultFontSize, WorkbookImpl book)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		TextSettings textSettings = new TextSettings();
		textSettings.FontSize = defaultFontSize;
		if (reader.MoveToAttribute("b"))
		{
			textSettings.Bold = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("i"))
		{
			textSettings.Italic = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("sz"))
		{
			textSettings.FontSize = XmlConvertExtension.ToSingle(reader.Value) / 100f;
			textSettings.ShowSizeProperties = true;
		}
		if (reader.MoveToAttribute("strike"))
		{
			textSettings.Striked = reader.Value != "noStrike";
		}
		if (reader.MoveToAttribute("lang"))
		{
			textSettings.Language = reader.Value;
		}
		if (reader.MoveToAttribute("baseline"))
		{
			int baseline = XmlConvertExtension.ToInt32(reader.Value);
			textSettings.Baseline = baseline;
		}
		if (reader.MoveToAttribute("cap") && reader.Value == "all")
		{
			textSettings.HasCapitalization = true;
		}
		if (reader.MoveToAttribute("spc"))
		{
			textSettings.SpacingValue = float.Parse(reader.Value) / 100f;
		}
		if (reader.MoveToAttribute("kern"))
		{
			textSettings.KerningValue = float.Parse(reader.Value) / 100f;
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "latin":
						textSettings.HasLatin = true;
						if (reader.MoveToAttribute("typeface"))
						{
							textSettings.ActualFontName = reader.Value;
							string fontName = CheckValue(reader.Value.ToString(), book);
							textSettings.FontName = fontName;
						}
						reader.MoveToElement();
						reader.Skip();
						break;
					case "solidFill":
						ParseDefaultFontColor(reader, textSettings, parser);
						break;
					case "gradFill":
					case "effectLst":
					case "ln":
						textSettings.PreservedElements.Add(reader.LocalName, UtilityMethods.ReadSingleNodeIntoStream(reader));
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		return textSettings;
	}

	public static string CheckValue(string strValue, WorkbookImpl book)
	{
		FontImpl value = null;
		switch (strValue)
		{
		case "+mj-lt":
		case "+mj-cs":
		case "+mj-ea":
		{
			string[] array = strValue.Split('-');
			if (array[0] == "+mj")
			{
				switch (array[1])
				{
				case "lt":
					book.MajorFonts.TryGetValue("latin", out value);
					break;
				case "ea":
					book.MajorFonts.TryGetValue("ea", out value);
					break;
				case "cs":
					book.MajorFonts.TryGetValue("cs", out value);
					break;
				}
			}
			return value.FontName;
		}
		default:
			return strValue;
		}
	}

	private static void ParseDefaultFontColor(XmlReader reader, TextSettings result, Excel2007Parser parser)
	{
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "srgbClr"))
					{
						if (localName == "schemeClr")
						{
							result.FontColor = ParseSchemeColor(reader, parser);
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						result.FontColor = ParseSRgbColor(reader, parser);
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private static List<ChartAlrunsRecord.TRuns> ParseParagraphRun(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, TextSettings defaultSettings, List<ChartAlrunsRecord.TRuns> tRuns)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "r")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		CopyDefaultSettings(textArea, defaultSettings, parser.Workbook);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "rPr"))
				{
					if (localName == "t")
					{
						ushort firstChar = 0;
						string text = reader.ReadElementContentAsString();
						if (textArea.Text == "Chart Title" && text != null)
						{
							textArea.Text = text;
						}
						else
						{
							if (textArea.Text != null)
							{
								firstChar = (ushort)textArea.Text.Length;
							}
							textArea.Text += text;
						}
						if (tRuns != null)
						{
							tRuns.Add(new ChartAlrunsRecord.TRuns(firstChar, (ushort)textArea.Font.Index));
							if (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).ChartAlRuns != null)
							{
								(textArea as ChartTextAreaImpl).ChartAlRuns.Runs = tRuns.ToArray();
							}
							else if (textArea is ChartDataLabelsImpl && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns != null)
							{
								(textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns.Runs = tRuns.ToArray();
							}
						}
						if (textArea.Text.Contains("\r"))
						{
							textArea.Text = textArea.Text.Remove(textArea.Text.IndexOf('\r'), 1);
						}
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ParseParagraphRunProperites(reader, textArea, parser, defaultSettings);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		return tRuns;
	}

	public static void ParseParagraphRunProperites(XmlReader reader, IInternalOfficeChartTextArea textArea, Excel2007Parser parser, TextSettings defaultSettings)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		bool flag = false;
		CopyDefaultSettings(textArea, defaultSettings, parser.Workbook);
		if (reader.MoveToAttribute("lang"))
		{
			textArea.Font.Language = reader.Value;
			FontImpl value = null;
			if (parser.Workbook != null && parser.Workbook.MinorFonts != null && parser.Workbook.MinorFonts.TryGetValue("latin", out value))
			{
				textArea.FontName = value.FontName;
			}
		}
		if (defaultSettings != null && defaultSettings.FontName != textArea.FontName && defaultSettings.FontName != null)
		{
			textArea.FontName = defaultSettings.FontName;
		}
		if (reader.MoveToAttribute("b"))
		{
			textArea.Bold = XmlConvertExtension.ToBoolean(reader.Value);
			if (textArea is ChartDataLabelsImpl)
			{
				(textArea as ChartDataLabelsImpl).ShowBoldProperties = true;
			}
			else if (textArea is ChartTextAreaImpl)
			{
				(textArea as ChartTextAreaImpl).ShowBoldProperties = true;
			}
		}
		if (reader.MoveToAttribute("i"))
		{
			textArea.Italic = XmlConvertExtension.ToBoolean(reader.Value);
			if (textArea is ChartDataLabelsImpl)
			{
				(textArea as ChartDataLabelsImpl).ShowItalicProperties = true;
			}
			else if (textArea is ChartTextAreaImpl)
			{
				(textArea as ChartTextAreaImpl).ShowItalicProperties = true;
			}
		}
		if (reader.MoveToAttribute("strike"))
		{
			textArea.Strikethrough = reader.Value != "noStrike";
		}
		if (reader.MoveToAttribute("sz"))
		{
			textArea.Size = (double)int.Parse(reader.Value) / 100.0;
			if (textArea is ChartDataLabelsImpl)
			{
				(textArea as ChartDataLabelsImpl).ShowSizeProperties = true;
			}
			else if (textArea is ChartTextAreaImpl)
			{
				(textArea as ChartTextAreaImpl).ShowSizeProperties = true;
			}
		}
		if (reader.MoveToAttribute("u"))
		{
			textArea.Underline = (OfficeUnderline)Enum.Parse(typeof(OfficeUnderline), DocGen.Drawing.Helper.GetOfficeUnderlineType(reader.Value).ToString(), ignoreCase: true);
		}
		if (reader.MoveToAttribute("baseline"))
		{
			string value2 = reader.Value;
			int num = 0;
			if (value2.Contains("%"))
			{
				num = XmlConvertExtension.ToInt32(value2) / 100;
				(textArea as ChartTextAreaImpl).IsBaselineWithPercentage = true;
			}
			else
			{
				num = int.Parse(value2);
			}
			textArea.Font.BaseLine = num;
			if (num > 0)
			{
				textArea.Superscript = true;
			}
			else if (num < 0)
			{
				textArea.Subscript = true;
			}
			else
			{
				textArea.Superscript = false;
				textArea.Subscript = false;
			}
		}
		if (reader.MoveToAttribute("cap") && reader.Value == "all" && textArea is ChartTextAreaImpl)
		{
			(textArea as ChartTextAreaImpl).IsCapitalize = true;
			(textArea as ChartTextAreaImpl).HasCapOrCharacterSpaceOrKerning = true;
		}
		if (reader.MoveToAttribute("spc") && textArea is ChartTextAreaImpl)
		{
			(textArea as ChartTextAreaImpl).CharacterSpacingValue = float.Parse(reader.Value) / 100f;
			(textArea as ChartTextAreaImpl).HasCapOrCharacterSpaceOrKerning = true;
		}
		if (reader.MoveToAttribute("kern") && textArea is ChartTextAreaImpl)
		{
			(textArea as ChartTextAreaImpl).KerningValue = float.Parse(reader.Value) / 100f;
			(textArea as ChartTextAreaImpl).HasCapOrCharacterSpaceOrKerning = true;
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "latin":
						if (reader.MoveToAttribute("typeface"))
						{
							textArea.Font.ActualFontName = reader.Value;
							textArea.FontName = CheckValue(reader.Value, parser.Workbook);
							textArea.Font.ColorObject.CopyFrom(textArea.ColorObject, callEvent: true);
							textArea.Font.HasLatin = true;
							reader.MoveToElement();
						}
						reader.Skip();
						break;
					case "solidFill":
						ParseSolidFill(reader, parser, textArea.ColorObject);
						flag = true;
						break;
					case "highlight":
					{
						CommonObject commonObject = textArea as CommonObject;
						ChartImpl chart = null;
						if (commonObject != null)
						{
							chart = commonObject.FindParent(typeof(ChartImpl)) as ChartImpl;
						}
						if (textArea is ChartDataLabelsImpl)
						{
							(textArea as ChartDataLabelsImpl).IsHighlightColor = true;
						}
						ParseHighlight(reader, chart);
						break;
					}
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag && defaultSettings != null && defaultSettings.FontColor.HasValue && defaultSettings.FontColor != textArea.RGBColor)
		{
			textArea.RGBColor = Color.Empty;
			textArea.RGBColor = defaultSettings.FontColor.Value;
		}
		reader.Skip();
	}

	private static void ParseHighlight(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		chart.HighlightStream = ShapeParser.ReadNodeAsStream(reader);
	}

	private static void CopyDefaultTextAreaSettings(IInternalOfficeChartTextArea textArea, IInternalOfficeChartTextArea defaultTextArea)
	{
		if (defaultTextArea != null)
		{
			if (textArea.Bold != defaultTextArea.Bold)
			{
				textArea.Bold = defaultTextArea.Bold;
			}
			if (textArea.Color != defaultTextArea.Color)
			{
				textArea.Color = defaultTextArea.Color;
			}
			if (textArea.RGBColor != defaultTextArea.RGBColor)
			{
				textArea.RGBColor = defaultTextArea.RGBColor;
			}
			if (textArea.FontName != defaultTextArea.FontName)
			{
				textArea.FontName = defaultTextArea.FontName;
			}
			if (textArea.Size != defaultTextArea.Size)
			{
				textArea.Size = defaultTextArea.Size;
			}
			if (textArea.Strikethrough != textArea.Strikethrough)
			{
				textArea.Strikethrough = defaultTextArea.Strikethrough;
			}
			if (textArea.Subscript != defaultTextArea.Subscript)
			{
				textArea.Subscript = defaultTextArea.Subscript;
			}
			if (textArea.Superscript != defaultTextArea.Superscript)
			{
				textArea.Superscript = defaultTextArea.Superscript;
			}
			if (textArea.TextRotationAngle != defaultTextArea.TextRotationAngle)
			{
				textArea.TextRotationAngle = defaultTextArea.TextRotationAngle;
			}
			if (textArea.Underline != defaultTextArea.Underline)
			{
				textArea.Underline = defaultTextArea.Underline;
			}
			if (textArea.VerticalAlignment != defaultTextArea.VerticalAlignment)
			{
				textArea.VerticalAlignment = defaultTextArea.VerticalAlignment;
			}
			if (textArea.Italic != defaultTextArea.Italic)
			{
				textArea.Italic = defaultTextArea.Italic;
			}
			if (textArea.Font != defaultTextArea.Font)
			{
				textArea.Font.Clone(defaultTextArea.Font.Parent);
			}
		}
	}

	public static void CopyDefaultSettings(IInternalFont textArea, TextSettings defaultSettings, WorkbookImpl book)
	{
		if (defaultSettings == null)
		{
			return;
		}
		if ((defaultSettings.HasCapitalization || defaultSettings.SpacingValue > 0f || defaultSettings.KerningValue > 0f) && textArea is FontWrapper)
		{
			(textArea as FontWrapper).HasCapOrCharacterSpaceOrKerning = true;
		}
		if (defaultSettings.Bold.HasValue)
		{
			textArea.Bold = defaultSettings.Bold.Value;
			if (textArea is ChartTextAreaImpl)
			{
				(textArea as ChartTextAreaImpl).ShowBoldProperties = true;
			}
		}
		if (defaultSettings.Italic.HasValue)
		{
			textArea.Italic = defaultSettings.Italic.Value;
		}
		if (defaultSettings.FontSize.HasValue)
		{
			textArea.Size = defaultSettings.FontSize.Value;
		}
		if (defaultSettings.FontName != null)
		{
			textArea.FontName = defaultSettings.FontName;
		}
		if (defaultSettings.Striked.HasValue)
		{
			textArea.Strikethrough = defaultSettings.Striked.Value;
		}
		_ = defaultSettings.Baseline;
		if (textArea is FontWrapper)
		{
			(textArea as FontWrapper).Baseline = defaultSettings.Baseline;
		}
		if (defaultSettings.Baseline > 0)
		{
			textArea.Superscript = true;
		}
		else if (defaultSettings.Baseline < 0)
		{
			textArea.Subscript = true;
		}
		else
		{
			textArea.Subscript = false;
			textArea.Superscript = false;
		}
		if (defaultSettings.Language != null)
		{
			textArea.Font.Language = defaultSettings.Language;
			FontImpl value = null;
			if (book.MinorFonts.TryGetValue("latin", out value))
			{
				textArea.FontName = value.FontName;
			}
		}
		if (defaultSettings.FontColor.HasValue)
		{
			textArea.RGBColor = defaultSettings.FontColor.Value;
		}
		if (defaultSettings.PreservedElements != null)
		{
			foreach (KeyValuePair<string, Stream> preservedElement in defaultSettings.PreservedElements)
			{
				if (!textArea.Font.PreservedElements.ContainsKey(preservedElement.Key))
				{
					textArea.Font.PreservedElements.Add(preservedElement.Key, preservedElement.Value);
				}
			}
		}
		if (defaultSettings.HasLatin.HasValue)
		{
			textArea.Font.HasLatin = defaultSettings.HasLatin.Value;
		}
		if (defaultSettings.HasComplexScripts.HasValue)
		{
			textArea.Font.HasComplexScripts = defaultSettings.HasComplexScripts.Value;
		}
		if (defaultSettings.HasEastAsianFont.HasValue)
		{
			textArea.Font.HasEastAsianFont = defaultSettings.HasEastAsianFont.Value;
		}
		if (defaultSettings.ActualFontName != null)
		{
			textArea.Font.ActualFontName = defaultSettings.ActualFontName.ToString();
		}
		if (defaultSettings.ShowSizeProperties.HasValue)
		{
			if (textArea is ChartTextAreaImpl && defaultSettings.ShowSizeProperties.HasValue)
			{
				(textArea as ChartTextAreaImpl).ShowSizeProperties = defaultSettings.ShowSizeProperties.Value;
			}
			else if (textArea is ChartDataLabelsImpl && defaultSettings.ShowSizeProperties.HasValue)
			{
				(textArea as ChartDataLabelsImpl).ShowSizeProperties = defaultSettings.ShowSizeProperties.Value;
			}
		}
		if (defaultSettings.HasCapitalization && textArea is FontWrapper)
		{
			(textArea as FontWrapper).IsCapitalize = true;
		}
		if (defaultSettings.SpacingValue > 0f && textArea is FontWrapper)
		{
			(textArea as FontWrapper).CharacterSpacingValue = defaultSettings.SpacingValue;
		}
		if (defaultSettings.KerningValue > 0f && textArea is FontWrapper)
		{
			(textArea as FontWrapper).KerningValue = defaultSettings.KerningValue;
		}
		textArea.Font.m_textSettings = defaultSettings;
	}

	public static GradientStops ParseGradientFill(XmlReader reader, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "gradFill")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		GradientStops gradientStops = null;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "gsLst":
					gradientStops = ParseGradientStops(reader, parser);
					break;
				case "lin":
					gradientStops.GradientType = GradientType.Liniar;
					reader.MoveToAttribute("ang");
					gradientStops.Angle = int.Parse(reader.Value);
					reader.Read();
					break;
				case "path":
					ParseGradientPath(reader, gradientStops);
					break;
				case "tileRect":
				{
					int left = (reader.MoveToAttribute("l") ? int.Parse(reader.Value) : 0);
					int top = (reader.MoveToAttribute("t") ? int.Parse(reader.Value) : 0);
					int right = (reader.MoveToAttribute("r") ? int.Parse(reader.Value) : 0);
					int bottom = (reader.MoveToAttribute("b") ? int.Parse(reader.Value) : 0);
					gradientStops.TileRect = Rectangle.FromLTRB(left, top, right, bottom);
					reader.Read();
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
		return gradientStops;
	}

	private static void ParseGradientPath(XmlReader reader, GradientStops result)
	{
		bool isEmptyElement = reader.IsEmptyElement;
		reader.MoveToAttribute("path");
		result.GradientType = (GradientType)Enum.Parse(typeof(GradientType), reader.Value, ignoreCase: true);
		if (!isEmptyElement)
		{
			reader.Read();
			if (reader.LocalName == "fillToRect")
			{
				int left = (reader.MoveToAttribute("l") ? int.Parse(reader.Value) : 0);
				int top = (reader.MoveToAttribute("t") ? int.Parse(reader.Value) : 0);
				int right = (reader.MoveToAttribute("r") ? int.Parse(reader.Value) : 0);
				int bottom = (reader.MoveToAttribute("b") ? int.Parse(reader.Value) : 0);
				result.FillToRect = Rectangle.FromLTRB(left, top, right, bottom);
				reader.Read();
				reader.Read();
			}
		}
	}

	private static GradientStops ParseGradientStops(XmlReader reader, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "gsLst")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		GradientStops gradientStops = new GradientStops();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "gs")
				{
					GradientStopImpl item = ParseGradientStop(reader, parser);
					gradientStops.Add(item);
				}
				else
				{
					reader.Read();
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
		return gradientStops;
	}

	private static GradientStopImpl ParseGradientStop(XmlReader reader, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "gs")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		int position = -1;
		if (reader.MoveToAttribute("pos"))
		{
			position = XmlConvertExtension.ToInt32(reader.Value);
		}
		reader.Read();
		int transparecy = 100000;
		GradientStopImpl gradientStopImpl;
		if (reader.LocalName == "schemeClr" && reader.NodeType != XmlNodeType.EndElement)
		{
			string text = null;
			if (reader.MoveToAttribute("val"))
			{
				text = reader.Value;
			}
			Color themeColor = parser.GetThemeColor(text);
			reader.MoveToElement();
			gradientStopImpl = new GradientStopImpl(themeColor, position, transparecy);
			ChartColor colorObject = gradientStopImpl.ColorObject;
			colorObject.IsSchemeColor = true;
			colorObject.SchemaName = text;
			ParseSchemeColor(reader, parser, gradientStopImpl);
			reader.Read();
			reader.Read();
		}
		else
		{
			int tint;
			int shade;
			Color color = ReadColor(reader, out transparecy, out tint, out shade, parser);
			reader.Read();
			gradientStopImpl = new GradientStopImpl(color, position, transparecy, tint, shade);
		}
		return gradientStopImpl;
	}

	private static void ParseSchemeColor(XmlReader reader, Excel2007Parser parser, GradientStopImpl stop)
	{
		if (reader.LocalName != "schemeClr")
		{
			throw new ArgumentException("Invaild Tag");
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			ChartColor colorObject = stop.ColorObject;
			switch (reader.LocalName)
			{
			case "lumMod":
				colorObject.Luminance = ParseIntValueTag(reader);
				break;
			case "lumOff":
				colorObject.LuminanceOffSet = ParseIntValueTag(reader);
				break;
			case "satMod":
				colorObject.Saturation = ParseIntValueTag(reader);
				break;
			case "tint":
				colorObject.Tint = ParseIntValueTag(reader);
				break;
			case "shade":
				stop.Shade = ParseIntValueTag(reader);
				break;
			case "alpha":
				stop.Transparency = ParseIntValueTag(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
	}

	private static void ParseSchemeColor(XmlReader reader, Excel2007Parser parser, ShadowImpl shadow)
	{
		if (reader.LocalName != "schemeClr")
		{
			throw new ArgumentException("Invaild Tag");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				ChartColor colorObject = shadow.ColorObject;
				switch (reader.LocalName)
				{
				case "lumMod":
					colorObject.Luminance = ParseIntValueTag(reader);
					break;
				case "lumOff":
					colorObject.LuminanceOffSet = ParseIntValueTag(reader);
					break;
				case "satMod":
					colorObject.Saturation = ParseIntValueTag(reader);
					break;
				case "tint":
					colorObject.Tint = ParseIntValueTag(reader);
					break;
				case "alpha":
					shadow.Transparency = 100 - ParseIntValueTag(reader) / 1000;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		else
		{
			reader.Skip();
		}
	}

	internal static Color ReadColor(XmlReader reader, out int transparecy, out int tint, out int shade, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Color result = ColorExtension.Empty;
		transparecy = -1;
		tint = -1;
		shade = -1;
		switch (reader.LocalName)
		{
		case "srgbClr":
			result = ParseSRgbColor(reader, out transparecy, out tint, out shade, parser);
			break;
		case "schemeClr":
			result = ParseSchemeColor(reader, out transparecy, parser);
			break;
		case "sysClr":
			result = ParseSystemColor(reader, out transparecy, parser);
			break;
		default:
			reader.Skip();
			break;
		}
		return result;
	}

	private static void ConvertGradientStopsToProperties(GradientStops gradientStops, IInternalFill fill)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		OfficeGradientPreset officeGradientPreset = OfficeGradientPreset.Grad_Brass;
		bool isInverted = false;
		OfficeGradientColor officeGradientColor = DetectGradientColor(gradientStops);
		fill.IsGradientSupported = true;
		if (officeGradientColor < OfficeGradientColor.OneColor)
		{
			officeGradientPreset = FindPreset(gradientStops, out isInverted);
			if (officeGradientPreset >= (OfficeGradientPreset)0)
			{
				officeGradientColor = OfficeGradientColor.Preset;
				fill.PresetGradient(officeGradientPreset);
			}
		}
		if (officeGradientColor < OfficeGradientColor.OneColor)
		{
			officeGradientColor = OfficeGradientColor.TwoColor;
			fill.IsGradientSupported = false;
		}
		if (officeGradientColor != OfficeGradientColor.Preset)
		{
			CopyGradientColor(fill.ForeColorObject, gradientStops[0]);
			CopyGradientColor(fill.BackColorObject, gradientStops[gradientStops.Count - 1]);
			fill.FillType = OfficeFillType.Gradient;
			fill.GradientColorType = officeGradientColor;
		}
		fill.FillType = OfficeFillType.Gradient;
		OfficeGradientStyle gradientStyle = (fill.GradientStyle = DetectGradientStyle(gradientStops));
		fill.GradientVariant = DetectGradientVariant(gradientStops, gradientStyle, officeGradientColor, isInverted);
		SetGradientDegree(gradientStops, officeGradientColor, fill);
	}

	internal static void CheckDefaultSettings(ChartTextAreaImpl textArea)
	{
		bool num = textArea.Font.Language != null;
		bool flag = textArea.FontName != "Calibri";
		bool flag2 = textArea.Size != 10.0;
		bool bold = textArea.Bold;
		bool italic = textArea.Italic;
		bool flag3 = textArea.Underline != OfficeUnderline.None;
		bool superscript = textArea.Superscript;
		bool subscript = textArea.Subscript;
		bool strikethrough = textArea.Strikethrough;
		bool hasLatin = textArea.Font.HasLatin;
		bool flag4 = !textArea.IsAutoColor;
		if (num || flag || flag2 || bold || italic || flag3 || superscript || subscript || strikethrough || hasLatin || flag4)
		{
			((IInternalOfficeChartTextArea)textArea).ParagraphType = ChartParagraphType.CustomDefault;
		}
	}

	private static void CopyGradientColor(ChartColor colorObject, GradientStopImpl gradientStop)
	{
		ChartColor chartColor = gradientStop.ColorObject;
		int tint = gradientStop.Tint;
		if (tint >= 0)
		{
			double dTint = (double)tint / 100000.0;
			chartColor = Excel2007Parser.ConvertColorByTint(chartColor.GetRGB(null), dTint);
		}
		colorObject.CopyFrom(chartColor, callEvent: true);
	}

	public static void ParseShapeProperties(XmlReader reader, IChartFillObjectGetter objectGetter, FileDataHolder dataHolder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "spPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (objectGetter == null)
		{
			throw new ArgumentNullException("objectGetter");
		}
		bool flag = false;
		if (objectGetter.Border != null && objectGetter.Fill == null && objectGetter.Interior == null && objectGetter.Shadow == null && objectGetter.ThreeD == null)
		{
			flag = true;
		}
		IOfficeChartInterior interior = objectGetter.Interior;
		if (!flag)
		{
			interior = objectGetter.Interior;
		}
		int Alpha = 100000;
		if (interior != null)
		{
			interior.Pattern = OfficePattern.None;
			interior.UseAutomaticFormat = true;
		}
		Excel2007Parser parser = dataHolder.Parser;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			objectGetter.Border.AutoFormat = true;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "ln":
						ParseLineProperties(reader, objectGetter.Border, parser);
						break;
					case "solidFill":
						if (!flag && objectGetter.Fill != null)
						{
							ParseSolidFill(reader, objectGetter.Interior, parser, out Alpha);
							if (objectGetter.Fill.FillType != 0)
							{
								objectGetter.Fill.FillType = OfficeFillType.SolidColor;
							}
							objectGetter.Fill.Transparency = 1f - (float)Alpha / 100000f;
						}
						else
						{
							reader.Skip();
						}
						break;
					case "pattFill":
						if (!flag && objectGetter.Fill != null)
						{
							if (objectGetter.Fill.FillType != OfficeFillType.Pattern)
							{
								objectGetter.Fill.FillType = OfficeFillType.Pattern;
							}
							ParsePatternFill(reader, objectGetter.Fill, parser);
						}
						else
						{
							reader.Skip();
						}
						break;
					case "gradFill":
						if (!flag && objectGetter.Fill != null)
						{
							GradientStops gradientStops = ParseGradientFill(reader, parser);
							ConvertGradientStopsToProperties(gradientStops, objectGetter.Fill);
							objectGetter.Fill.PreservedGradient = gradientStops;
							objectGetter.Interior.UseAutomaticFormat = false;
						}
						else
						{
							reader.Skip();
						}
						break;
					case "blipFill":
						if (objectGetter.Fill != null)
						{
							ParsePictureFill(reader, objectGetter.Fill, relations, dataHolder);
						}
						else
						{
							reader.Skip();
						}
						break;
					case "noFill":
						if (objectGetter.Fill != null)
						{
							objectGetter.Fill.FillType = OfficeFillType.Pattern;
							objectGetter.Fill.Pattern = (OfficeGradientPattern)0;
							objectGetter.Interior.Pattern = OfficePattern.None;
						}
						reader.Skip();
						break;
					case "effectLst":
						if (flag)
						{
							reader.Skip();
						}
						else
						{
							ParseShadowproperties(reader, objectGetter.Shadow, relations, dataHolder, parser);
						}
						if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "effectLst")
						{
							reader.Read();
						}
						break;
					case "scene3d":
						ParseLighting(reader, objectGetter.ThreeD, relations, dataHolder);
						if (reader.LocalName != "sp3d")
						{
							reader.Read();
						}
						break;
					case "sp3d":
					{
						string text = "null";
						if (reader.MoveToAttribute("prstMaterial"))
						{
							text = reader.Value;
							objectGetter.ThreeD.Material = Check(text, reader);
						}
						else
						{
							objectGetter.ThreeD.Material = Office2007ChartMaterialProperties.NoEffect;
						}
						reader.MoveToElement();
						if (!reader.IsEmptyElement)
						{
							reader.Read();
							while (reader.NodeType != XmlNodeType.EndElement)
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									string text2 = "null";
									string text3 = "null";
									string presetShape = "null";
									string localName = reader.LocalName;
									if (!(localName == "bevelT"))
									{
										if (localName == "bevelB")
										{
											if (reader.MoveToAttribute("w"))
											{
												text2 = reader.Value;
												objectGetter.ThreeD.BevelBottomWidth = Convert.ToInt32(text2) / 12700;
											}
											if (reader.MoveToAttribute("h"))
											{
												text3 = reader.Value;
												objectGetter.ThreeD.BevelBottomHeight = Convert.ToInt32(text3) / 12700;
											}
											if (reader.MoveToAttribute("prst"))
											{
												presetShape = reader.Value;
												objectGetter.ThreeD.PresetShape = presetShape;
											}
											objectGetter.ThreeD.BevelBottom = Check(text2, text3, presetShape, reader);
										}
										else
										{
											reader.Skip();
										}
									}
									else
									{
										if (reader.MoveToAttribute("w"))
										{
											text2 = reader.Value;
											objectGetter.ThreeD.BevelTopWidth = Convert.ToInt32(text2) / 12700;
										}
										if (reader.MoveToAttribute("h"))
										{
											text3 = reader.Value;
											objectGetter.ThreeD.BevelTopHeight = Convert.ToInt32(text3) / 12700;
										}
										if (reader.MoveToAttribute("prst"))
										{
											presetShape = reader.Value;
											objectGetter.ThreeD.PresetShape = presetShape;
										}
										objectGetter.ThreeD.BevelTop = Check(text2, text3, presetShape, reader);
									}
								}
								else
								{
									reader.Skip();
								}
							}
						}
						reader.Read();
						break;
					}
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
		if (reader.LocalName == "spPr")
		{
			reader.Read();
		}
	}

	private static void ParseLighting(XmlReader reader, ThreeDFormatImpl Three_D, RelationCollection relations, FileDataHolder holder)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "scene3d")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "lightRig"))
					{
						if (localName == "rot")
						{
							if (reader.MoveToAttribute("lat"))
							{
								Three_D.LightningLatitude = XmlConvertExtension.ToInt32(reader.Value);
							}
							if (reader.MoveToAttribute("lon"))
							{
								Three_D.LightningLongitude = XmlConvertExtension.ToInt32(reader.Value);
							}
							if (reader.MoveToAttribute("rev"))
							{
								Three_D.LightningAngle = XmlConvertExtension.ToInt32(reader.Value) / 60000;
							}
							reader.Read();
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						reader.MoveToAttribute("rig");
						string value = reader.Value;
						Three_D.Lighting = Check(value);
						reader.Read();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName != "scene3d")
		{
			reader.Read();
		}
	}

	public static Office2007ChartLightingProperties Check(string lighttype)
	{
		Office2007ChartLightingProperties result = Office2007ChartLightingProperties.ThreePoint;
		for (int i = 0; i < ChartSerializatorCommon.LightingProperties.GetLength(0); i++)
		{
			if (lighttype.Equals(ChartSerializatorCommon.LightingProperties[i][0]))
			{
				result = (Office2007ChartLightingProperties)i;
				break;
			}
		}
		return result;
	}

	public static Office2007ChartBevelProperties Check(string LineWidth, string LineHeight, string PresetShape, XmlReader reader)
	{
		Office2007ChartBevelProperties result = Office2007ChartBevelProperties.NoAngle;
		if (LineWidth.Equals("null") && LineHeight.Equals("null") && PresetShape.Equals("null"))
		{
			result = Office2007ChartBevelProperties.Circle;
			reader.Skip();
		}
		else
		{
			for (int i = 0; i < ChartSerializatorCommon.BevelProperties.GetLength(0); i++)
			{
				if (PresetShape.Equals(ChartSerializatorCommon.BevelProperties[i][2]))
				{
					result = (Office2007ChartBevelProperties)i;
					break;
				}
			}
		}
		return result;
	}

	public static Office2007ChartMaterialProperties Check(string material, XmlReader reader)
	{
		Office2007ChartMaterialProperties result = Office2007ChartMaterialProperties.NoEffect;
		for (int i = 0; i < ChartSerializatorCommon.MaterialProperties.GetLength(0); i++)
		{
			if (material.Equals(ChartSerializatorCommon.MaterialProperties[i][0]))
			{
				result = (Office2007ChartMaterialProperties)(i + 1);
				break;
			}
		}
		return result;
	}

	internal static void ParseShadowColor(ShadowImpl Shadow, XmlReader reader, Excel2007Parser parser)
	{
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "prstClr":
					if (reader.MoveToAttribute("val"))
					{
						string value2 = reader.Value;
						Shadow.ShadowColor = parser.GetThemeColor(value2);
					}
					reader.MoveToElement();
					if (!reader.IsEmptyElement)
					{
						ParseShadowAlpha(reader, Shadow);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "schemeClr":
					if (reader.MoveToAttribute("val"))
					{
						string value3 = reader.Value;
						Shadow.ShadowColor = parser.GetThemeColor(value3);
					}
					reader.MoveToElement();
					ParseSchemeColor(reader, parser, Shadow);
					break;
				case "srgbClr":
					if (reader.MoveToAttribute("val"))
					{
						int value = int.Parse(reader.Value, NumberStyles.HexNumber, null);
						Shadow.ShadowColor = ColorExtension.FromArgb(value);
					}
					reader.MoveToElement();
					if (!reader.IsEmptyElement)
					{
						reader.Read();
						while (reader.NodeType != XmlNodeType.EndElement)
						{
							if (reader.NodeType == XmlNodeType.Element)
							{
								if (reader.LocalName == "alpha")
								{
									if (reader.MoveToAttribute("val"))
									{
										Shadow.Transparency = 100 - Convert.ToInt32(reader.Value) / 1000;
									}
									else
									{
										Shadow.Transparency = 100000;
									}
								}
								else
								{
									reader.Skip();
								}
							}
							else
							{
								reader.Skip();
							}
						}
					}
					reader.Read();
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private static void ParseShadowproperties(XmlReader reader, ShadowImpl shadow, RelationCollection relations, FileDataHolder holder, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "effectLst")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string text = "null";
					string text2 = "null";
					string sizey = "null";
					string disttag = "null";
					string dirtag = "null";
					string align = "null";
					string rot = "null";
					string text3 = "null";
					switch (reader.LocalName)
					{
					case "outerShdw":
						if (reader.MoveToAttribute("blurRad"))
						{
							text = reader.Value;
						}
						if (reader.MoveToAttribute("sx"))
						{
							text2 = reader.Value;
						}
						if (reader.MoveToAttribute("sy"))
						{
							sizey = reader.Value;
						}
						if (reader.MoveToAttribute("kx"))
						{
							text3 = reader.Value;
						}
						if (reader.MoveToAttribute("dist"))
						{
							disttag = reader.Value;
						}
						if (reader.MoveToAttribute("dir"))
						{
							dirtag = reader.Value;
						}
						if (reader.MoveToAttribute("algn"))
						{
							align = reader.Value;
						}
						if (reader.MoveToAttribute("rotWithShape"))
						{
							rot = reader.Value;
						}
						if ((text3 == "null" && text.Equals("50800")) || (text3 == "null" && text.Equals("63500") && !text2.Equals("null")))
						{
							shadow.ShadowOuterPresets = Check(text, text2, sizey, disttag, dirtag, align, rot, shadow, reader, parser);
						}
						else if (!text3.Equals("null") || (text2.Equals("90000") && text3.Equals("null")))
						{
							shadow.ShadowPerspectivePresets = Check(text, text2, sizey, text3, disttag, dirtag, align, rot, shadow, reader, parser);
						}
						else
						{
							shadow.ShadowOuterPresets = Check(text, text2, sizey, disttag, dirtag, align, rot, shadow, reader, parser);
						}
						break;
					case "innerShdw":
						if (reader.MoveToAttribute("blurRad"))
						{
							text = reader.Value;
						}
						if (reader.MoveToAttribute("dist"))
						{
							disttag = reader.Value;
						}
						if (reader.MoveToAttribute("dir"))
						{
							dirtag = reader.Value;
						}
						shadow.ShadowInnerPresets = Check(text, disttag, dirtag, reader, shadow, parser);
						break;
					case "glow":
					{
						Stream stream = ShapeParser.ReadNodeAsStream(reader);
						stream.Position = 0L;
						shadow.m_glowStream = stream;
						break;
					}
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		else
		{
			shadow.HasCustomShadowStyle = true;
		}
		reader.Read();
	}

	internal static Office2007ChartPresetsOuter Check(string blurval, string sizex, string sizey, string disttag, string dirtag, string align, string rot, ShadowImpl Shadow, XmlReader reader, Excel2007Parser parser)
	{
		int num = 0;
		Office2007ChartPresetsOuter office2007ChartPresetsOuter = Office2007ChartPresetsOuter.NoShadow;
		if (blurval.Equals("null") && sizex.Equals("null") && sizey.Equals("null") && disttag.Equals("null") && dirtag.Equals("null") && align.Equals("null"))
		{
			office2007ChartPresetsOuter = Office2007ChartPresetsOuter.NoShadow;
		}
		else
		{
			for (int i = 0; i < ChartSerializatorCommon.OuterAttributeArray.GetLength(0); i++)
			{
				num++;
				if (blurval.Equals(ChartSerializatorCommon.OuterAttributeArray[i][0]) && sizex.Equals(ChartSerializatorCommon.OuterAttributeArray[i][1]) && sizey.Equals(ChartSerializatorCommon.OuterAttributeArray[i][2]) && disttag.Equals(ChartSerializatorCommon.OuterAttributeArray[i][3]) && dirtag.Equals(ChartSerializatorCommon.OuterAttributeArray[i][4]) && align.Equals(ChartSerializatorCommon.OuterAttributeArray[i][5]) && rot.Equals(ChartSerializatorCommon.OuterAttributeArray[i][6]))
				{
					office2007ChartPresetsOuter = (Office2007ChartPresetsOuter)(i + 1);
					break;
				}
			}
			if (num == ChartSerializatorCommon.OuterAttributeArray.GetLength(0) && office2007ChartPresetsOuter == Office2007ChartPresetsOuter.NoShadow)
			{
				office2007ChartPresetsOuter = Check(blurval, sizex, disttag, dirtag, align, rot, Shadow, reader, parser);
			}
		}
		ParseShadowColor(Shadow, reader, parser);
		return office2007ChartPresetsOuter;
	}

	internal static Office2007ChartPresetsOuter Check(string blurval, string sizex, string disttag, string dirtag, string align, string rot, ShadowImpl Shadow, XmlReader reader, Excel2007Parser parser)
	{
		Office2007ChartPresetsOuter result = Office2007ChartPresetsOuter.NoShadow;
		for (int i = 0; i < ChartSerializatorCommon.OuterAttributeArray.GetLength(0); i++)
		{
			if (align.Equals(ChartSerializatorCommon.OuterAttributeArray[i][5]))
			{
				result = (Office2007ChartPresetsOuter)(i + 1);
				break;
			}
		}
		Shadow.HasCustomShadowStyle = true;
		Shadow.Blur = ((blurval != "null") ? (Convert.ToInt32(blurval) / 12700) : 0);
		Shadow.Size = ((sizex != "null") ? (Convert.ToInt32(sizex) / 1000) : 100);
		Shadow.Distance = ((disttag != "null") ? (Convert.ToInt32(disttag) / 12700) : 0);
		Shadow.Angle = ((dirtag != "null") ? (Convert.ToInt32(dirtag) / 60000) : 0);
		return result;
	}

	public static void ParseShadowAlpha(XmlReader reader, ShadowImpl Shadow)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "prstClr" && reader.LocalName != "schemeClr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "alpha")
				{
					if (reader.MoveToAttribute("val"))
					{
						Shadow.Transparency = 100 - XmlConvertExtension.ToInt32(reader.Value) / 1000;
					}
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	public static void ParseNumberFormat(XmlReader reader, IOfficeChartDataLabels dataLabels)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = null;
		if (reader.MoveToAttribute("formatCode"))
		{
			text = reader.Value;
			if (text != "")
			{
				(dataLabels as ChartDataLabelsImpl).SetNumberFormat(text);
			}
		}
		if (reader.MoveToAttribute("sourceLinked"))
		{
			(dataLabels as ChartDataLabelsImpl).IsSourceLinked = XmlConvertExtension.ToBoolean(reader.Value);
		}
	}

	internal static Office2007ChartPresetsPerspective Check(string blurval, string sizex, string sizey, string kxtag, string disttag, string dirtag, string align, string rot, ShadowImpl shadow, XmlReader reader, Excel2007Parser parser)
	{
		Office2007ChartPresetsPerspective result = Office2007ChartPresetsPerspective.NoShadow;
		for (int i = 0; i < ChartSerializatorCommon.PerspectiveAttributeArray.GetLength(0); i++)
		{
			if (blurval.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][0]) && sizex.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][4]) && sizey.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][3]) && disttag.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][2]) && dirtag.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][1]) && kxtag.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][5]) && align.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][6]) && rot.Equals(ChartSerializatorCommon.PerspectiveAttributeArray[i][7]))
			{
				result = (Office2007ChartPresetsPerspective)i;
				break;
			}
		}
		ParseShadowColor(shadow, reader, parser);
		return result;
	}

	internal static Office2007ChartPresetsInner Check(string blurval, string disttag, string dirtag, XmlReader reader, ShadowImpl Shadow, Excel2007Parser parser)
	{
		int num = 0;
		Office2007ChartPresetsInner office2007ChartPresetsInner = Office2007ChartPresetsInner.NoShadow;
		if (blurval.Equals("null") && disttag.Equals("null") && dirtag.Equals("null"))
		{
			office2007ChartPresetsInner = Office2007ChartPresetsInner.NoShadow;
		}
		else
		{
			for (int i = 0; i < ChartSerializatorCommon.InnerAttributeArray.GetLength(0); i++)
			{
				num++;
				if (blurval.Equals(ChartSerializatorCommon.InnerAttributeArray[i][0]) && disttag.Equals(ChartSerializatorCommon.InnerAttributeArray[i][1]) && dirtag.Equals(ChartSerializatorCommon.InnerAttributeArray[i][2]))
				{
					office2007ChartPresetsInner = (Office2007ChartPresetsInner)(i + 1);
					break;
				}
			}
			if (num == ChartSerializatorCommon.InnerAttributeArray.GetLength(0) && office2007ChartPresetsInner == Office2007ChartPresetsInner.NoShadow)
			{
				office2007ChartPresetsInner = Check(blurval, disttag, dirtag, Shadow, reader, m_HasShadowStyle: true, parser);
			}
		}
		ParseShadowColor(Shadow, reader, parser);
		return office2007ChartPresetsInner;
	}

	internal static Office2007ChartPresetsInner Check(string blurval, string disttag, string dirtag, ShadowImpl Shadow, XmlReader reader, bool m_HasShadowStyle, Excel2007Parser parser)
	{
		Shadow.HasCustomShadowStyle = m_HasShadowStyle;
		if (blurval != "null")
		{
			Shadow.Blur = Convert.ToInt32(blurval) / 12700;
		}
		if (disttag != "null")
		{
			Shadow.Distance = Convert.ToInt32(disttag) / 12700;
		}
		if (dirtag != "null")
		{
			Shadow.Angle = Convert.ToInt32(dirtag) / 60000;
		}
		return Office2007ChartPresetsInner.InsideBottom;
	}

	private static OfficeGradientColor DetectGradientColor(GradientStops gradientStops)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		OfficeGradientColor result = (OfficeGradientColor)(-1);
		switch (gradientStops.Count)
		{
		case 2:
		{
			GradientStopImpl gradientStopImpl4 = gradientStops[0];
			GradientStopImpl gradientStopImpl5 = gradientStops[1];
			result = ((!(gradientStopImpl4.ColorObject == gradientStopImpl5.ColorObject)) ? OfficeGradientColor.TwoColor : OfficeGradientColor.OneColor);
			break;
		}
		case 3:
		{
			GradientStopImpl gradientStopImpl = gradientStops[0];
			GradientStopImpl gradientStopImpl2 = gradientStops[1];
			GradientStopImpl gradientStopImpl3 = gradientStops[2];
			if (gradientStopImpl.ColorObject == gradientStopImpl3.ColorObject)
			{
				result = ((!(gradientStopImpl.ColorObject == gradientStopImpl2.ColorObject)) ? OfficeGradientColor.TwoColor : OfficeGradientColor.OneColor);
			}
			break;
		}
		}
		return result;
	}

	private static OfficeGradientVariants DetectGradientVariant(GradientStops gradientStops, OfficeGradientStyle gradientStyle, OfficeGradientColor gradientColor, bool isPresetInverted)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		OfficeGradientVariants result = OfficeGradientVariants.ShadingVariants_1;
		bool flag = IsInverted(gradientStops, gradientColor, isPresetInverted);
		bool isDoubled = gradientStops.IsDoubled;
		switch (gradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
		case OfficeGradientStyle.Vertical:
		case OfficeGradientStyle.DiagonalUp:
			result = DetectStandardVariant(flag, isDoubled);
			break;
		case OfficeGradientStyle.DiagonalDown:
			result = DetectDiagonalDownVariant(flag, isDoubled);
			break;
		case OfficeGradientStyle.FromCorner:
			result = DetectGradientVariantCorner(gradientStops.FillToRect);
			break;
		case OfficeGradientStyle.FromCenter:
			result = (flag ? OfficeGradientVariants.ShadingVariants_1 : OfficeGradientVariants.ShadingVariants_2);
			break;
		}
		return result;
	}

	private static OfficeGradientVariants DetectDiagonalDownVariant(bool bInverted, bool bDoubled)
	{
		if (bInverted && bDoubled)
		{
			return OfficeGradientVariants.ShadingVariants_4;
		}
		if (bDoubled)
		{
			return OfficeGradientVariants.ShadingVariants_3;
		}
		if (bInverted)
		{
			return OfficeGradientVariants.ShadingVariants_1;
		}
		return OfficeGradientVariants.ShadingVariants_2;
	}

	private static OfficeGradientVariants DetectStandardVariant(bool bInverted, bool bDoubled)
	{
		if (bInverted && bDoubled)
		{
			return OfficeGradientVariants.ShadingVariants_4;
		}
		if (bDoubled)
		{
			return OfficeGradientVariants.ShadingVariants_3;
		}
		if (bInverted)
		{
			return OfficeGradientVariants.ShadingVariants_2;
		}
		return OfficeGradientVariants.ShadingVariants_1;
	}

	private static OfficeGradientVariants DetectGradientVariantCorner(Rectangle rectangle)
	{
		Rectangle[] rectanglesCorner = ShapeFillImpl.RectanglesCorner;
		OfficeGradientVariants result = OfficeGradientVariants.ShadingVariants_1;
		int i = 0;
		for (int num = rectanglesCorner.Length; i < num; i++)
		{
			if (rectanglesCorner[i] == rectangle)
			{
				result = (OfficeGradientVariants)i;
				break;
			}
		}
		return result;
	}

	private static bool IsInverted(GradientStops gradientStops, OfficeGradientColor gradientColor, bool isPresetInverted)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		bool result = false;
		switch (gradientColor)
		{
		case OfficeGradientColor.OneColor:
			if (gradientStops[0].Shade > 0 || gradientStops[0].Tint > 0)
			{
				result = true;
			}
			break;
		case OfficeGradientColor.TwoColor:
			result = false;
			break;
		case OfficeGradientColor.Preset:
			result = isPresetInverted;
			break;
		}
		return result;
	}

	private static OfficeGradientStyle DetectGradientStyle(GradientStops gradientStops)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		OfficeGradientStyle result = OfficeGradientStyle.Horizontal;
		switch (gradientStops.GradientType)
		{
		case GradientType.Liniar:
			result = GetLiniarGradientStyle(gradientStops);
			break;
		case GradientType.Rect:
			result = GetRectGradientStyle(gradientStops);
			break;
		}
		return result;
	}

	private static OfficeGradientStyle GetRectGradientStyle(GradientStops gradientStops)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		if (!(gradientStops.FillToRect == ShapeFillImpl.RectangleFromCenter))
		{
			return OfficeGradientStyle.FromCorner;
		}
		return OfficeGradientStyle.FromCenter;
	}

	private static OfficeGradientStyle GetLiniarGradientStyle(GradientStops gradientStops)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		int angle = gradientStops.Angle;
		if (angle == 0)
		{
			return OfficeGradientStyle.Vertical;
		}
		if (angle <= 2700000)
		{
			return OfficeGradientStyle.DiagonalUp;
		}
		if (angle <= 5400000)
		{
			return OfficeGradientStyle.Horizontal;
		}
		return OfficeGradientStyle.DiagonalDown;
	}

	private static OfficeGradientPreset FindPreset(GradientStops gradientStops, out bool isInverted)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		OfficeGradientPreset[] array = (OfficeGradientPreset[])Enum.GetValues(typeof(OfficeGradientPreset));
		if (gradientStops.IsDoubled)
		{
			gradientStops = gradientStops.ShrinkGradientStops();
		}
		isInverted = false;
		OfficeGradientPreset result = (OfficeGradientPreset)(-1);
		GradientStops gradientStops2 = gradientStops.Clone();
		gradientStops2.InvertGradientStops();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			OfficeGradientPreset officeGradientPreset = array[i];
			GradientStops presetGradientStops = ShapeFillImpl.GetPresetGradientStops(officeGradientPreset);
			if (presetGradientStops.EqualColors(gradientStops))
			{
				result = officeGradientPreset;
				break;
			}
			if (presetGradientStops.EqualColors(gradientStops2))
			{
				isInverted = true;
				result = officeGradientPreset;
				break;
			}
		}
		return result;
	}

	private static void SetGradientDegree(GradientStops gradientStops, OfficeGradientColor gradientColor, IOfficeFill fill)
	{
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		if (gradientColor == OfficeGradientColor.OneColor)
		{
			int num = Math.Max(gradientStops[0].Tint, gradientStops[1].Tint);
			int num2 = Math.Max(gradientStops[0].Shade, gradientStops[1].Shade);
			double gradientDegree = ((num2 > 0) ? ((double)num2 / 100000.0) : ((num <= 0) ? 0.5 : (1.0 - (double)num / 100000.0)));
			fill.GradientDegree = gradientDegree;
		}
	}

	internal static void SetShadowValues(IShadow axisShadow, IShadow frameShadow)
	{
		axisShadow.Angle = frameShadow.Angle;
		axisShadow.Blur = frameShadow.Blur;
		axisShadow.Distance = frameShadow.Distance;
		axisShadow.HasCustomShadowStyle = frameShadow.HasCustomShadowStyle;
		axisShadow.ShadowColor = frameShadow.ShadowColor;
		axisShadow.ShadowInnerPresets = frameShadow.ShadowInnerPresets;
		axisShadow.ShadowOuterPresets = frameShadow.ShadowOuterPresets;
		axisShadow.ShadowPerspectivePresets = frameShadow.ShadowPerspectivePresets;
		axisShadow.Size = frameShadow.Size;
		axisShadow.Transparency = frameShadow.Transparency;
	}

	internal static void SetInteriorValues(IOfficeChartInterior interior, IOfficeChartInterior frameInterior)
	{
		interior.BackgroundColor = frameInterior.BackgroundColor;
		interior.BackgroundColorIndex = frameInterior.BackgroundColorIndex;
		interior.ForegroundColor = frameInterior.ForegroundColor;
		interior.ForegroundColorIndex = frameInterior.ForegroundColorIndex;
		interior.Pattern = frameInterior.Pattern;
		interior.SwapColorsOnNegative = frameInterior.SwapColorsOnNegative;
		interior.UseAutomaticFormat = frameInterior.UseAutomaticFormat;
	}

	internal static void SetThreeDValues(IThreeDFormat threeD, IThreeDFormat frameThreeD)
	{
		threeD.BevelBottom = frameThreeD.BevelBottom;
		threeD.BevelBottomHeight = frameThreeD.BevelBottomHeight;
		threeD.BevelBottomWidth = frameThreeD.BevelBottomWidth;
		threeD.BevelTop = frameThreeD.BevelTop;
		threeD.BevelTopHeight = frameThreeD.BevelTopHeight;
		threeD.BevelTopWidth = frameThreeD.BevelTopWidth;
		threeD.Lighting = frameThreeD.Lighting;
		threeD.Material = frameThreeD.Material;
	}

	internal static void clear()
	{
		if (m_book != null)
		{
			m_book = null;
		}
	}
}

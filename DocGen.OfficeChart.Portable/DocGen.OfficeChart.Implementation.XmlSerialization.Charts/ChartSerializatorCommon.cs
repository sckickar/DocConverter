using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartSerializatorCommon
{
	private static Dictionary<OfficeChartLinePattern, KeyValuePair<string, string>> s_dicLinePatterns;

	public static string[][] OuterAttributeArray;

	public static string[][] InnerAttributeArray;

	public static string[][] PerspectiveAttributeArray;

	public static string[][] BevelProperties;

	public static string[][] MaterialProperties;

	public static string[][] LightingProperties;

	static ChartSerializatorCommon()
	{
		s_dicLinePatterns = new Dictionary<OfficeChartLinePattern, KeyValuePair<string, string>>();
		OuterAttributeArray = new string[9][];
		InnerAttributeArray = new string[9][];
		PerspectiveAttributeArray = new string[6][];
		BevelProperties = new string[13][];
		MaterialProperties = new string[11][];
		LightingProperties = new string[15][];
		s_dicLinePatterns.Add(OfficeChartLinePattern.Solid, new KeyValuePair<string, string>("solid", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.Dash, new KeyValuePair<string, string>("dash", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.Dot, new KeyValuePair<string, string>("sysDash", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.CircleDot, new KeyValuePair<string, string>("sysDot", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.DashDot, new KeyValuePair<string, string>("dashDot", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.DashDotDot, new KeyValuePair<string, string>("lgDashDotDot", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.LongDash, new KeyValuePair<string, string>("lgDash", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.LongDashDot, new KeyValuePair<string, string>("lgDashDot", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.LongDashDotDot, new KeyValuePair<string, string>("lgDashDotDot", string.Empty));
		s_dicLinePatterns.Add(OfficeChartLinePattern.DarkGray, new KeyValuePair<string, string>("solid", "pct75"));
		s_dicLinePatterns.Add(OfficeChartLinePattern.MediumGray, new KeyValuePair<string, string>("solid", "pct50"));
		s_dicLinePatterns.Add(OfficeChartLinePattern.LightGray, new KeyValuePair<string, string>("solid", "pct25"));
		OuterAttributeArray[0] = new string[7] { "50800", "null", "null", "38100", "null", "l", "0" };
		OuterAttributeArray[1] = new string[7] { "50800", "null", "null", "38100", "2700000", "tl", "0" };
		OuterAttributeArray[2] = new string[7] { "50800", "null", "null", "38100", "5400000", "t", "0" };
		OuterAttributeArray[3] = new string[7] { "50800", "null", "null", "38100", "13500000", "br", "0" };
		OuterAttributeArray[4] = new string[7] { "63500", "102000", "102000", "null", "null", "ctr", "0" };
		OuterAttributeArray[5] = new string[7] { "50800", "null", "null", "38100", "16200000", "null", "0" };
		OuterAttributeArray[6] = new string[7] { "50800", "null", "null", "38100", "10800000", "r", "0" };
		OuterAttributeArray[7] = new string[7] { "50800", "null", "null", "38100", "18900000", "bl", "0" };
		OuterAttributeArray[8] = new string[7] { "50800", "null", "null", "38100", "8100000", "tr", "0" };
		InnerAttributeArray[0] = new string[3] { "63500", "50800", "8100000" };
		InnerAttributeArray[1] = new string[3] { "63500", "50800", "16200000" };
		InnerAttributeArray[2] = new string[3] { "63500", "50800", "null" };
		InnerAttributeArray[3] = new string[3] { "63500", "50800", "10800000" };
		InnerAttributeArray[4] = new string[3] { "63500", "50800", "18900000" };
		InnerAttributeArray[5] = new string[3] { "63500", "50800", "2700000" };
		InnerAttributeArray[6] = new string[3] { "114300", "null", "null" };
		InnerAttributeArray[7] = new string[3] { "63500", "50800", "5400000" };
		InnerAttributeArray[8] = new string[3] { "63500", "50800", "13500000" };
		PerspectiveAttributeArray[0] = new string[8] { "null", "null", "null", "null", "null", "null", "null", "null" };
		PerspectiveAttributeArray[1] = new string[8] { "76200", "18900000", "null", "23000", "null", "-1200000", "bl", "0" };
		PerspectiveAttributeArray[2] = new string[8] { "76200", "2700000", "12700", "-23000", "null", "-800400", "bl", "0" };
		PerspectiveAttributeArray[3] = new string[8] { "76200", "13500000", "null", "23000", "null", "1200000", "br", "0" };
		PerspectiveAttributeArray[4] = new string[8] { "76200", "8100000", "12700", "-23000", "null", "800400", "br", "0" };
		PerspectiveAttributeArray[5] = new string[8] { "152400", "5400000", "317500", "-19000", "90000", "null", "null", "0" };
		BevelProperties[0] = new string[3] { "null", "null", "null" };
		BevelProperties[1] = new string[3] { "null", "null", "angle" };
		BevelProperties[2] = new string[3] { "114300", "null", "artDeco" };
		BevelProperties[3] = new string[3] { "null", "null", "null" };
		BevelProperties[4] = new string[3] { "null", "null", "convex" };
		BevelProperties[5] = new string[3] { "165100", "null", "coolSlant" };
		BevelProperties[6] = new string[3] { "139700", "null", "cross" };
		BevelProperties[7] = new string[3] { "139700", "139700", "divot" };
		BevelProperties[8] = new string[3] { "114300", "null", "hardEdge" };
		BevelProperties[9] = new string[3] { "null", "null", "relaxedInset" };
		BevelProperties[10] = new string[3] { "101600", "null", "riblet" };
		BevelProperties[11] = new string[3] { "null", "null", "slope" };
		BevelProperties[12] = new string[3] { "152400", "50800", "softRound" };
		MaterialProperties[0] = new string[1] { "matte" };
		MaterialProperties[1] = new string[1] { "null" };
		MaterialProperties[2] = new string[1] { "plastic" };
		MaterialProperties[3] = new string[1] { "metal" };
		MaterialProperties[4] = new string[1] { "dkEdge" };
		MaterialProperties[5] = new string[1] { "softEdge" };
		MaterialProperties[6] = new string[1] { "flat" };
		MaterialProperties[7] = new string[1] { "legacyWireframe" };
		MaterialProperties[8] = new string[1] { "powder" };
		MaterialProperties[9] = new string[1] { "translucentPowder" };
		MaterialProperties[10] = new string[1] { "matte" };
		LightingProperties[0] = new string[1] { "threePt" };
		LightingProperties[1] = new string[1] { "balanced" };
		LightingProperties[2] = new string[1] { "brightRoom" };
		LightingProperties[3] = new string[1] { "chilly" };
		LightingProperties[4] = new string[1] { "contrasting" };
		LightingProperties[5] = new string[1] { "flat" };
		LightingProperties[6] = new string[1] { "flood" };
		LightingProperties[7] = new string[1] { "freezing" };
		LightingProperties[8] = new string[1] { "glow" };
		LightingProperties[9] = new string[1] { "harsh" };
		LightingProperties[10] = new string[1] { "morning" };
		LightingProperties[11] = new string[1] { "soft" };
		LightingProperties[12] = new string[1] { "sunrise" };
		LightingProperties[13] = new string[1] { "sunset" };
		LightingProperties[14] = new string[1] { "twoPt" };
	}

	[SecurityCritical]
	public static void SerializeFrameFormat(XmlWriter writer, IOfficeChartFillBorder format, ChartImpl chart, bool isRoundCorners)
	{
		SerializeFrameFormat(writer, format, chart, isRoundCorners, serializeLineAutoValues: false);
	}

	[SecurityCritical]
	public static void SerializeFrameFormat(XmlWriter writer, IOfficeChartFillBorder format, ChartImpl chart, bool isRoundCorners, bool serializeLineAutoValues)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (format != null)
		{
			FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
			RelationCollection relations = chart.Relations;
			SerializeFrameFormat(writer, format, parentHolder, relations, isRoundCorners, serializeLineAutoValues);
		}
	}

	[SecurityCritical]
	public static void SerializeFrameFormat(XmlWriter writer, IOfficeChartFillBorder format, FileDataHolder holder, RelationCollection relations, bool isRoundCorners, bool serilaizeLineAutoValues)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			return;
		}
		if (ChartImpl.IsChartExSerieType(((ChartImpl)(format as CommonObject).FindParent(typeof(ChartImpl))).ChartType))
		{
			writer.WriteStartElement("spPr", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		}
		else
		{
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		if (format.HasInterior)
		{
			if (!format.Interior.UseAutomaticFormat && (format.Interior.Pattern != 0 || (format.Fill.FillType != OfficeFillType.Pattern && format.Fill.FillType != 0)))
			{
				if (format.Fill is IInternalFill fill)
				{
					SerializeFill(writer, fill, holder, relations);
				}
			}
			else if (format.Interior.Pattern == OfficePattern.None)
			{
				writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
			}
		}
		if (format.HasLineProperties)
		{
			IOfficeChartBorder lineProperties = format.LineProperties;
			if (!lineProperties.AutoFormat)
			{
				SerializeLineProperties(writer, lineProperties, isRoundCorners, holder.Workbook, serilaizeLineAutoValues);
			}
		}
		if (format.HasShadowProperties)
		{
			IShadow shadow = format.Shadow;
			SerializeShadow(writer, shadow, format.Shadow.HasCustomShadowStyle);
		}
		else
		{
			IShadow shadow2 = format.Shadow;
			SerializeShadow(writer, shadow2, format.Shadow.HasCustomShadowStyle);
		}
		if (format.Has3dProperties)
		{
			IThreeDFormat threeD = format.ThreeD;
			Serialize3D(writer, threeD);
		}
		writer.WriteEndElement();
	}

	public static void SerializeShadow(XmlWriter writer, IShadow shadow, bool CustomShadow)
	{
		if (shadow.ShadowInnerPresets != 0)
		{
			int inner = (int)(shadow.ShadowInnerPresets - 1);
			SerializeInner(writer, inner, CustomShadow, shadow);
		}
		else if (shadow.ShadowOuterPresets != 0)
		{
			int outer = (int)(shadow.ShadowOuterPresets - 1);
			SerailizeOuter(writer, outer, CustomShadow, shadow);
		}
		else if (shadow.ShadowPerspectivePresets != 0)
		{
			int shadowPerspectivePresets = (int)shadow.ShadowPerspectivePresets;
			SerializePerspective(writer, shadowPerspectivePresets, CustomShadow, shadow);
		}
		else if ((shadow as ShadowImpl).m_glowStream != null)
		{
			writer.WriteStartElement("effectLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
			ShapeParser.WriteNodeFromStream(writer, (shadow as ShadowImpl).m_glowStream);
			writer.WriteEndElement();
		}
		else if (shadow.HasCustomShadowStyle)
		{
			writer.WriteStartElement("effectLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeCustomShadowProperties(writer, shadow);
			writer.WriteEndElement();
		}
	}

	internal static void SerializeCustomShadowProperties(XmlWriter writer, IShadow shadow)
	{
		if (shadow.Blur != 0 || shadow.Size != 0 || shadow.Distance != 0 || shadow.Angle != 0 || shadow.Transparency != -1)
		{
			writer.WriteStartElement("outerShdw", "http://schemas.openxmlformats.org/drawingml/2006/main");
			string[] array = new string[8] { "50800", "100000", "100000", "50800", "5400000", "ctr", "0", "43137" };
			writer.WriteAttributeString("blurRad", (shadow.Blur != 0) ? (shadow.Blur * 12700).ToString() : array[0]);
			writer.WriteAttributeString("sx", (shadow.Size != 0) ? (shadow.Size * 1000).ToString() : array[1]);
			writer.WriteAttributeString("sy", (shadow.Size != 0) ? (shadow.Size * 1000).ToString() : array[2]);
			writer.WriteAttributeString("dist", (shadow.Distance != 0) ? (shadow.Distance * 12700).ToString() : array[3]);
			writer.WriteAttributeString("dir", (shadow.Angle != 0) ? (shadow.Angle * 60000).ToString() : array[4]);
			writer.WriteAttributeString("algn", array[5]);
			writer.WriteAttributeString("rotWithShape", array[6].ToString());
			writer.WriteStartElement("srgbClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", (shadow.ShadowColor.ToArgb() & 0xFFFFFF).ToString("X6"));
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", (shadow.Transparency != -1) ? ((100 - shadow.Transparency) * 1000).ToString() : array[7]);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	public static void SerializeInner(XmlWriter writer, int inner, bool CustomShadow, IShadow Shadow)
	{
		writer.WriteStartElement("effectLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if ((Shadow as ShadowImpl).m_glowStream != null)
		{
			ShapeParser.WriteNodeFromStream(writer, (Shadow as ShadowImpl).m_glowStream);
		}
		writer.WriteStartElement("innerShdw", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (CustomShadow)
		{
			writer.WriteAttributeString("blurRad", (Shadow.Blur * 12700).ToString());
		}
		else
		{
			writer.WriteAttributeString("blurRad", InnerAttributeArray[inner][0].ToString());
		}
		if (CustomShadow)
		{
			writer.WriteAttributeString("dist", (Shadow.Distance * 12700).ToString());
		}
		else if (!InnerAttributeArray[inner][1].Equals("null"))
		{
			writer.WriteAttributeString("dist", InnerAttributeArray[inner][1].ToString());
		}
		if (CustomShadow)
		{
			writer.WriteAttributeString("dir", (Shadow.Angle * 60000).ToString());
		}
		else if (!InnerAttributeArray[inner][2].Equals("null"))
		{
			writer.WriteAttributeString("dir", InnerAttributeArray[inner][2].ToString());
		}
		writer.WriteStartElement("srgbClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", (Shadow.ShadowColor.ToArgb() & 0xFFFFFF).ToString("X6"));
		if (Shadow.Transparency != -1)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", ((100 - Shadow.Transparency) * 1000).ToString());
			writer.WriteEndElement();
		}
		else if (!CustomShadow && inner != 6)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", "50000");
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void SerailizeOuter(XmlWriter writer, int outer, bool CustomShadow, IShadow Shadow)
	{
		writer.WriteStartElement("effectLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if ((Shadow as ShadowImpl).m_glowStream != null)
		{
			ShapeParser.WriteNodeFromStream(writer, (Shadow as ShadowImpl).m_glowStream);
		}
		writer.WriteStartElement("outerShdw", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (CustomShadow)
		{
			writer.WriteAttributeString("blurRad", (Shadow.Blur * 12700).ToString());
		}
		else
		{
			writer.WriteAttributeString("blurRad", OuterAttributeArray[outer][0].ToString());
		}
		if (CustomShadow)
		{
			if (Shadow.Size != 0)
			{
				writer.WriteAttributeString("sx", (Shadow.Size * 1000).ToString());
				writer.WriteAttributeString("sy", (Shadow.Size * 1000).ToString());
			}
		}
		else
		{
			if (!OuterAttributeArray[outer][1].Equals("null"))
			{
				writer.WriteAttributeString("sx", OuterAttributeArray[outer][1].ToString());
			}
			if (!OuterAttributeArray[outer][2].Equals("null"))
			{
				writer.WriteAttributeString("sy", OuterAttributeArray[outer][2].ToString());
			}
		}
		if (CustomShadow)
		{
			writer.WriteAttributeString("dist", (Shadow.Distance * 12700).ToString());
		}
		else if (!OuterAttributeArray[outer][3].Equals("null"))
		{
			writer.WriteAttributeString("dist", OuterAttributeArray[outer][3].ToString());
		}
		if (CustomShadow)
		{
			writer.WriteAttributeString("dir", (Shadow.Angle * 60000).ToString());
		}
		else if (!OuterAttributeArray[outer][4].Equals("null"))
		{
			writer.WriteAttributeString("dir", OuterAttributeArray[outer][4].ToString());
		}
		if (!OuterAttributeArray[outer][5].Equals("null"))
		{
			writer.WriteAttributeString("algn", OuterAttributeArray[outer][5].ToString());
		}
		writer.WriteAttributeString("rotWithShape", OuterAttributeArray[outer][6].ToString());
		writer.WriteStartElement("srgbClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", (Shadow.ShadowColor.ToArgb() & 0xFFFFFF).ToString("X6"));
		SerializeSchemeColor(writer, Shadow);
		if (Shadow.Transparency != -1)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", ((100 - Shadow.Transparency) * 1000).ToString());
			writer.WriteEndElement();
		}
		else if (!CustomShadow)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", "40000");
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private static void SerializeSchemeColor(XmlWriter writer, IShadow shadow)
	{
		ChartColor colorObject = (shadow as ShadowImpl).ColorObject;
		if (colorObject.Tint > 0.0)
		{
			SerializeDoubleValueTag(writer, "tint", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Tint);
		}
		if (colorObject.Saturation > 0.0)
		{
			SerializeDoubleValueTag(writer, "satMod", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Saturation);
		}
		if (colorObject.Luminance > 0.0)
		{
			SerializeDoubleValueTag(writer, "lumMod", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Luminance);
		}
		if (colorObject.LuminanceOffSet > 0.0)
		{
			SerializeDoubleValueTag(writer, "lumOff", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.LuminanceOffSet);
		}
	}

	public static void SerializePerspective(XmlWriter writer, int perspective, bool CustomShadow, IShadow Shadow)
	{
		writer.WriteStartElement("effectLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if ((Shadow as ShadowImpl).m_glowStream != null)
		{
			ShapeParser.WriteNodeFromStream(writer, (Shadow as ShadowImpl).m_glowStream);
		}
		writer.WriteStartElement("outerShdw", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (CustomShadow)
		{
			writer.WriteAttributeString("blurRad", (Shadow.Blur * 12700).ToString());
			writer.WriteAttributeString("dir", (Shadow.Angle * 60000).ToString());
		}
		else
		{
			writer.WriteAttributeString("blurRad", PerspectiveAttributeArray[perspective][0].ToString());
			writer.WriteAttributeString("dir", PerspectiveAttributeArray[perspective][1].ToString());
		}
		if (CustomShadow)
		{
			writer.WriteAttributeString("dist", (Shadow.Distance * 12700).ToString());
		}
		else if (!PerspectiveAttributeArray[perspective][2].Equals("null"))
		{
			writer.WriteAttributeString("dist", PerspectiveAttributeArray[perspective][2].ToString());
		}
		if (CustomShadow)
		{
			if (Shadow.Size != 0)
			{
				writer.WriteAttributeString("sy", (Shadow.Size * 1000).ToString());
				writer.WriteAttributeString("sx", (Shadow.Size * 1000).ToString());
			}
		}
		else
		{
			if (!PerspectiveAttributeArray[perspective][3].Equals("null"))
			{
				writer.WriteAttributeString("sy", PerspectiveAttributeArray[perspective][3].ToString());
			}
			if (!PerspectiveAttributeArray[perspective][4].Equals("null"))
			{
				writer.WriteAttributeString("sx", PerspectiveAttributeArray[perspective][4].ToString());
			}
		}
		if (!PerspectiveAttributeArray[perspective][5].Equals("null"))
		{
			writer.WriteAttributeString("kx", PerspectiveAttributeArray[perspective][5].ToString());
		}
		if (!PerspectiveAttributeArray[perspective][6].Equals("null"))
		{
			writer.WriteAttributeString("algn", PerspectiveAttributeArray[perspective][6].ToString());
		}
		writer.WriteAttributeString("rotWithShape", PerspectiveAttributeArray[perspective][7].ToString());
		writer.WriteStartElement("srgbClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", (Shadow.ShadowColor.ToArgb() & 0xFFFFFF).ToString("X6"));
		if (Shadow.Transparency != -1)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", ((100 - Shadow.Transparency) * 1000).ToString());
			writer.WriteEndElement();
		}
		else if (!CustomShadow)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if (perspective != 4)
			{
				writer.WriteAttributeString("val", "20000");
			}
			else
			{
				writer.WriteAttributeString("val", "15000");
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void Serialize3D(XmlWriter writer, IThreeDFormat Three_D)
	{
		bool num = Three_D.BevelBottom == Office2007ChartBevelProperties.NoAngle;
		bool flag = Three_D.BevelTop == Office2007ChartBevelProperties.NoAngle;
		bool flag2 = Three_D.Material == Office2007ChartMaterialProperties.NoEffect;
		ThreeDFormatImpl threeDFormatImpl = Three_D as ThreeDFormatImpl;
		bool flag3 = !threeDFormatImpl.IsBevelBottomHeightSet && !threeDFormatImpl.IsBevelBottomWidthSet;
		bool flag4 = !threeDFormatImpl.IsBevelTopHeightSet && !threeDFormatImpl.IsBevelTopWidthSet;
		if (!(num && flag && flag2 && flag3 && flag4))
		{
			writer.WriteStartElement("scene3d", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteStartElement("camera", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("prst", "orthographicFront");
			writer.WriteEndElement();
			writer.WriteStartElement("lightRig", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if (Three_D.Lighting != 0)
			{
				int lighting = (int)Three_D.Lighting;
				SerializeLight(writer, lighting);
			}
			else
			{
				writer.WriteAttributeString("rig", LightingProperties[0][0].ToString());
			}
			writer.WriteAttributeString("dir", "t");
			if (threeDFormatImpl.LightningAngle != 0.0)
			{
				writer.WriteStartElement("rot", "http://schemas.openxmlformats.org/drawingml/2006/main");
				writer.WriteAttributeString("lat", threeDFormatImpl.LightningLatitude.ToString());
				writer.WriteAttributeString("lon", threeDFormatImpl.LightningLongitude.ToString());
				writer.WriteAttributeString("rev", (threeDFormatImpl.LightningAngle * 60000.0).ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
			if (Three_D.Material != 0)
			{
				writer.WriteStartElement("sp3d", "http://schemas.openxmlformats.org/drawingml/2006/main");
				int material = (int)(Three_D.Material - 1);
				SerializeMaterial(writer, material);
			}
			else
			{
				writer.WriteStartElement("sp3d", "http://schemas.openxmlformats.org/drawingml/2006/main");
			}
			if (Three_D.BevelTop != 0)
			{
				int bevelTop = (int)Three_D.BevelTop;
				SerializeTopBevel(writer, bevelTop, threeDFormatImpl);
			}
			else if (!flag4)
			{
				SerializeTopBevel(writer, threeDFormatImpl, threeDFormatImpl.PresetShape);
			}
			if (Three_D.BevelBottom != 0)
			{
				int bevelBottom = (int)Three_D.BevelBottom;
				SerializeBottomBevel(writer, bevelBottom, threeDFormatImpl);
			}
			else if (!flag3)
			{
				SerializeBottomBevel(writer, threeDFormatImpl);
			}
			writer.WriteEndElement();
		}
	}

	public static void SerializeLight(XmlWriter writer, int light)
	{
		writer.WriteAttributeString("rig", LightingProperties[light][0].ToString());
	}

	public static void SerializeMaterial(XmlWriter writer, int material)
	{
		writer.WriteAttributeString("prstMaterial", MaterialProperties[material][0].ToString());
	}

	internal static void SerializeTopBevel(XmlWriter writer, int bevel, ThreeDFormatImpl threeDFormatImpl)
	{
		writer.WriteStartElement("bevelT", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (threeDFormatImpl.IsBevelTopWidthSet)
		{
			writer.WriteAttributeString("w", (threeDFormatImpl.BevelTopWidth * 12700).ToString());
		}
		else if (!BevelProperties[bevel][0].Equals("null"))
		{
			writer.WriteAttributeString("w", BevelProperties[bevel][0].ToString());
		}
		if (threeDFormatImpl.IsBevelTopHeightSet)
		{
			writer.WriteAttributeString("h", (threeDFormatImpl.BevelTopHeight * 12700).ToString());
		}
		else if (!BevelProperties[bevel][1].Equals("null"))
		{
			writer.WriteAttributeString("h", BevelProperties[bevel][1].ToString());
		}
		if (!BevelProperties[bevel][2].Equals("null"))
		{
			writer.WriteAttributeString("prst", BevelProperties[bevel][2].ToString());
		}
		writer.WriteEndElement();
	}

	public static void SerializeBottomBevel(XmlWriter writer, int bevel, ThreeDFormatImpl threeDFormatImpl)
	{
		writer.WriteStartElement("bevelB", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (threeDFormatImpl.IsBevelBottomWidthSet)
		{
			writer.WriteAttributeString("w", (threeDFormatImpl.BevelBottomWidth * 12700).ToString());
		}
		else if (!BevelProperties[bevel][0].Equals("null"))
		{
			writer.WriteAttributeString("w", BevelProperties[bevel][0].ToString());
		}
		if (threeDFormatImpl.IsBevelBottomHeightSet)
		{
			writer.WriteAttributeString("h", (threeDFormatImpl.BevelBottomHeight * 12700).ToString());
		}
		else if (!BevelProperties[bevel][1].Equals("null"))
		{
			writer.WriteAttributeString("h", BevelProperties[bevel][1].ToString());
		}
		if (!BevelProperties[bevel][2].Equals("null"))
		{
			writer.WriteAttributeString("prst", BevelProperties[bevel][2].ToString());
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	internal static void SerializeFill(XmlWriter writer, IInternalFill fill, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fill == null)
		{
			return;
		}
		switch (fill.FillType)
		{
		case OfficeFillType.SolidColor:
			SerializeSolidFill(writer, fill.ForeColorObject, isAutoColor: false, holder.Workbook, 1.0 - fill.Transparency);
			break;
		case OfficeFillType.Pattern:
			SerializePatternFill(writer, fill.ForeColorObject, isAutoFore: false, fill.BackColorObject, isAutoBack: false, fill.Pattern, holder.Workbook);
			break;
		case OfficeFillType.Picture:
		{
			CommonObject commonObject = fill as CommonObject;
			ChartImpl chart = null;
			if (commonObject != null)
			{
				chart = commonObject.FindParent(typeof(ChartImpl)) as ChartImpl;
			}
			SerializePictureFill(writer, fill.Picture, holder, relations, fill as ShapeFillImpl, chart);
			break;
		}
		case OfficeFillType.Texture:
			SerializeTextureFill(writer, fill, holder, relations);
			break;
		case OfficeFillType.Gradient:
			SerializeGradientFill(writer, fill, holder.Workbook);
			break;
		default:
			writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
			break;
		}
	}

	[SecurityCritical]
	public static void SerializeTextArea(XmlWriter writer, IOfficeChartTextArea textArea, WorkbookImpl book, RelationCollection relations, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		ChartTextAreaImpl chartTextAreaImpl = textArea as ChartTextAreaImpl;
		FileDataHolder dataHolder = book.DataHolder;
		bool flag = false;
		writer.WriteStartElement("title", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (chartTextAreaImpl.Text != "Chart Title" && chartTextAreaImpl.Text != null)
		{
			SerializeTextAreaText(writer, textArea, book, defaultFontSize);
			flag = true;
		}
		SerializeLayout(writer, textArea);
		SerializeValueTag(writer, "overlay", chartTextAreaImpl.Overlay ? "1" : "0");
		SerializeFrameFormat(writer, textArea.FrameFormat, dataHolder, relations, isRoundCorners: false, serilaizeLineAutoValues: false);
		if (chartTextAreaImpl.ParagraphType == ChartParagraphType.CustomDefault || (!flag && chartTextAreaImpl.ParagraphType == ChartParagraphType.Default && chartTextAreaImpl.ShowBoldProperties))
		{
			SerializeDefaultTextFormatting(writer, textArea, book, defaultFontSize, isAutoTextRotation: true, 0, Excel2007TextRotation.horz, "http://schemas.openxmlformats.org/drawingml/2006/chart", isChartExText: false, isEndParagraph: false);
		}
		writer.WriteEndElement();
	}

	public static void SerializeValueTag(XmlWriter writer, string tagName, string value)
	{
		SerializeValueTag(writer, tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart", value);
	}

	public static void SerializeDoubleValueTag(XmlWriter writer, string tagName, double value)
	{
		SerializeDoubleValueTag(writer, tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart", value);
	}

	public static void SerializeValueTag(XmlWriter writer, string tagName, string tagNamespace, string value)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null)
		{
			throw new ArgumentNullException("tagName");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		writer.WriteStartElement(tagName, tagNamespace);
		writer.WriteAttributeString("val", value);
		writer.WriteEndElement();
	}

	public static void SerializeDoubleValueTag(XmlWriter writer, string tagName, string tagNamespace, double value)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null)
		{
			throw new ArgumentNullException("tagName");
		}
		writer.WriteStartElement(tagName, tagNamespace);
		writer.WriteStartAttribute("val");
		writer.WriteValue(value);
		writer.WriteEndAttribute();
		writer.WriteEndElement();
	}

	public static void SerializeBoolValueTag(XmlWriter writer, string tagName, bool value)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null)
		{
			throw new ArgumentNullException("tagName");
		}
		writer.WriteStartElement(tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string value2 = (value ? "1" : "0");
		writer.WriteAttributeString("val", value2);
		writer.WriteEndElement();
	}

	public static void SerializeLineProperties(XmlWriter writer, IOfficeChartBorder border, IWorkbook book)
	{
		SerializeLineProperties(writer, border, bRoundCorners: false, book, serializeAutoFormat: false);
	}

	public static void SerializePatternFill(XmlWriter writer, ChartColor color, bool bAutoColor, string strDash2007, string strPreset, IWorkbook book, double Alphavalue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strPreset == null || strPreset.Length == 0)
		{
			SerializeSolidFill(writer, color, bAutoColor, book, Alphavalue);
		}
		else
		{
			ChartColor backColor = new ChartColor(ColorExtension.White);
			SerializePatternFill(writer, color, bAutoColor, backColor, bAutoColor, strPreset, book, Alphavalue);
		}
		SerializeValueTag(writer, "prstDash", "http://schemas.openxmlformats.org/drawingml/2006/main", strDash2007);
	}

	public static void SerializePatternFill(XmlWriter writer, ChartColor foreColor, bool isAutoFore, ChartColor backColor, bool isAutoBack, string strPreset, IWorkbook book, double Alphavalue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strPreset == null || strPreset.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strPreset");
		}
		writer.WriteStartElement("pattFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("prst", strPreset);
		if (!isAutoFore)
		{
			writer.WriteStartElement("fgClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeRgbColor(writer, foreColor.GetRGB(book), Alphavalue);
			writer.WriteEndElement();
		}
		if (!isAutoBack)
		{
			writer.WriteStartElement("bgClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeRgbColor(writer, backColor.GetRGB(book), Alphavalue);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	public static void SerializePatternFill(XmlWriter writer, ChartColor foreColor, bool isAutoFore, ChartColor backColor, bool isAutoBack, OfficeGradientPattern pattern, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("pattFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		Excel2007GradientPattern excel2007GradientPattern = (Excel2007GradientPattern)pattern;
		writer.WriteAttributeString("prst", excel2007GradientPattern.ToString());
		if (!isAutoFore)
		{
			writer.WriteStartElement("fgClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeRgbColor(writer, foreColor.GetRGB(book));
			writer.WriteEndElement();
		}
		if (!isAutoBack)
		{
			writer.WriteStartElement("bgClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeRgbColor(writer, backColor.GetRGB(book));
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	public static void SerializeSolidFill(XmlWriter writer, ChartColor color, bool isAutoColor, IWorkbook book, double alphavalue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (!isAutoColor)
		{
			SerializeRgbColor(writer, color.GetRGB(book), alphavalue);
		}
		writer.WriteEndElement();
	}

	public static void SerializeRgbColor(XmlWriter writer, Color color)
	{
		SerializeRgbColor(writer, color, -1.0);
	}

	public static void SerializeRgbColor(XmlWriter writer, OfficeKnownColors colorIndex, IWorkbook book)
	{
		SerializeRgbColor(writer, book.GetPaletteColor(colorIndex), -1.0);
	}

	public static void SerializeRgbColor(XmlWriter writer, Color color, double alphaValue)
	{
		int alpha = Convert.ToInt32(alphaValue * 100000.0);
		SerializeRgbColor(writer, color, alpha, -1, -1);
	}

	public static void SerializeRgbColor(XmlWriter writer, Color color, int alpha, int tint, int shade)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int num = 100000;
		writer.WriteStartElement("srgbClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", (color.ToArgb() & 0xFFFFFF).ToString("X6"));
		if (alpha != 100000 && alpha >= 0 && alpha <= num)
		{
			writer.WriteStartElement("alpha", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", alpha.ToString());
			writer.WriteEndElement();
		}
		if (shade >= 0)
		{
			writer.WriteElementString("gamma", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
			writer.WriteStartElement("shade", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", shade.ToString());
			writer.WriteEndElement();
			writer.WriteElementString("invGamma", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
		else if (tint >= 0)
		{
			writer.WriteElementString("gamma", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
			writer.WriteStartElement("tint", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", tint.ToString());
			writer.WriteEndElement();
			writer.WriteElementString("invGamma", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
		writer.WriteEndElement();
	}

	private static void SerializeLineProperties(XmlWriter writer, IOfficeChartBorder border, bool bRoundCorners, IWorkbook book, bool serializeAutoFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (border == null)
		{
			return;
		}
		ChartBorderImpl chartBorderImpl = border as ChartBorderImpl;
		writer.WriteStartElement("ln", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (!chartBorderImpl.AutoFormat || serializeAutoFormat)
		{
			if (chartBorderImpl.LineWeightString != null)
			{
				writer.WriteAttributeString("w", chartBorderImpl.LineWeightString);
			}
			else
			{
				int num = (short)border.LineWeight;
				if (num != -1)
				{
					writer.WriteAttributeString("w", (((short)border.LineWeight + 1) * 12700).ToString());
				}
				else if (!chartBorderImpl.HasLineProperties)
				{
					writer.WriteAttributeString("w", "3175");
				}
			}
			if (chartBorderImpl.LineStyle != 0 && chartBorderImpl.LineStyle != Excel2007ShapeLineStyle.sng)
			{
				writer.WriteAttributeString("cmpd", Enum.GetName(typeof(Excel2007ShapeLineStyle), chartBorderImpl.LineStyle).ToLower());
			}
			string capStyle = GetCapStyle(chartBorderImpl.CapStyle);
			if (capStyle != "")
			{
				writer.WriteAttributeString("cap", capStyle);
			}
			OfficeChartLinePattern linePattern = border.LinePattern;
			if (((ChartBorderImpl)border).HasLineProperties || ((WorkbookImpl)book).IsCreated || ((WorkbookImpl)book).IsConverted)
			{
				if (chartBorderImpl.HasGradientFill)
				{
					SerializeGradientFill(writer, chartBorderImpl.Fill, book);
				}
				else
				{
					switch (linePattern)
					{
					case OfficeChartLinePattern.None:
						writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
						break;
					case OfficeChartLinePattern.Solid:
						SerializeSolidFill(writer, border.LineColor, border.IsAutoLineColor, book, 1.0 - border.Transparency);
						writer.WriteStartElement("prstDash", "http://schemas.openxmlformats.org/drawingml/2006/main");
						writer.WriteAttributeString("val", "solid");
						writer.WriteEndElement();
						break;
					default:
					{
						KeyValuePair<string, string> keyValuePair = s_dicLinePatterns[linePattern];
						string key = keyValuePair.Key;
						string value = keyValuePair.Value;
						SerializePatternFill(writer, border.LineColor, border.IsAutoLineColor, key, value, book, 1.0 - border.Transparency);
						break;
					}
					}
				}
			}
			SerializeJoinType(writer, chartBorderImpl.JoinType);
			if (chartBorderImpl.BeginArrowType != OfficeArrowType.None)
			{
				SerializeArrowProperties(writer, chartBorderImpl, isHead: true);
			}
			if (chartBorderImpl.EndArrowType != OfficeArrowType.None)
			{
				SerializeArrowProperties(writer, chartBorderImpl, isHead: false);
			}
		}
		writer.WriteEndElement();
	}

	private static string GetCapStyle(LineCap lineCap)
	{
		return lineCap switch
		{
			LineCap.Square => "sq", 
			LineCap.Round => "rnd", 
			LineCap.Flat => "flat", 
			_ => "", 
		};
	}

	private static void SerializeArrowProperties(XmlWriter writer, IOfficeChartBorder border, bool isHead)
	{
		string text = null;
		string text2 = "";
		string text3 = "";
		text = (isHead ? "headEnd" : "tailEnd");
		OfficeArrowType style = (isHead ? border.BeginArrowType : border.EndArrowType);
		text2 = (isHead ? (border as ChartBorderImpl).m_beginArrowwidth : (border as ChartBorderImpl).m_endArrowWidth);
		text3 = (isHead ? (border as ChartBorderImpl).m_beginArrowLg : (border as ChartBorderImpl).m_beginArrowLg);
		writer.WriteStartElement(text, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("type", GetArrowStyle(style));
		if (text2 != null)
		{
			writer.WriteAttributeString("w", text2);
		}
		if (text3 != null)
		{
			writer.WriteAttributeString("len", text3);
		}
		writer.WriteEndElement();
	}

	private static string GetArrowStyle(OfficeArrowType style)
	{
		return style switch
		{
			OfficeArrowType.None => "none", 
			OfficeArrowType.Arrow => "triangle", 
			OfficeArrowType.StealthArrow => "stealth", 
			OfficeArrowType.DiamondArrow => "diamond", 
			OfficeArrowType.OvalArrow => "oval", 
			OfficeArrowType.OpenArrow => "arrow", 
			_ => "none", 
		};
	}

	private static string GetArrowWidth(OfficeShapeArrowWidth Width)
	{
		return Width switch
		{
			OfficeShapeArrowWidth.ArrowHeadNarrow => "sm", 
			OfficeShapeArrowWidth.ArrowHeadMedium => "med", 
			OfficeShapeArrowWidth.ArrowHeadWide => "lg", 
			_ => "med", 
		};
	}

	private static string GetArrowLength(OfficeShapeArrowLength Length)
	{
		return Length switch
		{
			OfficeShapeArrowLength.ArrowHeadShort => "sm", 
			OfficeShapeArrowLength.ArrowHeadMedium => "med", 
			OfficeShapeArrowLength.ArrowHeadLong => "lg", 
			_ => "med", 
		};
	}

	private static void SerializeJoinType(XmlWriter writer, Excel2007BorderJoinType joinType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		string text = null;
		switch (joinType)
		{
		case Excel2007BorderJoinType.Bevel:
			text = "bevel";
			break;
		case Excel2007BorderJoinType.Mitter:
			text = "miter";
			break;
		case Excel2007BorderJoinType.Round:
			text = "round";
			break;
		}
		if (text != null)
		{
			writer.WriteElementString(text, "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
	}

	[SecurityCritical]
	internal static Stream GetImageStream(Image image)
	{
		ImageFormat rawFormat = image.RawFormat;
		MemoryStream memoryStream;
		if (rawFormat.Equals(ImageFormat.Emf) || rawFormat.Equals(ImageFormat.Wmf))
		{
			new MemoryStream();
			memoryStream = MsoMetafilePicture.SerializeMetafile(image);
		}
		else
		{
			memoryStream = new MemoryStream();
			image.Save(memoryStream, rawFormat);
		}
		return memoryStream;
	}

	[SecurityCritical]
	private static void SerializePictureFill(XmlWriter writer, Image image, FileDataHolder holder, RelationCollection relations, bool tile, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		string text = relations.GenerateRelationId();
		if (chart != null)
		{
			if (!chart.RelationPreservedStreamCollection.ContainsKey(text))
			{
				chart.RelationPreservedStreamCollection.Add(text, GetImageStream(image));
			}
			relations[text] = new Relation("", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		}
		writer.WriteStartElement("blipFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("blip", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		writer.WriteEndElement();
		if (tile)
		{
			writer.WriteStartElement("tile", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("tx", "0");
			writer.WriteAttributeString("ty", "0");
			writer.WriteAttributeString("sx", "100000");
			writer.WriteAttributeString("sy", "100000");
			writer.WriteAttributeString("flip", "none");
			writer.WriteAttributeString("algn", "tl");
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteStartElement("stretch", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteElementString("fillRect", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private static void SerializePictureFill(XmlWriter writer, Image image, FileDataHolder holder, RelationCollection relations, IInternalFill fill, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		string text = relations.GenerateRelationId();
		if (chart != null)
		{
			if (!chart.RelationPreservedStreamCollection.ContainsKey(text))
			{
				chart.RelationPreservedStreamCollection.Add(text, GetImageStream(image));
			}
			relations[text] = new Relation("", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		}
		writer.WriteStartElement("blipFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("blip", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		if (fill.TransparencyColor != 0f)
		{
			writer.WriteStartElement("alphaModFix", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("amt", ((int)Math.Round((1f - fill.TransparencyColor) * 100000f)).ToString());
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		if (fill.Tile)
		{
			writer.WriteStartElement("tile", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("tx", (Math.Round(fill.TextureOffsetX) * 12700.0).ToString());
			writer.WriteAttributeString("ty", (Math.Round(fill.TextureOffsetY) * 12700.0).ToString());
			writer.WriteAttributeString("sx", (Math.Round(fill.TextureHorizontalScale) * 100000.0).ToString());
			writer.WriteAttributeString("sy", (Math.Round(fill.TextureVerticalScale) * 100000.0).ToString());
			writer.WriteAttributeString("flip", fill.TileFlipping);
			writer.WriteAttributeString("algn", fill.Alignment);
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteStartElement("srcRect", "http://schemas.openxmlformats.org/drawingml/2006/main");
			Rectangle sourceRect = (fill as ShapeFillImpl).SourceRect;
			if (sourceRect.Right != 0)
			{
				writer.WriteAttributeString("r", sourceRect.Right.ToString());
			}
			if (sourceRect.Bottom != 0)
			{
				writer.WriteAttributeString("b", sourceRect.Bottom.ToString());
			}
			if (sourceRect.Left != 0)
			{
				writer.WriteAttributeString("l", sourceRect.Left.ToString());
			}
			if (sourceRect.Top != 0)
			{
				writer.WriteAttributeString("t", sourceRect.Top.ToString());
			}
			writer.WriteEndElement();
			writer.WriteStartElement("stretch", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteStartElement("fillRect", "http://schemas.openxmlformats.org/drawingml/2006/main");
			Rectangle fillRect = (fill as ShapeFillImpl).FillRect;
			if (fillRect.Right != 0)
			{
				writer.WriteAttributeString("r", fillRect.Right.ToString());
			}
			if (fillRect.Bottom != 0)
			{
				writer.WriteAttributeString("b", fillRect.Bottom.ToString());
			}
			if (fillRect.Left != 0)
			{
				writer.WriteAttributeString("l", fillRect.Left.ToString());
			}
			if (fillRect.Top != 0)
			{
				writer.WriteAttributeString("t", fillRect.Top.ToString());
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private static void SerializeTextureFill(XmlWriter writer, IOfficeFill fill, FileDataHolder holder, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		OfficeTexture texture = fill.Texture;
		Image image;
		if (texture != OfficeTexture.User_Defined)
		{
			int num = (int)texture;
			byte[] resData = ShapeFillImpl.GetResData("Text" + num);
			byte[] array = new byte[resData.Length - 25];
			Array.Copy(resData, 25, array, 0, array.Length);
			MemoryStream memoryStream = new MemoryStream();
			ShapeFillImpl.UpdateBitMapHederToStream(memoryStream, resData);
			memoryStream.Write(array, 0, array.Length);
			image = ApplicationImpl.CreateImage(memoryStream);
		}
		else
		{
			image = fill.Picture;
		}
		CommonObject commonObject = fill as CommonObject;
		ChartImpl chart = null;
		if (commonObject != null)
		{
			chart = commonObject.FindParent(typeof(ChartImpl)) as ChartImpl;
		}
		SerializePictureFill(writer, image, holder, relations, (fill as IInternalFill).Tile, chart);
	}

	private static void SerializeGradientFill(XmlWriter writer, IOfficeFill fill, IWorkbook book)
	{
		ShapeFillImpl shapeFillImpl = (ShapeFillImpl)fill;
		GradientStops gradientStops = shapeFillImpl.GradientStops;
		GradientStops preservedGradient = shapeFillImpl.PreservedGradient;
		GradientSerializator gradientSerializator = new GradientSerializator();
		if (preservedGradient != null)
		{
			gradientStops = preservedGradient;
			_ = gradientStops.TileRect;
			gradientStops.TileRect = shapeFillImpl.PreservedGradient.TileRect;
		}
		gradientSerializator.Serialize(writer, gradientStops, book);
	}

	private static bool HasSchemaColor(GradientStops stops)
	{
		for (int i = 0; i < stops.Count; i++)
		{
			if (stops[i].ColorObject.IsSchemeColor)
			{
				return true;
			}
		}
		return false;
	}

	private void SerializeTextProperties(XmlWriter writer, IOfficeChartTextArea textArea)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	internal static void SerializeDefaultTextFormatting(XmlWriter writer, IOfficeFont textFormatting, IWorkbook book, double defaultFontSize, bool isAutoTextRotation, int rotationAngle, Excel2007TextRotation textRotation, string nameSpace, bool isChartExText, bool isEndParagraph)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (textFormatting != null)
		{
			writer.WriteStartElement("txPr", nameSpace);
			writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if (!isAutoTextRotation && nameSpace == "http://schemas.microsoft.com/office/drawing/2014/chartex")
			{
				writer.WriteAttributeString("rot", (rotationAngle * 60000).ToString());
				writer.WriteAttributeString("vert", textRotation.ToString());
			}
			writer.WriteEndElement();
			writer.WriteStartElement("lstStyle", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteEndElement();
			writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeParagraphRunProperites(writer, textFormatting, "defRPr", book, defaultFontSize);
			writer.WriteEndElement();
			if (isChartExText)
			{
				ChartTextAreaImpl chartTextAreaImpl = textFormatting as ChartTextAreaImpl;
				writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
				SerializeParagraphRunProperites(writer, chartTextAreaImpl, "rPr", book, defaultFontSize);
				writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
				writer.WriteString(chartTextAreaImpl.Text);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			if (isEndParagraph)
			{
				SerializeParagraphRunProperites(writer, textFormatting, "endParaRPr", book, defaultFontSize);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	public static void SerializeTextAreaText(XmlWriter writer, IOfficeChartTextArea textArea, IWorkbook book, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("tx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		bool flag = false;
		string[] strCache = null;
		if (textArea is ChartTextAreaImpl)
		{
			ChartTextAreaImpl obj = textArea as ChartTextAreaImpl;
			strCache = obj.StringCache;
			flag = obj.IsFormula;
		}
		else if (textArea is ChartDataLabelsImpl)
		{
			ChartDataLabelsImpl obj2 = textArea as ChartDataLabelsImpl;
			strCache = obj2.StringCache;
			flag = obj2.IsFormula;
		}
		if (flag)
		{
			SerializeStringReference(writer, textArea, strCache);
		}
		else
		{
			SerializeRichText(writer, textArea, book, "rich", defaultFontSize);
		}
		writer.WriteEndElement();
	}

	public static void SerializeRichText(XmlWriter writer, IOfficeChartTextArea textArea, IWorkbook book, string tagName, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		bool flag = false;
		if (textArea is ChartTextAreaImpl)
		{
			flag = ChartImpl.IsChartExSerieType(((ChartImpl)(textArea as ChartTextAreaImpl).FindParent(typeof(ChartImpl))).ChartType);
		}
		if (flag)
		{
			writer.WriteStartElement(tagName, "http://schemas.microsoft.com/office/drawing/2014/chartex");
		}
		else
		{
			writer.WriteStartElement(tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		SerializeBodyProperties(writer, textArea);
		SerializeListStyles(writer, textArea);
		SerializeParagraphs(writer, textArea, book, defaultFontSize);
		writer.WriteEndElement();
	}

	private static void SerializeBodyProperties(XmlWriter writer, IOfficeChartTextArea textArea)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		IInternalOfficeChartTextArea internalOfficeChartTextArea = textArea as IInternalOfficeChartTextArea;
		if (internalOfficeChartTextArea.HasTextRotation)
		{
			writer.WriteAttributeString("rot", (internalOfficeChartTextArea.TextRotationAngle * 60000).ToString());
			if (internalOfficeChartTextArea is ChartTextAreaImpl)
			{
				writer.WriteAttributeString("vert", (internalOfficeChartTextArea as ChartTextAreaImpl).TextRotation.ToString());
			}
			else if (textArea is ChartDataLabelsImpl)
			{
				writer.WriteAttributeString("vert", (textArea as ChartDataLabelsImpl).TextRotation.ToString());
			}
		}
		writer.WriteEndElement();
	}

	private static void SerializeListStyles(XmlWriter writer, IOfficeChartTextArea textArea)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("lstStyle", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
	}

	private static void SerializeParagraphs(XmlWriter writer, IOfficeChartTextArea textArea, IWorkbook book, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).ChartAlRuns != null && (textArea as ChartTextAreaImpl).ChartAlRuns.Runs != null && (textArea as ChartTextAreaImpl).ChartAlRuns.Runs.Length != 0)
		{
			Serialize_TextArea_RichTextParagraph(writer, textArea, book, defaultFontSize);
			return;
		}
		if (textArea is ChartDataLabelsImpl && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns != null && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns.Runs != null && (textArea as ChartDataLabelsImpl).TextArea.ChartAlRuns.Runs.Length != 0)
		{
			Serialize_DataLabel_RichTextParagraph(writer, textArea, book, defaultFontSize);
			return;
		}
		string[] array = textArea.Text.Split('\n');
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			SerializeSingleParagraph(writer, textArea, array[i], book, defaultFontSize);
		}
	}

	private static void Serialize_TextArea_RichTextParagraph(XmlWriter writer, IOfficeChartTextArea textArea, IWorkbook book, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		int startRunProperty = 0;
		if ((textArea as ChartTextAreaImpl).DefaultParagarphProperties.Count > 1)
		{
			foreach (IInternalOfficeChartTextArea defaultParagarphProperty in (textArea as ChartTextAreaImpl).DefaultParagarphProperties)
			{
				Serialize_TextArea_RichTextSeparateParagraph(writer, textArea, book, defaultFontSize, defaultParagarphProperty, startRunProperty);
				startRunProperty = (defaultParagarphProperty as ChartTextAreaImpl).ChartAlRuns.Runs.Length;
			}
			return;
		}
		Serialize_TextArea_RichTextSeparateParagraph(writer, textArea, book, defaultFontSize, null, startRunProperty);
	}

	private static void Serialize_TextArea_RichTextSeparateParagraph(XmlWriter writer, IOfficeChartTextArea textArea, IWorkbook book, double defaultFontSize, IInternalOfficeChartTextArea defaultParagaphProperties, int startRunProperty)
	{
		int num = 0;
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (defaultParagaphProperties != null)
		{
			SerializeParagraphRunProperites(writer, defaultParagaphProperties, "defRPr", book, defaultFontSize);
			num = (defaultParagaphProperties as ChartTextAreaImpl).ChartAlRuns.Runs.Length;
		}
		else
		{
			SerializeParagraphRunProperites(writer, textArea, "defRPr", book, defaultFontSize);
			num = (textArea as ChartTextAreaImpl).ChartAlRuns.Runs.Length;
		}
		writer.WriteEndElement();
		for (int i = startRunProperty; i < num; i++)
		{
			writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
			int num2 = 0;
			int num3 = 0;
			num2 = (textArea as ChartTextAreaImpl).ChartAlRuns.Runs[i].FirstCharIndex;
			num3 = ((i >= (textArea as ChartTextAreaImpl).ChartAlRuns.Runs.Length - 1) ? (textArea.Text.Length - (textArea as ChartTextAreaImpl).ChartAlRuns.Runs[i].FirstCharIndex) : ((textArea as ChartTextAreaImpl).ChartAlRuns.Runs[i + 1].FirstCharIndex - num2));
			if ((textArea as ChartTextAreaImpl).ChartAlRuns.Runs[i].HasNewParagarphStart)
			{
				num3--;
			}
			string text = null;
			text = textArea.Text.Substring(num2, num3);
			if (i == 0 && (textArea as ChartTextAreaImpl).ChartAlRuns.Runs.Length < 2)
			{
				int index = (textArea as IInternalFont).Index;
				(textArea as ChartTextAreaImpl).SetFontIndex(index);
			}
			else
			{
				(textArea as ChartTextAreaImpl).SetFontIndex((textArea as ChartTextAreaImpl).ChartAlRuns.Runs[i].FontIndex);
			}
			SerializeParagraphRunProperites(writer, textArea, "rPr", book, defaultFontSize);
			writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteString(text);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private static void Serialize_DataLabel_RichTextParagraph(XmlWriter writer, IOfficeChartTextArea chartTextArea, IWorkbook book, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chartTextArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		ChartDataLabelsImpl chartDataLabelsImpl = chartTextArea as ChartDataLabelsImpl;
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("defRPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
		writer.WriteEndElement();
		ChartTextAreaImpl textArea = (chartTextArea as ChartDataLabelsImpl).TextArea;
		int num = textArea.ChartAlRuns.Runs.Length;
		bool flag = false;
		string fldElementType = null;
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			int num3 = 0;
			num2 = textArea.ChartAlRuns.Runs[i].FirstCharIndex;
			num3 = ((i >= num - 1) ? (textArea.Text.Length - textArea.ChartAlRuns.Runs[i].FirstCharIndex) : (textArea.ChartAlRuns.Runs[i + 1].FirstCharIndex - num2));
			string text = null;
			text = textArea.Text.Substring(num2, num3);
			if (!string.IsNullOrEmpty(text))
			{
				flag = CheckSerializeFldElement(chartDataLabelsImpl, text, out fldElementType);
			}
			if (flag)
			{
				writer.WriteStartElement("fld", "http://schemas.openxmlformats.org/drawingml/2006/main");
				writer.WriteAttributeString("id", "{C1C5B820-BA97-46F5-89C0-B4B82AE36AEC}");
				writer.WriteAttributeString("type", fldElementType);
			}
			else
			{
				writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
			}
			if (num > 1)
			{
				textArea.SetFontIndex(textArea.ChartAlRuns.Runs[i].FontIndex);
			}
			SerializeParagraphRunProperites(writer, textArea, "rPr", book, defaultFontSize);
			writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteString(text);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private static bool CheckSerializeFldElement(ChartDataLabelsImpl chartDataLabelsImpl, string text, out string fldElementType)
	{
		fldElementType = null;
		if (chartDataLabelsImpl.IsValueFromCells && text.Equals("[CELLRANGE]"))
		{
			fldElementType = "CELLRANGE";
			return true;
		}
		if (chartDataLabelsImpl.IsSeriesName && text.Equals("[SERIES NAME]"))
		{
			fldElementType = "SERIESNAME";
			return true;
		}
		if (chartDataLabelsImpl.IsCategoryName && text.Equals("[CATEGORY NAME]"))
		{
			fldElementType = "CATEGORYNAME";
			return true;
		}
		if (chartDataLabelsImpl.IsCategoryName && text.Equals("[X VALUE]"))
		{
			fldElementType = "XVALUE";
			return true;
		}
		if (chartDataLabelsImpl.IsValue && text.Equals("[VALUE]"))
		{
			fldElementType = "VALUE";
			return true;
		}
		if (chartDataLabelsImpl.IsValue && text.Equals("[Y VALUE]"))
		{
			fldElementType = "YVALUE";
			return true;
		}
		if (chartDataLabelsImpl.IsPercentage && text.Equals("[PERCENTAGE]"))
		{
			fldElementType = "PERCENTAGE";
			return true;
		}
		return false;
	}

	private static void SerializeSingleParagraph(XmlWriter writer, IOfficeChartTextArea textArea, string paragraphText, IWorkbook book, double defaultFontSize)
	{
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("defRPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (textArea.Size != defaultFontSize || (textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).ShowSizeProperties))
		{
			writer.WriteAttributeString("sz", ((int)(textArea.Size * 100.0)).ToString());
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
		SerializeParagraphRunProperites(writer, textArea, "rPr", book, defaultFontSize);
		writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteString(paragraphText);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void SerializeParagraphRunProperites(XmlWriter writer, IOfficeFont textArea, string mainTagName, IWorkbook book, double defaultFontSize)
	{
		SerializeParagraphRunProperites(writer, textArea, mainTagName, book, defaultFontSize, isCommonFontSize: false);
	}

	internal static void SerializeParagraphRunProperites(XmlWriter writer, IOfficeFont textArea, string mainTagName, IWorkbook book, double defaultFontSize, bool isCommonFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (mainTagName == null || mainTagName.Length == 0)
		{
			throw new ArgumentException("mainTagName");
		}
		writer.WriteStartElement(mainTagName, "http://schemas.openxmlformats.org/drawingml/2006/main");
		string value = (textArea.Bold ? "1" : "0");
		string value2 = (textArea.Italic ? "1" : "0");
		IInternalFont internalFont = textArea as IInternalFont;
		if (internalFont != null)
		{
			string language = internalFont.Font.Language;
			if (language != null)
			{
				writer.WriteAttributeString("lang", language);
			}
		}
		if (textArea.Bold || (internalFont.Font.m_textSettings != null && internalFont.Font.m_textSettings.Bold.HasValue) || (internalFont is ChartDataLabelsImpl && (internalFont as ChartDataLabelsImpl).ShowBoldProperties) || (internalFont is ChartTextAreaImpl && (internalFont as ChartTextAreaImpl).ShowBoldProperties))
		{
			writer.WriteAttributeString("b", value);
		}
		if (textArea.Italic || (internalFont.Font.m_textSettings != null && internalFont.Font.m_textSettings.Italic.HasValue) || (internalFont is ChartDataLabelsImpl && (internalFont as ChartDataLabelsImpl).ShowItalicProperties) || (internalFont is ChartTextAreaImpl && (internalFont as ChartTextAreaImpl).ShowItalicProperties))
		{
			writer.WriteAttributeString("i", value2);
		}
		if (textArea.Strikethrough)
		{
			writer.WriteAttributeString("strike", "sngStrike");
		}
		if (textArea.Size != defaultFontSize || (internalFont.Font.m_textSettings != null && internalFont.Font.m_textSettings.ShowSizeProperties == true) || (internalFont is ChartDataLabelsImpl && (internalFont as ChartDataLabelsImpl).ShowSizeProperties) || (internalFont is ChartTextAreaImpl && (internalFont as ChartTextAreaImpl).ShowSizeProperties))
		{
			writer.WriteAttributeString("sz", ((int)(textArea.Size * 100.0)).ToString());
		}
		else if (isCommonFontSize)
		{
			writer.WriteAttributeString("sz", ((int)(textArea.Size * 100.0)).ToString());
		}
		if (textArea.Underline != 0)
		{
			string value3 = Helper.ToString(textArea.Underline);
			writer.WriteAttributeString("u", value3);
		}
		int num = 0;
		string text = null;
		text = ((!textArea.Superscript && !textArea.Subscript) ? num.ToString() : ((textArea is ChartTextAreaImpl && (textArea as ChartTextAreaImpl).Font != null) ? ((!(textArea as ChartTextAreaImpl).IsBaselineWithPercentage) ? (textArea as ChartTextAreaImpl).Font.BaseLine.ToString() : ((textArea as ChartTextAreaImpl).Font.BaseLine * 100 + "%")) : ((!(textArea is FontWrapper)) ? num.ToString() : (textArea as FontWrapper).Baseline.ToString())));
		writer.WriteAttributeString("baseline", text);
		if (textArea is ChartTextAreaImpl)
		{
			if ((textArea as ChartTextAreaImpl).IsCapitalize)
			{
				writer.WriteAttributeString("cap", "all");
			}
			if ((textArea as ChartTextAreaImpl).CharacterSpacingValue != 0.0)
			{
				writer.WriteAttributeString("spc", (Math.Round((textArea as ChartTextAreaImpl).CharacterSpacingValue, 1) * 100.0).ToString());
			}
			if ((textArea as ChartTextAreaImpl).KerningValue != 0.0)
			{
				writer.WriteAttributeString("kern", ((textArea as ChartTextAreaImpl).KerningValue * 100.0).ToString());
			}
		}
		else if (textArea is FontWrapper)
		{
			if ((textArea as FontWrapper).IsCapitalize)
			{
				writer.WriteAttributeString("cap", "all");
			}
			if ((textArea as FontWrapper).CharacterSpacingValue != 0.0)
			{
				writer.WriteAttributeString("spc", (Math.Round((textArea as FontWrapper).CharacterSpacingValue, 1) * 100.0).ToString());
			}
			if ((textArea as FontWrapper).KerningValue != 0.0)
			{
				writer.WriteAttributeString("kern", ((textArea as FontWrapper).KerningValue * 100.0).ToString());
			}
		}
		if (!textArea.IsAutoColor || (textArea is FontWrapper && (book as WorkbookImpl).IsConverted))
		{
			writer.WriteStartElement("solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeRgbColor(writer, textArea.RGBColor, -100000, -1, -1);
			writer.WriteEndElement();
		}
		if (textArea is ChartDataLabelsImpl && (textArea as ChartDataLabelsImpl).IsHighlightColor)
		{
			CommonObject commonObject = textArea as CommonObject;
			ChartImpl chart = null;
			if (commonObject != null)
			{
				chart = commonObject.FindParent(typeof(ChartImpl)) as ChartImpl;
			}
			SerializeHighlightTag(writer, chart);
		}
		if (textArea is FontWrapper)
		{
			FontImpl font = (textArea as FontWrapper).Font;
			if (font.PreservedElements != null)
			{
				if (font.PreservedElements.ContainsKey("ln"))
				{
					WriteRawData(writer, font.PreservedElements, "ln");
				}
				if (font.PreservedElements.ContainsKey("gradFill"))
				{
					WriteRawData(writer, font.PreservedElements, "gradFill");
				}
				if (font.PreservedElements.ContainsKey("effectLst"))
				{
					WriteRawData(writer, font.PreservedElements, "effectLst");
				}
			}
		}
		if ((textArea.FontName != "Calibri" && !(textArea is ChartDataLabelsImpl) && !(textArea.Parent is ChartDataLabelsImpl)) || (textArea is ChartDataLabelsImpl && ((textArea as ChartDataLabelsImpl).Font.HasLatin || (textArea as ChartDataLabelsImpl).IsFontChanged)) || (textArea.Parent is ChartDataLabelsImpl && ((textArea.Parent as ChartDataLabelsImpl).Font.HasLatin || (textArea.Parent as ChartDataLabelsImpl).IsFontChanged)))
		{
			writer.WriteStartElement("latin", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("typeface", textArea.FontName);
			writer.WriteEndElement();
			writer.WriteStartElement("ea", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("typeface", textArea.FontName);
			writer.WriteEndElement();
			writer.WriteStartElement("cs", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("typeface", textArea.FontName);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private static void SerializeHighlightTag(XmlWriter writer, ChartImpl chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Stream highlightStream = chart.HighlightStream;
		if (highlightStream != null)
		{
			highlightStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, highlightStream);
		}
	}

	internal static void WriteRawData(XmlWriter xmlWriter, Dictionary<string, Stream> preservedStream, string elementName)
	{
		if (!preservedStream.ContainsKey(elementName))
		{
			return;
		}
		using Stream stream = preservedStream[elementName];
		if (stream == null || stream.Length <= 0)
		{
			return;
		}
		stream.Position = 0L;
		using XmlReader xmlReader = XmlReader.Create(stream);
		while (xmlReader.LocalName != elementName)
		{
			xmlReader.Read();
		}
		xmlWriter.WriteNode(xmlReader, defattr: false);
	}

	private static void SerializeStringReference(XmlWriter writer, IOfficeChartTextArea textArea, string[] strCache)
	{
		string text = textArea.Text;
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (text[0] == '=')
		{
			text = UtilityMethods.RemoveFirstCharUnsafe(text);
		}
		writer.WriteStartElement("strRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", text);
		if (strCache != null && strCache.Length != 0)
		{
			writer.WriteStartElement("strCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			SerializeValueTag(writer, "ptCount", strCache.Length.ToString());
			for (int i = 0; i < strCache.Length; i++)
			{
				writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteAttributeString("idx", i.ToString());
				writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", strCache[i]);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteElementString("strCache", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		}
		writer.WriteEndElement();
	}

	public static void SerializeLayout(XmlWriter writer, object textArea)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		IOfficeChartLayout officeChartLayout = null;
		if (textArea is ChartTextAreaImpl)
		{
			officeChartLayout = (textArea as ChartTextAreaImpl).Layout;
		}
		else if (textArea is ChartPlotAreaImpl)
		{
			officeChartLayout = (textArea as ChartPlotAreaImpl).Layout;
		}
		else if (textArea is ChartDataLabelsImpl)
		{
			officeChartLayout = (textArea as ChartDataLabelsImpl).Layout;
		}
		else if (textArea is ChartLegendImpl)
		{
			officeChartLayout = (textArea as ChartLegendImpl).Layout;
		}
		if (officeChartLayout != null)
		{
			writer.WriteStartElement("layout", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (officeChartLayout.ManualLayout != null && !IsAutoManualLayout(officeChartLayout.ManualLayout))
			{
				SerializeManualLayout(writer, officeChartLayout.ManualLayout);
			}
			writer.WriteEndElement();
		}
	}

	private static bool IsAutoManualLayout(IOfficeChartManualLayout manualLayout)
	{
		if (manualLayout == null)
		{
			throw new ArgumentNullException("manualLayout");
		}
		if (manualLayout != null)
		{
			_ = manualLayout.LayoutTarget;
			if (manualLayout.LayoutTarget == LayoutTargets.auto)
			{
				_ = manualLayout.LeftMode;
				if (manualLayout.LeftMode == LayoutModes.auto)
				{
					_ = manualLayout.TopMode;
					if (manualLayout.TopMode == LayoutModes.auto)
					{
						_ = manualLayout.WidthMode;
						if (manualLayout.WidthMode == LayoutModes.auto)
						{
							_ = manualLayout.HeightMode;
							if (manualLayout.HeightMode == LayoutModes.auto)
							{
								_ = manualLayout.Left;
								_ = manualLayout.Top;
								if (manualLayout.Left == 0.0 && manualLayout.Top == 0.0)
								{
									_ = manualLayout.Width;
									_ = manualLayout.Height;
									if (manualLayout.Width == 0.0 && manualLayout.Height == 0.0)
									{
										goto IL_00c7;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}
		goto IL_00c7;
		IL_00c7:
		return true;
	}

	public static void SerializeManualLayout(XmlWriter writer, IOfficeChartManualLayout manualLayout)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (manualLayout == null)
		{
			throw new ArgumentNullException("manualLayout");
		}
		if (manualLayout != null)
		{
			writer.WriteStartElement("manualLayout", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			_ = manualLayout.LayoutTarget;
			if (manualLayout.LayoutTarget != 0)
			{
				SerializeValueTag(writer, "layoutTarget", manualLayout.LayoutTarget.ToString());
			}
			_ = manualLayout.LeftMode;
			if (manualLayout.LeftMode != 0)
			{
				SerializeValueTag(writer, "xMode", manualLayout.LeftMode.ToString());
			}
			_ = manualLayout.TopMode;
			if (manualLayout.TopMode != 0)
			{
				SerializeValueTag(writer, "yMode", manualLayout.TopMode.ToString());
			}
			_ = manualLayout.WidthMode;
			if (manualLayout.WidthMode != 0)
			{
				SerializeValueTag(writer, "wMode", manualLayout.WidthMode.ToString());
			}
			_ = manualLayout.HeightMode;
			if (manualLayout.HeightMode != 0)
			{
				SerializeValueTag(writer, "hMode", manualLayout.HeightMode.ToString());
			}
			_ = manualLayout.Left;
			_ = manualLayout.Top;
			if (manualLayout.Left != 0.0 || manualLayout.Top != 0.0)
			{
				SerializeDoubleValueTag(writer, "x", manualLayout.Left);
				SerializeDoubleValueTag(writer, "y", manualLayout.Top);
			}
			_ = manualLayout.Width;
			_ = manualLayout.Height;
			if (manualLayout.Width != 0.0 || manualLayout.Height != 0.0)
			{
				SerializeDoubleValueTag(writer, "w", manualLayout.Width);
				SerializeDoubleValueTag(writer, "h", manualLayout.Height);
			}
			writer.WriteEndElement();
		}
	}

	internal static void SerializeBottomBevel(XmlWriter writer, ThreeDFormatImpl threeDFormatImpl)
	{
		writer.WriteStartElement("bevelB", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (threeDFormatImpl.IsBevelBottomHeightSet)
		{
			writer.WriteAttributeString("h", (threeDFormatImpl.BevelBottomHeight * 12700).ToString());
		}
		if (threeDFormatImpl.IsBevelBottomWidthSet)
		{
			writer.WriteAttributeString("w", (threeDFormatImpl.BevelBottomWidth * 12700).ToString());
		}
		writer.WriteEndElement();
	}

	internal static void SerializeTopBevel(XmlWriter writer, ThreeDFormatImpl threeDFormatImpl, string presetShape)
	{
		writer.WriteStartElement("bevelT", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (threeDFormatImpl.IsBevelTopHeightSet)
		{
			writer.WriteAttributeString("h", (threeDFormatImpl.BevelTopHeight * 12700).ToString());
		}
		if (threeDFormatImpl.IsBevelTopWidthSet)
		{
			writer.WriteAttributeString("w", (threeDFormatImpl.BevelTopWidth * 12700).ToString());
		}
		if (presetShape != null)
		{
			writer.WriteAttributeString("prst", presetShape);
		}
		writer.WriteEndElement();
	}
}

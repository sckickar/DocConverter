using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal static class Helper
{
	internal static Dictionary<string, int> columnAttributes;

	internal static string GetPlacementType(PlacementType placementType)
	{
		return placementType switch
		{
			PlacementType.FreeFloating => "absolute", 
			PlacementType.Move => "oneCell", 
			PlacementType.MoveAndSize => "twoCell", 
			_ => throw new ArgumentException("Invalid PlacementType val"), 
		};
	}

	internal static PlacementType GetPlacementType(string placementString)
	{
		return placementString switch
		{
			"absolute" => PlacementType.FreeFloating, 
			"oneCell" => PlacementType.Move, 
			"twoCell" => PlacementType.MoveAndSize, 
			_ => PlacementType.MoveAndSize, 
		};
	}

	internal static OfficeUnderline GetOfficeUnderlineType(string value)
	{
		return value switch
		{
			"none" => OfficeUnderline.None, 
			"dbl" => OfficeUnderline.Double, 
			"sng" => OfficeUnderline.Single, 
			"heavy" => OfficeUnderline.Heavy, 
			"dotted" => OfficeUnderline.Dotted, 
			"dottedHeavy" => OfficeUnderline.DottedHeavy, 
			"dash" => OfficeUnderline.Dash, 
			"dashHeavy" => OfficeUnderline.DashHeavy, 
			"dashLong" => OfficeUnderline.DashLong, 
			"dashLongHeavy" => OfficeUnderline.DashLongHeavy, 
			"dotDash" => OfficeUnderline.DotDash, 
			"dotDashHeavy" => OfficeUnderline.DotDashHeavy, 
			"dotDotDash" => OfficeUnderline.DotDotDash, 
			"dotDotDashHeavy" => OfficeUnderline.DotDotDashHeavy, 
			"wavy" => OfficeUnderline.Wavy, 
			"wavyHeavy" => OfficeUnderline.WavyHeavy, 
			"wavyDbl" => OfficeUnderline.WavyDouble, 
			"words" => OfficeUnderline.Words, 
			_ => OfficeUnderline.Single, 
		};
	}

	internal static string ToString(OfficeUnderline officeUnderlineType)
	{
		return officeUnderlineType switch
		{
			OfficeUnderline.None => "none", 
			OfficeUnderline.Single => "sng", 
			OfficeUnderline.Double => "dbl", 
			OfficeUnderline.Words => "words", 
			OfficeUnderline.Heavy => "heavy", 
			OfficeUnderline.Dotted => "dotted", 
			OfficeUnderline.DottedHeavy => "dottedHeavy", 
			OfficeUnderline.Dash => "dash", 
			OfficeUnderline.DashHeavy => "dashHeavy", 
			OfficeUnderline.DashLong => "dashLong", 
			OfficeUnderline.DashLongHeavy => "dashLongHeavy", 
			OfficeUnderline.DotDash => "dotDash", 
			OfficeUnderline.DotDashHeavy => "dotDashHeavy", 
			OfficeUnderline.DotDotDash => "dotDotDash", 
			OfficeUnderline.DotDotDashHeavy => "dotDotDashHeavy", 
			OfficeUnderline.Wavy => "wavy", 
			OfficeUnderline.WavyHeavy => "wavyHeavy", 
			OfficeUnderline.WavyDouble => "wavyDbl", 
			_ => throw new ArgumentException("Invalid OfficeUnderlineType value"), 
		};
	}

	internal static string GetOfficeChartType(OfficeChartType officeChartType)
	{
		switch (officeChartType)
		{
		case OfficeChartType.Area:
			return "areaChart";
		case OfficeChartType.Area_3D:
			return "area3DChart";
		case OfficeChartType.Bar_Clustered:
			return "barChart";
		case OfficeChartType.Bar_Clustered_3D:
			return "bar3DChart";
		case OfficeChartType.Line:
			return "lineChart";
		case OfficeChartType.Line_3D:
			return "line3DChart";
		case OfficeChartType.Bubble:
			return "bubbleChart";
		case OfficeChartType.Bubble_3D:
			return "bubble3D";
		case OfficeChartType.Surface_Contour:
		case OfficeChartType.Surface_NoColor_Contour:
			return "surfaceChart";
		case OfficeChartType.Surface_3D:
		case OfficeChartType.Surface_NoColor_3D:
			return "surface3DChart";
		case OfficeChartType.Radar:
			return "radarChart";
		case OfficeChartType.Scatter_Markers:
		case OfficeChartType.Scatter_SmoothedLine_Markers:
		case OfficeChartType.Scatter_SmoothedLine:
		case OfficeChartType.Scatter_Line_Markers:
		case OfficeChartType.Scatter_Line:
			return "scatterChart";
		case OfficeChartType.Pie:
			return "pieChart";
		case OfficeChartType.Pie_3D:
			return "pie3DChart";
		case OfficeChartType.Pie_Bar:
			return "bar";
		case OfficeChartType.Pie_Exploded:
		case OfficeChartType.Pie_Exploded_3D:
			return "explosion";
		case OfficeChartType.PieOfPie:
			return "pie";
		case OfficeChartType.Doughnut:
		case OfficeChartType.Doughnut_Exploded:
			return "doughnutChart";
		case OfficeChartType.Stock_HighLowClose:
		case OfficeChartType.Stock_OpenHighLowClose:
		case OfficeChartType.Stock_VolumeHighLowClose:
		case OfficeChartType.Stock_VolumeOpenHighLowClose:
			return "stockChart";
		default:
			return null;
		}
	}

	internal static string GetVerticalFlowType(TextVertOverflowType textVertOverflowType)
	{
		return textVertOverflowType switch
		{
			TextVertOverflowType.Clip => "clip", 
			TextVertOverflowType.Ellipsis => "ellipsis", 
			_ => "overflow", 
		};
	}

	internal static string GetHorizontalFlowType(TextHorzOverflowType textHorzOverflowType)
	{
		if (textHorzOverflowType == TextHorzOverflowType.Clip)
		{
			return "clip";
		}
		return "overflow";
	}

	internal static TextVertOverflowType GetVerticalFlowType(string value)
	{
		if (!(value == "clip"))
		{
			if (value == "ellipsis")
			{
				return TextVertOverflowType.Ellipsis;
			}
			return TextVertOverflowType.OverFlow;
		}
		return TextVertOverflowType.Clip;
	}

	internal static TextHorzOverflowType GetHorizontalFlowType(string value)
	{
		if (value == "clip")
		{
			return TextHorzOverflowType.Clip;
		}
		return TextHorzOverflowType.OverFlow;
	}

	internal static TextDirection SetTextDirection(string textVerticalType)
	{
		return textVerticalType switch
		{
			"horz" => TextDirection.Horizontal, 
			"vert" => TextDirection.RotateAllText90, 
			"vert270" => TextDirection.RotateAllText270, 
			"wordArtVert" => TextDirection.StackedLeftToRight, 
			"wordArtVertRtl" => TextDirection.StackedRightToLeft, 
			_ => TextDirection.Horizontal, 
		};
	}

	internal static void SetAnchorPosition(TextBodyPropertiesHolder textProperties, string anchorType, bool anchorCtrl)
	{
		switch (textProperties.TextDirection)
		{
		case TextDirection.Horizontal:
			switch (anchorType)
			{
			case "t":
				if (anchorCtrl)
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.TopCentered;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.Top;
				}
				break;
			case "ctr":
				if (anchorCtrl)
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.MiddleCentered;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.Middle;
				}
				break;
			case "b":
				if (anchorCtrl)
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.BottomCentered;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.VerticalAlignment = OfficeVerticalAlignment.Bottom;
				}
				break;
			}
			break;
		case TextDirection.RotateAllText90:
		case TextDirection.StackedRightToLeft:
			switch (anchorType)
			{
			case "t":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.RightMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Right;
				}
				break;
			case "ctr":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.CenterMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				break;
			case "b":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.LeftMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Left;
				}
				break;
			}
			break;
		case TextDirection.RotateAllText270:
		case TextDirection.StackedLeftToRight:
			switch (anchorType)
			{
			case "t":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.LeftMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Left;
				}
				break;
			case "ctr":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.CenterMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				break;
			case "b":
				if (anchorCtrl)
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.RightMiddle;
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Center;
				}
				else
				{
					textProperties.HorizontalAlignment = OfficeHorizontalAlignment.Right;
				}
				break;
			}
			break;
		}
	}

	internal static double ParseDouble(string value)
	{
		return double.Parse(value, CultureInfo.InvariantCulture);
	}

	internal static int ParseInt(string value)
	{
		return int.Parse(value, CultureInfo.InvariantCulture);
	}

	internal static string ToString(double value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	internal static string ToString(int value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	internal static bool ParseBoolen(string value)
	{
		if (value.Length != 1)
		{
			return string.Compare(value, "true") == 0;
		}
		switch (value)
		{
		default:
			return false;
		case "1":
		case "t":
		case "T":
			return true;
		}
	}

	internal static short ParseShort(string value)
	{
		return short.Parse(value, CultureInfo.InvariantCulture);
	}

	internal static int ConvertEmuToOffset(int emuValue, int resolution)
	{
		return (int)((double)emuValue / 12700.0 / 72.0 * (double)resolution + 0.5);
	}

	internal static int ConvertOffsetToEMU(int offsetValue, int resolution)
	{
		return (int)((double)offsetValue * 72.0 / (double)resolution * 12700.0 + 0.5);
	}

	internal static AnchorType GetAnchorType(string anchorType)
	{
		return anchorType switch
		{
			"oneCellAnchor" => AnchorType.OneCell, 
			"absoluteAnchor" => AnchorType.Absolute, 
			"relSizeAnchor" => AnchorType.RelSize, 
			_ => AnchorType.TwoCell, 
		};
	}

	internal static string GetAnchorTypeString(AnchorType anchorType)
	{
		return anchorType switch
		{
			AnchorType.Absolute => "absoluteAnchor", 
			AnchorType.OneCell => "oneCellAnchor", 
			AnchorType.RelSize => "relSizeAnchor", 
			_ => "twoCellAnchor", 
		};
	}

	internal static double EmuToPoint(int emu)
	{
		return Convert.ToDouble((double)emu / 12700.0);
	}

	internal static int PointToEmu(double point)
	{
		return (int)(point * 12700.0);
	}
}

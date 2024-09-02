using System.Collections.Generic;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.ChartToImageConverter;

internal static class ExtensionMethods
{
	internal static List<IOfficeChartSerie> OrderByType(this IOfficeChartSeries series)
	{
		List<IOfficeChartSerie> list = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list2 = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list3 = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list4 = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list5 = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list6 = new List<IOfficeChartSerie>(series.Count);
		List<IOfficeChartSerie> list7 = new List<IOfficeChartSerie>(series.Count);
		foreach (IOfficeChartSerie item in series)
		{
			if (!item.IsFiltered)
			{
				switch (item.SerieType)
				{
				case OfficeChartType.Area:
				case OfficeChartType.Area_Stacked:
				case OfficeChartType.Area_Stacked_100:
				case OfficeChartType.Area_3D:
				case OfficeChartType.Area_Stacked_3D:
				case OfficeChartType.Area_Stacked_100_3D:
					list2.Add(item);
					break;
				case OfficeChartType.Column_Clustered:
				case OfficeChartType.Column_Stacked:
				case OfficeChartType.Column_Stacked_100:
				case OfficeChartType.Column_Clustered_3D:
				case OfficeChartType.Column_Stacked_3D:
				case OfficeChartType.Column_Stacked_100_3D:
				case OfficeChartType.Column_3D:
					list3.Add(item);
					break;
				case OfficeChartType.Line:
				case OfficeChartType.Line_Stacked:
				case OfficeChartType.Line_Stacked_100:
				case OfficeChartType.Line_Markers:
				case OfficeChartType.Line_Markers_Stacked:
				case OfficeChartType.Line_Markers_Stacked_100:
				case OfficeChartType.Line_3D:
					list7.Add(item);
					break;
				case OfficeChartType.Pie:
				case OfficeChartType.Pie_3D:
				case OfficeChartType.PieOfPie:
				case OfficeChartType.Pie_Exploded:
				case OfficeChartType.Pie_Exploded_3D:
				case OfficeChartType.Pie_Bar:
					list.Add(item);
					break;
				case OfficeChartType.Radar:
				case OfficeChartType.Radar_Markers:
				case OfficeChartType.Radar_Filled:
					list5.Add(item);
					break;
				case OfficeChartType.Scatter_Markers:
				case OfficeChartType.Scatter_SmoothedLine_Markers:
				case OfficeChartType.Scatter_SmoothedLine:
				case OfficeChartType.Scatter_Line_Markers:
				case OfficeChartType.Scatter_Line:
					list6.Add(item);
					break;
				case OfficeChartType.Bar_Clustered:
				case OfficeChartType.Bar_Stacked:
				case OfficeChartType.Bar_Stacked_100:
				case OfficeChartType.Bar_Clustered_3D:
				case OfficeChartType.Bar_Stacked_3D:
				case OfficeChartType.Bar_Stacked_100_3D:
					list4.Add(item);
					break;
				}
			}
		}
		list.AddRange(list5);
		if (list2.Count > 1 && !(list2[0].SerieFormat.CommonSerieOptions as ChartFormatImpl).IsStacked)
		{
			list2.Reverse();
		}
		if (list5.Count > 1 && (list5[0].SerieFormat.CommonSerieOptions as ChartFormatImpl).FormatRecordType == TBIFFRecord.ChartRadarArea)
		{
			list5.Reverse();
		}
		list.AddRange(list6);
		list.AddRange(list7);
		list.AddRange(list4);
		list.AddRange(list3);
		list.AddRange(list2);
		return list;
	}
}

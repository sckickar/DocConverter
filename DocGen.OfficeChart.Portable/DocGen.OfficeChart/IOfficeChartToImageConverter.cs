using System.IO;

namespace DocGen.OfficeChart;

public interface IOfficeChartToImageConverter
{
	ScalingMode ScalingMode { get; set; }

	void SaveAsImage(IOfficeChart chart, Stream imageAsStream);
}

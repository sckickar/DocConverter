namespace DocGen.OfficeChart;

public interface IThreeDFormat
{
	Office2007ChartBevelProperties BevelTop { get; set; }

	Office2007ChartBevelProperties BevelBottom { get; set; }

	Office2007ChartMaterialProperties Material { get; set; }

	Office2007ChartLightingProperties Lighting { get; set; }

	int BevelTopHeight { get; set; }

	int BevelTopWidth { get; set; }

	int BevelBottomHeight { get; set; }

	int BevelBottomWidth { get; set; }
}

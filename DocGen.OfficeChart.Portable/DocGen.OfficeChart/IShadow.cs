using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IShadow
{
	Office2007ChartPresetsOuter ShadowOuterPresets { get; set; }

	Office2007ChartPresetsInner ShadowInnerPresets { get; set; }

	Office2007ChartPresetsPerspective ShadowPerspectivePresets { get; set; }

	bool HasCustomShadowStyle { get; set; }

	int Transparency { get; set; }

	int Size { get; set; }

	int Blur { get; set; }

	int Angle { get; set; }

	int Distance { get; set; }

	Color ShadowColor { get; set; }

	void CustomShadowStyles(Office2007ChartPresetsOuter iOuter, int iTransparency, int iSize, int iBlur, int iAngle, int iDistance, bool iCustomShadowStyle);

	void CustomShadowStyles(Office2007ChartPresetsInner iInner, int iTransparency, int iBlur, int iAngle, int iDistance, bool iCustomShadowStyle);

	void CustomShadowStyles(Office2007ChartPresetsPerspective iPerspective, int iTransparency, int iSize, int iBlur, int iAngle, int iDistance, bool iCustomShadowStyle);
}

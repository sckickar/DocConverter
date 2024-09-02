using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IOfficeFont : IParentApplication, IOptimizedUpdate
{
	bool Bold { get; set; }

	OfficeKnownColors Color { get; set; }

	Color RGBColor { get; set; }

	bool Italic { get; set; }

	bool MacOSOutlineFont { get; set; }

	bool MacOSShadow { get; set; }

	double Size { get; set; }

	bool Strikethrough { get; set; }

	bool Subscript { get; set; }

	bool Superscript { get; set; }

	OfficeUnderline Underline { get; set; }

	string FontName { get; set; }

	OfficeFontVerticalAlignment VerticalAlignment { get; set; }

	bool IsAutoColor { get; }

	Font GenerateNativeFont();
}

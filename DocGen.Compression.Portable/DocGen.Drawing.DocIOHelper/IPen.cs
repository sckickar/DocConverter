namespace DocGen.Drawing.DocIOHelper;

internal interface IPen
{
	float Width { get; set; }

	DashStyle DashStyle { get; set; }

	Color Color { get; set; }

	float[] CompoundArray { get; set; }

	float[] DashPattern { get; set; }

	PenAlignment Alignment { get; set; }

	LineJoin LineJoin { get; set; }

	LineCap StartCap { get; set; }

	LineCap EndCap { get; set; }

	DashCap DashCap { get; set; }
}

using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal interface ILayoutedFuntionWidget
{
	IOfficeMathFunctionBase Widget { get; }

	RectangleF Bounds { get; set; }

	LayoutedOMathWidget Owner { get; set; }

	void ShiftXYPosition(float xPosition, float yPosition);

	void Dispose();
}

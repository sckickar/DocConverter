namespace DocGen.OfficeChart;

internal interface IShape : IParentApplication
{
	int Height { get; set; }

	int Id { get; }

	int Left { get; set; }

	string Name { get; set; }

	int Top { get; set; }

	int Width { get; set; }

	OfficeShapeType ShapeType { get; }

	bool IsShapeVisible { get; set; }

	string AlternativeText { get; set; }

	bool IsMoveWithCell { get; set; }

	bool IsSizeWithCell { get; set; }

	IOfficeFill Fill { get; }

	IShapeLineFormat Line { get; }

	string OnAction { get; set; }

	IShadow Shadow { get; }

	IThreeDFormat ThreeD { get; }

	int ShapeRotation { get; set; }

	ITextFrame TextFrame { get; }

	void Remove();

	void Scale(int scaleWidth, int scaleHeight);
}

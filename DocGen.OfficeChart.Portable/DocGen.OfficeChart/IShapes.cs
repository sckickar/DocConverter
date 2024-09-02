using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IShapes : IParentApplication, IEnumerable
{
	int Count { get; }

	IShape this[int index] { get; }

	IShape this[string strShapeName] { get; }

	IOfficeChartShape AddChart();

	IShape AddCopy(IShape sourceShape);

	IShape AddCopy(IShape sourceShape, Dictionary<string, string> hashNewNames, List<int> arrFontIndexes);

	ITextBoxShapeEx AddTextBox();

	IShape AddAutoShapes(AutoShapeType autoShapeType, int topRow, int leftColumn, int height, int width);

	IPictureShape AddPicture(Image image, string pictureName, ExcelImageFormat imageFormat);
}

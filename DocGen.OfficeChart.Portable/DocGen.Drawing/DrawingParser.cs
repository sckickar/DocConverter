using System.IO;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.Drawing;

internal class DrawingParser
{
	internal AutoShapeType autoShapeType = AutoShapeType.Unknown;

	internal bool isHyperLink;

	internal bool isGroupShape;

	internal double topX;

	internal double topY;

	internal double bottomX;

	internal double bottomY;

	internal AutoShapeConstant autoShapeConstant = AutoShapeConstant.Index_187;

	internal int leftColumn;

	internal int leftColumnOffset;

	internal int posX;

	internal int posY;

	internal int extCX;

	internal int extCY;

	internal int topRow;

	internal int topRowOffset;

	internal int rightColumn;

	internal int rightColumnOffset;

	internal int bottomRow;

	internal int bottomRowOffset;

	internal int cx;

	internal int cy;

	internal ClientAnchor clientAnchor;

	internal string placement;

	internal string relationID;

	internal string anchorName;

	internal int id;

	internal string name;

	internal string descr;

	internal string tittle;

	public double shapeRotation;

	public Stream CustGeomStream;

	internal Stream FillStream;

	public bool IsHidden;

	public bool FlipVertical;

	public bool FlipHorizontal;

	public string preFix = "xdr";

	public string shapeType;

	internal void AddShape(AutoShapeImpl autoShapeImpl, WorksheetBaseImpl sheet)
	{
		autoShapeImpl.CreateShape(autoShapeType, sheet);
		autoShapeImpl.ShapeExt.AnchorType = Helper.GetAnchorType(anchorName);
		if (shapeType == "cxnSp")
		{
			autoShapeImpl.ShapeExt.ShapeType = ExcelAutoShapeType.cxnSp;
		}
		else
		{
			autoShapeImpl.ShapeExt.ShapeType = ExcelAutoShapeType.sp;
		}
		autoShapeImpl.SetShapeID(id);
		autoShapeImpl.Name = name;
		autoShapeImpl.AlternativeText = descr;
		autoShapeImpl.IsHidden = IsHidden;
		autoShapeImpl.IsShapeVisible = !IsHidden;
		autoShapeImpl.Title = tittle;
		autoShapeImpl.ShapeExt.Rotation = shapeRotation;
		autoShapeImpl.ShapeExt.FlipVertical = FlipVertical;
		autoShapeImpl.ShapeExt.FlipHorizontal = FlipHorizontal;
		if (CustGeomStream != null && CustGeomStream.Length > 0)
		{
			autoShapeImpl.ShapeExt.PreservedElements.Add("avLst", CustGeomStream);
		}
	}
}

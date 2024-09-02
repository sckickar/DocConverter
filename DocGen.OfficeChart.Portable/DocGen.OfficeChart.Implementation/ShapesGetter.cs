using System;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class ShapesGetter : IShapeGetter, ICloneable
{
	public ShapeCollectionBase GetShapes(WorksheetBaseImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		return sheet.InnerShapesBase;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}

using System;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class HeaderImageGetter : IShapeGetter, ICloneable
{
	public ShapeCollectionBase GetShapes(WorksheetBaseImpl sheet)
	{
		return sheet.HeaderFooterShapes;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}

using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.OfficeChart.Interfaces;

internal interface IShapeGetter : ICloneable
{
	ShapeCollectionBase GetShapes(WorksheetBaseImpl sheet);
}

using System.Collections;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IBorders : IEnumerable, IParentApplication
{
	OfficeKnownColors Color { get; set; }

	Color ColorRGB { get; set; }

	int Count { get; }

	IBorder this[OfficeBordersIndex Index] { get; }

	OfficeLineStyle LineStyle { get; set; }

	OfficeLineStyle Value { get; set; }
}

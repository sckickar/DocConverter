using System;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[CLSCompliant(false)]
internal interface IFopteOptionWrapper
{
	void AddOptionSorted(MsofbtOPT.FOPTE option);

	void RemoveOption(int index);
}

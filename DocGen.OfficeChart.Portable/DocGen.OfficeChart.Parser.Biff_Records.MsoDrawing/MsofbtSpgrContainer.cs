using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtSpgrContainer)]
[CLSCompliant(false)]
internal class MsofbtSpgrContainer : MsoContainerBase
{
	private const int DEF_VERSION = 15;

	public MsofbtSpgrContainer(MsoBase parent)
		: base(parent)
	{
		base.Version = 15;
	}

	public MsofbtSpgrContainer(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtSpgrContainer(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}
}

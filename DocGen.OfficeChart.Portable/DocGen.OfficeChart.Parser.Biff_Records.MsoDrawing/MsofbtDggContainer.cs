using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtDggContainer)]
[CLSCompliant(false)]
internal class MsofbtDggContainer : MsoContainerBase
{
	private const int DEF_VERSION = 15;

	public MsofbtDggContainer(MsoBase parent)
		: base(parent)
	{
		base.Version = 15;
	}

	public MsofbtDggContainer(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtDggContainer(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}

	protected override void OnDispose()
	{
		for (int i = 0; i < base.Items.Length; i++)
		{
			base.Items[i].Dispose();
		}
	}
}

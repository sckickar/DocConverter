using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtBstoreContainer)]
[CLSCompliant(false)]
internal class MsofbtBstoreContainer : MsoContainerBase
{
	private const int DEF_VERSION = 15;

	private const int DEF_INSTANCE = 1;

	public MsofbtBstoreContainer(MsoBase parent)
		: base(parent)
	{
		base.Version = 15;
		base.Instance = 1;
	}

	public MsofbtBstoreContainer(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtBstoreContainer(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}

	protected override void OnDispose()
	{
		if (base.Items.Length != 0)
		{
			int num = base.Items.Length;
			for (int i = 0; i < num; i++)
			{
				base.Items[i].Dispose();
			}
		}
		base.OnDispose();
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		base.Instance = base.Items.Length;
		base.InfillInternalData(stream, iOffset, arrBreaks, arrRecords);
	}
}

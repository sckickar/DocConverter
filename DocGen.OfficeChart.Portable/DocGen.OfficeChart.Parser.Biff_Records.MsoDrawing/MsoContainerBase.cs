using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[CLSCompliant(false)]
internal abstract class MsoContainerBase : MsoBase
{
	private List<MsoBase> m_arrItems = new List<MsoBase>();

	public MsoBase[] Items => m_arrItems.ToArray();

	internal List<MsoBase> ItemsList => m_arrItems;

	public MsoContainerBase(MsoBase parent)
		: base(parent)
	{
	}

	public MsoContainerBase(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsoContainerBase(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}

	private void ParseItems(Stream data, int iOffset)
	{
		long num = data.Position + m_iLength;
		while (num > data.Position)
		{
			MsoBase item = ((base.DataGetter == null) ? MsoFactory.CreateMsoRecord(this, data) : MsoFactory.CreateMsoRecord(this, data, base.DataGetter));
			m_arrItems.Add(item);
		}
	}

	public void AddItem(MsoBase itemToAdd)
	{
		if (itemToAdd == null)
		{
			throw new ArgumentNullException("itemToAdd");
		}
		m_arrItems.Add(itemToAdd);
	}

	public void AddItems(ICollection<MsoBase> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		m_arrItems.AddRange(items);
	}

	public override void ParseStructure(Stream stream)
	{
		ParseItems(stream, 0);
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		long position = stream.Position;
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			m_arrItems[i].FillArray(stream, (int)stream.Position, arrBreaks, arrRecords);
		}
		m_iLength = (int)(stream.Position - position);
	}

	protected override object InternalClone()
	{
		MsoContainerBase msoContainerBase = (MsoContainerBase)base.InternalClone();
		if (msoContainerBase.m_arrItems != null)
		{
			int count = m_arrItems.Count;
			List<MsoBase> list = new List<MsoBase>(count);
			for (int i = 0; i < count; i++)
			{
				MsoBase item = m_arrItems[i].Clone(msoContainerBase);
				list.Add(item);
			}
			msoContainerBase.m_arrItems = list;
		}
		return msoContainerBase;
	}
}

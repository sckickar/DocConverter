using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartGelFrame)]
[CLSCompliant(false)]
internal class ChartGelFrameRecord : BiffContinueRecordRaw
{
	private readonly byte[] DEF_FIRST_BYTES = new byte[4] { 227, 1, 11, 240 };

	private readonly byte[] DEF_LAST_BYTES = new byte[74]
	{
		179, 0, 34, 241, 66, 0, 0, 0, 158, 1,
		255, 255, 255, 255, 159, 1, 255, 255, 255, 255,
		160, 1, 0, 0, 0, 32, 161, 193, 0, 0,
		0, 0, 162, 1, 255, 255, 255, 255, 163, 1,
		255, 255, 255, 255, 164, 1, 0, 0, 0, 32,
		165, 193, 0, 0, 0, 0, 166, 1, 255, 255,
		255, 255, 167, 1, 255, 255, 255, 255, 191, 1,
		0, 0, 96, 0
	};

	public const int DEF_START_MSO_INDEX = 384;

	public const int DEF_LAST_MSO_INDEX = 412;

	public const int DEF_OFFSET = 8;

	private List<MsofbtOPT.FOPTE> m_list = new List<MsofbtOPT.FOPTE>();

	public override bool NeedDataArray => true;

	public List<MsofbtOPT.FOPTE> OptionList
	{
		get
		{
			return m_list;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("OptionList");
			}
			m_list = value;
		}
	}

	public ChartGelFrameRecord()
	{
	}

	public ChartGelFrameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartGelFrameRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		if (m_extractor != null)
		{
			AddContinueRecordType(base.TypeCode);
			base.ParseStructure();
			ParseData();
			m_arrContinuePos.Clear();
		}
	}

	private void ParseData()
	{
		int iOffset = 8;
		uint num = BitConverter.ToUInt32(m_data, 4) + (uint)iOffset;
		while (iOffset < num)
		{
			MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE(m_data, ref iOffset);
			m_list.Add(fOPTE);
			if (fOPTE.IsComplex)
			{
				num -= fOPTE.UInt32Value;
			}
		}
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			m_list[i].ReadComplexData(m_data, ref iOffset);
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
	}

	private void FillDataList()
	{
		m_iLength = 0;
		int num = 8;
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			MsofbtOPT.FOPTE fOPTE = m_list[i];
			m_iLength += fOPTE.MainData.Length;
			num += fOPTE.MainData.Length;
			if (fOPTE.AdditionalData != null && fOPTE.AdditionalData.Length != 0)
			{
				m_iLength += fOPTE.AdditionalData.Length;
			}
		}
		m_iLength += DEF_LAST_BYTES.Length + 8;
		m_data = new byte[m_iLength];
		DEF_FIRST_BYTES.CopyTo(m_data, 0);
		DEF_LAST_BYTES.CopyTo(m_data, m_iLength - DEF_LAST_BYTES.Length);
		Array.Copy(BitConverter.GetBytes(m_iLength - 8 - DEF_LAST_BYTES.Length), 0, m_data, 4, 4);
		int num2 = 8;
		int j = 0;
		for (int count2 = m_list.Count; j < count2; j++)
		{
			MsofbtOPT.FOPTE fOPTE2 = m_list[j];
			fOPTE2.MainData.CopyTo(m_data, num2);
			num2 += fOPTE2.MainData.Length;
			if (fOPTE2.AdditionalData != null && fOPTE2.AdditionalData.Length != 0)
			{
				fOPTE2.AdditionalData.CopyTo(m_data, num);
				num += fOPTE2.AdditionalData.Length;
			}
		}
	}

	public List<BiffRecordRaw> UpdatesToAddInStream()
	{
		FillDataList();
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		int num = m_data.Length;
		BiffRecordRawWithArray biffRecordRawWithArray = this;
		int num2 = 0;
		int num3 = 0;
		while (num > 8224)
		{
			biffRecordRawWithArray = (BiffRecordRawWithArray)BiffRecordFactory.GetRecord((num3 < 2) ? TBIFFRecord.ChartGelFrame : TBIFFRecord.Continue);
			byte[] array = new byte[8224];
			Array.Copy(m_data, num2, array, 0, 8224);
			biffRecordRawWithArray.SetInternalData(array);
			biffRecordRawWithArray.Length = 8224;
			list.Add(biffRecordRawWithArray);
			num2 += 8224;
			num3++;
			num -= 8224;
		}
		if (num2 == 0)
		{
			list.Add(biffRecordRawWithArray);
		}
		else
		{
			biffRecordRawWithArray = (BiffRecordRawWithArray)BiffRecordFactory.GetRecord((num3 < 2) ? TBIFFRecord.ChartGelFrame : TBIFFRecord.Continue);
			int num4 = m_data.Length - num2;
			byte[] array2 = new byte[num4];
			Array.Copy(m_data, num2, array2, 0, num4);
			biffRecordRawWithArray.SetInternalData(array2);
			biffRecordRawWithArray.Length = num4;
			list.Add(biffRecordRawWithArray);
		}
		return list;
	}

	public override object Clone()
	{
		ChartGelFrameRecord chartGelFrameRecord = (ChartGelFrameRecord)base.Clone();
		chartGelFrameRecord.m_list = new List<MsofbtOPT.FOPTE>();
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			MsofbtOPT.FOPTE fOPTE = m_list[i];
			chartGelFrameRecord.m_list.Add((MsofbtOPT.FOPTE)fOPTE.Clone());
		}
		return chartGelFrameRecord;
	}

	public void UpdateToSerialize()
	{
		m_iLength = -1;
		int index;
		for (int i = 384; i <= 412; i++)
		{
			MsoOptions id = (MsoOptions)i;
			if (!Contains(id, out index))
			{
				MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
				fOPTE.Id = id;
				if (fOPTE.Id == MsoOptions.Transparency || fOPTE.Id == MsoOptions.GradientTransparency)
				{
					fOPTE.UInt32Value = 65535u;
				}
				if (fOPTE.Id == MsoOptions.GradientColorType)
				{
					fOPTE.UInt32Value = 1u;
				}
				m_list.Insert(index, fOPTE);
			}
		}
		if (!Contains(MsoOptions.NoFillHitTest, out index))
		{
			MsofbtOPT.FOPTE fOPTE2 = new MsofbtOPT.FOPTE();
			fOPTE2.Id = MsoOptions.NoFillHitTest;
			m_list.Insert(index, fOPTE2);
		}
	}

	private bool Contains(MsoOptions id, out int index)
	{
		index = 0;
		int count = m_list.Count;
		while (index < count && m_list[index].Id != id)
		{
			index++;
		}
		return index < m_list.Count;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtClientTextbox)]
[CLSCompliant(false)]
internal class MsofbtClientTextBox : MsoBase
{
	private List<BiffRecordRaw> m_arrAdditionalData = new List<BiffRecordRaw>(3);

	public TextObjectRecord TextObject
	{
		get
		{
			if (m_arrAdditionalData.Count <= 0)
			{
				return null;
			}
			return m_arrAdditionalData[0] as TextObjectRecord;
		}
		set
		{
			m_arrAdditionalData.Insert(0, value);
		}
	}

	public string Text
	{
		get
		{
			int num = TextObject?.TextLen ?? 0;
			int count = m_arrAdditionalData.Count;
			if (num == 0 || count <= 1)
			{
				return null;
			}
			int num2 = 0;
			int num3 = 1;
			while (num2 < num)
			{
				ContinueRecord obj = m_arrAdditionalData[num3] as ContinueRecord;
				bool flag = obj.Data[0] != 0;
				int num4 = obj.Length - 1;
				num2 += (flag ? (num4 / 2) : num4);
				num3++;
			}
			return CombineAndExtractText(1, num3);
		}
	}

	public byte[] FormattingRuns
	{
		get
		{
			TextObjectRecord textObject = TextObject;
			byte[] array = null;
			if (textObject != null)
			{
				int formattingRunsLen = TextObject.FormattingRunsLen;
				int num = 0;
				int count = m_arrAdditionalData.Count;
				int num2 = count - 1;
				while (num < formattingRunsLen)
				{
					BiffRecordRaw biffRecordRaw = m_arrAdditionalData[num2];
					num += biffRecordRaw.Length;
					num2--;
				}
				array = ((formattingRunsLen > 0) ? new byte[formattingRunsLen] : null);
				int i = num2 + 1;
				int num3 = 0;
				for (; i < count; i++)
				{
					BiffRecordRaw biffRecordRaw2 = m_arrAdditionalData[i];
					int length = biffRecordRaw2.Length;
					if (array != null)
					{
						Buffer.BlockCopy(biffRecordRaw2.Data, 0, array, num3, length);
					}
					num3 += length;
				}
			}
			return array;
		}
	}

	public BiffRecordRaw[] AdditionalData
	{
		get
		{
			if (m_arrAdditionalData == null)
			{
				return null;
			}
			return m_arrAdditionalData.ToArray();
		}
	}

	private string CombineAndExtractText(int startIndex, int afterEndIndex)
	{
		string text = string.Empty;
		for (int i = startIndex; i < afterEndIndex; i++)
		{
			ContinueRecord continueRecord = (ContinueRecord)m_arrAdditionalData[i];
			byte[] data = continueRecord.Data;
			int length = continueRecord.Length;
			int iStrLen = ((data[0] != 0) ? ((length - 1) / 2) : (length - 1));
			text += continueRecord.GetString(0, iStrLen);
		}
		return text;
	}

	public MsofbtClientTextBox(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtClientTextBox(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtClientTextBox(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
		BiffRecordRaw[] array = dataGetter();
		if (array == null)
		{
			throw new ArgumentException("Additional data can't be null");
		}
		m_arrAdditionalData.Clear();
		m_arrAdditionalData.AddRange(array);
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_iLength = 0;
		if (arrBreaks != null && arrRecords != null)
		{
			arrBreaks.Add(m_iLength + iOffset);
			arrRecords.Add(m_arrAdditionalData);
		}
	}

	public override void ParseStructure(Stream stream)
	{
	}

	protected override object InternalClone()
	{
		MsofbtClientTextBox obj = (MsofbtClientTextBox)base.InternalClone();
		obj.m_arrAdditionalData = CloneUtils.CloneCloneable(m_arrAdditionalData);
		return obj;
	}

	public override void UpdateNextMsoDrawingData()
	{
		BiffRecordRaw[] array = base.DataGetter();
		if (array == null)
		{
			throw new ArgumentException("Additional data can't be null");
		}
		m_arrAdditionalData.Clear();
		m_arrAdditionalData.AddRange(array);
	}

	public void AddRecord(BiffRecordRaw record)
	{
		m_arrAdditionalData.Add(record);
	}
}

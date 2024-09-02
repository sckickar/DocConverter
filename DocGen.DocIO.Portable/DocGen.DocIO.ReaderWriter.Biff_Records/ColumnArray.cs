using System;
using System.Collections.Generic;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class ColumnArray : List<object>
{
	private const int DEF_DISTANCE_BETWEEN_COLUMNS = 720;

	private SinglePropertyModifierArray m_sprms;

	internal new ColumnDescriptor this[int index] => (ColumnDescriptor)base[index];

	internal bool ColumnsEvenlySpaced
	{
		get
		{
			return m_sprms.GetByte(12293, 1) == 1;
		}
		set
		{
			m_sprms.SetIntValue(12293, value ? 1 : 0);
		}
	}

	internal ColumnArray(SinglePropertyModifierArray sprms)
	{
		m_sprms = sprms;
	}

	internal ColumnDescriptor AddColumn()
	{
		m_sprms.SetUShortValue(20491, (ushort)base.Count);
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(61955);
		SinglePropertyModifierRecord singlePropertyModifierRecord2 = new SinglePropertyModifierRecord(61956);
		singlePropertyModifierRecord.ByteArray = new byte[3];
		singlePropertyModifierRecord2.ByteArray = new byte[3];
		singlePropertyModifierRecord.ByteArray[0] = (byte)base.Count;
		singlePropertyModifierRecord2.ByteArray[0] = (byte)base.Count;
		m_sprms.Add(singlePropertyModifierRecord);
		m_sprms.Add(singlePropertyModifierRecord2);
		ColumnDescriptor columnDescriptor = new ColumnDescriptor(singlePropertyModifierRecord, singlePropertyModifierRecord2);
		Add(columnDescriptor);
		return columnDescriptor;
	}

	internal ColumnDescriptor AddEmptyColumn()
	{
		m_sprms.SetUShortValue(20491, (ushort)base.Count);
		ColumnDescriptor columnDescriptor = new ColumnDescriptor();
		Add(columnDescriptor);
		return columnDescriptor;
	}

	internal void Close()
	{
		m_sprms = null;
	}

	internal void ReadColumnsProperties(bool isFromPropertyHash)
	{
		Dictionary<int, SinglePropertyModifierRecord> dictionary = new Dictionary<int, SinglePropertyModifierRecord>();
		if (m_sprms.Contain(12857))
		{
			if (isFromPropertyHash)
			{
				for (int i = 0; i < m_sprms.Modifiers.Count; i++)
				{
					SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms.Modifiers[i];
					_ = singlePropertyModifierRecord.TypedOptions;
					if (singlePropertyModifierRecord.TypedOptions == 12857)
					{
						break;
					}
					if (singlePropertyModifierRecord.TypedOptions == 20491)
					{
						dictionary.Add(i, singlePropertyModifierRecord);
						m_sprms.Modifiers.Remove(singlePropertyModifierRecord);
					}
				}
			}
			else
			{
				for (int num = m_sprms.Modifiers.Count - 1; num > 0; num--)
				{
					SinglePropertyModifierRecord singlePropertyModifierRecord2 = m_sprms.Modifiers[num];
					_ = singlePropertyModifierRecord2.TypedOptions;
					if (singlePropertyModifierRecord2.TypedOptions == 12857)
					{
						break;
					}
					if (singlePropertyModifierRecord2.TypedOptions == 20491)
					{
						dictionary.Add(num, singlePropertyModifierRecord2);
						m_sprms.Modifiers.Remove(singlePropertyModifierRecord2);
					}
				}
			}
		}
		ReadColumn();
		if (dictionary.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, SinglePropertyModifierRecord> item in dictionary)
		{
			if (m_sprms.Count <= item.Key)
			{
				m_sprms.Modifiers.Add(item.Value);
			}
			else
			{
				m_sprms.Modifiers.Insert(item.Key, item.Value);
			}
		}
		dictionary.Clear();
	}

	internal void ReadColumn()
	{
		int num = m_sprms.GetUShort(20491, 0) + 1;
		for (int i = 0; i < num; i++)
		{
			ColumnDescriptor item = new ColumnDescriptor();
			Add(item);
		}
		if (ColumnsEvenlySpaced)
		{
			ushort uShort = m_sprms.GetUShort(45087, 0);
			ushort uShort2 = m_sprms.GetUShort(45089, 0);
			ushort uShort3 = m_sprms.GetUShort(45090, 0);
			int num2 = uShort - uShort2 - uShort3;
			ushort uShort4 = m_sprms.GetUShort(36876, 720);
			int num3 = (num2 - (num - 1) * uShort4) / num;
			for (int j = 0; j < num; j++)
			{
				this[j].Width = (ushort)num3;
				this[j].Space = uShort4;
			}
			return;
		}
		List<ushort> list = new List<ushort>();
		List<ushort> list2 = new List<ushort>();
		for (int k = 0; k < m_sprms.Count; k++)
		{
			if (m_sprms.GetSprmByIndex(k).TypedOptions.ToString() == 61955.ToString())
			{
				byte[] byteArray = m_sprms.GetSprmByIndex(k).ByteArray;
				list.Add(BitConverter.ToUInt16(byteArray, 1));
			}
			else if (m_sprms.GetSprmByIndex(k).TypedOptions.ToString() == 61956.ToString())
			{
				byte[] byteArray2 = m_sprms.GetSprmByIndex(k).ByteArray;
				list2.Add(BitConverter.ToUInt16(byteArray2, 1));
			}
		}
		if (base.Count < 1)
		{
			return;
		}
		for (int l = 0; l < base.Count; l++)
		{
			if (list.Count > l)
			{
				this[l].Width = list[l];
				if (l + 1 < base.Count)
				{
					this[l].Space = list2[l];
				}
			}
		}
	}
}

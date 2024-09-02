using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtOPT)]
[CLSCompliant(false)]
internal class MsofbtOPT : MsoBase, ICloneable, IFopteOptionWrapper
{
	public class FOPTE : ICloneable
	{
		private const ushort DEF_ID_MASK = 16383;

		private const ushort DEF_VALID_MASK = 16384;

		private const ushort DEF_COMPLEX_MASK = 32768;

		private const int DEF_RECORD_SIZE = 6;

		private ushort m_usId;

		private bool m_bIdValid;

		private bool m_bComplex;

		private uint m_uiValue;

		private byte[] m_arrData;

		public MsoOptions Id
		{
			get
			{
				return (MsoOptions)m_usId;
			}
			set
			{
				m_usId = (ushort)value;
			}
		}

		public bool IsValid
		{
			get
			{
				return m_bIdValid;
			}
			set
			{
				m_bIdValid = value;
			}
		}

		public bool IsComplex
		{
			get
			{
				return m_bComplex;
			}
			set
			{
				m_bComplex = value;
			}
		}

		public uint UInt32Value
		{
			get
			{
				return m_uiValue;
			}
			set
			{
				m_uiValue = value;
			}
		}

		public int Int32Value
		{
			get
			{
				return (int)m_uiValue;
			}
			set
			{
				m_uiValue = (uint)value;
			}
		}

		public byte[] AdditionalData
		{
			get
			{
				return m_arrData;
			}
			set
			{
				m_arrData = value;
			}
		}

		public byte[] MainData
		{
			get
			{
				byte[] array = new byte[Size];
				ushort num = (ushort)(m_usId & 0x3FFFu);
				if (m_bIdValid)
				{
					num += 16384;
				}
				if (m_bComplex)
				{
					num += 32768;
				}
				BitConverter.GetBytes(num).CopyTo(array, 0);
				BitConverter.GetBytes(m_uiValue).CopyTo(array, 2);
				return array;
			}
		}

		public static int Size => 6;

		public FOPTE()
		{
		}

		public FOPTE(byte[] data, ref int iOffset)
		{
			ushort num = BitConverter.ToUInt16(data, iOffset);
			m_usId = (ushort)(num & 0x3FFFu);
			m_bIdValid = (num & 0x4000) != 0;
			m_bComplex = (num & 0x8000) != 0;
			iOffset += 2;
			m_uiValue = BitConverter.ToUInt32(data, iOffset);
			iOffset += 4;
		}

		public FOPTE(Stream stream)
		{
			ushort num = MsoBase.ReadUInt16(stream);
			m_usId = (ushort)(num & 0x3FFFu);
			m_bIdValid = (num & 0x4000) != 0;
			m_bComplex = (num & 0x8000) != 0;
			m_uiValue = MsoBase.ReadUInt32(stream);
		}

		public void ReadComplexData(byte[] m_data, ref int iOffset)
		{
			if (IsComplex)
			{
				m_arrData = new byte[UInt32Value];
				Array.Copy(m_data, iOffset, m_arrData, 0, (int)UInt32Value);
				iOffset += (int)UInt32Value;
			}
		}

		public void ReadComplexData(Stream stream)
		{
			if (IsComplex)
			{
				int uInt32Value = (int)UInt32Value;
				m_arrData = new byte[uInt32Value];
				stream.Read(m_arrData, 0, uInt32Value);
			}
		}

		public object Clone()
		{
			FOPTE fOPTE = (FOPTE)MemberwiseClone();
			if (m_arrData != null)
			{
				fOPTE.m_arrData = CloneUtils.CloneByteArray(m_arrData);
			}
			return fOPTE;
		}
	}

	private const int DEF_MINOPTION_INDEX = 127;

	private List<FOPTE> m_arrProperties = new List<FOPTE>();

	public FOPTE[] Properties => m_arrProperties.ToArray();

	public FOPTE this[int index]
	{
		get
		{
			if (index < 0 || index >= m_arrProperties.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than than Count - 1.");
			}
			return m_arrProperties[index];
		}
	}

	public IList<FOPTE> PropertyList => m_arrProperties;

	public MsofbtOPT(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtOPT(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void ParseStructure(Stream stream)
	{
		int i = 0;
		for (int num = m_iLength; i < num; i += FOPTE.Size)
		{
			FOPTE fOPTE = new FOPTE(stream);
			AddOptions(fOPTE);
			if (fOPTE.IsComplex)
			{
				num -= (int)fOPTE.UInt32Value;
			}
		}
		int j = 0;
		for (int count = m_arrProperties.Count; j < count; j++)
		{
			m_arrProperties[j].ReadComplexData(stream);
		}
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		if (base.Instance == 0)
		{
			m_usVersionAndInst = 51;
		}
		int count = m_arrProperties.Count;
		if (count > 0)
		{
			int id = (int)m_arrProperties[0].Id;
			int num = ((id > 0) ? 1 : 0);
			int num2 = id;
			bool flag = true;
			for (int i = 1; i < count; i++)
			{
				FOPTE fOPTE = m_arrProperties[i];
				if ((int)fOPTE.Id > id)
				{
					id = (int)fOPTE.Id;
					num++;
				}
				else
				{
					flag = false;
				}
			}
			int count2 = m_arrProperties.Count;
			if (num2 <= 4)
			{
				int id2 = (int)m_arrProperties[count2 - 1].Id;
				if ((id2 > 1000 || num != m_arrProperties.Count) && num > 10 && id2 > 100 && flag)
				{
					num--;
				}
			}
			base.Instance = num;
		}
		m_iLength = 0;
		for (int j = 0; j < count; j++)
		{
			byte[] mainData = m_arrProperties[j].MainData;
			int num3 = mainData.Length;
			stream.Write(mainData, 0, num3);
			m_iLength += num3;
		}
		for (int k = 0; k < count; k++)
		{
			FOPTE fOPTE2 = m_arrProperties[k];
			if (fOPTE2.AdditionalData != null)
			{
				byte[] mainData = fOPTE2.AdditionalData;
				int num4 = mainData.Length;
				stream.Write(mainData, 0, num4);
				m_iLength += num4;
			}
		}
	}

	public override object Clone()
	{
		return InternalClone();
	}

	protected override object InternalClone()
	{
		MsofbtOPT obj = (MsofbtOPT)base.InternalClone();
		obj.m_arrProperties = CloneUtils.CloneCloneable(m_arrProperties);
		return obj;
	}

	public void AddOptions(FOPTE option)
	{
		m_arrProperties.Add(option);
	}

	public void AddOptions(ICollection options)
	{
		foreach (FOPTE option in options)
		{
			AddOptions(option);
		}
	}

	public void AddOptionsOrReplace(FOPTE option)
	{
		int num = IndexOf(option);
		if (num == m_arrProperties.Count)
		{
			m_arrProperties.Add(option);
		}
		else
		{
			m_arrProperties[num] = option;
		}
	}

	public void AddOptionSorted(FOPTE option)
	{
		int i = 0;
		int count = m_arrProperties.Count;
		MsoOptions id = option.Id;
		for (int num = count; i < num && m_arrProperties[i].Id < id; i++)
		{
		}
		if (i < count)
		{
			if (m_arrProperties[i].Id == id)
			{
				m_arrProperties[i] = option;
			}
			else
			{
				m_arrProperties.Insert(i, option);
			}
		}
		else
		{
			m_arrProperties.Add(option);
		}
	}

	private int IndexOf(FOPTE option)
	{
		return IndexOf(option.Id);
	}

	public void RemoveOption(int index)
	{
		int i = 0;
		for (int count = m_arrProperties.Count; i < count; i++)
		{
			if (m_arrProperties[i].Id == (MsoOptions)index)
			{
				m_arrProperties.RemoveAt(i);
				break;
			}
		}
	}

	public int IndexOf(MsoOptions optionId)
	{
		int i = 0;
		for (int count = m_arrProperties.Count; i < count && m_arrProperties[i].Id != optionId; i++)
		{
		}
		return i;
	}
}

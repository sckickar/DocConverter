using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SupBook)]
[CLSCompliant(false)]
internal class SupBookRecord : BiffRecordWithContinue
{
	public const ushort INTERNAL_REFERENCE = 1025;

	public const ushort ADDIN_FUNCTION = 14849;

	private bool m_bIsInternal;

	private bool m_bIsAddInFunction;

	[BiffRecordPos(0, 2)]
	private ushort m_usSheetNumber;

	[BiffRecordPos(2, 2)]
	private ushort m_usUrlLength;

	private string m_strUrl;

	private List<string> m_arrSheetNames;

	private string m_strOriginalURL;

	public bool IsInternalReference
	{
		get
		{
			return m_bIsInternal;
		}
		set
		{
			m_bIsInternal = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize
	{
		get
		{
			if (m_bIsInternal)
			{
				return 4;
			}
			return base.MaximumRecordSize;
		}
	}

	public string URL
	{
		get
		{
			return m_strUrl;
		}
		set
		{
			m_strUrl = value;
			m_usUrlLength = (ushort)((m_strUrl != null) ? ((ushort)value.Length) : 0);
		}
	}

	public string OriginalURL
	{
		get
		{
			return m_strOriginalURL;
		}
		set
		{
			m_strOriginalURL = value;
		}
	}

	public List<string> SheetNames
	{
		get
		{
			return m_arrSheetNames;
		}
		set
		{
			m_arrSheetNames = value;
		}
	}

	public ushort SheetNumber
	{
		get
		{
			if (!IsInternalReference)
			{
				m_usSheetNumber = (ushort)((m_arrSheetNames != null) ? ((ushort)m_arrSheetNames.Count) : 0);
			}
			return m_usSheetNumber;
		}
		set
		{
			m_usSheetNumber = value;
			if (!IsInternalReference)
			{
				m_usSheetNumber = (ushort)((m_arrSheetNames != null) ? ((ushort)m_arrSheetNames.Count) : 0);
			}
		}
	}

	public bool IsAddInFunctions
	{
		get
		{
			return m_bIsAddInFunction;
		}
		set
		{
			m_bIsAddInFunction = value;
		}
	}

	public override void ParseStructure()
	{
		m_usSheetNumber = m_provider.ReadUInt16(0);
		m_usUrlLength = m_provider.ReadUInt16(2);
		m_bIsInternal = m_iLength == 4 && m_usUrlLength == 1025;
		if (m_bIsInternal || (m_bIsAddInFunction = m_iLength == 4 && m_usUrlLength == 14849))
		{
			return;
		}
		int num = 2;
		m_strOriginalURL = (m_strUrl = m_provider.ReadString16Bit(num, out var iFullLength));
		num += iFullLength;
		m_arrSheetNames = new List<string>(m_usSheetNumber);
		for (int i = 0; i < m_usSheetNumber; i++)
		{
			string item = m_provider.ReadString16BitUpdateOffset(ref num);
			m_arrSheetNames.Add(item);
			if (num > m_iLength)
			{
				throw new WrongBiffRecordDataException();
			}
		}
		if (num == m_iLength)
		{
			return;
		}
		throw new WrongBiffRecordDataException();
	}

	public override void InfillInternalData(OfficeVersion version)
	{
	}

	private void PrognoseRecordSize()
	{
		int num = 4;
		if (!m_bIsInternal && !m_bIsAddInFunction)
		{
			num += 3 + m_usUrlLength * 2;
			int num2 = num;
			if (m_arrSheetNames != null)
			{
				int i = 0;
				for (int count = m_arrSheetNames.Count; i < count; i++)
				{
					int num3 = m_arrSheetNames[i].Length * 2 + 3;
					if (num2 + num3 > 8224)
					{
						num += 4;
						num2 = 0;
					}
					num += num3;
					num2 += num3;
				}
			}
		}
		m_provider.EnsureCapacity(num);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (base.NeedInfill)
		{
			InfillInternalData(version);
			base.NeedInfill = false;
		}
		return m_iLength;
	}
}

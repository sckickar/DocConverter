using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExternSheet)]
[CLSCompliant(false)]
internal class ExternSheetRecord : BiffRecordRawWithArray
{
	public class TREF
	{
		public const int DEF_TREF_SIZE = 6;

		private ushort m_usSupBookIndex;

		private ushort m_usFirstSheet;

		private ushort m_usLastSheet;

		public ushort SupBookIndex
		{
			get
			{
				return m_usSupBookIndex;
			}
			set
			{
				m_usSupBookIndex = value;
			}
		}

		public ushort FirstSheet
		{
			get
			{
				return m_usFirstSheet;
			}
			set
			{
				m_usFirstSheet = value;
			}
		}

		public ushort LastSheet
		{
			get
			{
				return m_usLastSheet;
			}
			set
			{
				m_usLastSheet = value;
			}
		}

		public TREF(int supIndex, int firstSheet, int lastSheet)
		{
			FirstSheet = (ushort)firstSheet;
			LastSheet = (ushort)lastSheet;
			SupBookIndex = (ushort)supIndex;
		}
	}

	private const int DEF_FIXED_PART_SIZE = 2;

	public const int MaximumRefsCount = 1370;

	[BiffRecordPos(0, 2)]
	private ushort m_usRefCount;

	private List<TREF> m_arrRef;

	private ushort m_cXTI;

	public ushort RefCount
	{
		get
		{
			return m_usRefCount;
		}
		set
		{
			m_usRefCount = value;
		}
	}

	public TREF[] Refs
	{
		get
		{
			if (m_arrRef == null)
			{
				return null;
			}
			return m_arrRef.ToArray();
		}
		set
		{
			m_arrRef = new List<TREF>();
			m_arrRef.AddRange(value);
			m_usRefCount = (ushort)m_arrRef.Count;
		}
	}

	public List<TREF> RefList => m_arrRef;

	public override int MinimumRecordSize => 2;

	public ExternSheetRecord()
	{
	}

	public ExternSheetRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExternSheetRecord(int iReserve)
		: base(iReserve)
	{
	}

	public int AddReference(int supIndex, int firstSheet, int lastSheet)
	{
		if (m_arrRef != null)
		{
			int count = m_arrRef.Count;
			for (int i = 0; i < count; i++)
			{
				TREF tREF = m_arrRef[i];
				if (tREF.SupBookIndex == supIndex && tREF.FirstSheet == firstSheet && tREF.LastSheet == lastSheet)
				{
					return i;
				}
			}
		}
		TREF reference = new TREF(supIndex, firstSheet, lastSheet);
		AppendReference(reference);
		return m_usRefCount - 1;
	}

	public int GetBookReference(int iBookIndex)
	{
		int i = 0;
		for (int count = m_arrRef.Count; i < count; i++)
		{
			if (m_arrRef[i].SupBookIndex == iBookIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public void AppendReference(TREF reference)
	{
		if (m_arrRef == null)
		{
			m_arrRef = new List<TREF>();
		}
		m_arrRef.Add(reference);
		m_usRefCount++;
	}

	public void AppendReferences(IList<TREF> refs)
	{
		m_arrRef.AddRange(refs);
		m_usRefCount += (ushort)refs.Count;
	}

	public void PrependReferences(IList<TREF> refs)
	{
		m_arrRef.InsertRange(0, refs);
		m_usRefCount += (ushort)refs.Count;
	}

	public override object Clone()
	{
		ExternSheetRecord obj = (ExternSheetRecord)base.Clone();
		obj.m_arrRef = CloneUtils.CloneCloneable(m_arrRef);
		return obj;
	}

	public override void ParseStructure()
	{
		m_usRefCount = BitConverter.ToUInt16(m_data, 0);
		_ = m_iLength;
		_ = m_usRefCount * 6 + 2;
		m_arrRef = new List<TREF>(m_usRefCount);
		int num = 0;
		int num2 = 2;
		while (num < m_usRefCount)
		{
			if (num2 >= m_data.Length)
			{
				m_cXTI = m_usRefCount;
				m_usRefCount = (ushort)num;
				break;
			}
			TREF item = new TREF(GetUInt16(num2), GetUInt16(num2 + 2), GetUInt16(num2 + 4));
			m_arrRef.Add(item);
			num++;
			num2 += 6;
		}
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = GetStoreSize(OfficeVersion.Excel97to2003);
		m_data = new byte[m_iLength];
		if (m_cXTI != 0)
		{
			SetUInt16(0, m_cXTI);
		}
		else
		{
			SetUInt16(0, m_usRefCount);
		}
		if (m_arrRef != null)
		{
			int num = 0;
			int num2 = 2;
			int count = m_arrRef.Count;
			while (num < count)
			{
				SetUInt16(num2, m_arrRef[num].SupBookIndex);
				SetUInt16(num2 + 2, m_arrRef[num].FirstSheet);
				SetUInt16(num2 + 4, m_arrRef[num].LastSheet);
				num++;
				num2 += 6;
			}
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = ((m_arrRef != null) ? (m_arrRef.Count * 6) : 0);
		return 2 + num;
	}
}

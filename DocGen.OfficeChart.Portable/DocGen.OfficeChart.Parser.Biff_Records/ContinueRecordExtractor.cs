using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class ContinueRecordExtractor : IEnumerator
{
	private static readonly int[] DEF_RECORDS = new int[1] { 60 };

	private BinaryReader m_tmpReader;

	private long m_lStartPos;

	private BiffRecordRaw m_continue;

	private bool m_bReset;

	private List<int> m_arrAllowedRecords = new List<int>(DEF_RECORDS);

	private byte[] arrBuffer = new byte[4096];

	private DataProvider m_provider;

	protected bool IsStreamEOF
	{
		get
		{
			if (m_tmpReader == null)
			{
				throw new ArgumentNullException("m_tmpReader");
			}
			try
			{
				if (m_tmpReader.BaseStream.Position == m_tmpReader.BaseStream.Length)
				{
					return true;
				}
				if (PeekRecord() == null)
				{
					return true;
				}
				return false;
			}
			catch (Exception)
			{
				return true;
			}
		}
	}

	public BiffRecordRaw Current
	{
		get
		{
			if (m_tmpReader == null)
			{
				throw new ArgumentNullException("m_tmpReader");
			}
			if (m_continue == null || !m_bReset)
			{
				throw new ArgumentException("First call Reset method and then MoveNext. Wrong enumerator initialization.");
			}
			return m_continue;
		}
	}

	object IEnumerator.Current
	{
		get
		{
			if (m_tmpReader == null)
			{
				throw new ArgumentNullException("m_tmpReader");
			}
			if (m_continue == null || !m_bReset)
			{
				throw new ArgumentException("First call Reset method and then MoveNext. Wrong enumerator initialization.");
			}
			return m_continue;
		}
	}

	private ContinueRecordExtractor()
	{
	}

	protected BiffRecordRaw PeekRecord()
	{
		if (m_tmpReader == null)
		{
			throw new ArgumentNullException("m_tmpReader");
		}
		long position = m_tmpReader.BaseStream.Position;
		BiffRecordRaw untypedRecord = BiffRecordFactory.GetUntypedRecord(m_tmpReader);
		m_tmpReader.BaseStream.Position = position;
		return untypedRecord;
	}

	public long StoreStreamPosition()
	{
		m_lStartPos = m_tmpReader.BaseStream.Position;
		m_continue = null;
		m_bReset = false;
		return m_lStartPos;
	}

	public void AddRecordType(TBIFFRecord recordType)
	{
		if (m_arrAllowedRecords.IndexOf((int)recordType) == -1)
		{
			m_arrAllowedRecords.Add((int)recordType);
		}
	}

	void IEnumerator.Reset()
	{
		if (m_tmpReader == null)
		{
			throw new ArgumentNullException("m_tmpReader");
		}
		m_tmpReader.BaseStream.Position = m_lStartPos;
		m_bReset = true;
	}

	bool IEnumerator.MoveNext()
	{
		if (m_tmpReader == null)
		{
			throw new ArgumentNullException("m_tmpReader");
		}
		if (!IsStreamEOF)
		{
			int num = m_tmpReader.ReadInt16();
			int num2 = m_tmpReader.ReadInt16();
			if (m_arrAllowedRecords.IndexOf(num) != -1)
			{
				m_continue = BiffRecordFactory.GetRecord(num);
				m_continue.Length = num2;
				byte[] data = m_tmpReader.ReadBytes(num2);
				m_continue.Data = data;
				m_bReset = true;
				return true;
			}
			m_tmpReader.BaseStream.Position -= 4L;
		}
		m_continue = null;
		m_bReset = false;
		return false;
	}
}

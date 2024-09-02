using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.FilePass)]
internal class FilePassRecord : BiffRecordRaw
{
	internal const int DEF_STANDARD_HASH = 1;

	internal const int DEF_STRONG_HASH = 2;

	private ushort m_usNotWeakEncryption;

	private ushort m_usKey;

	private ushort m_usHash;

	private FilePassStandardBlock m_standardBlock;

	private FilePassStrongBlock m_strongBlock;

	public bool IsWeakEncryption
	{
		get
		{
			return m_usNotWeakEncryption == 0;
		}
		set
		{
			m_usNotWeakEncryption = ((!value) ? ((ushort)1) : ((ushort)0));
		}
	}

	[CLSCompliant(false)]
	public ushort Key
	{
		get
		{
			return m_usKey;
		}
		set
		{
			m_usKey = value;
		}
	}

	[CLSCompliant(false)]
	public ushort Hash
	{
		get
		{
			return m_usHash;
		}
		set
		{
			m_usHash = value;
		}
	}

	public FilePassStandardBlock StandardBlock => m_standardBlock;

	public override bool NeedDecoding => false;

	public FilePassRecord()
	{
	}

	public FilePassRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FilePassRecord(int iReserve)
		: base(iReserve)
	{
	}

	public void CreateStandardBlock()
	{
		m_standardBlock = new FilePassStandardBlock();
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		m_usNotWeakEncryption = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usKey = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usHash = provider.ReadUInt16(iOffset);
		iOffset += 2;
		if (!IsWeakEncryption)
		{
			switch (m_usHash)
			{
			case 1:
				m_standardBlock = new FilePassStandardBlock();
				m_standardBlock.ParseStructure(provider, iOffset, iLength);
				break;
			case 2:
				m_strongBlock = new FilePassStrongBlock();
				m_strongBlock.ParseStructure(provider, iOffset, iLength);
				break;
			default:
				throw new ParseException("Cannot parse FilePass record");
			}
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usNotWeakEncryption);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usKey);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usHash);
		iOffset += 2;
		if (!IsWeakEncryption)
		{
			if (m_usHash != 1)
			{
				throw new NotImplementedException();
			}
			m_standardBlock.InfillInternalData(provider, iOffset, int.MaxValue);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 6;
		if (!IsWeakEncryption)
		{
			if (m_usHash != 1)
			{
				throw new NotImplementedException();
			}
			num += FilePassStandardBlock.GetStoreSize(version);
		}
		return num;
	}
}

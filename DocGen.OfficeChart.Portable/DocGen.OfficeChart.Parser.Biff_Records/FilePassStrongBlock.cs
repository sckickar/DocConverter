using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class FilePassStrongBlock
{
	private uint m_uiOptions;

	private uint m_uiReserved;

	private uint m_uiStreamEncryption;

	private uint m_uiPassword;

	private uint m_uiHashKeyLength;

	private uint m_uiCryptographicProvider;

	private byte[] m_arrUnknown = new byte[8];

	private string m_strProviderName;

	private byte[] m_arrDocumentId;

	private byte[] m_arrEncryptedDocumentId;

	private byte[] m_arrDigest;

	public uint Options
	{
		get
		{
			return m_uiOptions;
		}
		set
		{
			m_uiOptions = value;
		}
	}

	public uint Reserved
	{
		get
		{
			return m_uiReserved;
		}
		set
		{
			m_uiReserved = value;
		}
	}

	public uint StreamEncryption
	{
		get
		{
			return m_uiStreamEncryption;
		}
		set
		{
			m_uiStreamEncryption = value;
		}
	}

	public uint Password
	{
		get
		{
			return m_uiPassword;
		}
		set
		{
			m_uiPassword = value;
		}
	}

	public uint HashKeyLength
	{
		get
		{
			return m_uiHashKeyLength;
		}
		set
		{
			m_uiHashKeyLength = value;
		}
	}

	public uint CryptographicProvider
	{
		get
		{
			return m_uiCryptographicProvider;
		}
		set
		{
			m_uiCryptographicProvider = value;
		}
	}

	public byte[] UnknownData => m_arrUnknown;

	public string ProviderName
	{
		get
		{
			return m_strProviderName;
		}
		set
		{
			m_strProviderName = value;
		}
	}

	public byte[] Digest => m_arrDigest;

	public int ParseStructure(DataProvider provider, int iOffset, int iLength)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		m_uiOptions = provider.ReadUInt32(iOffset);
		iOffset += 4;
		uint num = provider.ReadUInt32(iOffset);
		num += 4;
		int num2 = iOffset;
		m_uiOptions = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiReserved = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiStreamEncryption = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiPassword = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiHashKeyLength = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiCryptographicProvider = provider.ReadUInt32(iOffset);
		iOffset += 4;
		iOffset = provider.ReadArray(iOffset, m_arrUnknown);
		iOffset = num2 + (int)num;
		num = provider.ReadUInt32(iOffset);
		num += 4;
		m_arrDocumentId = new byte[num];
		m_arrEncryptedDocumentId = new byte[num];
		iOffset = provider.ReadArray(iOffset, m_arrDocumentId);
		iOffset = provider.ReadArray(iOffset, m_arrEncryptedDocumentId);
		num = provider.ReadUInt32(iOffset);
		num += 4;
		throw new NotSupportedException("Strong encryption algorithms are not supported.");
	}
}

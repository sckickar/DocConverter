using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class FilePassStandardBlock
{
	private byte[] m_arrDocumentID = new byte[16];

	private byte[] m_arrEncyptedDocumentID = new byte[16];

	private byte[] m_arrDigest = new byte[16];

	public const int StoreSize = 48;

	public byte[] DocumentID => m_arrDocumentID;

	public byte[] EncyptedDocumentID => m_arrEncyptedDocumentID;

	public byte[] Digest => m_arrDigest;

	public void ParseStructure(DataProvider provider, int iOffset, int iLength)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		iOffset = provider.ReadArray(iOffset, m_arrDocumentID);
		iOffset = provider.ReadArray(iOffset, m_arrEncyptedDocumentID);
		iOffset = provider.ReadArray(iOffset, m_arrDigest);
	}

	public void InfillInternalData(DataProvider provider, int iOffset, int iLength)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		int num = m_arrDocumentID.Length;
		provider.WriteBytes(iOffset, m_arrDocumentID, 0, num);
		iOffset += num;
		num = m_arrEncyptedDocumentID.Length;
		provider.WriteBytes(iOffset, m_arrEncyptedDocumentID, 0, num);
		iOffset += num;
		num = m_arrDigest.Length;
		provider.WriteBytes(iOffset, m_arrDigest, 0, num);
		iOffset += num;
	}

	public static int GetStoreSize(OfficeVersion version)
	{
		return 48;
	}
}

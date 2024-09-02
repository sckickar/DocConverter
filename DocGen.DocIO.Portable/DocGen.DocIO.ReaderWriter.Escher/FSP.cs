using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class FSP : BaseWordRecord
{
	private uint m_spid;

	private uint m_grfPersistent;

	public uint Spid
	{
		get
		{
			return m_spid;
		}
		set
		{
			m_spid = value;
		}
	}

	public uint GzfPersistent
	{
		get
		{
			return m_grfPersistent;
		}
		set
		{
			m_grfPersistent = value;
		}
	}

	public bool IsGroup => (m_grfPersistent & 1) == 1;

	public bool ISChild => (m_grfPersistent & 2) == 1;

	public bool IsPatriarch => (m_grfPersistent & 4) == 1;

	public bool IsDeleted => (m_grfPersistent & 8) == 1;

	public bool IsOleShape => (m_grfPersistent & 0x10) == 1;

	public bool IsHaveMaster => (m_grfPersistent & 0x20) == 1;

	public bool IsFliph => (m_grfPersistent & 0x40) == 1;

	public bool IsFlipv => (m_grfPersistent & 0x80) == 1;

	public bool IsConnector => (m_grfPersistent & 0x100) == 1;

	public bool IsHaveAnchor => (m_grfPersistent & 0x200) == 1;

	public bool IsBackground => (m_grfPersistent & 0x400) == 1;

	public bool IsHavespt => (m_grfPersistent & 0x800) == 1;

	public void Read(Stream stream)
	{
		m_spid = BaseWordRecord.ReadUInt32(stream);
		m_grfPersistent = BaseWordRecord.ReadUInt32(stream);
	}

	public void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt32(stream, m_spid);
		BaseWordRecord.WriteUInt32(stream, m_grfPersistent);
	}
}

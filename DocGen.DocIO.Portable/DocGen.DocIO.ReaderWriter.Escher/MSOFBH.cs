using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class MSOFBH : BaseWordRecord
{
	private uint m_ver;

	private uint m_inst;

	private MSOFBT m_fbt;

	private uint m_cbLength;

	public uint Version
	{
		get
		{
			return m_ver;
		}
		set
		{
			m_ver = value;
		}
	}

	public uint Inst
	{
		get
		{
			return m_inst;
		}
		set
		{
			m_inst = value;
		}
	}

	internal MSOFBT Msofbt
	{
		get
		{
			return m_fbt;
		}
		set
		{
			m_fbt = value;
		}
	}

	public new uint Length
	{
		get
		{
			return m_cbLength;
		}
		set
		{
			m_cbLength = value;
		}
	}

	public void Read(Stream stream)
	{
		uint num = BaseWordRecord.ReadUInt32(stream);
		m_ver = num & 0xFu;
		m_inst = (num & 0xFFF0) >> 4;
		m_fbt = (MSOFBT)((num & 0xFFFF0000u) >> 16);
		m_cbLength = BaseWordRecord.ReadUInt32(stream);
	}

	public void Write(Stream stream)
	{
		uint ver = m_ver;
		ver += m_inst << 4;
		ver += (uint)((int)m_fbt << 16);
		BaseWordRecord.WriteUInt32(stream, ver);
		BaseWordRecord.WriteUInt32(stream, m_cbLength);
	}
}

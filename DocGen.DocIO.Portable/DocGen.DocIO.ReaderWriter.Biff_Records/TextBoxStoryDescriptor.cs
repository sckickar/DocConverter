using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class TextBoxStoryDescriptor : BaseWordRecord
{
	internal static int DEF_TXBX_LENGTH = 22;

	private int m_cTxbxAndiNextReuse;

	private int m_cReusable;

	private bool m_fReusable;

	private uint m_reserved;

	private int m_lid;

	private int m_txidUndo;

	internal int TextBoxCnt
	{
		get
		{
			return m_cTxbxAndiNextReuse;
		}
		set
		{
			m_cTxbxAndiNextReuse = value;
		}
	}

	internal int ReusableCnt
	{
		get
		{
			return m_cReusable;
		}
		set
		{
			m_cReusable = value;
		}
	}

	internal bool IsReusable
	{
		get
		{
			return m_fReusable;
		}
		set
		{
			m_fReusable = value;
		}
	}

	internal int ShapeIdent
	{
		get
		{
			return m_lid;
		}
		set
		{
			m_lid = value;
		}
	}

	internal uint Reserved
	{
		get
		{
			return m_reserved;
		}
		set
		{
			m_reserved = value;
		}
	}

	internal TextBoxStoryDescriptor()
	{
	}

	internal TextBoxStoryDescriptor(Stream stream)
	{
		Read(stream);
	}

	internal void Read(Stream stream)
	{
		m_cTxbxAndiNextReuse = BaseWordRecord.ReadInt32(stream);
		m_cReusable = BaseWordRecord.ReadInt32(stream);
		m_fReusable = BaseWordRecord.ReadInt16(stream) == 1;
		m_reserved = BaseWordRecord.ReadUInt32(stream);
		m_lid = BaseWordRecord.ReadInt32(stream);
		m_txidUndo = BaseWordRecord.ReadInt32(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_cTxbxAndiNextReuse);
		BaseWordRecord.WriteInt32(stream, m_cReusable);
		if (m_fReusable)
		{
			BaseWordRecord.WriteInt16(stream, 1);
		}
		else
		{
			BaseWordRecord.WriteInt16(stream, 0);
		}
		BaseWordRecord.WriteInt32(stream, (int)m_reserved);
		BaseWordRecord.WriteInt32(stream, m_lid);
		BaseWordRecord.WriteInt32(stream, m_txidUndo);
	}
}

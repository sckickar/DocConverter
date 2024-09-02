using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class _FBSE : BaseWordRecord
{
	public const int DEF_GUID_LENGTH = 16;

	public const int DEF_FBSE_LENGTH = 36;

	internal int m_btWin32;

	internal int m_btMacOS;

	internal byte[] m_rgbUid;

	internal int m_tag;

	internal int m_size;

	internal int m_cRef;

	internal int m_foDelay;

	internal int m_usage;

	internal int m_cbName;

	internal int m_unused2;

	internal int m_unused3;

	public void Read(Stream stream)
	{
		m_btWin32 = stream.ReadByte();
		m_btMacOS = stream.ReadByte();
		m_rgbUid = ReadBytes(stream, 16);
		m_tag = BaseWordRecord.ReadInt16(stream);
		m_size = BaseWordRecord.ReadInt32(stream);
		m_cRef = BaseWordRecord.ReadInt32(stream);
		m_foDelay = BaseWordRecord.ReadInt32(stream);
		m_usage = stream.ReadByte();
		m_cbName = stream.ReadByte();
		m_unused2 = stream.ReadByte();
		m_unused3 = stream.ReadByte();
		if (m_cbName > 0)
		{
			throw new NotImplementedException("A BLIP with a name was found.");
		}
	}

	public void Write(Stream stream)
	{
		stream.WriteByte((byte)m_btWin32);
		stream.WriteByte((byte)m_btMacOS);
		stream.Write(m_rgbUid, 0, 16);
		BaseWordRecord.WriteInt16(stream, (short)m_tag);
		BaseWordRecord.WriteInt32(stream, m_size);
		BaseWordRecord.WriteInt32(stream, m_cRef);
		BaseWordRecord.WriteInt32(stream, m_foDelay);
		stream.WriteByte((byte)m_usage);
		stream.WriteByte((byte)m_cbName);
		stream.WriteByte((byte)m_unused2);
		stream.WriteByte((byte)m_unused3);
	}

	public _FBSE Clone()
	{
		_FBSE fBSE = (_FBSE)MemberwiseClone();
		fBSE.m_rgbUid = new byte[m_rgbUid.Length];
		m_rgbUid.CopyTo(fBSE.m_rgbUid, 0);
		return fBSE;
	}

	internal bool Compare(_FBSE fbse)
	{
		if (!m_btMacOS.Equals(fbse.m_btMacOS))
		{
			return false;
		}
		if (!m_btWin32.Equals(fbse.m_btWin32))
		{
			return false;
		}
		if (!WordDocument.CompareArray(m_rgbUid, fbse.m_rgbUid))
		{
			return false;
		}
		if (!m_tag.Equals(fbse.m_tag))
		{
			return false;
		}
		if (!m_size.Equals(fbse.m_size))
		{
			return false;
		}
		if (!m_cRef.Equals(fbse.m_cRef))
		{
			return false;
		}
		if (!m_foDelay.Equals(fbse.m_foDelay))
		{
			return false;
		}
		if (!m_usage.Equals(fbse.m_usage))
		{
			return false;
		}
		if (!m_cbName.Equals(fbse.m_cbName))
		{
			return false;
		}
		if (!m_unused2.Equals(fbse.m_unused2))
		{
			return false;
		}
		if (!m_unused3.Equals(fbse.m_unused3))
		{
			return false;
		}
		return true;
	}
}

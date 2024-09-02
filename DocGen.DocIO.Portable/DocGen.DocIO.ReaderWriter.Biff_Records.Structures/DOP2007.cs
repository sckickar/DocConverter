using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP2007
{
	private ushort m_flagsA = 1059;

	private DopMth m_dopMath;

	private DOPDescriptor m_dopBase;

	internal bool RMTrackFormatting
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool RMTrackMoves
	{
		get
		{
			return (m_flagsA & 2) >> 1 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal byte Ssm
	{
		get
		{
			return (byte)((m_flagsA & 0x1E0) >> 5);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFE1Fu) | (uint)(value << 5));
		}
	}

	internal bool ReadingModeInkLockDownActualPage
	{
		get
		{
			return (m_flagsA & 0x200) >> 9 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	internal bool AutoCompressPictures
	{
		get
		{
			return (m_flagsA & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal DopMth DopMath
	{
		get
		{
			if (m_dopMath == null)
			{
				m_dopMath = new DopMth();
			}
			return m_dopMath;
		}
	}

	internal DOP2007(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		BaseWordRecord.ReadUInt32(stream);
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt16(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		BaseWordRecord.ReadUInt32(stream);
		DopMath.Parse(stream);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, 0);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		BaseWordRecord.WriteUInt32(stream, 0u);
		DopMath.Write(stream);
	}
}

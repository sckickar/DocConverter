using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundStreamDirect : CompoundStreamNet, ICompoundItem
{
	private const int MinimumSize = 32768;

	private long m_lPosition;

	public override long Length
	{
		get
		{
			if (base.Stream == null)
			{
				return base.Entry.Size;
			}
			return base.Stream.Length;
		}
	}

	public override long Position
	{
		get
		{
			return m_lPosition;
		}
		set
		{
			m_lPosition = value;
			if (base.Stream != null)
			{
				base.Stream.Position = value;
			}
		}
	}

	public CompoundStreamDirect(CompoundFile file, DirectoryEntry entry)
		: base(file, entry)
	{
	}

	public override void Open()
	{
		if (base.Entry.Size < 32768)
		{
			base.Open();
		}
		m_lPosition = 0L;
	}

	public override int Read(byte[] buffer, int offset, int length)
	{
		int num = ((base.Stream == null) ? base.ParentFile.ReadData(base.Entry, m_lPosition, buffer, length) : base.Read(buffer, offset, length));
		m_lPosition += num;
		return num;
	}

	public override void Write(byte[] buffer, int offset, int length)
	{
		if (base.Stream == null)
		{
			base.ParentFile.WriteData(base.Entry, m_lPosition, buffer, offset, length);
		}
		else
		{
			base.Write(buffer, offset, length);
			if (base.Stream.Length > 32768)
			{
				base.ParentFile.SetEntryStream(base.Entry, base.Stream);
				base.Stream = null;
			}
		}
		m_lPosition += length;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (base.Stream != null)
		{
			base.Stream.Seek(offset, origin);
		}
		switch (origin)
		{
		case SeekOrigin.Begin:
			m_lPosition = offset;
			break;
		case SeekOrigin.Current:
			m_lPosition += offset;
			break;
		case SeekOrigin.End:
			m_lPosition = base.Entry.Size + offset;
			break;
		}
		return m_lPosition;
	}

	public override void SetLength(long value)
	{
		if (base.Stream != null)
		{
			base.SetLength(value);
		}
		base.Entry.Size = (uint)value;
	}

	public override void Flush()
	{
		if (base.Stream != null)
		{
			base.Flush();
		}
	}
}

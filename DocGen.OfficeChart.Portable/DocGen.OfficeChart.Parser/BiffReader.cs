using System;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Parser;

[CLSCompliant(false)]
internal class BiffReader : IDisposable
{
	private const int DEF_BUFFER_SIZE = 262144;

	private const int BIFF8_VERSION = 1536;

	private Stream m_stream;

	private BinaryReader m_reader;

	private bool m_bDisposed;

	private bool m_bDestroyStream;

	private int m_iMinimalVersion = 1536;

	private byte[] m_arrBuffer = new byte[8228];

	private DataProvider m_dataProvider;

	public Stream BaseStream => m_stream;

	public BinaryReader BaseReader => m_reader;

	public int MinimalVersion
	{
		get
		{
			return m_iMinimalVersion;
		}
		set
		{
			m_iMinimalVersion = value;
		}
	}

	public byte[] Buffer => m_arrBuffer;

	public DataProvider DataProvider => m_dataProvider;

	public bool IsEOF
	{
		get
		{
			if (m_stream == null)
			{
				throw new ArgumentNullException("internal stream");
			}
			try
			{
				if (m_stream.Position == m_stream.Length)
				{
					return true;
				}
				short num = m_reader.ReadInt16();
				m_reader.BaseStream.Position -= 2L;
				return num == 0;
			}
			catch (Exception)
			{
				return true;
			}
		}
	}

	private BiffReader()
	{
		m_dataProvider = new ByteArrayDataProvider(m_arrBuffer);
	}

	public BiffReader(Stream stream)
		: this()
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = stream;
		m_reader = new BinaryReader(m_stream);
	}

	public BiffReader(Stream stream, bool bControlStream)
		: this(stream)
	{
		m_bDestroyStream = bControlStream;
	}

	public void Dispose()
	{
		if (!m_bDisposed)
		{
			m_bDisposed = true;
			if (m_bDestroyStream)
			{
				((IDisposable)m_reader).Dispose();
				((IDisposable)m_stream).Dispose();
			}
			m_stream = null;
			m_reader = null;
			m_arrBuffer = null;
			if (m_dataProvider != null)
			{
				m_dataProvider.Dispose();
				m_dataProvider = null;
			}
		}
	}

	public BiffRecordRaw GetRecord()
	{
		if (m_stream == null)
		{
			throw new ArgumentNullException("internal stream");
		}
		return null;
	}

	public BiffRecordRaw PeekRecord()
	{
		if (m_stream == null)
		{
			throw new ArgumentNullException("internal stream");
		}
		long position = m_stream.Position;
		BiffRecordRaw record = GetRecord();
		m_stream.Position = position;
		return record;
	}

	public TBIFFRecord PeekRecordType()
	{
		if (m_stream == null)
		{
			throw new ArgumentNullException("internal stream");
		}
		long position = m_stream.Position;
		int result = BiffRecordFactory.ExtractRecordType(m_reader);
		m_stream.Position = position;
		return (TBIFFRecord)result;
	}

	protected BiffRecordRaw TestPeekRecord()
	{
		if (m_stream == null)
		{
			throw new ArgumentNullException("internal stream");
		}
		Stream baseStream = m_reader.BaseStream;
		long position = baseStream.Position;
		BiffRecordRaw result = null;
		if (m_reader.ReadInt16() > 0)
		{
			result = UnknownRecord.Empty;
		}
		baseStream.Position = position;
		return result;
	}
}

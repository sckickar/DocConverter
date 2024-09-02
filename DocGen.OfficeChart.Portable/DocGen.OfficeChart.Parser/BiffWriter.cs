using System;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Parser;

[CLSCompliant(false)]
internal class BiffWriter : IDisposable
{
	private const int DEF_BUFFER_SIZE = 1048576;

	private Stream m_stream;

	private bool m_bDisposed;

	private bool m_bDestroyStream;

	private BinaryWriter m_writer;

	private byte[] m_arrBuffer = new byte[8228];

	private ByteArrayDataProvider m_provider;

	public Stream BaseStream => m_stream;

	public byte[] Buffer => m_arrBuffer;

	private BiffWriter()
	{
		m_provider = new ByteArrayDataProvider(m_arrBuffer);
	}

	public BiffWriter(Stream stream)
		: this(stream, bControlsStream: false)
	{
	}

	public BiffWriter(Stream stream, bool bControlsStream)
	{
		m_provider = new ByteArrayDataProvider(m_arrBuffer);
		m_bDestroyStream = bControlsStream;
		m_stream = stream;
		m_writer = new BinaryWriter(m_stream);
	}

	public void Dispose()
	{
		if (!m_bDisposed)
		{
			m_bDisposed = true;
			m_writer.Flush();
			if (m_bDestroyStream)
			{
				m_stream.SetLength(m_stream.Position);
				((IDisposable)m_stream).Dispose();
			}
			m_stream = null;
			m_provider = null;
		}
	}
}

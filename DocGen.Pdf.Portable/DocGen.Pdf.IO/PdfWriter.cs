using System;
using System.IO;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class PdfWriter : IPdfWriter, IDisposable
{
	internal Stream m_stream;

	private PdfDocumentBase m_document;

	private bool m_cannotSeek;

	private long m_position;

	private long m_length;

	internal bool isCompress;

	private Stream Stream => ObtainStream();

	public PdfDocumentBase Document
	{
		get
		{
			return m_document;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Document");
			}
			m_document = value;
		}
	}

	public long Position
	{
		get
		{
			if (m_cannotSeek)
			{
				return m_position;
			}
			return m_stream.Position;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("Position", "The stream position can't be less then zero.");
			}
			m_stream.Position = value;
		}
	}

	public long Length
	{
		get
		{
			if (m_cannotSeek)
			{
				return m_length;
			}
			return m_stream.Length;
		}
	}

	internal PdfWriter(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException("Can't write to the specified stream", "stream");
		}
		m_stream = stream;
		if (!stream.CanRead || !stream.CanSeek)
		{
			m_cannotSeek = true;
		}
	}

	public void Dispose()
	{
		Close();
	}

	internal void Close()
	{
		if (m_stream != null)
		{
			m_stream.Flush();
			m_stream = null;
		}
	}

	public void Write(IPdfPrimitive pdfObject)
	{
		pdfObject.Save(this);
	}

	public void Write(long number)
	{
		new PdfNumber(number).Save(this);
	}

	public void Write(float number)
	{
		new PdfNumber(number).Save(this);
	}

	public void Write(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		Write(bytes);
	}

	public void Write(char[] text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		Write(bytes);
	}

	public void Write(byte[] data)
	{
		Stream stream = ObtainStream();
		int num = data.Length;
		m_length += num;
		m_position += num;
		stream.Write(data, 0, num);
	}

	internal void Write(byte[] data, int end)
	{
		Stream stream = ObtainStream();
		m_length += end;
		m_position += end;
		stream.Write(data, 0, end);
	}

	internal Stream ObtainStream()
	{
		return m_stream;
	}
}

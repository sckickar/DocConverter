using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSound : IPdfWrapper
{
	private int m_rate = 22050;

	private PdfSoundEncoding m_encoding;

	private PdfSoundChannels m_channels = PdfSoundChannels.Mono;

	private int m_bits = 8;

	private string m_fileName = string.Empty;

	private PdfStream m_stream = new PdfStream();

	private Stream m_data;

	public int Rate
	{
		get
		{
			return m_rate;
		}
		set
		{
			if (m_rate != value)
			{
				m_rate = value;
				m_stream.SetNumber("R", m_rate);
			}
		}
	}

	public int Bits
	{
		get
		{
			return m_bits;
		}
		set
		{
			if (m_bits != value)
			{
				m_bits = value;
				m_stream.SetNumber("B", m_bits);
			}
		}
	}

	public PdfSoundEncoding Encoding
	{
		get
		{
			return m_encoding;
		}
		set
		{
			if (m_encoding != value)
			{
				m_encoding = value;
				m_stream.SetName("E", m_encoding.ToString());
			}
		}
	}

	public PdfSoundChannels Channels
	{
		get
		{
			return m_channels;
		}
		set
		{
			if (m_channels != value)
			{
				m_channels = value;
				m_stream.SetNumber("C", (int)m_channels);
			}
		}
	}

	public string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArithmeticException("FileName can't be empty string.");
			}
			m_fileName = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_stream;

	internal PdfSound(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileName = fileName;
		m_stream.SetString("T", fileName);
		m_stream.SetProperty("R", new PdfNumber(m_rate));
		m_stream.BeginSave += Stream_BeginSave;
	}

	public PdfSound(Stream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("fileName");
		}
		m_data = data;
		m_stream.SetProperty("R", new PdfNumber(m_rate));
		m_stream.BeginSave += Stream_BeginSave;
	}

	internal PdfSound(string fileName, bool test)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileName = fileName;
		m_stream.SetString("T", fileName);
		m_stream.SetProperty("R", new PdfNumber(m_rate));
		m_stream.BeginSave += Stream_BeginSave;
	}

	internal PdfSound()
	{
		m_stream.SetProperty("R", new PdfNumber(m_rate));
		m_stream.BeginSave += Stream_BeginSave;
	}

	private void Stream_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected void Save()
	{
		byte[] array = PdfStream.StreamToBigEndian(m_data);
		m_stream.Clear();
		m_stream.InternalStream.Write(array, 0, array.Length);
	}
}

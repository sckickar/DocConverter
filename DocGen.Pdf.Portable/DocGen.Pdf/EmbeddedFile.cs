using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class EmbeddedFile : IPdfWrapper
{
	private string m_fileName = string.Empty;

	private string m_filePath = string.Empty;

	private string m_mimeType = string.Empty;

	private byte[] m_data;

	private EmbeddedFileParams m_params = new EmbeddedFileParams();

	private PdfStream m_stream = new PdfStream();

	public string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			if (m_fileName != value)
			{
				m_fileName = GetFileName(value);
			}
		}
	}

	internal string FilePath
	{
		get
		{
			return m_filePath;
		}
		set
		{
			if (m_filePath != value)
			{
				m_filePath = value;
			}
		}
	}

	public byte[] Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public string MimeType
	{
		get
		{
			return m_mimeType;
		}
		set
		{
			if (m_mimeType != value)
			{
				m_mimeType = value;
				m_stream.SetName("Subtype", m_mimeType, processSpecialCharacters: true);
			}
		}
	}

	internal EmbeddedFileParams Params => m_params;

	public IPdfPrimitive Element => m_stream;

	internal EmbeddedFile(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		Initialize();
		FileName = fileName;
		FilePath = fileName;
	}

	public EmbeddedFile(string fileName, byte[] data)
		: this(fileName)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Data = data;
	}

	public EmbeddedFile(string fileName, Stream stream)
		: this(fileName)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int num = (int)stream.Length;
		int num2 = 0;
		int num3 = 0;
		m_data = new byte[stream.Length];
		while (num > 0)
		{
			num3 = stream.Read(m_data, num2, num);
			num2 += num3;
			num -= num3;
		}
		m_stream.InternalStream.Write(m_data, 0, m_data.Length);
	}

	protected void Initialize()
	{
		m_stream.SetProperty("Type", new PdfName("EmbeddedFile"));
		m_stream.SetProperty("Params", m_params);
		m_stream.BeginSave += Stream_BeginSave;
	}

	protected void Save()
	{
		bool flag = false;
		m_stream.Clear();
		m_stream.Compress = false;
		if (m_data != null)
		{
			m_stream.InternalStream.Write(m_data, 0, m_data.Length);
			if (flag)
			{
				m_stream.AddFilter("FlateDecode");
			}
			m_params.Size = m_data.Length;
		}
	}

	private void Stream_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	private string GetFileName(string attachmentName)
	{
		char[] separator = new char[2] { '\\', '/' };
		return attachmentName.Split(separator)[^1];
	}
}

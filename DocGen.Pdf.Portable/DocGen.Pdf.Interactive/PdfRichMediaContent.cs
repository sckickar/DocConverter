using System;
using System.IO;
using DocGen.Pdf.Graphics.Images.Decoder;

namespace DocGen.Pdf.Interactive;

public class PdfRichMediaContent
{
	private Stream m_data;

	private PdfRichMediaContentType m_type;

	internal PdfEmbeddedFileSpecification m_fileSpecification;

	internal bool isInternalLoad;

	private string m_name = string.Empty;

	private string m_stringType;

	private string m_fileExtension = "mp4";

	public Stream Data
	{
		get
		{
			if (m_fileSpecification != null && m_data == null)
			{
				m_data = new MemoryStream(m_fileSpecification.Data);
			}
			return m_data;
		}
	}

	public PdfRichMediaContentType ContentType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public string FileExtension
	{
		get
		{
			return m_fileExtension;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_fileExtension = value;
				if (m_fileSpecification != null && !m_fileSpecification.FileName.EndsWith("." + value))
				{
					m_fileSpecification.FileName = m_fileSpecification.FileName + "." + ValidateFileExtension(m_fileExtension);
				}
			}
		}
	}

	public string FileName
	{
		get
		{
			return m_name;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_name = value;
				if (m_fileSpecification != null)
				{
					m_fileSpecification.FileName = m_name + "." + ValidateFileExtension(m_fileExtension);
				}
			}
		}
	}

	public PdfRichMediaContent(string name, Stream mediaFileStream, string fileExtension)
	{
		if (mediaFileStream == null)
		{
			throw new ArgumentNullException("mediaFileStream");
		}
		if (string.IsNullOrEmpty(fileExtension))
		{
			throw new ArgumentNullException("fileExtension");
		}
		m_fileExtension = ValidateFileExtension(fileExtension);
		m_fileSpecification = new PdfEmbeddedFileSpecification(name + "." + m_fileExtension, mediaFileStream);
		m_type = GetRichMediaContentType(mediaFileStream);
		m_name = name;
	}

	public PdfRichMediaContent(string name, Stream mediaFileStream, string fileExtension, PdfRichMediaContentType type)
	{
		if (mediaFileStream == null)
		{
			throw new ArgumentNullException("mediaFileStream");
		}
		m_fileExtension = ValidateFileExtension(fileExtension);
		m_fileSpecification = new PdfEmbeddedFileSpecification(name + "." + m_fileExtension, mediaFileStream);
		m_type = type;
		m_name = name;
	}

	internal PdfRichMediaContent(string mediaFile, Stream file, bool isInternal)
	{
		m_type = GetRichMediaContentType(file);
		m_data = file;
		isInternalLoad = isInternal;
		m_name = TrimFileExtension(mediaFile);
		m_fileExtension = GetFileExtension(mediaFile);
	}

	private PdfRichMediaContentType GetRichMediaContentType(Stream mediaFile)
	{
		PdfRichMediaContentType result = PdfRichMediaContentType.Video;
		mediaFile.Position = 0L;
		if (mediaFile.IsMP3())
		{
			result = PdfRichMediaContentType.Sound;
			mediaFile.Position = 0L;
		}
		return result;
	}

	private string ValidateFileExtension(string fileExtension)
	{
		char[] trimChars = new char[1] { '.' };
		return fileExtension.TrimStart(trimChars).ToLower();
	}

	private string TrimFileExtension(string fileName)
	{
		char[] separator = new char[1] { '.' };
		string[] array = fileName.Split(separator);
		if (array.Length >= 2)
		{
			return array[^2];
		}
		return fileName;
	}

	private string GetFileExtension(string fileName)
	{
		char[] separator = new char[1] { '.' };
		string[] array = fileName.Split(separator);
		if (array.Length >= 0)
		{
			return array[^1];
		}
		return null;
	}
}

using System.IO;
using DocGen.DocIO.DLS.Entities;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ImageRecord
{
	private int m_imageId;

	internal byte[] m_imageBytes;

	private int m_occurenceCount;

	private byte m_bFlags;

	private WordDocument m_doc;

	private Size m_size = new Size(int.MaxValue, int.MaxValue);

	private DocGen.DocIO.DLS.Entities.ImageFormat m_imageFormat;

	private DocGen.DocIO.DLS.Entities.ImageFormat m_imageFormatForPartialTrustMode;

	private int m_length = int.MinValue;

	internal string comparedImageName;

	internal int ImageId
	{
		get
		{
			return m_imageId;
		}
		set
		{
			m_imageId = value;
		}
	}

	internal byte[] ImageBytes
	{
		get
		{
			if (IsMetafile && m_doc != null)
			{
				return m_doc.Images.DecompressImageBytes(m_imageBytes);
			}
			return m_imageBytes;
		}
		set
		{
			m_imageBytes = m_doc.Images.CompressImageBytes(value);
		}
	}

	internal int OccurenceCount
	{
		get
		{
			return m_occurenceCount;
		}
		set
		{
			m_occurenceCount = value;
			if (m_occurenceCount == 0)
			{
				if (m_doc != null)
				{
					m_doc.Images.Remove(m_imageId);
				}
				Close();
			}
		}
	}

	internal bool IsMetafile
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsAdded
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal Size Size
	{
		get
		{
			if ((float)m_size.Width == float.MinValue || (float)m_size.Height == float.MinValue)
			{
				UpdateImageSize(GetImageInternal(ImageBytes));
			}
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	internal DocGen.DocIO.DLS.Entities.ImageFormat ImageFormatForPartialTrustMode
	{
		get
		{
			UpdateImageSizeForPartialTrustMode(GetImageInternalForPartialTrustMode(ImageBytes));
			return m_imageFormatForPartialTrustMode;
		}
		set
		{
			m_imageFormatForPartialTrustMode = value;
		}
	}

	internal DocGen.DocIO.DLS.Entities.ImageFormat ImageFormat
	{
		get
		{
			UpdateImageSize(GetImageInternal(ImageBytes));
			return m_imageFormat;
		}
		set
		{
			m_imageFormat = value;
		}
	}

	internal int Length
	{
		get
		{
			if (m_length == int.MinValue)
			{
				m_length = ImageBytes.Length;
			}
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}

	internal ImageRecord(WordDocument doc, byte[] imageBytes)
	{
		m_doc = doc;
		m_imageBytes = imageBytes;
	}

	internal ImageRecord(WordDocument doc, ImageRecord imageRecord)
	{
		m_doc = doc;
		m_imageBytes = imageRecord.m_imageBytes;
		IsMetafile = imageRecord.IsMetafile;
		m_length = imageRecord.m_length;
		m_imageFormat = imageRecord.m_imageFormat;
		m_size = imageRecord.m_size;
		m_imageId = imageRecord.ImageId;
	}

	internal void Detach()
	{
		m_occurenceCount--;
		if (m_occurenceCount == 0)
		{
			if (m_doc != null)
			{
				m_doc.Images.Remove(m_imageId);
			}
			m_imageId = 0;
			m_occurenceCount = 0;
		}
	}

	internal bool IsMetafileHeaderPresent(byte[] imagebytes)
	{
		bool result = false;
		byte[] array = new byte[4] { 215, 205, 198, 154 };
		if (imagebytes != null && imagebytes.Length > 22)
		{
			result = true;
			int num = 0;
			if (num < 4)
			{
				if (array[num] != imagebytes[num])
				{
					result = false;
				}
				return result;
			}
		}
		return result;
	}

	internal void Attach()
	{
		m_occurenceCount++;
		m_doc.Images.Add(this);
	}

	internal void Close()
	{
		m_bFlags = 0;
		m_doc = null;
		m_imageBytes = null;
		m_imageId = 0;
		m_occurenceCount = 0;
	}

	internal void UpdateImageSizeForPartialTrustMode(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image != null)
		{
			m_size = image.Size;
			m_imageFormatForPartialTrustMode = image.RawFormat;
			image.Dispose();
		}
	}

	internal void UpdateImageSize(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image != null)
		{
			m_size = image.Size;
			m_imageFormat = image.RawFormat;
			image.Dispose();
		}
	}

	private DocGen.DocIO.DLS.Entities.Image GetImageInternalForPartialTrustMode(byte[] imageBytes)
	{
		if (imageBytes != null)
		{
			m_length = imageBytes.Length;
			return GetImageInternalForPartialTrustMode(imageBytes);
		}
		return null;
	}

	private DocGen.DocIO.DLS.Entities.Image GetImageInternal(byte[] imageBytes)
	{
		if (imageBytes != null)
		{
			m_length = imageBytes.Length;
			return GetImage(imageBytes);
		}
		return null;
	}

	internal static DocGen.DocIO.DLS.Entities.Image GetImage(byte[] imageBytes)
	{
		DocGen.DocIO.DLS.Entities.Image result = null;
		if (imageBytes != null)
		{
			try
			{
				result = DocGen.DocIO.DLS.Entities.Image.FromStream(new MemoryStream(imageBytes));
				imageBytes = null;
			}
			catch
			{
				Stream manifestResourceStream = WPicture.GetManifestResourceStream("ImageNotFound.jpg");
				MemoryStream memoryStream = new MemoryStream();
				manifestResourceStream.CopyTo(memoryStream);
				result = DocGen.DocIO.DLS.Entities.Image.FromStream(memoryStream);
				imageBytes = null;
			}
		}
		return result;
	}
}

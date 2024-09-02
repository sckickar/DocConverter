using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtImage : _Blip
{
	private const int DEF_COLOR_USED_OFFSET = 32;

	private const int DEF_DIB_HEADER_SIZE = 14;

	private static readonly byte[] DEF_SIGNATURE = new byte[2] { 66, 77 };

	private static readonly byte[] DEF_RESERVED = new byte[4];

	private const uint DEF_COLOR_SIZE = 4u;

	private ImageRecord m_imageRecord;

	internal override byte[] ImageBytes
	{
		get
		{
			if (m_imageRecord == null)
			{
				return null;
			}
			return m_imageRecord.ImageBytes;
		}
		set
		{
			m_imageRecord = m_doc.Images.LoadImage(value);
			if (m_imageRecord != null)
			{
				m_imageRecord.OccurenceCount--;
			}
		}
	}

	internal override ImageRecord ImageRecord
	{
		get
		{
			return m_imageRecord;
		}
		set
		{
			m_imageRecord = value;
		}
	}

	internal MsofbtImage(WordDocument doc)
		: base(doc)
	{
	}

	internal MsofbtImage(ImageRecord imageRecord, bool isBitmap, WordDocument doc)
		: base(doc)
	{
		if (isBitmap)
		{
			base.Header.Type = MSOFBT.msofbtBlipPNG;
			base.Header.Instance = 1760;
		}
		else
		{
			base.Header.Type = MSOFBT.msofbtBlipJPEG;
			base.Header.Instance = 1130;
		}
		base.Uid = Guid.NewGuid();
		base.Uid2 = base.Uid;
		m_imageRecord = imageRecord;
	}

	protected override void ReadRecordData(Stream stream)
	{
		ReadGuid(stream);
		stream.ReadByte();
		int num = base.Header.Length - 16 - 1;
		byte[] array = new byte[num];
		stream.Read(array, 0, num);
		if (base.IsDib)
		{
			array = ConvertDibToBmp(array);
		}
		m_imageRecord = m_doc.Images.LoadImage(array);
		array = null;
	}

	protected override void WriteRecordData(Stream stream)
	{
		byte[] array = base.Uid.ToByteArray();
		stream.Write(array, 0, array.Length);
		if (HasUid2())
		{
			array = base.Uid2.ToByteArray();
			stream.Write(array, 0, array.Length);
		}
		stream.WriteByte(byte.MaxValue);
		stream.Write(ImageBytes, 0, ImageBytes.Length);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtImage msofbtImage = new MsofbtImage(m_doc);
		if (m_imageRecord != null)
		{
			msofbtImage.m_imageRecord = new ImageRecord(m_doc, m_imageRecord);
		}
		msofbtImage.Header = base.Header.Clone();
		msofbtImage.Uid = new Guid(base.Uid.ToByteArray());
		msofbtImage.Uid2 = new Guid(base.Uid2.ToByteArray());
		msofbtImage.m_doc = m_doc;
		return msofbtImage;
	}

	internal override void Close()
	{
		base.Close();
	}

	private byte[] ConvertDibToBmp(byte[] imageBytes)
	{
		uint num = BitConverter.ToUInt32(imageBytes, 0);
		uint num2 = BitConverter.ToUInt32(imageBytes, 32);
		int value = imageBytes.Length + 14;
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = BitConverter.GetBytes(value);
		memoryStream.Write(DEF_SIGNATURE, 0, DEF_SIGNATURE.Length);
		memoryStream.Write(bytes, 0, bytes.Length);
		memoryStream.Write(DEF_RESERVED, 0, DEF_RESERVED.Length);
		bytes = BitConverter.GetBytes(num + 14 + num2 * 4);
		memoryStream.Write(bytes, 0, bytes.Length);
		memoryStream.Write(imageBytes, 0, imageBytes.Length);
		imageBytes = memoryStream.ToArray();
		memoryStream.Close();
		return imageBytes;
	}
}

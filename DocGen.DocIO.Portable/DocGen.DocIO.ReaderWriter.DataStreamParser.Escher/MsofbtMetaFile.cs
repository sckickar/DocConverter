using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtMetaFile : _Blip
{
	internal int m_length;

	internal int m_rectLeft;

	internal int m_rectTop;

	internal int m_rectRight;

	internal int m_rectBottom;

	internal int m_rectWidth;

	internal int m_rectHeight;

	private byte m_fFilter;

	private ImageRecord m_imageRecord;

	internal CompressionMethod Compression => CompressionMethod.msocompressionZip;

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
			m_imageRecord = m_doc.Images.LoadMetaFileImage(value, isCompressed: false);
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

	internal MsofbtMetaFile(WordDocument doc)
		: base(doc)
	{
	}

	internal MsofbtMetaFile(ImageRecord imageRecord, WordDocument doc)
		: base(doc)
	{
		if (imageRecord != null)
		{
			base.Header.Type = MSOFBT.msofbtBlipEMF;
			base.Header.Instance = 980;
			base.Uid = Guid.NewGuid();
			base.Uid2 = base.Uid;
			m_imageRecord = imageRecord;
			m_length = m_imageRecord.Length;
			m_fFilter = 254;
			m_rectWidth = imageRecord.Size.Width * 12700;
			m_rectHeight = imageRecord.Size.Height * 12700;
		}
	}

	internal MsofbtMetaFile(WPicture picture, WordDocument doc)
		: base(doc)
	{
		if (picture.ImageRecord != null)
		{
			if (picture.ImageRecord.ImageFormat.Equals(ImageFormat.Emf))
			{
				base.Header.Type = MSOFBT.msofbtBlipEMF;
				base.Header.Instance = 980;
			}
			else if (picture.ImageRecord.ImageFormat.Equals(ImageFormat.Wmf))
			{
				base.Header.Type = MSOFBT.msofbtBlipWMF;
				base.Header.Instance = 534;
			}
			base.Uid = Guid.NewGuid();
			base.Uid2 = base.Uid;
			m_imageRecord = picture.ImageRecord;
			m_length = m_imageRecord.Length;
			m_fFilter = 254;
			m_rectWidth = picture.ImageRecord.Size.Width * 12700;
			m_rectHeight = picture.ImageRecord.Size.Height * 12700;
		}
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtMetaFile msofbtMetaFile = (MsofbtMetaFile)MemberwiseClone();
		if (m_imageRecord != null)
		{
			msofbtMetaFile.m_imageRecord = new ImageRecord(m_doc, m_imageRecord);
		}
		msofbtMetaFile.Header = base.Header.Clone();
		msofbtMetaFile.Uid = new Guid(base.Uid.ToByteArray());
		msofbtMetaFile.Uid2 = new Guid(base.Uid2.ToByteArray());
		msofbtMetaFile.m_doc = m_doc;
		return msofbtMetaFile;
	}

	internal override void Close()
	{
		base.Close();
	}

	protected override void ReadRecordData(Stream stream)
	{
		int num = (int)stream.Position;
		ReadGuid(stream);
		m_length = BaseWordRecord.ReadInt32(stream);
		m_rectLeft = BaseWordRecord.ReadInt32(stream);
		m_rectTop = BaseWordRecord.ReadInt32(stream);
		m_rectRight = BaseWordRecord.ReadInt32(stream);
		m_rectBottom = BaseWordRecord.ReadInt32(stream);
		m_rectWidth = BaseWordRecord.ReadInt32(stream);
		m_rectHeight = BaseWordRecord.ReadInt32(stream);
		int num2 = BaseWordRecord.ReadInt32(stream);
		int num3 = stream.ReadByte();
		m_fFilter = (byte)stream.ReadByte();
		byte[] array = new byte[num2];
		stream.Read(array, 0, num2);
		if (num3 == 0)
		{
			m_imageRecord = m_doc.Images.LoadMetaFileImage(array, isCompressed: true);
		}
		else
		{
			m_imageRecord = m_doc.Images.LoadMetaFileImage(array, isCompressed: false);
		}
		array = null;
		stream.Position = num + base.Header.Length;
	}

	protected override void WriteRecordData(Stream stream)
	{
		if (ImageRecord.IsMetafileHeaderPresent(ImageRecord.ImageBytes))
		{
			byte[] array = new byte[ImageRecord.ImageBytes.Length - 22];
			Buffer.BlockCopy(ImageRecord.ImageBytes, 22, array, 0, ImageRecord.ImageBytes.Length - 22);
			ImageRecord.m_imageBytes = m_doc.Images.CompressImageBytes(array);
		}
		byte[] array2 = base.Uid.ToByteArray();
		stream.Write(array2, 0, array2.Length);
		if (HasUid2())
		{
			array2 = base.Uid2.ToByteArray();
			stream.Write(array2, 0, array2.Length);
		}
		BaseWordRecord.WriteInt32(stream, m_length);
		BaseWordRecord.WriteInt32(stream, m_rectLeft);
		BaseWordRecord.WriteInt32(stream, m_rectTop);
		BaseWordRecord.WriteInt32(stream, m_rectRight);
		BaseWordRecord.WriteInt32(stream, m_rectBottom);
		BaseWordRecord.WriteInt32(stream, m_rectWidth);
		BaseWordRecord.WriteInt32(stream, m_rectHeight);
		BaseWordRecord.WriteInt32(stream, ImageRecord.m_imageBytes.Length);
		stream.WriteByte(0);
		stream.WriteByte(m_fFilter);
		stream.Write(ImageRecord.m_imageBytes, 0, ImageRecord.m_imageBytes.Length);
	}
}

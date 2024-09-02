using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class OLEStream
{
	private class MonokerStream
	{
		private const int DEF_STRUCT_LEN = 0;

		private const int DEF_UNICODE_MARKER = -559022081;

		private const int DEF_UNICODE_MARKER_SIZE = 4;

		internal CLSID m_Clsid;

		internal byte[] m_streamData;

		internal string m_stringData;

		internal int Length
		{
			get
			{
				if (m_streamData != null)
				{
					return m_streamData.Length + 16;
				}
				return 0;
			}
		}

		internal MonokerStream(string data)
		{
			m_stringData = data;
		}

		internal void Parse(byte[] arrData, int iOffset)
		{
			m_Clsid = new CLSID();
			m_Clsid.Parse(arrData, iOffset);
			iOffset += m_Clsid.Length;
			int length = arrData.Length - m_Clsid.Length;
			m_streamData = ByteConverter.ReadBytes(arrData, length, ref iOffset);
		}

		internal int Save(byte[] arrData, int iOffset)
		{
			m_Clsid.Save(arrData, iOffset);
			iOffset += m_Clsid.Length;
			ByteConverter.WriteBytes(arrData, ref iOffset, m_streamData);
			return 0;
		}
	}

	private class CLSID
	{
		internal const int DEF_STRUCT_LEN = 16;

		internal int m_data1;

		internal short m_data2;

		internal short m_data3;

		internal long m_data4;

		internal int Length => 16;

		internal CLSID()
		{
		}

		internal void Parse(byte[] arrData, int iOffset)
		{
			m_data1 = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_data2 = ByteConverter.ReadInt16(arrData, ref iOffset);
			m_data3 = ByteConverter.ReadInt16(arrData, ref iOffset);
			m_data4 = ByteConverter.ReadInt64(arrData, ref iOffset);
		}

		internal int Save(byte[] arrData, int iOffset)
		{
			ByteConverter.WriteInt32(arrData, ref iOffset, m_data1);
			ByteConverter.WriteInt16(arrData, ref iOffset, m_data2);
			ByteConverter.WriteInt16(arrData, ref iOffset, m_data3);
			ByteConverter.WriteInt64(arrData, ref iOffset, m_data4);
			return 16;
		}
	}

	private const int DEF_VERSION_CONSTANT = 33554433;

	private const int DEF_RESERVED_VALUE = 0;

	private const int DEF_EMBEDDED_SIZE = 20;

	private const int DEF_CLSID_INDICATOR = -1;

	private const int DEF_EMBED_FLAG = 8;

	private const int DEF_LINK_FLAG = 1;

	private int m_streamLeng;

	private int m_oleVersion;

	private int m_flags;

	private int m_linkUpdateOption;

	private int m_reserved1;

	private int m_reservedMonikerStreamSize;

	private MonokerStream m_reservedMonikerStream;

	private int m_relativeSourceMonikerStreamSize;

	private MonokerStream m_relativeSourceMonikerStream;

	private int m_absoluteSourceMonikerStreamSize;

	private MonokerStream m_absoluteSourceMonikerStream;

	private int m_clsidIndicator;

	private CLSID m_clsid;

	private int m_reservedDisplayName;

	private int m_reserved2;

	private int m_localUpdateTime;

	private int m_localCheckUpdateTime;

	private int m_remoteUpdateTime;

	private OleLinkType m_linkType;

	private string m_filePath = string.Empty;

	internal int Length
	{
		get
		{
			if (m_streamLeng == 0)
			{
				m_streamLeng = 20;
			}
			return m_streamLeng;
		}
	}

	internal OLEStream(Stream stream)
	{
		Parse((stream as MemoryStream).ToArray(), 0);
	}

	internal OLEStream(OleLinkType type, string filePath)
	{
		m_linkType = type;
		m_oleVersion = 33554433;
		m_reserved1 = 0;
		m_reservedMonikerStreamSize = 0;
		if (type == OleLinkType.Embed)
		{
			m_flags = 8;
			return;
		}
		m_flags = 1;
		m_filePath = filePath;
		m_linkUpdateOption = 3;
	}

	internal void Parse(byte[] arrData, int iOffset)
	{
		m_streamLeng = arrData.Length;
		m_oleVersion = ByteConverter.ReadInt32(arrData, ref iOffset);
		if (m_oleVersion != 33554433)
		{
			throw new Exception("OLE stream is not valid");
		}
		m_flags = ByteConverter.ReadInt32(arrData, ref iOffset);
		m_linkUpdateOption = ByteConverter.ReadInt32(arrData, ref iOffset);
		m_reserved1 = ByteConverter.ReadInt32(arrData, ref iOffset);
		if (m_reserved1 != 0)
		{
			throw new Exception("OLE stream is not valid");
		}
		if (m_flags != 0 && m_flags != 8)
		{
			m_reservedMonikerStreamSize = ByteConverter.ReadInt32(arrData, ref iOffset);
			if (m_reservedMonikerStreamSize != 0)
			{
				byte[] arrData2 = ByteConverter.ReadBytes(arrData, m_reservedMonikerStreamSize, ref iOffset);
				m_reservedMonikerStream = new MonokerStream(m_filePath);
				m_reservedMonikerStream.Parse(arrData2, 0);
			}
			m_relativeSourceMonikerStreamSize = ByteConverter.ReadInt32(arrData, ref iOffset);
			if (m_relativeSourceMonikerStreamSize != 0)
			{
				byte[] arrData3 = ByteConverter.ReadBytes(arrData, m_relativeSourceMonikerStreamSize, ref iOffset);
				m_relativeSourceMonikerStream = new MonokerStream(m_filePath);
				m_relativeSourceMonikerStream.Parse(arrData3, 0);
			}
			m_absoluteSourceMonikerStreamSize = ByteConverter.ReadInt32(arrData, ref iOffset);
			byte[] arrData4 = ByteConverter.ReadBytes(arrData, m_absoluteSourceMonikerStreamSize, ref iOffset);
			m_absoluteSourceMonikerStream = new MonokerStream(m_filePath);
			m_absoluteSourceMonikerStream.Parse(arrData4, 0);
			m_clsidIndicator = ByteConverter.ReadInt32(arrData, ref iOffset);
			if (m_clsidIndicator == -1)
			{
				throw new Exception("OLE stream is not valid");
			}
			byte[] arrData5 = ByteConverter.ReadBytes(arrData, 16, ref iOffset);
			m_clsid = new CLSID();
			m_clsid.Parse(arrData5, 0);
			m_reservedDisplayName = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_reserved2 = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_localUpdateTime = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_localCheckUpdateTime = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_remoteUpdateTime = ByteConverter.ReadInt32(arrData, ref iOffset);
		}
	}

	internal int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteInt32(arrData, ref iOffset, m_oleVersion);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_flags);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_linkUpdateOption);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_reserved1);
		if (m_flags == 0)
		{
			return arrData.Length;
		}
		ByteConverter.WriteInt32(arrData, ref iOffset, m_reservedMonikerStreamSize);
		if (m_reservedMonikerStreamSize != 0)
		{
			m_reservedMonikerStream.Save(arrData, iOffset);
			iOffset += m_reservedMonikerStream.Length;
		}
		ByteConverter.WriteInt32(arrData, ref iOffset, m_relativeSourceMonikerStreamSize);
		if (m_relativeSourceMonikerStreamSize != 0)
		{
			m_relativeSourceMonikerStream.Save(arrData, iOffset);
			iOffset += m_relativeSourceMonikerStream.Length;
		}
		ByteConverter.WriteInt32(arrData, ref iOffset, m_absoluteSourceMonikerStreamSize);
		m_absoluteSourceMonikerStream.Save(arrData, iOffset);
		iOffset += m_absoluteSourceMonikerStream.Length;
		ByteConverter.WriteInt32(arrData, ref iOffset, m_clsidIndicator);
		m_clsid.Save(arrData, iOffset);
		iOffset += m_clsid.Length;
		ByteConverter.WriteInt32(arrData, ref iOffset, m_reservedDisplayName);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_reserved2);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_localUpdateTime);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_localCheckUpdateTime);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_remoteUpdateTime);
		return arrData.Length;
	}

	internal void SaveTo(Stream stream)
	{
		int iOffset = 0;
		byte[] array = new byte[20];
		ByteConverter.WriteInt32(array, ref iOffset, m_oleVersion);
		ByteConverter.WriteInt32(array, ref iOffset, m_flags);
		ByteConverter.WriteInt32(array, ref iOffset, m_linkUpdateOption);
		ByteConverter.WriteInt32(array, ref iOffset, m_reserved1);
		if (m_linkType == OleLinkType.Embed)
		{
			ByteConverter.WriteInt32(array, ref iOffset, m_reservedMonikerStreamSize);
			stream.Write(array, 0, array.Length);
		}
	}
}

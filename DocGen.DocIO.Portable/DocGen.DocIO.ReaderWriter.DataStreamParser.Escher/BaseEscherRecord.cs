using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal abstract class BaseEscherRecord : BaseWordRecord
{
	private _MSOFBH m_msofbh;

	internal WordDocument m_doc;

	internal _MSOFBH Header
	{
		get
		{
			return m_msofbh;
		}
		set
		{
			m_msofbh = value;
		}
	}

	internal BaseEscherRecord(WordDocument doc)
	{
		m_doc = doc;
		m_msofbh = new _MSOFBH(doc);
	}

	internal BaseEscherRecord(MSOFBT type, int version, WordDocument doc)
		: this(doc)
	{
		m_msofbh.Type = type;
		m_msofbh.Version = version;
	}

	protected abstract void ReadRecordData(Stream stream);

	protected abstract void WriteRecordData(Stream stream);

	internal abstract BaseEscherRecord Clone();

	internal bool ReadRecord(_MSOFBH msofbh, Stream stream)
	{
		m_msofbh = msofbh;
		int num = (int)stream.Position;
		ReadRecordData(stream);
		if ((int)stream.Position - num != m_msofbh.Length)
		{
			return false;
		}
		return true;
	}

	internal void ReadMsofbhWithRecord(Stream stream)
	{
		ReadRecord(new _MSOFBH(stream, m_doc), stream);
	}

	internal int WriteMsofbhWithRecord(Stream stream)
	{
		int num = Convert.ToInt32(stream.Position);
		Header.Write(stream);
		int num2 = Convert.ToInt32(stream.Position);
		WriteRecordData(stream);
		int num3 = Convert.ToInt32(stream.Position);
		Header.Length = num3 - num2;
		stream.Position = num;
		Header.Write(stream);
		stream.Position = num3;
		return num3 - num;
	}

	internal new virtual void Close()
	{
		m_doc = null;
	}
}

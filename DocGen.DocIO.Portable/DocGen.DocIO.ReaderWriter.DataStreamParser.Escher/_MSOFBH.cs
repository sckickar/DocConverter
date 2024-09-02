using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class _MSOFBH : BaseWordRecord
{
	internal const int DEF_ISCONTAINER = 15;

	private int m_version;

	private int m_instance;

	private int m_type;

	private int m_length;

	internal WordDocument m_doc;

	internal int Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	internal bool IsContainer
	{
		get
		{
			return m_version == 15;
		}
		set
		{
			if (value)
			{
				m_version = 15;
			}
		}
	}

	internal new int Length
	{
		get
		{
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}

	internal MSOFBT Type
	{
		get
		{
			return (MSOFBT)m_type;
		}
		set
		{
			m_type = (int)value;
		}
	}

	internal int Version
	{
		get
		{
			return m_version;
		}
		set
		{
			m_version = value;
		}
	}

	internal _MSOFBH(WordDocument doc)
	{
		m_doc = doc;
	}

	internal _MSOFBH(Stream stream, WordDocument doc)
	{
		m_doc = doc;
		Read(stream);
	}

	internal void Read(Stream stream)
	{
		int num = BaseWordRecord.ReadInt32(stream);
		m_version = num & 0xF;
		m_instance = (num & 0xFFF0) >> 4;
		m_type = (int)((num & 0xFFFF0000u) >> 16);
		m_length = BaseWordRecord.ReadInt32(stream);
	}

	internal void Write(Stream stream)
	{
		int num = 0;
		num |= m_version;
		num |= m_instance << 4;
		num |= m_type << 16;
		BaseWordRecord.WriteInt32(stream, num);
		BaseWordRecord.WriteInt32(stream, m_length);
	}

	internal BaseEscherRecord CreateRecordFromHeader()
	{
		switch (Type)
		{
		case MSOFBT.msofbtDggContainer:
			return new MsofbtDggContainer(m_doc);
		case MSOFBT.msofbtBstoreContainer:
			return new MsofbtBstoreContainer(m_doc);
		case MSOFBT.msofbtDgContainer:
			return new MsofbtDgContainer(m_doc);
		case MSOFBT.msofbtSpgrContainer:
			return new MsofbtSpgrContainer(m_doc);
		case MSOFBT.msofbtSpContainer:
			return new MsofbtSpContainer(m_doc);
		case MSOFBT.msofbtSolverContainer:
			return new MsofbtSolverContainer(m_doc);
		case MSOFBT.msofbtDgg:
			return new MsofbtDgg(m_doc);
		case MSOFBT.msofbtBSE:
			return new MsofbtBSE(m_doc);
		case MSOFBT.msofbtDg:
			return new MsofbtDg(m_doc);
		case MSOFBT.msofbtSpgr:
			return new MsofbtSpgr(m_doc);
		case MSOFBT.msofbtSp:
			return new MsofbtSp(m_doc);
		case MSOFBT.msofbtOPT:
			return new MsofbtOPT(m_doc);
		case MSOFBT.msofbtClientTextbox:
			return new MsofbtClientTextbox(m_doc);
		case MSOFBT.msofbtClientAnchor:
			return new MsofbtClientAnchor(m_doc);
		case MSOFBT.msofbtClientData:
			return new MsofbtClientData(m_doc);
		case MSOFBT.msofbtBlipEMF:
		case MSOFBT.msofbtBlipWMF:
			return new MsofbtMetaFile(m_doc);
		case MSOFBT.msofbtBlipJPEG:
		case MSOFBT.msofbtBlipPNG:
		case MSOFBT.msofbtBlipDIB:
			return new MsofbtImage(m_doc);
		case MSOFBT.msofbtREGROUPItems:
			return new MsofbtGeneral(m_doc);
		case MSOFBT.msofbtSecondaryFOPT:
			return new MsofbtSecondaryFOPT(m_doc);
		case MSOFBT.msofbtTertiaryFOPT:
			return new MsofbtTertiaryFOPT(m_doc);
		default:
			if (IsContainer)
			{
				return new BaseContainer(m_doc);
			}
			return new MsofbtGeneral(m_doc);
		}
	}

	internal _MSOFBH Clone()
	{
		_MSOFBH obj = (_MSOFBH)MemberwiseClone();
		obj.m_doc = m_doc;
		return obj;
	}

	internal static BaseEscherRecord ReadHeaderWithRecord(Stream stream, WordDocument doc)
	{
		_MSOFBH mSOFBH = new _MSOFBH(stream, doc);
		BaseEscherRecord baseEscherRecord = mSOFBH.CreateRecordFromHeader();
		if (!baseEscherRecord.ReadRecord(mSOFBH, stream))
		{
			return null;
		}
		return baseEscherRecord;
	}
}

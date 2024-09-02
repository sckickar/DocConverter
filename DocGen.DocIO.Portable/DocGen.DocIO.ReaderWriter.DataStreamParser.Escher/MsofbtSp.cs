using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSp : BaseEscherRecord
{
	private int m_shapeId;

	private int m_shapeFlags;

	internal bool IsGroup
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 1, 0) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 1, 0, value ? 1 : 0);
		}
	}

	internal bool IsChild
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 2, 1) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 2, 1, value ? 1 : 0);
		}
	}

	internal bool IsPatriarch
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 4, 2) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 4, 2, value ? 1 : 0);
		}
	}

	internal bool IsDeleted
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 8, 3) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 8, 3, value ? 1 : 0);
		}
	}

	internal bool IsOle
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 16, 4) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 16, 4, value ? 1 : 0);
		}
	}

	internal bool HasMaster
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 32, 5) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 32, 5, value ? 1 : 0);
		}
	}

	internal bool IsFlippedHor
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 64, 6) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 64, 6, value ? 1 : 0);
		}
	}

	internal bool IsFlippedVert
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 128, 7) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 128, 7, value ? 1 : 0);
		}
	}

	internal bool IsConnector
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 256, 8) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 256, 8, value ? 1 : 0);
		}
	}

	internal bool HasAnchor
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 512, 9) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 512, 9, value ? 1 : 0);
		}
	}

	internal bool IsBackground
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 1024, 10) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 1024, 10, value ? 1 : 0);
		}
	}

	internal bool HasShapeTypeProperty
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_shapeFlags, 2048, 11) == 1;
		}
		set
		{
			m_shapeFlags = BaseWordRecord.SetBitsByMask(m_shapeFlags, 2048, 11, value ? 1 : 0);
		}
	}

	internal int ShapeId
	{
		get
		{
			return m_shapeId;
		}
		set
		{
			m_shapeId = value;
		}
	}

	internal EscherShapeType ShapeType
	{
		get
		{
			return (EscherShapeType)base.Header.Instance;
		}
		set
		{
			base.Header.Instance = (int)value;
		}
	}

	internal MsofbtSp(WordDocument doc)
		: base(MSOFBT.msofbtSp, 2, doc)
	{
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_shapeId = BaseWordRecord.ReadInt32(stream);
		m_shapeFlags = BaseWordRecord.ReadInt32(stream);
	}

	protected override void WriteRecordData(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_shapeId);
		BaseWordRecord.WriteInt32(stream, m_shapeFlags);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtSp obj = (MsofbtSp)MemberwiseClone();
		obj.m_doc = m_doc;
		return obj;
	}

	internal bool Compare(MsofbtSp shape)
	{
		if (IsGroup != shape.IsGroup || IsChild != shape.IsChild || IsPatriarch != shape.IsPatriarch || IsDeleted != shape.IsDeleted || IsOle != shape.IsOle || HasMaster != shape.HasMaster || IsFlippedHor != shape.IsFlippedHor || IsFlippedVert != shape.IsFlippedVert || IsConnector != shape.IsConnector || HasAnchor != shape.HasAnchor || IsBackground != shape.IsBackground || HasShapeTypeProperty != shape.HasShapeTypeProperty || ShapeId != shape.ShapeId || ShapeType != shape.ShapeType)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (IsGroup ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsChild ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsPatriarch ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsDeleted ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsOle ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasMaster ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsFlippedHor ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsFlippedVert ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsConnector ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasAnchor ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsBackground ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasShapeTypeProperty ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(ShapeId + ";");
		stringBuilder.Append((int)ShapeType);
		return stringBuilder;
	}
}

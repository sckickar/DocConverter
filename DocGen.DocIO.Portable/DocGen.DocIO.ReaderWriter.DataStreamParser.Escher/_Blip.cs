using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal abstract class _Blip : BaseEscherRecord
{
	protected const int DEF_UID_LENGTH = 16;

	private Guid m_guid;

	private Guid m_guid2;

	internal MSOBlipType Type => (MSOBlipType)(base.Header.Type - 61464);

	internal abstract byte[] ImageBytes { get; set; }

	internal abstract ImageRecord ImageRecord { get; set; }

	internal ImageFormat ImageFormat => Type switch
	{
		MSOBlipType.msoblipEMF => ImageFormat.Emf, 
		MSOBlipType.msoblipWMF => ImageFormat.Wmf, 
		MSOBlipType.msoblipJPEG => ImageFormat.Jpeg, 
		MSOBlipType.msoblipPNG => ImageFormat.Png, 
		MSOBlipType.msoblipDIB => ImageFormat.Bmp, 
		_ => throw new Exception(Type.ToString() + "is not supported"), 
	};

	internal Guid Uid
	{
		get
		{
			return m_guid;
		}
		set
		{
			m_guid = value;
		}
	}

	internal Guid Uid2
	{
		get
		{
			return m_guid2;
		}
		set
		{
			m_guid2 = value;
		}
	}

	internal bool IsDib => Type == MSOBlipType.msoblipDIB;

	protected _Blip(WordDocument doc)
		: base(doc)
	{
	}

	protected void ReadGuid(Stream stream)
	{
		byte[] array = new byte[16];
		stream.Read(array, 0, array.Length);
		m_guid = new Guid(array);
		if (HasUid2())
		{
			array = new byte[16];
			stream.Read(array, 0, array.Length);
			m_guid2 = new Guid(array);
		}
	}

	internal bool HasUid2()
	{
		if ((base.Header.Type != MSOFBT.msofbtBlipEMF || base.Header.Instance != 981) && (base.Header.Type != MSOFBT.msofbtBlipWMF || base.Header.Instance != 535) && (base.Header.Type != (MSOFBT)61468 || base.Header.Instance != 1347) && ((base.Header.Type != MSOFBT.msofbtBlipJPEG && base.Header.Type != (MSOFBT)61482) || (base.Header.Instance != 1131 && base.Header.Instance != 1763)) && (base.Header.Type != MSOFBT.msofbtBlipPNG || base.Header.Instance != 1761) && (base.Header.Type != MSOFBT.msofbtBlipDIB || base.Header.Instance != 1961))
		{
			if (base.Header.Type == (MSOFBT)61481)
			{
				return base.Header.Instance == 1765;
			}
			return false;
		}
		return true;
	}

	internal abstract override BaseEscherRecord Clone();
}

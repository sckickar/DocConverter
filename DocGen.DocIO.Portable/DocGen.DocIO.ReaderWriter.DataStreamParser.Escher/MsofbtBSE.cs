using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtBSE : BaseEscherRecord
{
	private _FBSE m_fbse;

	private _Blip m_blip;

	private byte m_bFlags;

	internal _Blip Blip
	{
		get
		{
			return m_blip;
		}
		set
		{
			m_blip = value;
		}
	}

	internal _FBSE Fbse => m_fbse;

	internal bool IsInlineBlip
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

	internal bool IsPictureInShapeField
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal MsofbtBSE(WordDocument doc)
		: base(MSOFBT.msofbtBSE, 2, doc)
	{
		m_fbse = new _FBSE();
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_fbse.Read(stream);
		if (base.Header.Length > 36)
		{
			IsInlineBlip = true;
			m_blip = _MSOFBH.ReadHeaderWithRecord(stream, m_doc) as _Blip;
		}
	}

	protected override void WriteRecordData(Stream stream)
	{
		int num = Convert.ToInt32(stream.Position);
		m_fbse.Write(stream);
		if (IsInlineBlip && m_blip != null)
		{
			m_fbse.m_size = m_blip.WriteMsofbhWithRecord(stream);
			int num2 = Convert.ToInt32(stream.Position);
			stream.Position = num;
			m_fbse.Write(stream);
			stream.Position = num2;
		}
	}

	internal void Read(Stream stream)
	{
		long position = stream.Position;
		stream.Position = m_fbse.m_foDelay;
		m_blip = _MSOFBH.ReadHeaderWithRecord(stream, m_doc) as _Blip;
		stream.Position = position;
	}

	internal void Write(Stream stream)
	{
		m_fbse.m_foDelay = (int)stream.Position;
		m_fbse.m_size = 0;
		if (m_blip != null && !IsPictureInShapeField)
		{
			m_fbse.m_size = m_blip.WriteMsofbhWithRecord(stream);
		}
	}

	internal void Initialize(ImageRecord imageRecord)
	{
		_Blip blip;
		if (imageRecord.IsMetafile)
		{
			blip = new MsofbtMetaFile(imageRecord, m_doc);
		}
		else
		{
			bool isBitmap = IsBitmap(imageRecord.ImageFormat);
			blip = new MsofbtImage(imageRecord, isBitmap, m_doc);
		}
		base.Header.Instance = (int)blip.Type;
		Fbse.m_btWin32 = (int)blip.Type;
		Fbse.m_btMacOS = (int)blip.Type;
		Fbse.m_rgbUid = blip.Uid.ToByteArray();
		Fbse.m_tag = 255;
		Fbse.m_cRef = 1;
		Blip = blip;
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtBSE msofbtBSE = new MsofbtBSE(m_doc);
		msofbtBSE.IsInlineBlip = IsInlineBlip;
		msofbtBSE.m_fbse = m_fbse.Clone();
		if (m_blip != null)
		{
			msofbtBSE.m_blip = (_Blip)m_blip.Clone();
		}
		msofbtBSE.Header = base.Header.Clone();
		msofbtBSE.m_doc = m_doc;
		return msofbtBSE;
	}

	private bool IsMetafile(ImageFormat imageFormat)
	{
		if (!imageFormat.Equals(ImageFormat.Emf))
		{
			return imageFormat.Equals(ImageFormat.Wmf);
		}
		return true;
	}

	private bool IsBitmap(ImageFormat imageFormat)
	{
		if (!imageFormat.Equals(ImageFormat.Png))
		{
			return imageFormat.Equals(ImageFormat.Bmp);
		}
		return true;
	}

	internal bool Compare(MsofbtBSE bse)
	{
		if (IsInlineBlip != bse.IsInlineBlip || IsPictureInShapeField != bse.IsPictureInShapeField)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (IsInlineBlip ? "1" : "0");
		stringBuilder.Append("boolValue:" + text + ",");
		text = (IsPictureInShapeField ? "1" : "0");
		stringBuilder.Append("boolValue:" + text + ",");
		return stringBuilder;
	}
}

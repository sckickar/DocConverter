using System;
using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegCodecTagMethods : TiffTagMethods
{
	public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
	{
		JpegCodec jpegCodec = tif.m_currentCodec as JpegCodec;
		switch (tag)
		{
		case TiffTag.JPEGTABLES:
		{
			int num = ap[0].ToInt();
			if (num == 0)
			{
				return false;
			}
			jpegCodec.m_jpegtables = new byte[num];
			Buffer.BlockCopy(ap[1].ToByteArray(), 0, jpegCodec.m_jpegtables, 0, num);
			jpegCodec.m_jpegtables_length = num;
			tif.setFieldBit(66);
			break;
		}
		case TiffTag.JPEGQUALITY:
			jpegCodec.m_jpegquality = ap[0].ToInt();
			return true;
		case TiffTag.JPEGCOLORMODE:
			jpegCodec.m_jpegcolormode = (JpegColorMode)ap[0].ToShort();
			jpegCodec.JPEGResetUpsampled();
			return true;
		case TiffTag.PHOTOMETRIC:
		{
			bool result = base.SetField(tif, tag, ap);
			jpegCodec.JPEGResetUpsampled();
			return result;
		}
		case TiffTag.JPEGTABLESMODE:
			jpegCodec.m_jpegtablesmode = (JpegTablesMode)ap[0].ToShort();
			return true;
		case TiffTag.YCBCRSUBSAMPLING:
			jpegCodec.m_ycbcrsampling_fetched = true;
			return base.SetField(tif, tag, ap);
		case TiffTag.FAXRECVPARAMS:
			jpegCodec.m_recvparams = ap[0].ToInt();
			break;
		case TiffTag.FAXSUBADDRESS:
			Tiff.setString(out jpegCodec.m_subaddress, ap[0].ToString());
			break;
		case TiffTag.FAXRECVTIME:
			jpegCodec.m_recvtime = ap[0].ToInt();
			break;
		case TiffTag.FAXDCS:
			Tiff.setString(out jpegCodec.m_faxdcs, ap[0].ToString());
			break;
		default:
			return base.SetField(tif, tag, ap);
		}
		TiffFieldInfo tiffFieldInfo = tif.FieldWithTag(tag);
		if (tiffFieldInfo != null)
		{
			tif.setFieldBit(tiffFieldInfo.Bit);
			tif.m_flags |= TiffFlags.DIRTYDIRECT;
			return true;
		}
		return false;
	}

	public override FieldValue[] GetField(Tiff tif, TiffTag tag)
	{
		JpegCodec jpegCodec = tif.m_currentCodec as JpegCodec;
		FieldValue[] array = null;
		switch (tag)
		{
		case TiffTag.JPEGTABLES:
			array = new FieldValue[2];
			array[0].Set(jpegCodec.m_jpegtables_length);
			array[1].Set(jpegCodec.m_jpegtables);
			break;
		case TiffTag.JPEGQUALITY:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_jpegquality);
			break;
		case TiffTag.JPEGCOLORMODE:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_jpegcolormode);
			break;
		case TiffTag.JPEGTABLESMODE:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_jpegtablesmode);
			break;
		case TiffTag.YCBCRSUBSAMPLING:
			JPEGFixupTestSubsampling(tif);
			return base.GetField(tif, tag);
		case TiffTag.FAXRECVPARAMS:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_recvparams);
			break;
		case TiffTag.FAXSUBADDRESS:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_subaddress);
			break;
		case TiffTag.FAXRECVTIME:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_recvtime);
			break;
		case TiffTag.FAXDCS:
			array = new FieldValue[1];
			array[0].Set(jpegCodec.m_faxdcs);
			break;
		default:
			return base.GetField(tif, tag);
		}
		return array;
	}

	public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
	{
		JpegCodec jpegCodec = tif.m_currentCodec as JpegCodec;
		if (tif.fieldSet(66))
		{
			Tiff.fprintf(fd, "  JPEG Tables: ({0} bytes)\n", jpegCodec.m_jpegtables_length);
		}
		if (tif.fieldSet(67))
		{
			Tiff.fprintf(fd, "  Fax Receive Parameters: {0,8:x}\n", jpegCodec.m_recvparams);
		}
		if (tif.fieldSet(68))
		{
			Tiff.fprintf(fd, "  Fax SubAddress: {0}\n", jpegCodec.m_subaddress);
		}
		if (tif.fieldSet(69))
		{
			Tiff.fprintf(fd, "  Fax Receive Time: {0} secs\n", jpegCodec.m_recvtime);
		}
		if (tif.fieldSet(70))
		{
			Tiff.fprintf(fd, "  Fax DCS: {0}\n", jpegCodec.m_faxdcs);
		}
	}

	private static void JPEGFixupTestSubsampling(Tiff tif)
	{
		JpegCodec jpegCodec = tif.m_currentCodec as JpegCodec;
		jpegCodec.InitializeLibJPEG(force_encode: false, force_decode: false);
		if (!jpegCodec.m_common.IsDecompressor || jpegCodec.m_ycbcrsampling_fetched || tif.m_dir.td_photometric != Photometric.YCBCR)
		{
			return;
		}
		jpegCodec.m_ycbcrsampling_fetched = true;
		if (tif.IsTiled())
		{
			if (!tif.fillTile(0))
			{
				return;
			}
		}
		else if (!tif.fillStrip(0))
		{
			return;
		}
		tif.SetField(TiffTag.YCBCRSUBSAMPLING, jpegCodec.m_h_sampling, jpegCodec.m_v_sampling);
		tif.m_curstrip = -1;
	}
}

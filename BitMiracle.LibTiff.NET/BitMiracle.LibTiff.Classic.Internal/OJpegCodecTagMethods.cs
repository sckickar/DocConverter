using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class OJpegCodecTagMethods : TiffTagMethods
{
	public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
	{
		OJpegCodec oJpegCodec = tif.m_currentCodec as OJpegCodec;
		switch (tag)
		{
		case TiffTag.JPEGIFOFFSET:
			oJpegCodec.m_jpeg_interchange_format = ap[0].ToUInt();
			break;
		case TiffTag.JPEGIFBYTECOUNT:
			oJpegCodec.m_jpeg_interchange_format_length = ap[0].ToUInt();
			break;
		case TiffTag.YCBCRSUBSAMPLING:
			oJpegCodec.m_subsampling_tag = true;
			oJpegCodec.m_subsampling_hor = ap[0].ToByte();
			oJpegCodec.m_subsampling_ver = ap[1].ToByte();
			tif.m_dir.td_ycbcrsubsampling[0] = oJpegCodec.m_subsampling_hor;
			tif.m_dir.td_ycbcrsubsampling[1] = oJpegCodec.m_subsampling_ver;
			break;
		case TiffTag.JPEGQTABLES:
		{
			uint num = ap[0].ToUInt();
			switch (num)
			{
			default:
				Tiff.ErrorExt(tif, tif.m_clientdata, "OJPEGVSetField", "JpegQTables tag has incorrect count");
				return false;
			case 1u:
			case 2u:
			case 3u:
			{
				oJpegCodec.m_qtable_offset_count = (byte)num;
				uint[] array = ap[1].ToUIntArray();
				for (uint num2 = 0u; num2 < num; num2++)
				{
					oJpegCodec.m_qtable_offset[num2] = array[num2];
				}
				break;
			}
			case 0u:
				break;
			}
			break;
		}
		case TiffTag.JPEGDCTABLES:
		{
			uint num = ap[0].ToUInt();
			switch (num)
			{
			default:
				Tiff.ErrorExt(tif, tif.m_clientdata, "OJPEGVSetField", "JpegDcTables tag has incorrect count");
				return false;
			case 1u:
			case 2u:
			case 3u:
			{
				oJpegCodec.m_dctable_offset_count = (byte)num;
				uint[] array = ap[1].ToUIntArray();
				for (uint num2 = 0u; num2 < num; num2++)
				{
					oJpegCodec.m_dctable_offset[num2] = array[num2];
				}
				break;
			}
			case 0u:
				break;
			}
			break;
		}
		case TiffTag.JPEGACTABLES:
		{
			uint num = ap[0].ToUInt();
			switch (num)
			{
			default:
				Tiff.ErrorExt(tif, tif.m_clientdata, "OJPEGVSetField", "JpegAcTables tag has incorrect count");
				return false;
			case 1u:
			case 2u:
			case 3u:
			{
				oJpegCodec.m_actable_offset_count = (byte)num;
				uint[] array = ap[1].ToUIntArray();
				for (uint num2 = 0u; num2 < num; num2++)
				{
					oJpegCodec.m_actable_offset[num2] = array[num2];
				}
				break;
			}
			case 0u:
				break;
			}
			break;
		}
		case TiffTag.JPEGPROC:
			oJpegCodec.m_jpeg_proc = ap[0].ToByte();
			break;
		case TiffTag.JPEGRESTARTINTERVAL:
			oJpegCodec.m_restart_interval = ap[0].ToUShort();
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
		OJpegCodec oJpegCodec = tif.m_currentCodec as OJpegCodec;
		FieldValue[] array = null;
		switch (tag)
		{
		case TiffTag.JPEGIFOFFSET:
			array = new FieldValue[1];
			array[0].Set(oJpegCodec.m_jpeg_interchange_format);
			break;
		case TiffTag.JPEGIFBYTECOUNT:
			array = new FieldValue[1];
			array[0].Set(oJpegCodec.m_jpeg_interchange_format_length);
			break;
		case TiffTag.YCBCRSUBSAMPLING:
			if (!oJpegCodec.m_subsamplingcorrect_done)
			{
				oJpegCodec.OJPEGSubsamplingCorrect();
			}
			array = new FieldValue[2];
			array[0].Set(oJpegCodec.m_subsampling_hor);
			array[1].Set(oJpegCodec.m_subsampling_ver);
			break;
		case TiffTag.JPEGQTABLES:
			array = new FieldValue[2];
			array[0].Set(oJpegCodec.m_qtable_offset_count);
			array[1].Set(oJpegCodec.m_qtable_offset);
			break;
		case TiffTag.JPEGDCTABLES:
			array = new FieldValue[2];
			array[0].Set(oJpegCodec.m_dctable_offset_count);
			array[1].Set(oJpegCodec.m_dctable_offset);
			break;
		case TiffTag.JPEGACTABLES:
			array = new FieldValue[2];
			array[0].Set(oJpegCodec.m_actable_offset_count);
			array[1].Set(oJpegCodec.m_actable_offset);
			break;
		case TiffTag.JPEGPROC:
			array = new FieldValue[1];
			array[0].Set(oJpegCodec.m_jpeg_proc);
			break;
		case TiffTag.JPEGRESTARTINTERVAL:
			array = new FieldValue[1];
			array[0].Set(oJpegCodec.m_restart_interval);
			break;
		default:
			return base.GetField(tif, tag);
		}
		return array;
	}

	public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
	{
		OJpegCodec oJpegCodec = tif.m_currentCodec as OJpegCodec;
		if (tif.fieldSet(66))
		{
			Tiff.fprintf(fd, "  JpegInterchangeFormat: {0}\n", oJpegCodec.m_jpeg_interchange_format);
		}
		if (tif.fieldSet(67))
		{
			Tiff.fprintf(fd, "  JpegInterchangeFormatLength: {0}\n", oJpegCodec.m_jpeg_interchange_format_length);
		}
		if (tif.fieldSet(68))
		{
			Tiff.fprintf(fd, "  JpegQTables:");
			for (byte b = 0; b < oJpegCodec.m_qtable_offset_count; b++)
			{
				Tiff.fprintf(fd, " {0}", oJpegCodec.m_qtable_offset[b]);
			}
			Tiff.fprintf(fd, "\n");
		}
		if (tif.fieldSet(69))
		{
			Tiff.fprintf(fd, "  JpegDcTables:");
			for (byte b2 = 0; b2 < oJpegCodec.m_dctable_offset_count; b2++)
			{
				Tiff.fprintf(fd, " {0}", oJpegCodec.m_dctable_offset[b2]);
			}
			Tiff.fprintf(fd, "\n");
		}
		if (tif.fieldSet(70))
		{
			Tiff.fprintf(fd, "  JpegAcTables:");
			for (byte b3 = 0; b3 < oJpegCodec.m_actable_offset_count; b3++)
			{
				Tiff.fprintf(fd, " {0}", oJpegCodec.m_actable_offset[b3]);
			}
			Tiff.fprintf(fd, "\n");
		}
		if (tif.fieldSet(71))
		{
			Tiff.fprintf(fd, "  JpegProc: {0}\n", oJpegCodec.m_jpeg_proc);
		}
		if (tif.fieldSet(72))
		{
			Tiff.fprintf(fd, "  JpegRestartInterval: {0}\n", oJpegCodec.m_restart_interval);
		}
	}
}

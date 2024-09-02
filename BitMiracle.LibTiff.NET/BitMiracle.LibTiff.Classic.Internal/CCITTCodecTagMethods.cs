using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class CCITTCodecTagMethods : TiffTagMethods
{
	public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
	{
		CCITTCodec cCITTCodec = tif.m_currentCodec as CCITTCodec;
		switch (tag)
		{
		case TiffTag.FAXMODE:
			cCITTCodec.m_mode = (FaxMode)ap[0].ToShort();
			return true;
		case TiffTag.FAXFILLFUNC:
			cCITTCodec.fill = ap[0].Value as Tiff.FaxFillFunc;
			return true;
		case TiffTag.GROUP3OPTIONS:
			if (tif.m_dir.td_compression == Compression.CCITTFAX3)
			{
				cCITTCodec.m_groupoptions = (Group3Opt)ap[0].ToShort();
			}
			break;
		case TiffTag.GROUP4OPTIONS:
			if (tif.m_dir.td_compression == Compression.CCITTFAX4)
			{
				cCITTCodec.m_groupoptions = (Group3Opt)ap[0].ToShort();
			}
			break;
		case TiffTag.BADFAXLINES:
			cCITTCodec.m_badfaxlines = ap[0].ToInt();
			break;
		case TiffTag.CLEANFAXDATA:
			cCITTCodec.m_cleanfaxdata = (CleanFaxData)ap[0].ToByte();
			break;
		case TiffTag.CONSECUTIVEBADFAXLINES:
			cCITTCodec.m_badfaxrun = ap[0].ToInt();
			break;
		case TiffTag.FAXRECVPARAMS:
			cCITTCodec.m_recvparams = ap[0].ToInt();
			break;
		case TiffTag.FAXSUBADDRESS:
			Tiff.setString(out cCITTCodec.m_subaddress, ap[0].ToString());
			break;
		case TiffTag.FAXRECVTIME:
			cCITTCodec.m_recvtime = ap[0].ToInt();
			break;
		case TiffTag.FAXDCS:
			Tiff.setString(out cCITTCodec.m_faxdcs, ap[0].ToString());
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
		CCITTCodec cCITTCodec = tif.m_currentCodec as CCITTCodec;
		FieldValue[] array = new FieldValue[1];
		switch (tag)
		{
		case TiffTag.FAXMODE:
			array[0].Set(cCITTCodec.m_mode);
			break;
		case TiffTag.FAXFILLFUNC:
			array[0].Set(cCITTCodec.fill);
			break;
		case TiffTag.GROUP3OPTIONS:
		case TiffTag.GROUP4OPTIONS:
			array[0].Set(cCITTCodec.m_groupoptions);
			break;
		case TiffTag.BADFAXLINES:
			array[0].Set(cCITTCodec.m_badfaxlines);
			break;
		case TiffTag.CLEANFAXDATA:
			array[0].Set(cCITTCodec.m_cleanfaxdata);
			break;
		case TiffTag.CONSECUTIVEBADFAXLINES:
			array[0].Set(cCITTCodec.m_badfaxrun);
			break;
		case TiffTag.FAXRECVPARAMS:
			array[0].Set(cCITTCodec.m_recvparams);
			break;
		case TiffTag.FAXSUBADDRESS:
			array[0].Set(cCITTCodec.m_subaddress);
			break;
		case TiffTag.FAXRECVTIME:
			array[0].Set(cCITTCodec.m_recvtime);
			break;
		case TiffTag.FAXDCS:
			array[0].Set(cCITTCodec.m_faxdcs);
			break;
		default:
			return base.GetField(tif, tag);
		}
		return array;
	}

	public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
	{
		CCITTCodec cCITTCodec = tif.m_currentCodec as CCITTCodec;
		if (tif.fieldSet(73))
		{
			string text = " ";
			if (tif.m_dir.td_compression == Compression.CCITTFAX4)
			{
				Tiff.fprintf(fd, "  Group 4 Options:");
				if ((cCITTCodec.m_groupoptions & Group3Opt.UNCOMPRESSED) != 0)
				{
					Tiff.fprintf(fd, "{0}uncompressed data", text);
				}
			}
			else
			{
				Tiff.fprintf(fd, "  Group 3 Options:");
				if ((cCITTCodec.m_groupoptions & Group3Opt.ENCODING2D) != 0)
				{
					Tiff.fprintf(fd, "{0}2-d encoding", text);
					text = "+";
				}
				if ((cCITTCodec.m_groupoptions & Group3Opt.FILLBITS) != 0)
				{
					Tiff.fprintf(fd, "{0}EOL padding", text);
					text = "+";
				}
				if ((cCITTCodec.m_groupoptions & Group3Opt.UNCOMPRESSED) != 0)
				{
					Tiff.fprintf(fd, "{0}uncompressed data", text);
				}
			}
			Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", cCITTCodec.m_groupoptions, cCITTCodec.m_groupoptions);
		}
		if (tif.fieldSet(67))
		{
			Tiff.fprintf(fd, "  Fax Data:");
			switch (cCITTCodec.m_cleanfaxdata)
			{
			case CleanFaxData.CLEAN:
				Tiff.fprintf(fd, " clean");
				break;
			case CleanFaxData.REGENERATED:
				Tiff.fprintf(fd, " receiver regenerated");
				break;
			case CleanFaxData.UNCLEAN:
				Tiff.fprintf(fd, " uncorrected errors");
				break;
			}
			Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", cCITTCodec.m_cleanfaxdata, cCITTCodec.m_cleanfaxdata);
		}
		if (tif.fieldSet(66))
		{
			Tiff.fprintf(fd, "  Bad Fax Lines: {0}\n", cCITTCodec.m_badfaxlines);
		}
		if (tif.fieldSet(68))
		{
			Tiff.fprintf(fd, "  Consecutive Bad Fax Lines: {0}\n", cCITTCodec.m_badfaxrun);
		}
		if (tif.fieldSet(69))
		{
			Tiff.fprintf(fd, "  Fax Receive Parameters: {0,8:x}\n", cCITTCodec.m_recvparams);
		}
		if (tif.fieldSet(70))
		{
			Tiff.fprintf(fd, "  Fax SubAddress: {0}\n", cCITTCodec.m_subaddress);
		}
		if (tif.fieldSet(71))
		{
			Tiff.fprintf(fd, "  Fax Receive Time: {0} secs\n", cCITTCodec.m_recvtime);
		}
		if (tif.fieldSet(72))
		{
			Tiff.fprintf(fd, "  Fax DCS: {0}\n", cCITTCodec.m_faxdcs);
		}
	}
}

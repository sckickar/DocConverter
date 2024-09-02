using System;
using System.IO;
using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic;

public class TiffTagMethods
{
	private const short DATATYPE_VOID = 0;

	private const short DATATYPE_INT = 1;

	private const short DATATYPE_UINT = 2;

	private const short DATATYPE_IEEEFP = 3;

	public virtual bool SetField(Tiff tif, TiffTag tag, FieldValue[] value)
	{
		TiffDirectory dir = tif.m_dir;
		bool flag = true;
		int num = 0;
		int v = 0;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		switch (tag)
		{
		case TiffTag.SUBFILETYPE:
			dir.td_subfiletype = (FileType)value[0].ToByte();
			break;
		case TiffTag.IMAGEWIDTH:
			dir.td_imagewidth = value[0].ToInt();
			break;
		case TiffTag.IMAGELENGTH:
			dir.td_imagelength = value[0].ToInt();
			break;
		case TiffTag.BITSPERSAMPLE:
			dir.td_bitspersample = value[0].ToShort();
			if ((tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				switch (dir.td_bitspersample)
				{
				case 16:
					tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab16Bit;
					break;
				case 24:
					tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab24Bit;
					break;
				case 32:
					tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab32Bit;
					break;
				case 64:
					tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab64Bit;
					break;
				case 128:
					tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab64Bit;
					break;
				}
			}
			break;
		case TiffTag.COMPRESSION:
		{
			v = value[0].ToInt() & 0xFFFF;
			Compression compression = (Compression)v;
			if (tif.fieldSet(7))
			{
				if (dir.td_compression == compression)
				{
					break;
				}
				tif.m_currentCodec.Cleanup();
				tif.m_flags &= ~TiffFlags.CODERSETUP;
			}
			flag = tif.setCompressionScheme(compression);
			if (flag)
			{
				dir.td_compression = compression;
			}
			else
			{
				flag = false;
			}
			break;
		}
		case TiffTag.PHOTOMETRIC:
			dir.td_photometric = (Photometric)value[0].ToInt();
			break;
		case TiffTag.THRESHHOLDING:
			dir.td_threshholding = (Threshold)value[0].ToByte();
			break;
		case TiffTag.FILLORDER:
		{
			v = value[0].ToInt();
			FillOrder fillOrder = (FillOrder)v;
			if (fillOrder != FillOrder.LSB2MSB && fillOrder != FillOrder.MSB2LSB)
			{
				flag3 = true;
			}
			else
			{
				dir.td_fillorder = fillOrder;
			}
			break;
		}
		case TiffTag.ORIENTATION:
		{
			v = value[0].ToInt();
			Orientation orientation = (Orientation)v;
			if (orientation < Orientation.TOPLEFT || Orientation.LEFTBOT < orientation)
			{
				flag3 = true;
			}
			else
			{
				dir.td_orientation = orientation;
			}
			break;
		}
		case TiffTag.SAMPLESPERPIXEL:
			v = value[0].ToInt();
			if (v == 0)
			{
				flag3 = true;
			}
			else
			{
				dir.td_samplesperpixel = (short)v;
			}
			break;
		case TiffTag.ROWSPERSTRIP:
			num = value[0].ToInt();
			if (num == 0)
			{
				flag4 = true;
				break;
			}
			dir.td_rowsperstrip = num;
			if (!tif.fieldSet(2))
			{
				dir.td_tilelength = num;
				dir.td_tilewidth = dir.td_imagewidth;
			}
			break;
		case TiffTag.MINSAMPLEVALUE:
			dir.td_minsamplevalue = value[0].ToUShort();
			break;
		case TiffTag.MAXSAMPLEVALUE:
			dir.td_maxsamplevalue = value[0].ToUShort();
			break;
		case TiffTag.SMINSAMPLEVALUE:
			dir.td_sminsamplevalue = value[0].ToDouble();
			break;
		case TiffTag.SMAXSAMPLEVALUE:
			dir.td_smaxsamplevalue = value[0].ToDouble();
			break;
		case TiffTag.XRESOLUTION:
			dir.td_xresolution = value[0].ToFloat();
			break;
		case TiffTag.YRESOLUTION:
			dir.td_yresolution = value[0].ToFloat();
			break;
		case TiffTag.PLANARCONFIG:
		{
			v = value[0].ToInt();
			PlanarConfig planarConfig = (PlanarConfig)v;
			if (planarConfig != PlanarConfig.CONTIG && planarConfig != PlanarConfig.SEPARATE)
			{
				flag3 = true;
			}
			else
			{
				dir.td_planarconfig = planarConfig;
			}
			break;
		}
		case TiffTag.XPOSITION:
			dir.td_xposition = value[0].ToFloat();
			break;
		case TiffTag.YPOSITION:
			dir.td_yposition = value[0].ToFloat();
			break;
		case TiffTag.RESOLUTIONUNIT:
		{
			v = value[0].ToInt();
			ResUnit resUnit = (ResUnit)v;
			if (resUnit < ResUnit.NONE || ResUnit.CENTIMETER < resUnit)
			{
				flag3 = true;
			}
			else
			{
				dir.td_resolutionunit = resUnit;
			}
			break;
		}
		case TiffTag.PAGENUMBER:
			dir.td_pagenumber[0] = value[0].ToShort();
			dir.td_pagenumber[1] = value[1].ToShort();
			break;
		case TiffTag.HALFTONEHINTS:
			dir.td_halftonehints[0] = value[0].ToShort();
			dir.td_halftonehints[1] = value[1].ToShort();
			break;
		case TiffTag.COLORMAP:
			num = 1 << (int)dir.td_bitspersample;
			Tiff.setShortArray(out dir.td_colormap[0], value[0].ToShortArray(), num);
			Tiff.setShortArray(out dir.td_colormap[1], value[1].ToShortArray(), num);
			Tiff.setShortArray(out dir.td_colormap[2], value[2].ToShortArray(), num);
			break;
		case TiffTag.EXTRASAMPLES:
			if (!setExtraSamples(dir, ref v, value))
			{
				flag3 = true;
			}
			break;
		case TiffTag.MATTEING:
			if (value[0].ToShort() != 0)
			{
				dir.td_extrasamples = 1;
			}
			else
			{
				dir.td_extrasamples = 0;
			}
			if (dir.td_extrasamples != 0)
			{
				dir.td_sampleinfo = new ExtraSample[1];
				dir.td_sampleinfo[0] = ExtraSample.ASSOCALPHA;
			}
			break;
		case TiffTag.TILEWIDTH:
			num = value[0].ToInt();
			if (num % 16 != 0)
			{
				if (tif.m_mode != 0)
				{
					flag4 = true;
					break;
				}
				Tiff.WarningExt(tif, tif.m_clientdata, tif.m_name, "Nonstandard tile width {0}, convert file", num);
			}
			dir.td_tilewidth = num;
			tif.m_flags |= TiffFlags.ISTILED;
			break;
		case TiffTag.TILELENGTH:
			num = value[0].ToInt();
			if (num % 16 != 0)
			{
				if (tif.m_mode != 0)
				{
					flag4 = true;
					break;
				}
				Tiff.WarningExt(tif, tif.m_clientdata, tif.m_name, "Nonstandard tile length {0}, convert file", num);
			}
			dir.td_tilelength = num;
			tif.m_flags |= TiffFlags.ISTILED;
			break;
		case TiffTag.TILEDEPTH:
			num = value[0].ToInt();
			if (num == 0)
			{
				flag4 = true;
			}
			else
			{
				dir.td_tiledepth = num;
			}
			break;
		case TiffTag.DATATYPE:
		{
			v = value[0].ToInt();
			SampleFormat sampleFormat = SampleFormat.VOID;
			switch (v)
			{
			case 0:
				sampleFormat = SampleFormat.VOID;
				break;
			case 1:
				sampleFormat = SampleFormat.INT;
				break;
			case 2:
				sampleFormat = SampleFormat.UINT;
				break;
			case 3:
				sampleFormat = SampleFormat.IEEEFP;
				break;
			default:
				flag3 = true;
				break;
			}
			if (!flag3)
			{
				dir.td_sampleformat = sampleFormat;
			}
			break;
		}
		case TiffTag.SAMPLEFORMAT:
		{
			v = value[0].ToInt();
			SampleFormat sampleFormat = (SampleFormat)v;
			if (sampleFormat < SampleFormat.UINT || SampleFormat.COMPLEXIEEEFP < sampleFormat)
			{
				flag3 = true;
				break;
			}
			dir.td_sampleformat = sampleFormat;
			if (dir.td_sampleformat == SampleFormat.COMPLEXINT && dir.td_bitspersample == 32 && tif.m_postDecodeMethod == Tiff.PostDecodeMethodType.pdmSwab32Bit)
			{
				tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab16Bit;
			}
			else if ((dir.td_sampleformat == SampleFormat.COMPLEXINT || dir.td_sampleformat == SampleFormat.COMPLEXIEEEFP) && dir.td_bitspersample == 64 && tif.m_postDecodeMethod == Tiff.PostDecodeMethodType.pdmSwab64Bit)
			{
				tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab32Bit;
			}
			break;
		}
		case TiffTag.IMAGEDEPTH:
			dir.td_imagedepth = value[0].ToInt();
			break;
		case TiffTag.SUBIFD:
			if ((tif.m_flags & TiffFlags.INSUBIFD) != TiffFlags.INSUBIFD)
			{
				dir.td_nsubifd = value[0].ToInt();
				Tiff.setLong8Array(out dir.td_subifd, value[1].TolongArray(), dir.td_nsubifd);
			}
			else
			{
				Tiff.ErrorExt(tif, tif.m_clientdata, "vsetfield", "{0}: Sorry, cannot nest SubIFDs", tif.m_name);
				flag = false;
			}
			break;
		case TiffTag.YCBCRPOSITIONING:
			dir.td_ycbcrpositioning = (YCbCrPosition)value[0].ToShort();
			break;
		case TiffTag.YCBCRSUBSAMPLING:
			dir.td_ycbcrsubsampling[0] = value[0].ToShort();
			dir.td_ycbcrsubsampling[1] = value[1].ToShort();
			break;
		case TiffTag.TRANSFERFUNCTION:
		{
			v = ((dir.td_samplesperpixel - dir.td_extrasamples <= 1) ? 1 : 3);
			for (int j = 0; j < v; j++)
			{
				Tiff.setShortArray(out dir.td_transferfunction[j], value[0].ToShortArray(), 1 << (int)dir.td_bitspersample);
			}
			break;
		}
		case TiffTag.REFERENCEBLACKWHITE:
			Tiff.setFloatArray(out dir.td_refblackwhite, value[0].ToFloatArray(), 6);
			break;
		case TiffTag.INKNAMES:
		{
			v = value[0].ToInt();
			string text = value[1].ToString();
			v = checkInkNamesString(tif, v, text);
			flag = v > 0;
			if (v > 0)
			{
				setNString(out dir.td_inknames, text, v);
				dir.td_inknameslen = v;
			}
			break;
		}
		default:
		{
			TiffFieldInfo tiffFieldInfo = tif.FindFieldInfo(tag, TiffType.NOTYPE);
			if (tiffFieldInfo == null || tiffFieldInfo.Bit != 65)
			{
				Tiff.ErrorExt(tif, tif.m_clientdata, "vsetfield", "{0}: Invalid {1}tag \"{2}\" (not supported by codec)", tif.m_name, Tiff.isPseudoTag(tag) ? "pseudo-" : string.Empty, (tiffFieldInfo != null) ? tiffFieldInfo.Name : "Unknown");
				flag = false;
				break;
			}
			int num2 = -1;
			for (int i = 0; i < dir.td_customValueCount; i++)
			{
				if (dir.td_customValues[i].info.Tag == tag)
				{
					num2 = i;
					dir.td_customValues[i].value = null;
					break;
				}
			}
			if (num2 == -1)
			{
				dir.td_customValueCount++;
				TiffTagValue[] td_customValues = Tiff.Realloc(dir.td_customValues, dir.td_customValueCount - 1, dir.td_customValueCount);
				dir.td_customValues = td_customValues;
				num2 = dir.td_customValueCount - 1;
				dir.td_customValues[num2].info = tiffFieldInfo;
				dir.td_customValues[num2].value = null;
				dir.td_customValues[num2].count = 0;
			}
			int num3 = Tiff.dataSize(tiffFieldInfo.Type);
			if (num3 == 0)
			{
				flag = false;
				Tiff.ErrorExt(tif, tif.m_clientdata, "vsetfield", "{0}: Bad field type {1} for \"{2}\"", tif.m_name, tiffFieldInfo.Type, tiffFieldInfo.Name);
				flag2 = true;
				break;
			}
			int num4 = 0;
			if (tiffFieldInfo.PassCount)
			{
				if (tiffFieldInfo.WriteCount == -3)
				{
					dir.td_customValues[num2].count = value[num4++].ToInt();
				}
				else
				{
					dir.td_customValues[num2].count = value[num4++].ToInt();
				}
			}
			else if (tiffFieldInfo.WriteCount == -1 || tiffFieldInfo.WriteCount == -3)
			{
				dir.td_customValues[num2].count = 1;
			}
			else if (tiffFieldInfo.WriteCount == -2)
			{
				dir.td_customValues[num2].count = dir.td_samplesperpixel;
			}
			else
			{
				dir.td_customValues[num2].count = tiffFieldInfo.WriteCount;
			}
			if (tiffFieldInfo.Type == TiffType.ASCII)
			{
				Tiff.setString(out var cpp, value[num4++].ToString());
				dir.td_customValues[num2].value = Tiff.Latin1Encoding.GetBytes(cpp);
				break;
			}
			dir.td_customValues[num2].value = new byte[num3 * dir.td_customValues[num2].count];
			if ((tiffFieldInfo.PassCount || tiffFieldInfo.WriteCount == -1 || tiffFieldInfo.WriteCount == -3 || tiffFieldInfo.WriteCount == -2 || dir.td_customValues[num2].count > 1) && tiffFieldInfo.Tag != TiffTag.PAGENUMBER && tiffFieldInfo.Tag != TiffTag.HALFTONEHINTS && tiffFieldInfo.Tag != TiffTag.YCBCRSUBSAMPLING && tiffFieldInfo.Tag != TiffTag.DOTRANGE)
			{
				byte[] bytes = value[num4++].GetBytes();
				Buffer.BlockCopy(bytes, 0, dir.td_customValues[num2].value, 0, Math.Min(bytes.Length, dir.td_customValues[num2].value.Length));
				break;
			}
			byte[] value2 = dir.td_customValues[num2].value;
			int num5 = 0;
			int num6 = 0;
			while (num6 < dir.td_customValues[num2].count)
			{
				switch (tiffFieldInfo.Type)
				{
				case TiffType.BYTE:
				case TiffType.UNDEFINED:
					value2[num5] = value[num4 + num6].GetBytes()[0];
					break;
				case TiffType.SBYTE:
					value2[num5] = value[num4 + num6].ToByte();
					break;
				case TiffType.SHORT:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToShort()), 0, value2, num5, num3);
					break;
				case TiffType.SSHORT:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToShort()), 0, value2, num5, num3);
					break;
				case TiffType.LONG:
				case TiffType.IFD:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToInt()), 0, value2, num5, num3);
					break;
				case TiffType.SLONG:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToInt()), 0, value2, num5, num3);
					break;
				case TiffType.RATIONAL:
				case TiffType.SRATIONAL:
				case TiffType.FLOAT:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToFloat()), 0, value2, num5, num3);
					break;
				case TiffType.DOUBLE:
					Buffer.BlockCopy(BitConverter.GetBytes(value[num4 + num6].ToDouble()), 0, value2, num5, num3);
					break;
				default:
					Array.Clear(value2, num5, num3);
					flag = false;
					break;
				}
				num6++;
				num5 += num3;
			}
			break;
		}
		}
		if (!flag2 && !flag3 && !flag4 && flag)
		{
			tif.setFieldBit(tif.FieldWithTag(tag).Bit);
			tif.m_flags |= TiffFlags.DIRTYDIRECT;
		}
		if (flag3)
		{
			Tiff.ErrorExt(tif, tif.m_clientdata, "vsetfield", "{0}: Bad value {1} for \"{2}\" tag", tif.m_name, v, tif.FieldWithTag(tag).Name);
			return false;
		}
		if (flag4)
		{
			Tiff.ErrorExt(tif, tif.m_clientdata, "vsetfield", "{0}: Bad value {1} for \"{2}\" tag", tif.m_name, num, tif.FieldWithTag(tag).Name);
			return false;
		}
		return flag;
	}

	public virtual FieldValue[] GetField(Tiff tif, TiffTag tag)
	{
		TiffDirectory dir = tif.m_dir;
		FieldValue[] array = null;
		switch (tag)
		{
		case TiffTag.SUBFILETYPE:
			array = new FieldValue[1];
			array[0].Set(dir.td_subfiletype);
			break;
		case TiffTag.IMAGEWIDTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_imagewidth);
			break;
		case TiffTag.IMAGELENGTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_imagelength);
			break;
		case TiffTag.BITSPERSAMPLE:
			array = new FieldValue[1];
			array[0].Set(dir.td_bitspersample);
			break;
		case TiffTag.COMPRESSION:
			array = new FieldValue[1];
			array[0].Set(dir.td_compression);
			break;
		case TiffTag.PHOTOMETRIC:
			array = new FieldValue[1];
			array[0].Set(dir.td_photometric);
			break;
		case TiffTag.THRESHHOLDING:
			array = new FieldValue[1];
			array[0].Set(dir.td_threshholding);
			break;
		case TiffTag.FILLORDER:
			array = new FieldValue[1];
			array[0].Set(dir.td_fillorder);
			break;
		case TiffTag.ORIENTATION:
			array = new FieldValue[1];
			array[0].Set(dir.td_orientation);
			break;
		case TiffTag.SAMPLESPERPIXEL:
			array = new FieldValue[1];
			array[0].Set(dir.td_samplesperpixel);
			break;
		case TiffTag.ROWSPERSTRIP:
			array = new FieldValue[1];
			array[0].Set(dir.td_rowsperstrip);
			break;
		case TiffTag.MINSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_minsamplevalue);
			break;
		case TiffTag.MAXSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_maxsamplevalue);
			break;
		case TiffTag.SMINSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_sminsamplevalue);
			break;
		case TiffTag.SMAXSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_smaxsamplevalue);
			break;
		case TiffTag.XRESOLUTION:
			array = new FieldValue[1];
			array[0].Set(dir.td_xresolution);
			break;
		case TiffTag.YRESOLUTION:
			array = new FieldValue[1];
			array[0].Set(dir.td_yresolution);
			break;
		case TiffTag.PLANARCONFIG:
			array = new FieldValue[1];
			array[0].Set(dir.td_planarconfig);
			break;
		case TiffTag.XPOSITION:
			array = new FieldValue[1];
			array[0].Set(dir.td_xposition);
			break;
		case TiffTag.YPOSITION:
			array = new FieldValue[1];
			array[0].Set(dir.td_yposition);
			break;
		case TiffTag.RESOLUTIONUNIT:
			array = new FieldValue[1];
			array[0].Set(dir.td_resolutionunit);
			break;
		case TiffTag.PAGENUMBER:
			array = new FieldValue[2];
			array[0].Set(dir.td_pagenumber[0]);
			array[1].Set(dir.td_pagenumber[1]);
			break;
		case TiffTag.HALFTONEHINTS:
			array = new FieldValue[2];
			array[0].Set(dir.td_halftonehints[0]);
			array[1].Set(dir.td_halftonehints[1]);
			break;
		case TiffTag.COLORMAP:
			array = new FieldValue[3];
			array[0].Set(dir.td_colormap[0]);
			array[1].Set(dir.td_colormap[1]);
			array[2].Set(dir.td_colormap[2]);
			break;
		case TiffTag.STRIPOFFSETS:
		case TiffTag.TILEOFFSETS:
			array = new FieldValue[1];
			array[0].Set(dir.td_stripoffset);
			break;
		case TiffTag.STRIPBYTECOUNTS:
		case TiffTag.TILEBYTECOUNTS:
			array = new FieldValue[1];
			array[0].Set(dir.td_stripbytecount);
			break;
		case TiffTag.MATTEING:
			array = new FieldValue[1];
			array[0].Set(dir.td_extrasamples == 1 && dir.td_sampleinfo[0] == ExtraSample.ASSOCALPHA);
			break;
		case TiffTag.EXTRASAMPLES:
			array = new FieldValue[2];
			array[0].Set(dir.td_extrasamples);
			array[1].Set(dir.td_sampleinfo);
			break;
		case TiffTag.TILEWIDTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_tilewidth);
			break;
		case TiffTag.TILELENGTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_tilelength);
			break;
		case TiffTag.TILEDEPTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_tiledepth);
			break;
		case TiffTag.DATATYPE:
			switch (dir.td_sampleformat)
			{
			case SampleFormat.UINT:
				array = new FieldValue[1];
				array[0].Set((short)2);
				break;
			case SampleFormat.INT:
				array = new FieldValue[1];
				array[0].Set((short)1);
				break;
			case SampleFormat.IEEEFP:
				array = new FieldValue[1];
				array[0].Set((short)3);
				break;
			case SampleFormat.VOID:
				array = new FieldValue[1];
				array[0].Set((short)0);
				break;
			}
			break;
		case TiffTag.SAMPLEFORMAT:
			array = new FieldValue[1];
			array[0].Set(dir.td_sampleformat);
			break;
		case TiffTag.IMAGEDEPTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_imagedepth);
			break;
		case TiffTag.SUBIFD:
			array = new FieldValue[2];
			array[0].Set(dir.td_nsubifd);
			array[1].Set(dir.td_subifd);
			break;
		case TiffTag.YCBCRPOSITIONING:
			array = new FieldValue[1];
			array[0].Set(dir.td_ycbcrpositioning);
			break;
		case TiffTag.YCBCRSUBSAMPLING:
			array = new FieldValue[2];
			array[0].Set(dir.td_ycbcrsubsampling[0]);
			array[1].Set(dir.td_ycbcrsubsampling[1]);
			break;
		case TiffTag.TRANSFERFUNCTION:
			array = new FieldValue[3];
			array[0].Set(dir.td_transferfunction[0]);
			if (dir.td_samplesperpixel - dir.td_extrasamples > 1)
			{
				array[1].Set(dir.td_transferfunction[1]);
				array[2].Set(dir.td_transferfunction[2]);
			}
			break;
		case TiffTag.REFERENCEBLACKWHITE:
			if (dir.td_refblackwhite != null)
			{
				array = new FieldValue[1];
				array[0].Set(dir.td_refblackwhite);
			}
			break;
		case TiffTag.INKNAMES:
			array = new FieldValue[1];
			array[0].Set(dir.td_inknames);
			break;
		default:
		{
			TiffFieldInfo tiffFieldInfo = tif.FindFieldInfo(tag, TiffType.NOTYPE);
			if (tiffFieldInfo == null || tiffFieldInfo.Bit != 65)
			{
				Tiff.ErrorExt(tif, tif.m_clientdata, "_TIFFVGetField", "{0}: Invalid {1}tag \"{2}\" (not supported by codec)", tif.m_name, Tiff.isPseudoTag(tag) ? "pseudo-" : string.Empty, (tiffFieldInfo != null) ? tiffFieldInfo.Name : "Unknown");
				array = null;
				break;
			}
			array = null;
			for (int i = 0; i < dir.td_customValueCount; i++)
			{
				TiffTagValue tiffTagValue = dir.td_customValues[i];
				if (tiffTagValue.info.Tag != tag)
				{
					continue;
				}
				if (tiffFieldInfo.PassCount)
				{
					array = new FieldValue[2];
					if (tiffFieldInfo.ReadCount == -3)
					{
						array[0].Set(tiffTagValue.count);
					}
					else
					{
						array[0].Set(tiffTagValue.count);
					}
					array[1].Set(tiffTagValue.value);
					break;
				}
				if ((tiffFieldInfo.Type == TiffType.ASCII || tiffFieldInfo.ReadCount == -1 || tiffFieldInfo.ReadCount == -3 || tiffFieldInfo.ReadCount == -2 || tiffTagValue.count > 1) && tiffFieldInfo.Tag != TiffTag.PAGENUMBER && tiffFieldInfo.Tag != TiffTag.HALFTONEHINTS && tiffFieldInfo.Tag != TiffTag.YCBCRSUBSAMPLING && tiffFieldInfo.Tag != TiffTag.DOTRANGE)
				{
					array = new FieldValue[1];
					byte[] array2 = tiffTagValue.value;
					if (tiffFieldInfo.Type == TiffType.ASCII && tiffTagValue.value.Length != 0 && tiffTagValue.value[tiffTagValue.value.Length - 1] == 0)
					{
						array2 = new byte[Math.Max(tiffTagValue.value.Length - 1, 0)];
						Buffer.BlockCopy(tiffTagValue.value, 0, array2, 0, array2.Length);
					}
					array[0].Set(array2);
					break;
				}
				array = new FieldValue[tiffTagValue.count];
				byte[] value = tiffTagValue.value;
				int num = 0;
				int num2 = 0;
				while (num2 < tiffTagValue.count)
				{
					switch (tiffFieldInfo.Type)
					{
					case TiffType.BYTE:
					case TiffType.SBYTE:
					case TiffType.UNDEFINED:
						array[num2].Set(value[num]);
						break;
					case TiffType.SHORT:
					case TiffType.SSHORT:
						array[num2].Set(BitConverter.ToInt16(value, num));
						break;
					case TiffType.LONG:
					case TiffType.SLONG:
					case TiffType.IFD:
						array[num2].Set(BitConverter.ToInt32(value, num));
						break;
					case TiffType.RATIONAL:
					case TiffType.SRATIONAL:
					case TiffType.FLOAT:
						array[num2].Set(BitConverter.ToSingle(value, num));
						break;
					case TiffType.DOUBLE:
						array[num2].Set(BitConverter.ToDouble(value, num));
						break;
					default:
						array = null;
						break;
					}
					num2++;
					num += Tiff.dataSize(tiffTagValue.info.Type);
				}
				break;
			}
			break;
		}
		}
		return array;
	}

	public virtual void PrintDir(Tiff tif, Stream stream, TiffPrintFlags flags)
	{
	}

	private static bool setExtraSamples(TiffDirectory td, ref int v, FieldValue[] ap)
	{
		v = ap[0].ToInt();
		if (v > td.td_samplesperpixel)
		{
			return false;
		}
		byte[] array = ap[1].ToByteArray();
		if (v > 0 && array == null)
		{
			return false;
		}
		for (int i = 0; i < v; i++)
		{
			if (array[i] > 2)
			{
				if (i >= v - 1)
				{
					return false;
				}
				if (BitConverter.ToInt16(array, i) == 999)
				{
					array[i] = 2;
				}
			}
		}
		td.td_extrasamples = (short)v;
		td.td_sampleinfo = new ExtraSample[td.td_extrasamples];
		for (int j = 0; j < td.td_extrasamples; j++)
		{
			td.td_sampleinfo[j] = (ExtraSample)array[j];
		}
		return true;
	}

	private static int checkInkNamesString(Tiff tif, int slen, string s)
	{
		bool flag = false;
		short num = tif.m_dir.td_samplesperpixel;
		if (slen > 0)
		{
			int i = 0;
			while (num > 0)
			{
				for (; s[i] != 0; i++)
				{
					if (i >= slen)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				i++;
				num--;
			}
			if (!flag)
			{
				return i;
			}
		}
		Tiff.ErrorExt(tif, tif.m_clientdata, "TIFFSetField", "{0}: Invalid InkNames value; expecting {1} names, found {2}", tif.m_name, tif.m_dir.td_samplesperpixel, tif.m_dir.td_samplesperpixel - num);
		return 0;
	}

	private static void setNString(out string cpp, string cp, int n)
	{
		cpp = cp.Substring(0, n);
	}
}

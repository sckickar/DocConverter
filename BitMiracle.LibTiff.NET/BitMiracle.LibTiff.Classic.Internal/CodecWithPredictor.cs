using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class CodecWithPredictor : TiffCodec
{
	private enum PredictorType
	{
		ptNone,
		ptHorAcc8,
		ptHorAcc16,
		ptHorAcc32,
		ptSwabHorAcc16,
		ptSwabHorAcc32,
		ptHorDiff8,
		ptHorDiff16,
		ptHorDiff32,
		ptFpAcc,
		ptFpDiff
	}

	public const int FIELD_PREDICTOR = 66;

	private static readonly TiffFieldInfo[] m_predictFieldInfo = new TiffFieldInfo[1]
	{
		new TiffFieldInfo(TiffTag.PREDICTOR, 1, 1, TiffType.SHORT, 66, okToChange: false, passCount: false, "Predictor")
	};

	private Predictor m_predictor;

	private int m_stride;

	private int m_rowSize;

	private TiffTagMethods m_parentTagMethods;

	private TiffTagMethods m_tagMethods;

	private TiffTagMethods m_childTagMethods;

	private bool m_passThruDecode;

	private bool m_passThruEncode;

	private PredictorType m_predictorType;

	public CodecWithPredictor(Tiff tif, Compression scheme, string name)
		: base(tif, scheme, name)
	{
		m_tagMethods = new CodecWithPredictorTagMethods();
	}

	public void TIFFPredictorInit(TiffTagMethods tagMethods)
	{
		m_tif.MergeFieldInfo(m_predictFieldInfo, m_predictFieldInfo.Length);
		m_childTagMethods = tagMethods;
		m_parentTagMethods = m_tif.m_tagmethods;
		m_tif.m_tagmethods = m_tagMethods;
		m_predictor = Predictor.NONE;
		m_predictorType = PredictorType.ptNone;
	}

	public void TIFFPredictorCleanup()
	{
		m_tif.m_tagmethods = m_parentTagMethods;
	}

	public override bool SetupDecode()
	{
		return PredictorSetupDecode();
	}

	public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruDecode)
		{
			return PredictorDecodeRow(buffer, offset, count, plane);
		}
		return predictor_decoderow(buffer, offset, count, plane);
	}

	public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruDecode)
		{
			return PredictorDecodeTile(buffer, offset, count, plane);
		}
		return predictor_decodestrip(buffer, offset, count, plane);
	}

	public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruDecode)
		{
			return PredictorDecodeTile(buffer, offset, count, plane);
		}
		return predictor_decodetile(buffer, offset, count, plane);
	}

	public override bool SetupEncode()
	{
		return PredictorSetupEncode();
	}

	public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruEncode)
		{
			return PredictorEncodeRow(buffer, offset, count, plane);
		}
		return predictor_encoderow(buffer, offset, count, plane);
	}

	public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruEncode)
		{
			return PredictorEncodeTile(buffer, offset, count, plane);
		}
		return predictor_encodestrip(buffer, offset, count, plane);
	}

	public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		if (!m_passThruEncode)
		{
			return PredictorEncodeTile(buffer, offset, count, plane);
		}
		return predictor_encodetile(buffer, offset, count, plane);
	}

	public virtual bool predictor_setupdecode()
	{
		return base.SetupDecode();
	}

	public virtual bool predictor_decoderow(byte[] buffer, int offset, int count, short plane)
	{
		return base.DecodeRow(buffer, offset, count, plane);
	}

	public virtual bool predictor_decodestrip(byte[] buffer, int offset, int count, short plane)
	{
		return base.DecodeStrip(buffer, offset, count, plane);
	}

	public virtual bool predictor_decodetile(byte[] buffer, int offset, int count, short plane)
	{
		return base.DecodeTile(buffer, offset, count, plane);
	}

	public virtual bool predictor_setupencode()
	{
		return base.SetupEncode();
	}

	public virtual bool predictor_encoderow(byte[] buffer, int offset, int count, short plane)
	{
		return base.EncodeRow(buffer, offset, count, plane);
	}

	public virtual bool predictor_encodestrip(byte[] buffer, int offset, int count, short plane)
	{
		return base.EncodeStrip(buffer, offset, count, plane);
	}

	public virtual bool predictor_encodetile(byte[] buffer, int offset, int count, short plane)
	{
		return base.EncodeTile(buffer, offset, count, plane);
	}

	public Predictor GetPredictorValue()
	{
		return m_predictor;
	}

	public void SetPredictorValue(Predictor value)
	{
		m_predictor = value;
	}

	public TiffTagMethods GetChildTagMethods()
	{
		return m_childTagMethods;
	}

	private void predictorFunc(byte[] buffer, int offset, int count)
	{
		switch (m_predictorType)
		{
		case PredictorType.ptHorAcc8:
			horAcc8(buffer, offset, count);
			break;
		case PredictorType.ptHorAcc16:
			horAcc16(buffer, offset, count);
			break;
		case PredictorType.ptHorAcc32:
			horAcc32(buffer, offset, count);
			break;
		case PredictorType.ptSwabHorAcc16:
			swabHorAcc16(buffer, offset, count);
			break;
		case PredictorType.ptSwabHorAcc32:
			swabHorAcc32(buffer, offset, count);
			break;
		case PredictorType.ptHorDiff8:
			horDiff8(buffer, offset, count);
			break;
		case PredictorType.ptHorDiff16:
			horDiff16(buffer, offset, count);
			break;
		case PredictorType.ptHorDiff32:
			horDiff32(buffer, offset, count);
			break;
		case PredictorType.ptFpAcc:
			fpAcc(buffer, offset, count);
			break;
		case PredictorType.ptFpDiff:
			fpDiff(buffer, offset, count);
			break;
		}
	}

	private void horAcc8(byte[] buffer, int offset, int count)
	{
		int num = offset;
		if (count <= m_stride)
		{
			return;
		}
		count -= m_stride;
		if (m_stride == 3)
		{
			int num2 = buffer[num];
			int num3 = buffer[num + 1];
			int num4 = buffer[num + 2];
			do
			{
				count -= 3;
				num += 3;
				num2 += buffer[num];
				buffer[num] = (byte)num2;
				num3 += buffer[num + 1];
				buffer[num + 1] = (byte)num3;
				num4 += buffer[num + 2];
				buffer[num + 2] = (byte)num4;
			}
			while (count > 0);
			return;
		}
		if (m_stride == 4)
		{
			int num5 = buffer[num];
			int num6 = buffer[num + 1];
			int num7 = buffer[num + 2];
			int num8 = buffer[num + 3];
			do
			{
				count -= 4;
				num += 4;
				num5 += buffer[num];
				buffer[num] = (byte)num5;
				num6 += buffer[num + 1];
				buffer[num + 1] = (byte)num6;
				num7 += buffer[num + 2];
				buffer[num + 2] = (byte)num7;
				num8 += buffer[num + 3];
				buffer[num + 3] = (byte)num8;
			}
			while (count > 0);
			return;
		}
		do
		{
			for (int num9 = m_stride; num9 > 0; num9--)
			{
				buffer[num + m_stride] = (byte)(buffer[num + m_stride] + buffer[num]);
				num++;
			}
			count -= m_stride;
		}
		while (count > 0);
	}

	private void horAcc16(byte[] buffer, int offset, int count)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, count);
		int num = 0;
		int num2 = count / 2;
		if (num2 > m_stride)
		{
			num2 -= m_stride;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] += array[num];
					num++;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.ShortsToByteArray(array, 0, count / 2, buffer, offset);
	}

	private void horAcc32(byte[] buffer, int offset, int count)
	{
		int[] array = Tiff.ByteArrayToInts(buffer, offset, count);
		int num = 0;
		int num2 = count / 4;
		if (num2 > m_stride)
		{
			num2 -= m_stride;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] += array[num];
					num++;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.IntsToByteArray(array, 0, count / 4, buffer, offset);
	}

	private void swabHorAcc16(byte[] buffer, int offset, int count)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, count);
		int num = 0;
		int num2 = count / 2;
		if (num2 > m_stride)
		{
			Tiff.SwabArrayOfShort(array, num2);
			num2 -= m_stride;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] += array[num];
					num++;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.ShortsToByteArray(array, 0, count / 2, buffer, offset);
	}

	private void swabHorAcc32(byte[] buffer, int offset, int count)
	{
		int[] array = Tiff.ByteArrayToInts(buffer, offset, count);
		int num = 0;
		int num2 = count / 4;
		if (num2 > m_stride)
		{
			Tiff.SwabArrayOfLong(array, num2);
			num2 -= m_stride;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] += array[num];
					num++;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.IntsToByteArray(array, 0, count / 4, buffer, offset);
	}

	private void horDiff8(byte[] buffer, int offset, int count)
	{
		if (count <= m_stride)
		{
			return;
		}
		count -= m_stride;
		int num = offset;
		if (m_stride == 3)
		{
			int num2 = buffer[num];
			int num3 = buffer[num + 1];
			int num4 = buffer[num + 2];
			do
			{
				int num5 = buffer[num + 3];
				buffer[num + 3] = (byte)(num5 - num2);
				num2 = num5;
				int num6 = buffer[num + 4];
				buffer[num + 4] = (byte)(num6 - num3);
				num3 = num6;
				int num7 = buffer[num + 5];
				buffer[num + 5] = (byte)(num7 - num4);
				num4 = num7;
				num += 3;
			}
			while ((count -= 3) > 0);
			return;
		}
		if (m_stride == 4)
		{
			int num8 = buffer[num];
			int num9 = buffer[num + 1];
			int num10 = buffer[num + 2];
			int num11 = buffer[num + 3];
			do
			{
				int num12 = buffer[num + 4];
				buffer[num + 4] = (byte)(num12 - num8);
				num8 = num12;
				int num13 = buffer[num + 5];
				buffer[num + 5] = (byte)(num13 - num9);
				num9 = num13;
				int num14 = buffer[num + 6];
				buffer[num + 6] = (byte)(num14 - num10);
				num10 = num14;
				int num15 = buffer[num + 7];
				buffer[num + 7] = (byte)(num15 - num11);
				num11 = num15;
				num += 4;
			}
			while ((count -= 4) > 0);
			return;
		}
		num += count - 1;
		do
		{
			for (int num16 = m_stride; num16 > 0; num16--)
			{
				buffer[num + m_stride] -= buffer[num];
				num--;
			}
		}
		while ((count -= m_stride) > 0);
	}

	private void horDiff16(byte[] buffer, int offset, int count)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, count);
		int num = 0;
		int num2 = count / 2;
		if (num2 > m_stride)
		{
			num2 -= m_stride;
			num += num2 - 1;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] -= array[num];
					num--;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.ShortsToByteArray(array, 0, count / 2, buffer, offset);
	}

	private void horDiff32(byte[] buffer, int offset, int count)
	{
		int[] array = Tiff.ByteArrayToInts(buffer, offset, count);
		int num = 0;
		int num2 = count / 4;
		if (num2 > m_stride)
		{
			num2 -= m_stride;
			num += num2 - 1;
			do
			{
				for (int num3 = m_stride; num3 > 0; num3--)
				{
					array[num + m_stride] -= array[num];
					num--;
				}
				num2 -= m_stride;
			}
			while (num2 > 0);
		}
		Tiff.IntsToByteArray(array, 0, count / 4, buffer, offset);
	}

	private void fpAcc(byte[] buffer, int offset, int count)
	{
		int num = m_tif.m_dir.td_bitspersample / 8;
		int num2 = count / num;
		int num3 = count;
		int num4 = offset;
		while (num3 > m_stride)
		{
			for (int num5 = m_stride; num5 > 0; num5--)
			{
				buffer[num4 + m_stride] += buffer[num4];
				num4++;
			}
			num3 -= m_stride;
		}
		byte[] array = new byte[count];
		Buffer.BlockCopy(buffer, offset, array, 0, count);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				buffer[offset + num * i + j] = array[(num - j - 1) * num2 + i];
			}
		}
	}

	private void fpDiff(byte[] buffer, int offset, int count)
	{
		byte[] array = new byte[count];
		Buffer.BlockCopy(buffer, offset, array, 0, count);
		int num = m_tif.m_dir.td_bitspersample / 8;
		int num2 = count / num;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				buffer[offset + (num - j - 1) * num2 + i] = array[num * i + j];
			}
		}
		int num3 = offset + count - m_stride - 1;
		for (int num4 = count; num4 > m_stride; num4 -= m_stride)
		{
			for (int num5 = m_stride; num5 > 0; num5--)
			{
				buffer[num3 + m_stride] -= buffer[num3];
				num3--;
			}
		}
	}

	private bool PredictorDecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		if (predictor_decoderow(buffer, offset, count, plane))
		{
			predictorFunc(buffer, offset, count);
			return true;
		}
		return false;
	}

	private bool PredictorDecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		if (predictor_decodetile(buffer, offset, count, plane))
		{
			while (count > 0)
			{
				predictorFunc(buffer, offset, m_rowSize);
				count -= m_rowSize;
				offset += m_rowSize;
			}
			return true;
		}
		return false;
	}

	private bool PredictorEncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		predictorFunc(buffer, offset, count);
		return predictor_encoderow(buffer, offset, count, plane);
	}

	private bool PredictorEncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		byte[] array = new byte[count];
		Buffer.BlockCopy(buffer, 0, array, 0, count);
		int num = count;
		while (num > 0)
		{
			predictorFunc(array, offset, m_rowSize);
			num -= m_rowSize;
			offset += m_rowSize;
		}
		return predictor_encodetile(array, 0, count, plane);
	}

	private bool PredictorSetupDecode()
	{
		if (!predictor_setupdecode() || !PredictorSetup())
		{
			return false;
		}
		m_passThruDecode = true;
		if (m_predictor == Predictor.HORIZONTAL)
		{
			switch (m_tif.m_dir.td_bitspersample)
			{
			case 8:
				m_predictorType = PredictorType.ptHorAcc8;
				break;
			case 16:
				m_predictorType = PredictorType.ptHorAcc16;
				break;
			case 32:
				m_predictorType = PredictorType.ptHorAcc32;
				break;
			}
			m_passThruDecode = false;
			if ((m_tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				if (m_predictorType == PredictorType.ptHorAcc16)
				{
					m_predictorType = PredictorType.ptSwabHorAcc16;
					m_tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmNone;
				}
				else if (m_predictorType == PredictorType.ptHorAcc32)
				{
					m_predictorType = PredictorType.ptSwabHorAcc32;
					m_tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmNone;
				}
			}
		}
		else if (m_predictor == Predictor.FLOATINGPOINT)
		{
			m_predictorType = PredictorType.ptFpAcc;
			m_passThruDecode = false;
			if ((m_tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				m_tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmNone;
			}
		}
		return true;
	}

	private bool PredictorSetupEncode()
	{
		if (!predictor_setupencode() || !PredictorSetup())
		{
			return false;
		}
		m_passThruEncode = true;
		if (m_predictor == Predictor.HORIZONTAL)
		{
			switch (m_tif.m_dir.td_bitspersample)
			{
			case 8:
				m_predictorType = PredictorType.ptHorDiff8;
				break;
			case 16:
				m_predictorType = PredictorType.ptHorDiff16;
				break;
			case 32:
				m_predictorType = PredictorType.ptHorDiff32;
				break;
			}
			m_passThruEncode = false;
		}
		else if (m_predictor == Predictor.FLOATINGPOINT)
		{
			m_predictorType = PredictorType.ptFpDiff;
			m_passThruEncode = false;
		}
		return true;
	}

	private bool PredictorSetup()
	{
		TiffDirectory dir = m_tif.m_dir;
		switch (m_predictor)
		{
		case Predictor.NONE:
			return true;
		case Predictor.HORIZONTAL:
			if (dir.td_bitspersample != 8 && dir.td_bitspersample != 16 && dir.td_bitspersample != 32)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "PredictorSetup", "Horizontal differencing \"Predictor\" not supported with {0}-bit samples", dir.td_bitspersample);
				return false;
			}
			break;
		case Predictor.FLOATINGPOINT:
			if (dir.td_sampleformat != SampleFormat.IEEEFP)
			{
				Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "PredictorSetup", "Floating point \"Predictor\" not supported with {0} data format", dir.td_sampleformat);
				return false;
			}
			break;
		default:
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "PredictorSetup", "\"Predictor\" value {0} not supported", m_predictor);
			return false;
		}
		m_stride = ((dir.td_planarconfig != PlanarConfig.CONTIG) ? 1 : dir.td_samplesperpixel);
		if (m_tif.IsTiled())
		{
			m_rowSize = m_tif.TileRowSize();
		}
		else
		{
			m_rowSize = m_tif.ScanlineSize();
		}
		return true;
	}
}

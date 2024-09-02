namespace BitMiracle.LibTiff.Classic.Internal;

internal class DeflateCodecTagMethods : TiffTagMethods
{
	public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
	{
		DeflateCodec deflateCodec = tif.m_currentCodec as DeflateCodec;
		if (tag == TiffTag.ZIPQUALITY)
		{
			deflateCodec.m_zipquality = ap[0].ToInt();
			if (((uint)deflateCodec.m_state & 2u) != 0 && deflateCodec.m_stream.deflateParams(deflateCodec.m_zipquality, 0) != 0)
			{
				Tiff.ErrorExt(tif, tif.m_clientdata, "ZIPVSetField", "{0}: zlib error: {0}", tif.m_name, deflateCodec.m_stream.msg);
				return false;
			}
			return true;
		}
		return base.SetField(tif, tag, ap);
	}

	public override FieldValue[] GetField(Tiff tif, TiffTag tag)
	{
		DeflateCodec deflateCodec = tif.m_currentCodec as DeflateCodec;
		if (tag == TiffTag.ZIPQUALITY)
		{
			FieldValue[] array = new FieldValue[1];
			array[0].Set(deflateCodec.m_zipquality);
			return array;
		}
		return base.GetField(tif, tag);
	}
}

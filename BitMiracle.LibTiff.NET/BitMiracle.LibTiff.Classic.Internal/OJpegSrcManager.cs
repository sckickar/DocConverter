using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class OJpegSrcManager : jpeg_source_mgr
{
	protected OJpegCodec m_sp;

	public OJpegSrcManager(OJpegCodec sp)
	{
		initInternalBuffer(null, 0);
		m_sp = sp;
	}

	public override void init_source()
	{
	}

	public override bool fill_input_buffer()
	{
		Tiff tiff = m_sp.GetTiff();
		byte[] mem = null;
		uint len = 0u;
		if (!m_sp.OJPEGWriteStream(out mem, out len))
		{
			Tiff.ErrorExt(tiff, tiff.m_clientdata, "LibJpeg", "Premature end of JPEG data");
		}
		initInternalBuffer(mem, (int)len);
		return true;
	}

	public override void skip_input_data(int num_bytes)
	{
		Tiff tiff = m_sp.GetTiff();
		Tiff.ErrorExt(tiff, tiff.m_clientdata, "LibJpeg", "Unexpected error");
	}

	public override bool resync_to_restart(jpeg_decompress_struct cinfo, int desired)
	{
		Tiff tiff = m_sp.GetTiff();
		Tiff.ErrorExt(tiff, tiff.m_clientdata, "LibJpeg", "Unexpected error");
		return false;
	}

	public override void term_source()
	{
	}
}

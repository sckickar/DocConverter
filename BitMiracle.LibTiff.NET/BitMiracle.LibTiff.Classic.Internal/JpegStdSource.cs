using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegStdSource : jpeg_source_mgr
{
	private static readonly byte[] dummy_EOI = new byte[2] { 255, 217 };

	protected JpegCodec m_sp;

	public JpegStdSource(JpegCodec sp)
	{
		initInternalBuffer(null, 0);
		m_sp = sp;
	}

	public override void init_source()
	{
		Tiff tiff = m_sp.GetTiff();
		initInternalBuffer(tiff.m_rawdata, tiff.m_rawcc);
	}

	public override bool fill_input_buffer()
	{
		m_sp.m_decompression.WARNMS(J_MESSAGE_CODE.JWRN_JPEG_EOF);
		initInternalBuffer(dummy_EOI, 2);
		return true;
	}
}

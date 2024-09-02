using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegStdDestination : jpeg_destination_mgr
{
	private Tiff m_tif;

	public JpegStdDestination(Tiff tif)
	{
		m_tif = tif;
	}

	public override void init_destination()
	{
		initInternalBuffer(m_tif.m_rawdata, 0);
	}

	public override bool empty_output_buffer()
	{
		m_tif.m_rawcc = m_tif.m_rawdatasize;
		m_tif.flushData1();
		initInternalBuffer(m_tif.m_rawdata, 0);
		return true;
	}

	public override void term_destination()
	{
		m_tif.m_rawcp = m_tif.m_rawdatasize - base.freeInBuffer;
		m_tif.m_rawcc = m_tif.m_rawdatasize - base.freeInBuffer;
	}
}

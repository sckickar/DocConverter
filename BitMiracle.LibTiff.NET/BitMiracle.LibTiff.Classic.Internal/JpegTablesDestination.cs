using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegTablesDestination : jpeg_destination_mgr
{
	private JpegCodec m_sp;

	public JpegTablesDestination(JpegCodec sp)
	{
		m_sp = sp;
	}

	public override void init_destination()
	{
		initInternalBuffer(m_sp.m_jpegtables, 0);
	}

	public override bool empty_output_buffer()
	{
		byte[] array = Tiff.Realloc(m_sp.m_jpegtables, m_sp.m_jpegtables_length + 1000);
		initInternalBuffer(array, m_sp.m_jpegtables_length);
		m_sp.m_jpegtables = array;
		m_sp.m_jpegtables_length += 1000;
		return true;
	}

	public override void term_destination()
	{
		m_sp.m_jpegtables_length -= base.freeInBuffer;
	}
}

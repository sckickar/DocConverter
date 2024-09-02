namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegTablesSource : JpegStdSource
{
	public JpegTablesSource(JpegCodec sp)
		: base(sp)
	{
	}

	public override void init_source()
	{
		initInternalBuffer(m_sp.m_jpegtables, m_sp.m_jpegtables_length);
	}
}

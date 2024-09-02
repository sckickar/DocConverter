using System;
using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class OJpegErrorManager : jpeg_error_mgr
{
	private OJpegCodec m_sp;

	public OJpegErrorManager(OJpegCodec sp)
	{
		m_sp = sp;
	}

	public override void error_exit()
	{
		string text = format_message();
		Tiff.ErrorExt(m_sp.GetTiff(), m_sp.GetTiff().m_clientdata, "LibJpeg", "{0}", text);
		m_sp.m_libjpeg_jpeg_decompress_struct.jpeg_abort();
		throw new Exception(text);
	}

	public override void output_message()
	{
		string text = format_message();
		Tiff.WarningExt(m_sp.GetTiff(), m_sp.GetTiff().m_clientdata, "LibJpeg", "{0}", text);
	}
}

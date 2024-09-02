using System;
using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class JpegErrorManager : jpeg_error_mgr
{
	private JpegCodec m_sp;

	public JpegErrorManager(JpegCodec sp)
	{
		m_sp = sp;
	}

	public override void error_exit()
	{
		string text = format_message();
		Tiff.ErrorExt(m_sp.GetTiff(), m_sp.GetTiff().m_clientdata, "JPEGLib", "{0}", text);
		m_sp.m_common.jpeg_abort();
		throw new Exception(text);
	}

	public override void output_message()
	{
		string text = format_message();
		Tiff.WarningExt(m_sp.GetTiff(), m_sp.GetTiff().m_clientdata, "JPEGLib", "{0}", text);
	}
}

using System;

namespace DocGen.DocIO.DLS;

public class HtmlConverterFactory
{
	[ThreadStatic]
	private static IHtmlConverter s_htmlConverter;

	public static IHtmlConverter GetInstance()
	{
		if (s_htmlConverter == null)
		{
			s_htmlConverter = new HTMLConverterImpl();
		}
		return s_htmlConverter;
	}

	public static void Register(IHtmlConverter converter)
	{
		if (converter == null)
		{
			throw new ArgumentNullException("convertor");
		}
		s_htmlConverter = converter;
	}
}

using System;

namespace DocGen.PdfViewer.Base;

internal class EncodingConverter : IConverter
{
	public object Convert(Type resultType, object value)
	{
		if (value is PostScriptArray)
		{
			return value;
		}
		if (value is string)
		{
			return PresettedEncodings.CreateEncoding((string)value);
		}
		return null;
	}
}

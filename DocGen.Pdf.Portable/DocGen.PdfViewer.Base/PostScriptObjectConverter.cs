using System;

namespace DocGen.PdfViewer.Base;

internal class PostScriptObjectConverter : IConverter
{
	public object Convert(Type resultType, object value)
	{
		if (!(value is PostScriptDict fromDict))
		{
			return null;
		}
		if (!(Activator.CreateInstance(resultType) is PostScriptObj postScriptObj))
		{
			return null;
		}
		postScriptObj.Load(fromDict);
		return postScriptObj;
	}
}

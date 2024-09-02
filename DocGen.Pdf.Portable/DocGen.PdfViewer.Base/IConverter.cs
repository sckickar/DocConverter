using System;

namespace DocGen.PdfViewer.Base;

internal interface IConverter
{
	object Convert(Type resultType, object value);
}

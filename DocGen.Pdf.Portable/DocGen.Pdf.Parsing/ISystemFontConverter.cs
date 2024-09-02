using System;

namespace DocGen.Pdf.Parsing;

internal interface ISystemFontConverter
{
	object Convert(Type resultType, object value);
}

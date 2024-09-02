using System;

namespace DocGen.Pdf;

internal class PdfAutomaticFieldInfoCollection : PdfCollection
{
	public int Add(PdfAutomaticFieldInfo fieldInfo)
	{
		if (fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		base.List.Add(fieldInfo);
		return base.List.Count - 1;
	}
}

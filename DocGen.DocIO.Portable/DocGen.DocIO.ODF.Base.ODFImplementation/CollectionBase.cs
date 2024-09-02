using System;
using System.Collections;
using System.Globalization;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CollectionBase<T>
{
	internal static string GenerateDefaultName(string strStart, params ICollection[] arrCollections)
	{
		int num = 1;
		strStart = strStart.ToUpper();
		int length = strStart.Length;
		int i = 0;
		for (int num2 = arrCollections.Length; i < num2; i++)
		{
			foreach (object item in arrCollections[i])
			{
				string text = ((!(item is INamedObject)) ? item.ToString() : (item as INamedObject).Name);
				if (text.StartsWith(strStart))
				{
					string text2 = text.Substring(length, text.Length - length);
					if (double.TryParse(text2, NumberStyles.Integer, null, out var result))
					{
						num = Math.Max((int)result + 1, num);
					}
					else if (text2 == "")
					{
						num++;
					}
				}
			}
		}
		return strStart + num;
	}
}

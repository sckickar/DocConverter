using System.Collections.Generic;
using System.Globalization;

namespace DocGen.OfficeChart.Implementation;

public static class HelperMethods
{
	public static string ToUpper(this string strValue, CultureInfo culture)
	{
		if (CultureInfo.InvariantCulture.Equals(culture))
		{
			return strValue.ToUpperInvariant();
		}
		return strValue.ToUpper();
	}

	public static string ToLower(this string strValue, CultureInfo culture)
	{
		if (CultureInfo.InvariantCulture.Equals(culture))
		{
			return strValue.ToLowerInvariant();
		}
		return strValue.ToLower();
	}

	public static T[] ToArray<T>(this IEnumerable<T> enumObject)
	{
		IEnumerator<T> enumerator = enumObject.GetEnumerator();
		IList<T> list = new List<T>();
		while (enumerator.MoveNext())
		{
			list.Add(enumerator.Current);
		}
		T[] array = new T[list.Count];
		list.CopyTo(array, 0);
		return array;
	}
}

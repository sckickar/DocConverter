using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontRealByteEncoding : SystemFontByteEncoding
{
	private static Dictionary<SystemFontNibble, string> nibbleMapping;

	private static SystemFontNibble endOfNumber;

	static SystemFontRealByteEncoding()
	{
		nibbleMapping = new Dictionary<SystemFontNibble, string>();
		nibbleMapping[new SystemFontNibble(0)] = "0";
		nibbleMapping[new SystemFontNibble(1)] = "1";
		nibbleMapping[new SystemFontNibble(2)] = "2";
		nibbleMapping[new SystemFontNibble(3)] = "3";
		nibbleMapping[new SystemFontNibble(4)] = "4";
		nibbleMapping[new SystemFontNibble(5)] = "5";
		nibbleMapping[new SystemFontNibble(6)] = "6";
		nibbleMapping[new SystemFontNibble(7)] = "7";
		nibbleMapping[new SystemFontNibble(8)] = "8";
		nibbleMapping[new SystemFontNibble(9)] = "9";
		nibbleMapping[new SystemFontNibble(10)] = ".";
		nibbleMapping[new SystemFontNibble(11)] = "E";
		nibbleMapping[new SystemFontNibble(12)] = "E-";
		nibbleMapping[new SystemFontNibble(13)] = "";
		nibbleMapping[new SystemFontNibble(14)] = "-";
		endOfNumber = new SystemFontNibble(15);
	}

	public SystemFontRealByteEncoding()
		: base(30, 30)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		bool flag = false;
		reader.Read();
		StringBuilder stringBuilder = new StringBuilder();
		do
		{
			SystemFontNibble[] nibbles = SystemFontNibble.GetNibbles(reader.Read());
			foreach (SystemFontNibble systemFontNibble in nibbles)
			{
				if (systemFontNibble == endOfNumber)
				{
					flag = true;
					break;
				}
				stringBuilder.Append(nibbleMapping[systemFontNibble]);
			}
		}
		while (!flag);
		return double.Parse(stringBuilder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat);
	}
}

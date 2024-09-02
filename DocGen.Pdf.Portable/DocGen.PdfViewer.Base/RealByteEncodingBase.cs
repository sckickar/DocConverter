using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DocGen.PdfViewer.Base;

internal class RealByteEncodingBase : ByteEncodingBase
{
	private static Dictionary<Nibble, string> nibbleMapping;

	private static Nibble endOfNumber;

	static RealByteEncodingBase()
	{
		nibbleMapping = new Dictionary<Nibble, string>();
		nibbleMapping[new Nibble(0)] = "0";
		nibbleMapping[new Nibble(1)] = "1";
		nibbleMapping[new Nibble(2)] = "2";
		nibbleMapping[new Nibble(3)] = "3";
		nibbleMapping[new Nibble(4)] = "4";
		nibbleMapping[new Nibble(5)] = "5";
		nibbleMapping[new Nibble(6)] = "6";
		nibbleMapping[new Nibble(7)] = "7";
		nibbleMapping[new Nibble(8)] = "8";
		nibbleMapping[new Nibble(9)] = "9";
		nibbleMapping[new Nibble(10)] = ".";
		nibbleMapping[new Nibble(11)] = "E";
		nibbleMapping[new Nibble(12)] = "E-";
		nibbleMapping[new Nibble(13)] = "";
		nibbleMapping[new Nibble(14)] = "-";
		endOfNumber = new Nibble(15);
	}

	public RealByteEncodingBase()
		: base(30, 30)
	{
	}

	public override object Read(EncodedDataParser reader)
	{
		bool flag = false;
		reader.Read();
		StringBuilder stringBuilder = new StringBuilder();
		do
		{
			Nibble[] nibbles = Nibble.GetNibbles(reader.Read());
			foreach (Nibble nibble in nibbles)
			{
				if (nibble == endOfNumber)
				{
					flag = true;
					break;
				}
				stringBuilder.Append(nibbleMapping[nibble]);
			}
		}
		while (!flag);
		return double.Parse(stringBuilder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat);
	}
}

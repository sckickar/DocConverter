using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DocGen.DocIO.DLS;

internal static class XDocumentExtension
{
	internal static System.Xml.Linq.XNode SelectSingleNode(this System.Xml.Linq.XDocument document, string xPath)
	{
		Regex regex = new Regex("(\\w+)(\\[\\w*\\])");
		string[] array = xPath.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		XElement xElement = document.Root;
		string[] array2 = array;
		foreach (string text in array2)
		{
			Match match = regex.Match(text);
			if (text.Contains("["))
			{
				if (!match.Success)
				{
					continue;
				}
				int num = Convert.ToInt32(match.Groups[2].Value.Trim('[', ']'));
				int num2 = 1;
				foreach (XElement item in xElement.Descendants(match.Groups[1].Value))
				{
					if (num2 == num)
					{
						xElement = item;
						break;
					}
					num2++;
				}
			}
			else
			{
				if (!(xElement.Name != text))
				{
					continue;
				}
				foreach (XElement item2 in xElement.Elements())
				{
					if (item2.Name == text)
					{
						xElement = item2;
						break;
					}
				}
			}
		}
		return xElement;
	}
}

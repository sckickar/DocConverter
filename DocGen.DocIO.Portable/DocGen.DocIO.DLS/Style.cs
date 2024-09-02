using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public abstract class Style : XDLSSerializableBase, IStyle
{
	public class BuiltinStyleLoader
	{
		private const string DEF_DOCIO_RESOURCES = "DocGen.DocIO.Resources";

		private const string DEF_STYLE_TAG = "builtin-styles";

		private const int DEF_LIST_STYLES_NUMBER = 10;

		public static readonly string[] BuiltinStyleNames = new string[106]
		{
			"Normal", "Heading 1", "Heading 2", "Heading 3", "Heading 4", "Heading 5", "Heading 6", "Heading 7", "Heading 8", "Heading 9",
			"Index 1", "Index 2", "Index 3", "Index 4", "Index 5", "Index 6", "Index 7", "Index 8", "Index 9", "TOC 1",
			"TOC 2", "TOC 3", "TOC 4", "TOC 5", "TOC 6", "TOC 7", "TOC 8", "TOC 9", "Normal Indent", "Footnote Text",
			"Comment Text", "Header", "Footer", "Index Heading", "Caption", "Table of Figures", "Footnote Reference", "Comment Reference", "Line Number", "Page Number",
			"Endnote Reference", "Endnote Text", "Table of Authorities", "Macro Text", "TOA Heading", "List", "List Bullet", "List Number", "List 2", "List 3",
			"List 4", "List 5", "List Bullet 2", "List Bullet 3", "List Bullet 4", "List Bullet 5", "List Number 2", "List Number 3", "List Number 4", "List Number 5",
			"Title", "Closing", "Signature", "Default Paragraph Font", "Body Text", "Body Text Indent", "List Continue", "List Continue 2", "List Continue 3", "List Continue 4",
			"List Continue 5", "Message Header", "Subtitle", "Salutation", "Date", "Body Text First Indent", "Body Text First Indent 2", "Note Heading", "Body Text 2", "Body Text 3",
			"Body Text Indent 2", "Body Text Indent 3", "Block Text", "Hyperlink", "FollowedHyperlink", "Strong", "Emphasis", "Document Map", "Plain Text", "E-mail Signature",
			"Normal (Web)", "HTML Acronym", "HTML Address", "HTML Cite", "HTML Code", "HTML Definition", "HTML Keyboard", "HTML Preformatted", "HTML Sample", "HTML Typewriter",
			"HTML Variable", "Comment Subject", "No List", "Balloon Text", "User", "NoStyle"
		};

		internal static readonly string[] BuiltinTableStyleNames = new string[143]
		{
			"Normal Table", "Table Grid", "Light Shading", "Light Shading Accent 1", "Light Shading Accent 2", "Light Shading Accent 3", "Light Shading Accent 4", "Light Shading Accent 5", "Light Shading Accent 6", "Light List",
			"Light List Accent 1", "Light List Accent 2", "Light List Accent 3", "Light List Accent 4", "Light List Accent 5", "Light List Accent 6", "Light Grid", "Light Grid Accent 1", "Light Grid Accent 2", "Light Grid Accent 3",
			"Light Grid Accent 4", "Light Grid Accent 5", "Light Grid Accent 6", "Medium Shading 1", "Medium Shading 1 Accent 1", "Medium Shading 1 Accent 2", "Medium Shading 1 Accent 3", "Medium Shading 1 Accent 4", "Medium Shading 1 Accent 5", "Medium Shading 1 Accent 6",
			"Medium Shading 2", "Medium Shading 2 Accent 1", "Medium Shading 2 Accent 2", "Medium Shading 2 Accent 3", "Medium Shading 2 Accent 4", "Medium Shading 2 Accent 5", "Medium Shading 2 Accent 6", "Medium List 1", "Medium List 1 Accent 1", "Medium List 1 Accent 2",
			"Medium List 1 Accent 3", "Medium List 1 Accent 4", "Medium List 1 Accent 5", "Medium List 1 Accent 6", "Medium List 2", "Medium List 2 Accent 1", "Medium List 2 Accent 2", "Medium List 2 Accent 3", "Medium List 2 Accent 4", "Medium List 2 Accent 5",
			"Medium List 2 Accent 6", "Medium Grid 1", "Medium Grid 1 Accent 1", "Medium Grid 1 Accent 2", "Medium Grid 1 Accent 3", "Medium Grid 1 Accent 4", "Medium Grid 1 Accent 5", "Medium Grid 1 Accent 6", "Medium Grid 2", "Medium Grid 2 Accent 1",
			"Medium Grid 2 Accent 2", "Medium Grid 2 Accent 3", "Medium Grid 2 Accent 4", "Medium Grid 2 Accent 5", "Medium Grid 2 Accent 6", "Medium Grid 3", "Medium Grid 3 Accent 1", "Medium Grid 3 Accent 2", "Medium Grid 3 Accent 3", "Medium Grid 3 Accent 4",
			"Medium Grid 3 Accent5", "Medium Grid 3 Accent 6", "Dark List", "Dark List Accent 1", "Dark List Accent 2", "Dark List Accent 3", "Dark List Accent 4", "Dark List Accent 5", "Dark List Accent 6", "Colorful Shading",
			"Colorful Shading Accent 1", "Colorful Shading Accent 2", "Colorful Shading Accent 3", "Colorful Shading Accent 4", "Colorful Shading Accent 5", "Colorful Shading Accent 6", "Colorful List", "Colorful List Accent 1", "Colorful List Accent 2", "Colorful List Accent 3",
			"Colorful List Accent 4", "Colorful List Accent 5", "Colorful List Accent 6", "Colorful Grid", "Colorful Grid Accent 1", "Colorful Grid Accent 2", "Colorful Grid Accent 3", "Colorful Grid Accent 4", "Colorful Grid Accent 5", "Colorful Grid Accent 6",
			"Table 3D effects 1", "Table 3D effects 2", "Table 3D effects 3", "Table Classic 1", "Table Classic 2", "Table Classic 3", "Table Classic 4", "Table Colorful 1", "Table Colorful 2", "Table Colorful 3",
			"Table Columns 1", "Table Columns 2", "Table Columns 3", "Table Columns 4", "Table Columns 5", "Table Contemporary", "Table Elegant", "Table Grid 1", "Table Grid 2", "Table Grid 3",
			"Table Grid 4", "Table Grid 5", "Table Grid 6", "Table Grid 7", "Table Grid 8", "Table List 1", "Table List 2", "Table List 3", "Table List 4", "Table List 5",
			"Table List 6", "Table List 7", "Table List 8", "Table Professional", "Table Simple 1", "Table Simple 2", "Table Simple 3", "Table Subtle 1", "Table Subtle 2", "Table Theme",
			"Table Web 1", "Table Web 2", "Table Web 3"
		};

		internal static void LoadStyle(IStyle style, BuiltinStyle bStyle)
		{
			Stream stream = UpdateXMLResAndReader();
			stream.Position = 0L;
			XmlReader xmlReader = XmlReader.Create(stream);
			while (xmlReader.Name != "builtin-styles")
			{
				xmlReader.Read();
			}
			xmlReader.Read();
			string text = BuiltInToName(bStyle);
			_ = string.Empty;
			while (!xmlReader.EOF)
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					if (xmlReader.GetAttribute("Name") == text)
					{
						new XDLSReader(xmlReader).ReadChildElement(style);
						break;
					}
					xmlReader.Skip();
				}
				else
				{
					xmlReader.Read();
				}
			}
		}

		internal static void LoadStyle(IStyle style, BuiltinTableStyle bStyle)
		{
			style.Name = BuiltInToName(bStyle);
			switch (bStyle)
			{
			case BuiltinTableStyle.TableNormal:
				LoadStyleTableNormal(style);
				break;
			case BuiltinTableStyle.TableGrid:
				LoadStyleTableGrid(style);
				break;
			case BuiltinTableStyle.LightShading:
				LoadStyleLightShading(style, Color.Black, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 192, 192, 192));
				break;
			case BuiltinTableStyle.LightShadingAccent1:
				LoadStyleLightShading(style, Color.FromArgb(255, 54, 95, 145), Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238));
				break;
			case BuiltinTableStyle.LightShadingAccent2:
				LoadStyleLightShading(style, Color.FromArgb(255, 148, 54, 52), Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210));
				break;
			case BuiltinTableStyle.LightShadingAccent3:
				LoadStyleLightShading(style, Color.FromArgb(255, 118, 146, 60), Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213));
				break;
			case BuiltinTableStyle.LightShadingAccent4:
				LoadStyleLightShading(style, Color.FromArgb(255, 95, 73, 122), Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232));
				break;
			case BuiltinTableStyle.LightShadingAccent5:
				LoadStyleLightShading(style, Color.FromArgb(255, 49, 132, 155), Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241));
				break;
			case BuiltinTableStyle.LightShadingAccent6:
				LoadStyleLightShading(style, Color.FromArgb(255, 227, 108, 10), Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208));
				break;
			case BuiltinTableStyle.LightList:
				LoadStyleLightList(style, Color.FromArgb(255, 0, 0, 0), Color.Black);
				break;
			case BuiltinTableStyle.LightListAccent1:
				LoadStyleLightList(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 79, 129, 189));
				break;
			case BuiltinTableStyle.LightListAccent2:
				LoadStyleLightList(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 192, 80, 77));
				break;
			case BuiltinTableStyle.LightListAccent3:
				LoadStyleLightList(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 155, 187, 89));
				break;
			case BuiltinTableStyle.LightListAccent4:
				LoadStyleLightList(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 128, 100, 162));
				break;
			case BuiltinTableStyle.LightListAccent5:
				LoadStyleLightList(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 75, 172, 198));
				break;
			case BuiltinTableStyle.LightListAccent6:
				LoadStyleLightList(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 247, 150, 70));
				break;
			case BuiltinTableStyle.LightGrid:
				LoadStyleLightGrid(style, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 192, 192, 192));
				break;
			case BuiltinTableStyle.LightGridAccent1:
				LoadStyleLightGrid(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238));
				break;
			case BuiltinTableStyle.LightGridAccent2:
				LoadStyleLightGrid(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210));
				break;
			case BuiltinTableStyle.LightGridAccent3:
				LoadStyleLightGrid(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213));
				break;
			case BuiltinTableStyle.LightGridAccent4:
				LoadStyleLightGrid(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232));
				break;
			case BuiltinTableStyle.LightGridAccent5:
				LoadStyleLightGrid(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241));
				break;
			case BuiltinTableStyle.LightGridAccent6:
				LoadStyleLightGrid(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208));
				break;
			case BuiltinTableStyle.MediumShading1:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 64, 64, 64), Color.Black, Color.FromArgb(255, 192, 192, 192));
				break;
			case BuiltinTableStyle.MediumShading1Accent1:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 123, 160, 205), Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238));
				break;
			case BuiltinTableStyle.MediumShading1Accent2:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 207, 123, 121), Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210));
				break;
			case BuiltinTableStyle.MediumShading1Accent3:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 179, 204, 130), Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213));
				break;
			case BuiltinTableStyle.MediumShading1Accent4:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 159, 138, 185), Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232));
				break;
			case BuiltinTableStyle.MediumShading1Accent5:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 120, 192, 212), Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241));
				break;
			case BuiltinTableStyle.MediumShading1Accent6:
				LoadStyleMediumShading1(style, Color.FromArgb(255, 249, 176, 116), Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208));
				break;
			case BuiltinTableStyle.MediumShading2:
				LoadStyleMediumShading2(style, Color.Black);
				break;
			case BuiltinTableStyle.MediumShading2Accent1:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 79, 129, 189));
				break;
			case BuiltinTableStyle.MediumShading2Accent2:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 192, 80, 77));
				break;
			case BuiltinTableStyle.MediumShading2Accent3:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 155, 187, 89));
				break;
			case BuiltinTableStyle.MediumShading2Accent4:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 128, 100, 162));
				break;
			case BuiltinTableStyle.MediumShading2Accent5:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 75, 172, 198));
				break;
			case BuiltinTableStyle.MediumShading2Accent6:
				LoadStyleMediumShading2(style, Color.FromArgb(255, 247, 150, 70));
				break;
			case BuiltinTableStyle.MediumList1:
				LoadStyleMediumList1(style, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 192, 192, 192));
				break;
			case BuiltinTableStyle.MediumList1Accent1:
				LoadStyleMediumList1(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238));
				break;
			case BuiltinTableStyle.MediumList1Accent2:
				LoadStyleMediumList1(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210));
				break;
			case BuiltinTableStyle.MediumList1Accent3:
				LoadStyleMediumList1(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213));
				break;
			case BuiltinTableStyle.MediumList1Accent4:
				LoadStyleMediumList1(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232));
				break;
			case BuiltinTableStyle.MediumList1Accent5:
				LoadStyleMediumList1(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241));
				break;
			case BuiltinTableStyle.MediumList1Accent6:
				LoadStyleMediumList1(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208));
				break;
			case BuiltinTableStyle.MediumList2:
				LoadStyleMediumList2(style, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 192, 192, 192));
				break;
			case BuiltinTableStyle.MediumList2Accent1:
				LoadStyleMediumList2(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238));
				break;
			case BuiltinTableStyle.MediumList2Accent2:
				LoadStyleMediumList2(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210));
				break;
			case BuiltinTableStyle.MediumList2Accent3:
				LoadStyleMediumList2(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213));
				break;
			case BuiltinTableStyle.MediumList2Accent4:
				LoadStyleMediumList2(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232));
				break;
			case BuiltinTableStyle.MediumList2Accent5:
				LoadStyleMediumList2(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241));
				break;
			case BuiltinTableStyle.MediumList2Accent6:
				LoadStyleMediumList2(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208));
				break;
			case BuiltinTableStyle.MediumGrid1:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 64, 64, 64), Color.FromArgb(255, 192, 192, 192), Color.FromArgb(255, 128, 128, 128));
				break;
			case BuiltinTableStyle.MediumGrid1Accent1:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 123, 160, 205), Color.FromArgb(255, 211, 223, 238), Color.FromArgb(255, 167, 191, 222));
				break;
			case BuiltinTableStyle.MediumGrid1Accent2:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 207, 123, 121), Color.FromArgb(255, 239, 211, 210), Color.FromArgb(255, 223, 167, 166));
				break;
			case BuiltinTableStyle.MediumGrid1Accent3:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 179, 204, 130), Color.FromArgb(255, 230, 238, 213), Color.FromArgb(255, 205, 221, 172));
				break;
			case BuiltinTableStyle.MediumGrid1Accent4:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 159, 138, 185), Color.FromArgb(255, 223, 216, 232), Color.FromArgb(255, 191, 177, 208));
				break;
			case BuiltinTableStyle.MediumGrid1Accent5:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 120, 192, 212), Color.FromArgb(255, 210, 234, 241), Color.FromArgb(255, 165, 213, 226));
				break;
			case BuiltinTableStyle.MediumGrid1Accent6:
				LoadStyleMediumGrid1(style, Color.FromArgb(255, 249, 176, 116), Color.FromArgb(255, 253, 228, 208), Color.FromArgb(255, 251, 202, 162));
				break;
			case BuiltinTableStyle.MediumGrid2:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 192, 192, 192), Color.FromArgb(255, 230, 230, 230), Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 128, 128, 128));
				break;
			case BuiltinTableStyle.MediumGrid2Accent1:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 211, 223, 238), Color.FromArgb(255, 237, 242, 248), Color.FromArgb(255, 219, 229, 241), Color.FromArgb(255, 167, 191, 222));
				break;
			case BuiltinTableStyle.MediumGrid2Accent2:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 239, 211, 210), Color.FromArgb(255, 248, 237, 237), Color.FromArgb(255, 242, 219, 219), Color.FromArgb(255, 223, 167, 166));
				break;
			case BuiltinTableStyle.MediumGrid2Accent3:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 230, 238, 213), Color.FromArgb(255, 245, 248, 238), Color.FromArgb(255, 234, 241, 221), Color.FromArgb(255, 205, 221, 172));
				break;
			case BuiltinTableStyle.MediumGrid2Accent4:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 223, 216, 232), Color.FromArgb(255, 242, 239, 246), Color.FromArgb(255, 229, 223, 236), Color.FromArgb(255, 191, 177, 208));
				break;
			case BuiltinTableStyle.MediumGrid2Accent5:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 210, 234, 241), Color.FromArgb(255, 237, 246, 249), Color.FromArgb(255, 218, 238, 243), Color.FromArgb(255, 165, 213, 226));
				break;
			case BuiltinTableStyle.MediumGrid2Accent6:
				LoadStyleMediumGrid2(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 253, 228, 208), Color.FromArgb(255, 254, 244, 236), Color.FromArgb(255, 253, 233, 217), Color.FromArgb(255, 251, 202, 162));
				break;
			case BuiltinTableStyle.MediumGrid3:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 192, 192, 192), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 128, 128, 128));
				break;
			case BuiltinTableStyle.MediumGrid3Accent1:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 211, 223, 238), Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 167, 191, 222));
				break;
			case BuiltinTableStyle.MediumGrid3Accent2:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 239, 211, 210), Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 223, 167, 166));
				break;
			case BuiltinTableStyle.MediumGrid3Accent3:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 230, 238, 213), Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 205, 221, 172));
				break;
			case BuiltinTableStyle.MediumGrid3Accent4:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 223, 216, 232), Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 191, 177, 208));
				break;
			case BuiltinTableStyle.MediumGrid3Accent5:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 210, 234, 241), Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 165, 213, 226));
				break;
			case BuiltinTableStyle.MediumGrid3Accent6:
				LoadStyleMediumGrid3(style, Color.FromArgb(255, 253, 228, 208), Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 251, 202, 162));
				break;
			case BuiltinTableStyle.DarkList:
				LoadStyleDarkList(style, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 0, 0));
				break;
			case BuiltinTableStyle.DarkListAccent1:
				LoadStyleDarkList(style, Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 36, 63, 96), Color.FromArgb(255, 54, 95, 145));
				break;
			case BuiltinTableStyle.DarkListAccent2:
				LoadStyleDarkList(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 98, 36, 35), Color.FromArgb(255, 148, 54, 52));
				break;
			case BuiltinTableStyle.DarkListAccent3:
				LoadStyleDarkList(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 78, 97, 40), Color.FromArgb(255, 118, 146, 60));
				break;
			case BuiltinTableStyle.DarkListAccent4:
				LoadStyleDarkList(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 63, 49, 81), Color.FromArgb(255, 95, 73, 122));
				break;
			case BuiltinTableStyle.DarkListAccent5:
				LoadStyleDarkList(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 32, 88, 103), Color.FromArgb(255, 49, 132, 155));
				break;
			case BuiltinTableStyle.DarkListAccent6:
				LoadStyleDarkList(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 151, 71, 6), Color.FromArgb(255, 227, 108, 10));
				break;
			case BuiltinTableStyle.ColorfulShading:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 230, 230, 230), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 128, 128, 128));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent1:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 79, 129, 189), Color.FromArgb(255, 237, 242, 248), Color.FromArgb(255, 44, 76, 116), Color.FromArgb(255, 184, 204, 228), Color.FromArgb(255, 167, 191, 222));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent2:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 192, 80, 77), Color.FromArgb(255, 248, 237, 237), Color.FromArgb(255, 119, 44, 42), Color.FromArgb(255, 229, 184, 183), Color.FromArgb(255, 223, 167, 166));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent3:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 245, 248, 238), Color.FromArgb(255, 94, 117, 48), Color.FromArgb(255, 214, 227, 188), Color.FromArgb(255, 205, 221, 172));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent4:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 155, 187, 89), Color.FromArgb(255, 128, 100, 162), Color.FromArgb(255, 242, 239, 246), Color.FromArgb(255, 76, 59, 98), Color.FromArgb(255, 204, 192, 217), Color.FromArgb(255, 191, 177, 208));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent5:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 237, 246, 249), Color.FromArgb(255, 39, 106, 124), Color.FromArgb(255, 182, 221, 232), Color.FromArgb(255, 165, 213, 226));
				break;
			case BuiltinTableStyle.ColorfulShadingAccent6:
				LoadStyleColorfulShading(style, Color.FromArgb(255, 75, 172, 198), Color.FromArgb(255, 247, 150, 70), Color.FromArgb(255, 254, 244, 236), Color.FromArgb(255, 182, 86, 8), Color.FromArgb(255, 251, 212, 180), Color.FromArgb(255, 251, 202, 162));
				break;
			case BuiltinTableStyle.ColorfulList:
				LoadStyleColorfulList(style, Color.FromArgb(255, 230, 230, 230), Color.FromArgb(255, 158, 58, 56), Color.FromArgb(255, 192, 192, 192), Color.FromArgb(255, 204, 204, 204));
				break;
			case BuiltinTableStyle.ColorfulListAccent1:
				LoadStyleColorfulList(style, Color.FromArgb(255, 237, 242, 248), Color.FromArgb(255, 158, 58, 56), Color.FromArgb(255, 211, 223, 238), Color.FromArgb(255, 219, 229, 241));
				break;
			case BuiltinTableStyle.ColorfulListAccent2:
				LoadStyleColorfulList(style, Color.FromArgb(255, 248, 237, 237), Color.FromArgb(255, 158, 58, 56), Color.FromArgb(255, 239, 211, 210), Color.FromArgb(255, 242, 219, 219));
				break;
			case BuiltinTableStyle.ColorfulListAccent3:
				LoadStyleColorfulList(style, Color.FromArgb(255, 245, 248, 238), Color.FromArgb(255, 102, 78, 130), Color.FromArgb(255, 230, 238, 213), Color.FromArgb(255, 234, 241, 221));
				break;
			case BuiltinTableStyle.ColorfulListAccent4:
				LoadStyleColorfulList(style, Color.FromArgb(255, 242, 239, 246), Color.FromArgb(255, 126, 156, 64), Color.FromArgb(255, 223, 216, 232), Color.FromArgb(255, 229, 223, 236));
				break;
			case BuiltinTableStyle.ColorfulListAccent5:
				LoadStyleColorfulList(style, Color.FromArgb(255, 237, 246, 249), Color.FromArgb(255, 242, 115, 10), Color.FromArgb(255, 210, 234, 241), Color.FromArgb(255, 218, 238, 243));
				break;
			case BuiltinTableStyle.ColorfulListAccent6:
				LoadStyleColorfulList(style, Color.FromArgb(255, 254, 244, 236), Color.FromArgb(255, 52, 141, 165), Color.FromArgb(255, 253, 228, 208), Color.FromArgb(255, 253, 233, 217));
				break;
			case BuiltinTableStyle.ColorfulGrid:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 204, 204, 204), Color.FromArgb(255, 153, 153, 153), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 128, 128, 128));
				break;
			case BuiltinTableStyle.ColorfulGridAccent1:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 219, 229, 241), Color.FromArgb(255, 184, 204, 228), Color.FromArgb(255, 54, 95, 145), Color.FromArgb(255, 167, 191, 222));
				break;
			case BuiltinTableStyle.ColorfulGridAccent2:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 242, 219, 219), Color.FromArgb(255, 229, 184, 183), Color.FromArgb(255, 148, 54, 52), Color.FromArgb(255, 223, 167, 166));
				break;
			case BuiltinTableStyle.ColorfulGridAccent3:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 234, 241, 221), Color.FromArgb(255, 214, 227, 188), Color.FromArgb(255, 118, 146, 60), Color.FromArgb(255, 205, 221, 172));
				break;
			case BuiltinTableStyle.ColorfulGridAccent4:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 229, 223, 236), Color.FromArgb(255, 204, 192, 217), Color.FromArgb(255, 95, 73, 122), Color.FromArgb(255, 191, 177, 208));
				break;
			case BuiltinTableStyle.ColorfulGridAccent5:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 218, 238, 243), Color.FromArgb(255, 182, 221, 232), Color.FromArgb(255, 49, 132, 155), Color.FromArgb(255, 165, 213, 226));
				break;
			case BuiltinTableStyle.ColorfulGridAccent6:
				LoadStyleColorfulGrid(style, Color.FromArgb(255, 253, 233, 217), Color.FromArgb(255, 251, 212, 180), Color.FromArgb(255, 227, 108, 10), Color.FromArgb(255, 251, 202, 162));
				break;
			case BuiltinTableStyle.Table3Deffects1:
				LoadStyleTable3Deffects1(style);
				break;
			case BuiltinTableStyle.Table3Deffects2:
				LoadStyleTable3Deffects2(style);
				break;
			case BuiltinTableStyle.Table3Deffects3:
				LoadStyleTable3Deffects3(style);
				break;
			case BuiltinTableStyle.TableClassic1:
				LoadStyleTableClassic1(style);
				break;
			case BuiltinTableStyle.TableClassic2:
				LoadStyleTableClassic2(style);
				break;
			case BuiltinTableStyle.TableClassic3:
				LoadStyleTableClassic3(style);
				break;
			case BuiltinTableStyle.TableClassic4:
				LoadStyleTableClassic4(style);
				break;
			case BuiltinTableStyle.TableColorful1:
				LoadStyleTableColorful1(style);
				break;
			case BuiltinTableStyle.TableColorful2:
				LoadStyleTableColorful2(style);
				break;
			case BuiltinTableStyle.TableColorful3:
				LoadStyleTableColorful3(style);
				break;
			case BuiltinTableStyle.TableColumns1:
				LoadStyleTableColumns1(style);
				break;
			case BuiltinTableStyle.TableColumns2:
				LoadStyleTableColumns2(style);
				break;
			case BuiltinTableStyle.TableColumns3:
				LoadStyleTableColumns3(style);
				break;
			case BuiltinTableStyle.TableColumns4:
				LoadStyleTableColumns4(style);
				break;
			case BuiltinTableStyle.TableColumns5:
				LoadStyleTableColumns5(style);
				break;
			case BuiltinTableStyle.TableContemporary:
				LoadStyleTableContemporary(style);
				break;
			case BuiltinTableStyle.TableElegant:
				LoadStyleTableElegant(style);
				break;
			case BuiltinTableStyle.TableGrid1:
				LoadStyleTableGrid1(style);
				break;
			case BuiltinTableStyle.TableGrid2:
				LoadStyleTableGrid2(style);
				break;
			case BuiltinTableStyle.TableGrid3:
				LoadStyleTableGrid3(style);
				break;
			case BuiltinTableStyle.TableGrid4:
				LoadStyleTableGrid4(style);
				break;
			case BuiltinTableStyle.TableGrid5:
				LoadStyleTableGrid5(style);
				break;
			case BuiltinTableStyle.TableGrid6:
				LoadStyleTableGrid6(style);
				break;
			case BuiltinTableStyle.TableGrid7:
				LoadStyleTableGrid7(style);
				break;
			case BuiltinTableStyle.TableGrid8:
				LoadStyleTableGrid8(style);
				break;
			case BuiltinTableStyle.TableList1:
				LoadStyleTableList1(style);
				break;
			case BuiltinTableStyle.TableList2:
				LoadStyleTableList2(style);
				break;
			case BuiltinTableStyle.TableList3:
				LoadStyleTableList3(style);
				break;
			case BuiltinTableStyle.TableList4:
				LoadStyleTableList4(style);
				break;
			case BuiltinTableStyle.TableList5:
				LoadStyleTableList5(style);
				break;
			case BuiltinTableStyle.TableList6:
				LoadStyleTableList6(style);
				break;
			case BuiltinTableStyle.TableList7:
				LoadStyleTableList7(style);
				break;
			case BuiltinTableStyle.TableList8:
				LoadStyleTableList8(style);
				break;
			case BuiltinTableStyle.TableProfessional:
				LoadStyleTableProfessional(style);
				break;
			case BuiltinTableStyle.TableSimple1:
				LoadStyleTableSimple1(style);
				break;
			case BuiltinTableStyle.TableSimple2:
				LoadStyleTableSimple2(style);
				break;
			case BuiltinTableStyle.TableSimple3:
				LoadStyleTableSimple3(style);
				break;
			case BuiltinTableStyle.TableSubtle1:
				LoadStyleTableSubtle1(style);
				break;
			case BuiltinTableStyle.TableSubtle2:
				LoadStyleTableSubtle2(style);
				break;
			case BuiltinTableStyle.TableTheme:
				LoadStyleTableTheme(style);
				break;
			case BuiltinTableStyle.TableWeb1:
				LoadStyleTableWeb1(style);
				break;
			case BuiltinTableStyle.TableWeb2:
				LoadStyleTableWeb2(style);
				break;
			case BuiltinTableStyle.TableWeb3:
				LoadStyleTableWeb3(style);
				break;
			}
		}

		private static void LoadStyleTableNormal(IStyle style)
		{
			(style as Style).IsSemiHidden = true;
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
		}

		private static void LoadStyleTableGrid(IStyle style)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
		}

		private static void LoadStyleLightShading(IStyle style, Color textColor, Color borderColor, Color backColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = textColor;
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = backColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.BackColor = backColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleLightList(IStyle style, Color borderColor, Color backColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.BackColor = backColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Double;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Space = 0f;
		}

		private static void LoadStyleLightGrid(IStyle style, Color borderColor, Color backColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Double;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle5.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle5.CellProperties.BackColor = backColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Space = 0f;
			conditionalFormattingStyle6.CellProperties.BackColor = backColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle7.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle7.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle7.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle7.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle7.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle7.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle7.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.Color = borderColor;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.Space = 0f;
		}

		private static void LoadStyleMediumShading1(IStyle style, Color borderColor, Color firstRowBackColor, Color backColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.BackColor = firstRowBackColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Double;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = backColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.BackColor = backColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle7.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
		}

		private static void LoadStyleMediumShading2(IStyle style, Color backColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 2.25f;
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.BackColor = backColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(8, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(9, 0f);
			conditionalFormattingStyle2.ParagraphFormat.SetPropertyValue(52, 12f);
			conditionalFormattingStyle2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Double;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = backColor;
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = backColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 216, 216, 216);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 216, 216, 216);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle7.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Top.LineWidth = 2.25f;
			conditionalFormattingStyle7.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle7.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle7.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle7.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle7.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			ConditionalFormattingStyle conditionalFormattingStyle8 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle8.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle8.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle8.CellProperties.Borders.Top.LineWidth = 2.25f;
			conditionalFormattingStyle8.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle8.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle8.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle8.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle8.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle8.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
		}

		private static void LoadStyleMediumList1(IStyle style, Color borderColor, Color backColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.Black;
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 31, 73, 125);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = backColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = backColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleMediumList2(IStyle style, Color borderColor, Color backColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.Black;
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.SetPropertyValue(3, 12f);
			conditionalFormattingStyle.CharacterFormat.SetPropertyValue(62, 12f);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 3f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = borderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = borderColor;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = borderColor;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = backColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.BackColor = backColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle7.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle7.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle7.CellProperties.TextureStyle = TextureStyle.TextureNone;
			(style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell).CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
		}

		private static void LoadStyleMediumGrid1(IStyle style, Color borderColor, Color backColor, Color bandCellColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 2.25f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = borderColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleMediumGrid2(IStyle style, Color borderColor, Color backColor, Color firstRowColor, Color lastColumnColor, Color bandCellColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.Black;
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Black;
			conditionalFormattingStyle.CellProperties.BackColor = firstRowColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = false;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = lastColumnColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.LineWidth = 0.75f;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.LineWidth = 0.75f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Color = borderColor;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Space = 0f;
			conditionalFormattingStyle6.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle7.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle7.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle7.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleMediumGrid3(IStyle style, Color backColor, Color firstRowColor, Color bandCellColor)
		{
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = false;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 3f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Vertical.Space = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = firstRowColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.Italic = false;
			conditionalFormattingStyle2.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 3f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.Space = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = firstRowColor;
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.Italic = false;
			conditionalFormattingStyle3.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle3.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 3f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = firstRowColor;
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CharacterFormat.Italic = false;
			conditionalFormattingStyle4.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 3f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = firstRowColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle5.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Top.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Left.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Right.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.LineWidth = 1f;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.Space = 0f;
			conditionalFormattingStyle6.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleDarkList(IStyle style, Color backColor, Color lastRowColor, Color bandCellColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 2.25f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 2.25f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle2.CellProperties.BackColor = lastRowColor;
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 2.25f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 2.25f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle6.CellProperties.BackColor = bandCellColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleColorfulShading(IStyle style, Color topBorderColor, Color borderColor, Color backColor, Color lastRowColor, Color bandColumnColor, Color bandRowColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 3f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = topBorderColor;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = borderColor;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 3f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = topBorderColor;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = lastRowColor;
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.LineWidth = 0.5f;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.Color = lastRowColor;
			conditionalFormattingStyle3.CellProperties.Borders.Horizontal.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle3.CellProperties.BackColor = lastRowColor;
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle4.CellProperties.BackColor = lastRowColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = bandColumnColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = bandRowColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
			if (style.Name != "Colorful Shading Accent 3")
			{
				(style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
				(style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			}
		}

		private static void LoadStyleColorfulList(IStyle style, Color backColor, Color rowColor, Color bandColumnColor, Color bandRowColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = rowColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = rowColor;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Top.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Right.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Horizontal.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.Borders.Vertical.BorderType = BorderStyle.Cleared;
			conditionalFormattingStyle5.CellProperties.BackColor = bandColumnColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = bandRowColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleColorfulGrid(IStyle style, Color backColor, Color rowColor, Color columnColor, Color bandColor)
		{
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(9, 0f);
			(style as WTableStyle).ParagraphFormat.SetPropertyValue(52, 12f);
			(style as WTableStyle).ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = backColor;
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.BackColor = rowColor;
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.BackColor = rowColor;
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.BackColor = columnColor;
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.BackColor = columnColor;
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CellProperties.BackColor = bandColor;
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.BackColor = bandColor;
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.TextureNone;
		}

		private static void LoadStyleTable3Deffects1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 128, 0, 128);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Right.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowLastCell);
			conditionalFormattingStyle7.CellProperties.Borders.Top.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.Top.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Top.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Left.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.Left.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.Left.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle8 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle8.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle8.CellProperties.Borders.Top.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.Top.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.Top.LineWidth = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.Right.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.Right.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.Right.LineWidth = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTable3Deffects2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle4.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Top.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle4.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTable3Deffects3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle4.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture50Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle6.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle7.CharacterFormat.Bold = true;
			conditionalFormattingStyle7.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableClassic1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CharacterFormat.Italic = false;
			conditionalFormattingStyle4.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableClassic2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 128, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 128, 0, 128);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle6.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableClassic3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableClassic4(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.Texture50Percent;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.Texture50Percent;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColorful1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.Italic = true;
			conditionalFormattingStyle2.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CharacterFormat.Italic = false;
			conditionalFormattingStyle4.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColorful2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.Texture20Percent;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 128, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.Italic = true;
			conditionalFormattingStyle2.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CharacterFormat.Italic = false;
			conditionalFormattingStyle4.CharacterFormat.ItalicBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColorful3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 192, 192, 192);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Left.LineWidth = 4.5f;
			conditionalFormattingStyle2.CellProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureSolid;
		}

		private static void LoadStyleTableColumns1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.Bold = true;
			(style as WTableStyle).CharacterFormat.BoldBidi = true;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = false;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Double;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = false;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = false;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = false;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding);
			conditionalFormattingStyle6.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle7.CharacterFormat.Bold = true;
			conditionalFormattingStyle7.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle8 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle8.CharacterFormat.Bold = true;
			conditionalFormattingStyle8.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColumns2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.Bold = true;
			(style as WTableStyle).CharacterFormat.BoldBidi = true;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = false;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = false;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = false;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture30Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding);
			conditionalFormattingStyle6.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(255, 0, 255, 0);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle7.CharacterFormat.Bold = true;
			conditionalFormattingStyle7.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle8 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle8.CharacterFormat.Bold = true;
			conditionalFormattingStyle8.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle8.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColumns3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.Bold = true;
			(style as WTableStyle).CharacterFormat.BoldBidi = true;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = false;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = false;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = false;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding);
			conditionalFormattingStyle6.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.Texture10Percent;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle7.CharacterFormat.Bold = true;
			conditionalFormattingStyle7.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableColumns4(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle4.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.Texture50Percent;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture10Percent;
		}

		private static void LoadStyleTableColumns5(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.ColumnStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 128, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 128, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 128, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 128, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 192, 192, 192);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddColumnBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			(style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenColumnBanding).CharacterFormat.TextColor = Color.Empty;
		}

		private static void LoadStyleTableContemporary(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 2.25f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.Texture20Percent;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.Texture5Percent;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.Texture20Percent;
		}

		private static void LoadStyleTableElegant(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Double;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Double;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Double;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Double;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.AllCaps = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle2.CharacterFormat.Italic = true;
			conditionalFormattingStyle2.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.Texture30Percent;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid4(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.Texture30Percent;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.Texture30Percent;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid5(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid6(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid7(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).CharacterFormat.Bold = true;
			(style as WTableStyle).CharacterFormat.BoldBidi = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = false;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = false;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = false;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = false;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = false;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowFirstCell);
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0.75f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableGrid8(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 128);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableList1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 128, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableList2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 2L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 128, 128, 128);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.Texture75Percent;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 255, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.Texture20Percent;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle4.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableList3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle3.CharacterFormat.Italic = true;
			conditionalFormattingStyle3.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle3.CharacterFormat.TextColor = Color.FromArgb(255, 0, 0, 128);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableList4(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 128, 128, 128);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
		}

		private static void LoadStyleTableList5(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableList6(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.Texture50Percent;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Right.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
		}

		private static void LoadStyleTableList7(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 192, 192, 192);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture20Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
		}

		private static void LoadStyleTableList8(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.Italic = true;
			conditionalFormattingStyle.CharacterFormat.ItalicBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle5.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 255, 255, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.EvenRowBanding);
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle6.CellProperties.ForeColor = Color.FromArgb(255, 255, 0, 0);
			conditionalFormattingStyle6.CellProperties.TextureStyle = TextureStyle.Texture50Percent;
		}

		private static void LoadStyleTableProfessional(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
		}

		private static void LoadStyleTableSimple1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableSimple2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CharacterFormat.Bold = true;
			conditionalFormattingStyle2.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle2.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CharacterFormat.Bold = true;
			conditionalFormattingStyle3.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 1.5f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CharacterFormat.Bold = true;
			conditionalFormattingStyle4.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 0.75f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.Left.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Left.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle6.CharacterFormat.Bold = true;
			conditionalFormattingStyle6.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle6.CellProperties.Borders.Top.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.Top.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableSimple3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 1.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.Bold = true;
			conditionalFormattingStyle.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle.CellProperties.ForeColor = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.TextureStyle = TextureStyle.TextureSolid;
		}

		private static void LoadStyleTableSubtle1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.RowStripe = 1L;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Top.LineWidth = 0.75f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle2.CellProperties.ForeColor = Color.FromArgb(255, 128, 0, 128);
			conditionalFormattingStyle2.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 1.5f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 1.5f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.OddRowBanding);
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.Bottom.LineWidth = 0.75f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle5.CellProperties.ForeColor = Color.FromArgb(255, 128, 128, 0);
			conditionalFormattingStyle5.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle6.CharacterFormat.Bold = true;
			conditionalFormattingStyle6.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle7 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle7.CharacterFormat.Bold = true;
			conditionalFormattingStyle7.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle7.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableSubtle2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle.CellProperties.Borders.Bottom.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.Bottom.LineWidth = 1.5f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle2 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle2.CellProperties.Borders.Top.BorderType = BorderStyle.Single;
			conditionalFormattingStyle2.CellProperties.Borders.Top.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle2.CellProperties.Borders.Top.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.Top.LineWidth = 1.5f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle2.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle3 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstColumn);
			conditionalFormattingStyle3.CellProperties.Borders.Right.BorderType = BorderStyle.Single;
			conditionalFormattingStyle3.CellProperties.Borders.Right.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle3.CellProperties.Borders.Right.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.Right.LineWidth = 1.5f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle3.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle3.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle3.CellProperties.ForeColor = Color.FromArgb(255, 0, 128, 0);
			conditionalFormattingStyle3.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle4 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastColumn);
			conditionalFormattingStyle4.CellProperties.Borders.Left.BorderType = BorderStyle.Single;
			conditionalFormattingStyle4.CellProperties.Borders.Left.Color = Color.FromArgb(255, 0, 0, 0);
			conditionalFormattingStyle4.CellProperties.Borders.Left.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.Left.LineWidth = 1.5f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle4.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			conditionalFormattingStyle4.CellProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
			conditionalFormattingStyle4.CellProperties.ForeColor = Color.FromArgb(255, 128, 128, 0);
			conditionalFormattingStyle4.CellProperties.TextureStyle = TextureStyle.Texture25Percent;
			ConditionalFormattingStyle conditionalFormattingStyle5 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRowLastCell);
			conditionalFormattingStyle5.CharacterFormat.Bold = true;
			conditionalFormattingStyle5.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle5.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
			ConditionalFormattingStyle conditionalFormattingStyle6 = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.LastRowFirstCell);
			conditionalFormattingStyle6.CharacterFormat.Bold = true;
			conditionalFormattingStyle6.CharacterFormat.BoldBidi = true;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle6.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableTheme(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Single;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.5f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
		}

		private static void LoadStyleTableWeb1(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.CellSpacing = 1f;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			(style as WTableStyle).RowProperties.CellSpacing = 1f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableWeb2(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.CellSpacing = 1f;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Inset;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			(style as WTableStyle).RowProperties.CellSpacing = 1f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static void LoadStyleTableWeb3(IStyle style)
		{
			(style as Style).UnhideWhenUsed = true;
			(style as WTableStyle).TableProperties.CellSpacing = 1f;
			(style as WTableStyle).TableProperties.LeftIndent = 0f;
			(style as WTableStyle).TableProperties.Paddings.Top = 0f;
			(style as WTableStyle).TableProperties.Paddings.Bottom = 0f;
			(style as WTableStyle).TableProperties.Paddings.Left = 5.4f;
			(style as WTableStyle).TableProperties.Paddings.Right = 5.4f;
			(style as WTableStyle).TableProperties.Borders.Top.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Top.LineWidth = 3f;
			(style as WTableStyle).TableProperties.Borders.Top.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Top.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Bottom.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Bottom.LineWidth = 3f;
			(style as WTableStyle).TableProperties.Borders.Bottom.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Bottom.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Left.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Left.LineWidth = 3f;
			(style as WTableStyle).TableProperties.Borders.Left.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Left.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Right.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Right.LineWidth = 3f;
			(style as WTableStyle).TableProperties.Borders.Right.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Right.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Horizontal.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Horizontal.Space = 0f;
			(style as WTableStyle).TableProperties.Borders.Vertical.BorderType = BorderStyle.Outset;
			(style as WTableStyle).TableProperties.Borders.Vertical.LineWidth = 0.75f;
			(style as WTableStyle).TableProperties.Borders.Vertical.Color = Color.Black;
			(style as WTableStyle).TableProperties.Borders.Vertical.Space = 0f;
			(style as WTableStyle).CellProperties.BackColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.ForeColor = Color.FromArgb(0, 255, 255, 255);
			(style as WTableStyle).CellProperties.TextureStyle = TextureStyle.TextureNone;
			(style as WTableStyle).RowProperties.CellSpacing = 1f;
			ConditionalFormattingStyle conditionalFormattingStyle = (style as WTableStyle).ConditionalFormat(ConditionalFormattingType.FirstRow);
			conditionalFormattingStyle.CharacterFormat.TextColor = Color.Empty;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalDown.LineWidth = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.BorderType = BorderStyle.None;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Color = Color.Black;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.Space = 0f;
			conditionalFormattingStyle.CellProperties.Borders.DiagonalUp.LineWidth = 0f;
		}

		private static Stream UpdateXMLResAndReader()
		{
			return GetManifestResourceStream("builtin-styles.xml") ?? throw new Exception("Resource file builtin-styles.xml not found.");
		}

		private static Stream GetManifestResourceStream(string fileName)
		{
			Assembly assembly = typeof(BuiltinStyleLoader).GetTypeInfo().Assembly;
			string[] manifestResourceNames = assembly.GetManifestResourceNames();
			foreach (string text in manifestResourceNames)
			{
				if (text.EndsWith("." + fileName))
				{
					fileName = text;
					break;
				}
			}
			return assembly.GetManifestResourceStream(fileName);
		}

		internal static bool IsListStyle(BuiltinStyle bstyle)
		{
			bool result = false;
			for (int i = 0; i < 10; i++)
			{
				string text = bstyle.ToString();
				BuiltinListStyle builtinListStyle = (BuiltinListStyle)i;
				if (text == builtinListStyle.ToString())
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}

	protected const int DEF_USER_STYLE_ID = 4094;

	private int m_styleId = 4094;

	private string m_strName;

	protected IStyle m_baseStyle;

	protected WCharacterFormat m_chFormat;

	protected string m_nextStyle;

	protected string m_linkedStyleName;

	private string m_styleIDName;

	protected WordStyleType m_typeCode;

	protected byte[] m_tapx;

	private byte m_bFlags;

	private int uiPriority = int.MinValue;

	private List<Entity> m_rangeCollection;

	internal bool IsRemoving;

	internal byte[] TableStyleData
	{
		get
		{
			return m_tapx;
		}
		set
		{
			m_tapx = value;
		}
	}

	internal WordStyleType TypeCode
	{
		get
		{
			return m_typeCode;
		}
		set
		{
			if (StyleType == StyleType.ParagraphStyle && value != WordStyleType.ParagraphStyle)
			{
				RemoveBaseStyle();
			}
			m_typeCode = value;
		}
	}

	public WCharacterFormat CharacterFormat => m_chFormat;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			if (!base.Document.IsOpening && (value == null || value.Length == 0))
			{
				throw new ArgumentNullException("Name");
			}
			if (StyleType == StyleType.ParagraphStyle && value == "Normal" && !base.Document.IsNormalStyleDefined)
			{
				(base.Document.Styles as StyleCollection).InnerList.Remove(m_baseStyle);
				RemoveBaseStyle();
				base.Document.IsNormalStyleDefined = true;
			}
			else if (StyleType == StyleType.CharacterStyle && (value == "DefaultParagraphFont" || value == "Default Paragraph Font") && !base.Document.IsDefaultParagraphFontStyleDefined)
			{
				(base.Document.Styles as StyleCollection).InnerList.Remove(m_baseStyle);
				RemoveBaseStyle();
				base.Document.IsDefaultParagraphFontStyleDefined = true;
			}
			if (!base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && base.Document != null && base.Document.Styles.FindByName(value, StyleType) != null)
			{
				throw new ArgumentException("Name of style already exists");
			}
			string key = value.Replace(" ", string.Empty).ToLower();
			Dictionary<string, int> builtinStyleIds = GetBuiltinStyleIds();
			if (builtinStyleIds.ContainsKey(key))
			{
				StyleId = builtinStyleIds[key];
			}
			else
			{
				StyleId = 4094;
			}
			m_strName = value;
		}
	}

	internal IStyle BaseStyle => m_baseStyle;

	internal int StyleId
	{
		get
		{
			return m_styleId;
		}
		set
		{
			m_styleId = value;
		}
	}

	public abstract StyleType StyleType { get; }

	public BuiltinStyle BuiltInStyleIdentifier => NameToBuiltIn(Name);

	internal string NextStyle
	{
		get
		{
			if (m_nextStyle != null)
			{
				if (base.Document.StyleNameIds.ContainsKey(m_nextStyle))
				{
					return base.Document.StyleNameIds[m_nextStyle];
				}
				if (base.Document.Styles.FindByName(m_nextStyle) != null)
				{
					return m_nextStyle;
				}
				return Name;
			}
			return Name;
		}
		set
		{
			m_nextStyle = value;
		}
	}

	internal string LinkStyle
	{
		get
		{
			return m_linkedStyleName;
		}
		set
		{
			m_linkedStyleName = value;
		}
	}

	public string LinkedStyleName
	{
		get
		{
			return m_linkedStyleName;
		}
		set
		{
			if (!base.Document.IsOpening && !base.Document.IsCloning && !base.Document.IsMailMerge && !base.Document.IsClosing)
			{
				SetLinkedStyle(value);
			}
			else
			{
				m_linkedStyleName = value;
			}
		}
	}

	internal string StyleIDName
	{
		get
		{
			return m_styleIDName;
		}
		set
		{
			m_styleIDName = value;
		}
	}

	public bool IsPrimaryStyle
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsSemiHidden
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool UnhideWhenUsed
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsCustom
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal int UIPriority
	{
		get
		{
			return uiPriority;
		}
		set
		{
			uiPriority = value;
		}
	}

	internal List<Entity> RangeCollection
	{
		get
		{
			if (m_rangeCollection == null)
			{
				m_rangeCollection = new List<Entity>();
			}
			return m_rangeCollection;
		}
	}

	protected Style(WordDocument doc)
		: base(doc, doc)
	{
		m_chFormat = new WCharacterFormat(base.Document);
		m_chFormat.SetOwner(this);
		m_strName = "Style" + doc.Styles.Count;
	}

	public virtual void ApplyBaseStyle(string styleName)
	{
		if (styleName == Name)
		{
			switch (StyleType)
			{
			case StyleType.ParagraphStyle:
				if (Name == "Normal")
				{
					return;
				}
				m_baseStyle = m_doc.Styles.FindByName("Normal");
				break;
			case StyleType.CharacterStyle:
				if (Name == "Default Paragraph Font" || Name == "DefaultParagraphFont")
				{
					return;
				}
				m_baseStyle = m_doc.Styles.FindByName("Default Paragraph Font");
				break;
			case StyleType.TableStyle:
				if (Name == "Normal Table" || Name == "TableNormal")
				{
					return;
				}
				m_baseStyle = m_doc.Styles.FindByName("Normal Table");
				break;
			case StyleType.NumberingStyle:
				if (Name == "No List" || Name == "NoList")
				{
					return;
				}
				m_baseStyle = m_doc.Styles.FindByName("No List");
				break;
			}
		}
		else
		{
			m_baseStyle = m_doc.Styles.FindByName(styleName, StyleType);
		}
		if (m_baseStyle == null)
		{
			m_baseStyle = m_doc.Styles.FindByName(styleName);
		}
		if (m_baseStyle == null && StyleType == StyleType.CharacterStyle)
		{
			m_baseStyle = new WCharacterStyle(m_doc);
		}
		else if (m_baseStyle == null)
		{
			m_baseStyle = new WParagraphStyle(m_doc);
		}
		CharacterFormat.ApplyBase(((Style)BaseStyle).CharacterFormat);
	}

	public void ApplyBaseStyle(BuiltinStyle bStyle)
	{
		IStyle style = m_doc.AddStyle(bStyle);
		if (style != null)
		{
			ApplyBaseStyle(style.Name);
		}
	}

	private bool IsStyleNotInBuiltinStyles()
	{
		return !GetBuiltinStyles().ContainsKey(Name.ToLower());
	}

	public void Remove()
	{
		if (IsCustom || IsStyleNotInBuiltinStyles())
		{
			IsRemoving = true;
			if (this is WParagraphStyle)
			{
				foreach (Entity item in RangeCollection)
				{
					if (item is WParagraph)
					{
						(item as WParagraph).ApplyStyle(BuiltinStyle.Normal);
					}
				}
				foreach (Style style3 in base.Document.Styles)
				{
					WParagraphStyle wParagraphStyle = null;
					if (style3 is WParagraphStyle wParagraphStyle2 && this == style3.BaseStyle)
					{
						wParagraphStyle2.ParagraphFormat.CopyFormat((this as WParagraphStyle).ParagraphFormat);
						wParagraphStyle2.CharacterFormat.CopyFormat((this as WParagraphStyle).CharacterFormat);
						style3.ApplyBaseStyle(BuiltinStyle.Normal);
					}
				}
			}
			else if (this is WCharacterStyle)
			{
				foreach (Entity item2 in RangeCollection)
				{
					if (item2 is WParagraph)
					{
						(item2 as WParagraph).BreakCharacterFormat.CharStyleName = "Default Paragraph Font";
					}
					else
					{
						(item2 as ParagraphItem).GetCharFormat().CharStyleName = "Default Paragraph Font";
					}
				}
				foreach (Style style4 in base.Document.Styles)
				{
					WCharacterStyle wCharacterStyle = null;
					if (style4 is WCharacterStyle wCharacterStyle2 && this == style4.BaseStyle)
					{
						wCharacterStyle2.CharacterFormat.CopyFormat(((WCharacterStyle)this).CharacterFormat);
						wCharacterStyle2.ApplyBaseStyle(BuiltinStyle.DefaultParagraphFont);
					}
				}
			}
			else if (StyleType == StyleType.NumberingStyle)
			{
				foreach (Entity item3 in RangeCollection)
				{
					if (item3 is WParagraph)
					{
						(item3 as WParagraph).ListFormat.RemoveList();
					}
				}
				foreach (ListStyle listStyle in base.Document.ListStyles)
				{
					if (listStyle.StyleLink == Name)
					{
						listStyle.StyleLink = null;
					}
				}
			}
			(base.Document.Styles as StyleCollection).Remove(this);
			Close();
			return;
		}
		throw new InvalidOperationException("Built-in styles cannot be removed from a Word document");
	}

	public abstract IStyle Clone();

	private void SetLinkedStyle(string linkStyleName)
	{
		if (!(base.Document.Styles.FindByName(linkStyleName) is Style style))
		{
			throw new Exception("Specified style does not exist in the document style collection");
		}
		if (LinkedStyleName == null && style.LinkedStyleName == null && StyleType != style.StyleType)
		{
			m_linkedStyleName = linkStyleName;
			style.m_linkedStyleName = Name;
			if ((StyleType == StyleType.ParagraphStyle && style.StyleType == StyleType.CharacterStyle) || (StyleType == StyleType.CharacterStyle && style.StyleType == StyleType.ParagraphStyle) || (StyleType == StyleType.TableStyle && style.StyleType == StyleType.CharacterStyle))
			{
				m_chFormat = style.CharacterFormat;
			}
			return;
		}
		throw new Exception("The specified link style is of an incorrect type");
	}

	internal virtual void ApplyBaseStyle(Style baseStyle)
	{
		m_baseStyle = baseStyle;
		CharacterFormat.ApplyBase(((Style)BaseStyle).CharacterFormat);
	}

	internal void RemoveBaseStyle()
	{
		if (this is WParagraphStyle)
		{
			WParagraphStyle obj = this as WParagraphStyle;
			obj.CharacterFormat.BaseFormat = null;
			obj.ParagraphFormat.ApplyBase(null);
			obj.m_baseStyle = null;
		}
		else if (this is WCharacterStyle)
		{
			WCharacterStyle wCharacterStyle = this as WCharacterStyle;
			if (wCharacterStyle.CharacterFormat.BaseFormat != null)
			{
				wCharacterStyle.CharacterFormat.BaseFormat = null;
			}
			if (wCharacterStyle.m_baseStyle != null)
			{
				wCharacterStyle.m_baseStyle = null;
			}
		}
	}

	internal void SetStyleName(string name)
	{
		if (name == null || name.Length == 0)
		{
			throw new ArgumentNullException("Style Name should not be null or empty");
		}
		m_strName = name;
	}

	protected override object CloneImpl()
	{
		Style style = (Style)base.CloneImpl();
		style.m_chFormat = new WCharacterFormat(base.Document);
		style.m_chFormat.ImportContainer(CharacterFormat);
		style.m_chFormat.CopyProperties(CharacterFormat);
		style.m_chFormat.SetOwner(style);
		return style;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (doc == base.Document)
		{
			return;
		}
		if (m_baseStyle != null && (m_baseStyle as Style).ImportStyleTo(doc, isParagraphStyle: false) is Style baseStyle)
		{
			if (this is WParagraphStyle)
			{
				(this as WParagraphStyle).ApplyBaseStyle(baseStyle);
			}
			else if (this is WTableStyle)
			{
				(this as WTableStyle).ApplyBaseStyle(baseStyle);
			}
			else
			{
				ApplyBaseStyle(baseStyle);
			}
		}
		CharacterFormat.CloneRelationsTo(doc);
		if (this is WParagraphStyle)
		{
			(this as WParagraphStyle).ListFormat.CloneListRelationsTo(doc, Name);
		}
		else if (this is WTableStyle)
		{
			(this as WTableStyle).ListFormat.CloneListRelationsTo(doc, Name);
		}
		SetOwner(doc);
	}

	internal static bool HasGuid(string styleName, out string guid)
	{
		guid = string.Empty;
		char[] separator = new char[1] { '-' };
		if (styleName.Contains("_") && styleName.Contains("-"))
		{
			int num = styleName.LastIndexOf("_") + 1;
			if (styleName.Length > num)
			{
				guid = styleName.Substring(num);
			}
			string[] array = guid.Split(separator);
			if (array.Length == 5 && guid.Length - 4 == 32 && array[0].Length == 8 && array[1].Length == 4 && array[2].Length == 4 && array[3].Length == 4 && array[4].Length == 12)
			{
				return true;
			}
		}
		return false;
	}

	internal virtual bool Compare(Style style)
	{
		if (StyleType != style.StyleType)
		{
			return false;
		}
		if (BaseStyle != null && style.BaseStyle != null)
		{
			if (!(BaseStyle as Style).Compare((Style)style.BaseStyle))
			{
				return false;
			}
		}
		else if ((BaseStyle != null && style.BaseStyle == null) || (BaseStyle == null && style.BaseStyle != null))
		{
			return false;
		}
		if (style.CharacterFormat != null && CharacterFormat != null && !CharacterFormat.Compare(style.CharacterFormat))
		{
			return false;
		}
		return true;
	}

	internal IStyle ImportStyleTo(WordDocument doc, bool isParagraphStyle)
	{
		if (doc == base.Document)
		{
			return this;
		}
		List<string> styelNames = new List<string>();
		bool isDiffTypeStyleFound = false;
		Style style = (doc.Styles as StyleCollection).FindByName(Name, StyleType, ref styelNames, ref isDiffTypeStyleFound) as Style;
		if (style == null)
		{
			Style style2 = Clone() as Style;
			if (isDiffTypeStyleFound)
			{
				style = (doc.ImportStylesOnTypeMismatch ? (AddNewStyle(doc, style2, isToClone: true, styelNames) as Style) : ((StyleType == StyleType.CharacterStyle) ? (doc.Styles.FindByName("Default Paragraph Font", StyleType.CharacterStyle) as Style) : ((StyleType == StyleType.ParagraphStyle) ? (doc.Styles.FindByName("Normal", StyleType.ParagraphStyle) as Style) : ((StyleType != StyleType.TableStyle) ? (doc.Styles.FindByName("No List", StyleType.NumberingStyle) as Style) : (doc.Styles.FindByName("Normal Table", StyleType.TableStyle) as Style)))));
			}
			else
			{
				if (isParagraphStyle && doc.UpdateAlternateChunk && style2 is WParagraphStyle)
				{
					CopyBaseStyleFormatting(doc, style2 as WParagraphStyle);
				}
				style = AddNewStyle(doc, style2, isToClone: false, styelNames) as Style;
			}
		}
		else if (doc.ImportStyles)
		{
			style = CompareAndImportStyle(style, doc, styelNames);
		}
		styelNames.Clear();
		return style;
	}

	private Style CompareAndImportStyle(Style foundStyle, WordDocument doc, List<string> styleNames)
	{
		if (StyleType == StyleType.CharacterStyle || StyleType == StyleType.ParagraphStyle)
		{
			if (Compare(foundStyle))
			{
				return foundStyle;
			}
			foreach (Style style in doc.Styles)
			{
				if (style.StyleType == StyleType && style.Name.StartsWith(Name + "_") && Compare(style))
				{
					return style;
				}
			}
		}
		foundStyle = AddNewStyle(doc, Clone() as Style, isToClone: true, styleNames) as Style;
		return foundStyle;
	}

	private IStyle AddNewStyle(WordDocument doc, Style newStyle, bool isToClone, List<string> styleNames)
	{
		string name = Name;
		if (isToClone)
		{
			if (newStyle != null)
			{
				newStyle.SetStyleName(GetUniqueStyleName(name, styleNames));
				newStyle.StyleId = 4094;
			}
		}
		else if (base.Document.StyleNameIds.ContainsValue(name) && !doc.StyleNameIds.ContainsValue(name))
		{
			string styleNameId = GetStyleNameId(name);
			if (!doc.StyleNameIds.ContainsKey(styleNameId))
			{
				doc.StyleNameIds.Add(styleNameId, name);
			}
		}
		newStyle.RangeCollection.Clear();
		doc.Styles.Add(newStyle);
		return newStyle;
	}

	private void CopyBaseStyleFormatting(WordDocument destDocument, WParagraphStyle paraStyle)
	{
		WParagraphStyle baseStyle = paraStyle.BaseStyle;
		if (baseStyle != null && !string.IsNullOrEmpty(baseStyle.Name) && destDocument.Styles.FindByName(baseStyle.Name) is WParagraphStyle wParagraphStyle)
		{
			paraStyle.ParagraphFormat.UpdateSourceFormat(wParagraphStyle.ParagraphFormat);
			paraStyle.CharacterFormat.UpdateSourceFormat(wParagraphStyle.CharacterFormat);
		}
	}

	internal string GetUniqueStyleName(string styleName, List<string> styleNames)
	{
		while (styleNames.Contains(styleName))
		{
			styleName = styleName + "_" + Guid.NewGuid();
		}
		return styleName;
	}

	private string GetStyleNameId(string styleName)
	{
		string result = "";
		foreach (KeyValuePair<string, string> styleNameId in base.Document.StyleNameIds)
		{
			if (styleNameId.Value == styleName)
			{
				result = styleNameId.Key;
				break;
			}
		}
		return result;
	}

	public static IStyle CreateBuiltinStyle(BuiltinStyle bStyle, WordDocument doc)
	{
		IStyle style = new WParagraphStyle(doc);
		if (doc.Styles.FindByName(BuiltInToName(bStyle), StyleType.ParagraphStyle) is WParagraphStyle result)
		{
			return result;
		}
		if (doc.IsOpening && doc.ActualFormatType == FormatType.Docx && bStyle == BuiltinStyle.Normal)
		{
			(style as Style).SetStyleName(BuiltInToName(bStyle));
		}
		else
		{
			BuiltinStyleLoader.LoadStyle(style, bStyle);
		}
		if (style.Name == "Normal" && style.StyleType == StyleType.ParagraphStyle)
		{
			(style as WParagraphStyle).CharacterFormat.LocaleIdASCII = 1033;
		}
		if (style.Name == "Normal (Web)" && style.StyleType == StyleType.ParagraphStyle && doc.ActualFormatType == FormatType.Html)
		{
			(style as WParagraphStyle).ParagraphFormat.BeforeSpacing = 5f;
			(style as WParagraphStyle).ParagraphFormat.AfterSpacing = 5f;
			(style as WParagraphStyle).ParagraphFormat.SpaceAfterAuto = true;
			(style as WParagraphStyle).ParagraphFormat.SpaceBeforeAuto = true;
		}
		return style;
	}

	internal static IStyle CreateBuiltinCharacterStyle(BuiltinStyle bStyle, WordDocument doc)
	{
		IStyle style = new WCharacterStyle(doc);
		if (doc.Styles.FindByName(BuiltInToName(bStyle), StyleType.CharacterStyle) is WCharacterStyle result)
		{
			return result;
		}
		BuiltinStyleLoader.LoadStyle(style, bStyle);
		return style;
	}

	internal static IStyle CreateBuiltinStyle(BuiltinTableStyle bStyle, WordDocument doc)
	{
		WTableStyle wTableStyle = new WTableStyle(doc);
		BuiltinStyleLoader.LoadStyle(wTableStyle, bStyle);
		return wTableStyle;
	}

	public static IStyle CreateBuiltinStyle(BuiltinStyle bStyle, StyleType type, WordDocument doc)
	{
		IStyle style = null;
		switch (type)
		{
		case StyleType.CharacterStyle:
			style = new WCharacterStyle(doc);
			break;
		case StyleType.ParagraphStyle:
			style = new WParagraphStyle(doc);
			break;
		case StyleType.OtherStyle:
			style = new ListStyle(doc);
			break;
		}
		BuiltinStyleLoader.LoadStyle(style, bStyle);
		return style;
	}

	internal static string BuiltInToName(BuiltinStyle bstyle)
	{
		return BuiltinStyleLoader.BuiltinStyleNames[(int)bstyle];
	}

	internal static string BuiltInToName(BuiltinTableStyle bstyle)
	{
		return BuiltinStyleLoader.BuiltinTableStyleNames[(int)bstyle];
	}

	public static BuiltinStyle NameToBuiltIn(string styleName)
	{
		string text = styleName.Trim();
		BuiltinStyle result = BuiltinStyle.User;
		int num = BuiltinStyleLoader.BuiltinStyleNames.Length;
		for (int i = 0; i < num; i++)
		{
			if (BuiltinStyleLoader.BuiltinStyleNames[i] == text)
			{
				result = (BuiltinStyle)i;
				break;
			}
		}
		return result;
	}

	internal static bool IsListStyle(BuiltinStyle bstyle)
	{
		return BuiltinStyleLoader.IsListStyle(bstyle);
	}

	void IStyle.Close()
	{
		Close();
	}

	internal new virtual void Close()
	{
		if (m_chFormat != null)
		{
			m_chFormat.Close();
			m_chFormat = null;
		}
		if (m_tapx != null)
		{
			m_tapx = null;
		}
		if (m_baseStyle != null)
		{
			m_baseStyle = null;
		}
		if (m_rangeCollection != null)
		{
			m_rangeCollection.Clear();
			m_rangeCollection = null;
		}
		base.Close();
	}

	internal Dictionary<string, string> GetBuiltinStyles()
	{
		return new Dictionary<string, string>
		{
			{ "normal", "Normal" },
			{ "heading 1", "Heading 1" },
			{ "heading 2", "Heading 2" },
			{ "heading 3", "Heading 3" },
			{ "heading 4", "Heading 4" },
			{ "heading 5", "Heading 5" },
			{ "heading 6", "Heading 6" },
			{ "heading 7", "Heading 7" },
			{ "heading 8", "Heading 8" },
			{ "heading 9", "Heading 9" },
			{ "index 1", "Index 1" },
			{ "index 2", "Index 2" },
			{ "index 3", "Index 3" },
			{ "index 4", "Index 4" },
			{ "index 5", "Index 5" },
			{ "index 6", "Index 6" },
			{ "index 7", "Index 7" },
			{ "index 8", "Index 8" },
			{ "index 9", "Index 9" },
			{ "toc 1", "TOC 1" },
			{ "toc 2", "TOC 2" },
			{ "toc 3", "TOC 3" },
			{ "toc 4", "TOC 4" },
			{ "toc 5", "TOC 5" },
			{ "toc 6", "TOC 6" },
			{ "toc 7", "TOC 7" },
			{ "toc 8", "TOC 8" },
			{ "toc 9", "TOC 9" },
			{ "normal indent", "Normal Indent" },
			{ "footnote text", "Footnote Text" },
			{ "comment text", "Comment Text" },
			{ "header", "Header" },
			{ "footer", "Footer" },
			{ "index heading", "Index Heading" },
			{ "caption", "Caption" },
			{ "table of figures", "Table of Figures" },
			{ "footnote reference", "Footnote Reference" },
			{ "comment reference", "Comment Reference" },
			{ "line number", "Line Number" },
			{ "page number", "Page Number" },
			{ "endnote reference", "Endnote Reference" },
			{ "endnote text", "Endnote Text" },
			{ "table of authorities", "Table of Authorities" },
			{ "macro", "Macro Text" },
			{ "toa heading", "TOA Heading" },
			{ "list", "List" },
			{ "list bullet", "List Bullet" },
			{ "list number", "List Number" },
			{ "list 2", "List 2" },
			{ "list 3", "List 3" },
			{ "list 4", "List 4" },
			{ "list 5", "List 5" },
			{ "list bullet 2", "List Bullet 2" },
			{ "list bullet 3", "List Bullet 3" },
			{ "list bullet 4", "List Bullet 4" },
			{ "list bullet 5", "List Bullet 5" },
			{ "list number 2", "List Number 2" },
			{ "list number 3", "List Number 3" },
			{ "list number 4", "List Number 4" },
			{ "list number 5", "List Number 5" },
			{ "title", "Title" },
			{ "closing", "Closing" },
			{ "signature", "Signature" },
			{ "default paragraph font", "Default Paragraph Font" },
			{ "body text", "Body Text" },
			{ "body text indent", "Body Text Indent" },
			{ "list continue", "List Continue" },
			{ "list continue 2", "List Continue 2" },
			{ "list continue 3", "List Continue 3" },
			{ "list continue 4", "List Continue 4" },
			{ "list continue 5", "List Continue 5" },
			{ "message header", "Message Header" },
			{ "subtitle", "Subtitle" },
			{ "salutation", "Salutation" },
			{ "date", "Date" },
			{ "body text first indent", "Body Text First Indent" },
			{ "body text first indent 2", "Body Text First Indent 2" },
			{ "note heading", "Note Heading" },
			{ "body text 2", "Body Text 2" },
			{ "body text 3", "Body Text 3" },
			{ "body text indent 2", "Body Text Indent 2" },
			{ "body text indent 3", "Body Text Indent 3" },
			{ "block text", "Block Text" },
			{ "hyperlink", "Hyperlink" },
			{ "followedhyperlink", "FollowedHyperlink" },
			{ "strong", "Strong" },
			{ "emphasis", "Emphasis" },
			{ "document map", "Document Map" },
			{ "plain text", "Plain Text" },
			{ "e-mail signature", "E-mail Signature" },
			{ "normal (web)", "Normal (Web)" },
			{ "html acronym", "HTML Acronym" },
			{ "html address", "HTML Address" },
			{ "html cite", "HTML Cite" },
			{ "html code", "HTML Code" },
			{ "html definition", "HTML Definition" },
			{ "html keyboard", "HTML Keyboard" },
			{ "html preformatted", "HTML Preformatted" },
			{ "html sample", "HTML Sample" },
			{ "html typewriter", "HTML Typewriter" },
			{ "html variable", "HTML Variable" },
			{ "comment subject", "Comment Subject" },
			{ "no list", "No List" },
			{ "balloon text", "Balloon Text" },
			{ "user", "User" },
			{ "nostyle", "NoStyle" },
			{ "list paragraph", "List Paragraph" },
			{ "quote", "Quote" },
			{ "normal table", "Normal Table" },
			{ "table grid", "Table Grid" },
			{ "light shading", " Light Shading" },
			{ "light shading accent 1", "Light Shading Accent 1" },
			{ "light shading accent 2", "Light Shading Accent 2" },
			{ "light shading accent 3", "Light Shading Accent 3" },
			{ "light shading accent 4", "Light Shading Accent 4" },
			{ "light shading accent 5", "Light Shading Accent 5" },
			{ "light shading accent 6", "Light Shading Accent 6" },
			{ "light list", "Light List" },
			{ "light list accent 1", "Light List Accent 1" },
			{ "light list accent 2", "Light List Accent 2" },
			{ "light list accent 3", "Light List Accent 3" },
			{ "light list accent 4", "Light List Accent 4" },
			{ "light list accent 5", "Light List Accent 5" },
			{ "light list accent 6", "Light List Accent 6" },
			{ "light grid", "Light Grid" },
			{ "light grid accent 1", "Light Grid Accent 1" },
			{ "light grid accent 2", "Light Grid Accent 2" },
			{ "light grid accent 3", " Light Grid Accent 3" },
			{ "light grid accent 4", "Light Grid Accent 4" },
			{ "light grid accent 5", "Light Grid Accent 5" },
			{ "light grid accent 6", "Light Grid Accent 6" },
			{ "medium shading 1", "Medium Shading 1" },
			{ "medium shading 1 accent 1", "Medium Shading 1 Accent 1" },
			{ "medium shading 1 accent 2", "Medium Shading 1 Accent 2" },
			{ "medium shading 1 accent 3", "Medium Shading 1 Accent 3" },
			{ "medium shading 1 accent 4", "Medium Shading 1 Accent 4" },
			{ "medium shading 1 accent 5", "Medium Shading 1 Accent 5" },
			{ "medium shading 1 accent 6", "Medium Shading 1 Accent 6" },
			{ "medium shading 2", "Medium Shading 2" },
			{ "medium shading 2 accent 1", "Medium Shading 2 Accent 1" },
			{ "medium shading 2 accent 2", "Medium Shading 2 Accent 2" },
			{ "medium shading 2 accent 3", "Medium Shading 2 Accent 3" },
			{ "medium shading 2 accent 4", "Medium Shading 2 Accent 4" },
			{ "medium shading 2 accent 5", "Medium Shading 2 Accent 5" },
			{ "medium shading 2 accent 6", "Medium Shading 2 Accent 6" },
			{ "medium list 1", "Medium List 1" },
			{ "medium list 1 accent 1", "Medium List 1 Accent 1" },
			{ "medium list 1 accent 2", "Medium List 1 Accent 2" },
			{ "medium list 1 accent 3", "Medium List 1 Accent 3" },
			{ "medium list 1 accent 4", "Medium List 1 Accent 4" },
			{ "medium list 1 accent 5", "Medium List 1 Accent 5" },
			{ "medium list 1 accent 6", "Medium List 1 Accent 6" },
			{ "medium list 2", "Medium List 2" },
			{ "medium list 2 accent 1", "Medium List 2 Accent 1" },
			{ "medium list 2 accent 2", "Medium List 2 Accent 2" },
			{ "medium list 2 accent 3", "Medium List 2 Accent 3" },
			{ "medium list 2 accent 4", "Medium List 2 Accent 4" },
			{ "medium list 2 accent 5", "Medium List 2 Accent 5" },
			{ "medium list 2 accent 6", "Medium List 2 Accent 6" },
			{ "medium grid 1", "Medium Grid 1" },
			{ "medium grid 1 accent 1", "Medium Grid 1 Accent 1" },
			{ "medium grid 1 accent 2", "Medium Grid 1 Accent 2" },
			{ "medium grid 1 accent 3", "Medium Grid 1 Accent 3" },
			{ "medium grid 1 accent 4", "Medium Grid 1 Accent 4" },
			{ "medium grid 1 accent 5", "Medium Grid 1 Accent 5" },
			{ "medium grid 1 accent 6", "Medium Grid 1 Accent 6" },
			{ "medium grid 2", "Medium Grid 2" },
			{ "medium grid 2 accent 1", "Medium Grid 2 Accent 1" },
			{ "medium grid 2 accent 2", "Medium Grid 2 Accent 2" },
			{ "medium grid 2 accent 3", "Medium Grid 2 Accent 3" },
			{ "medium grid 2 accent 4", "Medium Grid 2 Accent 4" },
			{ "medium grid 2 accent 5", "Medium Grid 2 Accent 5" },
			{ "medium grid 2 accent 6", "Medium Grid 2 Accent 6" },
			{ "medium grid 3", "Medium Grid 3" },
			{ "medium grid 3 accent 1", "Medium Grid 3 Accent 1" },
			{ "medium grid 3 accent 2", "Medium Grid 3 Accent 2" },
			{ "medium grid 3 accent 3", "Medium Grid 3 Accent 3" },
			{ "medium grid 3 accent 4", "Medium Grid 3 Accent 4" },
			{ "medium grid 3 accent 5", "Medium Grid 3 Accent5" },
			{ "medium grid 3 accent 6", "Medium Grid 3 Accent 6" },
			{ "dark list", "Dark List" },
			{ "dark list accent 1", "Dark List Accent 1" },
			{ "dark list accent 2", "Dark List Accent 2" },
			{ "dark list accent 3", "Dark List Accent 3" },
			{ "dark list accent 4", "Dark List Accent 4" },
			{ "dark list accent 5", "Dark List Accent 5" },
			{ "dark list accent 6", "Dark List Accent 6" },
			{ "colorful shading", "Colorful Shading" },
			{ "colorful shading accent 1", "Colorful Shading Accent 1" },
			{ "colorful shading accent 2", "Colorful Shading Accent 2" },
			{ "colorful shading accent 3", "Colorful Shading Accent 3" },
			{ "colorful shading accent 4", "Colorful Shading Accent 4" },
			{ "colorful shading accent 5", "Colorful Shading Accent 5" },
			{ "colorful shading accent 6", "Colorful Shading Accent 6" },
			{ "colorful list", "Colorful List" },
			{ "colorful list accent 1", "Colorful List Accent 1" },
			{ "colorful list accent 2", "Colorful List Accent 2" },
			{ "colorful list accent 3", "Colorful List Accent 3" },
			{ "colorful list accent 4", "Colorful List Accent 4" },
			{ "colorful list accent 5", "Colorful List Accent 5" },
			{ "colorful list accent 6", "Colorful List Accent 6" },
			{ "colorful grid", "Colorful Grid" },
			{ "colorful grid accent 1", "Colorful Grid Accent 1" },
			{ "colorful grid accent 2", "Colorful Grid Accent 2" },
			{ "colorful grid accent 3", "Colorful Grid Accent 3" },
			{ "colorful grid accent 4", "Colorful Grid Accent 4" },
			{ "colorful grid accent 5", "Colorful Grid Accent 5" },
			{ "colorful grid accent 6", "Colorful Grid Accent 6" },
			{ "table 3d effects 1", "Table 3D effects 1" },
			{ "table 3d effects 2", "Table 3D effects 2" },
			{ "table 3d effects 3", "Table 3D effects 3" },
			{ "table classic 1", "Table Classic 1" },
			{ "table classic 2", "Table Classic 2" },
			{ "table classic 3", "Table Classic 3" },
			{ "table classic 4", "Table Classic 4" },
			{ "table colorful 1", "Table Colorful 1" },
			{ "table colorful 2", "Table Colorful 2" },
			{ "table colorful 3", "Table Colorful 3" },
			{ "table columns 1", "Table Columns 1" },
			{ "table columns 2", "Table Columns 2" },
			{ "table columns 3", "Table Columns 3" },
			{ "table columns 4", "Table Columns 4" },
			{ "table columns 5", "Table Columns 5" },
			{ "table contemporary", "Table Contemporary" },
			{ "table elegant", "Table Elegant" },
			{ "table grid 1", "Table Grid 1" },
			{ "table grid 2", "Table Grid 2" },
			{ "table grid 3", "Table Grid 3" },
			{ "table grid 4", "Table Grid 4" },
			{ "table grid 5", "Table Grid 5" },
			{ "table grid 6", "Table Grid 6" },
			{ "table grid 7", "Table Grid 7" },
			{ "table grid 8", "Table Grid 8" },
			{ "table list 1", "Table List 1" },
			{ "table list 2", "Table List 2" },
			{ "table list 3", "Table List 3" },
			{ "table list 4", "Table List 4" },
			{ "table list 5", "Table List 5" },
			{ "table list 6", "Table List 6" },
			{ "table list 7", "Table List 7" },
			{ "table list 8", "Table List 8" },
			{ "table professional", "Table Professional" },
			{ "table simple 1", "Table Simple 1" },
			{ "table simple 2", "Table Simple 2" },
			{ "table simple 3", "Table Simple 3" },
			{ "table subtle 1", "Table Subtle 1" },
			{ "table subtle 2", "Table Subtle 2" },
			{ "table theme", "Table Theme" },
			{ "table web 1", "Table Web 1" },
			{ "table web 2", "Table Web 2" },
			{ "table web 3", "Table Web 3" }
		};
	}

	internal Dictionary<string, int> GetBuiltinStyleIds()
	{
		return new Dictionary<string, int>
		{
			{ "normal", 0 },
			{ "defaultparagraphfont", 65 },
			{ "nospacing", 157 },
			{ "heading1", 1 },
			{ "heading2", 2 },
			{ "heading3", 3 },
			{ "heading4", 4 },
			{ "heading5", 5 },
			{ "heading6", 6 },
			{ "heading7", 7 },
			{ "heading8", 8 },
			{ "heading9", 9 },
			{ "title", 62 },
			{ "subtitle", 74 },
			{ "subtleemphasis", 260 },
			{ "emphasis", 88 },
			{ "intenseemphasis", 261 },
			{ "strong", 87 },
			{ "quote", 180 },
			{ "intensequote", 181 },
			{ "subtlereference", 262 },
			{ "intensereference", 263 },
			{ "booktitle", 264 },
			{ "listparagraph", 179 },
			{ "caption", 34 },
			{ "bibliography", 265 },
			{ "toc1", 19 },
			{ "toc2", 20 },
			{ "toc3", 21 },
			{ "toc4", 22 },
			{ "toc5", 23 },
			{ "toc6", 24 },
			{ "toc7", 25 },
			{ "toc8", 26 },
			{ "toc9", 27 },
			{ "tocheading", 266 },
			{ "tablegrid", 154 },
			{ "lightshading", 158 },
			{ "lightshadingaccent1", 172 },
			{ "lightshadingaccent2", 190 },
			{ "lightshadingaccent3", 204 },
			{ "lightshadingaccent4", 218 },
			{ "lightshadingaccent5", 232 },
			{ "lightshadingaccent6", 246 },
			{ "lightlist", 159 },
			{ "lightlistaccent1", 173 },
			{ "lightlistaccent2", 191 },
			{ "lightlistaccent3", 205 },
			{ "lightlistaccent4", 219 },
			{ "lightlistaccent5", 233 },
			{ "lightlistaccent6", 247 },
			{ "lightgrid", 160 },
			{ "lightgridaccent1", 174 },
			{ "lightgridaccent2", 192 },
			{ "lightgridaccent3", 206 },
			{ "lightgridaccent4", 220 },
			{ "lightgridaccent5", 234 },
			{ "lightgridaccent6", 248 },
			{ "mediumshading1", 161 },
			{ "mediumshading1accent1", 175 },
			{ "mediumshading1accent2", 193 },
			{ "mediumshading1accent3", 207 },
			{ "mediumshading1accent4", 221 },
			{ "mediumshading1accent5", 235 },
			{ "mediumshading1accent6", 249 },
			{ "mediumshading2", 162 },
			{ "mediumshading2accent1", 176 },
			{ "mediumshading2accent2", 194 },
			{ "mediumshading2accent3", 208 },
			{ "mediumshading2accent4", 222 },
			{ "mediumshading2accent5", 236 },
			{ "mediumshading2accent6", 250 },
			{ "mediumlist1", 163 },
			{ "mediumlist1accent1", 177 },
			{ "mediumlist1accent2", 195 },
			{ "mediumlist1accent3", 209 },
			{ "mediumlist1accent4", 223 },
			{ "mediumlist1accent5", 237 },
			{ "mediumlist1accent6", 251 },
			{ "mediumlist2", 164 },
			{ "mediumlist2accent1", 182 },
			{ "mediumlist2accent2", 196 },
			{ "mediumlist2accent3", 210 },
			{ "mediumlist2accent4", 224 },
			{ "mediumlist2accent5", 238 },
			{ "mediumlist2accent6", 252 },
			{ "mediumgrid1", 165 },
			{ "mediumgrid1accent1", 183 },
			{ "mediumgrid1accent2", 197 },
			{ "mediumgrid1accent3", 211 },
			{ "mediumgrid1accent4", 225 },
			{ "mediumgrid1accent5", 239 },
			{ "mediumgrid1accent6", 253 },
			{ "mediumgrid2", 166 },
			{ "mediumgrid2accent1", 184 },
			{ "mediumgrid2accent2", 198 },
			{ "mediumgrid2accent3", 212 },
			{ "mediumgrid2accent4", 226 },
			{ "mediumgrid2accent5", 240 },
			{ "mediumgrid2accent6", 254 },
			{ "mediumgrid3", 167 },
			{ "mediumgrid3accent1", 185 },
			{ "mediumgrid3accent2", 199 },
			{ "mediumgrid3accent3", 213 },
			{ "mediumgrid3accent4", 227 },
			{ "mediumgrid3accent5", 241 },
			{ "mediumgrid3accent6", 255 },
			{ "darklist", 168 },
			{ "darklistaccent1", 186 },
			{ "darklistaccent2", 200 },
			{ "darklistaccent3", 214 },
			{ "darklistaccent4", 228 },
			{ "darklistaccent5", 242 },
			{ "darklistaccent6", 256 },
			{ "colorfulshading", 169 },
			{ "colorfulshadingaccent1 ", 187 },
			{ "colorfulshadingaccent2", 201 },
			{ "colorfulshadingaccent3 ", 215 },
			{ "colorfulshadingaccent4", 229 },
			{ "colorfulshadingaccent5", 243 },
			{ "colorfulshadingaccent6", 257 },
			{ "colorfullist", 170 },
			{ "colorfullistaccent1", 188 },
			{ "colorfullistaccent2", 202 },
			{ "colorfullistaccent3", 216 },
			{ "colorfullistaccent4", 230 },
			{ "colorfullistaccent5", 244 },
			{ "colorfullistaccent6", 258 },
			{ "colorfulgrid", 171 },
			{ "colorfulgridaccent1", 189 },
			{ "colorfulgridaccent2", 203 },
			{ "colorfulgridaccent3", 217 },
			{ "colorfulgridaccent4", 231 },
			{ "colorfulgridaccent5", 245 },
			{ "colorfulgridaccent6", 259 },
			{ "balloontext", 153 },
			{ "blocktext", 84 },
			{ "bodytext", 66 },
			{ "bodytext2", 80 },
			{ "bodytext3", 81 },
			{ "bodytextfirstindent", 77 },
			{ "bodytextfirstindent2", 78 },
			{ "bodytextindent", 67 },
			{ "bodytextindent2", 82 },
			{ "bodytextindent3", 83 },
			{ "closing", 63 },
			{ "commentreference", 39 },
			{ "commentsubject", 106 },
			{ "commenttext", 30 },
			{ "date", 76 },
			{ "documentmap", 89 },
			{ "e-mailsignature", 91 },
			{ "endnotereference", 42 },
			{ "endnotetext", 43 },
			{ "envelopeaddress", 36 },
			{ "envelopereturn", 37 },
			{ "followedhyperlink", 86 },
			{ "footer", 32 },
			{ "footnotereference", 38 },
			{ "footnotetext", 29 },
			{ "header", 31 },
			{ "htmlacronym", 95 },
			{ "htmladdress", 96 },
			{ "htmlcite", 97 },
			{ "htmlcode", 98 },
			{ "htmldefinition", 99 },
			{ "htmlkeyboard", 100 },
			{ "htmlpreformatted", 101 },
			{ "htmlsample", 102 },
			{ "htmltypewriter", 103 },
			{ "htmlvariable", 104 },
			{ "hyperlink", 85 },
			{ "index1", 10 },
			{ "index2", 11 },
			{ "index3", 12 },
			{ "index4", 13 },
			{ "index5", 14 },
			{ "index6", 15 },
			{ "index7", 16 },
			{ "index8", 17 },
			{ "index9", 18 },
			{ "indexheading", 33 },
			{ "linenumber", 40 },
			{ "list", 47 },
			{ "list2", 50 },
			{ "list3", 51 },
			{ "list4", 52 },
			{ "list5", 53 },
			{ "listbullet", 48 },
			{ "listbullet2", 54 },
			{ "listbullet3", 55 },
			{ "listbullet4", 56 },
			{ "listbullet5", 57 },
			{ "listcontinue", 68 },
			{ "listcontinue2", 69 },
			{ "listcontinue3", 70 },
			{ "listcontinue4", 71 },
			{ "listcontinue5", 72 },
			{ "listnumber", 49 },
			{ "listnumber2", 58 },
			{ "listnumber3", 59 },
			{ "listnumber4", 60 },
			{ "listnumber5", 61 },
			{ "macrotext", 45 },
			{ "messageheader", 73 },
			{ "nolist", 107 },
			{ "normal(web)", 94 },
			{ "normalindent", 28 },
			{ "noteheading", 79 },
			{ "pagenumber", 41 },
			{ "placeholdertext", 156 },
			{ "plaintext", 90 },
			{ "salutation", 75 },
			{ "signature", 64 },
			{ "table3deffects1", 142 },
			{ "table3deffects2", 143 },
			{ "table3deffects3", 144 },
			{ "tableclassic1", 114 },
			{ "tableclassic2", 115 },
			{ "tableclassic3", 116 },
			{ "tableclassic4", 117 },
			{ "tablecolorful1", 118 },
			{ "tablecolorful2", 119 },
			{ "tablecolorful3", 120 },
			{ "tablecolumns1", 121 },
			{ "tablecolumns2", 122 },
			{ "tablecolumns3", 123 },
			{ "tablecolumns4", 124 },
			{ "tablecolumns5", 125 },
			{ "tablecontemporary", 145 },
			{ "tableelegant", 146 },
			{ "tablegrid1", 126 },
			{ "tablegrid2", 127 },
			{ "tablegrid3", 128 },
			{ "tablegrid4", 129 },
			{ "tablegrid5", 130 },
			{ "tablegrid6", 131 },
			{ "tablegrid7", 132 },
			{ "tablegrid8", 133 },
			{ "tablelist1", 134 },
			{ "tablelist2", 135 },
			{ "tablelist3", 136 },
			{ "tablelist4", 137 },
			{ "tablelist5", 138 },
			{ "tablelist6", 139 },
			{ "tablelist7", 140 },
			{ "tablelist8", 141 },
			{ "tablenormal", 105 },
			{ "normaltable", 105 },
			{ "tableofauthorities", 44 },
			{ "tableoffigures", 35 },
			{ "tableprofessional", 147 },
			{ "tablesimple1", 111 },
			{ "tablesimple2", 112 },
			{ "tablesimple3", 113 },
			{ "tablesubtle1", 148 },
			{ "tablesubtle2", 149 },
			{ "tabletheme", 155 },
			{ "tableweb1", 150 },
			{ "tableweb2", 151 },
			{ "tableweb3", 152 },
			{ "toaheading", 46 },
			{ "htmltopofform", 92 },
			{ "htmlbottomofform", 93 },
			{ "revision", 178 },
			{ "outlinelist1", 108 },
			{ "outlinelist2", 109 },
			{ "outlinelist3", 110 }
		};
	}

	internal virtual bool CompareStyleBetweenDocuments(Style style)
	{
		if (StyleType != style.StyleType)
		{
			return false;
		}
		if (style.CharacterFormat != null && CharacterFormat != null && !CharacterFormat.Compare(style.CharacterFormat))
		{
			CharacterFormat.CompareProperties(style.CharacterFormat);
			CharacterFormat.IsChangedFormat = true;
			CharacterFormat.FormatChangeAuthorName = base.Document.m_authorName;
			CharacterFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.CharacterFormatChange(CharacterFormat, null, null);
		}
		return true;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		writer.WriteValue("Name", Name);
		writer.WriteValue("StyleId", m_styleId);
		writer.WriteValue("type", StyleType);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		m_strName = reader.ReadString("Name");
		m_styleId = reader.ReadInt("StyleId");
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.EnableID = true;
		base.XDLSHolder.AddRefElement("base", m_baseStyle);
		base.XDLSHolder.AddElement("character-format", m_chFormat);
	}

	protected override void RestoreReference(string name, int index)
	{
		if (index > -1)
		{
			m_baseStyle = base.Document.Styles[index];
		}
	}
}

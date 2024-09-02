using System;
using System.Collections.Generic;
using System.Linq;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Redaction;

internal class StringMappingNet
{
	internal string text;

	internal Glyph[] glyph;

	private Dictionary<char, int> m_macReverseEncodeTable;

	internal string GetText(FontStructureHelperBase fontStructure, bool isHex)
	{
		if (this.glyph == null)
		{
			return this.text;
		}
		string text = string.Empty;
		string text2 = "";
		bool flag = this.text.Length >= 2;
		bool flag2 = this.text.StartsWith("(");
		bool flag3 = this.text.EndsWith(")");
		if (flag && flag2 && !flag3)
		{
			text2 = this.text.Substring(1, this.text.Length - 1);
		}
		else if (flag && !flag2 && flag3)
		{
			text2 = this.text.Substring(0, this.text.Length - 1);
		}
		else if (flag)
		{
			text2 = this.text.Substring(1, this.text.Length - 2);
		}
		bool flag4 = false;
		if (text2.Contains("(") || text2.Contains(")"))
		{
			string text3 = string.Empty;
			string text4 = text2;
			for (int i = 0; i < text4.Length; i++)
			{
				char c = text4[i];
				if (c.ToString() == "(" || c.ToString() == ")")
				{
					text3 = text3 + "\\" + c;
					flag4 = true;
				}
				else
				{
					text3 += c;
				}
			}
			text2 = text3;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, Glyph[]> dictionary2 = new Dictionary<string, Glyph[]>();
		string text5 = string.Empty;
		List<Glyph> list = new List<Glyph>();
		string text6 = string.Empty;
		int num = 0;
		int num2 = 0;
		Glyph[] array = this.glyph;
		foreach (Glyph glyph in array)
		{
			if (glyph.IsReplace)
			{
				if (text6 != string.Empty)
				{
					dictionary.Add("O-" + num2++, text6);
					text6 = string.Empty;
				}
				list.Add(glyph);
				text5 += glyph.ToUnicode;
				continue;
			}
			if (text5 != string.Empty)
			{
				string key = "R-" + num++;
				dictionary2.Add(key, list.ToArray());
				list.Clear();
				list = new List<Glyph>();
				dictionary.Add(key, text5);
				text5 = string.Empty;
			}
			text6 = ((!(glyph.ToUnicode == "(") && !(glyph.ToUnicode == ")")) ? (text6 + glyph.ToUnicode) : (text6 + "\\" + glyph.ToUnicode));
		}
		if (text5 != string.Empty)
		{
			string key2 = "R-" + num++;
			dictionary2.Add(key2, list.ToArray());
			dictionary.Add(key2, text5);
			list.Clear();
			list = new List<Glyph>();
			text5 = string.Empty;
		}
		if (text6 != string.Empty)
		{
			dictionary.Add("O-" + num2++, text6);
			text6 = string.Empty;
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			if (item.Key.Contains("O"))
			{
				string text7 = string.Empty;
				if (fontStructure.FontEncoding == "Identity-H" || (fontStructure.FontEncoding == "" && fontStructure.CharacterMapTable != null && fontStructure.CharacterMapTable.Count > 0))
				{
					string text4 = item.Value;
					for (int i = 0; i < text4.Length; i++)
					{
						char c2 = text4[i];
						double num3 = 0.0;
						if (c2.ToString() != "\\" || flag4)
						{
							string empty = string.Empty;
							if (fontStructure.CharacterMapTable.ContainsKey((int)c2))
							{
								empty = fontStructure.CharacterMapTable[(int)c2];
								string text8 = text2.Substring(0, empty.Length);
								text2 = text2.Substring(empty.Length, text2.Length - empty.Length);
								num3 = ((!(empty == text8)) ? GetKey(fontStructure.CharacterMapTable, c2.ToString()) : ((double)(int)c2));
							}
							else
							{
								num3 = GetKey(fontStructure.CharacterMapTable, c2.ToString());
								text2 = text2.Substring(c2.ToString().Length, text2.Length - c2.ToString().Length);
							}
							text7 += (char)num3;
						}
					}
				}
				else if (fontStructure.FontEncoding == "MacRomanEncoding")
				{
					if (m_macReverseEncodeTable == null || m_macReverseEncodeTable.Count <= 0)
					{
						GetReverseMacEncodeTable();
					}
					string text9 = string.Empty;
					string text4 = item.Value;
					for (int i = 0; i < text4.Length; i++)
					{
						char c3 = text4[i];
						text9 = ((!m_macReverseEncodeTable.ContainsKey(c3) || c3 == ' ') ? (text9 + c3) : (text9 + "\\" + Convert.ToString(m_macReverseEncodeTable[c3], 8)));
					}
					text7 += text9;
				}
				else
				{
					text7 += item.Value;
				}
				if (fontStructure.FontEncoding == "Identity-H")
				{
					string token = PdfString.ByteToString(PdfString.ToUnicodeArray(text7, bAddPrefix: false));
					byte[] value = GetUnicodeString(token).PdfEncode(null);
					text += new PdfString(value).Value;
				}
				else if (!isHex)
				{
					text += "(";
					text = ((!(fontStructure.FontName == "ZapfDingbats") || fontStructure.IsEmbedded) ? (text + text7) : (text + ReverseMapZapf(text7, fontStructure)));
					text += ")";
				}
				else
				{
					text += "<";
					if (fontStructure.FontName == "ZapfDingbats" && !fontStructure.IsEmbedded)
					{
						text += ReverseMapZapf(text7, fontStructure);
					}
					else
					{
						char[] array2 = text7.ToCharArray();
						text7 = string.Empty;
						char[] array3 = array2;
						for (int i = 0; i < array3.Length; i++)
						{
							text7 += Convert.ToByte(array3[i]).ToString("X2");
						}
						text += text7;
					}
					text += ">";
				}
			}
			if (item.Key.Contains("R"))
			{
				text = text + " -" + GetReplacedChar(dictionary2[item.Key], fontStructure) + " ";
				text2 = ReplacedText(text2, dictionary2[item.Key], fontStructure);
			}
		}
		return text;
	}

	private string ReplacedText(string text, Glyph[] glyphs, FontStructureHelperBase structure)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		int num = 0;
		for (int i = 0; i < glyphs.Length; i++)
		{
			string toUnicode = glyphs[i].ToUnicode;
			if (toUnicode != null)
			{
				char[] array = toUnicode.ToCharArray();
				if (array.Length != 0 && structure.CharacterMapTable.ContainsKey((int)array[0]) && structure.FontEncoding == string.Empty)
				{
					string text2 = structure.CharacterMapTable[(int)array[0]];
					num += text2.Length;
				}
				else
				{
					num += toUnicode.Length;
				}
			}
		}
		return text.Substring(num, text.Length - num);
	}

	private string ReverseMapZapf(string encodedText, FontStructureHelperBase structure)
	{
		string text = null;
		for (int i = 0; i < encodedText.Length; i++)
		{
			text = encodedText[i] switch
			{
				' ' => text + (char)Convert.ToInt32("20", 16), 
				'✁' => text + (char)Convert.ToInt32("21", 16), 
				'✂' => text + (char)Convert.ToInt32("22", 16), 
				'✃' => text + (char)Convert.ToInt32("23", 16), 
				'✄' => text + (char)Convert.ToInt32("24", 16), 
				'☎' => text + (char)Convert.ToInt32("25", 16), 
				'✆' => text + (char)Convert.ToInt32("26", 16), 
				'✇' => text + (char)Convert.ToInt32("27", 16), 
				'✈' => text + (char)Convert.ToInt32("28", 16), 
				'✉' => text + (char)Convert.ToInt32("29", 16), 
				'☛' => text + (char)Convert.ToInt32("2A", 16), 
				'☞' => text + (char)Convert.ToInt32("2B", 16), 
				'✌' => text + (char)Convert.ToInt32("2C", 16), 
				'✍' => text + (char)Convert.ToInt32("2D", 16), 
				'✎' => text + (char)Convert.ToInt32("2E", 16), 
				'✏' => text + (char)Convert.ToInt32("2F", 16), 
				'✐' => text + (char)Convert.ToInt32("30", 16), 
				'✑' => text + (char)Convert.ToInt32("31", 16), 
				'✒' => text + (char)Convert.ToInt32("32", 16), 
				'✓' => text + (char)Convert.ToInt32("33", 16), 
				'✔' => text + (char)Convert.ToInt32("34", 16), 
				'✕' => text + (char)Convert.ToInt32("35", 16), 
				'✖' => text + (char)Convert.ToInt32("36", 16), 
				'✗' => text + (char)Convert.ToInt32("37", 16), 
				'✘' => text + (char)Convert.ToInt32("38", 16), 
				'✙' => text + (char)Convert.ToInt32("39", 16), 
				'✚' => text + (char)Convert.ToInt32("3A", 16), 
				'✛' => text + (char)Convert.ToInt32("3B", 16), 
				'✜' => text + (char)Convert.ToInt32("3C", 16), 
				'✝' => text + (char)Convert.ToInt32("3D", 16), 
				'✞' => text + (char)Convert.ToInt32("3E", 16), 
				'✟' => text + (char)Convert.ToInt32("3F", 16), 
				'✠' => text + (char)Convert.ToInt32("40", 16), 
				'✡' => text + (char)Convert.ToInt32("41", 16), 
				'✢' => text + (char)Convert.ToInt32("42", 16), 
				'✣' => text + (char)Convert.ToInt32("43", 16), 
				'✤' => text + (char)Convert.ToInt32("44", 16), 
				'✥' => text + (char)Convert.ToInt32("45", 16), 
				'✦' => text + (char)Convert.ToInt32("46", 16), 
				'✧' => text + (char)Convert.ToInt32("47", 16), 
				'★' => text + (char)Convert.ToInt32("48", 16), 
				'✩' => text + (char)Convert.ToInt32("49", 16), 
				'✪' => text + (char)Convert.ToInt32("4A", 16), 
				'✫' => text + (char)Convert.ToInt32("4B", 16), 
				'✬' => text + (char)Convert.ToInt32("4C", 16), 
				'✭' => text + (char)Convert.ToInt32("4D", 16), 
				'✮' => text + (char)Convert.ToInt32("4E", 16), 
				'✯' => text + (char)Convert.ToInt32("4F", 16), 
				'✰' => text + (char)Convert.ToInt32("50", 16), 
				'✱' => text + (char)Convert.ToInt32("51", 16), 
				'✲' => text + (char)Convert.ToInt32("52", 16), 
				'✳' => text + (char)Convert.ToInt32("53", 16), 
				'✴' => text + (char)Convert.ToInt32("54", 16), 
				'✵' => text + (char)Convert.ToInt32("55", 16), 
				'✶' => text + (char)Convert.ToInt32("56", 16), 
				'✷' => text + (char)Convert.ToInt32("57", 16), 
				'✸' => text + (char)Convert.ToInt32("58", 16), 
				'✹' => text + (char)Convert.ToInt32("59", 16), 
				'✺' => text + (char)Convert.ToInt32("5A", 16), 
				'✻' => text + (char)Convert.ToInt32("5B", 16), 
				'✼' => text + (char)Convert.ToInt32("5C", 16), 
				'✽' => text + (char)Convert.ToInt32("5D", 16), 
				'✾' => text + (char)Convert.ToInt32("5E", 16), 
				'✿' => text + (char)Convert.ToInt32("5F", 16), 
				'❀' => text + (char)Convert.ToInt32("60", 16), 
				'❁' => text + (char)Convert.ToInt32("61", 16), 
				'❂' => text + (char)Convert.ToInt32("62", 16), 
				'❃' => text + (char)Convert.ToInt32("63", 16), 
				'❄' => text + (char)Convert.ToInt32("64", 16), 
				'❅' => text + (char)Convert.ToInt32("65", 16), 
				'❆' => text + (char)Convert.ToInt32("66", 16), 
				'❇' => text + (char)Convert.ToInt32("67", 16), 
				'❈' => text + (char)Convert.ToInt32("68", 16), 
				'❉' => text + (char)Convert.ToInt32("69", 16), 
				'❊' => text + (char)Convert.ToInt32("6A", 16), 
				'❋' => text + (char)Convert.ToInt32("6B", 16), 
				'●' => text + (char)Convert.ToInt32("6C", 16), 
				'╍' => text + (char)Convert.ToInt32("6D", 16), 
				'■' => text + (char)Convert.ToInt32("6E", 16), 
				'❏' => text + (char)Convert.ToInt32("6F", 16), 
				'❐' => text + (char)Convert.ToInt32("70", 16), 
				'❑' => text + (char)Convert.ToInt32("71", 16), 
				'❒' => text + (char)Convert.ToInt32("72", 16), 
				'▲' => text + (char)Convert.ToInt32("73", 16), 
				'▼' => text + (char)Convert.ToInt32("74", 16), 
				'⟆' => text + (char)Convert.ToInt32("75", 16), 
				'❖' => text + (char)Convert.ToInt32("76", 16), 
				'◗' => text + (char)Convert.ToInt32("77", 16), 
				'❘' => text + (char)Convert.ToInt32("78", 16), 
				'❙' => text + (char)Convert.ToInt32("79", 16), 
				'❚' => text + (char)Convert.ToInt32("7A", 16), 
				'❛' => text + (char)Convert.ToInt32("7B", 16), 
				'❜' => text + (char)Convert.ToInt32("7C", 16), 
				'❝' => text + (char)Convert.ToInt32("7D", 16), 
				'❞' => text + (char)Convert.ToInt32("7E", 16), 
				'\uf8d7' => text + (char)Convert.ToInt32("80", 16), 
				'\uf8d8' => text + (char)Convert.ToInt32("81", 16), 
				'\uf8d9' => text + (char)Convert.ToInt32("82", 16), 
				'\uf8da' => text + (char)Convert.ToInt32("83", 16), 
				'\uf8db' => text + (char)Convert.ToInt32("84", 16), 
				'\uf8dc' => text + (char)Convert.ToInt32("85", 16), 
				'\uf8dd' => text + (char)Convert.ToInt32("86", 16), 
				'\uf8de' => text + (char)Convert.ToInt32("87", 16), 
				'\uf8df' => text + (char)Convert.ToInt32("88", 16), 
				'\uf8e0' => text + (char)Convert.ToInt32("89", 16), 
				'\uf8e1' => text + (char)Convert.ToInt32("8A", 16), 
				'\uf8e2' => text + (char)Convert.ToInt32("8B", 16), 
				'\uf8e3' => text + (char)Convert.ToInt32("8C", 16), 
				'\uf8e4' => text + (char)Convert.ToInt32("8D", 16), 
				'❡' => text + (char)Convert.ToInt32("A1", 16), 
				'❢' => text + (char)Convert.ToInt32("A2", 16), 
				'❣' => text + (char)Convert.ToInt32("A3", 16), 
				'❤' => text + (char)Convert.ToInt32("A4", 16), 
				'❥' => text + (char)Convert.ToInt32("A5", 16), 
				'❦' => text + (char)Convert.ToInt32("A6", 16), 
				'❧' => text + (char)Convert.ToInt32("A7", 16), 
				'♣' => text + (char)Convert.ToInt32("A8", 16), 
				'♦' => text + (char)Convert.ToInt32("A9", 16), 
				'♥' => text + (char)Convert.ToInt32("AA", 16), 
				'♠' => text + (char)Convert.ToInt32("AB", 16), 
				'①' => text + (char)Convert.ToInt32("AC", 16), 
				'②' => text + (char)Convert.ToInt32("AD", 16), 
				'③' => text + (char)Convert.ToInt32("AE", 16), 
				'④' => text + (char)Convert.ToInt32("AF", 16), 
				'⑤' => text + (char)Convert.ToInt32("B0", 16), 
				'⑥' => text + (char)Convert.ToInt32("B1", 16), 
				'⑦' => text + (char)Convert.ToInt32("B2", 16), 
				'⑧' => text + (char)Convert.ToInt32("B3", 16), 
				'⑨' => text + (char)Convert.ToInt32("B4", 16), 
				'⑩' => text + (char)Convert.ToInt32("B5", 16), 
				'❶' => text + (char)Convert.ToInt32("B6", 16), 
				'❷' => text + (char)Convert.ToInt32("B7", 16), 
				'❸' => text + (char)Convert.ToInt32("B8", 16), 
				'❹' => text + (char)Convert.ToInt32("B9", 16), 
				'❺' => text + (char)Convert.ToInt32("BA", 16), 
				'❻' => text + (char)Convert.ToInt32("BB", 16), 
				'❼' => text + (char)Convert.ToInt32("BC", 16), 
				'❽' => text + (char)Convert.ToInt32("BD", 16), 
				'❾' => text + (char)Convert.ToInt32("BE", 16), 
				'❿' => text + (char)Convert.ToInt32("BF", 16), 
				'➀' => text + (char)Convert.ToInt32("C0", 16), 
				'➁' => text + (char)Convert.ToInt32("C1", 16), 
				'➂' => text + (char)Convert.ToInt32("C2", 16), 
				'➃' => text + (char)Convert.ToInt32("C3", 16), 
				'➄' => text + (char)Convert.ToInt32("C4", 16), 
				'➅' => text + (char)Convert.ToInt32("C5", 16), 
				'➆' => text + (char)Convert.ToInt32("C6", 16), 
				'➇' => text + (char)Convert.ToInt32("C7", 16), 
				'➈' => text + (char)Convert.ToInt32("C8", 16), 
				'➉' => text + (char)Convert.ToInt32("C9", 16), 
				'➊' => text + (char)Convert.ToInt32("CA", 16), 
				'➋' => text + (char)Convert.ToInt32("CB", 16), 
				'➌' => text + (char)Convert.ToInt32("CC", 16), 
				'➍' => text + (char)Convert.ToInt32("CD", 16), 
				'➎' => text + (char)Convert.ToInt32("CE", 16), 
				'➏' => text + (char)Convert.ToInt32("CF", 16), 
				'➐' => text + (char)Convert.ToInt32("D0", 16), 
				'➑' => text + (char)Convert.ToInt32("D1", 16), 
				'➒' => text + (char)Convert.ToInt32("D2", 16), 
				'➓' => text + (char)Convert.ToInt32("D3", 16), 
				'➔' => text + (char)Convert.ToInt32("D4", 16), 
				'→' => text + (char)Convert.ToInt32("D5", 16), 
				'↔' => text + (char)Convert.ToInt32("D6", 16), 
				'↕' => text + (char)Convert.ToInt32("D7", 16), 
				'➘' => text + (char)Convert.ToInt32("D8", 16), 
				'➙' => text + (char)Convert.ToInt32("D9", 16), 
				'➚' => text + (char)Convert.ToInt32("DA", 16), 
				'➛' => text + (char)Convert.ToInt32("DB", 16), 
				'➜' => text + (char)Convert.ToInt32("DC", 16), 
				'➝' => text + (char)Convert.ToInt32("DD", 16), 
				'➞' => text + (char)Convert.ToInt32("DE", 16), 
				'➟' => text + (char)Convert.ToInt32("DF", 16), 
				'➠' => text + (char)Convert.ToInt32("E0", 16), 
				'➡' => text + (char)Convert.ToInt32("E1", 16), 
				'➢' => text + (char)Convert.ToInt32("E2", 16), 
				'➣' => text + (char)Convert.ToInt32("E3", 16), 
				'➤' => text + (char)Convert.ToInt32("E4", 16), 
				'➥' => text + (char)Convert.ToInt32("E5", 16), 
				'➦' => text + (char)Convert.ToInt32("E6", 16), 
				'➧' => text + (char)Convert.ToInt32("E7", 16), 
				'➨' => text + (char)Convert.ToInt32("E8", 16), 
				'➩' => text + (char)Convert.ToInt32("E9", 16), 
				'➪' => text + (char)Convert.ToInt32("EA", 16), 
				'➫' => text + (char)Convert.ToInt32("EB", 16), 
				'➬' => text + (char)Convert.ToInt32("EC", 16), 
				'➭' => text + (char)Convert.ToInt32("ED", 16), 
				'➮' => text + (char)Convert.ToInt32("EE", 16), 
				'➯' => text + (char)Convert.ToInt32("EF", 16), 
				'➱' => text + (char)Convert.ToInt32("F1", 16), 
				'➲' => text + (char)Convert.ToInt32("F2", 16), 
				'➳' => text + (char)Convert.ToInt32("F3", 16), 
				'➴' => text + (char)Convert.ToInt32("F4", 16), 
				'➵' => text + (char)Convert.ToInt32("F5", 16), 
				'➶' => text + (char)Convert.ToInt32("F6", 16), 
				'➷' => text + (char)Convert.ToInt32("F7", 16), 
				'➸' => text + (char)Convert.ToInt32("F8", 16), 
				'➹' => text + (char)Convert.ToInt32("F9", 16), 
				'➺' => text + (char)Convert.ToInt32("FA", 16), 
				'➻' => text + (char)Convert.ToInt32("FB", 16), 
				'➼' => text + (char)Convert.ToInt32("FC", 16), 
				'➽' => text + (char)Convert.ToInt32("FD", 16), 
				'➾' => text + (char)Convert.ToInt32("FE", 16), 
				_ => (!structure.ReverseMapTable.ContainsKey(encodedText)) ? ((char)Convert.ToInt32("28", 16)).ToString() : encodedText, 
			};
		}
		return text;
	}

	private void GetReverseMacEncodeTable()
	{
		m_macReverseEncodeTable = new Dictionary<char, int>();
		m_macReverseEncodeTable.Add(' ', 127);
		m_macReverseEncodeTable.Add('Ä', 128);
		m_macReverseEncodeTable.Add('Å', 129);
		m_macReverseEncodeTable.Add('Ç', 130);
		m_macReverseEncodeTable.Add('É', 131);
		m_macReverseEncodeTable.Add('Ñ', 132);
		m_macReverseEncodeTable.Add('Ö', 133);
		m_macReverseEncodeTable.Add('Ü', 134);
		m_macReverseEncodeTable.Add('á', 135);
		m_macReverseEncodeTable.Add('à', 136);
		m_macReverseEncodeTable.Add('â', 137);
		m_macReverseEncodeTable.Add('ä', 138);
		m_macReverseEncodeTable.Add('ã', 139);
		m_macReverseEncodeTable.Add('å', 140);
		m_macReverseEncodeTable.Add('ç', 141);
		m_macReverseEncodeTable.Add('é', 142);
		m_macReverseEncodeTable.Add('è', 143);
		m_macReverseEncodeTable.Add('ê', 144);
		m_macReverseEncodeTable.Add('ë', 145);
		m_macReverseEncodeTable.Add('í', 146);
		m_macReverseEncodeTable.Add('ì', 147);
		m_macReverseEncodeTable.Add('î', 148);
		m_macReverseEncodeTable.Add('ï', 149);
		m_macReverseEncodeTable.Add('ñ', 150);
		m_macReverseEncodeTable.Add('ó', 151);
		m_macReverseEncodeTable.Add('ò', 152);
		m_macReverseEncodeTable.Add('ô', 153);
		m_macReverseEncodeTable.Add('ö', 154);
		m_macReverseEncodeTable.Add('õ', 155);
		m_macReverseEncodeTable.Add('ú', 156);
		m_macReverseEncodeTable.Add('ù', 157);
		m_macReverseEncodeTable.Add('û', 158);
		m_macReverseEncodeTable.Add('ü', 159);
		m_macReverseEncodeTable.Add('†', 160);
		m_macReverseEncodeTable.Add('°', 161);
		m_macReverseEncodeTable.Add('¢', 162);
		m_macReverseEncodeTable.Add('£', 163);
		m_macReverseEncodeTable.Add('§', 164);
		m_macReverseEncodeTable.Add('•', 165);
		m_macReverseEncodeTable.Add('¶', 166);
		m_macReverseEncodeTable.Add('ß', 167);
		m_macReverseEncodeTable.Add('®', 168);
		m_macReverseEncodeTable.Add('©', 169);
		m_macReverseEncodeTable.Add('™', 170);
		m_macReverseEncodeTable.Add('\u00b4', 171);
		m_macReverseEncodeTable.Add('\u00a8', 172);
		m_macReverseEncodeTable.Add('≠', 173);
		m_macReverseEncodeTable.Add('Æ', 174);
		m_macReverseEncodeTable.Add('Ø', 175);
		m_macReverseEncodeTable.Add('∞', 176);
		m_macReverseEncodeTable.Add('±', 177);
		m_macReverseEncodeTable.Add('≤', 178);
		m_macReverseEncodeTable.Add('≥', 179);
		m_macReverseEncodeTable.Add('¥', 180);
		m_macReverseEncodeTable.Add('µ', 181);
		m_macReverseEncodeTable.Add('∂', 182);
		m_macReverseEncodeTable.Add('∑', 183);
		m_macReverseEncodeTable.Add('∏', 184);
		m_macReverseEncodeTable.Add('π', 185);
		m_macReverseEncodeTable.Add('∫', 186);
		m_macReverseEncodeTable.Add('ª', 187);
		m_macReverseEncodeTable.Add('º', 188);
		m_macReverseEncodeTable.Add('Ω', 189);
		m_macReverseEncodeTable.Add('æ', 190);
		m_macReverseEncodeTable.Add('ø', 191);
		m_macReverseEncodeTable.Add('¿', 192);
		m_macReverseEncodeTable.Add('¡', 193);
		m_macReverseEncodeTable.Add('¬', 194);
		m_macReverseEncodeTable.Add('√', 195);
		m_macReverseEncodeTable.Add('ƒ', 196);
		m_macReverseEncodeTable.Add('≈', 197);
		m_macReverseEncodeTable.Add('∆', 198);
		m_macReverseEncodeTable.Add('«', 199);
		m_macReverseEncodeTable.Add('»', 200);
		m_macReverseEncodeTable.Add('…', 201);
		m_macReverseEncodeTable.Add('À', 203);
		m_macReverseEncodeTable.Add('Ã', 204);
		m_macReverseEncodeTable.Add('Õ', 205);
		m_macReverseEncodeTable.Add('Œ', 206);
		m_macReverseEncodeTable.Add('œ', 207);
		m_macReverseEncodeTable.Add('–', 208);
		m_macReverseEncodeTable.Add('—', 209);
		m_macReverseEncodeTable.Add('“', 210);
		m_macReverseEncodeTable.Add('”', 211);
		m_macReverseEncodeTable.Add('‘', 212);
		m_macReverseEncodeTable.Add('’', 213);
		m_macReverseEncodeTable.Add('÷', 214);
		m_macReverseEncodeTable.Add('◊', 215);
		m_macReverseEncodeTable.Add('ÿ', 216);
		m_macReverseEncodeTable.Add('Ÿ', 217);
		m_macReverseEncodeTable.Add('⁄', 218);
		m_macReverseEncodeTable.Add('€', 219);
		m_macReverseEncodeTable.Add('‹', 220);
		m_macReverseEncodeTable.Add('›', 221);
		m_macReverseEncodeTable.Add('ﬁ', 222);
		m_macReverseEncodeTable.Add('ﬂ', 223);
		m_macReverseEncodeTable.Add('‡', 224);
		m_macReverseEncodeTable.Add('·', 225);
		m_macReverseEncodeTable.Add(',', 226);
		m_macReverseEncodeTable.Add('„', 227);
		m_macReverseEncodeTable.Add('‰', 228);
		m_macReverseEncodeTable.Add('Â', 229);
		m_macReverseEncodeTable.Add('Ê', 230);
		m_macReverseEncodeTable.Add('Á', 231);
		m_macReverseEncodeTable.Add('Ë', 232);
		m_macReverseEncodeTable.Add('È', 233);
		m_macReverseEncodeTable.Add('Í', 234);
		m_macReverseEncodeTable.Add('Î', 235);
		m_macReverseEncodeTable.Add('Ï', 236);
		m_macReverseEncodeTable.Add('Ì', 237);
		m_macReverseEncodeTable.Add('Ó', 238);
		m_macReverseEncodeTable.Add('Ô', 239);
		m_macReverseEncodeTable.Add('\uf8ff', 240);
		m_macReverseEncodeTable.Add('Ò', 241);
		m_macReverseEncodeTable.Add('Ú', 242);
		m_macReverseEncodeTable.Add('Û', 243);
		m_macReverseEncodeTable.Add('Ù', 244);
		m_macReverseEncodeTable.Add('ı', 245);
		m_macReverseEncodeTable.Add('ˆ', 246);
		m_macReverseEncodeTable.Add('\u02dc', 247);
		m_macReverseEncodeTable.Add('\u00af', 248);
		m_macReverseEncodeTable.Add('\u02d8', 249);
		m_macReverseEncodeTable.Add('\u02d9', 250);
		m_macReverseEncodeTable.Add('\u02da', 251);
		m_macReverseEncodeTable.Add('\u00b8', 252);
		m_macReverseEncodeTable.Add('\u02dd', 253);
		m_macReverseEncodeTable.Add('\u02db', 254);
		m_macReverseEncodeTable.Add('ˇ', 255);
	}

	private double GetKey(Dictionary<double, string> charMapTable, string val)
	{
		double num = -1.0;
		foreach (KeyValuePair<double, string> item in charMapTable)
		{
			if (val == item.Value)
			{
				num = item.Key;
				break;
			}
		}
		if (num == -1.0)
		{
			return (int)val[0];
		}
		return num;
	}

	private double GetKey(Dictionary<double, string> charMapTable)
	{
		string text = this.text.Substring(1, this.text.Length - 2);
		string[] array = charMapTable.Values.ToArray();
		double result = -1.0;
		for (int i = 0; i < array.Length; i++)
		{
			if (text == array[i])
			{
				result = charMapTable.Keys.ToArray()[i];
			}
		}
		return result;
	}

	private PdfString GetUnicodeString(string token)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		return new PdfString(token)
		{
			Converted = true,
			Encode = PdfString.ForceEncoding.ASCII
		};
	}

	private float GetReplacedChar(Glyph[] glyphs, FontStructureHelperBase structure)
	{
		float num = 0f;
		if (structure.FontGlyphWidths != null && structure.FontGlyphWidths.Count == 0)
		{
			if (Enum.IsDefined(typeof(PdfFontFamily), structure.FontName))
			{
				PdfFontMetrics metrics = PdfStandardFontMetricsFactory.GetMetrics(GetFamily(structure.FontName), (PdfFontStyle)(structure as FontStructureHelperNet).FontStyle, structure.FontSize);
				Glyph[] array = glyphs;
				foreach (Glyph glyph in array)
				{
					int num2 = ((!(structure.FontName == "ZapfDingbats") || structure.IsEmbedded) ? (glyph.CharId.IntValue - 32) : (ReverseMapZapf(((char)glyph.CharId.IntValue).ToString(), structure)[0] - 32));
					num2 = ((num2 >= 0 && num2 != 128) ? num2 : 0);
					if (num2 < metrics.WidthTable.ToArray().Count)
					{
						float num3 = metrics.WidthTable[num2];
						float num4 = (float)(glyph.Width * glyph.FontSize);
						num += (float)((double)num3 + (double)(num3 / num4) * glyph.CharSpacing + (double)(num3 / num4) * glyph.WordSpacing);
					}
				}
			}
			else if (structure.FontDictionary != null && structure.FontDictionary.ContainsKey("BaseFont"))
			{
				string text = (structure.FontDictionary["BaseFont"] as PdfName).Value;
				if (text.Contains("-"))
				{
					text = text.Replace("-", "");
				}
				if (text.Contains(","))
				{
					text = text.Replace(",", "");
				}
				if (Enum.IsDefined(typeof(PdfFontFamily), text))
				{
					PdfFontMetrics metrics2 = PdfStandardFontMetricsFactory.GetMetrics(GetFamily(text), (PdfFontStyle)(structure as FontStructureHelperNet).FontStyle, structure.FontSize);
					Glyph[] array = glyphs;
					foreach (Glyph glyph2 in array)
					{
						int num5 = glyph2.CharId.IntValue - 32;
						num5 = ((num5 >= 0 && num5 != 128) ? num5 : 0);
						if (num5 < metrics2.WidthTable.ToArray().Count)
						{
							float num6 = metrics2.WidthTable[num5];
							float num7 = (float)(glyph2.Width * glyph2.FontSize);
							num += (float)((double)num6 + (double)(num6 / num7) * glyph2.CharSpacing + (double)(num6 / num7) * glyph2.WordSpacing);
						}
					}
				}
			}
		}
		else if (structure.FontGlyphWidths != null && structure.FontGlyphWidths.Count > 0)
		{
			Glyph[] array = glyphs;
			foreach (Glyph glyph3 in array)
			{
				if (structure.FontEncoding == "Identity-H" || (structure.FontEncoding == "" && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0))
				{
					double num8 = -1.0;
					if (structure.CharacterMapTable.ContainsValue(this.text.Substring(1, this.text.Length - 2)))
					{
						num8 = GetKey(structure.CharacterMapTable);
					}
					if (num8 == -1.0)
					{
						num8 = ((glyph3.ToUnicode == null || glyph3.ToUnicode.Length <= 0 || glyph3.ToUnicode[0] == '\0' || glyph3.CharId.IntValue != 0) ? GetKey(structure.CharacterMapTable, ((char)glyph3.CharId.IntValue).ToString()) : GetKey(structure.CharacterMapTable, glyph3.ToUnicode[0].ToString()));
					}
					structure.FontGlyphWidths.TryGetValue((int)num8, out var value);
					float num9 = (float)(glyph3.Width * glyph3.FontSize);
					num += (float)((double)value + (double)((float)value / num9) * glyph3.CharSpacing + (double)((float)value / num9) * glyph3.WordSpacing);
					if (float.IsNaN(num))
					{
						num = 0f;
					}
					continue;
				}
				int num10 = 0;
				_ = glyph3.CharId;
				if (glyph3.CharId.BytesCount != 0)
				{
					num10 = glyph3.CharId.IntValue;
				}
				else
				{
					_ = glyph3.CharId;
					if (glyph3.CharId.Bytes != null || (glyph3.ToUnicode != null && glyph3.ToUnicode.Length > 0 && structure.FontEncoding == "WinAnsiEncoding"))
					{
						num10 = glyph3.ToUnicode[0];
					}
				}
				if (structure.FontGlyphWidths.ContainsKey(num10))
				{
					float num11 = structure.FontGlyphWidths[num10];
					float num12 = (float)(glyph3.Width * glyph3.FontSize);
					num += (float)((double)num11 + (double)(num11 / num12) * glyph3.CharSpacing + (double)(num11 / num12) * glyph3.WordSpacing);
				}
				else
				{
					if (!(structure.FontEncoding == "Encoding"))
					{
						continue;
					}
					int itFormDifference = GetItFormDifference(structure.DifferencesDictionary, (char)num10);
					if (structure.FontGlyphWidths.ContainsKey(itFormDifference))
					{
						float num13 = structure.FontGlyphWidths[itFormDifference];
						float num14 = (float)(glyph3.Width * glyph3.FontSize);
						num += (float)((double)num13 + (double)(num13 / num14) * glyph3.CharSpacing + (double)(num13 / num14) * glyph3.WordSpacing);
						if (float.IsNaN(num))
						{
							num = 0f;
						}
					}
				}
			}
		}
		return num;
	}

	private int GetItFormDifference(Dictionary<string, string> diffTable, char val)
	{
		int result = 0;
		foreach (KeyValuePair<string, string> item in diffTable)
		{
			if (item.Value == val.ToString())
			{
				int.TryParse(item.Key, out result);
				break;
			}
		}
		return result;
	}

	private PdfFontFamily GetFamily(string name)
	{
		return name switch
		{
			"Helvetica" => PdfFontFamily.Helvetica, 
			"Courier" => PdfFontFamily.Courier, 
			"TimesRoman" => PdfFontFamily.TimesRoman, 
			"Symbol" => PdfFontFamily.Symbol, 
			"ZapfDingbats" => PdfFontFamily.ZapfDingbats, 
			_ => PdfFontFamily.Helvetica, 
		};
	}
}

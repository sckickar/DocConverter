using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Exporting;

internal class PdfTextExtractor
{
	private static int m_numberOfChars = 50;

	private static Dictionary<string, List<string>> m_differenceArray;

	private static List<string> m_decodedChar;

	private PdfTextExtractor()
	{
		throw new NotImplementedException();
	}

	public static string ExtractTextFromBytes(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return " ";
		}
		try
		{
			string text = string.Empty;
			bool flag = false;
			string text2 = null;
			string text3 = null;
			string text4 = null;
			string text5 = null;
			bool flag2 = false;
			bool flag3 = false;
			int num = 0;
			string text6 = "";
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			string text7 = string.Empty;
			string empty = string.Empty;
			string text8 = string.Empty;
			string empty2 = string.Empty;
			char[] array = new char[m_numberOfChars];
			int num2 = 0;
			for (int i = 0; i < m_numberOfChars; i++)
			{
				array[i] = ' ';
			}
			float num3 = 0f;
			bool flag7 = true;
			bool flag8 = false;
			for (int j = 0; j < data.Length; j++)
			{
				char c = (char)data[j];
				if (m_differenceArray.Count > 0 && CheckToken(new string[1] { "Tf" }, array))
				{
					int num4 = 0;
					int num5 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] == '/')
						{
							num4 = k;
						}
						else if (k > 0 && array[k] == 'f' && array[k - 1] == 'T')
						{
							num5 = k;
						}
					}
					string text9 = new string(array).Substring(num4 + 1, num5 - 1 - num4);
					string text10 = text9[..text9.IndexOf(' ')];
					if (m_differenceArray.ContainsKey(text10))
					{
						foreach (KeyValuePair<string, List<string>> item in m_differenceArray)
						{
							if (item.Key.Equals(text10))
							{
								m_decodedChar = new List<string>();
								m_decodedChar = item.Value;
								flag6 = true;
							}
						}
					}
					else
					{
						flag6 = false;
					}
				}
				if (flag)
				{
					if (c == '[' && data[j + 1] == 40 && data[j - 1] == 10)
					{
						flag8 = true;
					}
					if (c == ']' && data[j - 1] == 41 && data[j + 1] == 32 && data[j + 2] == 84 && data[j + 2] == 74)
					{
						flag8 = false;
					}
					if (c == 'T' && data[j + 1] == 42 && (data[j + 2] == 91 || data[j + 2] == 40))
					{
						text += Environment.NewLine;
					}
					if (data[j] == 39 && data[j - 1] == 41)
					{
						if (text.Length > 0 && text[text.Length - 1] != '\n')
						{
							text += Environment.NewLine;
						}
					}
					else if (data[j] == 84 && data[j + 1] == 68 && data[j - 1] == 32)
					{
						string text11 = string.Empty;
						string text12 = string.Empty;
						int num6 = array.Length - 2;
						while (num6 >= 0)
						{
							char c2 = array[num6];
							if (c2 != ' ')
							{
								text12 += c2;
								num6--;
								continue;
							}
							for (int num7 = text12.Length - 1; num7 >= 0; num7--)
							{
								text11 += text12[num7];
							}
							break;
						}
						if (text11 != string.Empty)
						{
							if (flag7)
							{
								num3 = Convert.ToSingle(text11);
								flag7 = false;
							}
							else
							{
								float num8 = Convert.ToSingle(text11);
								if (num3 - num8 != 0f && num8 != 0f)
								{
									text += Environment.NewLine;
								}
								num3 = num8;
							}
						}
					}
					else if (data[j] == 84 && data[j + 1] == 100 && data[j - 1] == 32)
					{
						string text13 = string.Empty;
						string text14 = string.Empty;
						int num9 = array.Length - 2;
						while (num9 >= 0)
						{
							char c3 = array[num9];
							if (c3 != ' ')
							{
								text14 += c3;
								num9--;
								continue;
							}
							for (int num10 = text14.Length - 1; num10 >= 0; num10--)
							{
								text13 += text14[num10];
							}
							break;
						}
						if (flag7)
						{
							num3 = Convert.ToSingle(text13);
							flag7 = false;
						}
						else
						{
							float num11 = Convert.ToSingle(text13);
							if (num3 - num11 != 0f && num11 != 0f)
							{
								text += Environment.NewLine;
							}
							num3 = num11;
						}
					}
					else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
					{
						text += Environment.NewLine;
					}
					else if (data[j] == 84 && data[j + 1] == 109 && data[j - 1] == 32 && (data[j + 2] == 10 || data[j + 2] == 13))
					{
						string text15 = string.Empty;
						_ = string.Empty;
						int num12 = j - 2;
						while (data[num12] != 32)
						{
							char c4 = (char)data[num12];
							text15 = c4 + text15;
							num12--;
						}
						empty = text15;
						if (text7 == "")
						{
							text7 = "0";
						}
						if (empty2 != string.Empty && (Convert.ToSingle(empty) < Convert.ToSingle(text7) || text7 == "0"))
						{
							text += Environment.NewLine;
						}
						if (empty2 != text8)
						{
							text += " ";
						}
						text8 = empty2;
						text7 = empty;
					}
					if (num == 0)
					{
						if (data[j] == 84 && data[j + 1] == 109 && data[j + 2] == 10 && text.Length > 0 && text[text.Length - 1] == ' ')
						{
							text = text.Remove(text.Length - 1, 1);
						}
						if (!CheckToken(new string[2] { "TD", "Td" }, array))
						{
							if (data[j - 1] == 10)
							{
								if (!text.EndsWith(Environment.NewLine))
								{
									if (text2 == null)
									{
										text2 = " ";
									}
									text3 = text;
									if (text2 == " ")
									{
										text2 = text3;
									}
									if (text2.Length != text3.Length)
									{
										try
										{
											text4 = text3.Substring(text2.Length, text3.Length - text2.Length);
											text4 = text4.Trim('\r');
											text4 = text4.Trim('\n');
											text4 = text4.Trim(' ');
											if (text4 == text5)
											{
												if (text4.Length > 0)
												{
													text = text.Substring(0, text.Length - text4.Length - 1);
												}
												if (text.Length > 0 && text[text.Length - 1] != '\n')
												{
													text += Environment.NewLine;
												}
												text3 = text;
											}
											text5 = text4;
										}
										catch (Exception)
										{
										}
									}
									text2 = text3;
								}
							}
							else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
							{
								text += Environment.NewLine;
							}
							else if (CheckToken(new string[1] { "Tj" }, array))
							{
								text += string.Empty;
							}
						}
					}
					if (num == 0 && CheckToken(new string[1] { "ET" }, array))
					{
						flag = false;
						if (!flag8)
						{
							text += " ";
						}
					}
					else if (c == '<' && num == 0 && !flag3)
					{
						flag4 = false;
						bool flag9 = true;
						if (!char.IsDigit((char)data[j + 1]))
						{
							flag9 = false;
						}
						for (int l = j; l < data.Length - 1; l++)
						{
							if (!(data[l] == 62 && flag9))
							{
								continue;
							}
							byte[] array2 = new byte[l - j - 1];
							int num13 = 0;
							for (int m = j + 1; m < l; m++)
							{
								array2[num13] = data[m];
								num13++;
							}
							UTF8Encoding uTF8Encoding = new UTF8Encoding();
							num2 = 0;
							byte[] array3 = new byte[4];
							for (int n = 0; n < l - j - 1; n++)
							{
								array3[num2] = array2[n];
								if (num2 == 3)
								{
									long num14 = long.Parse(uTF8Encoding.GetString(array3, 0, array3.Length), NumberStyles.HexNumber);
									if (num14 < 5000)
									{
										text += (char)num14;
										num2 = 0;
									}
								}
								else
								{
									num2++;
								}
							}
							char[] array4 = new char[3];
							Array.Copy(data, l + 1, array4, 0, 3);
							string text16 = new string(array4);
							if (text16.IndexOf("Tj") != -1 || text16.IndexOf("TJ") != -1)
							{
								flag4 = true;
								num = 1;
							}
							break;
						}
					}
					else if (c == '(' && num == 0 && !flag3)
					{
						num = 1;
						for (int num15 = j; data[num15] != 41; num15++)
						{
							if ((data[j + 1] < 32 || data[j + 1] > 126) && (data[j + 1] < 128 || data[j + 1] >= byte.MaxValue))
							{
								flag2 = true;
							}
						}
					}
					else if (c == ')' && num == 1 && !flag3)
					{
						num = 0;
						if (flag2 = true)
						{
							flag2 = false;
						}
						if (!flag8)
						{
							text += string.Empty;
						}
					}
					else if (c == '>' && num == 1 && !flag3 && flag4)
					{
						num = 0;
						flag4 = false;
					}
					else if (num == 1)
					{
						if (c == '\\' && !flag3)
						{
							flag3 = true;
						}
						else
						{
							if ((c >= ' ' && c <= '~') || (c >= '\u0080' && c < 'ÿ'))
							{
								if (flag4)
								{
									if (flag5)
									{
										text6 += c;
										char c5 = Convert.ToChar(Convert.ToUInt64(text6.ToString(), 16));
										if (c5 != 0)
										{
											text += c5;
										}
										flag5 = false;
										text6 = string.Empty;
									}
									else
									{
										text6 += c;
										flag5 = true;
									}
								}
								else if (!flag6)
								{
									text = ((!flag2) ? (text + c) : (text + (char)(c + 29)));
								}
								else if (c > m_decodedChar.Count)
								{
									if (c == 'n' && data[j - 1] == 92)
									{
										int index = 10;
										text += m_decodedChar[index];
									}
									else if (c == 'r' && data[j - 1] == 92)
									{
										int index2 = 13;
										text += m_decodedChar[index2];
									}
									else
									{
										text += c;
									}
								}
								else
								{
									text += m_decodedChar[c];
								}
							}
							else if (!flag6)
							{
								text = ((!flag2) ? (text + c) : (text + (char)(c + 29)));
							}
							else if (c > m_decodedChar.Count)
							{
								if (c == 'n' && data[j - 1] == 92)
								{
									int index3 = 10;
									text += m_decodedChar[index3];
								}
								else if (c == 'r' && data[j - 1] == 92)
								{
									int index4 = 13;
									text += m_decodedChar[index4];
								}
								else
								{
									text += c;
								}
							}
							else
							{
								text += m_decodedChar[c];
							}
							flag3 = false;
						}
					}
				}
				for (int num16 = 0; num16 < m_numberOfChars - 1; num16++)
				{
					array[num16] = array[num16 + 1];
				}
				array[m_numberOfChars - 1] = c;
				if (!flag && CheckToken(new string[1] { "BT" }, array))
				{
					flag = true;
				}
			}
			return text;
		}
		catch
		{
			return " ";
		}
	}

	internal static string ExtractTextFromBytes(byte[] data, bool type)
	{
		if (data == null || data.Length == 0)
		{
			return " ";
		}
		try
		{
			string text = string.Empty;
			bool flag = false;
			string text2 = null;
			string text3 = null;
			string text4 = null;
			string text5 = null;
			bool flag2 = false;
			int num = 0;
			char[] array = new char[m_numberOfChars];
			for (int i = 0; i < m_numberOfChars; i++)
			{
				array[i] = ' ';
			}
			bool flag3 = false;
			string text6 = string.Empty;
			float num2 = 0f;
			bool flag4 = true;
			for (int j = 0; j < data.Length; j++)
			{
				char c = (char)data[j];
				if (flag)
				{
					if (data[j] == 39 && data[j - 1] == 41)
					{
						if (text.Length > 0 && text[text.Length - 1] != '\n')
						{
							text += Environment.NewLine;
						}
					}
					else if (data[j] == 84 && data[j + 1] == 68 && data[j - 1] == 32)
					{
						string text7 = string.Empty;
						string text8 = string.Empty;
						int num3 = array.Length - 2;
						while (num3 >= 0)
						{
							char c2 = array[num3];
							if (c2 != ' ')
							{
								text8 += c2;
								num3--;
								continue;
							}
							for (int num4 = text8.Length - 1; num4 >= 0; num4--)
							{
								text7 += text8[num4];
							}
							break;
						}
						if (flag4)
						{
							num2 = Convert.ToSingle(text7);
							flag4 = false;
						}
						else
						{
							float num5 = Convert.ToSingle(text7);
							if (num2 - num5 != 0f && num5 != 0f)
							{
								text += Environment.NewLine;
							}
							num2 = num5;
						}
					}
					else if (data[j] == 84 && data[j + 1] == 100 && data[j - 1] == 32)
					{
						string text9 = string.Empty;
						string text10 = string.Empty;
						int num6 = array.Length - 2;
						while (num6 >= 0)
						{
							char c3 = array[num6];
							if (c3 != ' ')
							{
								text10 += c3;
								num6--;
								continue;
							}
							for (int num7 = text10.Length - 1; num7 >= 0; num7--)
							{
								text9 += text10[num7];
							}
							break;
						}
						if (flag4)
						{
							num2 = Convert.ToSingle(text9);
							flag4 = false;
						}
						else
						{
							float num8 = Convert.ToSingle(text9);
							if (num2 - num8 != 0f && num8 != 0f)
							{
								text += Environment.NewLine;
							}
							num2 = num8;
						}
					}
					else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
					{
						text += Environment.NewLine;
					}
					else if (data[j] == 84 && data[j + 1] == 109 && data[j - 1] == 32 && data[j + 2] == 10)
					{
						text += Environment.NewLine;
					}
					if (num == 0)
					{
						if (data[j] == 84 && data[j + 1] == 109 && data[j + 2] == 10 && text.Length > 0 && text[text.Length - 1] == ' ')
						{
							text = text.Remove(text.Length - 1, 1);
						}
						if (!CheckToken(new string[2] { "TD", "Td" }, array))
						{
							if (data[j - 1] == 10)
							{
								if (!text.EndsWith(Environment.NewLine))
								{
									if (text2 == null)
									{
										text2 = " ";
									}
									text3 = text;
									if (text2 == " ")
									{
										text2 = text3;
									}
									if (text2.Length != text3.Length)
									{
										try
										{
											text4 = text3.Substring(text2.Length, text3.Length - text2.Length);
											text4 = text4.Trim('\r');
											text4 = text4.Trim('\n');
											text4 = text4.Trim(' ');
											if (text4 == text5)
											{
												if (text4.Length > 0)
												{
													text = text.Substring(0, text.Length - text4.Length - 1);
												}
												if (text.Length > 0 && text[text.Length - 1] != '\n')
												{
													text += Environment.NewLine;
												}
												text3 = text;
											}
											text5 = text4;
										}
										catch (Exception ex)
										{
											throw ex;
										}
									}
									text2 = text3;
								}
							}
							else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
							{
								text += Environment.NewLine;
							}
							else if (CheckToken(new string[1] { "Tj" }, array))
							{
								text += string.Empty;
							}
						}
					}
					if (num == 0 && CheckToken(new string[1] { "ET" }, array))
					{
						flag = false;
						text += " ";
					}
					else if (c == '<' && num == 0 && !flag2)
					{
						num = 1;
					}
					else if (c == '>' && num == 1 && !flag2)
					{
						num = 0;
						text += " ";
					}
					else if (num == 1)
					{
						if (c == '\\' && !flag2)
						{
							flag2 = true;
						}
						else
						{
							if ((c >= ' ' && c <= '~') || (c >= '\u0080' && c < 'ÿ'))
							{
								if (flag3)
								{
									text6 += c;
									text += Convert.ToChar(Convert.ToUInt64(text6.ToString(), 16));
									flag3 = false;
									text6 = string.Empty;
								}
								else
								{
									text6 += c;
									flag3 = true;
								}
							}
							flag2 = false;
						}
					}
				}
				for (int k = 0; k < m_numberOfChars - 1; k++)
				{
					array[k] = array[k + 1];
				}
				array[m_numberOfChars - 1] = c;
				if (!flag && CheckToken(new string[1] { "BT" }, array))
				{
					flag = true;
				}
			}
			return text;
		}
		catch
		{
			return " ";
		}
	}

	public static string ExtractTextFromBytes(byte[] data, PdfPageBase lpage, List<PdfName> fontname, List<IPdfPrimitive> fontref)
	{
		if (fontname != null)
		{
			string result = null;
			PdfCrossTable pdfCrossTable = new PdfCrossTable();
			if (lpage is PdfLoadedPage)
			{
				pdfCrossTable = (lpage as PdfLoadedPage).Document.CrossTable;
			}
			else if (lpage != null)
			{
				pdfCrossTable = (lpage as PdfPage).Document.CrossTable;
			}
			m_differenceArray = new Dictionary<string, List<string>>();
			for (int i = 0; i < fontname.Count; i++)
			{
				if (!(fontref[i] is PdfReferenceHolder))
				{
					continue;
				}
				PdfReferenceHolder pointer = fontref[i] as PdfReferenceHolder;
				PdfDictionary pdfDictionary = pdfCrossTable.GetObject(pointer) as PdfDictionary;
				List<string> list = new List<string>();
				if (!(pdfDictionary["Subtype"].ToString() != "/Type3") || !pdfDictionary.ContainsKey("Encoding"))
				{
					continue;
				}
				PdfReferenceHolder pointer2 = pdfDictionary["Encoding"] as PdfReferenceHolder;
				if (!(pdfCrossTable.GetObject(pointer2) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("Differences"))
				{
					continue;
				}
				_ = pdfDictionary2.Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary2.Items)
				{
					if (item2.Key.Value.Equals("Differences"))
					{
						PdfArray pdfArray = pdfCrossTable.GetObject(item2.Value) as PdfArray;
						int count = pdfArray.Count;
						for (int j = 0; j < count; j++)
						{
							string item = ((!(pdfArray[j] is PdfNumber)) ? GetLatinCharacter(pdfArray[j].ToString().Trim('/')) : (pdfArray[j] as PdfNumber).IntValue.ToString());
							list.Add(item);
						}
					}
				}
				m_differenceArray.Add(fontname[i].ToString().Trim('/'), list);
			}
			for (int k = 0; k < fontname.Count; k++)
			{
				if (!(fontref[k] is PdfReferenceHolder))
				{
					continue;
				}
				PdfReferenceHolder pointer3 = fontref[k] as PdfReferenceHolder;
				PdfDictionary pdfDictionary3 = pdfCrossTable.GetObject(pointer3) as PdfDictionary;
				string text = string.Empty;
				if (pdfDictionary3.ContainsKey("BaseFont"))
				{
					text = pdfDictionary3["BaseFont"].ToString();
				}
				else if (pdfDictionary3.ContainsKey("Name"))
				{
					text = pdfDictionary3["Name"].ToString();
				}
				if (pdfDictionary3["Subtype"].ToString() == "/Type0")
				{
					if (pdfDictionary3.ContainsKey("ToUnicode") && !(pdfDictionary3["ToUnicode"].ToString() == "/Identity-H"))
					{
						return ExtractTextFromBytesEmbedFonts(data, lpage, fontname, fontref);
					}
					return ExtractTextFrom_Type0Fonts(data, fontref, fontname, pdfCrossTable);
				}
				if (!pdfDictionary3.ContainsKey("ToUnicode") || text.Equals("/Times−Roman") || text.Equals("/Times-Bold") || text.Equals("/Times-Italic") || text.Equals("/Times−BoldItalic") || text.Equals("/Helvetica") || text.Equals("/Helvetica−Bold") || text.Equals("/Helvetica−Oblique") || text.Equals("/Helvetica−BoldOblique") || text.Equals("/Courier") || text.Equals("/Courier−Bold") || text.Equals("/Courier−Oblique") || text.Equals("/Courier−BoldOblique") || text.Equals("/Symbol") || text.Equals("/ZapfDingbats"))
				{
					return ExtractTextFromBytes(data);
				}
				if (pdfDictionary3.ContainsKey("Encoding"))
				{
					if (pdfDictionary3["Encoding"] as PdfReferenceHolder == null)
					{
						if (pdfDictionary3["Encoding"].ToString() == "/WinAnsiEncoding")
						{
							return ExtractTextFromBytes(data);
						}
						return ExtractTextFromBytesTrueTypeFonts(data, fontref, fontname, pdfCrossTable);
					}
					return ExtractTextFromBytesEmbedFonts(data, lpage, fontname, fontref);
				}
				if (pdfDictionary3.ContainsKey("ToUnicode"))
				{
					return ExtractTextFromBytesEmbedFonts(data, lpage, fontname, fontref);
				}
			}
			return result;
		}
		return null;
	}

	internal static string ExtractTextFromBytesTrueTypeFonts(byte[] data, List<IPdfPrimitive> fontref, List<PdfName> fontname, PdfCrossTable crosstable)
	{
		string text = string.Empty;
		if (data == null || data.Length == 0)
		{
			return " ";
		}
		try
		{
			string text2 = string.Empty;
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			string text3 = "";
			Encoding.GetEncoding("Default").GetString(data, 0, data.Length);
			char[] array = new char[m_numberOfChars];
			for (int i = 0; i < m_numberOfChars; i++)
			{
				array[i] = ' ';
			}
			for (int j = 0; j < data.Length; j++)
			{
				char c = (char)data[j];
				if (CheckToken(new string[1] { "Tf" }, array))
				{
					int num2 = 0;
					int num3 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] == '/')
						{
							num2 = k;
						}
						else if (array[k] == 'f' && array[k - 1] == 'T')
						{
							num3 = k;
							break;
						}
					}
					string text4 = new string(array).Substring(num2 + 1, num3 - 1 - num2);
					string text5 = text4[..text4.IndexOf(' ')];
					if (fontname.Contains((PdfName)text5))
					{
						PdfReferenceHolder pointer = fontref[fontname.IndexOf((PdfName)text5)] as PdfReferenceHolder;
						PdfDictionary pdfDictionary = crosstable.GetObject(pointer) as PdfDictionary;
						pdfDictionary["BaseFont"].ToString();
						if (pdfDictionary["Subtype"].ToString() == "/Type0")
						{
							text = "Type0";
						}
						else if (pdfDictionary["Subtype"].ToString() == "/TrueType")
						{
							text = "TrueType";
						}
						else if (pdfDictionary["Subtype"].ToString() == "/Type1")
						{
							text = "Type1";
						}
					}
				}
				if (flag)
				{
					if (num == 0)
					{
						if (CheckToken(new string[2] { "TD", "Td" }, array))
						{
							string text6 = new string(array);
							int num4 = text6.IndexOf(' ');
							int num5 = text6.LastIndexOf(' ');
							string text7 = text6.Substring(num4, num5 - num4);
							if (text7 != text3)
							{
								text2 += Environment.NewLine;
							}
							text3 = text7;
						}
						else if (data[j - 1] != 10)
						{
							if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
							{
								text2 += Environment.NewLine;
							}
							else if (CheckToken(new string[1] { "Tj" }, array))
							{
								text2 += string.Empty;
							}
						}
					}
					if (num == 0 && CheckToken(new string[1] { "ET" }, array))
					{
						flag = false;
						text2 += " ";
					}
					else if (c == '(' && num == 0 && !flag2)
					{
						num = 1;
					}
					else if (c == ')' && num == 1 && !flag2)
					{
						num = 0;
					}
					else if (num == 1)
					{
						if (c == '\\' && !flag2)
						{
							flag2 = true;
						}
						else
						{
							if (text == "Type1")
							{
								text2 += c;
							}
							else if ((c >= '\u0003' && c <= 'a') || (c >= 'c' && c < 'â'))
							{
								c = (char)(c + 29);
								text2 += c;
							}
							else if (array[m_numberOfChars - 1] == '\0')
							{
								text2 += " ";
							}
							flag2 = false;
						}
					}
				}
				for (int l = 0; l < m_numberOfChars - 1; l++)
				{
					array[l] = array[l + 1];
				}
				array[m_numberOfChars - 1] = c;
				if (!flag && CheckToken(new string[1] { "BT" }, array))
				{
					flag = true;
				}
			}
			return text2;
		}
		catch
		{
			return " ";
		}
	}

	internal static string ExtractTextFromBytesEmbedFonts(byte[] data, PdfPageBase lpage, List<PdfName> m_font, List<IPdfPrimitive> m_fref)
	{
		if (m_font != null)
		{
			bool flag = false;
			List<PdfArray> list = new List<PdfArray>();
			List<List<string>> list2 = new List<List<string>>();
			List<Dictionary<double, double>> list3 = new List<Dictionary<double, double>>();
			PdfCrossTable pdfCrossTable = null;
			if (lpage is PdfLoadedPage)
			{
				pdfCrossTable = (lpage as PdfLoadedPage).Document.CrossTable;
			}
			else if (lpage != null)
			{
				pdfCrossTable = (lpage as PdfPage).Document.CrossTable;
			}
			for (int i = 0; i < m_font.Count; i++)
			{
				PdfReferenceHolder pointer = m_fref[i] as PdfReferenceHolder;
				PdfDictionary obj = pdfCrossTable.GetObject(pointer) as PdfDictionary;
				PdfReferenceHolder pdfReferenceHolder = obj["Encoding"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					PdfArray item = (pdfCrossTable.GetObject(pdfReferenceHolder) as PdfDictionary)["Differences"] as PdfArray;
					list.Add(item);
				}
				PdfReferenceHolder pdfReferenceHolder2 = obj["ToUnicode"] as PdfReferenceHolder;
				if (!(pdfReferenceHolder2 != null))
				{
					continue;
				}
				byte[] decompressedData = (pdfCrossTable.GetObject(pdfReferenceHolder2) as PdfStream).GetDecompressedData();
				string @string = Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length);
				int num = @string.IndexOf("beginbfchar");
				int num2 = @string.IndexOf("endbfchar");
				if (num < 0 && num2 < 0)
				{
					num = @string.IndexOf("begincmap");
					num2 = @string.IndexOf("endcmap");
				}
				int num3 = @string.IndexOf("beginbfrange");
				int num4 = @string.IndexOf("endbfrange");
				if (num3 < 0 && num4 < 0)
				{
					num3 = @string.IndexOf("begincidrange");
					num4 = @string.IndexOf("endcidrange");
				}
				if (num3 > 0)
				{
					num = num3;
					num2 = num4;
				}
				string text = @string.Substring(num + 11, num2 - num - 11);
				List<string> list4 = new List<string>();
				string text2 = text;
				int num5 = 0;
				int num6 = 0;
				string text3 = null;
				Dictionary<double, double> dictionary = new Dictionary<double, double>();
				int num7 = 0;
				while (num5 >= 0)
				{
					num5 = text2.IndexOf('<');
					num6 = text2.IndexOf('>');
					if (num5 >= 0 && num6 >= 0)
					{
						text3 = text2.Substring(num5 + 1, num6 - 1 - num5);
						list4.Add(text3);
						text2 = text2.Substring(num6 + 1, text2.Length - 1 - num6);
					}
					num7++;
				}
				bool flag2 = false;
				for (int j = 0; j < list4.Count; j += 3)
				{
					if (j + 2 < list4.Count && list4[j] != list4[j + 1] && long.Parse(list4[j], NumberStyles.HexNumber) == long.Parse(list4[j + 2], NumberStyles.HexNumber))
					{
						flag2 = true;
						break;
					}
				}
				int num8 = 0;
				while (num8 < list4.Count)
				{
					if (!flag2)
					{
						if (list4[num8] != list4[num8 + 1])
						{
							dictionary.Add(long.Parse(list4[num8], NumberStyles.HexNumber), long.Parse(list4[num8 + 1], NumberStyles.HexNumber));
							num8 += 2;
						}
						else if (num8 + 2 < list4.Count)
						{
							dictionary.Add(long.Parse(list4[num8], NumberStyles.HexNumber), long.Parse(list4[num8 + 2], NumberStyles.HexNumber));
							num8 += 3;
						}
						else
						{
							dictionary.Add(long.Parse(list4[num8], NumberStyles.HexNumber), long.Parse(list4[num8 + 1], NumberStyles.HexNumber));
							num8 += 2;
						}
					}
					else if (num8 + 2 < list4.Count)
					{
						dictionary.Add(long.Parse(list4[num8], NumberStyles.HexNumber), long.Parse(list4[num8 + 2], NumberStyles.HexNumber));
						num8 += 3;
					}
				}
				list3.Add(dictionary);
				list2.Add(list4);
			}
			if (data == null || data.Length == 0)
			{
				return " ";
			}
			string text4 = string.Empty;
			try
			{
				bool flag3 = false;
				bool flag4 = false;
				int num9 = 0;
				char[] array = new char[m_numberOfChars];
				for (int k = 0; k < m_numberOfChars; k++)
				{
					array[k] = ' ';
				}
				bool flag5 = false;
				string text5 = null;
				string text6 = string.Empty;
				string empty = string.Empty;
				int num10 = 0;
				List<byte> list5 = new List<byte>();
				List<string> list6 = new List<string>();
				List<string> list7 = new List<string>();
				bool flag6 = false;
				for (int l = 0; l < data.Length; l++)
				{
					char c = (char)data[l];
					if (CheckToken(new string[1] { "Tf" }, array))
					{
						int num11 = 0;
						int num12 = 0;
						for (int m = 0; m < array.Length; m++)
						{
							if (array[m] == '/')
							{
								num11 = m;
							}
							else if (array[m] == 'T' && array[m + 1] == 'f')
							{
								num12 = m;
							}
						}
						string text7 = new string(array).Substring(num11 + 1, num12 - 1 - num11);
						text5 = text7[..text7.IndexOf(' ')];
						list6.Add(text5);
					}
					if (CheckToken(new string[5] { "'", "T*", "Tj", "Td", "\n" }, array))
					{
						if (list5.Count > 0 && list5.Contains(41))
						{
							string text8 = string.Empty;
							list5.RemoveRange(list5.LastIndexOf(41), list5.Count - list5.LastIndexOf(41));
							string string2 = Encoding.BigEndianUnicode.GetString(list5.ToArray(), 0, list5.Count);
							string string3 = Encoding.GetEncoding("ascii").GetString(list5.ToArray(), 0, list5.Count);
							int num13 = 0;
							string text9 = string2;
							foreach (char c2 in text9)
							{
								if (list3.Count > num10 && list3[num10].ContainsKey((int)c2))
								{
									num13++;
									string text10 = ((char)list3[num10][(int)c2]).ToString();
									text8 += text10;
								}
							}
							list5.Clear();
							if (num13 != string2.Length)
							{
								text8 = "";
								text9 = string3;
								foreach (char c3 in text9)
								{
									if (list3.Count > num10 && list3[num10].ContainsKey((int)c3))
									{
										string text11 = ((char)list3[num10][(int)c3]).ToString();
										text8 += text11;
									}
								}
							}
							text4 += text8;
						}
						else
						{
							list5.Clear();
						}
						num9 = 0;
						flag = false;
					}
					if (flag3)
					{
						if (num9 == 0)
						{
							if (CheckToken(new string[2] { "TD", "Td" }, array))
							{
								if (flag5)
								{
									text4 += string.Empty;
								}
							}
							else if (CheckToken(new string[1] { "Tf" }, array))
							{
								int num14 = 0;
								int num15 = 0;
								for (int num16 = 0; num16 < array.Length; num16++)
								{
									if (array[num16] == '/')
									{
										num14 = num16;
									}
									else if (array[num16] == 'T' && array[num16 + 1] == 'f')
									{
										num15 = num16;
									}
								}
								string text12 = new string(array).Substring(num14 + 1, num15 - 1 - num14);
								text5 = text12[..text12.IndexOf(' ')];
								list6.Add(text5);
							}
							else
							{
								_ = data[l - 1];
								_ = 10;
							}
						}
						if (num9 == 0 && CheckToken(new string[1] { "ET" }, array))
						{
							flag3 = false;
							text4 += " ";
						}
						else if (c == '<' && num9 == 0 && !flag4)
						{
							if (!flag5)
							{
								num9 = 1;
								for (int num17 = l; num17 < data.Length - 1; num17++)
								{
									if (data[num17] != 62)
									{
										continue;
									}
									byte[] array2 = new byte[num17 - l - 1];
									int num18 = 0;
									for (int num19 = l + 1; num19 < num17; num19++)
									{
										array2[num18] = data[num19];
										num18++;
									}
									UTF8Encoding uTF8Encoding = new UTF8Encoding();
									int num20 = 0;
									int num21 = num17 - l - 1;
									byte[] array3 = new byte[num21];
									if (num21 > 3)
									{
										for (int num22 = 0; num22 < num21; num22++)
										{
											array3[num20] = array2[num22];
											string string4 = uTF8Encoding.GetString(array3, 0, array3.Length);
											int num23 = 0;
											if (flag6 && num20 == num21 - 1)
											{
												char[] array4 = string4.ToCharArray();
												for (int num24 = 0; num24 < num21 / 2; num24++)
												{
													string text13 = null;
													for (int num25 = 0; num25 < 2; num25++)
													{
														text13 += array4[num23];
														num23++;
													}
													int num26 = list2[0].IndexOf(text13);
													num26++;
													text4 += (char)long.Parse(list2[0][num26], NumberStyles.HexNumber);
												}
												num20 = 0;
											}
											else if (num20 == 3 && !flag6)
											{
												text4 += (char)long.Parse(string4, NumberStyles.HexNumber);
												num20 = 0;
											}
											else
											{
												num20++;
											}
										}
										break;
									}
									flag6 = true;
									for (int num27 = 0; num27 < 2; num27++)
									{
										array3[num20] = array2[num27];
										if (num20 == 1)
										{
											char[] array5 = uTF8Encoding.GetString(array3, 0, array3.Length).ToCharArray();
											string text14 = null;
											text14 += array5[0];
											text14 += array5[1];
											int num28 = list2[0].IndexOf(text14);
											num28++;
											text4 += (char)long.Parse(list2[0][num28], NumberStyles.HexNumber);
											num20 = 0;
										}
										else
										{
											num20++;
										}
									}
									break;
								}
							}
						}
						else
						{
							if (c == '(' && num9 == 0 && !flag4)
							{
								num9 = 1;
								flag5 = true;
							}
							else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
							{
								text4 += Environment.NewLine;
								flag = false;
							}
							else if (CheckToken(new string[1] { "T*" }, array))
							{
								text4 += Environment.NewLine;
								flag = false;
							}
							else if (CheckToken(new string[1] { "Tj" }, array))
							{
								num9 = 0;
								flag = false;
							}
							if (c == ')' && num9 == 1 && !flag4 && list5.Count <= 0)
							{
								num9 = 0;
								flag = false;
							}
							if (c == '>' && num9 == 1 && !flag4)
							{
								if (!flag5)
								{
									num9 = 0;
									text6 = string.Empty;
								}
							}
							else if (num9 == 1)
							{
								if (c == '\\' && !flag4)
								{
									flag4 = true;
								}
								else if ((c >= ' ' && c <= '~') || (c >= '\u0080' && c < 'ÿ'))
								{
									string text15 = null;
									string text16 = "/" + text5;
									if (list7.Count >= 1)
									{
										if (list7[list7.Count - 1].ToString() != text16)
										{
											list7.Add(text16);
										}
									}
									else
									{
										list7.Add(text16);
									}
									for (int num29 = 0; num29 < m_font.Count; num29++)
									{
										text15 = m_font[num29].ToString();
										if (text16.Equals(text15))
										{
											num10 = num29;
											break;
										}
									}
									empty = ((m_fref[num10] as PdfReferenceHolder).Object as PdfDictionary)["Subtype"].ToString();
									PdfDictionary pdfDictionary = (m_fref[num10] as PdfReferenceHolder).Object as PdfDictionary;
									if (pdfDictionary.ContainsKey("ToUnicode") && list2.Count > num10)
									{
										List<string> list8 = list2[num10];
										char[] array6 = new char[list8.Count];
										char[] array7 = new char[list8.Count];
										if (!flag5)
										{
											int num30 = 0;
											int num31 = 0;
											for (; num30 < list8.Count; num30 += 2)
											{
												ulong num32 = 0uL;
												char c4 = (char)Convert.ToInt32(Convert.ToUInt64(list8[num30].ToString(), 16).ToString());
												array6[num31] = c4;
												num31++;
											}
											int num33 = 1;
											int num34 = 0;
											for (; num33 < list8.Count; num33 += 2)
											{
												ulong num35 = 0uL;
												char c5 = (char)Convert.ToInt32(Convert.ToUInt64(list8[num33].ToString(), 16).ToString());
												array7[num34] = c5;
												num34++;
											}
										}
										if (text6.Length == 0)
										{
											if (list3[num10].ContainsKey((int)c))
											{
												string text17 = ((char)list3[num10][(int)c]).ToString();
												text6 += text17;
											}
										}
										else if (text6.Length == 1 && !flag5)
										{
											text6 += c;
											ulong num36 = 0uL;
											c = (char)Convert.ToInt32(Convert.ToUInt64(text6, 16).ToString());
											text6 = string.Empty;
											for (int num37 = 0; num37 < array6.Length; num37++)
											{
												if (c == array6[num37])
												{
													c = array7[num37];
													if (c != 0)
													{
														text4 += c;
													}
													break;
												}
											}
										}
									}
									if (flag5)
									{
										if (flag)
										{
											if (empty == "/Type0")
											{
												list5.Add((byte)c);
											}
											else if (pdfDictionary.ContainsKey("ToUnicode"))
											{
												if (list3.Count > num10)
												{
													if (list3[num10].ContainsKey((int)c))
													{
														string text18 = ((char)list3[num10][(int)c]).ToString();
														text4 += text18;
													}
													else
													{
														text4 += c;
													}
												}
												else
												{
													text4 += c;
												}
											}
											else
											{
												text4 += c;
											}
										}
										else
										{
											flag = true;
										}
									}
								}
								else
								{
									string text19 = null;
									string text20 = "/" + text5;
									if (list7.Count >= 1)
									{
										if (list7[list7.Count - 1].ToString() != text20)
										{
											list7.Add(text20);
										}
									}
									else
									{
										list7.Add(text20);
									}
									for (int num38 = 0; num38 < m_font.Count; num38++)
									{
										text19 = m_font[num38].ToString();
										if (text20.Equals(text19))
										{
											num10 = num38;
											break;
										}
									}
									empty = ((m_fref[num10] as PdfReferenceHolder).Object as PdfDictionary)["Subtype"].ToString();
									if (list2.Count > num10)
									{
										List<string> list9 = list2[num10];
										char[] array8 = new char[list9.Count];
										char[] array9 = new char[list9.Count];
										int num39 = 0;
										int num40 = 0;
										for (; num39 < list9.Count; num39 += 2)
										{
											ulong num41 = 0uL;
											char c6 = (char)Convert.ToInt32(Convert.ToUInt64(list9[num39].ToString(), 16).ToString());
											array8[num40] = c6;
											num40++;
										}
										int num42 = 0;
										int num43 = 0;
										for (; num42 < list9.Count; num42 += 2)
										{
											for (; list9[num42].ToString().Length < 4; num42++)
											{
											}
											ulong num44 = 0uL;
											char c7 = (char)Convert.ToInt32(Convert.ToUInt64(list9[num42].ToString(), 16).ToString());
											if (num42 > 2 && list9[num42 - 1] != list9[num42 - 2])
											{
												array9[num43] = c7;
												array9[num43 + 1] = (char)(c7 + 1);
												num43 += 2;
											}
											else
											{
												array9[num43] = c7;
												num43++;
											}
										}
										if (text6.Length == 0)
										{
											text6 += c;
										}
										else if (text6.Length == 1 && !flag5)
										{
											text6 += c;
											ulong num45 = 0uL;
											c = (char)Convert.ToInt32(Convert.ToUInt64(text6, 16).ToString());
											text6 = string.Empty;
											for (int num46 = 0; num46 < array8.Length; num46++)
											{
												if (c == array8[num46])
												{
													c = array9[num46];
													if (c != 0)
													{
														text4 += c;
													}
													break;
												}
											}
										}
										if (flag5)
										{
											if (empty == "/Type0")
											{
												list5.Add((byte)c);
											}
											else if (c < array9.Length)
											{
												char c8 = array9[c - 1];
												text4 += c8;
											}
										}
									}
								}
								flag4 = false;
							}
						}
					}
					for (int num47 = 0; num47 < m_numberOfChars - 1; num47++)
					{
						array[num47] = array[num47 + 1];
					}
					array[m_numberOfChars - 1] = c;
					if (!flag3 && CheckToken(new string[1] { "BT" }, array))
					{
						flag3 = true;
					}
				}
				return text4;
			}
			catch
			{
				return " ";
			}
		}
		return " ";
	}

	internal static string ExtractTextFrom_Type0Fonts(byte[] data, List<IPdfPrimitive> fontref, List<PdfName> fontname, PdfCrossTable crosstable)
	{
		string text = string.Empty;
		if (data == null || data.Length == 0)
		{
			return " ";
		}
		try
		{
			string text2 = string.Empty;
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			bool flag3 = false;
			bool flag4 = false;
			char[] array = new char[m_numberOfChars];
			for (int i = 0; i < m_numberOfChars; i++)
			{
				array[i] = ' ';
			}
			for (int j = 0; j < data.Length; j++)
			{
				char c = (char)data[j];
				if (CheckToken(new string[1] { "Tf" }, array))
				{
					int num2 = 0;
					int num3 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] == '/')
						{
							num2 = k;
						}
						else if (k > 0 && array[k] == 'f' && array[k - 1] == 'T')
						{
							num3 = k;
							break;
						}
					}
					string text3 = new string(array).Substring(num2 + 1, num3 - 1 - num2);
					string text4 = text3[..text3.IndexOf(' ')];
					if (fontname.Contains((PdfName)text4))
					{
						PdfReferenceHolder pointer = fontref[fontname.IndexOf((PdfName)text4)] as PdfReferenceHolder;
						PdfDictionary pdfDictionary = crosstable.GetObject(pointer) as PdfDictionary;
						pdfDictionary["BaseFont"].ToString();
						if (pdfDictionary["Subtype"].ToString() == "/Type0")
						{
							text = "Type0";
							if (pdfDictionary.ContainsKey("Encoding"))
							{
								PdfName pdfName = pdfDictionary["Encoding"] as PdfName;
								if (pdfName != null && (pdfName.Value == "Identity-H" || pdfName.Value == "Identity-V"))
								{
									flag4 = true;
								}
							}
							if (pdfDictionary.ContainsKey("ToUnicode") && pdfDictionary["ToUnicode"] as PdfReferenceHolder != null && flag4)
							{
								flag4 = false;
							}
						}
						else if (pdfDictionary["Subtype"].ToString() == "/TrueType")
						{
							text = "TrueType";
						}
					}
				}
				if (flag)
				{
					if (num == 0)
					{
						if (CheckToken(new string[1] { "TD" }, array))
						{
							text2 += Environment.NewLine;
							flag3 = !flag3;
						}
						else if (data[j - 1] == 10 && c == '-')
						{
							if (!text2.EndsWith(Environment.NewLine))
							{
								text2 += Environment.NewLine;
								flag3 = !flag3;
							}
						}
						else if (CheckToken(new string[3] { "'", "T*", "\"" }, array))
						{
							text2 += Environment.NewLine;
						}
						else if (CheckToken(new string[1] { "Tj" }, array))
						{
							text2 += string.Empty;
						}
					}
					if (num == 0 && CheckToken(new string[1] { "ET" }, array))
					{
						flag = false;
						text2 += " ";
					}
					else if (c == '(' && num == 0 && !flag2)
					{
						num = 1;
					}
					else if (c == ')' && num == 1 && !flag2)
					{
						num = 0;
					}
					else if (num == 1)
					{
						if (c == '\\' && !flag2)
						{
							flag2 = true;
							char[] array2 = new char[4];
							if (data.Length >= j + 4)
							{
								Array.Copy(data, j, array2, 0, 4);
								if (new string(array2) == "\\000")
								{
									j += 3;
									continue;
								}
							}
						}
						else
						{
							if (text == "TrueType" && ((c >= ' ' && c <= '~') || (c >= '\u0080' && c < 'ÿ')))
							{
								text2 += c;
							}
							else if (text == "Type0" && flag4)
							{
								text2 += c;
							}
							else if (text == "Type0" && ((c >= '\u0003' && c <= 'a') || (c >= 'c' && c < 'â')))
							{
								c = (char)(c + 29);
								text2 += c;
							}
							else if (array[m_numberOfChars - 1] == '\0')
							{
								text2 += " ";
							}
							flag2 = false;
						}
					}
				}
				for (int l = 0; l < m_numberOfChars - 1; l++)
				{
					array[l] = array[l + 1];
				}
				array[m_numberOfChars - 1] = c;
				if (!flag && CheckToken(new string[1] { "BT" }, array))
				{
					flag = true;
				}
			}
			return text2;
		}
		catch
		{
			return " ";
		}
	}

	private static bool CheckToken(string[] tokens, char[] recent)
	{
		foreach (string text in tokens)
		{
			if (text.Length > 1)
			{
				if (recent[m_numberOfChars - 3] == text[0] && recent[m_numberOfChars - 2] == text[1] && (recent[m_numberOfChars - 1] == ' ' || recent[m_numberOfChars - 1] == '\r' || recent[m_numberOfChars - 1] == '\n') && (recent[m_numberOfChars - 4] == ' ' || recent[m_numberOfChars - 4] == '\r' || recent[m_numberOfChars - 4] == '\n'))
				{
					return true;
				}
			}
			else if (recent[m_numberOfChars - 3] == text[0] && (recent[m_numberOfChars - 1] == ' ' || recent[m_numberOfChars - 1] == '\r' || recent[m_numberOfChars - 1] == '\n') && (recent[m_numberOfChars - 4] == ' ' || recent[m_numberOfChars - 4] == '\r' || recent[m_numberOfChars - 4] == '\n'))
			{
				return true;
			}
		}
		return false;
	}

	internal static string GetLatinCharacter(string decodedCharacter)
	{
		return decodedCharacter switch
		{
			"zero" => "0", 
			"one" => "1", 
			"two" => "2", 
			"three" => "3", 
			"four" => "4", 
			"five" => "5", 
			"six" => "6", 
			"seven" => "7", 
			"eight" => "8", 
			"nine" => "9", 
			"aring" => "å", 
			"asciicircum" => "^", 
			"asciitilde" => "~", 
			"asterisk" => "*", 
			"at" => "@", 
			"atilde" => "ã", 
			"backslash" => "\\", 
			"bar" => "|", 
			"braceleft" => "{", 
			"braceright" => "}", 
			"bracketleft" => "[", 
			"bracketright" => "]", 
			"breve" => "\u02d8", 
			"brokenbar" => "|", 
			"bullet3" => "•", 
			"bullet" => "•", 
			"caron" => "ˇ", 
			"ccedilla" => "ç", 
			"cedilla" => "\u00b8", 
			"cent" => "¢", 
			"circumflex" => "ˆ", 
			"colon" => ":", 
			"comma" => ",", 
			"copyright" => "©", 
			"currency1" => "¤", 
			"dagger" => "†", 
			"daggerdbl" => "‡", 
			"degree" => "°", 
			"dieresis" => "\u00a8", 
			"divide" => "÷", 
			"dollar" => "$", 
			"dotaccent" => "\u02d9", 
			"dotlessi" => "ı", 
			"eacute" => "é", 
			"ecircumflex" => "\u02d9", 
			"edieresis" => "ë", 
			"egrave" => "è", 
			"ellipsis" => "...", 
			"emdash" => "——", 
			"endash" => "–", 
			"equal" => "=", 
			"eth" => "ð", 
			"exclam" => "!", 
			"exclamdown" => "¡", 
			"fi" => "fl", 
			"florin" => "ƒ", 
			"fraction" => "⁄", 
			"germandbls" => "ß", 
			"grave" => "`", 
			"greater" => ">", 
			"guillemotleft4" => "«", 
			"guillemotright4" => "»", 
			"guilsinglleft" => "‹", 
			"guilsinglright" => "›", 
			"hungarumlaut" => "\u02dd", 
			"hyphen5" => "-", 
			"iacute" => "í", 
			"icircumflex" => "î", 
			"idieresis" => "ï", 
			"igrave" => "ì", 
			"less" => "<", 
			"logicalnot" => "¬", 
			"lslash" => "ł", 
			"macron" => "\u00af", 
			"minus" => "−", 
			"mu" => "μ", 
			"multiply" => "×", 
			"ntilde" => "ñ", 
			"numbersign" => "#", 
			"oacute" => "ó", 
			"ocircumflex" => "ô", 
			"odieresis" => "ö", 
			"oe" => "oe", 
			"ogonek" => "\u02db", 
			"ograve" => "ò", 
			"onehalf" => "1/2", 
			"onequarter" => "1/4", 
			"onesuperior" => "¹", 
			"ordfeminine" => "ª", 
			"ordmasculine" => "º", 
			"oslash" => "ø", 
			"otilde" => "õ", 
			"paragraph" => "¶", 
			"parenleft" => "(", 
			"parenright" => ")", 
			"percent" => "%", 
			"period" => ".", 
			"periodcentered" => "·", 
			"perthousand" => "‰", 
			"plus" => "+", 
			"plusminus" => "±", 
			"question" => "?", 
			"questiondown" => "¿", 
			"quotedbl" => "\"", 
			"quotedblbase" => "„", 
			"quotedblleft" => "“", 
			"quotedblright" => "”", 
			"quoteleft" => "‘", 
			"quoteright" => "’", 
			"quotesinglbase" => "‚", 
			"quotesingle" => "'", 
			"registered" => "®", 
			"ring" => "\u02da", 
			"scaron" => "š", 
			"section" => "§", 
			"semicolon" => ";", 
			"slash" => "/", 
			"space6" => " ", 
			"space" => " ", 
			"udieresis" => "ü", 
			"hyphen" => "-", 
			"underscore" => "_", 
			"adieresis" => "ä", 
			"ampersand" => "&", 
			"Adieresis" => "Ä", 
			"Udieresis" => "Ü", 
			"ccaron" => "č", 
			"Scaron" => "Š", 
			"zcaron" => "ž", 
			_ => decodedCharacter, 
		};
	}

	internal static string GetSpecialCharacter(string decodedCharacter)
	{
		switch (decodedCharacter)
		{
		case "head2right":
			return "➢";
		case "aacute":
			return "a\u0301";
		case "eacute":
			return "e\u0301";
		case "iacute":
			return "i\u0301";
		case "oacute":
			return "o\u0301";
		case "uacute":
			return "u\u0301";
		case "circleright":
			return "➲";
		case "bleft":
			return "⇦";
		case "bright":
			return "⇨";
		case "bup":
			return "⇧";
		case "bdown":
			return "⇩";
		case "barb4right":
			return "➔";
		case "bleftright":
			return "⬄";
		case "bupdown":
			return "⇳";
		case "bnw":
			return "⬀";
		case "bne":
			return "⬁";
		case "bsw":
			return "⬃";
		case "bse":
			return "⬂";
		case "bdash1":
			return "▭";
		case "bdash2":
			return "▫";
		case "xmarkbld":
			return "✗";
		case "checkbld":
			return "✓";
		case "boxxmarkbld":
			return "☒";
		case "boxcheckbld":
			return "☑";
		case "space":
			return " ";
		case "pencil":
			return "✏";
		case "scissors":
			return "✂";
		case "scissorscutting":
			return "✁";
		case "readingglasses":
			return "✁";
		case "bell":
			return "✁";
		case "book":
			return "✁";
		case "telephonesolid":
			return "✁";
		case "telhandsetcirc":
			return "✁";
		case "envelopeback":
			return "✁";
		case "hourglass":
			return "⌛";
		case "keyboard":
			return "⌨";
		case "tapereel":
			return "✇";
		case "handwrite":
			return "✍";
		case "handv":
			return "✌";
		case "handptleft":
			return "☜";
		case "handptright":
			return "☞";
		case "handptup":
			return "☝";
		case "handptdown":
			return "☟";
		case "smileface":
			return "☺";
		case "frownface":
			return "☹";
		case "skullcrossbones":
			return "☠";
		case "flag":
			return "⚐";
		case "pennant":
			return "Ὢ9";
		case "airplane":
			return "✈";
		case "sunshine":
			return "☼";
		case "droplet":
			return "Ὂ7";
		case "snowflake":
			return "❄";
		case "crossshadow":
			return "✞";
		case "crossmaltese":
			return "✠";
		case "starofdavid":
			return "✡";
		case "crescentstar":
			return "☪";
		case "yinyang":
			return "☯";
		case "om":
			return "ॐ";
		case "wheel":
			return "☸";
		case "aries":
			return "♈";
		case "taurus":
			return "♉";
		case "gemini":
			return "♊";
		case "cancer":
			return "♋";
		case "leo":
			return "♌";
		case "virgo":
			return "♍";
		case "libra":
			return "♎";
		case "scorpio":
			return "♏";
		case "saggitarius":
			return "♐";
		case "capricorn":
			return "♑";
		case "aquarius":
			return "♒";
		case "pisces":
			return "♓";
		case "ampersanditlc":
			return "&";
		case "ampersandit":
			return "&";
		case "circle6":
			return "●";
		case "circleshadowdwn":
			return "❍";
		case "square6":
			return "■";
		case "box3":
			return "□";
		case "boxshadowdwn":
			return "❑";
		case "boxshadowup":
			return "❒";
		case "lozenge4":
			return "⬧";
		case "lozenge6":
			return "⧫";
		case "rhombus6":
			return "◆";
		case "xrhombus":
			return "❖";
		case "rhombus4":
			return "⬥";
		case "clear":
			return "⌧";
		case "escape":
			return "⍓";
		case "command":
			return "⌘";
		case "rosette":
			return "❀";
		case "rosettesolid":
			return "✿";
		case "quotedbllftbld":
			return "❝";
		case "quotedblrtbld":
			return "❞";
		case ".notdef":
			return "▯";
		case "zerosans":
			return "⓪";
		case "onesans":
			return "①";
		case "twosans":
			return "②";
		case "threesans":
			return "③";
		case "foursans":
			return "④";
		case "fivesans":
			return "⑤";
		case "sixsans":
			return "⑥";
		case "sevensans":
			return "⑦";
		case "eightsans":
			return "⑧";
		case "ninesans":
			return "⑨";
		case "tensans":
			return "⑩";
		case "zerosansinv":
			return "⓿";
		case "onesansinv":
			return "❶";
		case "twosansinv":
			return "❷";
		case "threesansinv":
			return "❸";
		case "foursansinv":
			return "❹";
		case "circle2":
			return "·";
		case "circle4":
			return "•";
		case "square2":
			return "▪";
		case "ring2":
			return "○";
		case "ringbutton2":
			return "◉";
		case "target":
			return "◎";
		case "square4":
			return "▪";
		case "box2":
			return "◻";
		case "crosstar2":
			return "✦";
		case "pentastar2":
			return "★";
		case "hexstar2":
			return "✶";
		case "octastar2":
			return "✴";
		case "dodecastar3":
			return "✹";
		case "octastar4":
			return "✵";
		case "registercircle":
			return "⌖";
		case "cuspopen":
			return "⟡";
		case "cuspopen1":
			return "⌑";
		case "circlestar":
			return "★";
		case "starshadow":
			return "✰";
		case "deleteleft":
			return "⌫";
		case "deleteright":
			return "⌦";
		case "scissorsoutline":
			return "✄";
		case "telephone":
			return "☏";
		case "telhandset":
			return "ὍE";
		case "handptlft1":
			return "☜";
		case "handptrt1":
			return "☞";
		case "handptlftsld1":
			return "☚";
		case "handptrtsld1":
			return "☛";
		case "handptup1":
			return "☝";
		case "handptdwn1":
			return "☟";
		case "xmark":
			return "✗";
		case "check":
			return "✓";
		case "boxcheck":
			return "☑";
		case "boxx":
			return "☒";
		case "boxxbld":
			return "☒";
		case "circlex":
			return "=⌔";
		case "circlexbld":
			return "⌔";
		case "prohibitbld":
		case "prohibit":
			return "⦸";
		case "ampersanditaldm":
		case "ampersandsandm":
		case "ampersandbld":
		case "ampersandsans":
			return "&";
		case "interrobang":
		case "interrobangsans":
		case "interrobngsandm":
		case "interrobangdm":
			return "‽";
		case "park":
			return "\ue0e0";
		case "g120":
			return "·";
		case "g45":
		case "g383":
			return "☺";
		default:
			return decodedCharacter;
		}
	}
}

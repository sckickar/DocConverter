using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedBookmark : PdfBookmark
{
	private PdfNamedDestination m_namedDestination;

	private Regex regex = new Regex("[\u0080-ÿ]");

	private char[] pdfEncodingByteToChar = new char[256]
	{
		'\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
		'\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013',
		'\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d',
		'\u001e', '\u001f', ' ', '!', '"', '#', '$', '%', '&', '\'',
		'(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
		'2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
		'<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E',
		'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
		'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
		'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c',
		'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
		'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
		'x', 'y', 'z', '{', '|', '}', '~', '\u007f', '•', '†',
		'‡', '…', '—', '–', 'ƒ', '⁄', '‹', '›', '−', '‰',
		'„', '“', '”', '‘', '’', '‚', '™', 'ﬁ', 'ﬂ', 'Ł',
		'Œ', 'Š', 'Ÿ', 'Ž', 'ı', 'ł', 'œ', 'š', 'ž', '\ufffd',
		'€', '¡', '¢', '£', '¤', '¥', '¦', '§', '\u00a8', '©',
		'ª', '«', '¬', '\u00ad', '®', '\u00af', '°', '±', '²', '³',
		'\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼', '½',
		'¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç',
		'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ',
		'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú', 'Û',
		'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å',
		'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï',
		'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', '÷', 'ø', 'ù',
		'ú', 'û', 'ü', 'ý', 'þ', 'ÿ'
	};

	public override PdfDestination Destination
	{
		get
		{
			PdfDestination result = null;
			if (ObtainNamedDestination() == null)
			{
				result = ObtainDestination();
			}
			return result;
		}
		set
		{
			base.Destination = value;
		}
	}

	public override PdfNamedDestination NamedDestination
	{
		get
		{
			if (m_namedDestination == null)
			{
				m_namedDestination = ObtainNamedDestination();
			}
			return m_namedDestination;
		}
		set
		{
			base.NamedDestination = value;
		}
	}

	public override string Title
	{
		get
		{
			return ObtainTitle();
		}
		set
		{
			base.Title = value;
		}
	}

	public override PdfColor Color
	{
		get
		{
			return ObtainColor();
		}
		set
		{
			AssignColor(value);
		}
	}

	public override PdfTextStyle TextStyle
	{
		get
		{
			return ObtainTextStyle();
		}
		set
		{
			AssignTextStyle(value);
		}
	}

	internal override PdfBookmark Next
	{
		get
		{
			return ObtainNext();
		}
		set
		{
			base.Next = value;
		}
	}

	internal override PdfBookmark Previous
	{
		get
		{
			return ObtainPrevious();
		}
		set
		{
			base.Previous = value;
		}
	}

	internal override PdfBookmarkBase Parent => base.Parent;

	internal override List<PdfBookmarkBase> List
	{
		get
		{
			List<PdfBookmarkBase> list = base.List;
			if (list.Count == 0)
			{
				ReproduceTree();
			}
			return list;
		}
	}

	internal PdfLoadedBookmark(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
	}

	private PdfLoadedNamedDestination ObtainNamedDestination()
	{
		PdfLoadedDocument pdfLoadedDocument = base.CrossTable.Document as PdfLoadedDocument;
		PdfNamedDestinationCollection pdfNamedDestinationCollection = null;
		if (pdfLoadedDocument != null)
		{
			pdfNamedDestinationCollection = pdfLoadedDocument.NamedDestinationCollection;
		}
		PdfLoadedNamedDestination result = null;
		IPdfPrimitive pdfPrimitive = null;
		if (pdfNamedDestinationCollection != null)
		{
			if (base.Dictionary.ContainsKey("A"))
			{
				if (PdfCrossTable.Dereference(base.Dictionary["A"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("D"))
				{
					pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary["D"]) as PdfString;
				}
			}
			else if (base.Dictionary.ContainsKey("Dest"))
			{
				pdfPrimitive = base.CrossTable.GetObject(base.Dictionary["Dest"]);
			}
			if (pdfPrimitive != null)
			{
				PdfName pdfName = pdfPrimitive as PdfName;
				PdfString pdfString = pdfPrimitive as PdfString;
				string text = null;
				if (pdfName != null)
				{
					text = pdfName.Value;
				}
				else if (pdfString != null)
				{
					text = pdfString.Value;
				}
				if (text != null)
				{
					for (int i = 0; i < pdfNamedDestinationCollection.Count; i++)
					{
						if (pdfNamedDestinationCollection[i] is PdfLoadedNamedDestination pdfLoadedNamedDestination && pdfLoadedNamedDestination.Title.Equals(text))
						{
							result = pdfLoadedNamedDestination;
							break;
						}
					}
				}
			}
		}
		return result;
	}

	private string ObtainTitle()
	{
		string text = string.Empty;
		if (base.Dictionary.ContainsKey("Title"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Title"]) is PdfString pdfString)
			{
				text = pdfString.Value;
			}
			if (regex.IsMatch(text))
			{
				text = ConvertUnicodeToString(text);
			}
		}
		return text;
	}

	private string ConvertUnicodeToString(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (regex.IsMatch(text[i].ToString()))
			{
				char newChar = pdfEncodingByteToChar[(byte)text[i] & 0xFF];
				text = text.Replace(text[i], newChar);
			}
		}
		return text;
	}

	private PdfColor ObtainColor()
	{
		PdfColor result = new PdfColor(0, 0, 0);
		if (base.Dictionary.ContainsKey("C") && PdfCrossTable.Dereference(base.Dictionary["C"]) is PdfArray { Count: >2 } pdfArray)
		{
			float red = 0f;
			float green = 0f;
			float blue = 0f;
			if (PdfCrossTable.Dereference(pdfArray[0]) is PdfNumber pdfNumber)
			{
				red = pdfNumber.FloatValue;
			}
			if (PdfCrossTable.Dereference(pdfArray[1]) is PdfNumber pdfNumber2)
			{
				green = pdfNumber2.FloatValue;
			}
			if (PdfCrossTable.Dereference(pdfArray[2]) is PdfNumber pdfNumber3)
			{
				blue = pdfNumber3.FloatValue;
			}
			result = new PdfColor(red, green, blue);
		}
		return result;
	}

	private PdfTextStyle ObtainTextStyle()
	{
		PdfTextStyle pdfTextStyle = PdfTextStyle.Regular;
		if (base.Dictionary.ContainsKey("F"))
		{
			PdfNumber pdfNumber = PdfCrossTable.Dereference(base.Dictionary["F"]) as PdfNumber;
			int num = 0;
			if (pdfNumber != null)
			{
				num = pdfNumber.IntValue;
			}
			pdfTextStyle = (PdfTextStyle)((int)pdfTextStyle | num);
		}
		return pdfTextStyle;
	}

	private PdfBookmark ObtainNext()
	{
		PdfBookmark result = null;
		int num = Parent.List.IndexOf(this);
		num++;
		if (num < Parent.List.Count)
		{
			result = Parent.List[num] as PdfBookmark;
		}
		else if (base.Dictionary.ContainsKey("Next"))
		{
			PdfDictionary dictionary = base.CrossTable.GetObject(base.Dictionary["Next"]) as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = base.Dictionary["Next"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
			{
				if (Parent.m_bookmarkReference.Contains(pdfReferenceHolder.Reference.ObjNum))
				{
					Parent.m_bookmarkReference.Clear();
					return result;
				}
				Parent.m_bookmarkReference.Add(pdfReferenceHolder.Reference.ObjNum);
			}
			result = new PdfLoadedBookmark(dictionary, base.CrossTable);
		}
		return result;
	}

	private PdfBookmark ObtainPrevious()
	{
		PdfBookmark result = null;
		int num = List.IndexOf(this);
		num--;
		if (num >= 0)
		{
			result = List[num] as PdfBookmark;
		}
		else if (base.Dictionary.ContainsKey("Prev"))
		{
			result = new PdfLoadedBookmark(base.CrossTable.GetObject(base.Dictionary["Prev"]) as PdfDictionary, base.CrossTable);
		}
		return result;
	}

	private void AssignColor(PdfColor color)
	{
		PdfArray primitive = new PdfArray(new float[3] { color.Red, color.Green, color.Blue });
		base.Dictionary.SetProperty("C", primitive);
	}

	private void AssignTextStyle(PdfTextStyle value)
	{
		int num = (int)ObtainTextStyle();
		num |= (int)value;
		base.Dictionary.SetNumber("F", num);
	}

	private PdfDestination ObtainDestination()
	{
		if (base.Dictionary.ContainsKey("Dest") && base.Destination == null)
		{
			IPdfPrimitive @object = base.CrossTable.GetObject(base.Dictionary["Dest"]);
			PdfArray pdfArray = @object as PdfArray;
			PdfName pdfName = @object as PdfName;
			PdfString pdfString = @object as PdfString;
			PdfLoadedDocument pdfLoadedDocument = base.CrossTable.Document as PdfLoadedDocument;
			if (pdfLoadedDocument != null)
			{
				if (pdfName != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfName);
				}
				else if (pdfString != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfString);
				}
			}
			if (pdfArray != null)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[0] as PdfReferenceHolder;
				PdfPageBase pdfPageBase = null;
				if (pdfReferenceHolder == null && pdfArray[0] is PdfNumber)
				{
					PdfNumber pdfNumber = pdfArray[0] as PdfNumber;
					if (pdfNumber.IntValue >= 0)
					{
						if (pdfLoadedDocument != null && pdfLoadedDocument.PageCount > pdfNumber.IntValue)
						{
							pdfPageBase = pdfLoadedDocument.Pages[pdfNumber.IntValue];
						}
						PdfName pdfName2 = null;
						if (pdfArray.Count > 1)
						{
							pdfName2 = pdfArray[1] as PdfName;
						}
						if (pdfName2 != null)
						{
							if (pdfName2.Value == "XYZ")
							{
								PdfNumber pdfNumber2 = null;
								PdfNumber pdfNumber3 = null;
								if (pdfArray.Count > 2)
								{
									pdfNumber2 = pdfArray[2] as PdfNumber;
								}
								if (pdfArray.Count > 3)
								{
									pdfNumber3 = pdfArray[3] as PdfNumber;
								}
								PdfNumber pdfNumber4 = null;
								if (pdfArray.Count > 4)
								{
									pdfNumber4 = pdfArray[4] as PdfNumber;
								}
								if (pdfPageBase != null)
								{
									float y = ((pdfNumber3 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber3.FloatValue));
									float x = pdfNumber2?.FloatValue ?? 0f;
									if (pdfPageBase is PdfLoadedPage && pdfPageBase.Rotation != 0)
									{
										y = CheckRotation(pdfPageBase, pdfNumber3, pdfNumber2);
									}
									base.Destination = new PdfDestination(pdfPageBase, new PointF(x, y));
									base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
									if (pdfNumber4 != null)
									{
										base.Destination.Zoom = pdfNumber4.FloatValue;
									}
									base.Destination.isModified = false;
									if (pdfNumber2 == null || pdfNumber3 == null || pdfNumber4 == null)
									{
										base.Destination.SetValidation(valid: false);
									}
								}
							}
						}
						else if (pdfPageBase != null)
						{
							base.Destination = new PdfDestination(pdfPageBase);
							base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
							base.Destination.Mode = PdfDestinationMode.FitToPage;
							base.Destination.isModified = false;
						}
					}
				}
				if (pdfReferenceHolder != null)
				{
					PdfDictionary pdfDictionary = base.CrossTable.GetObject(pdfReferenceHolder) as PdfDictionary;
					if (pdfLoadedDocument != null && pdfDictionary != null)
					{
						pdfPageBase = pdfLoadedDocument.Pages.GetPage(pdfDictionary);
					}
					PdfName pdfName3 = null;
					if (pdfArray.Count > 1)
					{
						pdfName3 = pdfArray[1] as PdfName;
					}
					if (pdfName3 != null)
					{
						if (pdfName3.Value == "XYZ")
						{
							PdfNumber pdfNumber5 = null;
							PdfNumber pdfNumber6 = null;
							if (pdfArray.Count > 2)
							{
								pdfNumber5 = pdfArray[2] as PdfNumber;
							}
							if (pdfArray.Count > 3)
							{
								pdfNumber6 = pdfArray[3] as PdfNumber;
							}
							PdfNumber pdfNumber7 = null;
							if (pdfArray.Count > 4)
							{
								pdfNumber7 = pdfArray[4] as PdfNumber;
							}
							if (pdfPageBase != null)
							{
								float num = 0f;
								RectangleF cropBox = (pdfPageBase as PdfLoadedPage).GetCropBox();
								num = ((!(cropBox != RectangleF.Empty)) ? ((pdfNumber6 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber6.FloatValue)) : ((pdfNumber6 == null) ? 0f : (cropBox.Bottom - pdfNumber6.FloatValue)));
								float x2 = pdfNumber5?.FloatValue ?? 0f;
								if (pdfPageBase is PdfLoadedPage && pdfPageBase.Rotation != 0)
								{
									num = CheckRotation(pdfPageBase, pdfNumber6, pdfNumber5);
								}
								base.Destination = new PdfDestination(pdfPageBase, new PointF(x2, num));
								base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
								if (pdfNumber7 != null)
								{
									base.Destination.Zoom = pdfNumber7.FloatValue;
								}
								base.Destination.isModified = false;
								if (pdfNumber5 == null || pdfNumber6 == null || pdfNumber7 == null)
								{
									base.Destination.SetValidation(valid: false);
								}
							}
						}
						else if (pdfName3.Value == "FitR")
						{
							PdfNumber pdfNumber8 = null;
							PdfNumber pdfNumber9 = null;
							PdfNumber pdfNumber10 = null;
							PdfNumber pdfNumber11 = null;
							if (pdfArray.Count > 2)
							{
								pdfNumber8 = pdfArray[2] as PdfNumber;
							}
							if (pdfArray.Count > 3)
							{
								pdfNumber9 = pdfArray[3] as PdfNumber;
							}
							if (pdfArray.Count > 4)
							{
								pdfNumber10 = pdfArray[4] as PdfNumber;
							}
							if (pdfArray.Count > 5)
							{
								pdfNumber11 = pdfArray[5] as PdfNumber;
							}
							if (pdfPageBase != null)
							{
								pdfNumber8 = ((pdfNumber8 == null) ? new PdfNumber(0) : pdfNumber8);
								pdfNumber9 = ((pdfNumber9 == null) ? new PdfNumber(0) : pdfNumber9);
								pdfNumber10 = ((pdfNumber10 == null) ? new PdfNumber(0) : pdfNumber10);
								pdfNumber11 = ((pdfNumber11 == null) ? new PdfNumber(0) : pdfNumber11);
								base.Destination = new PdfDestination(pdfPageBase, new RectangleF(pdfNumber8.FloatValue, pdfNumber9.FloatValue, pdfNumber10.FloatValue, pdfNumber11.FloatValue));
								base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
								base.Destination.Mode = PdfDestinationMode.FitR;
								base.Destination.isModified = false;
							}
						}
						else if (pdfName3.Value == "FitBH" || pdfName3.Value == "FitH")
						{
							PdfNumber pdfNumber12 = null;
							if (pdfArray.Count >= 3)
							{
								pdfNumber12 = pdfArray[2] as PdfNumber;
							}
							if (pdfPageBase != null)
							{
								float y2 = ((pdfNumber12 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber12.FloatValue));
								base.Destination = new PdfDestination(pdfPageBase, new PointF(0f, y2));
								base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
								base.Destination.Mode = PdfDestinationMode.FitH;
								base.Destination.isModified = false;
								if (pdfNumber12 == null)
								{
									base.Destination.SetValidation(valid: false);
								}
							}
						}
						else if (pdfPageBase != null && pdfName3.Value == "Fit")
						{
							base.Destination = new PdfDestination(pdfPageBase);
							base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
							base.Destination.Mode = PdfDestinationMode.FitToPage;
							base.Destination.isModified = false;
						}
					}
					else if (pdfPageBase != null)
					{
						base.Destination = new PdfDestination(pdfPageBase);
						base.Destination.PageIndex = pdfLoadedDocument.Pages.IndexOf(pdfPageBase);
						base.Destination.Mode = PdfDestinationMode.FitToPage;
						base.Destination.isModified = false;
					}
				}
			}
		}
		else if (base.Dictionary.ContainsKey("A") && base.Destination == null)
		{
			IPdfPrimitive pdfPrimitive = base.CrossTable.GetObject(base.Dictionary["A"]);
			if (pdfPrimitive is PdfDictionary pdfDictionary2)
			{
				pdfPrimitive = pdfDictionary2["D"];
			}
			if (pdfPrimitive is PdfReferenceHolder)
			{
				pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
			}
			PdfArray pdfArray2 = pdfPrimitive as PdfArray;
			PdfName pdfName4 = pdfPrimitive as PdfName;
			PdfString pdfString2 = pdfPrimitive as PdfString;
			PdfLoadedDocument pdfLoadedDocument2 = base.CrossTable.Document as PdfLoadedDocument;
			if (pdfLoadedDocument2 != null)
			{
				if (pdfName4 != null)
				{
					pdfArray2 = pdfLoadedDocument2.GetNamedDestination(pdfName4);
				}
				else if (pdfString2 != null)
				{
					pdfArray2 = pdfLoadedDocument2.GetNamedDestination(pdfString2);
				}
			}
			if (pdfArray2 != null)
			{
				PdfReferenceHolder pdfReferenceHolder2 = pdfArray2[0] as PdfReferenceHolder;
				PdfPageBase pdfPageBase2 = null;
				if (pdfReferenceHolder2 != null && base.CrossTable.GetObject(pdfReferenceHolder2) is PdfDictionary dic && pdfLoadedDocument2 != null)
				{
					pdfPageBase2 = pdfLoadedDocument2.Pages.GetPage(dic);
				}
				PdfName pdfName5 = null;
				if (pdfArray2.Count > 1)
				{
					pdfName5 = pdfArray2[1] as PdfName;
				}
				if (pdfName5 != null)
				{
					if (pdfName5.Value == "FitBH" || pdfName5.Value == "FitH")
					{
						PdfNumber pdfNumber13 = null;
						if (pdfArray2.Count >= 3)
						{
							pdfNumber13 = pdfArray2[2] as PdfNumber;
						}
						if (pdfPageBase2 != null)
						{
							float y3 = ((pdfNumber13 == null) ? 0f : (pdfPageBase2.Size.Height - pdfNumber13.FloatValue));
							base.Destination = new PdfDestination(pdfPageBase2, new PointF(0f, y3));
							base.Destination.PageIndex = pdfLoadedDocument2.Pages.IndexOf(pdfPageBase2);
							base.Destination.Mode = PdfDestinationMode.FitH;
							base.Destination.isModified = false;
							if (pdfNumber13 == null)
							{
								base.Destination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfName5.Value == "FitR")
					{
						PdfNumber pdfNumber14 = null;
						PdfNumber pdfNumber15 = null;
						PdfNumber pdfNumber16 = null;
						PdfNumber pdfNumber17 = null;
						if (pdfArray2.Count > 2)
						{
							pdfNumber14 = pdfArray2[2] as PdfNumber;
						}
						if (pdfArray2.Count > 3)
						{
							pdfNumber15 = pdfArray2[3] as PdfNumber;
						}
						if (pdfArray2.Count > 4)
						{
							pdfNumber16 = pdfArray2[4] as PdfNumber;
						}
						if (pdfArray2.Count > 5)
						{
							pdfNumber17 = pdfArray2[5] as PdfNumber;
						}
						if (pdfPageBase2 != null)
						{
							pdfNumber14 = ((pdfNumber14 == null) ? new PdfNumber(0) : pdfNumber14);
							pdfNumber15 = ((pdfNumber15 == null) ? new PdfNumber(0) : pdfNumber15);
							pdfNumber16 = ((pdfNumber16 == null) ? new PdfNumber(0) : pdfNumber16);
							pdfNumber17 = ((pdfNumber17 == null) ? new PdfNumber(0) : pdfNumber17);
							base.Destination = new PdfDestination(pdfPageBase2, new RectangleF(pdfNumber14.FloatValue, pdfNumber15.FloatValue, pdfNumber16.FloatValue, pdfNumber17.FloatValue));
							base.Destination.PageIndex = pdfLoadedDocument2.Pages.IndexOf(pdfPageBase2);
							base.Destination.Mode = PdfDestinationMode.FitR;
							base.Destination.isModified = false;
						}
					}
					else if (pdfName5.Value == "XYZ")
					{
						PdfNumber pdfNumber18 = null;
						PdfNumber pdfNumber19 = null;
						if (pdfArray2.Count > 2)
						{
							pdfNumber18 = pdfArray2[2] as PdfNumber;
						}
						if (pdfArray2.Count > 3)
						{
							pdfNumber19 = pdfArray2[3] as PdfNumber;
						}
						PdfNumber pdfNumber20 = null;
						if (pdfArray2.Count > 4)
						{
							pdfNumber20 = pdfArray2[4] as PdfNumber;
						}
						if (pdfPageBase2 != null)
						{
							float y4 = ((pdfNumber19 == null) ? 0f : (pdfPageBase2.Size.Height - pdfNumber19.FloatValue));
							float x3 = pdfNumber18?.FloatValue ?? 0f;
							base.Destination = new PdfDestination(pdfPageBase2, new PointF(x3, y4));
							base.Destination.PageIndex = pdfLoadedDocument2.Pages.IndexOf(pdfPageBase2);
							if (pdfNumber20 != null)
							{
								base.Destination.Zoom = pdfNumber20.FloatValue;
							}
							base.Destination.isModified = false;
							if (pdfNumber18 == null || pdfNumber19 == null || pdfNumber20 == null)
							{
								base.Destination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfPageBase2 != null && pdfName5.Value == "Fit")
					{
						base.Destination = new PdfDestination(pdfPageBase2);
						base.Destination.PageIndex = pdfLoadedDocument2.Pages.IndexOf(pdfPageBase2);
						base.Destination.Mode = PdfDestinationMode.FitToPage;
						base.Destination.isModified = false;
					}
				}
				else if (pdfPageBase2 != null)
				{
					base.Destination = new PdfDestination(pdfPageBase2);
					base.Destination.PageIndex = pdfLoadedDocument2.Pages.IndexOf(pdfPageBase2);
					base.Destination.Mode = PdfDestinationMode.FitToPage;
					base.Destination.isModified = false;
				}
			}
		}
		return base.Destination;
	}

	private float CheckRotation(PdfPageBase page, PdfNumber top, PdfNumber left)
	{
		float result = 0f;
		left = ((left == null) ? new PdfNumber(0) : left);
		if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			result = ((top == null) ? 0f : left.FloatValue);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
		{
			result = top?.FloatValue ?? 0f;
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			result = ((top == null) ? 0f : (page.Size.Width - left.FloatValue));
		}
		return result;
	}
}

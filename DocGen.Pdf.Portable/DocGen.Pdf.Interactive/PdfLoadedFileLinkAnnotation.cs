using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedFileLinkAnnotation : PdfLoadedStyledAnnotation
{
	private int[] destinationArray;

	private PdfCrossTable m_crossTable;

	private PdfLaunchAction m_action;

	private PdfArray m_destination;

	public string FileName
	{
		get
		{
			return ObtainFileName();
		}
		set
		{
			PdfDictionary dictionary = base.Dictionary;
			if (base.Dictionary.ContainsKey("A"))
			{
				dictionary = m_crossTable.GetObject(base.Dictionary["A"]) as PdfDictionary;
				PdfDictionary pdfDictionary = m_crossTable.GetObject(dictionary["F"]) as PdfDictionary;
				pdfDictionary.SetString("F", value);
				if (pdfDictionary.ContainsKey("UF"))
				{
					pdfDictionary.SetString("UF", value);
				}
				base.Dictionary.Modify();
				NotifyPropertyChanged("FileName");
			}
		}
	}

	private PdfArray Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			m_destination = value;
		}
	}

	public int[] DestinationArray
	{
		get
		{
			return ObtainDestination();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DestinationPageNumber");
			}
			if (value != destinationArray)
			{
				destinationArray = value;
				if (Destination != null)
				{
					Destination.Clear();
				}
				else
				{
					Destination = new PdfArray();
				}
				Destination.Add(new PdfNumber(value[0] - 1));
				Destination.Add(new PdfName("XYZ"));
				Destination.Add(new PdfNumber(value[1]));
				Destination.Add(new PdfNumber(value[2]));
				Destination.Add(new PdfNull());
				PdfDictionary dictionary = base.Dictionary;
				if (base.Dictionary.ContainsKey("A"))
				{
					dictionary = m_crossTable.GetObject(base.Dictionary["A"]) as PdfDictionary;
					dictionary.Remove("D");
					dictionary.SetProperty("D", Destination);
					if (dictionary.ContainsKey("F"))
					{
						dictionary["S"] = new PdfName("GoToR");
					}
					base.Dictionary.Modify();
				}
			}
			NotifyPropertyChanged("DestinationArray");
		}
	}

	private string ObtainFileName()
	{
		string result = string.Empty;
		if (base.Dictionary.ContainsKey("A"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["A"]) as PdfDictionary;
			PdfString pdfString = null;
			PdfDictionary pdfDictionary2 = null;
			if (PdfCrossTable.Dereference(pdfDictionary["F"]) is PdfString)
			{
				pdfString = PdfCrossTable.Dereference(pdfDictionary["F"]) as PdfString;
			}
			else
			{
				pdfDictionary2 = m_crossTable.GetObject(pdfDictionary["F"]) as PdfDictionary;
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("F"))
				{
					pdfString = pdfDictionary2["F"] as PdfString;
				}
				else if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("UF"))
				{
					pdfString = pdfDictionary2["UF"] as PdfString;
				}
			}
			result = pdfString.Value.ToString();
		}
		return result;
	}

	internal PdfLoadedFileLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string filename)
		: base(dictionary, crossTable)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_action = new PdfLaunchAction(filename, loaded: true);
	}

	internal PdfLoadedFileLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, PdfArray destination, RectangleF rectangle, string filename)
		: base(dictionary, crossTable)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		Destination = destination;
	}

	private int[] ObtainDestination()
	{
		int[] array = null;
		if (base.Dictionary.ContainsKey("A"))
		{
			PdfArray obj = PdfCrossTable.Dereference((m_crossTable.GetObject(base.Dictionary["A"]) as PdfDictionary)["D"]) as PdfArray;
			int num = 0;
			array = new int[obj.Count - 1];
			foreach (object item in obj)
			{
				if (item is PdfNumber)
				{
					if (num == 0)
					{
						array[num] = (item as PdfNumber).IntValue + 1;
						num++;
					}
					else
					{
						array[num] = (item as PdfNumber).IntValue;
						num++;
					}
				}
				else if (item is PdfNull)
				{
					array[num] = 0;
					num++;
				}
			}
		}
		return array;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (!(base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			return;
		}
		if (base.Dictionary["AP"] != null && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate != null)
			{
				PdfGraphics pdfGraphics = ObtainlayerGraphics();
				PdfGraphicsState state = base.Page.Graphics.Save();
				if (Opacity < 1f)
				{
					base.Page.Graphics.SetTransparency(Opacity);
				}
				if (pdfGraphics != null)
				{
					pdfGraphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				else
				{
					base.Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				base.Page.Graphics.Restore(state);
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}
}

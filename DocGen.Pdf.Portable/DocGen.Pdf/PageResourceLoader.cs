using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal sealed class PageResourceLoader
{
	private static PageResourceLoader s_instance;

	private static object s_lock = new object();

	private bool m_isExtractImages;

	internal Dictionary<string, PdfMatrix> m_commonMatrix;

	private object m_resourceLock = new object();

	private bool m_hasImages;

	internal string m_colorSpaceText = string.Empty;

	public static PageResourceLoader Instance
	{
		get
		{
			if (s_instance == null)
			{
				lock (s_lock)
				{
					s_instance = new PageResourceLoader();
				}
			}
			return s_instance;
		}
	}

	internal bool HasImages
	{
		get
		{
			return m_hasImages;
		}
		set
		{
			m_hasImages = value;
		}
	}

	public PdfPageResources GetPageResources(PdfPageBase page)
	{
		PdfPageResources pageResources = new PdfPageResources();
		m_isExtractImages = page.isExtractImages;
		float num = 0f;
		if (PdfDocument.EnableThreadSafe)
		{
			Monitor.Enter(m_resourceLock);
		}
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(page.Dictionary["Resources"]) as PdfDictionary;
		if (pdfDictionary == null && page.Dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(page.Dictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Resources"))
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary2["Resources"]) as PdfDictionary;
		}
		if (PdfDocument.EnableThreadSafe)
		{
			Monitor.Exit(m_resourceLock);
		}
		PdfArray pdfArray = page.ObtainAnnotations();
		Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
		pageResources = UpdatePageResources(pageResources, GetFontResources(pdfDictionary, page));
		pageResources = UpdatePageResources(pageResources, GetImageResources(pdfDictionary, page, ref commonMatrix));
		pageResources = UpdatePageResources(pageResources, GetExtendedGraphicResources(pdfDictionary));
		pageResources = UpdatePageResources(pageResources, GetColorSpaceResource(pdfDictionary));
		pageResources = UpdatePageResources(pageResources, GetPatternResource(pdfDictionary));
		pageResources = UpdatePageResources(pageResources, GetShadingResource(pdfDictionary));
		if (pdfArray != null)
		{
			pageResources.Add("Annotations", pdfArray);
		}
		while (pdfDictionary != null && pdfDictionary.ContainsKey("XObject"))
		{
			PdfDictionary obj = pdfDictionary;
			PdfDictionary pdfDictionary3 = ((!(pdfDictionary["XObject"] is PdfReferenceHolder)) ? (pdfDictionary["XObject"] as PdfDictionary) : ((pdfDictionary["XObject"] as PdfReferenceHolder).Object as PdfDictionary));
			pdfDictionary = pdfDictionary3["Resources"] as PdfDictionary;
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary3.Items)
			{
				PdfDictionary pdfDictionary4 = ((!(item.Value is PdfReferenceHolder)) ? (item.Value as PdfDictionary) : ((item.Value as PdfReferenceHolder).Object as PdfDictionary));
				if (pdfDictionary4 == null || !pdfDictionary4.ContainsKey("Resources"))
				{
					continue;
				}
				if (pdfDictionary4["Resources"] is PdfReferenceHolder)
				{
					PdfReferenceHolder pdfReferenceHolder = pdfDictionary4["Resources"] as PdfReferenceHolder;
					if (num == (float)pdfReferenceHolder.Reference.ObjNum)
					{
						continue;
					}
					pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
					num = pdfReferenceHolder.Reference.ObjNum;
				}
				else
				{
					pdfDictionary = pdfDictionary4["Resources"] as PdfDictionary;
				}
				if (pdfDictionary.Equals(obj))
				{
					pdfDictionary = null;
					obj = null;
				}
				pageResources = UpdatePageResources(pageResources, GetFontResources(pdfDictionary, page));
				pageResources = UpdatePageResources(pageResources, GetImageResources(pdfDictionary, page, ref commonMatrix));
			}
		}
		m_commonMatrix = commonMatrix;
		if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			pageResources.Add("Rotate", 90f);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
		{
			pageResources.Add("Rotate", 180f);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			pageResources.Add("Rotate", 270f);
		}
		return pageResources;
	}

	internal Dictionary<string, object> GetExtendedGraphicResources(PdfDictionary resourceDictionary)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null && resourceDictionary.ContainsKey("ExtGState"))
		{
			IPdfPrimitive pdfPrimitive = ((!(resourceDictionary["ExtGState"] is PdfDictionary)) ? (resourceDictionary["ExtGState"] as PdfReferenceHolder).Object : resourceDictionary["ExtGState"]);
			if (pdfPrimitive is PdfDictionary)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in ((PdfDictionary)pdfPrimitive).Items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						PdfDictionary xobjectDictionary = (item.Value as PdfReferenceHolder).Object as PdfDictionary;
						dictionary.Add(item.Key.Value, new XObjectElement(xobjectDictionary, item.Key.Value));
					}
					else
					{
						PdfDictionary xobjectDictionary = item.Value as PdfDictionary;
						dictionary.Add(item.Key.Value, new XObjectElement(xobjectDictionary, item.Key.Value));
					}
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetColorSpaceResource(PdfDictionary resourceDic)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDic != null && resourceDic.ContainsKey("ColorSpace"))
		{
			IPdfPrimitive pdfPrimitive = ((!(resourceDic["ColorSpace"] is PdfDictionary)) ? (resourceDic["ColorSpace"] as PdfReferenceHolder).Object : (resourceDic["ColorSpace"] as PdfDictionary));
			if (pdfPrimitive is PdfDictionary)
			{
				Dictionary<PdfName, IPdfPrimitive> items = ((PdfDictionary)pdfPrimitive).Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						IPdfPrimitive @object = (item.Value as PdfReferenceHolder).Object;
						if (@object is PdfArray pdfArray)
						{
							for (int i = 0; i < pdfArray.Count; i++)
							{
								if (pdfArray.Elements[i] is PdfString)
								{
									PdfString pdfString = pdfArray.Elements[i] as PdfString;
									m_colorSpaceText = pdfString.Value;
									if (Regex.IsMatch(m_colorSpaceText, "^[A-Za-z]*"))
									{
										Match match = Regex.Match(m_colorSpaceText, "^[A-Za-z]*");
										m_colorSpaceText = match.Value;
									}
								}
							}
							dictionary.Add(item.Key.Value, new ExtendColorspace(pdfArray));
						}
						else if (@object is PdfName)
						{
							PdfName refHolderColorspace = @object as PdfName;
							dictionary.Add(item.Key.Value, new ExtendColorspace(refHolderColorspace));
						}
					}
					if (item.Value is PdfName)
					{
						PdfName refHolderColorspace2 = item.Value as PdfName;
						dictionary.Add(item.Key.Value, new ExtendColorspace(refHolderColorspace2));
					}
					if (item.Value is PdfArray refHolderColorspace3)
					{
						dictionary.Add(item.Key.Value, new ExtendColorspace(refHolderColorspace3));
					}
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetShadingResource(PdfDictionary resourceDictionary)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null && resourceDictionary.ContainsKey("Shading") && resourceDictionary["Shading"] is PdfDictionary pdfDictionary)
		{
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
			{
				PdfReferenceHolder pdfReferenceHolder = item.Value as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					dictionary.Add(item.Key.Value, new ExtendColorspace(pdfReferenceHolder.Object));
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetPatternResource(PdfDictionary resourceDictionary)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null && resourceDictionary.ContainsKey("Pattern"))
		{
			IPdfPrimitive pdfPrimitive = ((!(resourceDictionary["Pattern"] is PdfDictionary)) ? (resourceDictionary["Pattern"] as PdfReferenceHolder).Object : (resourceDictionary["Pattern"] as PdfDictionary));
			if (pdfPrimitive is PdfDictionary)
			{
				Dictionary<PdfName, IPdfPrimitive> items = ((PdfDictionary)pdfPrimitive).Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						IPdfPrimitive pdfPrimitive2 = (item.Value as PdfReferenceHolder).Object as PdfArray;
						if (pdfPrimitive2 != null)
						{
							dictionary.Add(item.Key.Value, new ExtendColorspace(pdfPrimitive2 as PdfArray));
						}
						pdfPrimitive2 = (item.Value as PdfReferenceHolder).Object as PdfDictionary;
						if (pdfPrimitive2 != null)
						{
							dictionary.Add(item.Key.Value, new ExtendColorspace(pdfPrimitive2 as PdfDictionary));
						}
					}
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetFontResources(PdfDictionary resourceDictionary)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null)
		{
			IPdfPrimitive pdfPrimitive = resourceDictionary["Font"];
			if (pdfPrimitive != null)
			{
				PdfDictionary pdfDictionary = ((!(pdfPrimitive is PdfReferenceHolder)) ? (pdfPrimitive as PdfDictionary) : ((pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary));
				if (pdfDictionary != null)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
					{
						if (item.Value is PdfReferenceHolder)
						{
							if ((item.Value as PdfReferenceHolder).Reference != null)
							{
								dictionary.Add(item.Key.Value, new FontStructure((item.Value as PdfReferenceHolder).Object, (item.Value as PdfReferenceHolder).Reference.ToString()));
							}
							else
							{
								dictionary.Add(item.Key.Value, new FontStructure((item.Value as PdfReferenceHolder).Object));
							}
						}
						else if (item.Value is PdfDictionary fontDictionary)
						{
							dictionary.Add(item.Key.Value, new FontStructure(fontDictionary));
						}
						else
						{
							dictionary.Add(item.Key.Value, new FontStructure(item.Value, (item.Value as PdfReferenceHolder).Reference.ToString()));
						}
					}
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetFontResources(PdfDictionary resourceDictionary, PdfPageBase page)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null)
		{
			IPdfPrimitive pdfPrimitive = resourceDictionary["Font"];
			if (pdfPrimitive != null)
			{
				PdfDictionary pdfDictionary = ((!(pdfPrimitive is PdfReferenceHolder)) ? (pdfPrimitive as PdfDictionary) : ((pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary));
				if (pdfDictionary != null)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
					{
						if (item.Value is PdfReferenceHolder)
						{
							if ((item.Value as PdfReferenceHolder).Reference != null)
							{
								if (!(PdfCrossTable.Dereference(item.Value) is PdfNull))
								{
									dictionary.Add(item.Key.Value, new FontStructure((item.Value as PdfReferenceHolder).Object, (item.Value as PdfReferenceHolder).Reference.ToString()));
								}
							}
							else
							{
								dictionary.Add(item.Key.Value, new FontStructure((item.Value as PdfReferenceHolder).Object));
							}
						}
						else if (item.Value is PdfDictionary fontDictionary)
						{
							dictionary.Add(item.Key.Value, new FontStructure(fontDictionary));
						}
						else
						{
							dictionary.Add(item.Key.Value, new FontStructure(item.Value, (item.Value as PdfReferenceHolder).Reference.ToString()));
						}
					}
				}
			}
			IPdfPrimitive pdfPrimitive2 = page.Dictionary["Parent"];
			if (pdfPrimitive2 != null)
			{
				pdfPrimitive = new PdfResources((pdfPrimitive2 as PdfReferenceHolder).Object as PdfDictionary)["Font"];
				if (pdfPrimitive != null && pdfPrimitive is PdfDictionary pdfDictionary2)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary2.Items)
					{
						if (item2.Value is PdfDictionary)
						{
							dictionary.Add(item2.Key.Value, (item2.Value as PdfReferenceHolder).Object);
						}
						dictionary.Add(item2.Key.Value, new FontStructure(item2.Value, (item2.Value as PdfReferenceHolder).Reference.ToString()));
					}
				}
			}
		}
		return dictionary;
	}

	internal Dictionary<string, object> GetImageResources(PdfDictionary resourceDictionary, PdfPageBase page, ref Dictionary<string, PdfMatrix> commonMatrix)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (resourceDictionary != null && resourceDictionary.ContainsKey("XObject"))
		{
			IPdfPrimitive pdfPrimitive = ((!(resourceDictionary["XObject"] is PdfDictionary)) ? (resourceDictionary["XObject"] as PdfReferenceHolder).Object : resourceDictionary["XObject"]);
			if (pdfPrimitive is PdfDictionary)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in ((PdfDictionary)pdfPrimitive).Items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						if (!((item.Value as PdfReferenceHolder).Object is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Subtype"))
						{
							continue;
						}
						if (!((pdfDictionary["Subtype"] as PdfName).Value == "Image") && (pdfDictionary["Subtype"] as PdfName).Value == "Form" && page != null)
						{
							if (pdfDictionary.ContainsKey("Resources") && pdfDictionary["Resources"] is PdfDictionary)
							{
								foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in (pdfDictionary["Resources"] as PdfDictionary).Items)
								{
									if (!(item2.Key.Value == "XObject") || !(item2.Value is PdfDictionary))
									{
										continue;
									}
									Dictionary<PdfName, IPdfPrimitive> items = (item2.Value as PdfDictionary).Items;
									if (!m_isExtractImages)
									{
										continue;
									}
									foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in items)
									{
										if (!commonMatrix.ContainsKey(item3.Key.Value))
										{
											PdfMatrix value = new PdfMatrix(new PdfReader(new MemoryStream((pdfDictionary as PdfStream).GetDecompressedData()))
											{
												Position = 0L
											}, item3.Key.Value, page.Size);
											commonMatrix.Add(item3.Key.Value, value);
										}
									}
								}
							}
							dictionary.Add(item.Key.Value, new XObjectElement(pdfDictionary, item.Key.Value));
						}
						if (!dictionary.ContainsKey(item.Key.Value))
						{
							dictionary.Add(item.Key.Value, new XObjectElement(pdfDictionary, item.Key.Value));
							m_hasImages = true;
						}
					}
					else if (item.Value is PdfDictionary xobjectDictionary)
					{
						dictionary.Add(item.Key.Value, new XObjectElement(xobjectDictionary, item.Key.Value));
						m_hasImages = true;
					}
				}
			}
		}
		return dictionary;
	}

	internal PdfPageResources UpdatePageResources(PdfPageResources pageResources, Dictionary<string, object> objects)
	{
		foreach (KeyValuePair<string, object> @object in objects)
		{
			pageResources.Add(@object.Key, @object.Value);
		}
		return pageResources;
	}
}

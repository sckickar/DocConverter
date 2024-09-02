using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Exporting;

internal class ImageExportHelperNet
{
	internal bool isFlatten;

	internal bool m_removedPage;

	internal bool isExtractImages;

	internal bool isFlateCompress;

	private bool m_modified;

	private Dictionary<int, string> m_imageLengthDict = new Dictionary<int, string>();

	private bool m_isContainsImage;

	private int m_nonBreakingSpaceCharValue = 160;

	private long resourceNumber;

	internal List<RectangleF> m_RedactionBounds = new List<RectangleF>();

	internal bool is_Contains_Redaction;

	private float pt = 1.3333f;

	internal PdfArray m_childSTR = new PdfArray();

	internal int m_id;

	private List<PdfMatrix> extractedImageMatrix = new List<PdfMatrix>();

	private List<bool> maskImageCollection = new List<bool>();

	private PageResourceLoader m_resourceLoader = new PageResourceLoader();

	private List<RectangleF> m_extractedImagesBounds = new List<RectangleF>();

	internal PdfImageInfo[] m_imageinfo;

	internal PdfRecordCollection m_recordCollection;

	internal List<IPdfPrimitive> m_image_reference = new List<IPdfPrimitive>();

	private List<PdfName> m_image_Names = new List<PdfName>();

	internal PdfPageResourcesHelper m_pageResources;

	private char[] m_symbolChars = new char[6] { '(', ')', '[', ']', '<', '>' };

	internal ArrayList extractedImages = new ArrayList();

	private List<PdfImageInfo> m_imageInfoList = new List<PdfImageInfo>();

	private List<string> m_ImageKey = new List<string>();

	internal Stack<GraphicsStateDataNet> m_currentMatrix = new Stack<GraphicsStateDataNet>();

	internal bool isRotate;

	internal Dictionary<PdfName, List<float>> m_roatedImages = new Dictionary<PdfName, List<float>>();

	internal bool isExtractImageInfo;

	private PdfPageBase m_page;

	private PdfArray Contents => m_page.Contents;

	private PdfPageRotateAngle Rotation => m_page.Rotation;

	private SizeF Size => m_page.Size;

	private PdfPageLayerCollection Layers => m_page.Layers;

	internal PdfImageInfo[] ImagesInfo
	{
		get
		{
			if (m_imageinfo == null)
			{
				try
				{
					int num = 0;
					ExtractImages(isImageExtraction: true);
					IEnumerator enumerator = m_extractedImagesBounds.GetEnumerator();
					while (enumerator.MoveNext())
					{
						m_imageinfo[num].Bounds = (RectangleF)enumerator.Current;
						m_imageinfo[num].Image = null;
						m_imageinfo[num].Index = num;
						num++;
						PdfArray pdfArray = null;
						if (m_page != null && m_page.Dictionary.ContainsKey("CropBox"))
						{
							pdfArray = PdfCrossTable.Dereference(m_page.Dictionary["CropBox"]) as PdfArray;
						}
						else if (m_page != null && m_page.Dictionary.ContainsKey("MediaBox"))
						{
							pdfArray = PdfCrossTable.Dereference(m_page.Dictionary["MediaBox"]) as PdfArray;
						}
						if (pdfArray != null && pdfArray.Count > 3)
						{
							PdfNumber pdfNumber = pdfArray[0] as PdfNumber;
							PdfNumber pdfNumber2 = pdfArray[1] as PdfNumber;
							if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
							{
								RectangleF bounds = m_imageinfo[num - 1].Bounds;
								bounds.X = 0f - pdfNumber.FloatValue - (0f - bounds.X);
								bounds.Y = pdfNumber2.FloatValue + bounds.Y;
								m_imageinfo[num - 1].Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
							}
						}
					}
				}
				catch (Exception)
				{
				}
				ClearImageResources();
				return m_imageinfo;
			}
			return m_imageinfo;
		}
	}

	private void ClearImageResources()
	{
		if (m_pageResources != null)
		{
			m_pageResources.Resources.Clear();
		}
		m_pageResources = null;
		if (m_recordCollection != null)
		{
			m_recordCollection.RecordCollection.Clear();
		}
		m_recordCollection = null;
	}

	internal void ReplaceImage(int imageIndex, PdfImage image, PdfPageBase page)
	{
		if (imageIndex < 0)
		{
			throw new ArgumentException("Image index is not valid");
		}
		if (image == null)
		{
			throw new NullReferenceException("image");
		}
		m_modified = true;
		try
		{
			PdfImageInfo[] imagesInfo = ImagesInfo;
			image.Save();
			PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(image);
			PdfResources resources = GetResources();
			for (int i = 0; i < imagesInfo.Length; i++)
			{
				if (!m_imageLengthDict.ContainsKey(imagesInfo[i].Index))
				{
					m_imageLengthDict.Add(imagesInfo[i].Index, imagesInfo[i].Name);
				}
			}
			if (!m_imageLengthDict.ContainsKey(imageIndex))
			{
				throw new ArgumentException("Image Index is not valid");
			}
			if (!resources.ContainsKey("XObject") || (!(resources["XObject"] is PdfDictionary) && !(resources["XObject"] is PdfReferenceHolder)))
			{
				return;
			}
			PdfDictionary pdfDictionary = resources["XObject"] as PdfDictionary;
			if (resources["XObject"] is PdfReferenceHolder && pdfDictionary == null)
			{
				pdfDictionary = (resources["XObject"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			if (pdfDictionary == null || pdfDictionary == null)
			{
				return;
			}
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
			{
				bool imageReplaced = false;
				int width = imagesInfo[imageIndex].Image.Width;
				int height = imagesInfo[imageIndex].Image.Height;
				KeyValuePair<PdfName, IPdfPrimitive> current = FetchImageDictionary(item, imagesInfo[imageIndex].Name, new SizeF(width, height), out imageReplaced);
				PdfDictionary pdfDictionary3;
				if (current.Value is PdfReferenceHolder)
				{
					pdfDictionary3 = (current.Value as PdfReferenceHolder).Object as PdfDictionary;
					PdfDictionary pdfDictionary4 = new PdfDictionary();
					PdfDictionary.SetProperty(pdfDictionary4, current.Key.Value, current.Value);
					pdfDictionary = pdfDictionary4;
				}
				else
				{
					pdfDictionary3 = current.Value as PdfDictionary;
					pdfDictionary = pdfDictionary3;
				}
				if (pdfDictionary3.ContainsKey("Subtype") && imageReplaced && (pdfDictionary3["Subtype"] as PdfName).Value == "Image")
				{
					_ = resources["XObject"];
					string text = current.Key.ToString();
					text = StripSlashes(text);
					ImageStructureNet imageStructureNet = new ImageStructureNet(pdfDictionary3, new PdfMatrix());
					SizeF sizeF = new SizeF(imageStructureNet.Width, imageStructureNet.Height);
					if (imageStructureNet.ImageDictionary.ContainsKey("SMask") && imageStructureNet.EmbeddedImage != null && (float)imageStructureNet.EmbeddedImage.Height > sizeF.Height && (float)imageStructureNet.EmbeddedImage.Width > sizeF.Width)
					{
						sizeF = new Size(imageStructureNet.EmbeddedImage.Width, imageStructureNet.EmbeddedImage.Height);
					}
					if (imagesInfo[imageIndex].Name == text && (((float)width == sizeF.Width && (float)height == sizeF.Height) || m_roatedImages.ContainsKey(new PdfName(text))))
					{
						PdfImageInfo[] array = imagesInfo;
						int num = 0;
						if (num < array.Length)
						{
							_ = array[num];
							imageReplaced = true;
							if (imagesInfo[imageIndex].Image != null)
							{
								long objNum = (pdfDictionary[text] as PdfReferenceHolder).Reference.ObjNum;
								int num2 = 0;
								foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary2.Items)
								{
									if (num2 == pdfDictionary2.Count - 1)
									{
										pdfDictionary = pdfDictionary2[item2.Key.Value] as PdfDictionary;
									}
									num2++;
								}
								pdfDictionary2.Clear();
								if ((current.Value as PdfReferenceHolder).Object is PdfStream)
								{
									PdfStream pdfStream = (current.Value as PdfReferenceHolder).Object as PdfStream;
									if (page is PdfPage)
									{
										if ((page as PdfPage).Document.FileStructure.IncrementalUpdate)
										{
											pdfStream.Modify();
											pdfStream.Clear();
										}
										else
										{
											pdfStream.Clear();
										}
									}
									else if (page is PdfLoadedPage)
									{
										pdfStream.Clear();
									}
								}
								float num3 = height;
								float top = imagesInfo[imageIndex].Bounds.Top;
								if (Size.Height - (top + num3) >= 0f)
								{
									if (page is PdfPage)
									{
										pdfDictionary.Items.Remove(current.Key);
										pdfDictionary.Items.Add(current.Key, pdfReferenceHolder);
										pdfDictionary.Modify();
									}
									else if (page is PdfLoadedPage)
									{
										ReplaceImageStream(objNum, pdfReferenceHolder, page);
									}
								}
								else if (page is PdfPage)
								{
									pdfDictionary.Items.Remove(current.Key);
									pdfDictionary.Items.Add(current.Key, pdfReferenceHolder);
									pdfDictionary.Modify();
								}
								else if (page is PdfLoadedPage)
								{
									ReplaceImageStream(objNum, pdfReferenceHolder, page);
								}
							}
							else
							{
								long objNum2 = (current.Value as PdfReferenceHolder).Reference.ObjNum;
								if ((current.Value as PdfReferenceHolder).Object is PdfStream)
								{
									PdfStream pdfStream2 = (current.Value as PdfReferenceHolder).Object as PdfStream;
									if (page is PdfPage)
									{
										if ((page as PdfPage).Document.FileStructure.IncrementalUpdate)
										{
											pdfStream2.Modify();
											pdfStream2.Clear();
										}
										else
										{
											pdfStream2.Clear();
										}
									}
									else if (page is PdfLoadedPage)
									{
										pdfStream2.Clear();
									}
								}
								float height2 = imagesInfo[imageIndex].Bounds.Height;
								float top2 = imagesInfo[imageIndex].Bounds.Top;
								if (Size.Height - (top2 + height2) >= 0f)
								{
									if (page is PdfPage)
									{
										pdfDictionary.Items.Remove(current.Key);
										pdfDictionary.Items.Add(current.Key, pdfReferenceHolder);
										pdfDictionary.Modify();
									}
									else if (page is PdfLoadedPage)
									{
										ReplaceImageStream(objNum2, pdfReferenceHolder, page);
									}
								}
								else if (page is PdfPage)
								{
									pdfDictionary.Items.Remove(current.Key);
									pdfDictionary.Items.Add(current.Key, pdfReferenceHolder);
									pdfDictionary.Modify();
								}
								else if (page is PdfLoadedPage)
								{
									ReplaceImageStream(objNum2, pdfReferenceHolder, page);
								}
							}
						}
					}
				}
				if (imageReplaced)
				{
					pdfDictionary = null;
					m_imageLengthDict.Clear();
					break;
				}
			}
		}
		catch (Exception ex)
		{
			if (ex is ArgumentException)
			{
				throw ex;
			}
		}
	}

	private string StripSlashes(string text)
	{
		return text.Replace("/", "");
	}

	private KeyValuePair<PdfName, IPdfPrimitive> FetchImageDictionary(KeyValuePair<PdfName, IPdfPrimitive> item, string ImageName, SizeF sizeImage, out bool imageReplaced)
	{
		imageReplaced = false;
		KeyValuePair<PdfName, IPdfPrimitive> result = item;
		new PdfDictionary();
		PdfDictionary pdfDictionary = ((!(item.Value is PdfReferenceHolder)) ? (item.Value as PdfDictionary) : ((item.Value as PdfReferenceHolder).Object as PdfDictionary));
		if (pdfDictionary.ContainsKey("Subtype") && (pdfDictionary["Subtype"] as PdfName).Value == "Image")
		{
			string text = item.Key.ToString();
			text = StripSlashes(text);
			ImageStructureNet imageStructureNet = new ImageStructureNet(pdfDictionary, new PdfMatrix());
			SizeF sizeF = new SizeF(imageStructureNet.Width, imageStructureNet.Height);
			if (imageStructureNet.ImageDictionary.ContainsKey("SMask") && imageStructureNet.EmbeddedImage != null && (float)imageStructureNet.EmbeddedImage.Height > sizeF.Height && (float)imageStructureNet.EmbeddedImage.Width > sizeF.Width)
			{
				sizeF = new Size(imageStructureNet.EmbeddedImage.Width, imageStructureNet.EmbeddedImage.Height);
			}
			bool flag = false;
			if (m_roatedImages.Count != 0 && m_roatedImages.ContainsKey(new PdfName(ImageName)))
			{
				flag = true;
			}
			if (text == ImageName && (sizeF == sizeImage || flag) && (imageStructureNet.ImageFilter == null || imageStructureNet.ImageFilter.Length <= 1 || imageStructureNet.ImageDictionary.ContainsKey("DecodeParms")) && (imageStructureNet.ImageFilter == null || !(imageStructureNet.ImageFilter[0] == "CCITTFaxDecode") || !imageStructureNet.ImageDictionary.ContainsKey("ImageMask")))
			{
				imageReplaced = true;
				result = item;
			}
		}
		if (pdfDictionary.ContainsKey("Resources"))
		{
			pdfDictionary = ((!(pdfDictionary["Resources"] is PdfReferenceHolder)) ? (pdfDictionary["Resources"] as PdfDictionary) : ((pdfDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary));
			if (pdfDictionary.ContainsKey("XObject"))
			{
				m_isContainsImage = true;
				PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["XObject"]) as PdfDictionary;
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Subtype"))
				{
					KeyValuePair<PdfName, IPdfPrimitive> item2 = new KeyValuePair<PdfName, IPdfPrimitive>(item.Key, pdfDictionary2);
					result = FetchImageDictionary(item2, ImageName, sizeImage, out imageReplaced);
				}
				else
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in pdfDictionary2.Items)
					{
						result = FetchImageDictionary(item3, ImageName, sizeImage, out imageReplaced);
						if (imageReplaced)
						{
							break;
						}
					}
				}
			}
		}
		return result;
	}

	internal void ReplaceImageStream(long objIndex, PdfReferenceHolder imageReference, PdfPageBase currentpage)
	{
		PdfCrossTable pdfCrossTable = ((currentpage is PdfPage) ? (currentpage as PdfPage).CrossTable : (currentpage as PdfLoadedPage).CrossTable);
		if (pdfCrossTable == null)
		{
			pdfCrossTable = ((currentpage as PdfLoadedPage).Document as PdfLoadedDocument).CrossTable;
		}
		PdfReference pointer = new PdfReference(objIndex, 0);
		PdfDictionary pdfDictionary = pdfCrossTable.GetObject(pointer) as PdfDictionary;
		IPdfPrimitive pdfPrimitive = null;
		if (pdfDictionary.ContainsKey("SMask"))
		{
			pdfPrimitive = pdfDictionary["SMask"];
		}
		pdfDictionary.Clear();
		PdfDictionary obj = imageReference.Object as PdfDictionary;
		PdfStream pdfStream = imageReference.Object as PdfStream;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in obj.Items)
		{
			bool flag = false;
			if (item.Key.Value == "SMask" && pdfPrimitive != null)
			{
				IPdfPrimitive pdfPrimitive2 = PdfCrossTable.Dereference(pdfPrimitive);
				IPdfPrimitive pdfPrimitive3 = PdfCrossTable.Dereference(item.Value);
				if (pdfPrimitive2 != null && pdfPrimitive2 is PdfDictionary && pdfPrimitive3 != null && pdfPrimitive3 is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = pdfPrimitive2 as PdfDictionary;
					if (!pdfDictionary2.isSkip)
					{
						pdfDictionary2.Clear();
						PdfDictionary pdfDictionary3 = pdfPrimitive3 as PdfDictionary;
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary3.Items)
						{
							pdfDictionary2.Items.Add(item2.Key, item2.Value);
						}
						pdfDictionary2.Modify();
						PdfStream pdfStream2 = pdfDictionary2 as PdfStream;
						if (isFlateCompress)
						{
							if (pdfDictionary2.ContainsKey("DecodeParms"))
							{
								if (pdfDictionary2["DecodeParms"] is PdfDictionary element)
								{
									PdfArray pdfArray = new PdfArray();
									pdfArray.Add(new PdfDictionary());
									pdfArray.Add(element);
									pdfDictionary2.SetProperty("DecodeParms", pdfArray);
									pdfStream2.Compress = true;
									pdfStream2.isImageDualFilter = true;
								}
								else
								{
									pdfStream2.Compress = false;
								}
							}
							else
							{
								pdfStream2.Compress = false;
							}
						}
						else if (pdfDictionary2.ContainsKey("Filter"))
						{
							pdfStream2.Compress = false;
						}
						else
						{
							pdfStream2.Compress = true;
						}
						pdfStream2.Data = (pdfDictionary3 as PdfStream).Data;
						pdfDictionary.Items.Add(item.Key, pdfPrimitive);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				pdfDictionary.Items.Add(item.Key, item.Value);
			}
		}
		pdfDictionary.Modify();
		PdfStream pdfStream3 = pdfDictionary as PdfStream;
		if (isFlateCompress)
		{
			if (pdfDictionary.ContainsKey("DecodeParms"))
			{
				if (pdfDictionary["DecodeParms"] is PdfDictionary element2)
				{
					PdfArray pdfArray2 = new PdfArray();
					pdfArray2.Add(new PdfDictionary());
					pdfArray2.Add(element2);
					pdfDictionary.SetProperty("DecodeParms", pdfArray2);
					pdfStream3.Compress = true;
					pdfStream3.isImageDualFilter = true;
				}
				else
				{
					pdfStream3.Compress = false;
				}
			}
			else
			{
				pdfStream3.Compress = false;
			}
		}
		else if (pdfDictionary.ContainsKey("Filter"))
		{
			pdfStream3.Compress = false;
		}
		else
		{
			pdfStream3.Compress = true;
		}
		pdfStream3.Data = pdfStream.Data;
	}

	private Dictionary<string, object> CopyResources(Dictionary<string, object> resources)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object> resource in resources)
		{
			dictionary.Add(resource.Key, resource.Value);
		}
		return dictionary;
	}

	private void UpdateResources(PageResourceLoader loader)
	{
		if (m_pageResources == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in CopyResources(m_pageResources.Resources))
		{
			if (item.Value is XObjectElement { ObjectType: "Image" } xObjectElement)
			{
				ImageStructureNet value = new ImageStructureNet(xObjectElement.XObjectDictionary, new PdfMatrix());
				if (loader.m_commonMatrix.ContainsKey(xObjectElement.ObjectName))
				{
					value = new ImageStructureNet(xObjectElement.XObjectDictionary, loader.m_commonMatrix[xObjectElement.ObjectName]);
				}
				m_pageResources[xObjectElement.ObjectName] = value;
			}
		}
	}

	internal PdfImageInfo[] ExtractImages(bool isImageExtraction)
	{
		isExtractImages = true;
		float internalRotation = 0f;
		_ = Contents;
		new PdfDictionary();
		PdfStream pdfStream = null;
		string empty = string.Empty;
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		RectangleF rectangleF = default(RectangleF);
		Matrix matrix = null;
		PageResourceLoader instance = PageResourceLoader.Instance;
		m_pageResources = instance.GetPageResources(m_page);
		UpdateResources(instance);
		PdfStream pdfStream2 = null;
		GraphicsStateDataNet graphicsStateDataNet = new GraphicsStateDataNet();
		graphicsStateDataNet.m_drawing2dMatrixCTM = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		m_currentMatrix.Push(graphicsStateDataNet);
		ContentParser contentParser = null;
		if (GetResources().ContainsKey("XObject"))
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				CombineContent(memoryStream);
				memoryStream.Position = 0L;
				contentParser = new ContentParser(memoryStream.ToArray());
				m_recordCollection = contentParser.ReadContent();
			}
			for (int i = 0; i < m_recordCollection.RecordCollection.Count; i++)
			{
				string text = m_recordCollection.RecordCollection[i].OperatorName;
				string[] operands = m_recordCollection.RecordCollection[i].Operands;
				char[] symbolChars = m_symbolChars;
				for (int j = 0; j < symbolChars.Length; j++)
				{
					char c = symbolChars[j];
					if (text.Contains(c.ToString()))
					{
						text = text.Replace(c.ToString(), "");
					}
				}
				switch (text.Trim())
				{
				case "q":
				{
					GraphicsStateDataNet graphicsStateDataNet2 = new GraphicsStateDataNet();
					if (m_currentMatrix.Count > 0)
					{
						GraphicsStateDataNet graphicsStateDataNet3 = m_currentMatrix.Peek();
						graphicsStateDataNet2.m_drawing2dMatrixCTM = graphicsStateDataNet3.m_drawing2dMatrixCTM;
					}
					m_currentMatrix.Push(graphicsStateDataNet2);
					break;
				}
				case "cm":
				{
					float m = float.Parse(operands[0]);
					float m2 = float.Parse(operands[1]);
					float m3 = float.Parse(operands[2]);
					float m4 = float.Parse(operands[3]);
					float dx = float.Parse(operands[4]);
					float dy = float.Parse(operands[5]);
					matrix = new Matrix(m, m2, m3, m4, dx, dy);
					m_currentMatrix.Peek().m_drawing2dMatrixCTM = Multiply(matrix, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
					break;
				}
				case "Do":
				{
					ImageStructureNet imageStructureNet = null;
					PdfMatrix pdfMatrix = null;
					PdfReader pdfReader = null;
					empty = operands[0].Replace("/", "");
					if (m_pageResources.Resources.Count > 0 && !(m_pageResources.Resources[operands[0].Replace("/", "")] is ImageStructureNet))
					{
						XObjectElement xObjectElement = m_pageResources[operands[0].Replace("/", "")] as XObjectElement;
						PdfStream pdfStream3 = xObjectElement.XObjectDictionary as PdfStream;
						pdfStream3.Decompress();
						PdfDictionary pdfDictionary = new PdfDictionary();
						PdfPageResourcesHelper pdfPageResourcesHelper = new PdfPageResourcesHelper();
						if (xObjectElement.XObjectDictionary.ContainsKey("Resources"))
						{
							pdfDictionary = ((xObjectElement.XObjectDictionary["Resources"] is PdfReference) ? ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectElement.XObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectElement.XObjectDictionary["Resources"] as PdfDictionary) : ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
							Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetFontResources(pdfDictionary));
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetExtendedGraphicResources(pdfDictionary));
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetColorSpaceResource(pdfDictionary));
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetShadingResource(pdfDictionary));
							pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetPatternResource(pdfDictionary));
						}
						ExtractInnerXObjectImages(pdfStream3, pdfPageResourcesHelper);
						break;
					}
					if (m_pageResources.Resources.Count > 0 && m_pageResources.Resources[operands[0].Replace("/", "")] is ImageStructureNet)
					{
						imageStructureNet = m_pageResources.Resources[operands[0].Replace("/", "")] as ImageStructureNet;
					}
					if (imageStructureNet == null)
					{
						break;
					}
					try
					{
						PdfDictionary imageDictionary = imageStructureNet.ImageDictionary;
						if (imageDictionary != null)
						{
							PdfArray pdfArray = null;
							PdfArray pdfArray2 = null;
							if (imageDictionary["ColorSpace"] is PdfArray)
							{
								pdfArray = imageDictionary["ColorSpace"] as PdfArray;
							}
							if (imageDictionary["ColorSpace"] is PdfReferenceHolder)
							{
								pdfArray = (imageDictionary["ColorSpace"] as PdfReferenceHolder).Object as PdfArray;
							}
							if (pdfArray != null && pdfArray[1] is PdfReferenceHolder)
							{
								pdfArray2 = (pdfArray[1] as PdfReferenceHolder).Object as PdfArray;
							}
							if (pdfArray2 != null && pdfArray2[1] is PdfReferenceHolder)
							{
								_ = (pdfArray2[1] as PdfReferenceHolder).Object;
							}
						}
						pdfStream2 = imageStructureNet.ImageDictionary as PdfStream;
						pdfMatrix = imageStructureNet.ImageInfo;
						for (int k = 0; k < Contents.Count; k++)
						{
							PdfStream pdfStream4 = ((pdfStream == null) ? ((Contents[k] as PdfReferenceHolder).Object as PdfStream) : pdfStream);
							pdfStream4.Decompress();
							MemoryStream internalStream = pdfStream4.InternalStream;
							pdfReader = new PdfReader(internalStream);
							if (pdfReader.ReadStream().Contains(empty))
							{
								internalStream.Position = 0L;
								pdfReader.Position = 0L;
								pdfMatrix = ((Rotation != PdfPageRotateAngle.RotateAngle90) ? new PdfMatrix(pdfReader, empty, Size) : new PdfMatrix(pdfReader, empty, Size));
								break;
							}
						}
						bool item = false;
						if (imageDictionary.ContainsKey("Mask"))
						{
							item = true;
						}
						if (pdfStream2.ContainsKey("Width"))
						{
							if (pdfStream2["Width"] is PdfNumber)
							{
								_ = (pdfStream2["Width"] as PdfNumber).IntValue;
							}
							if (pdfStream2["Width"] is PdfReferenceHolder)
							{
								_ = ((pdfStream2["Width"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
							}
							if (pdfStream2["Height"] is PdfNumber)
							{
								_ = (pdfStream2["Height"] as PdfNumber).IntValue;
							}
							if (pdfStream2["Height"] is PdfReferenceHolder)
							{
								_ = ((pdfStream2["Height"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
							}
						}
						else if (pdfStream2.ContainsKey("BBox"))
						{
							PdfArray obj = pdfStream2["BBox"] as PdfArray;
							_ = (obj[2] as PdfNumber).IntValue;
							_ = (obj[3] as PdfNumber).IntValue;
						}
						Matrix matrix2 = new Matrix(1f, 0f, 0f, -1.01f, 0f, 1f);
						Matrix matrix3 = Multiply(matrix2, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
						Matrix matrix4 = new Matrix(1.3333334f, 0f, 0f, -1.3333334f, 0f, Size.Height * 1.3333334f);
						matrix3 = Multiply(matrix3, matrix4);
						if (Rotation == PdfPageRotateAngle.RotateAngle270)
						{
							rectangleF = ((matrix3.Elements[0] == 0f || matrix3.Elements[3] == 0f) ? new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), (float)Math.Floor(Size.Width) - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + (float)Math.Floor(matrix3.Elements[0] / 1.3333334f)), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2])) : new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), Size.Width - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + matrix3.Elements[0] / 1.3333334f), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
						}
						else if (Rotation == PdfPageRotateAngle.RotateAngle90)
						{
							if (matrix3.Elements[0] == 0f && matrix3.Elements[3] == 0f)
							{
								rectangleF = new RectangleF(new PointF(Size.Height - matrix3.Elements[5] / 1.3333334f, matrix3.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2]));
								internalRotation = 90f;
							}
							else
							{
								rectangleF = new RectangleF(new PointF(Size.Height - matrix3.OffsetY / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], matrix3.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
							}
						}
						else if (Rotation == PdfPageRotateAngle.RotateAngle180)
						{
							rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
						else if (matrix3.Elements[0] == 0f && matrix3.Elements[3] == 0f)
						{
							if (matrix3.Elements[1] < 0f && matrix3.Elements[2] > 0f)
							{
								internalRotation = 270f;
								rectangleF = new RectangleF(Size.Height - matrix3.Elements[5] / 1.3333334f, (float)Math.Round(matrix3.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
							}
							else if (matrix3.Elements[1] > 0f && matrix3.Elements[2] < 0f)
							{
								internalRotation = 90f;
								rectangleF = new RectangleF(matrix3.Elements[5] / 1.3333334f, (float)Math.Round(Size.Width - matrix3.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
							}
							else if (matrix3.Elements[1] < 0f && matrix3.Elements[2] < 0f)
							{
								internalRotation = 180f;
								rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
							}
							else
							{
								rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
							}
						}
						else
						{
							rectangleF = new RectangleF(matrix3.OffsetX / 1.3333334f, (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
						bool flag = false;
						if (is_Contains_Redaction)
						{
							RectangleF b = rectangleF;
							b.X *= pt;
							b.Y *= pt;
							b.Width *= pt;
							b.Height *= pt;
							for (int l = 0; l < m_RedactionBounds.Count; l++)
							{
								RectangleF rectangleF2 = m_RedactionBounds[l];
								if (RectangleF.Intersect(new RectangleF(rectangleF2.X * pt, rectangleF2.Y * pt, rectangleF2.Width * pt, rectangleF2.Height * pt), b) != RectangleF.Empty)
								{
									flag = true;
								}
							}
						}
						if (!is_Contains_Redaction || flag)
						{
							PdfImageInfo pdfImageInfo = new PdfImageInfo();
							pdfImageInfo.Name = operands[0].Replace("/", "").ToString();
							pdfImageInfo.Metadata = GetMetadata(pdfStream2);
							imageStructureNet.IsImageForExtraction = true;
							pdfImageInfo.m_isImageMasked = imageStructureNet.IsImageMasked;
							pdfImageInfo.m_isImageInterpolated = imageStructureNet.IsImageInterpolated;
							pdfImageInfo.m_isSoftMasked = imageStructureNet.IsSoftMasked;
							pdfImageInfo.m_imageStructure = imageStructureNet;
							pdfImageInfo.internalRotation = internalRotation;
							pdfImageInfo.bounds = rectangleF;
							pdfImageInfo.helper = this;
							if (m_extractedImagesBounds.Contains(rectangleF) && m_ImageKey.Contains(pdfImageInfo.Name))
							{
								pdfImageInfo.regenerateImage = true;
								break;
							}
							m_extractedImagesBounds.Add(rectangleF);
							maskImageCollection.Add(item);
							extractedImageMatrix.Add(pdfMatrix);
							m_imageInfoList.Add(pdfImageInfo);
							m_ImageKey.Add(pdfImageInfo.Name);
						}
					}
					catch (Exception ex)
					{
						if (ex.Message.Equals("Document contains one or more images with unsupported encoding."))
						{
							throw new Exception(ex.Message);
						}
					}
					break;
				}
				case "Q":
					m_currentMatrix.Pop();
					break;
				}
			}
		}
		m_imageinfo = m_imageInfoList.ToArray();
		int num = 0;
		IEnumerator enumerator = m_extractedImagesBounds.GetEnumerator();
		while (enumerator.MoveNext())
		{
			m_imageinfo[num].Bounds = (RectangleF)enumerator.Current;
			m_imageinfo[num].Image = null;
			m_imageinfo[num].Index = num;
			m_imageinfo[num].Matrix = extractedImageMatrix[num];
			m_imageinfo[num].MaskImage = maskImageCollection[num];
			num++;
		}
		Thread.CurrentThread.CurrentCulture = currentCulture;
		isExtractImages = false;
		contentParser?.Dispose();
		ClearImageResources();
		return m_imageinfo;
	}

	internal void ExtractInnerXObjectImages(PdfStream contentstream, PdfPageResourcesHelper childResources)
	{
		float internalRotation = 0f;
		new PdfDictionary();
		PdfStream pdfStream = null;
		string empty = string.Empty;
		RectangleF rectangleF = default(RectangleF);
		Matrix matrix = null;
		PdfStream pdfStream2 = null;
		PdfRecordCollection pdfRecordCollection = new ContentParser(contentstream.InternalStream.ToArray()).ReadContent();
		for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
		{
			string text = pdfRecordCollection.RecordCollection[i].OperatorName;
			string[] operands = pdfRecordCollection.RecordCollection[i].Operands;
			char[] symbolChars = m_symbolChars;
			for (int j = 0; j < symbolChars.Length; j++)
			{
				char c = symbolChars[j];
				if (text.Contains(c.ToString()))
				{
					text = text.Replace(c.ToString(), "");
				}
			}
			switch (text.Trim())
			{
			case "q":
			{
				GraphicsStateDataNet graphicsStateDataNet = new GraphicsStateDataNet();
				if (m_currentMatrix.Count > 0)
				{
					GraphicsStateDataNet graphicsStateDataNet2 = m_currentMatrix.Peek();
					graphicsStateDataNet.m_drawing2dMatrixCTM = graphicsStateDataNet2.m_drawing2dMatrixCTM;
				}
				m_currentMatrix.Push(graphicsStateDataNet);
				break;
			}
			case "cm":
			{
				float m = float.Parse(operands[0]);
				float m2 = float.Parse(operands[1]);
				float m3 = float.Parse(operands[2]);
				float m4 = float.Parse(operands[3]);
				float dx = float.Parse(operands[4]);
				float dy = float.Parse(operands[5]);
				matrix = new Matrix(m, m2, m3, m4, dx, dy);
				m_currentMatrix.Peek().m_drawing2dMatrixCTM = Multiply(matrix, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
				break;
			}
			case "Do":
			{
				ImageStructureNet imageStructureNet = null;
				PdfMatrix pdfMatrix = null;
				PdfReader pdfReader = null;
				empty = operands[0].Replace("/", "");
				if (childResources != null && childResources.Resources.Count > 0 && !(childResources.Resources[operands[0].Replace("/", "")] is ImageStructureNet))
				{
					XObjectElement xObjectElement = childResources[operands[0].Replace("/", "")] as XObjectElement;
					PdfStream pdfStream3 = xObjectElement.XObjectDictionary as PdfStream;
					pdfStream3.Decompress();
					PdfDictionary pdfDictionary = new PdfDictionary();
					PdfPageResourcesHelper pdfPageResourcesHelper = new PdfPageResourcesHelper();
					if (xObjectElement.XObjectDictionary.ContainsKey("Resources"))
					{
						pdfDictionary = ((xObjectElement.XObjectDictionary["Resources"] is PdfReference) ? ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectElement.XObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectElement.XObjectDictionary["Resources"] as PdfDictionary) : ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
						Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetFontResources(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetExtendedGraphicResources(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetColorSpaceResource(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetShadingResource(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetPatternResource(pdfDictionary));
					}
					ExtractInnerXObjectImages(pdfStream3, pdfPageResourcesHelper);
					break;
				}
				if (childResources != null && childResources.Resources.Count > 0 && childResources.Resources[operands[0].Replace("/", "")] is ImageStructureNet)
				{
					imageStructureNet = childResources.Resources[operands[0].Replace("/", "")] as ImageStructureNet;
				}
				try
				{
					PdfDictionary imageDictionary = imageStructureNet.ImageDictionary;
					if (imageDictionary != null)
					{
						PdfArray pdfArray = null;
						PdfArray pdfArray2 = null;
						if (imageDictionary["ColorSpace"] is PdfArray)
						{
							pdfArray = imageDictionary["ColorSpace"] as PdfArray;
						}
						if (imageDictionary["ColorSpace"] is PdfReferenceHolder)
						{
							pdfArray = (imageDictionary["ColorSpace"] as PdfReferenceHolder).Object as PdfArray;
						}
						if (pdfArray != null && pdfArray[1] is PdfReferenceHolder)
						{
							pdfArray2 = (pdfArray[1] as PdfReferenceHolder).Object as PdfArray;
						}
						if (pdfArray2 != null && pdfArray2[1] is PdfReferenceHolder)
						{
							_ = (pdfArray2[1] as PdfReferenceHolder).Object;
						}
					}
					pdfStream2 = imageStructureNet.ImageDictionary as PdfStream;
					pdfMatrix = imageStructureNet.ImageInfo;
					for (int k = 0; k < Contents.Count; k++)
					{
						PdfStream pdfStream4 = ((pdfStream == null) ? ((Contents[k] as PdfReferenceHolder).Object as PdfStream) : pdfStream);
						pdfStream4.Decompress();
						MemoryStream internalStream = pdfStream4.InternalStream;
						internalStream.Position = 0L;
						pdfReader = new PdfReader(internalStream);
						pdfReader.Position = 0L;
						if (pdfReader.ReadStream().Contains(empty))
						{
							pdfMatrix = ((Rotation != PdfPageRotateAngle.RotateAngle90) ? new PdfMatrix(pdfReader, empty, Size) : new PdfMatrix(pdfReader, empty, Size));
							break;
						}
					}
					bool item = false;
					if (imageDictionary.ContainsKey("Mask"))
					{
						item = true;
					}
					if (pdfStream2.ContainsKey("Width"))
					{
						if (pdfStream2["Width"] is PdfNumber)
						{
							_ = (pdfStream2["Width"] as PdfNumber).IntValue;
						}
						if (pdfStream2["Width"] is PdfReferenceHolder)
						{
							_ = ((pdfStream2["Width"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
						}
						if (pdfStream2["Height"] is PdfNumber)
						{
							_ = (pdfStream2["Height"] as PdfNumber).IntValue;
						}
						if (pdfStream2["Height"] is PdfReferenceHolder)
						{
							_ = ((pdfStream2["Height"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
						}
					}
					else if (pdfStream2.ContainsKey("BBox"))
					{
						PdfArray obj = pdfStream2["BBox"] as PdfArray;
						_ = (obj[2] as PdfNumber).IntValue;
						_ = (obj[3] as PdfNumber).IntValue;
					}
					Matrix matrix2 = new Matrix(1f, 0f, 0f, -1.01f, 0f, 1f);
					Matrix matrix3 = Multiply(matrix2, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
					Matrix matrix4 = new Matrix(1.3333334f, 0f, 0f, -1.3333334f, 0f, Size.Height * 1.3333334f);
					matrix3 = Multiply(matrix3, matrix4);
					if (Rotation == PdfPageRotateAngle.RotateAngle270)
					{
						rectangleF = ((matrix3.Elements[0] == 0f || matrix3.Elements[3] == 0f) ? new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), (float)Math.Floor(Size.Width) - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + (float)Math.Floor(matrix3.Elements[0] / 1.3333334f)), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2])) : new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), Size.Width - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + matrix3.Elements[0] / 1.3333334f), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
					}
					else if (Rotation == PdfPageRotateAngle.RotateAngle90)
					{
						if (matrix3.Elements[0] == 0f && matrix3.Elements[3] == 0f)
						{
							rectangleF = new RectangleF(new PointF(Size.Height - matrix3.Elements[5] / 1.3333334f, matrix3.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2]));
						}
						else
						{
							internalRotation = 90f;
							rectangleF = new RectangleF(new PointF(Size.Height - matrix3.OffsetY / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], matrix3.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
						}
					}
					else if (Rotation == PdfPageRotateAngle.RotateAngle180)
					{
						rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
					}
					else if (matrix3.Elements[0] == 0f && matrix3.Elements[3] == 0f)
					{
						if (matrix3.Elements[1] < 0f && matrix3.Elements[2] > 0f)
						{
							internalRotation = 270f;
							rectangleF = new RectangleF(Size.Height - matrix3.Elements[5] / 1.3333334f, (float)Math.Round(matrix3.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
						}
						else if (matrix3.Elements[1] > 0f && matrix3.Elements[2] < 0f)
						{
							internalRotation = 90f;
							rectangleF = new RectangleF(matrix3.Elements[5] / 1.3333334f, (float)Math.Round(Size.Width - matrix3.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
						}
						else if (matrix3.Elements[1] < 0f && matrix3.Elements[2] < 0f)
						{
							internalRotation = 180f;
							rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
						else
						{
							rectangleF = new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
					}
					else
					{
						rectangleF = new RectangleF(matrix3.OffsetX / 1.3333334f, (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
					}
					PdfImageInfo pdfImageInfo = new PdfImageInfo();
					pdfImageInfo.Name = operands[0].Replace("/", "").ToString();
					imageStructureNet.IsImageForExtraction = true;
					pdfImageInfo.Metadata = GetMetadata(pdfStream2);
					pdfImageInfo.m_isImageMasked = imageStructureNet.IsImageMasked;
					pdfImageInfo.m_isImageInterpolated = imageStructureNet.IsImageInterpolated;
					pdfImageInfo.m_isSoftMasked = imageStructureNet.IsSoftMasked;
					pdfImageInfo.m_imageStructure = imageStructureNet;
					pdfImageInfo.internalRotation = internalRotation;
					pdfImageInfo.bounds = rectangleF;
					pdfImageInfo.helper = this;
					if (m_extractedImagesBounds.Contains(rectangleF) && m_ImageKey.Contains(pdfImageInfo.Name))
					{
						pdfImageInfo.regenerateImage = true;
						break;
					}
					m_extractedImagesBounds.Add(rectangleF);
					maskImageCollection.Add(item);
					extractedImageMatrix.Add(pdfMatrix);
					m_imageInfoList.Add(pdfImageInfo);
					m_ImageKey.Add(pdfImageInfo.Name);
				}
				catch (Exception)
				{
				}
				break;
			}
			case "Q":
				m_currentMatrix.Pop();
				break;
			}
		}
	}

	internal void CombineContent(Stream stream)
	{
		bool flag = m_page is PdfLoadedPage;
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		byte[] array = PdfString.StringToByte("\r\n");
		if (pdfLoadedPage != null)
		{
			for (int i = 0; i < pdfLoadedPage.Contents.Count; i++)
			{
				PdfStream pdfStream = null;
				IPdfPrimitive pdfPrimitive = pdfLoadedPage.Contents[i];
				if (pdfPrimitive is PdfReferenceHolder)
				{
					pdfStream = (pdfLoadedPage.Contents[i] as PdfReferenceHolder).Object as PdfStream;
				}
				else if (pdfPrimitive is PdfStream)
				{
					pdfStream = pdfPrimitive as PdfStream;
				}
				if (pdfStream == null)
				{
					continue;
				}
				if (flag)
				{
					byte[] decompressedData = pdfStream.GetDecompressedData();
					using (MemoryStream memoryStream = new MemoryStream(decompressedData))
					{
						byte[] array2 = new byte[32];
						memoryStream.Position = 0L;
						int count;
						while ((count = memoryStream.Read(array2, 0, array2.Length)) > 0)
						{
							stream.Write(array2, 0, count);
						}
					}
					Array.Clear(decompressedData, 0, decompressedData.Length);
					decompressedData = null;
				}
				stream.Write(array, 0, array.Length);
			}
		}
		else
		{
			Layers.CombineContent(stream);
		}
	}

	private PdfResources GetResources()
	{
		return m_page.GetResources();
	}

	private PdfDictionary GetObject(IPdfPrimitive primitive)
	{
		PdfDictionary result = null;
		if (primitive is PdfDictionary)
		{
			result = primitive as PdfDictionary;
		}
		else if (primitive is PdfReferenceHolder)
		{
			result = (primitive as PdfReferenceHolder).Object as PdfDictionary;
		}
		return result;
	}

	private void RemovedResourceImage(string imageName)
	{
		PdfDictionary dictionary = m_page.Dictionary;
		if (!dictionary.ContainsKey("Resources") || !(dictionary["Resources"] is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("XObject"))
		{
			return;
		}
		PdfDictionary pdfDictionary2 = null;
		PdfResources resources = GetResources();
		IPdfPrimitive pdfPrimitive = GetXObject(resources);
		while (true)
		{
			if (pdfPrimitive != null && pdfPrimitive is PdfDictionary)
			{
				Dictionary<PdfName, IPdfPrimitive> items = ((PdfDictionary)pdfPrimitive).Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						PdfStream pdfStream = ((PdfReferenceHolder)item.Value).Object as PdfStream;
						PdfDictionary pdfDictionary3 = pdfStream;
						if (pdfStream.ContainsKey("Subtype") && (pdfStream["Subtype"] as PdfName).Value == "Image" && item.Key.Value == imageName)
						{
							items.Remove(item.Key);
							break;
						}
						if (pdfDictionary3.ContainsKey("Resources"))
						{
							pdfDictionary2 = pdfDictionary3["Resources"] as PdfDictionary;
						}
					}
				}
			}
			if (pdfDictionary2 == null || !(pdfPrimitive is PdfDictionary) || (pdfPrimitive as PdfDictionary).Items.Count == 0 || pdfPrimitive == null || !pdfDictionary2.ContainsKey("XObject"))
			{
				break;
			}
			if (pdfDictionary2.ContainsKey("XObject"))
			{
				if (!(PdfCrossTable.Dereference(pdfDictionary2["XObject"]) is PdfDictionary))
				{
					break;
				}
				resources = new PdfResources(pdfDictionary2);
				IPdfPrimitive xObject = GetXObject(resources);
				if (pdfPrimitive == xObject)
				{
					break;
				}
				pdfPrimitive = xObject;
			}
		}
	}

	internal IPdfPrimitive GetXObject(PdfResources resources)
	{
		IPdfPrimitive result = null;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in resources.Items)
		{
			if (item.Key.ToString() == "/XObject")
			{
				result = PdfCrossTable.Dereference(item.Value);
			}
		}
		return result;
	}

	private PdfArray GetArrayFromReferenceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
			if (pdfReferenceHolder.Object is PdfReferenceHolder)
			{
				return GetArrayFromReferenceHolder(pdfReferenceHolder.Object);
			}
			return pdfReferenceHolder.Object as PdfArray;
		}
		return primitive as PdfArray;
	}

	internal void RemoveImage(PdfImageInfo imageInfo)
	{
		PdfDictionary dictionary = m_page.Dictionary;
		MemoryStream memoryStream = new MemoryStream();
		Layers.CombineContent(memoryStream);
		PageResourceLoader instance = PageResourceLoader.Instance;
		PdfPageResourcesHelper pageResources = instance.GetPageResources(m_page);
		UpdateResources(instance);
		PdfStream pdfStream = RemovedImageObject(memoryStream, imageInfo.Name, pageResources);
		if (dictionary.ContainsKey("Contents"))
		{
			PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(dictionary["Contents"]);
			pdfStream.Compress = true;
			if (arrayFromReferenceHolder != null)
			{
				foreach (IPdfPrimitive content in Contents)
				{
					if (PdfCrossTable.Dereference(content) is PdfDictionary pdfDictionary)
					{
						pdfDictionary.isSkip = true;
					}
				}
				arrayFromReferenceHolder.Clear();
				arrayFromReferenceHolder.Add(new PdfReferenceHolder(pdfStream));
			}
			else
			{
				PdfStream pdfStream2 = dictionary["Contents"] as PdfStream;
				if (pdfStream2 != null)
				{
					pdfStream2.Clear();
					pdfStream2.Items.Remove(new PdfName("Length"));
					pdfStream2.Data = pdfStream.Data;
					pdfStream2.Compress = true;
				}
				m_page.Graphics.StreamWriter = new PdfStreamWriter(pdfStream2);
			}
		}
		RemovedResourceImage(imageInfo.Name);
		if (m_page is PdfLoadedPage && (m_page as PdfLoadedPage).Document != null && (m_page as PdfLoadedPage).Document is PdfLoadedDocument)
		{
			((m_page as PdfLoadedPage).Document as PdfLoadedDocument).FileStructure.IncrementalUpdate = false;
		}
	}

	private PdfStream RemovedImageObject(MemoryStream input, string Name, PdfPageResourcesHelper pageResources)
	{
		PdfRecordCollection pdfRecordCollection = new ContentParser(input.ToArray()).ReadContent();
		for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
		{
			string text = pdfRecordCollection.RecordCollection[i].OperatorName;
			string[] operands = pdfRecordCollection.RecordCollection[i].Operands;
			char[] symbolChars = m_symbolChars;
			for (int j = 0; j < symbolChars.Length; j++)
			{
				char c = symbolChars[j];
				if (text.Contains(c.ToString()))
				{
					text = text.Replace(c.ToString(), "");
				}
			}
			if (!(text.Trim() == "Do"))
			{
				continue;
			}
			if (pageResources.ContainsKey(operands[0].Replace("/", "")))
			{
				if (pageResources[operands[0].Replace("/", "")] is XObjectElement)
				{
					_ = string.Empty;
					XObjectElement xObjectElement = pageResources[operands[0].Replace("/", "")] as XObjectElement;
					if (xObjectElement.ObjectType == "Form")
					{
						PdfStream pdfStream = xObjectElement.XObjectDictionary as PdfStream;
						pdfStream.Decompress();
						PageResourceLoader pageResourceLoader = new PageResourceLoader();
						PdfDictionary pdfDictionary = new PdfDictionary();
						PdfDictionary xObjectDictionary = xObjectElement.XObjectDictionary;
						PdfPageResourcesHelper pageResources2 = new PdfPageResourcesHelper();
						if (xObjectDictionary.ContainsKey("Resources"))
						{
							pdfDictionary = ((xObjectDictionary["Resources"] is PdfReference) ? ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectElement.XObjectDictionary["Resources"] as PdfDictionary) : ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
							Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetFontResources(pdfDictionary));
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetExtendedGraphicResources(pdfDictionary));
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetColorSpaceResource(pdfDictionary));
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetShadingResource(pdfDictionary));
							pageResources2 = pageResourceLoader.UpdatePageResources(pageResources2, pageResourceLoader.GetPatternResource(pdfDictionary));
						}
						PdfStream pdfStream2 = RemovedImageObject(pdfStream.InternalStream, Name, pageResources2);
						PdfStream pdfStream3 = new PdfStream();
						pdfStream3.Data = pdfStream2.Data;
						pdfStream3.Compress = true;
						pdfStream2.Dispose();
						pdfStream.Clear();
						pdfStream.Items.Remove(new PdfName("Length"));
						pdfStream.Data = pdfStream3.Data;
						pdfStream.Compress = true;
						pdfStream.Modify();
					}
				}
				else if (operands[0].Replace("/", "") == Name)
				{
					pdfRecordCollection.Remove(pdfRecordCollection.RecordCollection[i]);
				}
			}
			else if (operands[0].Replace("/", "") == Name)
			{
				pdfRecordCollection.Remove(pdfRecordCollection.RecordCollection[i]);
			}
		}
		PdfStream pdfStream4 = new PdfStream();
		for (int k = 0; k < pdfRecordCollection.RecordCollection.Count; k++)
		{
			OptimizeContent(pdfRecordCollection, k, null, pdfStream4);
		}
		return pdfStream4;
	}

	internal void OptimizeContent(PdfRecordCollection recordCollection, int i, string updatedText, PdfStream stream)
	{
		int count = recordCollection.RecordCollection.Count;
		PdfRecord pdfRecord = recordCollection.RecordCollection[i];
		if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
		{
			if (pdfRecord.OperatorName == "ID")
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int j = 0; j < pdfRecord.Operands.Length; j++)
				{
					if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/") && pdfRecord.Operands[j + 1].Contains("/"))
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append(" ");
						stringBuilder.Append(pdfRecord.Operands[j + 1]);
						stringBuilder.Append("\r\n");
						j++;
					}
					else if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/"))
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append(" ");
						stringBuilder.Append(pdfRecord.Operands[j + 1]);
						stringBuilder.Append("\r\n");
						j++;
					}
					else
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append("\r\n");
					}
				}
				string s = stringBuilder.ToString();
				byte[] bytes = Encoding.Default.GetBytes(s);
				stream.Write(bytes);
			}
			else
			{
				for (int k = 0; k < pdfRecord.Operands.Length; k++)
				{
					string value = pdfRecord.Operands[k];
					if ((pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "'" || pdfRecord.OperatorName == "\"" || pdfRecord.OperatorName == "TJ") && updatedText != null)
					{
						value = updatedText;
						if (pdfRecord.OperatorName == "'")
						{
							stream.Write("T*");
							stream.Write(" ");
						}
						pdfRecord.OperatorName = "TJ";
					}
					PdfString pdfString = new PdfString(value);
					stream.Write(pdfString.Bytes);
					if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
					{
						stream.Write(" ");
					}
				}
			}
		}
		else if (pdfRecord.Operands == null && pdfRecord.InlineImageBytes != null)
		{
			string @string = Encoding.Default.GetString(pdfRecord.InlineImageBytes);
			byte[] bytes2 = Encoding.Default.GetBytes(@string);
			stream.Write(bytes2);
			stream.Write(" ");
		}
		stream.Write(pdfRecord.OperatorName);
		if (i + 1 < count)
		{
			if (pdfRecord.OperatorName == "ID")
			{
				stream.Write("\n");
			}
			else if (i + 1 < count && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && recordCollection.RecordCollection[i + 1].OperatorName == "n")
			{
				stream.Write(" ");
			}
			else if (pdfRecord.OperatorName == "w" || pdfRecord.OperatorName == "EI")
			{
				stream.Write(" ");
			}
			else
			{
				stream.Write("\r\n");
			}
		}
	}

	internal bool ParseContentStream(int index, Matrix currentMatrix, List<string> keys)
	{
		bool flag = false;
		float internalRotation = 0f;
		string text = m_recordCollection.RecordCollection[index].OperatorName;
		string[] operands = m_recordCollection.RecordCollection[index].Operands;
		char[] symbolChars = m_symbolChars;
		for (int i = 0; i < symbolChars.Length; i++)
		{
			char c = symbolChars[i];
			if (text.Contains(c.ToString()))
			{
				text = text.Replace(c.ToString(), "");
			}
		}
		switch (text.Trim())
		{
		case "q":
		{
			GraphicsStateDataNet graphicsStateDataNet = new GraphicsStateDataNet();
			if (m_currentMatrix.Count > 0)
			{
				GraphicsStateDataNet graphicsStateDataNet2 = m_currentMatrix.Peek();
				graphicsStateDataNet.m_drawing2dMatrixCTM = graphicsStateDataNet2.m_drawing2dMatrixCTM;
			}
			m_currentMatrix.Push(graphicsStateDataNet);
			break;
		}
		case "cm":
		{
			float m = float.Parse(operands[0]);
			float m2 = float.Parse(operands[1]);
			float m3 = float.Parse(operands[2]);
			float m4 = float.Parse(operands[3]);
			float dx = float.Parse(operands[4]);
			float dy = float.Parse(operands[5]);
			currentMatrix = new Matrix(m, m2, m3, m4, dx, dy);
			m_currentMatrix.Peek().m_drawing2dMatrixCTM = Multiply(currentMatrix, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
			break;
		}
		case "Do":
		{
			string text2 = operands[0].Replace("/", "");
			if (isExtractImages)
			{
				ImageStructureNet imageStructureNet = null;
				if (!(m_pageResources.Resources[text2] is ImageStructureNet))
				{
					XObjectElement xObjectElement = m_pageResources[text2] as XObjectElement;
					PdfStream pdfStream = xObjectElement.XObjectDictionary as PdfStream;
					pdfStream.Decompress();
					PdfDictionary pdfDictionary = new PdfDictionary();
					PdfPageResourcesHelper pdfPageResourcesHelper = new PdfPageResourcesHelper();
					if (xObjectElement.XObjectDictionary.ContainsKey("Resources"))
					{
						pdfDictionary = ((xObjectElement.XObjectDictionary["Resources"] is PdfReference) ? ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectElement.XObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectElement.XObjectDictionary["Resources"] as PdfDictionary) : ((xObjectElement.XObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
						Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetFontResources(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetExtendedGraphicResources(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetColorSpaceResource(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetShadingResource(pdfDictionary));
						pdfPageResourcesHelper = m_resourceLoader.UpdatePageResources(pdfPageResourcesHelper, m_resourceLoader.GetPatternResource(pdfDictionary));
					}
					ExtractInnerXObjectImages(pdfStream, pdfPageResourcesHelper);
					return true;
				}
				if (m_pageResources.Resources[text2] is ImageStructureNet)
				{
					imageStructureNet = m_pageResources.Resources[text2] as ImageStructureNet;
				}
				try
				{
					PdfDictionary imageDictionary = imageStructureNet.ImageDictionary;
					if (imageDictionary != null)
					{
						PdfArray pdfArray = null;
						PdfArray pdfArray2 = null;
						if (imageDictionary["ColorSpace"] is PdfArray)
						{
							pdfArray = imageDictionary["ColorSpace"] as PdfArray;
						}
						if (imageDictionary["ColorSpace"] is PdfReferenceHolder)
						{
							pdfArray = (imageDictionary["ColorSpace"] as PdfReferenceHolder).Object as PdfArray;
						}
						if (pdfArray != null && pdfArray[1] is PdfReferenceHolder)
						{
							pdfArray2 = (pdfArray[1] as PdfReferenceHolder).Object as PdfArray;
						}
						if (pdfArray2 != null && pdfArray2[1] is PdfReferenceHolder)
						{
							_ = (pdfArray2[1] as PdfReferenceHolder).Object;
						}
					}
					PdfStream pdfStream2 = imageStructureNet.ImageDictionary as PdfStream;
					SizeF xObjectSize = GetXObjectSize(pdfStream2);
					_ = xObjectSize.Width;
					_ = xObjectSize.Height;
					Matrix matrix = new Matrix(1f, 0f, 0f, -1.01f, 0f, 1f);
					Matrix matrix2 = Multiply(matrix, m_currentMatrix.Peek().m_drawing2dMatrixCTM);
					Matrix matrix3 = new Matrix(1.3333334f, 0f, 0f, -1.3333334f, 0f, Size.Height * 1.3333334f);
					matrix2 = Multiply(matrix2, matrix3);
					RectangleF rectangleF;
					if (Rotation == PdfPageRotateAngle.RotateAngle270)
					{
						rectangleF = ((matrix2.Elements[0] == 0f || matrix2.Elements[3] == 0f) ? new RectangleF((float)Math.Floor(matrix2.OffsetY / 1.3333334f), (float)Math.Floor(Size.Width) - ((float)Math.Round(matrix2.OffsetX / 1.3333334f, 5) + (float)Math.Floor(matrix2.Elements[0] / 1.3333334f)), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]), Math.Abs(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2])) : new RectangleF((float)Math.Floor(matrix2.OffsetY / 1.3333334f), Size.Width - ((float)Math.Round(matrix2.OffsetX / 1.3333334f, 5) + matrix2.Elements[0] / 1.3333334f), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
					}
					else if (Rotation == PdfPageRotateAngle.RotateAngle90)
					{
						if (matrix2.Elements[0] == 0f && matrix2.Elements[3] == 0f)
						{
							rectangleF = new RectangleF(new PointF(Size.Height - matrix2.Elements[5] / 1.3333334f, matrix2.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2]));
						}
						else
						{
							internalRotation = 90f;
							rectangleF = new RectangleF(new PointF(Size.Height - matrix2.OffsetY / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], matrix2.Elements[4] / 1.3333334f), new SizeF(m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0]));
						}
					}
					else if (Rotation == PdfPageRotateAngle.RotateAngle180)
					{
						rectangleF = new RectangleF(Size.Width - matrix2.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix2.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
					}
					else if (matrix2.Elements[0] == 0f && matrix2.Elements[3] == 0f)
					{
						if (matrix2.Elements[1] < 0f && matrix2.Elements[2] > 0f)
						{
							internalRotation = 270f;
							rectangleF = new RectangleF(Size.Height - matrix2.Elements[5] / 1.3333334f, (float)Math.Round(matrix2.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
						}
						else if (matrix2.Elements[1] > 0f && matrix2.Elements[2] < 0f)
						{
							internalRotation = 90f;
							rectangleF = new RectangleF(matrix2.Elements[5] / 1.3333334f, (float)Math.Round(Size.Width - matrix2.Elements[4] / 1.3333334f, 5), 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[2], 0f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[1]);
						}
						else if (matrix2.Elements[1] < 0f && matrix2.Elements[2] < 0f)
						{
							internalRotation = 180f;
							rectangleF = new RectangleF(Size.Width - matrix2.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix2.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
						else
						{
							rectangleF = new RectangleF(Size.Width - matrix2.OffsetX / 1.3333334f - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], Size.Height - m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3] - (float)Math.Round(matrix2.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
						}
					}
					else
					{
						rectangleF = new RectangleF(matrix2.OffsetX / 1.3333334f, (float)Math.Round(matrix2.OffsetY / 1.3333334f, 5), m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[0], m_currentMatrix.Peek().m_drawing2dMatrixCTM.Elements[3]);
					}
					PdfImageInfo pdfImageInfo = new PdfImageInfo();
					pdfImageInfo.Name = text2.ToString();
					pdfImageInfo.Metadata = GetMetadata(pdfStream2);
					imageStructureNet.IsImageForExtraction = true;
					pdfImageInfo.m_isImageMasked = imageStructureNet.IsImageMasked;
					pdfImageInfo.m_isImageInterpolated = imageStructureNet.IsImageInterpolated;
					pdfImageInfo.m_isSoftMasked = imageStructureNet.IsSoftMasked;
					pdfImageInfo.m_imageStructure = imageStructureNet;
					pdfImageInfo.internalRotation = internalRotation;
					pdfImageInfo.bounds = rectangleF;
					pdfImageInfo.helper = this;
					if (m_extractedImagesBounds.Contains(rectangleF) && m_ImageKey.Contains(pdfImageInfo.Name))
					{
						pdfImageInfo.regenerateImage = true;
						break;
					}
					m_extractedImagesBounds.Add(rectangleF);
					m_imageInfoList.Add(pdfImageInfo);
					m_ImageKey.Add(pdfImageInfo.Name);
				}
				catch (Exception)
				{
				}
				break;
			}
			if (m_pageResources.Resources[text2] is XObjectElement)
			{
				XObjectElement xObjectElement2 = m_pageResources[text2] as XObjectElement;
				if (xObjectElement2.XObjectDictionary != null && xObjectElement2.XObjectDictionary is PdfStream)
				{
					SizeF xObjectSize2 = GetXObjectSize(xObjectElement2.XObjectDictionary as PdfStream);
					PointF xObjectPosition = GetXObjectPosition(xObjectElement2.XObjectDictionary as PdfStream);
					try
					{
						Matrix matrix4 = new Matrix(1f, 0f, 0f, -1f, 0f, 1f);
						Matrix matrix5 = Multiply(new Matrix(xObjectSize2.Width - xObjectPosition.X, 0f, 0f, xObjectSize2.Height - xObjectPosition.Y, xObjectPosition.X, xObjectPosition.Y), m_currentMatrix.Peek().m_drawing2dMatrixCTM);
						matrix5 = Multiply(matrix4, matrix5);
						Matrix matrix6 = new Matrix(1.3333334f, 0f, 0f, -1.3333334f, 0f, Size.Height * 1.3333334f);
						matrix5 = Multiply(matrix5, matrix6);
						if (Rotation == PdfPageRotateAngle.RotateAngle90)
						{
							if (matrix5.Elements[0] == 0f && matrix5.Elements[3] == 0f)
							{
								new RectangleF(Size.Height - matrix5.Elements[5] / 1.3333334f, matrix5.Elements[4] / 1.3333334f, (0f - matrix5.Elements[1]) / 1.3333334f, matrix5.Elements[2] / 1.3333334f);
							}
							else
							{
								new RectangleF(Size.Height - matrix5.OffsetY / 1.3333334f - (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5), matrix5.Elements[4] / 1.3333334f, (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[0] / 1.3333334f, 5));
							}
						}
						else if (Rotation == PdfPageRotateAngle.RotateAngle180)
						{
							new RectangleF(Size.Width - matrix5.OffsetX / 1.3333334f - (float)Math.Round(matrix5.Elements[0] / 1.3333334f, 5), Size.Height - (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5) - (float)Math.Round(matrix5.OffsetY / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[0] / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5));
						}
						else if (Rotation == PdfPageRotateAngle.RotateAngle270)
						{
							if (matrix5.Elements[0] != 0f && matrix5.Elements[3] != 0f)
							{
								new RectangleF((float)Math.Floor(matrix5.OffsetY / 1.3333334f), Size.Width - ((float)Math.Round(matrix5.OffsetX / 1.3333334f, 5) + matrix5.Elements[0] / 1.3333334f), (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[0] / 1.3333334f, 5));
							}
							else
							{
								new RectangleF((float)Math.Floor(matrix5.OffsetY / 1.3333334f), (float)Math.Floor(Size.Width) - ((float)Math.Round(matrix5.OffsetX / 1.3333334f, 5) + (float)Math.Floor(matrix5.Elements[0] / 1.3333334f)), (float)Math.Round(matrix5.Elements[1] / 1.3333334f, 5), (float)Math.Round((0f - matrix5.Elements[2]) / 1.3333334f, 5));
							}
						}
						else
						{
							new RectangleF(matrix5.OffsetX / 1.3333334f, (float)Math.Round(matrix5.OffsetY / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[0] / 1.3333334f, 5), (float)Math.Round(matrix5.Elements[3] / 1.3333334f, 5));
						}
					}
					catch (Exception ex2)
					{
						if (ex2.Message.Equals("Document contains one or more images with unsupported encoding."))
						{
							throw new Exception(ex2.Message);
						}
					}
				}
			}
			if (!flag)
			{
				keys.Add(text2);
			}
			break;
		}
		case "Q":
			m_currentMatrix.Pop();
			break;
		}
		return flag;
	}

	private XmpMetadata GetMetadata(PdfStream imageStream)
	{
		if (imageStream.ContainsKey("Metadata"))
		{
			IPdfPrimitive pdfPrimitive = imageStream["Metadata"];
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfStream stream = pdfReferenceHolder.Object as PdfStream;
				return TryGetMetadata(stream);
			}
			return TryGetMetadata(pdfPrimitive as PdfStream);
		}
		return null;
	}

	private XmpMetadata TryGetMetadata(PdfStream stream)
	{
		if (stream != null)
		{
			byte[] decompressedData = stream.GetDecompressedData();
			if (decompressedData.Length != 0)
			{
				return new ImageMetadataParser(new MemoryStream(decompressedData)).TryGetMetadata();
			}
		}
		return null;
	}

	internal void RegenerateImages(Bitmap image, string imageName)
	{
		for (int i = 0; i < m_imageInfoList.Count; i++)
		{
			if (!(m_imageInfoList[i].Name == imageName))
			{
				continue;
			}
			MemoryStream stream = new MemoryStream();
			image.Save(stream, image.m_format);
			Image image2 = Image.FromStream(stream);
			if (i < extractedImages.Count && extractedImages[i] is Image)
			{
				extractedImages.RemoveAt(i);
				if (image2 != null)
				{
					extractedImages.Add(image2);
					break;
				}
			}
		}
	}

	private SizeF GetXObjectSize(PdfStream element)
	{
		SizeF empty = SizeF.Empty;
		if (element.ContainsKey("Width"))
		{
			if (element["Width"] is PdfNumber)
			{
				empty.Width = (element["Width"] as PdfNumber).IntValue;
			}
			if (element["Width"] is PdfReferenceHolder)
			{
				empty.Width = ((element["Width"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
			}
			if (element["Height"] is PdfNumber)
			{
				empty.Height = (element["Height"] as PdfNumber).IntValue;
			}
			if (element["Height"] is PdfReferenceHolder)
			{
				empty.Height = ((element["Height"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
			}
		}
		else if (element.ContainsKey("BBox"))
		{
			PdfArray pdfArray = element["BBox"] as PdfArray;
			empty.Width = (pdfArray[2] as PdfNumber).FloatValue;
			empty.Height = (pdfArray[3] as PdfNumber).FloatValue;
		}
		return empty;
	}

	private PointF GetXObjectPosition(PdfStream element)
	{
		PointF empty = PointF.Empty;
		if (element.ContainsKey("BBox"))
		{
			PdfArray pdfArray = element["BBox"] as PdfArray;
			empty.X = (pdfArray[0] as PdfNumber).FloatValue;
			empty.Y = (pdfArray[1] as PdfNumber).FloatValue;
		}
		return empty;
	}

	private Matrix Multiply(Matrix matrix1, Matrix matrix2)
	{
		return new Matrix(matrix1.Elements[0] * matrix2.Elements[0] + matrix1.Elements[1] * matrix2.Elements[2], matrix1.Elements[0] * matrix2.Elements[1] + matrix1.Elements[1] * matrix2.Elements[3], matrix1.Elements[2] * matrix2.Elements[0] + matrix1.Elements[3] * matrix2.Elements[2], matrix1.Elements[2] * matrix2.Elements[1] + matrix1.Elements[3] * matrix2.Elements[3], matrix1.OffsetX * matrix2.Elements[0] + matrix1.OffsetY * matrix2.Elements[2] + matrix2.OffsetX, matrix1.OffsetX * matrix2.Elements[1] + matrix1.OffsetY * matrix2.Elements[3] + matrix2.OffsetY);
	}

	internal ImageExportHelperNet(PdfPageBase page)
	{
		m_page = page;
		isFlateCompress = false;
	}

	internal static ImageExportHelperNet GetImageExporter(PdfPageBase page)
	{
		return new ImageExportHelperNet(page);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfDocumentExtractor : IDisposable
{
	private PdfLoadedDocument _loadedDocument;

	private List<Stream> _imageStreamCollection;

	private Dictionary<PdfReference, PdfReference> _xobjectReferenceCollection;

	private Dictionary<PdfReference, PdfReference> _sMaskReference;

	private Dictionary<PdfReference, PdfReference> _imageReference;

	private Dictionary<PdfReference, Stream> _cacheStream;

	private bool _globalResource;

	private bool _extractionByPage;

	public int PageCount
	{
		get
		{
			if (_loadedDocument != null)
			{
				return _loadedDocument.Pages.Count;
			}
			return 0;
		}
	}

	public void Load(Stream stream, string password = null)
	{
		try
		{
			if (!string.IsNullOrEmpty(password))
			{
				_loadedDocument = new PdfLoadedDocument(stream, password, openAndRepair: true);
			}
			else
			{
				_loadedDocument = new PdfLoadedDocument(stream, openAndRepair: true);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public Stream[] ExtractImages()
	{
		_imageStreamCollection = new List<Stream>();
		_xobjectReferenceCollection = new Dictionary<PdfReference, PdfReference>();
		_sMaskReference = new Dictionary<PdfReference, PdfReference>();
		_imageReference = new Dictionary<PdfReference, PdfReference>();
		for (int i = 0; i < PageCount; i++)
		{
			PdfDictionary dictionary = _loadedDocument.Pages[i].Dictionary;
			if (dictionary != null)
			{
				ParseResources(dictionary);
			}
		}
		_xobjectReferenceCollection.Clear();
		_sMaskReference.Clear();
		_imageReference.Clear();
		return _imageStreamCollection.ToArray();
	}

	public Stream[] ExtractImages(int startPageIndex, int endPageIndex)
	{
		if (!_globalResource)
		{
			_imageStreamCollection = new List<Stream>();
			_xobjectReferenceCollection = new Dictionary<PdfReference, PdfReference>();
			_sMaskReference = new Dictionary<PdfReference, PdfReference>();
			_imageReference = new Dictionary<PdfReference, PdfReference>();
			if (_cacheStream == null)
			{
				_cacheStream = new Dictionary<PdfReference, Stream>();
			}
			_extractionByPage = true;
			for (int i = startPageIndex; i <= endPageIndex; i++)
			{
				PdfDictionary dictionary = _loadedDocument.Pages[i].Dictionary;
				if (dictionary != null)
				{
					ParseResources(dictionary);
				}
			}
			_xobjectReferenceCollection.Clear();
			_sMaskReference.Clear();
			_imageReference.Clear();
			return _imageStreamCollection.ToArray();
		}
		if (_imageStreamCollection != null)
		{
			return _imageStreamCollection.ToArray();
		}
		return null;
	}

	public void Dispose()
	{
		if (_imageStreamCollection != null)
		{
			Parallel.ForEach((IEnumerable<Stream>)_imageStreamCollection, (Action<Stream>)delegate(Stream stream)
			{
				stream.Dispose();
			});
			_imageStreamCollection.Clear();
			_imageStreamCollection = null;
		}
		if (_cacheStream != null)
		{
			Parallel.ForEach((IEnumerable<Stream>)_cacheStream.Values.ToList(), (Action<Stream>)delegate(Stream stream)
			{
				stream.Dispose();
			});
			_cacheStream.Clear();
			_cacheStream = null;
		}
		_globalResource = false;
		_loadedDocument.Close(completely: true);
	}

	private PdfDictionary GetResources(PdfDictionary dictionary)
	{
		PdfDictionary pdfDictionary = null;
		if (dictionary.ContainsKey("Resources"))
		{
			pdfDictionary = PdfCrossTable.Dereference(dictionary["Resources"]) as PdfDictionary;
		}
		else if (pdfDictionary == null && dictionary.ContainsKey("Parent") && !_globalResource && PdfCrossTable.Dereference(dictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Resources"))
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary2["Resources"]) as PdfDictionary;
			if (pdfDictionary != null)
			{
				_globalResource = true;
			}
		}
		return pdfDictionary;
	}

	private void ParseResources(PdfDictionary dictionary)
	{
		PdfDictionary resources = GetResources(dictionary);
		if (resources == null || !resources.ContainsKey("XObject") || !(PdfCrossTable.Dereference(resources["XObject"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		UpdateSMaskReference(pdfDictionary.Items);
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(item.Value) as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = item.Value as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfReference value = null;
				_sMaskReference.TryGetValue(pdfReferenceHolder.Reference, out value);
				if (value != null)
				{
					continue;
				}
			}
			if (pdfDictionary2 == null || !pdfDictionary2.ContainsKey("Subtype"))
			{
				continue;
			}
			PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["Subtype"]) as PdfName;
			if (pdfName != null && pdfName.Value == "Image")
			{
				if (!(pdfReferenceHolder != null))
				{
					continue;
				}
				if (_extractionByPage)
				{
					Stream value2 = null;
					_cacheStream.TryGetValue(pdfReferenceHolder.Reference, out value2);
					if (value2 != null && value2.CanRead)
					{
						_imageStreamCollection.Add(value2);
						continue;
					}
					_imageReference[pdfReferenceHolder.Reference] = pdfReferenceHolder.Reference;
				}
				else
				{
					PdfReference value3 = null;
					_imageReference.TryGetValue(pdfReferenceHolder.Reference, out value3);
					if (value3 != null)
					{
						continue;
					}
					_imageReference[pdfReferenceHolder.Reference] = pdfReferenceHolder.Reference;
				}
				ParseImage(pdfReferenceHolder, pdfDictionary2);
			}
			else if (pdfName != null && pdfName.Value == "Form")
			{
				ParseTemplate(pdfReferenceHolder, pdfDictionary2);
			}
		}
	}

	private void ParseTemplate(PdfReferenceHolder reference, PdfDictionary xObjectChildItem)
	{
		bool flag = false;
		if (reference != null)
		{
			PdfReference value = null;
			_xobjectReferenceCollection.TryGetValue(reference.Reference, out value);
			if (value != null)
			{
				flag = true;
			}
			else
			{
				_xobjectReferenceCollection[reference.Reference] = reference.Reference;
			}
		}
		if (!flag)
		{
			ParseResources(xObjectChildItem);
		}
	}

	private void ParseImage(PdfReferenceHolder reference, PdfDictionary xObjectChildItem)
	{
		ImageStructureNet imageStructureNet = new ImageStructureNet(xObjectChildItem, new PdfMatrix());
		if (imageStructureNet != null)
		{
			imageStructureNet.IsImageForExtraction = true;
			Stream stream = null;
			try
			{
				stream = imageStructureNet.GetImageStream();
			}
			catch
			{
			}
			if (stream != null)
			{
				_imageStreamCollection.Add(stream);
				if (_extractionByPage)
				{
					_cacheStream[reference.Reference] = stream;
				}
			}
			imageStructureNet.IsImageForExtraction = false;
		}
		imageStructureNet = null;
	}

	private void UpdateSMaskReference(Dictionary<PdfName, IPdfPrimitive> xObjectChilds)
	{
		List<PdfReference> list = new List<PdfReference>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> xObjectChild in xObjectChilds)
		{
			if (!(PdfCrossTable.Dereference(xObjectChild.Value) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Subtype"))
			{
				continue;
			}
			PdfName pdfName = pdfDictionary["Subtype"] as PdfName;
			if (!(pdfName != null) || !(pdfName.Value == "Image"))
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder = xObjectChild.Value as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null))
			{
				continue;
			}
			list.Add(pdfReferenceHolder.Reference);
			if (pdfDictionary.ContainsKey("SMask"))
			{
				PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["SMask"] as PdfReferenceHolder;
				if (pdfReferenceHolder2 != null && list.Contains(pdfReferenceHolder2.Reference))
				{
					_sMaskReference[pdfReferenceHolder2.Reference] = pdfReferenceHolder2.Reference;
				}
			}
		}
		list.Clear();
		list = null;
	}
}

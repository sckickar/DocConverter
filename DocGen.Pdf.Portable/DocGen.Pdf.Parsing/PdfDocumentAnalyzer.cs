using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfDocumentAnalyzer
{
	private Stream m_stream;

	private string m_password;

	private List<PdfException> result;

	private List<long> parsedObjNum = new List<long>();

	public PdfDocumentAnalyzer(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("file");
		}
		if (stream.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		m_stream = stream;
	}

	public PdfDocumentAnalyzer(Stream stream, string password)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("file");
		}
		if (stream.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		m_stream = stream;
		m_password = password;
	}

	public SyntaxAnalyzerResult AnalyzeSyntax()
	{
		SyntaxAnalyzerResult syntaxAnalyzerResult = new SyntaxAnalyzerResult();
		PdfLoadedDocument pdfLoadedDocument = null;
		try
		{
			pdfLoadedDocument = new PdfLoadedDocument(m_stream, m_password, out result);
		}
		catch (Exception ex)
		{
			result.Add(new PdfException(ex.Message));
		}
		if (pdfLoadedDocument != null)
		{
			PdfDictionary catalog = pdfLoadedDocument.Catalog;
			if (catalog != null)
			{
				try
				{
					if (!catalog.ContainsKey("Pages"))
					{
						throw new PdfException("The document has no page tree node");
					}
					if (!(PdfCrossTable.Dereference(catalog["Pages"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Type"))
					{
						throw new PdfException("The document has no page tree node");
					}
					PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName;
					if (pdfName != null && pdfName.Value != "Pages")
					{
						throw new PdfException("Type of PDF page tree node is invalid");
					}
					ParseDictionary(catalog);
				}
				catch (Exception ex2)
				{
					result.Add(new PdfException(ex2.Message));
				}
			}
			else
			{
				result.Add(new PdfException("The document has no calatog object"));
			}
		}
		if (result.Count == 0)
		{
			return syntaxAnalyzerResult;
		}
		syntaxAnalyzerResult.IsCorrupted = true;
		syntaxAnalyzerResult.Errors = result;
		return syntaxAnalyzerResult;
	}

	private void ParseDictionary(PdfDictionary dictionary)
	{
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			if (item.Key.Value == "Parent" || item.Key.Value == "First" || item.Key.Value == "Last" || item.Key.Value == "Next" || item.Key.Value == "Prev" || item.Key.Value == "P" || item.Key.Value == "Dest" || item.Key.Value == "Pg" || item.Key.Value == "Data" || item.Key.Value == "Reference" || item.Key.Value == "K" || item.Key.Value == "D" || item.Key.Value == "T" || item.Key.Value == "N" || item.Key.Value == "V")
			{
				continue;
			}
			if (item.Key.Value == "Contents" && dictionary.ContainsKey("Type") && (dictionary["Type"] as PdfName).Value == "Page")
			{
				try
				{
					if (PdfCrossTable.Dereference(item.Value) is PdfArray pdfArray)
					{
						foreach (IPdfPrimitive item2 in pdfArray)
						{
							if (!(PdfCrossTable.Dereference(item2) is PdfStream))
							{
								throw new PdfException("The value of the key 'Contents' is incorrect or missing.");
							}
						}
					}
					else if (item.Value is PdfReferenceHolder && !(PdfCrossTable.Dereference(item.Value) is PdfStream))
					{
						throw new PdfException("The value of the key 'Contents' is incorrect or missing.");
					}
				}
				catch (Exception ex)
				{
					result.Add(new PdfException(ex.Message));
				}
			}
			PdfDictionary pdfDictionary = item.Value as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = item.Value as PdfReferenceHolder;
			PdfArray pdfArray2 = item.Value as PdfArray;
			if (pdfDictionary != null)
			{
				ParseDictionary(pdfDictionary);
			}
			else if (pdfReferenceHolder != null)
			{
				ParseReferenceHolder(pdfReferenceHolder);
			}
			else if (pdfArray2 != null)
			{
				ParseArray(pdfArray2);
			}
		}
	}

	private void ParseReferenceHolder(PdfReferenceHolder holder)
	{
		try
		{
			if (holder.Reference != null && (parsedObjNum.Count <= 0 || !parsedObjNum.Contains(holder.Reference.ObjNum)))
			{
				parsedObjNum.Add(holder.Reference.ObjNum);
				IPdfPrimitive @object = holder.Object;
				if (@object is PdfDictionary)
				{
					ParseDictionary(@object as PdfDictionary);
				}
				else if (@object is PdfReferenceHolder)
				{
					ParseReferenceHolder(@object as PdfReferenceHolder);
				}
				else if (@object is PdfArray)
				{
					ParseArray(@object as PdfArray);
				}
			}
		}
		catch (Exception ex)
		{
			PdfException item = new PdfException(ex.Message.ToString());
			result.Add(item);
		}
	}

	private void ParseArray(PdfArray array)
	{
		foreach (IPdfPrimitive item in array)
		{
			PdfDictionary pdfDictionary = item as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = item as PdfReferenceHolder;
			PdfArray pdfArray = item as PdfArray;
			if (pdfDictionary != null)
			{
				ParseDictionary(pdfDictionary);
			}
			else if (pdfReferenceHolder != null)
			{
				ParseReferenceHolder(pdfReferenceHolder);
			}
			else if (pdfArray != null)
			{
				ParseArray(pdfArray);
			}
		}
	}

	public void Close()
	{
		if (m_stream != null)
		{
			m_stream.Dispose();
		}
		parsedObjNum.Clear();
	}
}

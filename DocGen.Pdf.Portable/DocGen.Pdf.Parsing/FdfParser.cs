using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class FdfParser
{
	private PdfReader m_reader;

	private PdfParser m_parser;

	private Stream m_stream;

	private FdfObjectCollection m_objects;

	private Dictionary<string, PdfReferenceHolder> m_annotationObjects;

	private Dictionary<string, string> m_groupObjects;

	internal bool hasTrailer = true;

	internal bool fdfImport;

	internal PdfReader Reader => m_reader;

	internal PdfParser Parser => m_parser;

	internal FdfObjectCollection FdfObjects => m_objects;

	internal Dictionary<string, string> GroupedObjects
	{
		get
		{
			if (m_groupObjects == null)
			{
				m_groupObjects = new Dictionary<string, string>();
			}
			return m_groupObjects;
		}
	}

	internal FdfParser(Stream stream)
	{
		m_stream = stream;
		m_reader = new PdfReader(stream);
		m_objects = new FdfObjectCollection();
	}

	internal void ImportAnnotations(PdfLoadedDocument document)
	{
		m_annotationObjects = GetAnnotationObjects();
		if (!GroupAnnotations())
		{
			return;
		}
		foreach (PdfReferenceHolder value2 in m_annotationObjects.Values)
		{
			PdfDictionary pdfDictionary = value2.Object as PdfDictionary;
			ParseDictionary(pdfDictionary);
			if (pdfDictionary == null || pdfDictionary.Items.Count <= 0)
			{
				continue;
			}
			if (pdfDictionary.ContainsKey("IRT") && pdfDictionary["IRT"] is PdfString pdfString && GroupedObjects.ContainsKey(pdfString.Value))
			{
				string key = GroupedObjects[pdfString.Value];
				pdfDictionary["IRT"] = m_annotationObjects[key];
			}
			if (pdfDictionary.ContainsKey("Contents"))
			{
				PdfString pdfString2 = pdfDictionary["Contents"] as PdfString;
				string value = pdfString2.Value;
				if (Regex.IsMatch(value, "[\u0085-ÿ]"))
				{
					byte[] array = pdfString2.PdfEncode(document);
					value = Encoding.UTF8.GetString(array, 0, array.Length);
					if (value.Contains("(") && value.Contains(")"))
					{
						value = value.Trim('(', ')');
					}
					pdfDictionary["Contents"] = new PdfString(value);
					if (pdfDictionary.ContainsKey("RC"))
					{
						pdfDictionary.SetString("RC", "<?xml version=\"1.0\"?><body xmlns=\"http://www.w3.org/1999/xhtml\"><p dir=\"ltr\">" + value + "</p></body>");
					}
					pdfDictionary.Modify();
				}
			}
			if (!pdfDictionary.ContainsKey("Page") || !(pdfDictionary["Page"] is PdfNumber { IntValue: var intValue }))
			{
				continue;
			}
			if (intValue < document.PageCount)
			{
				(document.Pages[intValue] as PdfLoadedPage).importAnnotation = true;
				PdfDictionary dictionary = document.Pages[intValue].Dictionary;
				if (dictionary != null)
				{
					if (!dictionary.ContainsKey("Annots"))
					{
						dictionary["Annots"] = new PdfArray();
					}
					IPdfPrimitive pdfPrimitive = dictionary["Annots"];
					if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfArray pdfArray)
					{
						pdfArray.Elements.Add(value2);
						pdfArray.MarkChanged();
						dictionary.Modify();
					}
				}
			}
			pdfDictionary.Remove("Page");
		}
	}

	private bool GroupAnnotations()
	{
		if (m_annotationObjects.Count > 0)
		{
			foreach (KeyValuePair<string, PdfReferenceHolder> annotationObject in m_annotationObjects)
			{
				if (annotationObject.Value != null && annotationObject.Value.Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("NM") && pdfDictionary["NM"] is PdfString pdfString && !string.IsNullOrEmpty(pdfString.Value))
				{
					if (GroupedObjects.ContainsKey(pdfString.Value))
					{
						GroupedObjects[pdfString.Value] = annotationObject.Key;
					}
					else
					{
						GroupedObjects.Add(pdfString.Value, annotationObject.Key);
					}
				}
			}
			return true;
		}
		return false;
	}

	internal void ParseObjectData()
	{
		PdfCrossTable pdfCrossTable = new PdfCrossTable(isFdf: true, m_stream);
		if (pdfCrossTable.CrossTable == null)
		{
			return;
		}
		m_parser = pdfCrossTable.CrossTable.Parser;
		m_parser.fdfImport = fdfImport;
		long num = CheckFdf();
		if (num == -1)
		{
			return;
		}
		long num2 = m_reader.Seek(0L, SeekOrigin.End);
		m_reader.SearchBack("trailer");
		m_parser.SetOffset(num);
		m_parser.Advance();
		while (m_parser.GetNext() == DocGen.Pdf.IO.TokenType.Number)
		{
			ParseObject(m_objects);
		}
		if (m_parser.Lexer != null && m_parser.Lexer.Text == "trailer")
		{
			IPdfPrimitive pdfPrimitive = m_parser.Trailer();
			if (pdfPrimitive != null)
			{
				m_objects.Add("trailer", pdfPrimitive);
			}
		}
		m_parser.Advance();
		while (m_parser.Lexer != null && m_parser.Lexer.Position != num2 && m_parser.GetNext() == DocGen.Pdf.IO.TokenType.Number)
		{
			ParseObject(m_objects);
		}
	}

	internal void ParseAnnotationData()
	{
		PdfCrossTable pdfCrossTable = new PdfCrossTable(isFdf: true, m_stream);
		if (pdfCrossTable.CrossTable == null)
		{
			return;
		}
		m_parser = pdfCrossTable.CrossTable.Parser;
		long num = CheckFdf();
		if (num == -1)
		{
			return;
		}
		long num2 = m_reader.Seek(0L, SeekOrigin.End);
		m_reader.SearchBack("trailer");
		m_parser.SetOffset(num);
		m_parser.Advance();
		while (m_parser.GetNext() == DocGen.Pdf.IO.TokenType.Number)
		{
			ParseObject(m_objects);
		}
		if (m_parser.Lexer != null && m_parser.Lexer.Text == "trailer")
		{
			IPdfPrimitive pdfPrimitive = m_parser.Trailer();
			if (pdfPrimitive != null)
			{
				m_objects.Add("trailer", pdfPrimitive);
			}
		}
		m_parser.Advance();
		while (m_parser.Lexer != null && m_parser.Lexer.Position != num2 && m_parser.GetNext() == DocGen.Pdf.IO.TokenType.Number)
		{
			ParseObject(m_objects);
		}
	}

	internal void Dispose()
	{
		m_objects.Dispose();
		m_parser = null;
		if (m_annotationObjects != null)
		{
			m_annotationObjects.Clear();
		}
		m_annotationObjects = null;
	}

	private long CheckFdf()
	{
		long num = 0L;
		long num2 = -1L;
		long num3 = 0L;
		int num4 = 8;
		while (num2 < 0 && num < m_stream.Length - 1)
		{
			byte[] array = new byte[1024];
			if (num - 5 > 0)
			{
				num -= 5;
			}
			m_stream.Position = num;
			m_stream.Read(array, 0, array.Length);
			num2 = Encoding.UTF8.GetString(array, 0, array.Length).IndexOf("%FDF-");
			num = m_stream.Position;
			if (num2 < 0)
			{
				num3 = num - 5;
			}
			else
			{
				num2 += num3;
			}
		}
		if (num2 < 0)
		{
			throw new Exception("Cannot import Fdf file. File format has been corrupted");
		}
		m_stream.Position = 0L;
		return num2 + num4;
	}

	private void ParseObject(FdfObjectCollection objects)
	{
		FdfObject fdfObject = m_parser.ParseObject();
		if (fdfObject.ObjectNumber > 0 && fdfObject.GenerationNumber >= 0)
		{
			string key = fdfObject.ObjectNumber + " " + fdfObject.GenerationNumber;
			objects.Add(key, fdfObject.Object);
		}
		m_parser.Advance();
	}

	private void ParseDictionary(PdfDictionary dictionary, PdfName key)
	{
		IPdfPrimitive pdfPrimitive = dictionary[key];
		if (pdfPrimitive is PdfDictionary || pdfPrimitive is PdfStream)
		{
			ParseDictionary(pdfPrimitive as PdfDictionary);
		}
		else if (pdfPrimitive is PdfArray)
		{
			ParseArray(pdfPrimitive as PdfArray);
		}
		else
		{
			if (!(pdfPrimitive is PdfReferenceHolder))
			{
				return;
			}
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null))
			{
				return;
			}
			PdfReference reference = pdfReferenceHolder.Reference;
			if (!(reference != null))
			{
				return;
			}
			string key2 = reference.ObjNum + " " + reference.GenNum;
			if (m_annotationObjects.ContainsKey(key2))
			{
				dictionary[key] = m_annotationObjects[key2];
				dictionary.Modify();
			}
			else if (m_objects.Objects.ContainsKey(key2))
			{
				Dictionary<string, IPdfPrimitive> objects = m_objects.Objects;
				if (objects[key2] is PdfReferenceHolder)
				{
					dictionary[key] = objects[key2];
					dictionary.Modify();
				}
				else if (objects[key2] is PdfName)
				{
					PdfReferenceHolder value = (PdfReferenceHolder)(dictionary[key] = new PdfReferenceHolder(objects[key2] as PdfName));
					objects[key2] = value;
					dictionary.Modify();
				}
				else if (objects[key2] is PdfArray)
				{
					PdfArray pdfArray = objects[key2] as PdfArray;
					ParseArray(pdfArray);
					PdfReferenceHolder value2 = (PdfReferenceHolder)(dictionary[key] = new PdfReferenceHolder(pdfArray));
					objects[key2] = value2;
					dictionary.Modify();
				}
				else if (objects[key2] is PdfStream)
				{
					PdfStream pdfStream = objects[key2] as PdfStream;
					ParseDictionary(pdfStream);
					PdfReferenceHolder value3 = (PdfReferenceHolder)(dictionary[key] = new PdfReferenceHolder(pdfStream));
					objects[key2] = value3;
					dictionary.Modify();
				}
				else if (objects[key2] is PdfDictionary)
				{
					PdfDictionary pdfDictionary = objects[key2] as PdfDictionary;
					ParseDictionary(pdfDictionary);
					PdfReferenceHolder value4 = (PdfReferenceHolder)(dictionary[key] = new PdfReferenceHolder(pdfDictionary));
					objects[key2] = value4;
					dictionary.Modify();
				}
			}
			else
			{
				dictionary.Remove(key);
			}
		}
	}

	private void ParseDictionary(PdfDictionary dictionary)
	{
		if (dictionary != null && dictionary.Items.Count > 0)
		{
			PdfName[] keys = GetKeys(dictionary);
			for (int i = 0; i < keys.Length; i++)
			{
				ParseDictionary(dictionary, keys[i]);
			}
		}
	}

	private void ParseArray(PdfArray array)
	{
		if (array == null)
		{
			return;
		}
		int count = array.Elements.Count;
		for (int i = 0; i < count; i++)
		{
			PdfReferenceHolder pdfReferenceHolder = array[i] as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null))
			{
				continue;
			}
			PdfReference reference = pdfReferenceHolder.Reference;
			if (!(reference != null))
			{
				continue;
			}
			string key = reference.ObjNum + " " + reference.GenNum;
			if (m_annotationObjects.ContainsKey(key))
			{
				array.Elements[i] = m_annotationObjects[key];
				array.MarkChanged();
			}
			else if (m_objects.Objects.ContainsKey(key))
			{
				Dictionary<string, IPdfPrimitive> objects = m_objects.Objects;
				if (objects[key] is PdfReferenceHolder)
				{
					array.Elements[i] = objects[key];
					array.MarkChanged();
				}
				else if (objects[key] is PdfDictionary || objects[key] is PdfStream)
				{
					PdfReferenceHolder value = new PdfReferenceHolder(objects[key]);
					array.Elements[i] = value;
					objects[key] = value;
					array.MarkChanged();
				}
			}
		}
	}

	private Dictionary<string, PdfReferenceHolder> GetAnnotationObjects()
	{
		Dictionary<string, PdfReferenceHolder> dictionary = new Dictionary<string, PdfReferenceHolder>();
		Dictionary<string, IPdfPrimitive> objects = m_objects.Objects;
		new List<string>();
		if (objects.Count > 0 && objects.ContainsKey("trailer"))
		{
			if (objects["trailer"] is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Root"))
			{
				PdfReferenceHolder pdfReferenceHolder = pdfDictionary["Root"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					PdfReference reference = pdfReferenceHolder.Reference;
					if (reference != null)
					{
						string key = reference.ObjNum + " " + reference.GenNum;
						if (objects.ContainsKey(key) && objects[key] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("FDF"))
						{
							if (pdfDictionary2["FDF"] is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Annots") && pdfDictionary3["Annots"] is PdfArray { Count: not 0 } pdfArray)
							{
								foreach (IPdfPrimitive element in pdfArray.Elements)
								{
									pdfReferenceHolder = element as PdfReferenceHolder;
									if (!(pdfReferenceHolder != null))
									{
										continue;
									}
									reference = pdfReferenceHolder.Reference;
									if (reference != null)
									{
										string key2 = reference.ObjNum + " " + reference.GenNum;
										if (objects.ContainsKey(key2))
										{
											dictionary.Add(key2, new PdfReferenceHolder(objects[key2]));
											objects.Remove(key2);
										}
									}
								}
							}
							objects.Remove(key);
						}
					}
				}
			}
			objects.Remove("trailer");
		}
		return dictionary;
	}

	private PdfName[] GetKeys(PdfDictionary dictionary)
	{
		List<PdfName> list = new List<PdfName>(dictionary.Keys.Count);
		foreach (PdfName key in dictionary.Keys)
		{
			list.Add(key);
		}
		return list.ToArray();
	}

	internal void ParseObjectTrailer()
	{
		PdfCrossTable pdfCrossTable = new PdfCrossTable(isFdf: true, m_stream);
		if (pdfCrossTable.CrossTable == null)
		{
			return;
		}
		m_parser = pdfCrossTable.CrossTable.Parser;
		m_parser.fdfImport = fdfImport;
		long num = CheckFdf();
		if (num != -1)
		{
			m_reader.Seek(0L, SeekOrigin.End);
			m_reader.SearchBack("trailer");
			m_parser.SetOffset(num);
			m_parser.Advance();
			int num2 = 0;
			while (m_parser.GetNext() == DocGen.Pdf.IO.TokenType.Number && num2 == 0)
			{
				ParseObject(m_objects);
				num2++;
			}
			if (m_parser.Lexer != null && m_parser.GetNext() == DocGen.Pdf.IO.TokenType.DictionaryStart && m_parser.Lexer.Text != "trailer")
			{
				hasTrailer = false;
			}
		}
	}
}

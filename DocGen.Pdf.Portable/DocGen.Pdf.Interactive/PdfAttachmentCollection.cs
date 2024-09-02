using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Interactive;

public class PdfAttachmentCollection : PdfCollection, IPdfWrapper
{
	private PdfArray m_array = new PdfArray();

	private PdfDictionary m_dictionary = new PdfDictionary();

	private Dictionary<string, PdfReferenceHolder> dic = new Dictionary<string, PdfReferenceHolder>();

	private List<string> orderList;

	private int count;

	internal PdfCrossTable m_CrossTable;

	private PdfMainObjectCollection m_objectCollection;

	private PdfDictionary attachmentDictionay;

	public PdfAttachment this[int index] => (PdfAttachment)base.List[index];

	internal PdfArray ArrayList => m_array;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfAttachmentCollection()
	{
		m_dictionary.SetProperty("Names", m_array);
	}

	internal PdfAttachmentCollection(PdfDictionary attachmentDictionary, PdfCrossTable table)
	{
		m_dictionary = attachmentDictionary;
		m_CrossTable = table;
		PdfDictionary pdfDictionary = m_dictionary["EmbeddedFiles"] as PdfDictionary;
		if (m_dictionary["EmbeddedFiles"] is PdfReferenceHolder && pdfDictionary == null)
		{
			pdfDictionary = (m_dictionary["EmbeddedFiles"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary == null)
		{
			return;
		}
		m_array = PdfCrossTable.Dereference(pdfDictionary["Names"]) as PdfArray;
		if (m_array == null && pdfDictionary["Kids"] is PdfArray)
		{
			if (!(pdfDictionary["Kids"] is PdfArray { Count: not 0 } pdfArray))
			{
				return;
			}
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (!(pdfArray[i] is PdfReferenceHolder) && !(pdfArray[i] is PdfDictionary))
				{
					continue;
				}
				pdfDictionary = pdfArray[i] as PdfDictionary;
				if (pdfArray[i] is PdfReferenceHolder && pdfDictionary == null)
				{
					pdfDictionary = (pdfArray[i] as PdfReferenceHolder).Object as PdfDictionary;
				}
				if (pdfDictionary != null)
				{
					m_array = PdfCrossTable.Dereference(pdfDictionary["Names"]) as PdfArray;
					if (m_array != null)
					{
						attachmentInformation(m_array);
					}
				}
			}
		}
		else if (m_array != null)
		{
			attachmentInformation(m_array);
		}
	}

	private void attachmentInformation(PdfArray m_array)
	{
		if (m_array.Count == 0)
		{
			return;
		}
		int num = 1;
		for (int i = 0; i < m_array.Count / 2; i++)
		{
			if (m_array[num] is PdfReferenceHolder || m_array[num] is PdfDictionary)
			{
				PdfDictionary pdfDictionary = m_array[num] as PdfDictionary;
				if (m_array[num] is PdfReferenceHolder && pdfDictionary == null)
				{
					pdfDictionary = (m_array[num] as PdfReferenceHolder).Object as PdfDictionary;
				}
				if (pdfDictionary != null)
				{
					PdfStream pdfStream = new PdfStream();
					PdfDictionary pdfDictionary2 = null;
					if (pdfDictionary.ContainsKey("EF"))
					{
						if (pdfDictionary["EF"] is PdfDictionary)
						{
							pdfDictionary2 = pdfDictionary["EF"] as PdfDictionary;
						}
						else if (pdfDictionary["EF"] is PdfReferenceHolder)
						{
							pdfDictionary2 = (pdfDictionary["EF"] as PdfReferenceHolder).Object as PdfDictionary;
						}
						PdfReferenceHolder pdfReferenceHolder = pdfDictionary2["F"] as PdfReferenceHolder;
						if (pdfReferenceHolder != null)
						{
							PdfReference reference = pdfReferenceHolder.Reference;
							pdfStream = pdfReferenceHolder.Object as PdfStream;
							IPdfDecryptable pdfDecryptable = pdfStream;
							if (pdfDecryptable != null && m_CrossTable.Encryptor != null && m_CrossTable.Encryptor.EncryptOnlyAttachment && reference != null)
							{
								pdfDecryptable.Decrypt(m_CrossTable.Encryptor, reference.ObjNum);
							}
						}
					}
					PdfAttachment pdfAttachment = null;
					PdfString pdfString = null;
					if (pdfStream == null)
					{
						pdfAttachment = ((!pdfDictionary.ContainsKey("Desc")) ? new PdfAttachment((pdfDictionary["F"] as PdfString).Value) : new PdfAttachment((pdfDictionary["Desc"] as PdfString).Value));
					}
					else
					{
						pdfStream.Decompress();
						if (pdfDictionary.ContainsKey("F"))
						{
							if (pdfDictionary.ContainsKey("UF"))
							{
								if (PdfCrossTable.Dereference(pdfDictionary["UF"]) is PdfString pdfString2)
								{
									pdfAttachment = new PdfAttachment(pdfString2.Value, pdfStream.Data);
								}
							}
							else if (PdfCrossTable.Dereference(pdfDictionary["F"]) is PdfString pdfString3)
							{
								pdfAttachment = new PdfAttachment(pdfString3.Value, pdfStream.Data);
							}
							PdfDictionary pdfDictionary3 = pdfStream;
							if (pdfDictionary3 != null)
							{
								PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary3["Subtype"]) as PdfName;
								if (pdfName != null)
								{
									pdfAttachment.MimeType = pdfName.Value.Replace("#23", "#").Replace("#20", " ").Replace("#2F", "/");
								}
							}
							if (pdfDictionary3.ContainsKey("Params") && PdfCrossTable.Dereference(pdfDictionary3["Params"]) is PdfDictionary pdfDictionary4)
							{
								PdfString pdfString4 = PdfCrossTable.Dereference(pdfDictionary4["CreationDate"]) as PdfString;
								PdfString pdfString5 = PdfCrossTable.Dereference(pdfDictionary4["ModDate"]) as PdfString;
								if (pdfString4 != null)
								{
									pdfAttachment.CreationDate = pdfDictionary4.GetDateTime(pdfString4);
								}
								if (pdfString5 != null)
								{
									pdfAttachment.ModificationDate = pdfDictionary4.GetDateTime(pdfString5);
								}
							}
							if (pdfDictionary.ContainsKey("AFRelationship"))
							{
								PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary["AFRelationship"]) as PdfName;
								if (pdfName2 != null)
								{
									pdfAttachment.Relationship = ObtainRelationShip(pdfName2.Value);
								}
							}
							if (pdfDictionary.ContainsKey("Desc"))
							{
								pdfAttachment.Description = (pdfDictionary["Desc"] as PdfString).Value;
							}
							if (pdfDictionary.ContainsKey("CI"))
							{
								PdfDictionary pdfDictionary5 = null;
								if (pdfDictionary["CI"] is PdfDictionary)
								{
									pdfDictionary5 = pdfDictionary["CI"] as PdfDictionary;
								}
								else if (pdfDictionary["CI"] is PdfReferenceHolder)
								{
									pdfDictionary5 = (pdfDictionary["CI"] as PdfReferenceHolder).Object as PdfDictionary;
								}
								if (pdfDictionary5 != null)
								{
									PdfPortfolioAttributes portfolioAttributes = new PdfPortfolioAttributes(pdfDictionary5);
									pdfAttachment.PortfolioAttributes = portfolioAttributes;
								}
							}
						}
						else
						{
							pdfAttachment = new PdfAttachment((pdfDictionary["Desc"] as PdfString).Value, pdfStream.Data);
						}
					}
					base.List.Add(pdfAttachment);
				}
			}
			num += 2;
		}
	}

	public int Add(PdfAttachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment");
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2U)
		{
			throw new PdfConformanceException("Attachment is not allowed in " + PdfDocument.ConformanceLevel.ToString() + " Conformance.");
		}
		int result = DoAdd(attachment);
		m_dictionary.Modify();
		return result;
	}

	public void Insert(int index, PdfAttachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment");
		}
		DoInsert(index, attachment);
	}

	public void Remove(PdfAttachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment");
		}
		DoRemove(attachment);
	}

	public void RemoveAt(int index)
	{
		DoRemoveAt(index);
	}

	public int IndexOf(PdfAttachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment");
		}
		return base.List.IndexOf(attachment);
	}

	public bool Contains(PdfAttachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment");
		}
		return base.List.Contains(attachment);
	}

	public void Clear()
	{
		DoClear();
	}

	private int DoAdd(PdfAttachment attachment)
	{
		string fileName = attachment.FileName;
		string text = "";
		text = ((!PdfString.IsUnicode(fileName)) ? fileName : ("Attachment " + count++));
		System.StringComparer ordinal = System.StringComparer.Ordinal;
		if (dic.Count == 0 && m_array.Count > 0)
		{
			for (int i = 0; i < m_array.Count; i += 2)
			{
				if (!dic.ContainsKey((m_array[i] as PdfString).Value))
				{
					dic.Add((m_array[i] as PdfString).Value, m_array[i + 1] as PdfReferenceHolder);
					continue;
				}
				string key = (m_array[i] as PdfString).Value + "_copy";
				dic.Add(key, m_array[i + 1] as PdfReferenceHolder);
			}
		}
		if (!dic.ContainsKey(text))
		{
			dic.Add(text, new PdfReferenceHolder(attachment));
		}
		else
		{
			string key2 = text + "_copy";
			dic.Add(key2, new PdfReferenceHolder(attachment));
		}
		orderList = new List<string>(dic.Keys);
		orderList.Sort(ordinal);
		m_array.Clear();
		foreach (string order in orderList)
		{
			m_array.Add(new PdfString(order));
			m_array.Add(dic[order]);
		}
		base.List.Add(attachment);
		return base.List.Count - 1;
	}

	private void DoInsert(int index, PdfAttachment attachment)
	{
		m_array.Insert(2 * index, new PdfString(attachment.FileName));
		m_array.Insert(2 * index + 1, new PdfReferenceHolder(attachment));
		base.List.Insert(index, attachment);
	}

	private void DoRemove(PdfAttachment attachment)
	{
		int num = base.List.IndexOf(attachment);
		m_array.RemoveAt(2 * num);
		attachmentDictionay = PdfCrossTable.Dereference(m_array[2 * num]) as PdfDictionary;
		if (attachmentDictionay != null)
		{
			RemoveAttachementObjects(attachmentDictionay);
			attachmentDictionay = null;
		}
		m_array.RemoveAt(2 * num);
		base.List.Remove(attachment);
	}

	private void DoRemoveAt(int index)
	{
		m_array.RemoveAt(2 * index);
		attachmentDictionay = PdfCrossTable.Dereference(m_array[2 * index]) as PdfDictionary;
		if (attachmentDictionay != null)
		{
			RemoveAttachementObjects(attachmentDictionay);
			attachmentDictionay = null;
		}
		m_array.RemoveAt(2 * index);
		base.List.RemoveAt(index);
	}

	private new void DoClear()
	{
		base.List.Clear();
		if (m_CrossTable != null && m_CrossTable.Document.PdfObjects != null)
		{
			for (int i = 1; i < m_array.Count; i += 2)
			{
				if (m_array[i] is PdfReferenceHolder && PdfCrossTable.Dereference(m_array[i]) is PdfDictionary attachmentDictionary)
				{
					RemoveAttachementObjects(attachmentDictionary);
				}
			}
		}
		m_array.Clear();
	}

	private void RemoveAttachementObjects(PdfDictionary attachmentDictionary)
	{
		if (m_CrossTable != null)
		{
			m_objectCollection = m_CrossTable.Document.PdfObjects;
		}
		if (m_objectCollection == null)
		{
			return;
		}
		if (attachmentDictionary != null)
		{
			if (attachmentDictionary.ContainsKey("EF") && PdfCrossTable.Dereference(attachmentDictionary["EF"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("F") && PdfCrossTable.Dereference(pdfDictionary["F"]) is PdfStream element && m_objectCollection.Contains(element))
			{
				m_objectCollection.Remove(m_objectCollection.IndexOf(element));
			}
			if (m_objectCollection.Contains(attachmentDictionary))
			{
				m_objectCollection.Remove(m_objectCollection.IndexOf(attachmentDictionary));
			}
		}
		if (dic.Count > 0)
		{
			dic.Clear();
		}
	}

	private PdfAttachmentRelationship ObtainRelationShip(string relation)
	{
		PdfAttachmentRelationship result = PdfAttachmentRelationship.Unspecified;
		switch (relation)
		{
		case "Alternative":
			result = PdfAttachmentRelationship.Alternative;
			break;
		case "Data":
			result = PdfAttachmentRelationship.Data;
			break;
		case "Source":
			result = PdfAttachmentRelationship.Source;
			break;
		case "Supplement":
			result = PdfAttachmentRelationship.Supplement;
			break;
		case "Unspecified":
			result = PdfAttachmentRelationship.Unspecified;
			break;
		}
		return result;
	}
}

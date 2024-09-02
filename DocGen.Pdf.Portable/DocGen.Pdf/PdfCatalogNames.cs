using System;
using System.Collections.Generic;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class PdfCatalogNames : IPdfWrapper
{
	private class NodeInfo
	{
		public PdfDictionary Node;

		public int Index;

		public int Count;

		public PdfArray Kids;

		public NodeInfo(PdfDictionary node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			Node = node;
			IPdfPrimitive obj = node["Kids"];
			Kids = PdfCrossTable.Dereference(obj) as PdfArray;
			Count = Kids.Count;
		}

		public NodeInfo(int index, int count)
		{
			Index = index;
			Count = count;
		}
	}

	private PdfAttachmentCollection m_attachments;

	internal PdfDictionary m_dictionary = new PdfDictionary();

	public PdfAttachmentCollection EmbeddedFiles
	{
		get
		{
			return m_attachments;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("EmbeddedFiles");
			}
			if (m_attachments != value)
			{
				m_attachments = value;
				m_dictionary.SetProperty("EmbeddedFiles", new PdfReferenceHolder(m_attachments));
			}
		}
	}

	internal PdfDictionary Destinations => PdfCrossTable.Dereference(m_dictionary["Dests"]) as PdfDictionary;

	public IPdfPrimitive Element => m_dictionary;

	public PdfCatalogNames()
	{
	}

	public PdfCatalogNames(PdfDictionary root)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		m_dictionary = root;
	}

	internal IPdfPrimitive GetNamedObjectFromTree(PdfDictionary root, PdfString name)
	{
		bool flag = false;
		PdfDictionary pdfDictionary = root;
		IPdfPrimitive result = null;
		while (!flag && pdfDictionary != null && pdfDictionary.Items.Count > 0)
		{
			if (pdfDictionary.ContainsKey("Kids"))
			{
				pdfDictionary = GetProperKid(pdfDictionary, name);
			}
			else if (pdfDictionary.ContainsKey("Names"))
			{
				result = FindName(pdfDictionary, name);
				flag = true;
			}
		}
		return result;
	}

	private IPdfPrimitive FindName(PdfDictionary current, PdfString name)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(current["Names"]) as PdfArray;
		int num = pdfArray.Count / 2;
		int num2 = 0;
		int num3 = num - 1;
		int num4 = 0;
		bool flag = false;
		while (!flag)
		{
			num4 = (num2 + num3) / 2;
			if (num2 > num3)
			{
				break;
			}
			PdfString str = PdfCrossTable.Dereference(pdfArray[num4 * 2]) as PdfString;
			int num5 = PdfString.ByteCompare(name, str);
			if (num5 > 0)
			{
				num2 = num4 + 1;
				continue;
			}
			if (num5 < 0)
			{
				num3 = num4 - 1;
				continue;
			}
			flag = true;
			break;
		}
		IPdfPrimitive result = null;
		if (flag)
		{
			result = PdfCrossTable.Dereference(pdfArray[num4 * 2 + 1]);
		}
		return result;
	}

	private PdfDictionary GetProperKid(PdfDictionary current, PdfString name)
	{
		PdfArray obj = PdfCrossTable.Dereference(current["Kids"]) as PdfArray;
		PdfDictionary pdfDictionary = null;
		foreach (IPdfPrimitive item in obj)
		{
			pdfDictionary = PdfCrossTable.Dereference(item) as PdfDictionary;
			if (CheckLimits(pdfDictionary, name))
			{
				break;
			}
			pdfDictionary = null;
		}
		return pdfDictionary;
	}

	private bool CheckLimits(PdfDictionary kid, PdfString name)
	{
		PdfArray pdfArray = kid["Limits"] as PdfArray;
		bool result = false;
		if (pdfArray != null && pdfArray.Count >= 2)
		{
			PdfString str = pdfArray[0] as PdfString;
			PdfString str2 = pdfArray[1] as PdfString;
			int num = PdfString.ByteCompare(str, name);
			int num2 = PdfString.ByteCompare(str2, name);
			if (num == 0 || num2 == 0)
			{
				result = true;
			}
			else if (num < 0 && num2 > 0)
			{
				result = true;
			}
		}
		return result;
	}

	internal void MergeEmbedded(PdfCatalogNames names, PdfCrossTable crossTable)
	{
		List<IPdfPrimitive> embedded = names.GetEmbedded();
		AppendEmbedded(embedded, crossTable);
	}

	private void AppendEmbedded(List<IPdfPrimitive> embedded, PdfCrossTable crossTable)
	{
		if (embedded != null && embedded.Count > 0)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_dictionary["EmbeddedFiles"]) as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = new PdfDictionary();
				pdfDictionary["Names"] = new PdfArray();
				m_dictionary["EmbeddedFiles"] = new PdfReferenceHolder(pdfDictionary);
			}
			string baseName = string.Empty;
			PdfDictionary pdfDictionary2 = null;
			if (pdfDictionary.ContainsKey("Names"))
			{
				pdfDictionary2 = pdfDictionary;
				baseName = GetNodeRightLimit(pdfDictionary2);
			}
			else if (pdfDictionary.ContainsKey("Kids"))
			{
				PdfArray obj = PdfCrossTable.Dereference(pdfDictionary["Kids"]) as PdfArray;
				pdfDictionary2 = PdfCrossTable.Dereference(obj[obj.Count - 1]) as PdfDictionary;
				baseName = GetNodeRightLimit(pdfDictionary2);
				pdfDictionary2 = new PdfDictionary { ["Names"] = new PdfArray() };
				obj.Add(new PdfReferenceHolder(pdfDictionary2));
			}
			AppendObjects(baseName, pdfDictionary2, embedded, pdfDictionary2 != pdfDictionary, crossTable);
		}
	}

	private string GetNodeRightLimit(PdfDictionary node)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(node["Limits"]) as PdfArray;
		PdfString pdfString = null;
		if (pdfArray != null)
		{
			pdfString = PdfCrossTable.Dereference(pdfArray[1]) as PdfString;
		}
		string result = string.Empty;
		if (pdfString != null)
		{
			result = pdfString.Value;
		}
		else if (node.ContainsKey("Names"))
		{
			PdfArray pdfArray2 = PdfCrossTable.Dereference(node["Names"]) as PdfArray;
			if (pdfArray2.Count > 1)
			{
				pdfString = PdfCrossTable.Dereference(pdfArray2[pdfArray2.Count - 2]) as PdfString;
				result = pdfString.Value;
			}
		}
		return result;
	}

	private void AppendObjects(string baseName, PdfDictionary node, List<IPdfPrimitive> embedded, bool updateLimits, PdfCrossTable crossTable)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(node["Names"]) as PdfArray;
		int count = embedded.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (crossTable == null)
			{
				pdfArray.Add(embedded[i]);
			}
			else
			{
				pdfArray.Add(embedded[i].Clone(crossTable));
			}
		}
		SortAttachmentNames(pdfArray, node);
		if (updateLimits)
		{
			PdfString element = new PdfString(baseName + count.ToString("X"));
			PdfString element2 = new PdfString(baseName + num.ToString("X"));
			PdfArray pdfArray2 = (PdfArray)(node["Limits"] = new PdfArray());
			pdfArray2.Add(element2);
			pdfArray2.Add(element);
		}
	}

	private void SortAttachmentNames(PdfArray loadedAttachmentNames, PdfDictionary catalogNames)
	{
		List<string> list = null;
		Dictionary<string, IPdfPrimitive> dictionary = new Dictionary<string, IPdfPrimitive>();
		if (loadedAttachmentNames == null || loadedAttachmentNames.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < loadedAttachmentNames.Count; i += 2)
		{
			string text = null;
			if (loadedAttachmentNames[i] is PdfString pdfString)
			{
				text = pdfString.Value;
			}
			else
			{
				PdfName pdfName = loadedAttachmentNames[i] as PdfName;
				if (pdfName != null)
				{
					text = pdfName.Value;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (!dictionary.ContainsKey(text))
			{
				IPdfPrimitive pdfPrimitive = loadedAttachmentNames[i + 1];
				if (pdfPrimitive != null)
				{
					dictionary[text] = pdfPrimitive;
				}
				continue;
			}
			string uniqueName = GetUniqueName(text, dictionary);
			IPdfPrimitive pdfPrimitive2 = loadedAttachmentNames[i + 1];
			if (pdfPrimitive2 != null)
			{
				dictionary[uniqueName] = pdfPrimitive2;
			}
		}
		System.StringComparer ordinal = System.StringComparer.Ordinal;
		list = new List<string>(dictionary.Keys);
		list.Sort(ordinal);
		PdfArray pdfArray = new PdfArray();
		foreach (string item in list)
		{
			pdfArray.Add(new PdfString(item));
			pdfArray.Add(dictionary[item]);
		}
		catalogNames.SetProperty("Names", pdfArray);
		dictionary.Clear();
		list.Clear();
	}

	private string GetUniqueName(string attachmentName, Dictionary<string, IPdfPrimitive> attachmentCollection)
	{
		int num = 0;
		string text = attachmentName + num++;
		while (attachmentCollection.ContainsKey(text))
		{
			text = attachmentName + num++;
		}
		return text;
	}

	private List<IPdfPrimitive> GetEmbedded()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_dictionary["EmbeddedFiles"]) as PdfDictionary;
		List<IPdfPrimitive> list = null;
		if (pdfDictionary != null && m_dictionary.ContainsKey("EmbeddedFiles"))
		{
			list = new List<IPdfPrimitive>();
			Stack<NodeInfo> stack = new Stack<NodeInfo>();
			PdfDictionary pdfDictionary2 = pdfDictionary;
			NodeInfo nodeInfo = new NodeInfo(0, 1);
			do
			{
				IL_00cc:
				if (nodeInfo.Index >= nodeInfo.Count)
				{
					continue;
				}
				if (nodeInfo.Kids != null)
				{
					pdfDictionary2 = PdfCrossTable.Dereference(nodeInfo.Kids[nodeInfo.Index]) as PdfDictionary;
				}
				if (pdfDictionary2.ContainsKey("Kids"))
				{
					stack.Push(nodeInfo);
					nodeInfo = new NodeInfo(pdfDictionary2);
					continue;
				}
				if (pdfDictionary2.ContainsKey("Names"))
				{
					CollectObjects(pdfDictionary2, list);
					if (stack.Count > 0)
					{
						nodeInfo = stack.Pop();
					}
				}
				nodeInfo.Index++;
				goto IL_00cc;
			}
			while (stack.Count > 0);
		}
		return list;
	}

	private void CollectObjects(PdfDictionary leafNode, List<IPdfPrimitive> array)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(leafNode["Names"]) as PdfArray;
		int i = 0;
		for (int count = pdfArray.Count; i < count; i++)
		{
			array.Add(pdfArray[i]);
		}
	}

	internal void Clear()
	{
		if (m_attachments != null)
		{
			m_attachments.Clear();
		}
		if (m_dictionary != null)
		{
			m_dictionary.Clear();
		}
	}
}

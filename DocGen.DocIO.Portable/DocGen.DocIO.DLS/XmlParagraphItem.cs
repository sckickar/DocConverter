using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS.Convertors;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal class XmlParagraphItem : ParagraphItem
{
	private Stream m_xmlNode;

	private Dictionary<string, DictionaryEntry> m_relations;

	internal string m_shapeHyperlink;

	private Dictionary<string, ImageRecord> m_imageRelations;

	private int m_zOrderPosition;

	private bool m_hasNestedImageRelations;

	internal ParagraphItemCollection MathParaItemsCollection;

	internal Dictionary<string, ImageRecord> ImageRelations
	{
		get
		{
			if (m_imageRelations == null)
			{
				m_imageRelations = new Dictionary<string, ImageRecord>();
			}
			return m_imageRelations;
		}
	}

	internal Dictionary<string, DictionaryEntry> Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new Dictionary<string, DictionaryEntry>();
			}
			return m_relations;
		}
	}

	internal Stream DataNode
	{
		get
		{
			return m_xmlNode;
		}
		set
		{
			m_xmlNode = value;
		}
	}

	internal WCharacterFormat CharacterFormat
	{
		get
		{
			return m_charFormat;
		}
		set
		{
			m_charFormat = value;
		}
	}

	internal int ZOrderIndex
	{
		get
		{
			return m_zOrderPosition;
		}
		set
		{
			m_zOrderPosition = value;
		}
	}

	public override EntityType EntityType => EntityType.XmlParaItem;

	internal bool HasNestedImageRelations
	{
		get
		{
			return m_hasNestedImageRelations;
		}
		set
		{
			m_hasNestedImageRelations = value;
		}
	}

	public XmlParagraphItem(Stream xmlNode, IWordDocument wordDocument)
		: base(wordDocument as WordDocument)
	{
		m_xmlNode = xmlNode;
		m_charFormat = new WCharacterFormat(wordDocument);
		m_charFormat.SetOwner(this);
	}

	internal void ApplyCharacterFormat(WCharacterFormat charFormat)
	{
		if (charFormat != null)
		{
			m_charFormat = charFormat.CloneInt() as WCharacterFormat;
		}
	}

	protected override object CloneImpl()
	{
		XmlParagraphItem xmlParagraphItem = base.CloneImpl() as XmlParagraphItem;
		if (m_charFormat != null)
		{
			xmlParagraphItem.CharacterFormat.ImportContainer(m_charFormat);
		}
		if (xmlParagraphItem.m_xmlNode != null)
		{
			xmlParagraphItem.m_xmlNode = UtilityMethods.CloneStream(m_xmlNode as MemoryStream);
		}
		xmlParagraphItem.m_relations = new Dictionary<string, DictionaryEntry>();
		if (m_relations != null)
		{
			foreach (string key in m_relations.Keys)
			{
				DictionaryEntry dictionaryEntry = m_relations[key];
				DictionaryEntry value = new DictionaryEntry((string)dictionaryEntry.Key, (string)dictionaryEntry.Value);
				xmlParagraphItem.Relations.Add(key, value);
			}
		}
		xmlParagraphItem.m_imageRelations = new Dictionary<string, ImageRecord>();
		if (m_imageRelations != null)
		{
			foreach (string key2 in m_imageRelations.Keys)
			{
				xmlParagraphItem.ImageRelations.Add(key2, m_imageRelations[key2]);
			}
		}
		return xmlParagraphItem;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (ImageRelations.Count > 0)
		{
			string[] array = new string[ImageRelations.Count];
			ImageRelations.Keys.CopyTo(array, 0);
			for (int i = 0; i < array.Length; i++)
			{
				ImageRecord imageRecord = ImageRelations[array[i]];
				imageRecord = ((!imageRecord.IsMetafile) ? doc.Images.LoadImage(imageRecord.ImageBytes) : doc.Images.LoadMetaFileImage(imageRecord.m_imageBytes, isCompressed: true));
				ImageRelations[array[i]] = imageRecord;
			}
		}
		if (doc.DocxPackage == null && base.Document.DocxPackage != null)
		{
			doc.DocxPackage = base.Document.DocxPackage.Clone();
		}
		else if (doc.DocxPackage != null && base.Document.DocxPackage != null)
		{
			UpdateXmlParts(doc);
		}
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		base.Document.FloatingItems.Add(this);
	}

	internal override void Detach()
	{
		base.Detach();
		base.Document.FloatingItems.Remove(this);
	}

	private void UpdateXmlParts(WordDocument destination)
	{
		if (Relations.Count == 0)
		{
			return;
		}
		string[] array = new string[Relations.Count];
		Relations.Keys.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			DictionaryEntry value = Relations[array[i]];
			if (value.Key == null || !value.Key.ToString().EndsWith("hyperlink"))
			{
				string[] array2 = value.Value.ToString().Split('/');
				PartContainer srcContainer = base.Document.DocxPackage.FindPartContainer("word/");
				PartContainer destContainer = destination.DocxPackage.FindPartContainer("word/");
				string text = UpdateXmlPartContainer(base.Document.DocxPackage, srcContainer, destContainer, array2, 0);
				if (text != string.Empty)
				{
					value.Value = value.Value.ToString().Replace(array2[^1], text);
					Relations[array[i]] = value;
				}
			}
		}
	}

	private string UpdateXmlPartContainer(Package srcPackage, PartContainer srcContainer, PartContainer destContainer, string[] parts, int index)
	{
		string result = string.Empty;
		for (int i = index; i < parts.Length; i++)
		{
			if (i < parts.Length - 1)
			{
				string text = parts[i] + "/";
				if (destContainer.XmlPartContainers.ContainsKey(text))
				{
					destContainer = destContainer.XmlPartContainers[text];
					srcContainer = srcContainer.XmlPartContainers[text];
					result = UpdateXmlPartContainer(srcPackage, srcContainer, destContainer, parts, i + 1);
				}
				else
				{
					PartContainer partContainer = new PartContainer();
					partContainer.Name = text;
					result = srcContainer.XmlPartContainers[text].CopyXmlPartContainer(partContainer, srcPackage, parts, i + 1);
					destContainer.XmlPartContainers.Add(text, partContainer);
				}
				break;
			}
			result = srcContainer.CopyXmlPartItems(destContainer, srcPackage, parts[i]);
		}
		return result;
	}

	internal override void Close()
	{
		base.Close();
		if (m_xmlNode != null)
		{
			m_xmlNode.Close();
		}
		if (m_relations != null)
		{
			m_relations.Clear();
			m_relations = null;
		}
		if (m_imageRelations == null)
		{
			return;
		}
		foreach (ImageRecord value in m_imageRelations.Values)
		{
			value.Close();
		}
		m_imageRelations.Clear();
		m_imageRelations = null;
	}
}

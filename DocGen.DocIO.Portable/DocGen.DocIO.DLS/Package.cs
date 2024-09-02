using DocGen.Compression.Zip;

namespace DocGen.DocIO.DLS;

internal class Package : PartContainer
{
	internal PartContainer FindPartContainer(string containerName)
	{
		string[] nameParts = containerName.Split('/');
		return EnsurePartContainer(nameParts, 0);
	}

	internal Part FindPart(string fullPartName)
	{
		int num = fullPartName.LastIndexOf("/");
		string containerName = fullPartName.Substring(0, num + 1);
		PartContainer partContainer = FindPartContainer(containerName);
		if (partContainer != null)
		{
			string key = fullPartName.Substring(num + 1, fullPartName.Length - (num + 1));
			if (partContainer.XmlParts.ContainsKey(key))
			{
				return partContainer.XmlParts[key];
			}
		}
		return null;
	}

	internal void Load(ZipArchive zipArc)
	{
		int i = 0;
		for (int count = zipArc.Count; i < count; i++)
		{
			LoadPart(zipArc[i]);
		}
	}

	private void LoadPart(ZipArchiveItem item)
	{
		string[] array = item.ItemName.Split('/');
		_ = array[^1];
		PartContainer partContainer = EnsurePartContainer(array, 0);
		if (array.Length > 1 && array[^2].EndsWith("_rels"))
		{
			partContainer.LoadRelations(item);
		}
		else
		{
			partContainer.AddPart(item);
		}
	}

	internal new Package Clone()
	{
		Package package = new Package();
		package.Name = m_name;
		if (m_xmlParts != null && m_xmlParts.Count > 0)
		{
			foreach (string key in m_xmlParts.Keys)
			{
				package.XmlParts.Add(key, m_xmlParts[key].Clone());
			}
		}
		if (m_relations != null && m_relations.Count > 0)
		{
			foreach (string key2 in m_relations.Keys)
			{
				package.Relations.Add(key2, m_relations[key2].Clone() as Relations);
			}
		}
		if (m_xmlPartContainers != null && m_xmlPartContainers.Count > 0)
		{
			foreach (string key3 in m_xmlPartContainers.Keys)
			{
				package.XmlPartContainers.Add(key3, m_xmlPartContainers[key3].Clone());
			}
		}
		return package;
	}

	internal override void Close()
	{
		base.Close();
	}
}

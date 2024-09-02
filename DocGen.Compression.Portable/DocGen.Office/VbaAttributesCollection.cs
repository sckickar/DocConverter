using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class VbaAttributesCollection : CollectionBase<VbaAttribute>
{
	private VbaModule m_module;

	internal VbaAttribute this[string name]
	{
		get
		{
			using (IEnumerator<VbaAttribute> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VbaAttribute current = enumerator.Current;
					if (string.Equals(name, current.Name, StringComparison.OrdinalIgnoreCase))
					{
						return current;
					}
				}
			}
			return null;
		}
	}

	internal VbaAttributesCollection(VbaModule module)
	{
		m_module = module;
	}

	internal VbaAttribute AddAttribute(string name, string value, bool isText)
	{
		VbaAttribute vbaAttribute = new VbaAttribute();
		vbaAttribute.Name = name;
		vbaAttribute.Value = value;
		vbaAttribute.IsText = isText;
		Add(vbaAttribute);
		return vbaAttribute;
	}

	internal VbaAttributesCollection Clone(VbaModule parent)
	{
		VbaAttributesCollection vbaAttributesCollection = (VbaAttributesCollection)Clone();
		vbaAttributesCollection.m_module = parent;
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			VbaAttribute vbaAttribute = base.InnerList[i];
			vbaAttributesCollection.Add(vbaAttribute.Clone());
		}
		return vbaAttributesCollection;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Office;

internal class VbaModulesCollection : CollectionBase<VbaModule>, IVbaModules, IEnumerable
{
	private VbaProject m_project;

	internal VbaProject Project => m_project;

	public IVbaModule this[string name]
	{
		get
		{
			using (IEnumerator<VbaModule> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VbaModule current = enumerator.Current;
					if (string.Equals(name, current.Name, StringComparison.OrdinalIgnoreCase))
					{
						return current;
					}
				}
			}
			return null;
		}
	}

	public new IVbaModule this[int index] => base.InnerList[index];

	internal VbaModulesCollection()
	{
	}

	internal VbaModulesCollection(VbaProject project)
	{
		m_project = project;
	}

	public IVbaModule Add(string name, VbaModuleType type)
	{
		VbaModule vbaModule = new VbaModule(this);
		vbaModule.Name = name;
		vbaModule.Type = type;
		vbaModule.InitializeAttributes(name, Project.ProjectCLSID);
		Add(vbaModule);
		return vbaModule;
	}

	public void Remove(string name)
	{
		if (this[name] != null)
		{
			Remove(this[name] as VbaModule);
		}
	}

	internal void Dispose()
	{
		using IEnumerator<VbaModule> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Dispose();
		}
	}

	internal VbaModulesCollection Clone(VbaProject parent)
	{
		VbaModulesCollection vbaModulesCollection = (VbaModulesCollection)Clone();
		vbaModulesCollection.m_project = parent;
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			VbaModule vbaModule = base.InnerList[i];
			vbaModulesCollection.Add(vbaModule.Clone(vbaModulesCollection));
		}
		return vbaModulesCollection;
	}
}

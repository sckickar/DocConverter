using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DocGen.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Styles;

[Serializable]
internal abstract class StyleInfoStore : IDisposable, IStyleInfo, ISerializable, ICloneable, IXmlSerializable
{
	private BitVector32[] data;

	private BitVector32[] include;

	private BitVector32[] changed;

	private StyleInfoObjectStore objects;

	private StyleInfoObjectStore expandableObjects;

	private Hashtable m_cache;

	private const int maxbits = 31;

	private const int maxbits1 = 30;

	private static Hashtable xmlSerializers = new Hashtable();

	[XmlIgnore]
	public ICollection StyleInfoProperties => StaticDataStore.styleInfoProperties;

	[XmlIgnore]
	protected abstract StaticData StaticDataStore { get; }

	[XmlIgnore]
	public string[] PropertyGridSortOrder => StaticDataStore.PropertyGridSortOrder;

	StyleInfoStore IStyleInfo.Store => this;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[XmlIgnore]
	public bool IsEmpty
	{
		get
		{
			if (include == null)
			{
				return true;
			}
			for (int i = 0; i < include.Length; i++)
			{
				if (!include[i].Equals(new BitVector32(0)))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[XmlIgnore]
	public bool IsChanged
	{
		get
		{
			for (int i = 0; i < changed.Length; i++)
			{
				if (!changed[i].Equals(new BitVector32(0)))
				{
					return true;
				}
			}
			return false;
		}
	}

	public StyleInfoProperty FindStyleInfoProperty(string name)
	{
		if (m_cache == null)
		{
			StaticData staticDataStore = StaticDataStore;
			m_cache = new Hashtable(staticDataStore.styleInfoProperties.Count);
			int i = 0;
			for (int count = staticDataStore.styleInfoProperties.Count; i < count; i++)
			{
				StyleInfoProperty styleInfoProperty = staticDataStore.styleInfoProperties[i] as StyleInfoProperty;
				m_cache[styleInfoProperty.PropertyName] = styleInfoProperty;
			}
		}
		return m_cache[name] as StyleInfoProperty;
	}

	private void FixVectorCount()
	{
		StaticData staticDataStore = StaticDataStore;
		int num = (staticDataStore.styleInfoProperties.Count + 30) / 31;
		if (include.Length < num)
		{
			BitVector32[] array = include;
			BitVector32[] array2 = changed;
			include = new BitVector32[num];
			Array.Copy(array, 0, include, 0, array.Length);
			changed = new BitVector32[num];
			Array.Copy(array2, 0, changed, 0, array2.Length);
		}
		if (data == null && staticDataStore.dataVectorCount > 0)
		{
			data = new BitVector32[staticDataStore.dataVectorCount];
		}
	}

	protected StyleInfoStore(SerializationInfo info, StreamingContext context)
	{
		StaticData staticDataStore = StaticDataStore;
		data = ((staticDataStore.dataVectorCount > 0) ? new BitVector32[staticDataStore.dataVectorCount] : null);
		int num = (staticDataStore.styleInfoProperties.Count + 30) / 31;
		if (num == 0)
		{
			throw new InvalidOperationException("Static ctor has not been called.");
		}
		include = new BitVector32[num];
		changed = new BitVector32[num];
		objects = ((staticDataStore.objectCount > 0) ? new StyleInfoObjectStore() : null);
		expandableObjects = ((staticDataStore.expandableObjectCount > 0) ? new StyleInfoObjectStore() : null);
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			StyleInfoProperty styleInfoProperty = FindStyleInfoProperty(enumerator.Name);
			if (styleInfoProperty == null)
			{
				continue;
			}
			if (styleInfoProperty.ObjectStoreKey != -1)
			{
				object value;
				if (enumerator.Value is string && styleInfoProperty.PropertyType.IsPrimitive)
				{
					value = NullableHelper.ChangeType(enumerator.Value, styleInfoProperty.PropertyType);
				}
				else if (styleInfoProperty.PropertyType == typeof(Type))
				{
					object value2 = enumerator.Value;
					value = ((!(value2 is string)) ? ((Type)value2) : Type.GetType((string)value2));
				}
				else
				{
					value = enumerator.Value;
				}
				SetValue(styleInfoProperty, value);
			}
			else if (styleInfoProperty.ExpandableObjectStoreKey != -1)
			{
				SetValue(styleInfoProperty, enumerator.Value);
			}
			else
			{
				SetValue(styleInfoProperty, Convert.ToInt16(enumerator.Value));
			}
		}
	}

	protected StyleInfoStore()
	{
		StaticData staticDataStore = StaticDataStore;
		data = ((staticDataStore.dataVectorCount > 0) ? new BitVector32[staticDataStore.dataVectorCount] : null);
		int num = (staticDataStore.styleInfoProperties.Count + 30) / 31;
		if (num == 0)
		{
			throw new InvalidOperationException("Static ctor has not been called.");
		}
		include = new BitVector32[num];
		changed = new BitVector32[num];
		objects = null;
		expandableObjects = null;
	}

	public virtual object Clone()
	{
		StyleInfoStore styleInfoStore = null;
		if (Activator.CreateInstance(GetType()) is StyleInfoStore)
		{
			styleInfoStore = Activator.CreateInstance(GetType()) as StyleInfoStore;
		}
		CopyTo(styleInfoStore);
		return styleInfoStore;
	}

	public void CopyTo(StyleInfoStore target)
	{
		target.FixVectorCount();
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			if (HasValue(styleInfoProperty))
			{
				target._AssignProperty(styleInfoProperty, this);
			}
		}
	}

	public void Dispose()
	{
		if (objects != null)
		{
			objects.Dispose();
			objects = null;
		}
		if (expandableObjects != null)
		{
			expandableObjects.Dispose();
			expandableObjects = null;
		}
		changed = null;
		data = null;
		include = null;
		GC.SuppressFinalize(this);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleInfoStore styleInfoStore))
		{
			return false;
		}
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			if ((HasValue(styleInfoProperty) || styleInfoStore.HasValue(styleInfoProperty)) && !_EqualsProperty(styleInfoProperty, styleInfoStore))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		uint num = 2016594420u;
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			int num2 = 0;
			if (!HasValue(styleInfoProperty))
			{
				continue;
			}
			if (styleInfoProperty.ExpandableObjectStoreKey != -1)
			{
				num2 = ((StyleInfoStore)GetValue(styleInfoProperty)).GetHashCode();
			}
			else
			{
				if (styleInfoProperty.ObjectStoreKey != -1)
				{
					continue;
				}
				num2 = GetShortValue(styleInfoProperty);
			}
			num ^= (uint)((int)(num << 5) + num2) + (num >> 2);
		}
		return (int)num;
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			if (!styleInfoProperty.IsSerializable || !HasValue(styleInfoProperty))
			{
				continue;
			}
			if (styleInfoProperty.ObjectStoreKey != -1 || styleInfoProperty.ExpandableObjectStoreKey != -1)
			{
				if (styleInfoProperty.PropertyType == typeof(Type))
				{
					object value = GetValue(styleInfoProperty);
					string value2 = "";
					if (value is Type)
					{
						value2 = ValueConvert.GetTypeName((Type)value);
					}
					info.AddValue(styleInfoProperty.PropertyName, value2);
				}
				else
				{
					info.AddValue(styleInfoProperty.PropertyName, GetValue(styleInfoProperty), styleInfoProperty.PropertyType);
				}
			}
			else if (styleInfoProperty.PropertyType.IsEnum)
			{
				info.AddValue(styleInfoProperty.PropertyName, Enum.ToObject(styleInfoProperty.PropertyType, GetShortValue(styleInfoProperty)));
			}
			else
			{
				info.AddValue(styleInfoProperty.PropertyName, GetShortValue(styleInfoProperty));
			}
		}
	}

	public override string ToString()
	{
		return _ToString("");
	}

	private string _ToString(string prefix)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			if (!styleInfoProperty.IsSerializable || !HasValue(styleInfoProperty))
			{
				continue;
			}
			if (styleInfoProperty.IsExpandable)
			{
				if (GetValue(styleInfoProperty) is StyleInfoStore styleInfoStore)
				{
					stringBuilder.Append(styleInfoStore._ToString(prefix + styleInfoProperty.PropertyName + "."));
				}
			}
			else
			{
				stringBuilder.Append(prefix);
				stringBuilder.Append(styleInfoProperty.PropertyName);
				stringBuilder.Append(" = ");
				stringBuilder.Append(styleInfoProperty.FormatValue(GetValue(styleInfoProperty)));
				stringBuilder.Append(Environment.NewLine);
			}
		}
		return stringBuilder.ToString();
	}

	void IStyleInfo.ParseString(string s)
	{
		throw new InvalidOperationException();
	}

	public void ResetChangedBits()
	{
		FixVectorCount();
		for (int i = 0; i < changed.Length; i++)
		{
			changed[i] = new BitVector32(0);
		}
	}

	public void Clear()
	{
		FixVectorCount();
		for (int i = 0; i < include.Length; i++)
		{
			include[i] = new BitVector32(0);
		}
	}

	public virtual bool HasValue(StyleInfoProperty sip)
	{
		if (include != null && sip.BitVectorIndex < include.Length)
		{
			return include[sip.BitVectorIndex][sip.BitVectorMask];
		}
		return false;
	}

	public bool IsValueModified(StyleInfoProperty sip)
	{
		if (sip.BitVectorIndex < changed.Length)
		{
			return changed[sip.BitVectorIndex][sip.BitVectorMask];
		}
		return false;
	}

	public void SetValueModified(StyleInfoProperty sip, bool value)
	{
		FixVectorCount();
		changed[sip.BitVectorIndex][sip.BitVectorMask] = value;
	}

	public void ResetValue(StyleInfoProperty sip)
	{
		if (sip == null)
		{
			return;
		}
		FixVectorCount();
		if (sip.ObjectStoreKey != -1)
		{
			if (HasValue(sip))
			{
				_ResetObject(sip.ObjectStoreKey, sip.IsDisposable);
			}
		}
		else if (sip.ExpandableObjectStoreKey != -1 && HasValue(sip))
		{
			_ResetExpandableObject(sip.ExpandableObjectStoreKey);
		}
		include[sip.BitVectorIndex][sip.BitVectorMask] = false;
	}

	public virtual object GetValue(StyleInfoProperty sipSrc)
	{
		StyleInfoProperty styleInfoProperty = ConversionSIP(sipSrc);
		if (styleInfoProperty == null)
		{
			return null;
		}
		if (styleInfoProperty.ObjectStoreKey != -1)
		{
			return _GetObject(styleInfoProperty.ObjectStoreKey);
		}
		if (styleInfoProperty.ExpandableObjectStoreKey != -1)
		{
			return _GetExpandableObject(styleInfoProperty.ExpandableObjectStoreKey);
		}
		if (styleInfoProperty.PropertyType.IsEnum)
		{
			return Enum.ToObject(styleInfoProperty.PropertyType, GetShortValue(styleInfoProperty));
		}
		return NullableHelper.ChangeType(GetShortValue(styleInfoProperty), styleInfoProperty.PropertyType);
	}

	public short GetShortValue(StyleInfoProperty sip)
	{
		if (sip.ObjectStoreKey != -1 || sip.ExpandableObjectStoreKey != -1)
		{
			throw new InvalidOperationException(sip.PropertyName + " is not of type Int16.");
		}
		return (short)data[sip.DataVectorIndex][sip.DataVectorSection];
	}

	public void SetValue(StyleInfoProperty sip, object value)
	{
		if (sip.ObjectStoreKey != -1)
		{
			FixVectorCount();
			_SetObject(sip.ObjectStoreKey, value, sip.IsDisposable);
			include[sip.BitVectorIndex][sip.BitVectorMask] = true;
		}
		else if (sip.ExpandableObjectStoreKey != -1)
		{
			FixVectorCount();
			_SetExpandableObject(sip.ExpandableObjectStoreKey, value);
			include[sip.BitVectorIndex][sip.BitVectorMask] = true;
		}
		else
		{
			SetValue(sip, Convert.ToInt16(value));
		}
	}

	public void SetValue(StyleInfoProperty sip, short value)
	{
		FixVectorCount();
		if (sip.ObjectStoreKey != -1 || sip.ExpandableObjectStoreKey != -1)
		{
			throw new InvalidOperationException(sip.PropertyName + " is not a Int16.");
		}
		data[sip.DataVectorIndex][sip.DataVectorSection] = value;
		include[sip.BitVectorIndex][sip.BitVectorMask] = true;
	}

	private bool _IsSubsetProperty(StyleInfoProperty sip, StyleInfoStore style)
	{
		if (!style.HasValue(sip))
		{
			return true;
		}
		if (HasValue(sip))
		{
			if (sip.ObjectStoreKey != -1)
			{
				object obj = _GetObject(sip.ObjectStoreKey);
				object obj2 = style._GetObject(sip.ObjectStoreKey);
				if (obj == obj2)
				{
					return true;
				}
				if (obj == null || obj2 == null)
				{
					return false;
				}
				if (sip.IsExpandable)
				{
					throw new InvalidOperationException("IsExpandable");
				}
				return obj.Equals(obj2);
			}
			if (sip.ExpandableObjectStoreKey != -1)
			{
				object obj3 = _GetExpandableObject(sip.ExpandableObjectStoreKey);
				object obj4 = style._GetExpandableObject(sip.ExpandableObjectStoreKey);
				if (obj3 == obj4)
				{
					return true;
				}
				if (obj3 == null || obj4 == null)
				{
					return false;
				}
				if (sip.IsExpandable)
				{
					return ((IStyleInfo)obj3).IsSubset(obj4 as IStyleInfo);
				}
				throw new InvalidOperationException("Not IsExpandable");
			}
			return GetShortValue(sip) == style.GetShortValue(sip);
		}
		return false;
	}

	private StyleInfoProperty GetCurrentStorageSIP(StyleInfoProperty sip)
	{
		StyleInfoProperty result = null;
		if (sip != null)
		{
			result = FindStyleInfoProperty(sip.PropertyName);
		}
		return result;
	}

	private StyleInfoProperty ConversionSIP(StyleInfoProperty sipSrc)
	{
		StyleInfoProperty styleInfoProperty = null;
		if (sipSrc != null && StaticDataStore.StyleInfoType == sipSrc.ComponentType)
		{
			return sipSrc;
		}
		return FindStyleInfoProperty(sipSrc.PropertyName);
	}

	private void _AssignProperty(StyleInfoProperty sip, StyleInfoStore style)
	{
		if (sip.ObjectStoreKey != -1 || sip.ExpandableObjectStoreKey != -1)
		{
			object obj = style.GetValue(sip);
			if (sip.IsCloneable)
			{
				if (obj is IStyleCloneable)
				{
					if (((IStyleCloneable)obj).ShouldClone())
					{
						obj = ((IStyleCloneable)obj).Clone();
					}
				}
				else if (obj is ICloneable)
				{
					obj = ((ICloneable)obj).Clone();
				}
			}
			SetValue(sip, obj);
		}
		else
		{
			SetValue(sip, style.GetShortValue(sip));
		}
	}

	private bool _EqualsProperty(StyleInfoProperty sip, StyleInfoStore style)
	{
		if (!HasValue(sip) || !style.HasValue(sip))
		{
			return false;
		}
		if (sip.ObjectStoreKey != -1 || sip.ExpandableObjectStoreKey != -1)
		{
			return _EqualsObject(GetValue(sip), style.GetValue(sip));
		}
		return GetShortValue(sip) == style.GetShortValue(sip);
	}

	private void _ModifyProperty(StyleInfoProperty sip, StyleInfoStore style, StyleModifyType mt)
	{
		switch (mt)
		{
		case StyleModifyType.Exclude:
			if (style.HasValue(sip))
			{
				ResetValue(sip);
			}
			break;
		case StyleModifyType.ApplyNew:
			if (!HasValue(sip) && style.HasValue(sip))
			{
				_AssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Override:
			if (style.HasValue(sip))
			{
				_AssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Changes:
			if (style == null)
			{
				throw new ArgumentNullException("style");
			}
			if (!style.IsValueModified(sip))
			{
				break;
			}
			goto case StyleModifyType.Copy;
		case StyleModifyType.Copy:
			if (!style.HasValue(sip))
			{
				ResetValue(sip);
			}
			else
			{
				_AssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Remove:
			break;
		}
	}

	private void _IStyleInfoModify(StyleInfoProperty sip, StyleInfoStore style, StyleModifyType mt)
	{
		((IStyleInfo)_GetExpandableObject(sip.ExpandableObjectStoreKey)).ModifyStyle(style._GetExpandableObject(sip.ExpandableObjectStoreKey) as IStyleInfo, mt);
	}

	private void _ModifyExpandableProperty(StyleInfoProperty sip, StyleInfoStore style, StyleModifyType mt)
	{
		switch (mt)
		{
		case StyleModifyType.Exclude:
			if (style.HasValue(sip) && HasValue(sip))
			{
				_IStyleInfoModify(sip, style, mt);
			}
			break;
		case StyleModifyType.ApplyNew:
			if (!HasValue(sip) && style.HasValue(sip))
			{
				_AssignProperty(sip, style);
			}
			else if (HasValue(sip) && style.HasValue(sip))
			{
				_IStyleInfoModify(sip, style, mt);
			}
			break;
		case StyleModifyType.Override:
			if (style.HasValue(sip))
			{
				if (!HasValue(sip))
				{
					_AssignProperty(sip, style);
				}
				else
				{
					_IStyleInfoModify(sip, style, mt);
				}
			}
			break;
		case StyleModifyType.Changes:
			if (style.IsValueModified(sip))
			{
				if (!style.HasValue(sip))
				{
					ResetValue(sip);
				}
				else if (!HasValue(sip))
				{
					_AssignProperty(sip, style);
				}
				else
				{
					_IStyleInfoModify(sip, style, mt);
				}
			}
			break;
		case StyleModifyType.Copy:
			if (!style.HasValue(sip))
			{
				ResetValue(sip);
			}
			else
			{
				_AssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Remove:
			break;
		}
	}

	private static bool _EqualsObject(object obj1, object obj2)
	{
		if (obj1 != obj2)
		{
			if (obj1 != null && obj2 != null)
			{
				return obj1.Equals(obj2);
			}
			return false;
		}
		return true;
	}

	private object _GetObject(int key)
	{
		bool found;
		if (objects != null)
		{
			return objects.GetObject(key, out found);
		}
		return null;
	}

	private void _SetObject(int key, object value, bool isDisposable)
	{
		if (objects == null)
		{
			objects = new StyleInfoObjectStore();
		}
		object @object = objects.GetObject(key);
		if (@object != value)
		{
			if (isDisposable)
			{
				_Dispose(@object);
			}
			objects.SetObject(key, value);
		}
	}

	private void _ResetObject(int key, bool isDisposable)
	{
		if (objects != null)
		{
			object oldObject = objects.GetObject(key) as IDisposable;
			objects.RemoveObject(key);
			if (isDisposable)
			{
				_Dispose(oldObject);
			}
		}
	}

	private void _Dispose(object oldObject)
	{
		if (oldObject is IStyleCloneable styleCloneable)
		{
			if (styleCloneable.ShouldDispose())
			{
				styleCloneable.Dispose();
			}
		}
		else if (oldObject is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	private object _GetExpandableObject(int key)
	{
		bool found;
		if (expandableObjects != null)
		{
			return expandableObjects.GetObject(key, out found);
		}
		return null;
	}

	private void _SetExpandableObject(int key, object value)
	{
		if (expandableObjects == null)
		{
			expandableObjects = new StyleInfoObjectStore();
		}
		object @object = expandableObjects.GetObject(key);
		if (@object != value)
		{
			if (@object is IDisposable disposable)
			{
				disposable.Dispose();
			}
			expandableObjects.SetObject(key, value);
		}
	}

	private void _ResetExpandableObject(int key)
	{
		if (expandableObjects != null)
		{
			IDisposable disposable = expandableObjects.GetObject(key) as IDisposable;
			expandableObjects.RemoveObject(key);
			disposable?.Dispose();
		}
	}

	private void _CopyStyle(StyleInfoStore style)
	{
		data = (BitVector32[])style.data.Clone();
		include = (BitVector32[])style.include.Clone();
		changed = (BitVector32[])style.changed.Clone();
		objects = ((style.objects != null) ? new StyleInfoObjectStore() : null);
		expandableObjects = ((style.expandableObjects != null) ? new StyleInfoObjectStore() : null);
		StaticData staticDataStore = StaticDataStore;
		for (int i = 0; i < staticDataStore.styleInfoProperties.Count; i++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
			if (styleInfoProperty.ObjectStoreKey != -1)
			{
				if (HasValue(styleInfoProperty))
				{
					objects.SetObject(styleInfoProperty.ObjectStoreKey, objects.GetObject(styleInfoProperty.ObjectStoreKey));
				}
			}
			else if (styleInfoProperty.ExpandableObjectStoreKey != -1 && HasValue(styleInfoProperty))
			{
				expandableObjects.SetObject(styleInfoProperty.ExpandableObjectStoreKey, expandableObjects.GetObject(styleInfoProperty.ExpandableObjectStoreKey));
			}
		}
		FixVectorCount();
	}

	public bool IsSubset(IStyleInfo istyle)
	{
		if (istyle is StyleInfoStore { IsEmpty: false } styleInfoStore)
		{
			StaticData staticDataStore = StaticDataStore;
			for (int i = 0; i < staticDataStore.styleInfoProperties.Count; i++)
			{
				StyleInfoProperty sip = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
				if (!_IsSubsetProperty(sip, styleInfoStore))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ModifyStyle(IStyleInfo istyle, StyleModifyType mt)
	{
		StyleInfoStore store = istyle.Store;
		store.FixVectorCount();
		StaticData staticDataStore = StaticDataStore;
		for (int i = 0; i < staticDataStore.styleInfoProperties.Count; i++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
			if (styleInfoProperty.IsExpandable)
			{
				_ModifyExpandableProperty(styleInfoProperty, store, mt);
			}
			else
			{
				_ModifyProperty(styleInfoProperty, store, mt);
			}
		}
	}

	public void ModifyStyleKeepChanges(IStyleInfo istyle, StyleModifyType mt)
	{
		StyleInfoStore store = istyle.Store;
		store.FixVectorCount();
		StaticData staticDataStore = StaticDataStore;
		for (int i = 0; i < staticDataStore.styleInfoProperties.Count; i++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
			if (!_EqualsProperty(styleInfoProperty, store))
			{
				SetValueModified(styleInfoProperty, value: true);
				if (styleInfoProperty.IsExpandable)
				{
					_ModifyExpandableProperty(styleInfoProperty, store, mt);
				}
				else
				{
					_ModifyProperty(styleInfoProperty, store, mt);
				}
			}
		}
	}

	public void MergeStyle(IStyleInfo istyle)
	{
		StyleInfoStore styleInfoStore = istyle as StyleInfoStore;
		StaticData staticDataStore = StaticDataStore;
		for (int i = 0; i < staticDataStore.styleInfoProperties.Count; i++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
			if (styleInfoProperty.IsExpandable)
			{
				IStyleInfo styleInfo = (IStyleInfo)_GetExpandableObject(styleInfoProperty.ExpandableObjectStoreKey);
				if (styleInfo != null && styleInfoStore._GetExpandableObject(styleInfoProperty.ExpandableObjectStoreKey) is IStyleInfo style)
				{
					styleInfo.MergeStyle(style);
					break;
				}
				ResetValue(styleInfoProperty);
			}
			else if (!_IsSubsetProperty(styleInfoProperty, styleInfoStore) || !styleInfoStore._IsSubsetProperty(styleInfoProperty, this))
			{
				ResetValue(styleInfoProperty);
			}
		}
	}

	public void InheritStyle(IStyleInfo istyle, StyleModifyType mt)
	{
		StyleInfoStore store = istyle.Store;
		store.FixVectorCount();
		StaticData staticDataStore = StaticDataStore;
		StaticData staticDataStore2 = store.StaticDataStore;
		Hashtable hashtable = new Hashtable();
		int i = 0;
		for (int count = staticDataStore.styleInfoProperties.Count; i < count; i++)
		{
			StyleInfoProperty styleInfoProperty = (StyleInfoProperty)staticDataStore.styleInfoProperties[i];
			hashtable[styleInfoProperty.PropertyName] = styleInfoProperty;
		}
		int j = 0;
		for (int count2 = staticDataStore2.styleInfoProperties.Count; j < count2; j++)
		{
			StyleInfoProperty styleInfoProperty2 = (StyleInfoProperty)staticDataStore2.styleInfoProperties[j];
			if (styleInfoProperty2.IsExpandable)
			{
				_ModifyExpandableProperty(styleInfoProperty2, store, mt);
			}
			else
			{
				MultiModifyProperty(styleInfoProperty2, store, mt);
			}
		}
	}

	private void MultiModifyProperty(StyleInfoProperty sip, StyleInfoStore style, StyleModifyType mt)
	{
		switch (mt)
		{
		case StyleModifyType.Exclude:
			if (style.HasValue(sip))
			{
				MultiResetValue(sip);
			}
			break;
		case StyleModifyType.ApplyNew:
			if (!HasValue(sip) && style.HasValue(sip))
			{
				MultiAssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Override:
			if (style.HasValue(sip))
			{
				MultiAssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Changes:
			if (style == null)
			{
				throw new ArgumentNullException("style");
			}
			if (!style.IsValueModified(sip))
			{
				break;
			}
			goto case StyleModifyType.Copy;
		case StyleModifyType.Copy:
			if (!style.HasValue(sip))
			{
				MultiResetValue(sip);
			}
			else
			{
				MultiAssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Remove:
			break;
		}
	}

	private void MultiAssignProperty(StyleInfoProperty sip, StyleInfoStore style)
	{
		StyleInfoProperty styleInfoProperty = null;
		if (sip != null)
		{
			styleInfoProperty = GetCurrentStorageSIP(sip);
		}
		if (sip.ObjectStoreKey != -1 || sip.ExpandableObjectStoreKey != -1)
		{
			object obj = style.GetValue(sip);
			if (sip.IsCloneable)
			{
				if (obj is IStyleCloneable)
				{
					if (((IStyleCloneable)obj).ShouldClone())
					{
						obj = ((IStyleCloneable)obj).Clone();
					}
				}
				else if (obj is ICloneable)
				{
					obj = ((ICloneable)obj).Clone();
				}
			}
			if (styleInfoProperty != null)
			{
				SetValue(styleInfoProperty, obj);
			}
		}
		else if (styleInfoProperty != null)
		{
			SetValue(styleInfoProperty, style.GetShortValue(sip));
		}
	}

	private void MultiResetValue(StyleInfoProperty sipSrc)
	{
		StyleInfoProperty currentStorageSIP = GetCurrentStorageSIP(sipSrc);
		if (currentStorageSIP == null)
		{
			return;
		}
		FixVectorCount();
		if (currentStorageSIP.ObjectStoreKey != -1)
		{
			if (HasValue(currentStorageSIP))
			{
				_ResetObject(currentStorageSIP.ObjectStoreKey, currentStorageSIP.IsDisposable);
			}
		}
		else if (currentStorageSIP.ExpandableObjectStoreKey != -1 && HasValue(currentStorageSIP))
		{
			_ResetExpandableObject(currentStorageSIP.ExpandableObjectStoreKey);
		}
		include[currentStorageSIP.BitVectorIndex][currentStorageSIP.BitVectorMask] = false;
	}

	private void MultiModifyExpandableProperty(StyleInfoProperty sip, StyleInfoStore style, StyleModifyType mt)
	{
		switch (mt)
		{
		case StyleModifyType.Exclude:
			if (style.HasValue(sip) && HasValue(sip))
			{
				_IStyleInfoModify(sip, style, mt);
			}
			break;
		case StyleModifyType.ApplyNew:
			if (!HasValue(sip) && style.HasValue(sip))
			{
				MultiAssignProperty(sip, style);
			}
			else if (HasValue(sip) && style.HasValue(sip))
			{
				_IStyleInfoModify(sip, style, mt);
			}
			break;
		case StyleModifyType.Override:
			if (style.HasValue(sip))
			{
				if (!HasValue(sip))
				{
					MultiAssignProperty(sip, style);
				}
				else
				{
					_IStyleInfoModify(sip, style, mt);
				}
			}
			break;
		case StyleModifyType.Changes:
			if (style.IsValueModified(sip))
			{
				if (!style.HasValue(sip))
				{
					MultiResetValue(sip);
				}
				else if (!HasValue(sip))
				{
					MultiAssignProperty(sip, style);
				}
				else
				{
					_IStyleInfoModify(sip, style, mt);
				}
			}
			break;
		case StyleModifyType.Copy:
			if (!style.HasValue(sip))
			{
				MultiResetValue(sip);
			}
			else
			{
				MultiAssignProperty(sip, style);
			}
			break;
		case StyleModifyType.Remove:
			break;
		}
	}

	protected virtual bool ProcessWriteXml(XmlWriter writer, StyleInfoProperty sip)
	{
		return sip.ProcessWriteXml(writer, this);
	}

	protected virtual bool ProcessReadXml(XmlReader reader, StyleInfoProperty sip)
	{
		return sip.ProcessReadXml(reader, this);
	}

	private void XmlSerialize(XmlWriter writer, object value, Type type, bool anyType)
	{
		XmlSerializer xmlSerializer;
		if (type != null && xmlSerializers.ContainsKey(type.FullName))
		{
			xmlSerializer = (XmlSerializer)xmlSerializers[type.FullName];
		}
		else
		{
			xmlSerializer = ((!(value == null || anyType)) ? new XmlSerializer(type) : new XmlSerializer(typeof(object), new Type[1] { type }));
			xmlSerializers[type.FullName] = xmlSerializer;
		}
		xmlSerializer.Serialize(writer, value);
	}

	private SerializeXmlBehavior GetDefaultSerializeXmlBehavior(StyleInfoProperty sip)
	{
		SerializeXmlBehavior serializeXmlBehavior = sip.SerializeXmlBehavior;
		if (serializeXmlBehavior == SerializeXmlBehavior.Default)
		{
			serializeXmlBehavior = ((sip.ObjectStoreKey != -1 && !(sip.PropertyType == typeof(Color)) && !(sip.PropertyType == typeof(Type)) && !(sip.PropertyType == typeof(string)) && !sip.PropertyType.IsPrimitive) ? SerializeXmlBehavior.SerializeWithXmlSerializer : SerializeXmlBehavior.SerializeAsString);
		}
		return serializeXmlBehavior;
	}

	public void WriteXml(XmlWriter writer)
	{
		foreach (StyleInfoProperty styleInfoProperty in StyleInfoProperties)
		{
			if (!styleInfoProperty.IsSerializable || !HasValue(styleInfoProperty) || ProcessWriteXml(writer, styleInfoProperty))
			{
				continue;
			}
			if (styleInfoProperty.IsExpandable)
			{
				writer.WriteStartElement(styleInfoProperty.PropertyName);
				if (GetValue(styleInfoProperty) is StyleInfoStore styleInfoStore)
				{
					styleInfoStore.WriteXml(writer);
				}
				writer.WriteEndElement();
			}
			else
			{
				if (styleInfoProperty.SerializeXmlBehavior == SerializeXmlBehavior.Skip)
				{
					continue;
				}
				object value = GetValue(styleInfoProperty);
				writer.WriteStartElement(styleInfoProperty.PropertyName);
				switch (GetDefaultSerializeXmlBehavior(styleInfoProperty))
				{
				case SerializeXmlBehavior.SerializeAsString:
					if (styleInfoProperty.PropertyType == typeof(Type))
					{
						writer.WriteString(ValueConvert.GetTypeName(value as Type));
					}
					else if (styleInfoProperty.PropertyType == typeof(bool))
					{
						writer.WriteString(styleInfoProperty.FormatValue(value).ToLower(CultureInfo.InvariantCulture));
					}
					else
					{
						writer.WriteString(styleInfoProperty.FormatValue(value));
					}
					break;
				case SerializeXmlBehavior.SerializeWithXmlSerializer:
					try
					{
						Type type = ((value != null) ? value.GetType() : styleInfoProperty.PropertyType);
						bool flag = type != styleInfoProperty.PropertyType;
						if (flag && !type.IsPrimitive && type.Module != typeof(object).Module)
						{
							writer.WriteStartElement("Type");
							writer.WriteString(ValueConvert.GetTypeName(type));
							writer.WriteEndElement();
						}
						XmlSerialize(writer, value, type, flag);
					}
					catch (InvalidOperationException)
					{
					}
					break;
				}
				writer.WriteEndElement();
			}
		}
	}

	public static void RegisterXmlSerializer(Type type, XmlSerializer xmlSerializer)
	{
		xmlSerializers[type.FullName] = xmlSerializer;
	}

	public void ReadXml(XmlReader r)
	{
		if (r.IsEmptyElement)
		{
			r.Read();
			return;
		}
		Hashtable hashtable = null;
		r.Read();
		while (!r.EOF && r.NodeType != XmlNodeType.EndElement)
		{
			if (r.NodeType == XmlNodeType.Element)
			{
				StyleInfoProperty styleInfoProperty = FindStyleInfoProperty(r.Name);
				if (styleInfoProperty != null)
				{
					if (ProcessReadXml(r, styleInfoProperty))
					{
						continue;
					}
					if (styleInfoProperty.IsExpandable)
					{
						StyleInfoBase styleInfoBase = ((styleInfoProperty.CreateObject == null) ? ((StyleInfoBase)Activator.CreateInstance(styleInfoProperty.PropertyType, new object[0])) : ((StyleInfoBase)styleInfoProperty.CreateObject(null, null)));
						StyleInfoStore store = styleInfoBase.Store;
						store.ReadXml(r);
						SetValue(styleInfoProperty, store);
						continue;
					}
					if (styleInfoProperty.SerializeXmlBehavior == SerializeXmlBehavior.Skip)
					{
						r.ReadInnerXml();
						r.ReadEndElement();
						continue;
					}
					XmlSerializer xmlSerializer = null;
					switch (GetDefaultSerializeXmlBehavior(styleInfoProperty))
					{
					case SerializeXmlBehavior.SerializeAsString:
					{
						string text2 = r.ReadString();
						object obj = ((!(styleInfoProperty.PropertyType == typeof(Type))) ? styleInfoProperty.ParseValue(text2) : Type.GetType(text2));
						SetValue(styleInfoProperty, (obj is DBNull) ? null : obj);
						if (obj is DBNull)
						{
							r.Read();
						}
						else
						{
							r.ReadEndElement();
						}
						break;
					}
					case SerializeXmlBehavior.SerializeWithXmlSerializer:
						try
						{
							while (r.NodeType == XmlNodeType.Whitespace)
							{
								r.Read();
							}
							r.ReadStartElement();
							while (r.NodeType == XmlNodeType.Whitespace)
							{
								r.Read();
							}
							while (r.Name == "Type")
							{
								xmlSerializer = null;
								string text = r.ReadString();
								if (hashtable == null)
								{
									hashtable = new Hashtable();
								}
								if (text.EndsWith(".exe") || text.EndsWith(".dll"))
								{
									text = text.Substring(0, text.Length - 4);
								}
								if (!hashtable.ContainsKey(text))
								{
									hashtable[text] = Type.GetType(text);
								}
								r.ReadEndElement();
								while (r.NodeType == XmlNodeType.Whitespace)
								{
									r.Read();
								}
							}
							if (r.AttributeCount <= 0 || hashtable == null)
							{
								xmlSerializer = ((!xmlSerializers.ContainsKey(styleInfoProperty.PropertyType.FullName)) ? new XmlSerializer(styleInfoProperty.PropertyType) : ((XmlSerializer)xmlSerializers[styleInfoProperty.PropertyType.FullName]));
							}
							else
							{
								Type[] array = new Type[hashtable.Count];
								hashtable.Values.CopyTo(array, 0);
								xmlSerializer = ((array.Length != 1 || !xmlSerializers.ContainsKey(array[0].FullName)) ? new XmlSerializer(styleInfoProperty.PropertyType, array) : ((XmlSerializer)xmlSerializers[array[0].FullName]));
							}
							object obj;
							try
							{
								obj = xmlSerializer.Deserialize(r);
							}
							catch (InvalidOperationException)
							{
								xmlSerializer = new XmlSerializer(typeof(object), new Type[1] { styleInfoProperty.PropertyType });
								obj = xmlSerializer.Deserialize(r);
							}
							if (obj != null && obj.GetType().FullName == "System.Xml.XmlNode[]")
							{
								XmlNode[] array2 = (XmlNode[])obj;
								if (array2.Length != 0 && array2[0] is XmlAttribute && ((XmlAttribute)array2[0]).Value == "DBNull")
								{
									obj = DBNull.Value;
								}
							}
							SetValue(styleInfoProperty, obj);
							r.ReadEndElement();
							while (r.NodeType == XmlNodeType.Whitespace)
							{
								r.Read();
							}
							if (r.NodeType == XmlNodeType.EndElement && r.Name == styleInfoProperty.PropertyName)
							{
								r.ReadEndElement();
							}
						}
						catch (InvalidOperationException)
						{
							styleInfoProperty.SerializeXmlBehavior = SerializeXmlBehavior.Skip;
							r.ReadEndElement();
							while (r.NodeType == XmlNodeType.Whitespace)
							{
								r.Read();
							}
							if (r.NodeType == XmlNodeType.EndElement && r.Name == styleInfoProperty.PropertyName)
							{
								r.ReadEndElement();
							}
						}
						break;
					}
				}
				else
				{
					r.Read();
				}
			}
			else
			{
				r.Read();
			}
		}
		if (!r.EOF)
		{
			r.Read();
		}
	}

	public XmlSchema GetSchema()
	{
		return null;
	}
}

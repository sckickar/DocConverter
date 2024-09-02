using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DocGen.Styles;

[Serializable]
[TypeConverter(typeof(StyleInfoBaseConverter))]
[RefreshProperties(RefreshProperties.Repaint)]
internal abstract class StyleInfoBase : IDisposable, IStyleInfo, IFormattable, IXmlSerializable
{
	internal StyleInfoIdentityBase identity;

	internal StyleInfoStore _store;

	internal StyleInfoObjectStore expandableObjects;

	private Hashtable baseStyleValuesCache;

	private bool cacheValues;

	private int updateCount;

	private bool changePending;

	private List<WeakReference> weakReferenceChangedListeners;

	internal bool inStyleChanged;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleInfoIdentityBase Identity
	{
		get
		{
			return identity;
		}
		set
		{
			identity = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleInfoStore Store => _store;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	protected StyleInfoObjectStore ExpandableObjects
	{
		get
		{
			if (expandableObjects == null)
			{
				expandableObjects = new StyleInfoObjectStore();
			}
			return expandableObjects;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool CacheValues
	{
		get
		{
			return cacheValues;
		}
		set
		{
			if (value != CacheValues)
			{
				cacheValues = value;
				if (value)
				{
					baseStyleValuesCache = new Hashtable();
				}
				else
				{
					baseStyleValuesCache = null;
				}
			}
		}
	}

	[Description("A list of listeners that will be referenced using a WeakReference")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<WeakReference> WeakReferenceChangedListeners
	{
		get
		{
			if (weakReferenceChangedListeners == null)
			{
				weakReferenceChangedListeners = new List<WeakReference>();
			}
			return weakReferenceChangedListeners;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsEmpty => _store.IsEmpty;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsChanged => _store.IsChanged;

	[Browsable(false)]
	public event StyleChangedEventHandler Changed;

	[Browsable(false)]
	public event StyleChangedEventHandler Changing;

	public void WriteXml(XmlWriter writer)
	{
		Store.WriteXml(writer);
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		Store.ReadXml(reader);
	}

	protected void SetStore(StyleInfoStore store)
	{
		_store = store;
		ClearCache();
	}

	public void ClearCache()
	{
		bool flag = CacheValues;
		CacheValues = false;
		CacheValues = flag;
	}

	protected StyleInfoBase()
	{
		identity = null;
		_store = null;
	}

	protected StyleInfoBase(StyleInfoStore store)
	{
		identity = null;
		_store = store;
	}

	protected StyleInfoBase(StyleInfoIdentityBase identity, StyleInfoStore store)
		: this(identity, store, cacheValues: false)
	{
	}

	protected StyleInfoBase(StyleInfoIdentityBase identity, StyleInfoStore store, bool cacheValues)
	{
		this.identity = identity;
		_store = store;
		CacheValues = cacheValues;
	}

	public virtual void Dispose()
	{
		if (identity == null || identity.IsDisposable)
		{
			if (expandableObjects != null)
			{
				expandableObjects.Dispose();
				expandableObjects = null;
			}
			if (identity != null)
			{
				identity.Dispose();
				identity = null;
			}
			if (_store != null)
			{
				_store.Dispose();
				_store = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleInfoBase styleInfoBase))
		{
			return false;
		}
		if (EqualsObject(identity, styleInfoBase.identity))
		{
			return EqualsObject(_store, styleInfoBase._store);
		}
		return false;
	}

	protected static bool EqualsObject(object obj1, object obj2)
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

	public override int GetHashCode()
	{
		if (_store == null)
		{
			return base.GetHashCode();
		}
		return _store.GetHashCode();
	}

	public override string ToString()
	{
		return ToString("", null);
	}

	public string ToString(string format, IFormatProvider provider)
	{
		if (_store == null)
		{
			return "Disposed";
		}
		string text = "";
		bool flag = false;
		if (format != null)
		{
			string[] array = format.Split(',');
			foreach (string text2 in array)
			{
				if (text2.Length <= 0)
				{
					continue;
				}
				switch (char.ToLower(text2[0], CultureInfo.InvariantCulture))
				{
				case 'p':
					text = text2.Substring(1);
					if (!text.EndsWith("."))
					{
						text += ".";
					}
					break;
				case 'd':
					flag = true;
					break;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (StyleInfoProperty styleInfoProperty in _store.StyleInfoProperties)
		{
			if (!styleInfoProperty.IsSerializable || (!flag && !HasValue(styleInfoProperty)))
			{
				continue;
			}
			if (styleInfoProperty.IsExpandable)
			{
				IFormattable formattable = _GetExpandableObjectProperty(styleInfoProperty) as IFormattable;
				format = "p" + text + styleInfoProperty.PropertyName;
				if (flag)
				{
					format += ",d";
				}
				stringBuilder.Append(formattable.ToString(format, provider));
				continue;
			}
			stringBuilder.Append(text);
			stringBuilder.Append(styleInfoProperty.PropertyName);
			stringBuilder.Append(" = ");
			string text3 = styleInfoProperty.FormatValue(GetValue(styleInfoProperty));
			if (HasValue(styleInfoProperty))
			{
				stringBuilder.Append(text3);
			}
			else
			{
				stringBuilder.Append("Default(" + text3 + ")");
			}
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder.ToString();
	}

	public void ParseString(string s)
	{
		string[] array = s.Split('\n');
		foreach (string text in array)
		{
			int num = text.IndexOf("=");
			if (num == -1)
			{
				continue;
			}
			string text2 = text.Substring(0, num - 1).Trim();
			int num2 = text2.IndexOf(".");
			StyleInfoProperty styleInfoProperty;
			if (num2 != -1)
			{
				text2 = text2.Substring(0, num2);
				styleInfoProperty = _store.FindStyleInfoProperty(text2);
				if (styleInfoProperty != null)
				{
					(_GetExpandableObjectProperty(styleInfoProperty) as IStyleInfo).ParseString(text.Substring(num2 + 1));
				}
				continue;
			}
			styleInfoProperty = _store.FindStyleInfoProperty(text2);
			if (styleInfoProperty != null)
			{
				string text3 = text.Substring(num + 1).Trim();
				if (text3.StartsWith("Default("))
				{
					ResetValue(styleInfoProperty);
					continue;
				}
				object value = styleInfoProperty.ParseValue(text3);
				SetValue(styleInfoProperty, value);
			}
		}
	}

	public void BeginUpdate()
	{
		updateCount++;
	}

	public void EndUpdate()
	{
		if (--updateCount == 0 && changePending)
		{
			if (!inStyleChanged)
			{
				inStyleChanged = true;
				OnStyleChanged(null);
				inStyleChanged = false;
			}
			changePending = false;
		}
	}

	protected internal abstract StyleInfoBase GetDefaultStyle();

	public virtual StyleInfoSubObjectIdentity CreateSubObjectIdentity(StyleInfoProperty sip)
	{
		throw new NotSupportedException("Nested subobjects are not supported.");
	}

	protected virtual StyleInfoStore GetDefaultStyleInfoStore(StyleInfoProperty sip)
	{
		StyleInfoBase styleInfoBase = IntGetDefaultStyleInfo(sip);
		if (styleInfoBase != null)
		{
			return styleInfoBase._store;
		}
		return GetDefaultStyle()._store;
	}

	protected virtual StyleInfoBase IntGetDefaultStyleInfo(StyleInfoProperty sip)
	{
		if (identity != null)
		{
			return identity.GetBaseStyle(this, sip);
		}
		return null;
	}

	protected virtual void OnStyleChanged(StyleInfoProperty sip)
	{
		if (sip != null)
		{
			_store.SetValueModified(sip, value: true);
		}
		if (updateCount > 0)
		{
			changePending = true;
			return;
		}
		if (identity != null)
		{
			identity.OnStyleChanged(this, sip);
		}
		StyleChangedEventArgs e = new StyleChangedEventArgs(sip);
		if (this.Changed != null)
		{
			this.Changed(this, e);
		}
		if (weakReferenceChangedListeners == null)
		{
			return;
		}
		WeakReference[] array = new WeakReference[weakReferenceChangedListeners.Count];
		weakReferenceChangedListeners.CopyTo(array, 0);
		WeakReference[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].Target is IStyleChanged styleChanged)
			{
				styleChanged.StyleChanged(e);
			}
		}
	}

	protected virtual void OnStyleChanging(StyleInfoProperty sip)
	{
		if (updateCount == 0)
		{
			if (identity != null)
			{
				identity.OnStyleChanging(this, sip);
			}
			if (this.Changing != null)
			{
				this.Changing(this, new StyleChangedEventArgs(sip));
			}
		}
	}

	public bool IsSubset(IStyleInfo istyle)
	{
		if (!(istyle is StyleInfoBase styleInfoBase))
		{
			throw new ArgumentNullException("style");
		}
		return _store.IsSubset(styleInfoBase._store);
	}

	public virtual void ModifyStyle(IStyleInfo istyle, StyleModifyType mt)
	{
		if (istyle == null)
		{
			throw new ArgumentNullException("style");
		}
		if (!inStyleChanged)
		{
			OnStyleChanging(null);
		}
		_store.ModifyStyle(istyle.Store, mt);
		if (!inStyleChanged)
		{
			inStyleChanged = true;
			OnStyleChanged(null);
			inStyleChanged = false;
		}
	}

	public void MergeStyle(IStyleInfo istyle)
	{
		if (!(istyle is StyleInfoBase styleInfoBase))
		{
			throw new ArgumentNullException("style");
		}
		_store.MergeStyle(styleInfoBase._store);
	}

	public bool HasValue(StyleInfoProperty sip)
	{
		return _store.HasValue(sip);
	}

	public void ResetValue(StyleInfoProperty sip)
	{
		if (!inStyleChanged)
		{
			OnStyleChanging(sip);
		}
		_store.ResetValue(sip);
		if (expandableObjects != null && sip.IsExpandable)
		{
			expandableObjects.RemoveObject(sip.ExpandableObjectStoreKey);
		}
		if (!inStyleChanged)
		{
			inStyleChanged = true;
			OnStyleChanged(sip);
			inStyleChanged = false;
		}
	}

	internal object _GetExpandableObjectProperty(StyleInfoProperty sip)
	{
		int expandableObjectStoreKey = sip.ExpandableObjectStoreKey;
		bool found;
		object obj = ExpandableObjects.GetObject(expandableObjectStoreKey, out found);
		if (obj == null)
		{
			object value = _store.GetValue(sip);
			obj = ((sip.CreateObject != null) ? sip.CreateObject(CreateSubObjectIdentity(sip), value) : ((value != null) ? Activator.CreateInstance(sip.PropertyType, CreateSubObjectIdentity(sip), value) : Activator.CreateInstance(sip.PropertyType, CreateSubObjectIdentity(sip))));
			expandableObjects.SetObject(expandableObjectStoreKey, obj);
		}
		return obj;
	}

	internal void OnSubObjectChanged(IStyleInfoSubObject subObject)
	{
		if (!inStyleChanged)
		{
			OnStyleChanging(subObject.Sip);
		}
		_store.SetValue(subObject.Sip, subObject.Data);
		if (!inStyleChanged)
		{
			inStyleChanged = true;
			OnStyleChanged(subObject.Sip);
			inStyleChanged = false;
		}
	}

	public object GetValue(StyleInfoProperty sip)
	{
		if (sip.IsExpandable)
		{
			return _GetExpandableObjectProperty(sip);
		}
		if (_store != null && _store.HasValue(sip))
		{
			return _store.GetValue(sip);
		}
		if (CacheValues)
		{
			object obj;
			if (!baseStyleValuesCache.ContainsKey(sip.PropertyKey))
			{
				obj = GetDefaultStyleInfoStore(sip).GetValue(sip);
				baseStyleValuesCache[sip.PropertyKey] = obj;
			}
			else
			{
				obj = baseStyleValuesCache[sip.PropertyKey];
			}
			return obj;
		}
		return GetDefaultStyleInfoStore(sip).GetValue(sip);
	}

	public short GetShortValue(StyleInfoProperty sip)
	{
		if (_store.HasValue(sip))
		{
			return _store.GetShortValue(sip);
		}
		return GetDefaultStyleInfoStore(sip).GetShortValue(sip);
	}

	public void SetValue(StyleInfoProperty sip, object value)
	{
		if (!inStyleChanged)
		{
			OnStyleChanging(sip);
		}
		if (sip.IsExpandable && value != null)
		{
			IStyleInfoSubObject styleInfoSubObject = (IStyleInfoSubObject)value;
			if (styleInfoSubObject.Owner != this)
			{
				styleInfoSubObject = styleInfoSubObject.MakeCopy(this, sip);
			}
			if (expandableObjects != null)
			{
				expandableObjects.RemoveObject(sip.ExpandableObjectStoreKey);
			}
			_store.SetValue(sip, styleInfoSubObject.Data);
		}
		else
		{
			_store.SetValue(sip, value);
		}
		if (!inStyleChanged)
		{
			inStyleChanged = true;
			OnStyleChanged(sip);
			inStyleChanged = false;
		}
	}

	public void SetValue(StyleInfoProperty sip, short value)
	{
		if (!inStyleChanged)
		{
			OnStyleChanging(sip);
		}
		_store.SetValue(sip, value);
		if (!inStyleChanged)
		{
			inStyleChanged = true;
			OnStyleChanged(sip);
			inStyleChanged = false;
		}
	}
}

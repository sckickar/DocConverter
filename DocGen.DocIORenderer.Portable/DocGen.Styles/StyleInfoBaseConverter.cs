using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace DocGen.Styles;

internal class StyleInfoBaseConverter : TypeConverter
{
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return false;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return string.Empty;
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		if (value is StyleInfoBase styleInfoBase)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, attributes);
			ICollection styleInfoProperties = styleInfoBase.Store.StyleInfoProperties;
			Type type = styleInfoBase.GetType();
			PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
			foreach (PropertyDescriptor item in properties)
			{
				StyleInfoProperty styleInfoProperty = styleInfoBase.Store.FindStyleInfoProperty(item.Name);
				if (styleInfoProperty == null || styleInfoProperty.IsBrowsable)
				{
					propertyDescriptorCollection.Add(item);
				}
			}
			foreach (StyleInfoProperty item2 in styleInfoProperties)
			{
				if (item2.ComponentType != null && !item2.ComponentType.IsAssignableFrom(type))
				{
					PropertyInfo propertyInfo = item2.GetPropertyInfo();
					if (item2.IsBrowsable && propertyInfo != null)
					{
						Attribute[] atts = (Attribute[])propertyInfo.GetCustomAttributes(typeof(Attribute), inherit: false);
						propertyDescriptorCollection.Add(new StyleInfoPropertyPropertyDescriptor(item2, item2.ComponentType, atts));
					}
				}
			}
			string[] propertyGridSortOrder = styleInfoBase.Store.PropertyGridSortOrder;
			return propertyDescriptorCollection.Sort(propertyGridSortOrder);
		}
		return base.GetProperties(context, value, attributes);
	}
}

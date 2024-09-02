using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class BlipFormat
{
	private float m_threshold;

	private Color m_inverseColor;

	private float m_inverseOpacity;

	private float m_alphaReplaceAmount;

	private float m_bilevelThreshold;

	private float m_blurRadius;

	private Color m_colorFrom;

	private Color m_colorTo;

	private float m_colorFromOpacity;

	private float m_colorToOpacity;

	private float m_hue;

	private float m_luminance;

	private float m_saturation;

	private float m_brightness;

	private float m_contrast;

	private float m_tintAmount;

	private float m_tintHue;

	private Color m_duotoneColor;

	private Color m_duotonePresetColor;

	private float m_duotoneOpacity;

	private Dictionary<string, Stream> m_docxProps;

	private ImageEffect m_imageEffect;

	private List<string> m_extensionUri;

	private ImageRecord m_ImageRecord;

	private byte m_flagA;

	private byte m_bFlags;

	private float m_transparency;

	private Shape m_shape;

	private BlipTransparency m_blipTransparency;

	internal const byte ColorFromOpacityKey = 5;

	internal const byte ColorToOpacityKey = 6;

	internal const byte HasAlphaKey = 1;

	internal float Threshold
	{
		get
		{
			return m_threshold;
		}
		set
		{
			m_threshold = value;
		}
	}

	internal Color InverseColor
	{
		get
		{
			return m_inverseColor;
		}
		set
		{
			m_inverseColor = value;
		}
	}

	internal float InverseOpacity
	{
		get
		{
			return m_inverseOpacity;
		}
		set
		{
			m_inverseOpacity = value;
		}
	}

	internal float AlphaReplaceAmount
	{
		get
		{
			return m_alphaReplaceAmount;
		}
		set
		{
			m_alphaReplaceAmount = value;
		}
	}

	internal float BilevelThreshold
	{
		get
		{
			return m_bilevelThreshold;
		}
		set
		{
			m_bilevelThreshold = value;
		}
	}

	internal bool Grow
	{
		get
		{
			return (m_flagA & 1) != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal float BlurRadius
	{
		get
		{
			return m_blurRadius;
		}
		set
		{
			m_blurRadius = value;
		}
	}

	internal bool HasAlpha
	{
		get
		{
			return (m_flagA & 2) >> 1 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsPresetColorAtFirst
	{
		get
		{
			return (m_flagA & 4) >> 2 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal Color ColorFrom
	{
		get
		{
			return m_colorFrom;
		}
		set
		{
			m_colorFrom = value;
		}
	}

	internal Color ColorTo
	{
		get
		{
			return m_colorTo;
		}
		set
		{
			m_colorTo = value;
		}
	}

	internal float ColorFromOpacity
	{
		get
		{
			return m_colorFromOpacity;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xDFu) | 0x20u);
			m_colorFromOpacity = value;
		}
	}

	internal float ColorToOpacity
	{
		get
		{
			return m_colorToOpacity;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xBFu) | 0x40u);
			m_colorToOpacity = value;
		}
	}

	internal float Hue
	{
		get
		{
			return m_hue;
		}
		set
		{
			m_hue = value;
		}
	}

	internal float Luminance
	{
		get
		{
			return m_luminance;
		}
		set
		{
			m_luminance = value;
		}
	}

	internal float Saturation
	{
		get
		{
			return m_saturation;
		}
		set
		{
			m_saturation = value;
		}
	}

	internal float Brightness
	{
		get
		{
			return m_brightness;
		}
		set
		{
			m_brightness = value;
		}
	}

	internal float Contrast
	{
		get
		{
			return m_contrast;
		}
		set
		{
			m_contrast = value;
		}
	}

	internal float TintAmount
	{
		get
		{
			return m_tintAmount;
		}
		set
		{
			m_tintAmount = value;
		}
	}

	internal float TintHue
	{
		get
		{
			return m_tintHue;
		}
		set
		{
			m_tintHue = value;
		}
	}

	internal Color DuotoneColor
	{
		get
		{
			return m_duotoneColor;
		}
		set
		{
			m_duotoneColor = value;
		}
	}

	internal Color DuotonePresetColor
	{
		get
		{
			return m_duotonePresetColor;
		}
		set
		{
			m_duotonePresetColor = value;
		}
	}

	internal float DuotoneOpacity
	{
		get
		{
			return m_duotoneOpacity;
		}
		set
		{
			m_duotoneOpacity = value;
		}
	}

	internal List<string> ExtensionURI
	{
		get
		{
			if (m_extensionUri == null)
			{
				m_extensionUri = new List<string>();
			}
			return m_extensionUri;
		}
		set
		{
			m_extensionUri = value;
		}
	}

	internal ImageEffect ImageEffect
	{
		get
		{
			if (m_imageEffect == null)
			{
				m_imageEffect = new ImageEffect();
			}
			return m_imageEffect;
		}
		set
		{
			m_imageEffect = value;
		}
	}

	internal bool HasCompression
	{
		get
		{
			return (m_flagA & 4) >> 2 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool HasImageProperties
	{
		get
		{
			return (m_flagA & 8) >> 3 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool RotateWithObject
	{
		get
		{
			return (m_flagA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal ImageRecord ImageRecord
	{
		get
		{
			return m_ImageRecord;
		}
		set
		{
			m_ImageRecord = value;
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	internal float Transparency
	{
		get
		{
			return m_transparency;
		}
		set
		{
			m_transparency = value;
		}
	}

	internal BlipTransparency BlipTransparency
	{
		get
		{
			return m_blipTransparency;
		}
		set
		{
			m_blipTransparency = value;
		}
	}

	internal bool GrayScale
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool BiLevel
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal BlipFormat(ShapeBase shape)
	{
	}

	internal BlipFormat(WPicture picture)
	{
	}

	internal void Close()
	{
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			m_docxProps.Clear();
		}
		if (m_imageEffect != null)
		{
			m_imageEffect.Close();
			m_imageEffect = null;
		}
		if (m_extensionUri != null && m_extensionUri.Count > 0)
		{
			m_extensionUri.Clear();
		}
		if (m_shape != null)
		{
			m_shape = null;
		}
	}

	internal BlipFormat Clone()
	{
		BlipFormat blipFormat = (BlipFormat)MemberwiseClone();
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			m_shape.Document.CloneProperties(DocxProps, ref blipFormat.m_docxProps);
		}
		if (ImageEffect != null)
		{
			blipFormat.ImageEffect = ImageEffect.Clone();
		}
		return blipFormat;
	}

	internal bool HasKey(int propertyKey)
	{
		return (m_flagA & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	internal bool Compare(BlipFormat blipFormat)
	{
		if (Grow != blipFormat.Grow || HasAlpha != blipFormat.HasAlpha || HasCompression != blipFormat.HasCompression || HasImageProperties != blipFormat.HasImageProperties || RotateWithObject != blipFormat.RotateWithObject || Threshold != blipFormat.Threshold || InverseOpacity != blipFormat.InverseOpacity || AlphaReplaceAmount != blipFormat.AlphaReplaceAmount || BilevelThreshold != blipFormat.BilevelThreshold || BlurRadius != blipFormat.BlurRadius || ColorFromOpacity != blipFormat.ColorFromOpacity || ColorToOpacity != blipFormat.ColorToOpacity || Hue != blipFormat.Hue || Luminance != blipFormat.Luminance || Saturation != blipFormat.Saturation || Brightness != blipFormat.Brightness || Contrast != blipFormat.Contrast || TintAmount != blipFormat.TintAmount || TintHue != blipFormat.TintHue || DuotoneOpacity != blipFormat.DuotoneOpacity || Transparency != blipFormat.Transparency || BlipTransparency != blipFormat.BlipTransparency || InverseColor.ToArgb() != blipFormat.InverseColor.ToArgb() || ColorFrom.ToArgb() != blipFormat.ColorFrom.ToArgb())
		{
			return false;
		}
		if ((ImageEffect == null && blipFormat.ImageEffect != null) || (ImageEffect != null && blipFormat.ImageEffect == null))
		{
			return false;
		}
		if (ImageEffect != null && blipFormat.ImageEffect != null && !ImageEffect.Compare(blipFormat.ImageEffect))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = (Grow ? "1" : "0");
		stringBuilder.Append(value);
		value = (HasAlpha ? "1" : "0");
		stringBuilder.Append(value);
		value = (HasCompression ? "1" : "0");
		stringBuilder.Append(value);
		value = (HasImageProperties ? "1" : "0");
		stringBuilder.Append(value);
		value = (RotateWithObject ? "1" : "0");
		stringBuilder.Append(value);
		stringBuilder.Append(Threshold.ToString());
		stringBuilder.Append(InverseOpacity);
		stringBuilder.Append(AlphaReplaceAmount);
		stringBuilder.Append(BilevelThreshold);
		stringBuilder.Append(BlurRadius);
		stringBuilder.Append(ColorFromOpacity.ToString());
		stringBuilder.Append(ColorToOpacity.ToString());
		stringBuilder.Append(Hue);
		stringBuilder.Append(Luminance);
		stringBuilder.Append(Saturation.ToString());
		stringBuilder.Append(Brightness.ToString());
		stringBuilder.Append(Contrast.ToString());
		stringBuilder.Append(TintAmount.ToString());
		stringBuilder.Append(TintHue.ToString());
		stringBuilder.Append(DuotoneOpacity);
		stringBuilder.Append(Transparency);
		stringBuilder.Append((int)BlipTransparency);
		stringBuilder.Append(InverseColor.ToArgb() + ";");
		stringBuilder.Append(ColorFrom.ToArgb() + ";");
		foreach (string item in ExtensionURI)
		{
			stringBuilder.Append(item + ";");
			if (ImageEffect != null)
			{
				stringBuilder.Append(ImageEffect.GetAsString());
			}
		}
		return stringBuilder;
	}
}

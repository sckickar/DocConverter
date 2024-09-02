using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ImageEffect
{
	private List<PointF> m_foregroundVertices;

	private List<PointF> m_backgroundVertices;

	private Dictionary<int, object> m_propertiesHash;

	private bool m_hasBackgroundRemoval;

	internal const byte SharpenAmountKey = 0;

	internal const byte ColorTempratureKey = 1;

	internal const byte SaturationKey = 2;

	internal const byte BrightnessKey = 3;

	internal const byte ContrastKey = 4;

	internal const byte PencilSizeKey = 5;

	internal const byte TransparencyKey = 6;

	internal const byte BlurRadiusKey = 7;

	internal const byte CementTransparencyKey = 8;

	internal const byte CementCrackingKey = 9;

	internal const byte ChalkSketchTransparencyKey = 10;

	internal const byte ChalkSketchPressureKey = 11;

	internal const byte CrisscrossEtchingTransparencyKey = 12;

	internal const byte CrisscrossEtchingPressureKey = 13;

	internal const byte CutoutTransparencyKey = 14;

	internal const byte CutoutShadesKey = 15;

	internal const byte GrainTransparencyKey = 16;

	internal const byte GrainSizeKey = 17;

	internal const byte GlassTransparencyKey = 18;

	internal const byte GlassScalingKey = 19;

	internal const byte GlowDiffusedTransparencyKey = 20;

	internal const byte GlowDiffusedIntensityKey = 21;

	internal const byte GlowEdgesTransparencyKey = 22;

	internal const byte GlowEdgesSmoothnessKey = 23;

	internal const byte LightScreenTransparencyKey = 24;

	internal const byte LightScreenGridKey = 25;

	internal const byte LineDrawingTransparencyKey = 26;

	internal const byte LineDrawingPensilSizeKey = 27;

	internal const byte MarkerTransparencyKey = 28;

	internal const byte MarkerSizeKey = 29;

	internal const byte MosiaicBubbleTransparencyKey = 30;

	internal const byte MosiaicBubblePressureKey = 31;

	internal const byte StrokeTransparencyKey = 32;

	internal const byte StrokeIntensityKey = 33;

	internal const byte BrushTransparencyKey = 34;

	internal const byte BrushSizeKey = 35;

	internal const byte PastelTransparencyKey = 36;

	internal const byte PastelSizeKey = 37;

	internal const byte PencilGrayScaleTransparencyKey = 38;

	internal const byte PencilGrayScaleSizeKey = 39;

	internal const byte PencilSketchTransparencyKey = 40;

	internal const byte PencilSketchSizeKey = 41;

	internal const byte PhotocopyTransparencyKey = 42;

	internal const byte PhotocopySizeKey = 43;

	internal const byte PlasticWrapTransparencyKey = 44;

	internal const byte PlasticWrapSmoothnessKey = 45;

	internal const byte TexturizerTransparencyKey = 46;

	internal const byte TexturizerSizeKey = 47;

	internal const byte SpongeTransparencyKey = 48;

	internal const byte SpongeBrushSizeKey = 49;

	internal const byte BackgroundRemovalRectangleKey = 50;

	internal float SharpenAmount
	{
		get
		{
			return GetValue(0);
		}
		set
		{
			if (value < -100000f || value > 100000f)
			{
				throw new ArgumentException("Sharpen Amount should be between -100% to 100%");
			}
			SetKeyValue(0, value);
		}
	}

	internal float ColorTemprature
	{
		get
		{
			return GetValue(1);
		}
		set
		{
			if (value < 1500f || value > 11500f)
			{
				throw new ArgumentException("ColorTemprature should be between 1500 to 11500");
			}
			SetKeyValue(1, value);
		}
	}

	internal float Saturation
	{
		get
		{
			return GetValue(2);
		}
		set
		{
			if (value < 0f || value > 400000f)
			{
				throw new ArgumentException("Saturation should be between 0% to 400%");
			}
			SetKeyValue(2, value);
		}
	}

	internal float Brightness
	{
		get
		{
			return GetValue(3);
		}
		set
		{
			if (value < -100000f || value > 100000f)
			{
				throw new ArgumentException("Brightness should be between -100% to 100%");
			}
			SetKeyValue(3, value);
		}
	}

	internal float Contrast
	{
		get
		{
			return GetValue(4);
		}
		set
		{
			if (value < -100000f || value > 100000f)
			{
				throw new ArgumentException("Contrast should be between -100% to 100%");
			}
			SetKeyValue(4, value);
		}
	}

	internal float BlurRadius
	{
		get
		{
			return GetValue(7);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Blur radius should be between 0 to 100");
			}
			SetKeyValue(7, value);
		}
	}

	internal float CementTransparency
	{
		get
		{
			return GetValue(8);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Cement transparency should be between 0% to 100%");
			}
			SetKeyValue(8, value);
		}
	}

	internal float CementCracking
	{
		get
		{
			return GetValue(9);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Cement space cracking should be between 0 to 100");
			}
			SetKeyValue(9, value);
		}
	}

	internal float ChalkSketchTransparency
	{
		get
		{
			return GetValue(10);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Chalk Sketch transparency should be between 0% to 100%");
			}
			SetKeyValue(10, value);
		}
	}

	internal float ChalkSketchPressure
	{
		get
		{
			return GetValue(11);
		}
		set
		{
			if (value < 0f || value > 4f)
			{
				throw new ArgumentException("Chalk Sketch Pressure should be between 0 to 4");
			}
			SetKeyValue(11, value);
		}
	}

	internal float CrisscrossEtchingTransparency
	{
		get
		{
			return GetValue(12);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Crisscross Etching Transparency should be between 0% to 100%");
			}
			SetKeyValue(12, value);
		}
	}

	internal float CrisscrossEtchingPressure
	{
		get
		{
			return GetValue(13);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Crisscross Etching Pressure should be between 0 to 100");
			}
			SetKeyValue(13, value);
		}
	}

	internal float CutoutTransparency
	{
		get
		{
			return GetValue(14);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Cutout Transparency should be between 0% to 100%");
			}
			SetKeyValue(14, value);
		}
	}

	internal float CutoutShades
	{
		get
		{
			return GetValue(15);
		}
		set
		{
			if (value < 0f || value > 6f)
			{
				throw new ArgumentException("Cutout shades should be between 0 to 6");
			}
			SetKeyValue(15, value);
		}
	}

	internal float GrainTransparency
	{
		get
		{
			return GetValue(16);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Grain Transparency should be between 0% to 100%");
			}
			SetKeyValue(16, value);
		}
	}

	internal float GrainSize
	{
		get
		{
			return GetValue(17);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Grain Size should be between 0 to 100");
			}
			SetKeyValue(17, value);
		}
	}

	internal float GlassTransparency
	{
		get
		{
			return GetValue(18);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Glass Transparency should be between 0% to 100%");
			}
			SetKeyValue(18, value);
		}
	}

	internal float GlassScaling
	{
		get
		{
			return GetValue(19);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Glass Scaling should be between 0 to 100");
			}
			SetKeyValue(19, value);
		}
	}

	internal float GlowDiffusedTransparency
	{
		get
		{
			return GetValue(20);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Glow Transparency should be between 0% to 100%");
			}
			SetKeyValue(20, value);
		}
	}

	internal float GlowDiffusedIntensity
	{
		get
		{
			return GetValue(21);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Glow Scaling should be between 0 to 10");
			}
			SetKeyValue(21, value);
		}
	}

	internal float GlowEdgesTransparency
	{
		get
		{
			return GetValue(22);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Glow edges Transparency should be between 0% to 100%");
			}
			SetKeyValue(20, value);
		}
	}

	internal float GlowEdgesSmoothness
	{
		get
		{
			return GetValue(23);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Glow edges smoothing factor should be between 0 to 10");
			}
			SetKeyValue(23, value);
		}
	}

	internal float LightScreenTransparency
	{
		get
		{
			return GetValue(24);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Light screen Transparency should be between 0% to 100%");
			}
			SetKeyValue(24, value);
		}
	}

	internal float LightScreenGrid
	{
		get
		{
			return GetValue(25);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Lightscreen grid factor should be between 0 to 10");
			}
			SetKeyValue(25, value);
		}
	}

	internal float LineDrawingTransparency
	{
		get
		{
			return GetValue(26);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("LineDrawing Transparency should be between 0% to 100%");
			}
			SetKeyValue(26, value);
		}
	}

	internal float LineDrawingPensilSize
	{
		get
		{
			return GetValue(27);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("LineDrawing pensil size should be between 0 to 100");
			}
			SetKeyValue(27, value);
		}
	}

	internal float MarkerTransparency
	{
		get
		{
			return GetValue(28);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Marker Transparency should be between 0% to 100%");
			}
			SetKeyValue(28, value);
		}
	}

	internal float MarkerSize
	{
		get
		{
			return GetValue(29);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Marker size should be between 0 to 100");
			}
			SetKeyValue(29, value);
		}
	}

	internal float MosiaicBubbleTransparency
	{
		get
		{
			return GetValue(30);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Mosiaic Transparency should be between 0% to 100%");
			}
			SetKeyValue(30, value);
		}
	}

	internal float MosiaicBubblePressure
	{
		get
		{
			return GetValue(31);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Mosiaic pressure should be between 0 to 100");
			}
			SetKeyValue(31, value);
		}
	}

	internal float StrokeTransparency
	{
		get
		{
			return GetValue(32);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Paint stroke Transparency should be between 0% to 100%");
			}
			SetKeyValue(32, value);
		}
	}

	internal float StrokeIntensity
	{
		get
		{
			return GetValue(33);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Paint Stroke intensity should be between 0 to 10");
			}
			SetKeyValue(33, value);
		}
	}

	internal float BrushTransparency
	{
		get
		{
			return GetValue(34);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Brush Transparency should be between 0% to 100%");
			}
			SetKeyValue(34, value);
		}
	}

	internal float BrushSize
	{
		get
		{
			return GetValue(35);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Brush size should be between 0 to 10");
			}
			SetKeyValue(35, value);
		}
	}

	internal float PastelTransparency
	{
		get
		{
			return GetValue(36);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Pastel Smooth Transparency should be between 0% to 100%");
			}
			SetKeyValue(36, value);
		}
	}

	internal float PastelSize
	{
		get
		{
			return GetValue(37);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Pastel size should be between 0 to 100");
			}
			SetKeyValue(37, value);
		}
	}

	internal float PencilGrayScaleTransparency
	{
		get
		{
			return GetValue(38);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Pastel Smooth Transparency should be between 0% to 100%");
			}
			SetKeyValue(38, value);
		}
	}

	internal float PencilGraySize
	{
		get
		{
			return GetValue(39);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Pastel size should be between 0 to 100");
			}
			SetKeyValue(39, value);
		}
	}

	internal float PencilSketchTransparency
	{
		get
		{
			return GetValue(40);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Pastel Smooth Transparency should be between 0% to 100%");
			}
			SetKeyValue(40, value);
		}
	}

	internal float PencilSketchSize
	{
		get
		{
			return GetValue(41);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Pastel size should be between 0 to 100");
			}
			SetKeyValue(41, value);
		}
	}

	internal float PhotocopyTransparency
	{
		get
		{
			return GetValue(42);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Pastel Smooth Transparency should be between 0% to 100%");
			}
			SetKeyValue(42, value);
		}
	}

	internal float PhotocopySize
	{
		get
		{
			return GetValue(43);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Pastel size should be between 0 to 10");
			}
			SetKeyValue(43, value);
		}
	}

	internal float PlasticWrapTransparency
	{
		get
		{
			return GetValue(44);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Plastic wrap Transparency should be between 0% to 100%");
			}
			SetKeyValue(44, value);
		}
	}

	internal float PlasticWrapSmoothness
	{
		get
		{
			return GetValue(45);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Plastic wrap smoothness should be between 0 to 10");
			}
			SetKeyValue(45, value);
		}
	}

	internal float TexturizerTransparency
	{
		get
		{
			return GetValue(46);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Texturizer Transparency should be between 0% to 100%");
			}
			SetKeyValue(46, value);
		}
	}

	internal float TexturizerSize
	{
		get
		{
			return GetValue(47);
		}
		set
		{
			if (value < 0f || value > 100f)
			{
				throw new ArgumentException("Texturizer Size should be between 0 to 100");
			}
			SetKeyValue(47, value);
		}
	}

	internal float SpongeTransparency
	{
		get
		{
			return GetValue(48);
		}
		set
		{
			if (value < 0f || value > 100000f)
			{
				throw new ArgumentException("Watercolor Sponge Transparency should be between 0% to 100%");
			}
			SetKeyValue(48, value);
		}
	}

	internal float SpongeBrushSize
	{
		get
		{
			return GetValue(49);
		}
		set
		{
			if (value < 0f || value > 10f)
			{
				throw new ArgumentException("Sponge Brush Size should be between 0 to 10");
			}
			SetKeyValue(49, value);
		}
	}

	internal List<PointF> ForegroundVertices
	{
		get
		{
			if (m_foregroundVertices == null)
			{
				m_foregroundVertices = new List<PointF>();
			}
			return m_foregroundVertices;
		}
		set
		{
			m_foregroundVertices = value;
		}
	}

	internal List<PointF> BackgroundVertices
	{
		get
		{
			if (m_backgroundVertices == null)
			{
				m_backgroundVertices = new List<PointF>();
			}
			return m_backgroundVertices;
		}
		set
		{
			m_backgroundVertices = value;
		}
	}

	internal TileRectangle BackgroundRemovalRectangle
	{
		get
		{
			if (!PropertiesHash.ContainsKey(50))
			{
				PropertiesHash.Add(50, new TileRectangle());
			}
			if (!(PropertiesHash[50] is TileRectangle))
			{
				PropertiesHash[50] = new TileRectangle();
			}
			return PropertiesHash[50] as TileRectangle;
		}
		set
		{
			SetKeyValue(50, value);
		}
	}

	internal bool HasBackgroundRemovalEffect
	{
		get
		{
			return m_hasBackgroundRemoval;
		}
		set
		{
			m_hasBackgroundRemoval = value;
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal ImageEffect()
	{
		ColorTemprature = 6500f;
		Saturation = 100000f;
	}

	private void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	private float GetValue(int key)
	{
		if (PropertiesHash.ContainsKey(key))
		{
			return (float)PropertiesHash[key];
		}
		return 0f;
	}

	internal void Close()
	{
		if (m_propertiesHash != null || m_propertiesHash.Count > 0)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_backgroundVertices != null && m_backgroundVertices.Count > 0)
		{
			m_backgroundVertices.Clear();
			m_backgroundVertices = null;
		}
		if (m_foregroundVertices != null && m_foregroundVertices.Count > 0)
		{
			m_foregroundVertices.Clear();
			m_foregroundVertices = null;
		}
	}

	internal ImageEffect Clone()
	{
		ImageEffect imageEffect = (ImageEffect)MemberwiseClone();
		if (PropertiesHash != null && PropertiesHash.Count > 0)
		{
			imageEffect.m_propertiesHash = new Dictionary<int, object>();
			foreach (KeyValuePair<int, object> item in PropertiesHash)
			{
				imageEffect.PropertiesHash.Add(item.Key, item.Value);
			}
		}
		if (BackgroundVertices != null && BackgroundVertices.Count > 0)
		{
			imageEffect.m_backgroundVertices = new List<PointF>();
			foreach (PointF backgroundVertex in BackgroundVertices)
			{
				imageEffect.BackgroundVertices.Add(backgroundVertex);
			}
		}
		if (ForegroundVertices != null && ForegroundVertices.Count > 0)
		{
			imageEffect.m_foregroundVertices = new List<PointF>();
			foreach (PointF foregroundVertex in ForegroundVertices)
			{
				imageEffect.ForegroundVertices.Add(foregroundVertex);
			}
		}
		return imageEffect;
	}

	internal bool Compare(ImageEffect imageEffect)
	{
		if (HasBackgroundRemovalEffect != imageEffect.HasBackgroundRemovalEffect || SharpenAmount != imageEffect.SharpenAmount || ColorTemprature != imageEffect.ColorTemprature || Saturation != imageEffect.Saturation || Brightness != imageEffect.Brightness || Contrast != imageEffect.Contrast || BlurRadius != imageEffect.BlurRadius || CementTransparency != imageEffect.CementTransparency || CementCracking != imageEffect.CementCracking || ChalkSketchTransparency != imageEffect.ChalkSketchTransparency || ChalkSketchPressure != imageEffect.ChalkSketchPressure || CrisscrossEtchingTransparency != imageEffect.CrisscrossEtchingTransparency || CrisscrossEtchingPressure != imageEffect.CrisscrossEtchingPressure || CutoutTransparency != imageEffect.CutoutTransparency || CutoutShades != imageEffect.CutoutShades || GrainTransparency != imageEffect.GrainTransparency || GrainSize != imageEffect.GrainSize || GlassTransparency != imageEffect.GlassTransparency || GlassScaling != imageEffect.GlassScaling || GlowDiffusedTransparency != imageEffect.GlowDiffusedTransparency || GlowDiffusedIntensity != imageEffect.GlowDiffusedIntensity || GlowEdgesTransparency != imageEffect.GlowEdgesTransparency || GlowEdgesSmoothness != imageEffect.GlowEdgesSmoothness || LightScreenTransparency != imageEffect.LightScreenTransparency || LightScreenGrid != imageEffect.LightScreenGrid || LineDrawingTransparency != imageEffect.LineDrawingTransparency || LineDrawingPensilSize != imageEffect.LineDrawingPensilSize || MarkerTransparency != imageEffect.MarkerTransparency || MarkerSize != imageEffect.MarkerSize || MosiaicBubbleTransparency != imageEffect.MosiaicBubbleTransparency || MosiaicBubblePressure != imageEffect.MosiaicBubblePressure || StrokeTransparency != imageEffect.StrokeTransparency || StrokeIntensity != imageEffect.StrokeIntensity || BrushTransparency != imageEffect.BrushTransparency || BrushSize != imageEffect.BrushSize || PastelTransparency != imageEffect.PastelTransparency || PastelSize != imageEffect.PastelSize || PencilGrayScaleTransparency != imageEffect.PencilGrayScaleTransparency || PencilGraySize != imageEffect.PencilGraySize || PencilSketchTransparency != imageEffect.PencilSketchTransparency || PencilSketchSize != imageEffect.PencilSketchSize || PhotocopyTransparency != imageEffect.PhotocopyTransparency || PhotocopySize != imageEffect.PhotocopySize || PlasticWrapTransparency != imageEffect.PlasticWrapTransparency || PlasticWrapSmoothness != imageEffect.PlasticWrapSmoothness || TexturizerTransparency != imageEffect.TexturizerTransparency || TexturizerSize != imageEffect.TexturizerSize || SpongeTransparency != imageEffect.SpongeTransparency || SpongeBrushSize != imageEffect.SpongeBrushSize)
		{
			return false;
		}
		if ((ForegroundVertices != null && imageEffect.ForegroundVertices == null) || (ForegroundVertices == null && imageEffect.ForegroundVertices != null) || (BackgroundVertices != null && imageEffect.BackgroundVertices == null) || (BackgroundVertices == null && imageEffect.BackgroundVertices != null) || (BackgroundRemovalRectangle != null && imageEffect.BackgroundRemovalRectangle == null) || (BackgroundRemovalRectangle == null && imageEffect.BackgroundRemovalRectangle != null))
		{
			return false;
		}
		if (ForegroundVertices != null && imageEffect.ForegroundVertices != null)
		{
			if (ForegroundVertices.Count != imageEffect.ForegroundVertices.Count)
			{
				return false;
			}
			for (int i = 0; i < ForegroundVertices.Count; i++)
			{
				if (ForegroundVertices[i].X != imageEffect.ForegroundVertices[i].X || ForegroundVertices[i].Y != imageEffect.ForegroundVertices[i].Y)
				{
					return false;
				}
			}
		}
		if (BackgroundVertices != null && imageEffect.BackgroundVertices != null)
		{
			if (BackgroundVertices.Count != imageEffect.BackgroundVertices.Count)
			{
				return false;
			}
			for (int j = 0; j < BackgroundVertices.Count; j++)
			{
				if (BackgroundVertices[j].X != imageEffect.BackgroundVertices[j].X || BackgroundVertices[j].Y != imageEffect.BackgroundVertices[j].Y)
				{
					return false;
				}
			}
		}
		if (BackgroundRemovalRectangle != null && imageEffect.BackgroundRemovalRectangle != null && !BackgroundRemovalRectangle.Compare(imageEffect.BackgroundRemovalRectangle))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (HasBackgroundRemovalEffect ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(SharpenAmount + ";");
		stringBuilder.Append(ColorTemprature + ";");
		stringBuilder.Append(Saturation + ";");
		stringBuilder.Append(Brightness + ";");
		stringBuilder.Append(Contrast + ";");
		stringBuilder.Append(BlurRadius + ";");
		stringBuilder.Append(CementTransparency + ";");
		stringBuilder.Append(CementCracking + ";");
		stringBuilder.Append(ChalkSketchTransparency + ";");
		stringBuilder.Append(ChalkSketchPressure + ";");
		stringBuilder.Append(CrisscrossEtchingTransparency + ";");
		stringBuilder.Append(CrisscrossEtchingPressure + ";");
		stringBuilder.Append(CutoutTransparency + ";");
		stringBuilder.Append(CutoutShades + ";");
		stringBuilder.Append(GrainTransparency + ";");
		stringBuilder.Append(GrainSize + ";");
		stringBuilder.Append(GlassTransparency + ";");
		stringBuilder.Append(GlassScaling + ";");
		stringBuilder.Append(GlowDiffusedTransparency + ";");
		stringBuilder.Append(GlowDiffusedIntensity + ";");
		stringBuilder.Append(GlowEdgesTransparency + ";");
		stringBuilder.Append(GlowEdgesSmoothness + ";");
		stringBuilder.Append(LightScreenTransparency + ";");
		stringBuilder.Append(LightScreenGrid + ";");
		stringBuilder.Append(LineDrawingTransparency + ";");
		stringBuilder.Append(LineDrawingPensilSize + ";");
		stringBuilder.Append(MarkerTransparency + ";");
		stringBuilder.Append(MarkerSize + ";");
		stringBuilder.Append(MosiaicBubbleTransparency + ";");
		stringBuilder.Append(MosiaicBubblePressure + ";");
		stringBuilder.Append(StrokeTransparency + ";");
		stringBuilder.Append(StrokeIntensity + ";");
		stringBuilder.Append(BrushTransparency + ";");
		stringBuilder.Append(BrushSize + ";");
		stringBuilder.Append(PastelTransparency + ";");
		stringBuilder.Append(PastelSize + ";");
		stringBuilder.Append(PencilGrayScaleTransparency + ";");
		stringBuilder.Append(PencilGraySize + ";");
		stringBuilder.Append(PencilSketchTransparency + ";");
		stringBuilder.Append(PencilSketchSize + ";");
		stringBuilder.Append(PhotocopyTransparency + ";");
		stringBuilder.Append(PhotocopySize + ";");
		stringBuilder.Append(PlasticWrapTransparency + ";");
		stringBuilder.Append(PlasticWrapSmoothness + ";");
		stringBuilder.Append(TexturizerTransparency + ";");
		stringBuilder.Append(TexturizerSize + ";");
		stringBuilder.Append(SpongeTransparency + ";");
		stringBuilder.Append(SpongeBrushSize + ";");
		foreach (PointF foregroundVertex in ForegroundVertices)
		{
			stringBuilder.Append(foregroundVertex.X + ";" + foregroundVertex.Y + ";");
		}
		foreach (PointF backgroundVertex in BackgroundVertices)
		{
			stringBuilder.Append(backgroundVertex.X + ";" + backgroundVertex.Y + ";");
		}
		if (BackgroundRemovalRectangle != null)
		{
			stringBuilder.Append(BackgroundRemovalRectangle.GetAsString());
		}
		return stringBuilder;
	}
}

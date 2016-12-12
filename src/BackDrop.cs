using System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;

namespace Slideshow
{
    public class BackDrop : Control
    {
        Compositor m_compositor;
        SpriteVisual m_blurVisual;
        CompositionBrush m_blurBrush;
        Visual m_rootVisual;

#if SDKVERSION_14393
        bool m_setUpExpressions;
        CompositionSurfaceBrush m_noiseBrush;
#endif

        public BackDrop()
        {
            this.m_rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            this.Compositor = this.m_rootVisual.Compositor;

            this.m_blurVisual = this.Compositor.CreateSpriteVisual();

#if SDKVERSION_14393
            m_noiseBrush = Compositor.CreateSurfaceBrush();

            CompositionEffectBrush brush = BuildBlurBrush();
            brush.SetSourceParameter("source", m_compositor.CreateBackdropBrush());
            m_blurBrush = brush;
            m_blurVisual.Brush = m_blurBrush;

            BlurAmount = 9;
            TintColor = Colors.Transparent;
#else
            this.m_blurBrush = this.Compositor.CreateColorBrush(Colors.White);
            this.m_blurVisual.Brush = this.m_blurBrush;
#endif
            ElementCompositionPreview.SetElementChildVisual(this as UIElement, this.m_blurVisual);

            this.Loading += this.OnLoading;
            this.Unloaded += this.OnUnloaded;
        }

        public const string BlurAmountProperty = nameof(BlurAmount);
        public const string TintColorProperty = nameof(TintColor);

        public double BlurAmount
        {
            get
            {
                float value = 0;
#if SDKVERSION_14393
                m_rootVisual.Properties.TryGetScalar(BlurAmountProperty, out value);
#endif
                return value;
            }
            set
            {
#if SDKVERSION_14393
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
                m_rootVisual.Properties.InsertScalar(BlurAmountProperty, (float)value);
#endif
            }
        }

        public Color TintColor
        {
            get
            {
                Color value;
#if SDKVERSION_14393
                m_rootVisual.Properties.TryGetColor("TintColor", out value);
#else
                value = ((CompositionColorBrush)this.m_blurBrush).Color;
#endif
                return value;
            }
            set
            {
#if SDKVERSION_14393
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertColor("Color.Color", value);
                }
                m_rootVisual.Properties.InsertColor(TintColorProperty, value);
#else
                ((CompositionColorBrush)this.m_blurBrush).Color = value;
#endif
            }
        }

        public Compositor Compositor
        {
            get
            {
                return this.m_compositor;
            }

            private set
            {
                this.m_compositor = value;
            }
        }

#pragma warning disable 1998
        private async void OnLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += this.OnSizeChanged;
            this.OnSizeChanged(this, null);

#if SDKVERSION_14393
            m_noiseBrush.Surface = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///sample.jpg"));
            m_noiseBrush.Stretch = CompositionStretch.UniformToFill;
#endif
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= this.OnSizeChanged;
        }


        private void OnSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (this.m_blurVisual != null)
            {
                this.m_blurVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
            }
        }

#if SDKVERSION_14393
        private void SetUpPropertySetExpressions()
        {
            m_setUpExpressions = true;

            var exprAnimation = Compositor.CreateExpressionAnimation();
            exprAnimation.Expression = $"sourceProperties.{BlurAmountProperty}";
            exprAnimation.SetReferenceParameter("sourceProperties", m_rootVisual.Properties);

            m_blurBrush.Properties.StartAnimation("Blur.BlurAmount", exprAnimation);

            exprAnimation.Expression = $"sourceProperties.{TintColorProperty}";

            m_blurBrush.Properties.StartAnimation("Color.Color", exprAnimation);
        }


        private CompositionEffectBrush BuildBlurBrush()
        {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source"),
            };

            BlendEffect blendEffect = new BlendEffect
            {
                Background = blurEffect,
                Foreground = new ColorSourceEffect { Name = "Color", Color = Color.FromArgb(64, 255, 255, 255) },
                Mode = BlendEffectMode.SoftLight
            };

            SaturationEffect saturationEffect = new SaturationEffect
            {
                Source = blendEffect,
                Saturation = 1.75f,
            };

            BlendEffect finalEffect = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("NoiseImage"),
                Background = saturationEffect,
                Mode = BlendEffectMode.Screen,
            };

            var factory = Compositor.CreateEffectFactory(
                finalEffect,
                new[] { "Blur.BlurAmount", "Color.Color" }
                );

            CompositionEffectBrush brush = factory.CreateBrush();
            brush.SetSourceParameter("NoiseImage", m_noiseBrush);
            return brush;
        }

        public CompositionPropertySet VisualProperties
        {
            get
            {
                if (!m_setUpExpressions)
                {
                    SetUpPropertySetExpressions();
                }
                return m_rootVisual.Properties;
            }
        }

#endif

    }
}
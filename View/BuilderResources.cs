using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;

namespace LevelConstructor
{
    public static class BuilderResources
    {
        private const string ResourcesPath = "pack://application:,,,/Resources/";

        public static ImageSource Background { get; private set; }

        public static Brush DefaultShadow { get; private set; }

        private static readonly Dictionary<ObjectType, Brush> ObjectsBrushes;

        private static readonly Dictionary<ObjectType, ImageSource> ImageSources = new Dictionary<ObjectType, ImageSource>()
        {
            { ObjectType.StacionarReflectorAll, new BitmapImage(new Uri(ResourcesPath + "all.png"))},
            { ObjectType.StacionarReflectorAngle, new BitmapImage(new Uri(ResourcesPath + "angle.png"))},
            { ObjectType.StacionarReflectorTriangle, new BitmapImage(new Uri(ResourcesPath + "triangle.png"))},
            { ObjectType.StacionarTube, new BitmapImage(new Uri(ResourcesPath + "tube.png"))},
            { ObjectType.StacionarTeleport, new BitmapImage(new Uri(ResourcesPath + "teleport_stacionar.png"))},
            { ObjectType.StacionarMirror, new BitmapImage(new Uri(ResourcesPath + "mirror.png"))},

            { ObjectType.MovedReflectorAngle, new BitmapImage(new Uri(ResourcesPath + "moveable_angle.png"))},
            { ObjectType.MovedReflectorTriangle, new BitmapImage(new Uri(ResourcesPath + "moveable_triangle.png"))},
            { ObjectType.MovedReflectorAll, new BitmapImage(new Uri(ResourcesPath + "moveable_all.png"))},
            { ObjectType.MovedReflectorTube, new BitmapImage(new Uri(ResourcesPath + "moveable_tube.png"))},
            { ObjectType.MovedTeleport, new BitmapImage(new Uri(ResourcesPath + "teleport_moveable.png"))},
            { ObjectType.MovedNone, new BitmapImage(new Uri(ResourcesPath + "none.png"))},
            
           // { ObjectType.BarierBomb, new BitmapImage(new Uri(ResourcesPath + "stacionar.png"))},
            { ObjectType.Barier, new BitmapImage(new Uri(ResourcesPath + "stacionar.png"))},
            { ObjectType.LaserSource, new BitmapImage(new Uri(ResourcesPath + "blaster.png"))},
            { ObjectType.MovedLaserSource, new BitmapImage(new Uri(ResourcesPath + "movedblaster.png"))},
            { ObjectType.LaserXSource, new BitmapImage(new Uri(ResourcesPath + "blasterx.png"))},
            { ObjectType.MovedLaserXSource, new BitmapImage(new Uri(ResourcesPath + "movedblasterx.png"))},

			{ ObjectType.LaserReceiver_0, new BitmapImage(new Uri(ResourcesPath + "receiver_none.png"))},
            { ObjectType.LaserReceiver_1, new BitmapImage(new Uri(ResourcesPath + "receiver_mono.png"))},
            { ObjectType.LaserReceiver_2, new BitmapImage(new Uri(ResourcesPath + "receiver_di.png"))},
            { ObjectType.LaserReceiver_3, new BitmapImage(new Uri(ResourcesPath + "receiver_tri.png"))},
            { ObjectType.LaserReceiver_4, new BitmapImage(new Uri(ResourcesPath + "receiver_tetra.png"))},
			
            { ObjectType.StacionarControllerRed, new BitmapImage(new Uri(ResourcesPath + "controller_red.png"))},
            { ObjectType.StacionarControllerBlue, new BitmapImage(new Uri(ResourcesPath + "controller_blue.png"))},
            { ObjectType.StacionarControllerGreen, new BitmapImage(new Uri(ResourcesPath + "controller_green.png"))}

        };

        public static Brush BrushFrom2Sources(ObjectType first, ObjectType second)
        {
            var dbrush = new DrawingBrush();
            var group = new DrawingGroup();
            group.Append();
            group.Children.Add(new ImageDrawing(GetObjectImage(first) as BitmapImage, new Rect(0, 0, 1, 1)));
            group.Children.Add(new ImageDrawing(GetObjectImage(second) as BitmapImage, new Rect(0, 0, 0.35f, 0.35f)));
            group.Freeze();
            dbrush.Drawing = group;
            return dbrush;
        }

        private static Brush BrushFrom2Sources(string first, string second)
        {
            var dbrush = new DrawingBrush();
            var group = new DrawingGroup();
            group.Append();
            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri(ResourcesPath + first)), new Rect(0, 0, 1, 1)));
            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri(ResourcesPath + second)), new Rect(0, 0, 0.4f, 0.4f)));
            group.Freeze();
            dbrush.Drawing = group;
            return dbrush; 
        }

        private static ImageSource SourceFrom2Images(string first, string second)
        {
            var group = new DrawingGroup();
            group.Append();
            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri(ResourcesPath + first)), new Rect(0, 0, 1, 1)));
            group.Children.Add(new ImageDrawing(new BitmapImage(new Uri(ResourcesPath + second)), new Rect(0, 0, 1, 1)));
            group.Freeze();
            return new DrawingImage(group);
        }

        static BuilderResources()
        {
            ObjectsBrushes = ImageSources.ToDictionary(x => x.Key, v => new ImageBrush(v.Value) as Brush);
            Background = new BitmapImage(new Uri(ResourcesPath + "background.png"));
            DefaultShadow = new ImageBrush(new BitmapImage(new Uri(ResourcesPath + "shadow_default.png")));
        }

        public static Brush GetObjectBrush(ObjectType type)
        {
            return ObjectsBrushes[type];
        }

        public static ImageSource GetObjectImage(ObjectType type)
        {
            return ImageSources[type];
        }
    }
}

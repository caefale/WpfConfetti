using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Wpf.Confetti
{
    public static class ConfettiPalette
    {
        public static IEnumerable<Brush> Default { get; }
        public static IEnumerable<Brush> Fire { get; }
        public static IEnumerable<Brush> Snow { get; }
        public static IEnumerable<Brush> Party { get; }

        static ConfettiPalette()
        {
            Default = new List<Brush>
            {
                new SolidColorBrush(Color.FromRgb(255,107,107)), // Coral red
                new SolidColorBrush(Color.FromRgb(255,213,0)), // Sunny yellow
                new SolidColorBrush(Color.FromRgb(164,212,0)), // Lime green
                new SolidColorBrush(Color.FromRgb(62,223,211)), // Aqua
                new SolidColorBrush(Color.FromRgb(84,175,255)), // Sky blue
                new SolidColorBrush(Color.FromRgb(200,156,255))  // Lavender
            };

            Fire = new List<Brush>
            {
                new SolidColorBrush(Color.FromRgb(139, 0, 0)),     // Dark red
                new SolidColorBrush(Color.FromRgb(255, 69, 0)),    // Red-orange
                new SolidColorBrush(Color.FromRgb(255, 140, 0)),   // Dark orange
                new SolidColorBrush(Color.FromRgb(255, 165, 0)),   // Orange
                new SolidColorBrush(Color.FromRgb(255, 215, 0))    // Gold
            };

            Snow = new List<Brush>
            {
                new SolidColorBrush(Color.FromRgb(255, 255, 255)), // Pure white
                new SolidColorBrush(Color.FromRgb(240, 248, 255)), // Alice blue
                new SolidColorBrush(Color.FromRgb(192, 192, 192)), // Silver
                new SolidColorBrush(Color.FromRgb(176, 224, 230)), // Powder blue
                new SolidColorBrush(Color.FromRgb(173, 216, 230))  // Light blue
            };

            Party = new List<Brush>
            {
                new SolidColorBrush(Color.FromRgb(255, 0, 0)),     // Pure red
                new SolidColorBrush(Color.FromRgb(0, 255, 0)),     // Pure green
                new SolidColorBrush(Color.FromRgb(0, 0, 255)),     // Pure blue
                new SolidColorBrush(Color.FromRgb(255, 255, 0)),   // Pure yellow
                new SolidColorBrush(Color.FromRgb(255, 0, 255)),   // Pure magenta
                new SolidColorBrush(Color.FromRgb(0, 255, 255))    // Pure cyan
            };
        }
    }
}

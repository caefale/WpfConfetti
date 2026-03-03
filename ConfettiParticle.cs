using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WpfConfetti
{
    public class ConfettiParticle
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public Brush Brush { get; set; }
        public double Size { get; set; }
        public ConfettiShape Shape { get; set; } 
        public double Rotation { get; set; }
        public double RotationSpeed { get; set; }

    }
}

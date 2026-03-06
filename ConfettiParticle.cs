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
        public Point BasePosition {  get; set; }
        public Vector Velocity { get; set; }
        public Brush? Brush { get; set; } 
        public double Size { get; set; }
        public ConfettiShape Shape { get; set; }
        public double Drag { get; set; } 
        public bool IsWide { get; set; }
        public double WobbleAmplitude { get; set; }
        public double WobblePhase { get; set; }
        public double WobbleFrequency { get; set; }
        public double Age { get; set; }
        public double Rotation { get; set; }
        public double RotationSpeed { get; set; }
        public bool IsDead {  get; set; } = false;
        public double Gravity {  get; set; }
    }
}

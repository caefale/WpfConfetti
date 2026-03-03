using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace WpfConfetti
{
    public class ConfettiControl : FrameworkElement
    {
        private List<ConfettiParticle> particles;
        private List<Brush> colors = new List<Brush> { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow, Brushes.Orange}; 
        private Random random = new Random();

        private int amount = 50;
        private double totalTime = 0;

        public ConfettiControl()
        {
            particles = new List<ConfettiParticle>();

            CompositionTarget.Rendering += OnFrame;
            Loaded += InitializeParticles;

        }

        private async void InitializeParticles(object? sender, EventArgs e)
        {
            await Task.Delay(5000);
            for (int i = 0; i < amount; i++)
            {
                ConfettiParticle particle = new ConfettiParticle
                {
                    Position = new Point(ActualWidth / 2, ActualHeight / 2),
                    Velocity = new Vector
                    (
                        (random.NextDouble() - 0.5) * 10,
                        (random.NextDouble() - 0.5) * 10
                    ),
                    Size = random.Next(10, 15),
                    Brush = colors[random.Next(colors.Count)],
                    Shape = (ConfettiShape) random.Next(2), // CHANGE THIS ACCORDING TO SHAPES
                    Rotation = random.NextDouble() * 360,
                    RotationSpeed = (random.NextDouble() - 0.5) * 10
                };
                particles.Add(particle);
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            foreach (ConfettiParticle p in particles)
            {
                double cx = p.Position.X + p.Size / 2;
                double cy = p.Position.Y + p.Size / 2;

                dc.PushTransform(new RotateTransform(p.Rotation, cx, cy));

                switch (p.Shape)
                {
                    case ConfettiShape.Rectangle:
                        dc.DrawRectangle(p.Brush, null, new Rect(p.Position, new Size(p.Size, p.Size)));
                        break;
                    case ConfettiShape.Ellipse:
                        dc.DrawEllipse(p.Brush, null, new Point(cx, cy), p.Size / 2, p.Size / 2);
                        break;
                }

                dc.Pop();
            }
        }

        
        private void UpdateParticles()
        {

            foreach (ConfettiParticle p in particles)
            {
                p.Position += p.Velocity;
                p.Velocity += new Vector(0, 0.05);
                p.Rotation += p.RotationSpeed * 0.3;
                p.Velocity *= 0.99;

            }
        }

        private void OnFrame(object? sender, EventArgs e) { 
            UpdateParticles();
            InvalidateVisual();
        }
    }
}

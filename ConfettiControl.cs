using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace WpfConfetti
{
    public class ConfettiControl : FrameworkElement
    {
        private List<ConfettiParticle> _particles;
        private List<Brush> _colors = new List<Brush> {
            new SolidColorBrush(Color.FromRgb(255,107,107)), // Coral red
            new SolidColorBrush(Color.FromRgb(255,213,0)), // Sunny yellow
            new SolidColorBrush(Color.FromRgb(164,212,0)), // Lime green
            new SolidColorBrush(Color.FromRgb(62,223,211)), // Aqua
            new SolidColorBrush(Color.FromRgb(84,175,255)), // Sky blue
            new SolidColorBrush(Color.FromRgb(200,156,255))  // Lavender
        };

        private TimeSpan _lastRenderTime = TimeSpan.Zero;
        private static readonly Random _random = new();
        public ConfettiMode ConfettiMode { get; set; } = ConfettiMode.Burst;
        public bool IsRaining { get; private set; }

        public double CannonRate { get; set; } = 100;
        private int _cannonRemaining;
        private double _cannonAccumulator = 0;

        public ConfettiControl() : this(ConfettiMode.Burst)
        {
        }
        public ConfettiControl(ConfettiMode confettiMode)
        {
            _particles = new List<ConfettiParticle>();
            CompositionTarget.Rendering += OnFrame;
            IsHitTestVisible = false;
            this.ConfettiMode = confettiMode;
        }

        public void Emit(Point position, int amount)
        {
            switch (ConfettiMode)
            {
                case ConfettiMode.Burst:
                    for (int i = 0; i < amount; i++)
                        SpawnParticle(position, 0, 360, 50, 300, 85);
                    break;

                case ConfettiMode.Cannon:
                    _cannonRemaining += amount;
                    break;
            }
        }

        private void SpawnCannonParticle(Point position)
        {
            double targetX = ActualWidth / 2 + (_random.NextDouble() - 0.5) * 80; // small horizontal variation
            double targetY = ActualHeight * 0.35;

            Vector dir = new Vector(
                targetX - position.X,
                targetY - position.Y
            );

            dir.Normalize();

            double baseAngle = Math.Atan2(dir.Y, dir.X) * 180 / Math.PI;
            double spread = 25;
            double speedScale = ActualHeight / 400.0;
            SpawnParticle(position,
                baseAngle - spread,
                baseAngle + spread,
                300 * speedScale,
                500 * speedScale,
                120);
        }

        private void EmitRain(int amount)
        {
            if (ConfettiMode != ConfettiMode.Rain) return;
            for (int i = 0; i < amount; i++)
            {
                SpawnParticle(new Point(_random.NextDouble() * ActualWidth, -10), 85, 95, 60, 120, 120);
            }
        }



        public void StartRain()
        {
            ConfettiMode = ConfettiMode.Rain;
            IsRaining = true;
        }
        public void StopRain()
        {
            ConfettiMode = ConfettiMode.Rain;
            IsRaining = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            foreach (ConfettiParticle p  in _particles)
            {
                if (p.IsDead) continue;
                double w = p.IsWide ? p.Size * 2 : p.Size;
                double h = p.IsWide ? p.Size / 2 : p.Size;
                double cx = p.Position.X + w / 2;
                double cy = p.Position.Y + h / 2;
                dc.PushTransform(new RotateTransform(p.Rotation, cx, cy));
                switch (p.Shape)
                {
                    case ConfettiShape.Rectangle:
                        dc.DrawRectangle(p.Brush, null, new Rect(p.Position,
                            p.IsWide ? new Size(p.Size * 2, p.Size / 2) : new Size(p.Size / 2, p.Size * 2)));
                        break;
                    case ConfettiShape.Ellipse:
                        dc.DrawEllipse(p.Brush, null, new Point(cx, cy), p.Size / 2, p.Size / 2);
                        break;
                    case ConfettiShape.Triangle:
                        var geo = new StreamGeometry();
                        using (var ctx = geo.Open())
                        {
                            ctx.BeginFigure(new Point(cx, p.Position.Y), true, true);
                            ctx.LineTo(new Point(p.Position.X + p.Size, p.Position.Y + p.Size), true, false);
                            ctx.LineTo(new Point(p.Position.X, p.Position.Y + p.Size), true, false);
                        }
                        dc.DrawGeometry(p.Brush, null, geo);
                        break;
                }
                dc.Pop();

            }
        }

        private void OnFrame(object? sender, EventArgs e)
        {
            if (_particles.Count == 0 && !IsRaining && _cannonRemaining == 0) return;
            var args = (RenderingEventArgs)e;
            if (_lastRenderTime == TimeSpan.Zero) { _lastRenderTime = args.RenderingTime; return; }
            double deltaTime = (args.RenderingTime - _lastRenderTime).TotalSeconds;
            _lastRenderTime = args.RenderingTime;

            if (IsRaining)
            {
                EmitRain(3);
            }

            if (_cannonRemaining > 0)
            {
                _cannonAccumulator += deltaTime;
                double interval = 1.0 / CannonRate;
                while (_cannonAccumulator >= interval && _cannonRemaining > 0)
                {
                    SpawnCannonParticle(new Point(0, ActualHeight));
                    SpawnCannonParticle(new Point(ActualWidth, ActualHeight));
                    _cannonAccumulator -= interval; 
                    _cannonRemaining = Math.Max(0, _cannonRemaining - 2);
                }
            }

            UpdateParticles(deltaTime);
            InvalidateVisual();
        }

        private void UpdateParticles(double deltaTime)
        {
            bool _hasDead = false;
            foreach (ConfettiParticle p in _particles)
            {
                p.Age += deltaTime;
                p.BasePosition += p.Velocity * deltaTime;

                p.Velocity = new Vector(p.Velocity.X, p.Velocity.Y + p.Gravity * deltaTime);
                p.Velocity *= Math.Pow(p.Drag, deltaTime);
                p.RotationSpeed *= Math.Pow(p.Drag, deltaTime);

                double wobbleStrength = Math.Clamp(p.Age * 1.5, 0.0, 1.0);
                double wobbleOffset = Math.Sin(p.Age * p.WobbleFrequency + p.WobblePhase)
                                      * p.WobbleAmplitude * wobbleStrength;

                p.Position = new Point(p.BasePosition.X + wobbleOffset, p.BasePosition.Y);
                p.Rotation += p.RotationSpeed * deltaTime;

                if (p.Position.Y > ActualHeight + 50)
                {
                    _hasDead = true;
                    p.IsDead = true;
                }
            }
            if (_hasDead) _particles.RemoveAll(p => p.IsDead);
        }

        private void SpawnParticle(Point position, double minAngle, double maxAngle,
            double minSpeed, double maxSpeed, double gravity)
        {
            double angleDeg = minAngle + _random.NextDouble() * (maxAngle - minAngle);
            if (ConfettiMode == ConfettiMode.Burst) angleDeg -= 90;
            double angleRad = angleDeg * Math.PI / 180.0;
            double speed = minSpeed + _random.NextDouble() * (maxSpeed - minSpeed);
            double shapeRoll = _random.NextDouble();
            ConfettiParticle confettiParticle = new ConfettiParticle
            {
                Position = position,
                BasePosition = position,
                Velocity = new Vector(Math.Cos(angleRad) * speed, Math.Sin(angleRad) * speed),
                Size = 2 + _random.NextDouble() * 3,
                Brush = _colors[_random.Next(_colors.Count)],
                Shape = shapeRoll < 0.6 ? ConfettiShape.Rectangle // 60% rectangles, 20% ellipses, 20% triangles
                      : shapeRoll < 0.8 ? ConfettiShape.Ellipse
                      : ConfettiShape.Triangle,
                Drag = 0.65 + _random.NextDouble() * 0.3,
                IsWide = _random.Next(2) == 0,
                WobbleAmplitude = 2 + _random.NextDouble() * 6,
                WobbleFrequency = 1 + _random.NextDouble() * 3,
                WobblePhase = _random.NextDouble() * Math.PI * 2,
                Age = 0,
                Rotation = _random.NextDouble() * 360,
                RotationSpeed = (_random.NextDouble() - 0.5) * 2 * (10 + _random.NextDouble() * 300),
                Gravity = gravity

            };
            _particles.Add(confettiParticle);
        }

    }
}

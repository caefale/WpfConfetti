using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf.Confetti
{
    internal class ConfettiParticle
    {
        public Point Position { get; set; }
        public Point BasePosition { get; set; }
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
        public bool IsDead { get; set; } = false;
        public double Gravity { get; set; }
    }

    internal class CannonBatch
    {
        public int Remaining;
        public double MinSpeed, MaxSpeed, Gravity, MinSize, MaxSize, Spread, Rate;
        public IEnumerable<Brush>? Colors;
    }

    internal enum ConfettiShape
    {
        Rectangle,
        Ellipse,
        Triangle
    }

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
        private List<Action>? _pendingActions;

        private static readonly Random _random = new();
        private static readonly Predicate<ConfettiParticle> _isDead = p => p.IsDead;

        private bool _isWindowActive = true;
        private bool _isSubscribed = false;
        private double _cannonAccumulator = 0;
        private Queue<CannonBatch> _cannonQueue = new();

        public bool IsRaining { get; private set; }
        private double _rainRate = 80;
        private double _rainAccumulator = 0;
        private double _rainMinSpeed = 60, _rainMaxSpeed = 120;
        private double _rainMinSize = 2, _rainMaxSize = 5;
        private double _rainGravity = 85;
        private IEnumerable<Brush>? _rainColors;


        public ConfettiControl()
        {
            foreach (var brush in _colors) brush.Freeze();

            _particles = new List<ConfettiParticle>();
            IsHitTestVisible = false;

            this.IsVisibleChanged += (s, e) => UpdateRenderSubscription();

            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Activated += (ss, ee) => { _isWindowActive = true; _cannonAccumulator = 0; _lastRenderTime = TimeSpan.Zero; UpdateRenderSubscription(); };
                    window.Deactivated += (ss, ee) => { _isWindowActive = false; UpdateRenderSubscription(); };
                }
                if (_pendingActions != null)
                {
                    foreach (var action in _pendingActions) action();
                    _pendingActions = null;
                }
                UpdateRenderSubscription();
            };


        }

        public void Burst(int amount = 75, Point? position = null, double minSpeed = 50, 
            double maxSpeed = 300, double minSize = 3, double maxSize = 5, double minAngle = 0,
            double maxAngle = 360, double gravity = 85, IEnumerable<Brush>? colors = null)
        {
            if (IsPendingLayout(() => Burst(amount, position, minSpeed, maxSpeed, minSize, maxSize, minAngle, maxAngle, gravity, colors))) return;
            Point p = position ?? new Point(ActualWidth / 2, ActualHeight / 2);

            UpdateRenderSubscription();
            for (int i = 0; i < amount; i++)
                SpawnParticle(p, minAngle, maxAngle, minSpeed, maxSpeed, gravity, minSize, maxSize, 90, colors);
        }

        public void Cannons(int amount = 500, double rate = 75, double spread = 15,
            double minSpeed = 300, double maxSpeed = 500, double minSize = 2,
            double maxSize = 5, double gravity = 120, IEnumerable<Brush>? colors = null)
        {
            if (IsPendingLayout(() => Cannons(amount, rate, spread, minSpeed, maxSpeed, minSize, maxSize, gravity, colors))) return;
            _cannonQueue.Enqueue(new CannonBatch
            {
                Remaining = amount,
                MinSpeed = minSpeed,
                MaxSpeed = maxSpeed,
                Gravity = gravity,
                MinSize = minSize,
                MaxSize = maxSize,
                Spread = spread,
                Rate = rate,
                Colors = colors
            });
            UpdateRenderSubscription();
        }

        private void SpawnCannonParticle(Point position, CannonBatch cannonBatch)
        {
            double targetX = ActualWidth / 2 + (_random.NextDouble() - 0.5) * 80; 
            double targetY = ActualHeight * 0.35;

            Vector dir = new Vector(
                targetX - position.X,
                targetY - position.Y
            );

            dir.Normalize();

            double baseAngle = Math.Atan2(dir.Y, dir.X) * 180 / Math.PI;
            double spread = cannonBatch.Spread;
            double speedScale = ActualHeight / 400.0;
            SpawnParticle(
                position: position,
                minAngle: baseAngle - spread,
                maxAngle: baseAngle + spread,
                minSpeed: cannonBatch.MinSpeed * speedScale,
                maxSpeed: cannonBatch.MaxSpeed * speedScale,
                gravity: cannonBatch.Gravity,
                minSize: cannonBatch.MinSize, maxSize: cannonBatch.MaxSize,
                colors: cannonBatch.Colors); 
        }

        public void StartRain(double rate = 80, double minSpeed = 60, double maxSpeed = 120,
            double minSize = 2, double maxSize = 5, double gravity = 85, IEnumerable<Brush>? colors = null)
        {
            if (IsPendingLayout(() => StartRain(rate, minSpeed, maxSpeed, minSize, maxSize, gravity, colors))) return;
            IsRaining = true;
            _rainMinSpeed = minSpeed;
            _rainMaxSpeed = maxSpeed;
            _rainGravity = gravity;
            _rainMinSize = minSize;
            _rainMaxSize = maxSize;
            _rainRate = rate;
            _rainColors = colors;
            UpdateRenderSubscription();
        }
        public void StopRain()
        {
            IsRaining = false;
            UpdateRenderSubscription();
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
                        double s = p.Size;
                        var geo = new StreamGeometry();
                        using (var ctx = geo.Open())
                        {
                            ctx.BeginFigure(new Point(cx, cy - s), true, true);
                            ctx.LineTo(new Point(cx + s, cy + s), true, false);
                            ctx.LineTo(new Point(cx - s, cy + s), true, false);
                        }
                        dc.DrawGeometry(p.Brush, null, geo);
                        break;
                }
                dc.Pop();

            }
        }

        private void OnFrame(object? sender, EventArgs e)
        {
            var args = (RenderingEventArgs)e;

            var currentRenderingTime = args.RenderingTime;

            if (_lastRenderTime == TimeSpan.Zero)
            {
                _lastRenderTime = currentRenderingTime;
                return;
            }

            double deltaTime;

            if (_isWindowActive)
            {
                deltaTime = (currentRenderingTime - _lastRenderTime).TotalSeconds;
            }
            else
            {
                // Pretend no time passed 
                deltaTime = 0;
                _lastRenderTime = currentRenderingTime;
                return; 
            }

            _lastRenderTime = currentRenderingTime;

            if (_particles.Count == 0 && !IsRaining && _cannonQueue.Count == 0) return;

            if (IsRaining)
            {
                _rainAccumulator += deltaTime;
                double interval = 1.0 / _rainRate;
                while (_rainAccumulator >= interval)
                {
                    SpawnParticle(position: new Point(_random.NextDouble() * ActualWidth, -10),
                        minAngle: 85, maxAngle: 95, 
                        minSpeed: _rainMinSpeed, 
                        maxSpeed: _rainMaxSpeed,
                        gravity: _rainGravity, 
                        minSize: _rainMinSize, 
                        maxSize: _rainMaxSize,
                        colors: _rainColors);
                    _rainAccumulator -= interval;
                }
            }

            if (_cannonQueue.Count > 0)
            {
                _cannonAccumulator += deltaTime;
                while (_cannonQueue.Count > 0)
                {
                    var batch = _cannonQueue.Peek();
                    double interval = 1.0 / batch.Rate;
                    if (_cannonAccumulator < interval) break;

                    SpawnCannonParticle(new Point(0, ActualHeight), batch);
                    SpawnCannonParticle(new Point(ActualWidth, ActualHeight), batch);
                    _cannonAccumulator -= interval;

                    if ((batch.Remaining -= 2) <= 0)
                    {
                        _cannonQueue.Dequeue();
                        _cannonAccumulator = 0;
                    }
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
                double drag = Math.Pow(p.Drag, deltaTime);
                p.Velocity *= drag;
                p.RotationSpeed *= drag;

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
            if (_hasDead) _particles.RemoveAll(_isDead);
        }

        private void SpawnParticle(Point position, double minAngle, double maxAngle,
            double minSpeed, double maxSpeed, double gravity, double minSize, double maxSize, 
            int angleAdjustment = 0, IEnumerable<Brush>? colors = null)
        {
            double angleDeg = minAngle + _random.NextDouble() * (maxAngle - minAngle);
            angleDeg -= angleAdjustment;
            double angleRad = angleDeg * Math.PI / 180.0;

            double speed = minSpeed + _random.NextDouble() * (maxSpeed - minSpeed);
            double shapeRoll = _random.NextDouble();
            var colorList = (colors ?? _colors).ToList();

            ConfettiParticle confettiParticle = new ConfettiParticle
            {
                Position = position,
                BasePosition = position,
                Velocity = new Vector(Math.Cos(angleRad) * speed, Math.Sin(angleRad) * speed),
                Size = minSize + _random.NextDouble() * (maxSize - minSize),
                Brush = colorList[_random.Next(colorList.Count)],
                Shape = shapeRoll < 0.7 ? ConfettiShape.Rectangle // 70% rectangles, 25% ellipses, 5% triangles
                      : shapeRoll < 0.95 ? ConfettiShape.Ellipse
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
            UpdateRenderSubscription();
        }

        private void UpdateRenderSubscription()
        {
            bool shouldBeSubscribed = IsVisible &&
                (_particles.Count > 0 || IsRaining || _cannonQueue.Count > 0);

            if (shouldBeSubscribed)
            {
                if (!_isSubscribed) 
                {
                    CompositionTarget.Rendering += OnFrame;
                    _isSubscribed = true;
                }
            }
            else
            {
                if (_isSubscribed)
                {
                    CompositionTarget.Rendering -= OnFrame;
                    _isSubscribed = false;
                }
            }
        }

        private bool IsPendingLayout(Action action)
        {
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                (_pendingActions ??= new()).Add(action);
                return true;
            }
            return false;
        }
    }
}

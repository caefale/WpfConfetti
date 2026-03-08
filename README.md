# WpfConfetti 🎉

A lightweight WPF confetti control with burst, cannons, and rain modes.

![GIF](https://github.com/user-attachments/assets/6f73b2ad-0b69-4178-b29f-4a2aecbcdd8e)

## Usage
Add the control to your XAML:
```xml
xmlns:confetti="clr-namespace:Wpf.Confetti;assembly=Wpf.Confetti"

<confetti:ConfettiControl x:Name="Confetti"/>
```

Then trigger effects from code:
```csharp
// One-time burst from center
Confetti.Burst();

// Cannons from both sides
Confetti.Cannons();

// Continuous rain
Confetti.StartRain();
Confetti.StopRain();
```

## API
| Method      | Parameters                                                                                                   |
| ----------- | ------------------------------------------------------------------------------------------------------------ |
| `Burst`     | `amount`, `position`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `minAngle`/`maxAngle`, `gravity`, `colors` |
| `Cannons`   | `amount`, `rate`, `spread`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `gravity`, `colors`                  |
| `StartRain` | `rate`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `gravity`, `colors`                                      |
| `StopRain`  |                                                                                                              |

# WpfConfetti 🎉

A lightweight WPF confetti control with burst, cannons, and rain modes.

![GIF](https://raw.githubusercontent.com/caefale/WpfConfetti/master/assets/preview.gif)

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
## Palettes
WpfConfetti includes built-in color palettes via the `ConfettiPalette` class:

```csharp
// Default color palette
Confetti.Burst(colors: ConfettiPalette.Default);

// Warm red-orange-gold tones
Confetti.Burst(colors: ConfettiPalette.Fire);

// Cool white-blue-silver tones
Confetti.Burst(colors: ConfettiPalette.Snow);

// High saturation primary colors
Confetti.Burst(colors: ConfettiPalette.Party);
```

## API
| Method      | Parameters                                                                                                   |
| ----------- | ------------------------------------------------------------------------------------------------------------ |
| `Burst`     | `amount`, `position`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `minAngle`/`maxAngle`, `gravity`, `colors` |
| `Cannons`   | `amount`, `rate`, `spread`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `gravity`, `colors`                  |
| `StartRain` | `rate`, `minSpeed`/`maxSpeed`, `minSize`/`maxSize`, `gravity`, `colors`                                      |
| `StopRain`  |                                                                                                              |

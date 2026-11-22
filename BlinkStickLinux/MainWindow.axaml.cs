using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using BlinkStickNETCore;

namespace BlinkStickLinux;

public partial class MainWindow : Window
{
    private readonly ConfigWriter configWriter = new();
    private readonly BlinkStickFlex blinkstick = new();

    public MainWindow()
    {
        InitializeComponent();

        sliderRed.ValueChanged += (s, e) => UpdatePreview();
        sliderGreen.ValueChanged += (s, e) => UpdatePreview();
        sliderBlue.ValueChanged += (s, e) => UpdatePreview();
        setColorButton.Click += SetColor_Click;
        saveConfigButton.Click += SaveConfig_Click;

        turnOffSwitch.IsChecked = configWriter.loadedConfig.TurnOffOnExit == true;
        turnOffSwitch.IsCheckedChanged += (s, e) => SaveTurnOff(turnOffSwitch.IsChecked == true);

        UpdatePreview();

        // Load config
        Console.WriteLine("Loading config");
        ApplyConfig();
    }

    private void SetColor_Click(object? sender, RoutedEventArgs e)
    {
        blinkstick.SetColorAll(0, new byte[] { (byte)sliderRed.Value, (byte)sliderGreen.Value, (byte)sliderBlue.Value });
    }

    private void Preset_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag })
        {
            return;
        }

        var parts = tag.Split(',');
        byte r = byte.Parse(parts[0]);
        byte g = byte.Parse(parts[1]);
        byte b = byte.Parse(parts[2]);

        // reflect the preset in the sliders + preview, then apply
        sliderRed.Value = r;
        sliderGreen.Value = g;
        sliderBlue.Value = b;
        UpdatePreview();
        blinkstick.SetColorAll(0, new byte[] { r, g, b });
    }

    private void SaveConfig_Click(object? sender, RoutedEventArgs e)
    {
        // Save into the loaded config so TurnOffOnExit is preserved and the
        // in-memory copy stays in sync with the file.
        var config = configWriter.loadedConfig;
        config.StartupColor = new Tuple<int, int, int>((int)sliderRed.Value, (int)sliderGreen.Value, (int)sliderBlue.Value);
        configWriter.Save(config);
    }

    private void UpdatePreview()
    {
        var r = (byte)sliderRed.Value;
        var g = (byte)sliderGreen.Value;
        var b = (byte)sliderBlue.Value;
        var color = MakeColor(r, g, b);
        var contrast = Contrast(color);
        var colorBrush = new SolidColorBrush(color);
        var contrastBrush = new SolidColorBrush(contrast);

        previewSwatch.Background = colorBrush;
        previewHex.Foreground = contrastBrush;
        previewRgb.Foreground = contrastBrush;
        previewHex.Text = Hex(r, g, b);
        previewRgb.Text = $"R {r}   G {g}   B {b}";

        valueRed.Text = r.ToString();
        valueGreen.Text = g.ToString();
        valueBlue.Text = b.ToString();

        setColorButton.Background = colorBrush;
        setColorButton.Foreground = contrastBrush;
    }

    private static Color MakeColor(byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);

    private static string Hex(byte r, byte g, byte b) => $"#{r:X2}{g:X2}{b:X2}";

    private static Color Contrast(Color c)
    {
        double lum = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
        return lum > 0.6 ? Color.FromUInt32(0xFF1A1A1A) : Color.FromUInt32(0xFFFFFFFF);
    }

    private void SaveTurnOff(bool on)
    {
        var config = configWriter.loadedConfig;
        if (config == null)
        {
            return;
        }
        config.TurnOffOnExit = on;
        configWriter.Save(config);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (configWriter.loadedConfig.TurnOffOnExit == true)
        {
            blinkstick.SetColorAll(0, new byte[] { 0, 0, 0 });
        }

        blinkstick.Shutdown();
    }

    private void ApplyConfig()
    {
        Console.WriteLine("Applying config");
        var loadedConfig = configWriter.loadedConfig;
        if (loadedConfig == null)
        {
            Console.WriteLine("No config found, using default values");
            return;
        }
        if (loadedConfig.StartupColor == null)
        {
            Console.WriteLine("No startup color found, using default values");
            return;
        }

        // Reflect the startup color in the sliders so the preview matches.
        sliderRed.Value = loadedConfig.StartupColor.Item1;
        sliderGreen.Value = loadedConfig.StartupColor.Item2;
        sliderBlue.Value = loadedConfig.StartupColor.Item3;
        UpdatePreview();

        blinkstick.SetColorAll(0, new byte[] { (byte)loadedConfig.StartupColor.Item1, (byte)loadedConfig.StartupColor.Item2, (byte)loadedConfig.StartupColor.Item3 });
    }
}

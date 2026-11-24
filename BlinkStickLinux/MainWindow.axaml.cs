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

        colorPicker.ColorChanged += (s, e) => UpdatePreview();
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
        var c = colorPicker.Color;
        try
        {
            blinkstick.SetColorAll(0, new byte[] { c.R, c.G, c.B });
            SetStatus("Color applied.");
        }
        catch (Exception ex)
        {
            SetStatus($"Could not set color: {ex.Message}");
        }
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

        // reflect the preset in the picker + preview, then apply
        colorPicker.Color = MakeColor(r, g, b);
        UpdatePreview();
        try
        {
            blinkstick.SetColorAll(0, new byte[] { r, g, b });
            SetStatus("Color applied.");
        }
        catch (Exception ex)
        {
            SetStatus($"Could not set color: {ex.Message}");
        }
    }

    private void SaveConfig_Click(object? sender, RoutedEventArgs e)
    {
        // Save into the loaded config so TurnOffOnExit is preserved and the
        // in-memory copy stays in sync with the file.
        var config = configWriter.loadedConfig;
        var c = colorPicker.Color;
        config.StartupColor = new Tuple<int, int, int>(c.R, c.G, c.B);

        try
        {
            configWriter.Save(config);
            SetStatus("Config saved.");
        }
        catch (Exception ex)
        {
            SetStatus($"Could not save config: {ex.Message}");
        }
    }

    private void UpdatePreview()
    {
        var c = colorPicker.Color;
        var r = c.R;
        var g = c.G;
        var b = c.B;
        var color = MakeColor(r, g, b);
        var contrast = Contrast(color);
        var colorBrush = new SolidColorBrush(color);
        var contrastBrush = new SolidColorBrush(contrast);

        previewSwatch.Background = colorBrush;
        previewHex.Foreground = contrastBrush;
        previewRgb.Foreground = contrastBrush;
        previewHex.Text = Hex(r, g, b);
        previewRgb.Text = $"R {r}   G {g}   B {b}";

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
        try
        {
            configWriter.Save(config);
        }
        catch (Exception ex)
        {
            SetStatus($"Could not save config: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        statusText.Text = message;
        statusText.Foreground = new SolidColorBrush(Color.FromUInt32(0xFF1A1A1A));
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (configWriter.loadedConfig.TurnOffOnExit == true)
        {
            try
            {
                blinkstick.SetColorAll(0, new byte[] { 0, 0, 0 });
            }
            catch
            {
                // Best-effort on exit; don't block shutdown.
            }
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

        // Reflect the startup color in the picker so the preview matches.
        colorPicker.Color = MakeColor((byte)loadedConfig.StartupColor.Item1, (byte)loadedConfig.StartupColor.Item2, (byte)loadedConfig.StartupColor.Item3);
        UpdatePreview();

        try
        {
            blinkstick.SetColorAll(0, new byte[] { (byte)loadedConfig.StartupColor.Item1, (byte)loadedConfig.StartupColor.Item2, (byte)loadedConfig.StartupColor.Item3 });
        }
        catch (Exception ex)
        {
            SetStatus($"Could not set startup color: {ex.Message}");
        }
    }
}

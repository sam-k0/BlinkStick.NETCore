using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using BlinkStickNETCore;

public partial class MainWindow : Window
{
    private readonly ConfigWriter configWriter = new();
    private readonly BlinkStickFlex blinkstick = new();

    private Slider sliderRed = null!;
    private Slider sliderGreen = null!;
    private Slider sliderBlue = null!;
    private TextBlock valueRed = null!;
    private TextBlock valueGreen = null!;
    private TextBlock valueBlue = null!;

    private Border previewSwatch = null!;
    private TextBlock previewHex = null!;
    private TextBlock previewRgb = null!;
    private Button setColorButton = null!;

    // Preset colors: name, R, G, B
    private static readonly (string, byte, byte, byte)[] presetColors =
    {
        ("Red", 255, 0, 0),
        ("Green", 0, 255, 0),
        ("Blue", 0, 0, 255),
        ("Yellow", 255, 255, 0),
        ("Purple", 255, 0, 255),
        ("Cyan", 0, 255, 255),
        ("White", 255, 255, 255),
        ("Off", 0, 0, 0)
    };

    public MainWindow()
    {
        Title = "BlinkStick Control Panel";
        Width = 720;
        Height = 760;
        MinWidth = 560;

        BuildPreview();
        var slidersPanel = BuildSliders();
        BuildSetColorButton();

        // Presets header
        var presetsHeader = new TextBlock
        {
            Text = "Presets",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(2, 12, 0, 0)
        };

        // Preset swatches
        var presetsPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal
        };
        foreach (var (colorName, r, g, b) in presetColors)
        {
            var color = MakeColor(r, g, b);
            var presetButton = new Button
            {
                Content = colorName,
                Width = 132,
                Height = 56,
                Background = new SolidColorBrush(color),
                Foreground = new SolidColorBrush(Contrast(color)),
                Margin = new Thickness(6)
            };
            presetButton.Click += (s, e) =>
            {
                // reflect the preset in the sliders + preview, then apply
                sliderRed.Value = r;
                sliderGreen.Value = g;
                sliderBlue.Value = b;
                UpdatePreview();
                blinkstick.SetColorAll(0, new byte[] { r, g, b });
            };
            presetsPanel.Children.Add(presetButton);
        }

        // Save config
        var saveConfigButton = new Button
        {
            Content = "Save Config",
            Width = 220,
            Height = 40,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        saveConfigButton.Click += (s, e) =>
        {
            var config = new Config
            {
                StartupColor = new Tuple<int, int, int>((int)sliderRed.Value, (int)sliderGreen.Value, (int)sliderBlue.Value)
            };
            configWriter.Save(config);
        };

        // Turn off on exit
        var turnOffSwitch = new ToggleSwitch
        {
            IsChecked = configWriter.loadedConfig.TurnOffOnExit == true
        };
        turnOffSwitch.IsCheckedChanged += (s, e) => SaveTurnOff(turnOffSwitch.IsChecked == true);

        var turnOffRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 12,
            Margin = new Thickness(0, 18, 0, 0)
        };
        turnOffRow.Children.Add(new TextBlock
        {
            Text = "Turn off on exit",
            FontSize = 14,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        turnOffRow.Children.Add(turnOffSwitch);

        var root = new Border
        {
            Padding = new Thickness(24),
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    previewSwatch,
                    slidersPanel,
                    setColorButton,
                    presetsHeader,
                    presetsPanel,
                    saveConfigButton,
                    turnOffRow
                }
            }
        };

        Content = root;

        UpdatePreview();

        // Load config
        Console.WriteLine("Loading config");
        ApplyConfig(configWriter, blinkstick);
    }

    private void BuildPreview()
    {
        previewHex = new TextBlock
        {
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        previewRgb = new TextBlock
        {
            FontSize = 14,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        previewSwatch = new Border
        {
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 18, 20, 18),
            MinHeight = 96,
            Background = new SolidColorBrush(MakeColor(0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromUInt32(0xFFDDDDDD)),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Children = { previewHex, previewRgb }
            }
        };
    }

    private StackPanel BuildSliders()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 28
        };
        panel.Children.Add(MakeSliderChannel("Red", out sliderRed, out valueRed));
        panel.Children.Add(MakeSliderChannel("Green", out sliderGreen, out valueGreen));
        panel.Children.Add(MakeSliderChannel("Blue", out sliderBlue, out valueBlue));

        sliderRed.ValueChanged += (s, e) => UpdatePreview();
        sliderGreen.ValueChanged += (s, e) => UpdatePreview();
        sliderBlue.ValueChanged += (s, e) => UpdatePreview();

        return panel;
    }

    private static StackPanel MakeSliderChannel(string label, out Slider slider, out TextBlock valueLabel)
    {
        slider = new Slider
        {
            Minimum = 0,
            Maximum = 255,
            Value = 0,
            Width = 170
        };
        valueLabel = new TextBlock
        {
            Text = "0",
            FontSize = 13,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                },
                slider,
                valueLabel
            }
        };
    }

    private void BuildSetColorButton()
    {
        setColorButton = new Button
        {
            Content = "Set Color",
            Width = 220,
            Height = 44,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        setColorButton.Click += (s, e) =>
        {
            blinkstick.SetColorAll(0, new byte[] { (byte)sliderRed.Value, (byte)sliderGreen.Value, (byte)sliderBlue.Value });
        };
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

    private static void ApplyConfig(ConfigWriter configWriter, BlinkStickFlex blinkstick)
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

        blinkstick.SetColorAll(0, new byte[] { (byte)loadedConfig.StartupColor.Item1, (byte)loadedConfig.StartupColor.Item2, (byte)loadedConfig.StartupColor.Item3 });
    }

    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
        appBuilder.StartWithClassicDesktopLifetime(args);
    }
}

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}

using Gtk;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
public partial class MainWindow : Gtk.Window
{
    private static ConfigWriter configWriter = new ConfigWriter();
    private Gtk.ScrolledWindow scrolledWindow;
    private Gtk.Layout layout;

    private Button setColorButton;

    private HScale SliderRed;
    private HScale SliderGreen;
    private HScale SliderBlue;


    // Add some preset colors
    List<(string, byte[])> presetColors = new List<(string, byte[])> {
        ("Red", new byte[] { 255, 0, 0 }),
        ("Green", new byte[] { 0, 255, 0 }),
        ("Blue", new byte[] { 0, 0, 255 }),
        ("Yellow", new byte[] { 255, 255, 0 }),
        ("Purple", new byte[] { 255, 0, 255 }),
        ("Cyan", new byte[] { 0, 255, 255 }),
        ("White", new byte[] { 255, 255, 255 }),
        ("Black", new byte[] { 0, 0, 0 })
    };

    private BlinkstickController blinkstick = new BlinkstickController();

    private static bool IsSuperuser()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUDO_USER"));
    }

    private static void ShowPopup(string message, string title, MessageType messageType)
    {
        var dialog = new MessageDialog(null, DialogFlags.Modal, messageType, ButtonsType.Ok, message)
        {
            Title = title
        };
        dialog.Run();
        dialog.Destroy();
    }
    public static void ApplyConfig(ConfigWriter configWriter, BlinkstickController blinkstick)
    {   
        Console.WriteLine("Applying config");
        var loadedConfig = configWriter.loadedConfig;
        if (loadedConfig == null)
        {
            Console.WriteLine("No config found, using default values");
            return;
        }
        if(loadedConfig.StartupColor == null)
        {
            Console.WriteLine("No startup color found, using default values");
            return;
        }

        blinkstick.SetColorAll(0, new byte[] { (byte)loadedConfig.StartupColor.Item1, (byte)loadedConfig.StartupColor.Item2, (byte)loadedConfig.StartupColor.Item3 });
    }

    private Button AddButton(Layout layout, string text, int x, int y, int width, int height, EventHandler OnButtonClicked)
    {
        var button = new Gtk.Button(text);
        button.WidthRequest = width;
        button.HeightRequest = height;
        button.Clicked += OnButtonClicked;
        layout.Put(button, x, y);
        return button;
    }

    [Obsolete]
    private HScale AddSliderWithLabel(Layout layout, string label, int x, int y, int width, int height, int min, int max, int step, EventHandler OnValueChanged)
    {
        var labelWidget = new Gtk.Label(label);
        labelWidget.WidthRequest = width;
        labelWidget.HeightRequest = height;
        layout.Put(labelWidget, x, y);

        var slider = new HScale(min, max, step);
        slider.WidthRequest = width;
        slider.HeightRequest = height;

        slider.ValueChanged += OnValueChanged;

        layout.Put(slider, x, y + height);
        return slider;
    }

    [Obsolete]
    private void SetColorButtonTextColor()
    {
        if (setColorButton == null || SliderBlue == null || SliderGreen == null || SliderRed == null)
        {
            return;
        }
        var color = new byte[] {
            (byte)SliderRed.Value,
            (byte)SliderGreen.Value,
            (byte)SliderBlue.Value
        };
        setColorButton.ModifyFg(StateType.Normal, new Gdk.Color(color[0], color[1], color[2]));
    }

    [Obsolete]
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        // Set window properties
        Title = "BlinkStick GTK Control Panel";
        DefaultWidth = 600;
        DefaultHeight = 400;

        // Create scrolled window
        scrolledWindow = new Gtk.ScrolledWindow();
        scrolledWindow.HscrollbarPolicy = PolicyType.Never;
        scrolledWindow.VscrollbarPolicy = PolicyType.Never;

        // Create layout
        layout = new Gtk.Layout(null, null);
        layout.Width = 600;
        layout.Height = 400;

        SliderRed = AddSliderWithLabel(layout, "Red",     0, 0, 200, 25, 0, 255, 1, (sender, e) => {
            SetColorButtonTextColor();
        });
        SliderGreen = AddSliderWithLabel(layout, "Green", 200, 0, 200, 25, 0, 255, 1, (sender, e) => {
            SetColorButtonTextColor();
        });
        SliderBlue = AddSliderWithLabel(layout, "Blue",   400, 0, 200, 25, 0, 255, 1, (sender, e) => {
            SetColorButtonTextColor();
        });

        setColorButton = AddButton(layout, "Set Color", 100, 75, 400, 25, (sender, e) => {
            blinkstick.SetColorAll(0, new byte[] {
                (byte)SliderRed.Value,
                (byte)SliderGreen.Value,
                (byte)SliderBlue.Value
            });
        });



        Label label = new Label("Preset Colors");
        // Center the label
        label.SetSizeRequest(600, 25);
        layout.Put(label, 0, 125);

        // Add buttons for preset colors
        for (int i = 0; i < presetColors.Count; i++)
        {
            var (colorName, color) = presetColors[i];
            // Place buttons in a 4x2 grid, each button is 150x25, starting placements at (0, 100)
            AddButton(layout, colorName, (i % 4) * 150, 150 + (i / 4) * 50, 150, 25, (sender, e) => {
                blinkstick.SetColorAll(0, color);
            });

        }

        // button to save config
        AddButton(layout, "Save Config", 100, 300, 400, 25, (sender, e) => 
        {
            var config = new Config
            {
                StartupColor = new Tuple<int, int, int>((int)SliderRed.Value, (int)SliderGreen.Value, (int)SliderBlue.Value)
            };
            configWriter.Save(config);
        });

        Button saveOnExit = AddButton(layout, "Toggle Turn Off On Exit", 100, 350, 400, 25, (sender, e) => 
        {
            var config = configWriter.loadedConfig;
            if (config == null)
            {
                return;
            }
            config.TurnOffOnExit = !config.TurnOffOnExit;
            configWriter.Save(config);
            // Update button label by args (sender, e)
            Button self = (Button)sender;
            if(self != null)
            {
                self.Label = config.TurnOffOnExit == true ? "Turn Off On Exit: On" : "Turn Off On Exit: Off";
            }
            
        });
        saveOnExit.Label = configWriter.loadedConfig.TurnOffOnExit == true ? "Turn Off On Exit: On" : "Turn Off On Exit: Off";

        scrolledWindow.Add(layout);
        Add(scrolledWindow);

        // Connect delete event
        DeleteEvent += OnDeleteEvent;

        // Load config
        Console.WriteLine("Loading config");
        ApplyConfig(configWriter, blinkstick);


    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        // Free resources and quit application
        if (configWriter.loadedConfig.TurnOffOnExit == true)
        {
            blinkstick.SetColorAll(0, new byte[] { 0, 0, 0 }); 
        }

        blinkstick.Shutdown();

        Application.Quit();
        a.RetVal = true;
    }

    [Obsolete]
    public static void Main(string[] args)
    {
        Application.Init();

        if (!IsSuperuser())
        {
            // Show error popup
            Console.WriteLine("Please run this program as superuser.");
            ShowPopup("Please run this program as superuser.", "Error", MessageType.Error);
            return;
        }
        MainWindow win = new MainWindow
        {
            // disable resizing
            Resizable = false
        };
        win.ShowAll(); // Show all widgets
        Application.Run();
    }
}

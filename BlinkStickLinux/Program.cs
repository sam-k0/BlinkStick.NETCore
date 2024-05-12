using Gtk;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
public partial class MainWindow : Gtk.Window
{
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
        Title = "Amogus Window";
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


        scrolledWindow.Add(layout);
        Add(scrolledWindow);

        // Connect delete event
        DeleteEvent += OnDeleteEvent;

    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        // Free resources and quit application
        blinkstick.Shutdown();

        Application.Quit();
        a.RetVal = true;
    }

    [Obsolete]
    public static void Main(string[] args)
    {
        if (!IsSuperuser())
        {
            // Show error popup
            Console.WriteLine("Please run this program as superuser.");
            return;
        }

        Application.Init();
        MainWindow win = new MainWindow
        {
            // disable resizing
            Resizable = false
        };
        win.ShowAll(); // Show all widgets
        Application.Run();
    }
}

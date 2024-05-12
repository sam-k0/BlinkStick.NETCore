using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using System.Diagnostics;
public class Config
{
    public Tuple<int, int, int> ?StartupColor { get; set; }
}

public class ConfigWriter
{
    public Config loadedConfig;

    private string GetApplicationPath()
    {
        var process = Process.GetCurrentProcess();
        var path = process.MainModule.FileName;
        return Path.GetDirectoryName(path);
    }


    public ConfigWriter()
    {
        // Check if config file exists
        if(!File.Exists(GetApplicationPath() + "/config.json"))
        {
            // Create a new config file with default values 
            var defaultConfig = new Config
            {
                StartupColor = new Tuple<int, int, int>(255, 0, 0)
            };

            File.WriteAllText(GetApplicationPath()+"/config.json", JsonConvert.SerializeObject(defaultConfig));
        }
        // Load config file
        var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(GetApplicationPath()+"/config.json"));
        if (config == null)
        {
            throw new Exception("Failed to load config file");
        }
        loadedConfig = config;
    }

    public void Save(Config config)
    {
        File.WriteAllText(GetApplicationPath()+"/config.json", JsonConvert.SerializeObject(config));
    }

}
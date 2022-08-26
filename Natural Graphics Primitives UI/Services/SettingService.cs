using System;
using System.IO;
using Natural_Graphics_Primitives_UI.Models;
using Newtonsoft.Json;

namespace Natural_Graphics_Primitives_UI.Services;

public class SettingService
{
    private string _dataPath { get; set; } = "settings.json";

    public SettingsModel Settings
    {
        get => !File.Exists(_dataPath) ? new SettingsModel() : LoadData();
        set => SaveData(value);
    }

    private void SaveData(SettingsModel data)
    {
        try
        {
            File.WriteAllText(_dataPath, JsonConvert.SerializeObject(data));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private SettingsModel LoadData()
    {
        try
        {
            return JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(_dataPath));
        }
        catch (Exception e)
        {
            return default;
        }
    }
}
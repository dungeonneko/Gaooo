using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GaoooRuntime
{
    public class ProjectSettings
    {
        public string Title { get; private set; }
        public bool WindowResizable { get; private set; }
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }
        public int RenderWidth { get; private set; }
        public int RenderHeight { get; private set; }
        public List<string> MacroScripts { get; private set; }
        public string EntryPoint { get; private set; }

        public ProjectSettings(JsonDocument doc)
        {
            var rootElement = doc.RootElement;
            Title = getProperty(rootElement, "title", "GaoooProject");
            WindowResizable = getProperty(rootElement, "window_resizable", true);
            WindowWidth = getProperty(rootElement, "window_width", 1280);
            WindowHeight = getProperty(rootElement, "window_height", 720);
            RenderWidth = getProperty(rootElement, "render_width", WindowWidth);
            RenderHeight = getProperty(rootElement, "render_height", WindowHeight);
            MacroScripts = getProperty(rootElement, "macro_scripts", MacroScripts);
            EntryPoint = getProperty(rootElement, "entry_point", string.Empty);
        }

        private T getProperty<T>(JsonElement rootElement, string propertyName, T defaultValue)
        {
            if (rootElement.TryGetProperty(propertyName, out var property))
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)Convert.ChangeType(property.GetString(), typeof(T));
                    }
                    else if (typeof(T) == typeof(List<string>))
                    {
                        var jsonString = property.GetRawText();
                        return JsonSerializer.Deserialize<T>(jsonString);
                    }
                    else
                    {
                        return (T)Convert.ChangeType(property.GetRawText(), typeof(T));
                    }
                }
                catch (Exception)
                {
                    // Conversion failed, return default value
                }
            }
            return defaultValue;
        }
    }
}

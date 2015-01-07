using System;
using System.Configuration;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF.Properties;

namespace Invert.GraphDesigner.WPF
{
    public class WindowsPrefs : IPlatformPreferences
    {
        public object GetSetting(string name, object def)
        {
            if (Settings.Default.Properties[name] == null)
            {
                Properties.Settings.Default.Properties.Add(new SettingsProperty(name)
                {
                    DefaultValue = def,
                    SerializeAs = SettingsSerializeAs.String
                });
                Settings.Default.Save();
                return def;
            }

            return Settings.Default.Properties[name].DefaultValue;
        }

        public void SetSetting(string name, object val)
        {
            var setting = Settings.Default.Properties[name];
            if ( setting == null)
            {
                Properties.Settings.Default.Properties.Add(new SettingsProperty(name)
                {
                    DefaultValue = val,
                    SerializeAs = SettingsSerializeAs.String
                });

            }
            else
            {
                setting.DefaultValue = val;
            }
            Settings.Default.Save();
        }
        public bool GetBool(string name, bool def)
        {
            return Convert.ToBoolean(GetSetting(name, def));
        }

        public string GetString(string name, string def)
        {
            return Convert.ToString(GetSetting(name, def));
        }

        public float GetFloat(string name, float def)
        {
            return Convert.ToSingle(GetSetting(name, def));
        }

        public float GetInt(string name, int def)
        {
            return Convert.ToInt32(GetSetting(name, def));
        }

        public void SetBool(string name, bool value)
        {
            SetSetting(name,value);
        }

        public void SetString(string name, string value)
        {
            SetSetting(name, value);
        }

        public void SetFloat(string name, float value)
        {
            SetSetting(name, value);
        }

        public void SetInt(string name, int value)
        {
            SetSetting(name, value);
        }

        public bool HasKey(string name)
        {
            return Settings.Default.Properties[name] != null;
        }

        public void DeleteKey(string name)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }
    }
}
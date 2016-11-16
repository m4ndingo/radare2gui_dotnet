using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace r2pipe_test
{
    public class RConfig
    {
        public string r2path = null;
        public string tempPath = null;
        public RConfig()
        {
            ReloadConfig();
        }
        public void ReloadConfig() { 
            r2path = reg_read(@"SOFTWARE\r2pipe_gui_dotnet", "r2path");         
            tempPath = System.IO.Path.GetTempPath(); ;
        }
        public void save(string name, string value)
        {
            reg_write(@"SOFTWARE\r2pipe_gui_dotnet", name, value);
        }
        public void reg_write(string subkey, string name, string value)
        {
            RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subkey);
            key.SetValue(name, value);
            key.Close();
        }
        public string reg_read(string subkey, string name)
        {
            RegistryKey key;
            object value;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey);
            if (key == null) return null;
            value = key.GetValue(name);
            key.Close();
            return value.ToString();
        }
    }
}

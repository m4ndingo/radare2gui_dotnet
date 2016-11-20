using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace r2pipe_test
{
    public class RConfig
    {
        public string r2path   = null;
        public string tempPath = null;
        public string guiPath  = null;
        public string dataPath = null;
        public string regPath  = null;
        public RConfig()
        {
            ReloadConfig();
        }
        public void ReloadConfig() {
            regPath  = @"SOFTWARE\r2pipe_gui_dotnet";
            r2path   = reg_read(regPath, "r2path");         
            tempPath = System.IO.Path.GetTempPath();
            guiPath  = Directory.GetCurrentDirectory();
            dataPath = reg_read(regPath, "gui.datapath");
        }
        public void save(string name, object value)
        {
            if (value is Int32) value = value.ToString();
            reg_write(@"SOFTWARE\r2pipe_gui_dotnet", name, (string)value);
            ReloadConfig();
        }
        public string load<T>(string name, object default_value=null)
        {
            string value;
            value=reg_read(@"SOFTWARE\r2pipe_gui_dotnet", name);
            if (value == null)
            {
                if (typeof(T) == typeof(int))
                {
                    if (default_value != null)
                    {
                        value = ((int)default_value).ToString();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("no default value for {0} type {1}",name,typeof(T)), "RConfig:load()");
                    }
                }
                else
                    value = (string)default_value;
            }
            return value;
        }
        public void reg_write(string subkey, string name, string value)
        {
            RegistryKey key;
            if (value == null)
            {
                Console.WriteLine("reg_write: subkey={0} name={1} null?", subkey, name);
                return;
            }
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subkey);
            key.SetValue(name, value);
            key.Close();
        }
        public string reg_read(string subkey, string name)
        {
            RegistryKey key;
            object value;
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey);
                value = key.GetValue(name);
                key.Close();
                return value==null?null:value.ToString();
            }
            catch (Exception) { }
            return null;
        }
        public List<string> reg_enumkeys(string subkey=null)
        {
            RegistryKey key;
            List<string> results=new List<string>();
            try
            {
                if (subkey == null) subkey = regPath;
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey);
                foreach (string valueName in key.GetValueNames())
                {
                    //Console.WriteLine("key: " + valueName);
                    results.Add(valueName);
                }
                key.Close();
                return results;
            }
            catch (Exception) { }
            return null;
        }
    }
}

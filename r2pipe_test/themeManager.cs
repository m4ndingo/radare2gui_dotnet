using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r2pipe_test
{
    class themeManager
    {
        public string themeName = null;
        private RConfig config = null;
        public themeManager(string themeName, RConfig config)
        {
            this.config = config;
            set_theme(themeName);
        }
        public void set_theme(string themeName)
        {
            this.themeName = themeName;
            config.save("gui.theme_name", themeName);
            switch (themeName)
            {
                case "classic":
                    config.save("gui.output.bg", "white");
                    config.save("gui.output.fg", "black");
                    break;
                case "blue":
                    config.save("gui.output.bg", "blue");
                    config.save("gui.output.fg", "white");
                    break;
                case "pink":
                    config.save("gui.output.bg", "indigo");
                    config.save("gui.output.fg", "white");
                    break;
                case "control":
                    config.save("gui.output.bg", "Control");
                    config.save("gui.output.fg", "ControlText");
                    break;
            }
            Console.WriteLine("set_theme: {0}", themeName);
        }
    }
}

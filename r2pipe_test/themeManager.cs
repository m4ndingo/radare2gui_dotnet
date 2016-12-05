using System;
using System.Drawing;

namespace r2pipe_test
{
    class themeManager
    {
        public string themeName = null;
        private RConfig config = null;
        public themeManager(RConfig config)
        {
            this.config = config;
            string themeName = config.load<string>("gui.theme_name");
            if( themeName != null )
               set_theme(themeName);
        }
        public void set_theme(string themeName)
        {
            this.themeName = themeName;
            config.save("gui.theme_name", themeName);
            config.save("gui.hexview.css", string.Format("hexview.{0}.css", themeName));
            switch (themeName)
            {
                case "classic":
                    config.save("gui.output.bg", "white");
                    config.save("gui.output.fg", "black");
                    config.save("gui.output.fg.fcn", "black");
                    config.save("gui.output.bg.fcn", "white");
                    config.save("gui.output.fg.loc", "black");
                    config.save("gui.output.bg.loc", "lime");
                    config.save("gui.output.fg.sym", "pink");
                    config.save("gui.output.bg.sym", "white");
                    config.save("gui.output.fg.imp", "black");
                    config.save("gui.output.bg.imp", "lightcyan");
                    config.save("gui.output.fg.sub", "black");
                    config.save("gui.output.bg.sub", "white");//DarkTurquoise
                    config.save("gui.output.fg.entry0", "highlight");
                    config.save("gui.output.bg.entry0", "white");
                    config.save("gui.hexview.css", "r2pipe.css");
                    break;
                case "azure":
                    config.save("gui.output.bg", "Azure");
                    config.save("gui.output.fg", "DarkSlateGray");
                    break;
                case "pink":
                    config.save("gui.output.bg", "Purple");
                    config.save("gui.output.fg", "white");
                    break;
                case "control":
                    config.save("gui.output.bg", "Control");
                    config.save("gui.output.fg", "ControlText");
                    break;
                case "terminal256":
                    config.save("gui.output.bg", "black");
                    config.save("gui.output.fg", "Aquamarine");
                    config.save("gui.output.fg.fcn", "white");
                    config.save("gui.output.bg.fcn", "black");
                    config.save("gui.output.fg.loc", "lime");
                    config.save("gui.output.bg.loc", "black");
                    config.save("gui.output.fg.sym", "pink");
                    config.save("gui.output.bg.sym", "black");
                    config.save("gui.output.fg.imp", "highlight");
                    config.save("gui.output.bg.imp", "black");
                    config.save("gui.output.fg.sub", "DarkTurquoise");
                    config.save("gui.output.bg.sub", "black");
                    config.save("gui.output.fg.entry0", "yellow");
                    config.save("gui.output.bg.entry0", "black");
                    break;
                case "lemon":
                    config.save("gui.output.bg", "Khaki");
                    config.save("gui.output.fg", "black");
                    break;
            }
            Console.WriteLine("set_theme: {0}", themeName);
        }
        public Color get_color_address(string colorLocation, string addr_type, Color def)
        {
            Color found = Color.FromName(
                config.load<Color>(
                    string.Format("gui.output.{0}.{1}", colorLocation, addr_type), "pink")
            );
            if (found == Color.FromName("pink")) return def;
            return found;
        }
        public Color get_current_background()
        {
            Color backColor = Color.FromName(config.load<string>("gui.output.bg", "white"));
            if (backColor.Name.Equals("Purple"))
                backColor = Color.FromArgb(255, 0x23, 0x00, 0x1b); //0xb9, 0x5a, 0x7d);
            return backColor;
        }
    }
}

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace r2pipe_test
{
    public partial class webbrowser_container_form : Form
    {
        R2PIPE_WRAPPER r2pw = null;
        GuiControl control = null;
        IntPtr MenuHandle;
        public webbrowser_container_form(R2PIPE_WRAPPER r2pw, GuiControl control)
        {
            this.r2pw = r2pw;
            this.control = control;
            MenuHandle = GetSystemMenu((System.IntPtr)this.Handle, false);
            InsertMenu(MenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(MenuHandle, 6, MF_BYPOSITION, MENU_SYNCHRONIZE,  "Synchronize");
            InsertMenu(MenuHandle, 7, MF_BYPOSITION, MENU_ALLWAYSONTOP, "Always on top");
            InitializeComponent();
        }

        private void webbrowser_container_form_Load(object sender, System.EventArgs e)
        {
            string layout = r2pw.rconfig.load<string>(string.Format("gui.layout.{0}", control.sName));
            if (layout != null)
            {
                try
                {
                    string[] fields = layout.Split(';');
                    Top = int.Parse(fields[0]);
                    Left = int.Parse(fields[1]);
                    Width = int.Parse(fields[2]);
                    Height = int.Parse(fields[3]);
                    TopMost = bool.Parse(fields[4]);
                }
                catch (Exception) {
                    TopMost = true;
                } // may fail if some field not found
            }
            this.ShowIcon = true;
            refresh_menu();
        }
        public const int WM_SYSCOMMAND      = 0x0112;
        public const int NCACTIVATE         = 0x0086;
        public const int MF_BYPOSITION      = 0x0400;
        public const int MF_CHECKED         = 0x0008;
        public const int MENU_ALLWAYSONTOP  =   1000;
        public const int MENU_SYNCHRONIZE   =   1001;
        public const Int32 MF_SEPARATOR = 0x800;
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
        [DllImport("user32.dll")]
        private static extern bool ModifyMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 uIDNewItem, string lpNewItem);
        protected override void WndProc(ref Message msg)
        {
            // messages list: http://pinvoke.net/default.aspx/Enums/WindowsMessages.html            
            if (msg.Msg == NCACTIVATE)
                this.ShowIcon = !this.ShowIcon;
            if (msg.Msg == WM_SYSCOMMAND)
            {
                switch (msg.WParam.ToInt32())
                {
                    case MENU_ALLWAYSONTOP:
                        this.TopMost = !this.TopMost;
                        refresh_menu();
                        save_layout();
                        return;
                    case MENU_SYNCHRONIZE:
                        control.synchronize = !control.synchronize;
                        refresh_menu();
                        save_layout();
                        return;
                    default:
                        break;
                }
            }
            base.WndProc(ref msg);
        }
        private void refresh_menu()
        {
            Int32 newflags = MF_BYPOSITION;
            if (this.TopMost) newflags |= MF_CHECKED;
            ModifyMenu(MenuHandle, 7, newflags, MENU_ALLWAYSONTOP, "Always on top");
            newflags = MF_BYPOSITION;
            if (control.synchronize) newflags |= MF_CHECKED;
            ModifyMenu(MenuHandle, 6, newflags, MENU_SYNCHRONIZE, "Synchronize");
        }
        private void webbrowser_container_form_ResizeEnd(object sender, EventArgs e)
        {
            save_layout();
        }
        private void save_layout()
        {
            r2pw.rconfig.save(
                string.Format("gui.layout.{0}", control.sName),
                string.Format("{0};{1};{2};{3};{4};", Top, Left, Width, Height, TopMost)
                );
            
        }
        private void webbrowser_container_form_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.unregister_control(control.name);
        }
    }
}

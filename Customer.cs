using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DynamicLottery
{
    class Customer
    {
        public string Lotte { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Acntno { get; set; }
        public string Brchno { get; set; }
        public string BranchName { get; set; }
        public string Register { get; set; }
        public int RepeatCount { get; set; }
    }

    class Lotte
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public Image Image { get; set; }
    }
    
    internal static class NativeWinAPI
    {
        internal static readonly int GWL_EXSTYLE = -20;
        internal static readonly int WS_EX_COMPOSIED = 0x02000000;

        [DllImport("User32")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("User32")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int drNewLong);
    }
}

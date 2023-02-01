using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Principal;
using System.Management;
using System.Diagnostics;
using System.IO;

namespace Yu3zx.Util
{
    public static class WindowsHelper
    {
        #region DoWindowsEvents
        /// <summary>
        /// DoWindowsEvents 在UI线程中调用该方法将使UI线程处理windows事件。
        /// </summary>
        public static void DoWindowsEvents()
        {
            Application.DoEvents();
        }
        #endregion End

        #region 让计算机峰鸣
        [DllImport("kernel32")]
        public static extern int Beep(int dwFreq, int dwDuration);//让计算机蜂鸣

        /// <summary>
        /// 让计算机峰鸣-需要支持峰鸣的计算机
        /// </summary>
        /// <param name="iFreq">频率</param>
        /// <param name="iDuration">持续时间</param>
        public static void SystemBeep(int iFreq,int iDuration)
        {
            int rtn = Beep(iFreq, iDuration);//计算机蜂鸣
        }
        #endregion End

        #region GetMdiChildForm ,MdiChildIsExist
        public static Form GetMdiChildForm(Form parentForm, Type childFormType)
        {
            foreach (Form child in parentForm.MdiChildren)
            {
                if (child.GetType() == childFormType)
                {
                    return child;
                }
            }
            return null;
        }

        public static bool MdiChildIsExist(Form parentForm, Type childFormType)
        {
            if (WindowsHelper.GetMdiChildForm(parentForm, childFormType) != null)
            {
                return true;
            }
            return false;
        }
        #endregion End

        #region 光标所在屏幕位置
        public static Point GetCursorPosition()
        {
            return Cursor.Position;
        }
        #endregion

        #region GetStartupDirectoryPath
        /// <summary>
        /// GetStartupDirectoryPath 获取当前应用程序所在的目录
        /// </summary>        
        public static string GetStartupDirectoryPath()
        {
            return Application.StartupPath; //AppDomain.CurrentDomain.BaseDirectory
        }
        #endregion

        #region MouseEvent 模拟鼠标/键盘点击
        /// <summary>
        /// MouseEvent 模拟鼠标点击
        /// </summary>       
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void MouseEvent(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);

        //在(34, 258)处点击鼠标
        //SetCursorPos(34, 258);
        //MouseEvent(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        //MouseEvent(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);

        /// <summary>
        /// SetCursorPos 设置光标的绝对位置
        /// </summary>  
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        /// <summary>
        /// KeybdEvent 模拟键盘。已经过期。
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void KeybdEvent2(byte key, byte bScan, KeybdEventFlag flags, uint dwExtraInfo);

        public static void KeybdEvent(byte key, byte bScan, KeybdEventFlag flags, uint dwExtraInfo)
        {
            INPUT input = new INPUT();
            input.type = 1; //keyboard
            input.ki.wVk = key;
            input.ki.wScan = MapVirtualKey(key, 0);
            input.ki.dwFlags = (int)flags;
            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        [DllImport("user32.dll")]
        private static extern UInt32 SendInput(UInt32 nInputs, ref INPUT pInputs, int cbSize);
        [DllImport("user32.dll")]
        private static extern byte MapVirtualKey(byte wCode, int wMap);
        #endregion

        #region 根据扩展名获取系统图标
        /// <summary>
        /// 根据扩展名获取系统图标。
        /// </summary> 
        /// <param name="fileType">文件类型，使用扩展名表示，如".txt"</param>      
        public static Icon GetSystemIconByFileType(string fileType, bool isLarge)
        {
            if (isLarge)
            {
                return GetIcon(fileType, FILE_ATTRIBUTE.NORMAL, SHGFI.USEFILEATTRIBUTES | SHGFI.ICON | SHGFI.LARGEICON);
            }

            return GetIcon(fileType, FILE_ATTRIBUTE.NORMAL, SHGFI.USEFILEATTRIBUTES | SHGFI.ICON | SHGFI.SMALLICON);
        }

        private static Icon GetIcon(string path, FILE_ATTRIBUTE dwAttr, SHGFI dwFlag)
        {
            SHFILEINFO fi = new SHFILEINFO();
            Icon ic = null;
            int iTotal = (int)SHGetFileInfo(path, dwAttr, ref fi, 0, dwFlag);
            ic = Icon.FromHandle(fi.hIcon);
            return ic;
        }

        #region PInvoke
        [DllImport("shell32", EntryPoint = "SHGetFileInfo", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SHGetFileInfo(string pszPath, FILE_ATTRIBUTE dwFileAttributes, ref SHFILEINFO sfi, int cbFileInfo, SHGFI uFlags);


        // Contains information about a file object
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;    //文件的图标句柄
            public IntPtr iIcon;    //图标的系统索引号
            public uint dwAttributes;   //文件的属性值
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;  //文件的显示名
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;   //文件的类型名
        };

        // Flags that specify the file information to retrieve with SHGetFileInfo
        [Flags]
        public enum SHGFI : uint
        {
            ADDOVERLAYS = 0x20,
            ATTR_SPECIFIED = 0x20000,
            ATTRIBUTES = 0x800,   //获得属性
            DISPLAYNAME = 0x200,  //获得显示名
            EXETYPE = 0x2000,
            ICON = 0x100,    //获得图标
            ICONLOCATION = 0x1000,
            LARGEICON = 0,    //获得大图标
            LINKOVERLAY = 0x8000,
            OPENICON = 2,
            OVERLAYINDEX = 0x40,
            PIDL = 8,
            SELECTED = 0x10000,
            SHELLICONSIZE = 4,
            SMALLICON = 1,    //获得小图标
            SYSICONINDEX = 0x4000,
            TYPENAME = 0x400,   //获得类型名
            USEFILEATTRIBUTES = 0x10
        }
        // Flags that specify the file information to retrieve with SHGetFileInfo
        [Flags]
        public enum FILE_ATTRIBUTE
        {
            READONLY = 0x00000001,
            HIDDEN = 0x00000002,
            SYSTEM = 0x00000004,
            DIRECTORY = 0x00000010,
            ARCHIVE = 0x00000020,
            DEVICE = 0x00000040,
            NORMAL = 0x00000080,
            TEMPORARY = 0x00000100,
            SPARSE_FILE = 0x00000200,
            REPARSE_POINT = 0x00000400,
            COMPRESSED = 0x00000800,
            OFFLINE = 0x00001000,
            NOT_CONTENT_INDEXED = 0x00002000,
            ENCRYPTED = 0x00004000
        }
        #endregion
        #endregion

        #region 开机自动启动
        /// <summary> 
        /// 开机自动启动,使用注册表 
        /// </summary> 
        /// <param name=\"Started\">是否开机自动启动</param> 
        /// <param name=\"name\">取一个唯一的注册表Key名称</param> 
        /// <param name=\"path\">启动程序的完整路径</param> 
        public static void RunWhenStart_usingReg(bool started, string name, string path)
        {
            RegistryKey HKLM = Registry.LocalMachine;
            try
            {
                RegistryKey run = HKLM.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                if (started)
                {
                    run.SetValue(name, path);
                }
                else
                {
                    object val = run.GetValue(name);
                    if (val != null)
                    {
                        run.DeleteValue(name);
                    }
                }
            }
            finally
            {
                HKLM.Close();
            }
        }
        #endregion

        #region CaptureScreenImage
        public static Image CaptureScreenImage()
        {
            //可以抓悬浮窗等LayeredWindow    
            Size sz = Screen.PrimaryScreen.Bounds.Size;
            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrce = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);
            bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            Bitmap bmp = Bitmap.FromHbitmap(hBmp);
            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);
            return bmp;
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr ptr);
        #endregion

        #region SetForegroundWindow
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetActiveWindow(IntPtr hWnd);
        /// <summary>
        /// 设置目标窗体为活动窗体。将其TopLevel在最前。
        /// </summary>     
        public static void SetForegroundWindow(Form window)
        {
            SetActiveWindow(window.Handle);
        }
        #endregion

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #region 获取CPU使用率

        #region AIP声明
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        public static float? GetCpuUsedRate()
        {
            try
            {
                PerformanceCounter pcCpuLoad;
                pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total")
                {
                    MachineName = "."
                };
                pcCpuLoad.NextValue();
                System.Threading.Thread.Sleep(1500);
                float CpuLoad = pcCpuLoad.NextValue();

                return CpuLoad;
            }
            catch
            {
            }
            return 0;
        }
        #endregion

        #region 获取内存使用率
        #region 可用内存

        /// <summary>
        ///     获取可用内存
        /// </summary>
        internal static long? GetMemoryAvailable()
        {

            long availablebytes = 0;
            var managementClassOs = new ManagementClass("Win32_OperatingSystem");
            foreach (var managementBaseObject in managementClassOs.GetInstances())
                if (managementBaseObject["FreePhysicalMemory"] != null)
                    availablebytes = 1024 * long.Parse(managementBaseObject["FreePhysicalMemory"].ToString());
            return availablebytes / MbDiv;

        }

        #endregion
        internal static double? GetMemoryUsed()
        {
            float? PhysicalMemory = GetPhysicalMemory();
            float? MemoryAvailable = GetMemoryAvailable();
            double? MemoryUsed = (double?)(PhysicalMemory - MemoryAvailable);
            double currentMemoryUsed = (double)MemoryUsed;
            return currentMemoryUsed;
        }
        private static long? GetPhysicalMemory()
        {
            //获得物理内存
            var managementClass = new ManagementClass("Win32_ComputerSystem");
            var managementObjectCollection = managementClass.GetInstances();
            long PhysicalMemory;
            foreach (var managementBaseObject in managementObjectCollection)
                if (managementBaseObject["TotalPhysicalMemory"] != null)
                {
                    return long.Parse(managementBaseObject["TotalPhysicalMemory"].ToString()) / MbDiv;
                }
            return null;

        }
        public static double? GetMemoryUsedRate()
        {
            float? PhysicalMemory = GetPhysicalMemory();
            float? MemoryAvailable = GetMemoryAvailable();
            double? MemoryUsedRate = (double?)(PhysicalMemory - MemoryAvailable) / PhysicalMemory;
            return MemoryUsedRate.HasValue ? Convert.ToDouble(MemoryUsedRate * 100) : 0;
        }
        #endregion

        #region 单位转换进制

        private const int KbDiv = 1024;
        private const int MbDiv = 1024 * 1024;
        private const int GbDiv = 1024 * 1024 * 1024;

        #endregion

        #region 获取磁盘占用率
        internal static double GetUsedDiskPercent()
        {
            float? usedSize = GetUsedDiskSize();
            float? totalSize = GetTotalSize();
            double? percent = (double?)usedSize / totalSize;
            return percent.HasValue ? Convert.ToDouble(percent * 100) : 0;
        }

        internal static float? GetUsedDiskSize()
        {
            var currentDrive = GetCurrentDrive();
            float UsedDiskSize = 0;
            if(currentDrive != null)
            {
                UsedDiskSize =(long)currentDrive.TotalSize - (long)currentDrive.TotalFreeSpace;
            }
            return UsedDiskSize / MbDiv;
        }

        internal static float? GetTotalSize()
        {
            var currentDrive = GetCurrentDrive();
            float TotalSize = 0;
            if(currentDrive != null)
            {
                TotalSize = (long)currentDrive.TotalSize / MbDiv;
            }

            return TotalSize;
        }

        /// <summary>
        /// 获取当前执行的盘符信息
        /// </summary>
        /// <returns></returns>
        private static DriveInfo GetCurrentDrive()
        {
            string path = Application.StartupPath.ToString().Substring(0, 3);
            return DriveInfo.GetDrives().FirstOrDefault<DriveInfo>(p => p.Name.Equals(path));
        }
        #endregion
    }

    #region MouseEventFlag ,KeybdEventFlag
    /// <summary>
    /// MouseEventFlag 模拟鼠标点击的相关标志。
    /// </summary>
    [Flags]
    public enum MouseEventFlag : uint
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        XDown = 0x0080,
        XUp = 0x0100,
        Wheel = 0x0800,
        VirtualDesk = 0x4000,
        Absolute = 0x8000 //绝对坐标
    }

    /// <summary>
    /// KeybdEventFlag 模拟键盘点击的相关标志。
    /// </summary>
    [Flags]
    public enum KeybdEventFlag : uint
    {
        Down = 0,
        Up = 0x0002
    }
    #endregion

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public Int32 type;
        [FieldOffset(4)]
        public KEYBDINPUT ki;
        [FieldOffset(4)]
        public MOUSEINPUT mi;
        [FieldOffset(4)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public Int32 dx;
        public Int32 dy;
        public Int32 mouseData;
        public Int32 dwFlags;
        public Int32 time;
        public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public Int16 wVk;
        public Int16 wScan;
        public Int32 dwFlags;
        public Int32 time;
        public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public Int32 uMsg;
        public Int16 wParamL;
        public Int16 wParamH;
    }
}

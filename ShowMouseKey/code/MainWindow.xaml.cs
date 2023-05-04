using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices;

// http://blogs.msdn.com/b/toub/archive/2006/05/03/589468.aspx
// http://blogs.msdn.com/b/toub/archive/2006/05/03/589423.aspx

namespace ShowMouseKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static private MainWindow _singleton = null;

        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int vkShiftLeft = 160;
        private const int vkShiftRight = 161;
        private const int vkCtrlLeft = 162;
        private const int vkCtrlRight = 163;
        private const int vkAltLeft = 164;
        private const int vkAltRight = 165;

        private const int vkKey0 = 0x30;
        private const int vkKey9 = 0x3A;
        private const int vkKeyA = 0x41;
        private const int vkKeyZ = 0x5A;

        private const int vkKeyTab = 9;
        private const int vkKeyColon = 186;
        private const int vkKeyEquals = 187;
        private const int vkKeyComma = 188;
        private const int vkKeySubtract = 189;
        private const int vkKeyPeriod = 190;
        private const int vkKeySlash = 191;
        private const int vkKeyTilde = 192;
        private const int vkKeyOpenBracket = 219;
        private const int vkKeyOemPipe = 220;
        private const int vkKeyCloseBracket = 221;
        private const int vkKeyApostrophe = 222;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
        }

        private static LowLevelMouseProc _mouseProc = MouseHookCallback;
        private static IntPtr _mouseHookID = IntPtr.Zero;

        private static LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;
        private static IntPtr _keyboardHookID = IntPtr.Zero;

        private String _mouseLeftUp = "L0";
        private String _mouseLeftDown = "L1";
        private String _mouseMiddleUp = "M0";
        private String _mouseMiddleDown = "M1";
        private String _mouseRightUp = "R0";
        private String _mouseRightDown = "R1";

        private String _mouseLeft;
        private String _mouseMiddle;
        private String _mouseRight;

        private List<int> _modifiersDown = null;
        private List<int> _keysDown = null;

        List<Image> _keyImages = null;

        public MainWindow()
        {

            InitializeComponent();
            _singleton = this;

            this.Topmost = true;

            _mouseHookID = SetMouseHook(_mouseProc);

            _mouseLeft = _mouseLeftUp;
            _mouseMiddle = _mouseMiddleUp;
            _mouseRight = _mouseRightUp;

            _keyboardHookID = SetKeyboardHook(_keyboardProc);

            _modifiersDown = new List<int>();
            _keysDown = new List<int>();

            _keyImages = new List<Image>();
            _keyImages.Add(boxKey1);
            _keyImages.Add(boxKey2);
            _keyImages.Add(boxKey3);
            _keyImages.Add(boxKey4);
            _keyImages.Add(boxKey5);
            _keyImages.Add(boxKey6);
            _keyImages.Add(boxKey7);
            _keyImages.Add(boxKey8);
            _keyImages.Add(boxKey9);

            foreach (Image img in _keyImages)
            {
                img.Source = null;
            }

            //Image img = new Image();
            //img.Source = new BitmapImage(new Uri( "pack://application:,,,/Images/KeyShift.png" ));
        }

        private static IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                _singleton.UpdateMouseUI(nCode, wParam, lParam);
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                //Console.WriteLine("{0}", vkCode);
                _singleton.UpdateKeyboardUI(nCode, wParam, lParam);
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private void UpdateMouseUI(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int mm = (int)wParam;
            //Console.WriteLine("Mouse: " + mm);

            bool updateMouse = false;

            if (nCode >= 0)
            {
                if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                {
                    _mouseLeft = _mouseLeftDown;
                    updateMouse = true;
                }
                else if (MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                {
                    _mouseLeft = _mouseLeftUp;
                    updateMouse = true;
                }
                else if (MouseMessages.WM_MBUTTONDOWN == (MouseMessages)wParam)
                {
                    _mouseMiddle = _mouseMiddleDown;
                    updateMouse = true;
                }
                else if (MouseMessages.WM_MBUTTONUP == (MouseMessages)wParam)
                {
                    _mouseMiddle = _mouseMiddleUp;
                    updateMouse = true;
                }
                else if (MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
                {
                    _mouseRight = _mouseRightDown;
                    updateMouse = true;
                }
                else if (MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
                {
                    _mouseRight = _mouseRightUp;
                    updateMouse = true;
                }
            }

            if (updateMouse)
            {
                String mouseImage = "pack://application:,,,/Images/Mouse_" + _mouseLeft + _mouseMiddle + _mouseRight + ".png";
                MouseImage.Source = new BitmapImage(new Uri(mouseImage));
            }
        }

        private void UpdateKeyboardUI(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_SYSKEYDOWN || wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (vkCode == vkShiftLeft || vkCode == vkShiftRight)
                    {
                        PushModifierDown(vkShiftLeft);
                    }
                    else if (vkCode == vkCtrlLeft || vkCode == vkCtrlRight)
                    {
                        PushModifierDown(vkCtrlLeft);
                    }
                    else if (vkCode == vkAltLeft || vkCode == vkAltRight)
                    {
                        PushModifierDown(vkAltLeft);
                    }
                    else
                    {
                        PushKeyDown(vkCode);
                    }
                }
                if (wParam == (IntPtr)WM_SYSKEYUP || wParam == (IntPtr)WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (vkCode == vkShiftLeft || vkCode == vkShiftRight)
                    {
                        PopModifierDown(vkShiftLeft);
                    }
                    else if (vkCode == vkCtrlLeft || vkCode == vkCtrlRight)
                    {
                        PopModifierDown(vkCtrlLeft);
                    }
                    else if (vkCode == vkAltLeft || vkCode == vkAltRight)
                    {
                        PopModifierDown(vkAltLeft);
                    }
                    else
                    {
                        PopKeyDown(vkCode);
                    }
                }

                int i_img = 0;
                for (int i = 0; i < _modifiersDown.Count; ++i)
                {
                    if (i_img < _keyImages.Count)
                    {
                        int keyDown = _modifiersDown[i];
                        SetImage(_keyImages[i_img], KeyToString(keyDown));
                        i_img++;
                    }
                }
                for (int i = 0; i < _keysDown.Count; ++i)
                {
                    if (i_img < _keyImages.Count)
                    {
                        int keyDown = _keysDown[i];
                        SetImage(_keyImages[i_img], KeyToString(keyDown));
                        i_img++;
                    }
                }
                for (; i_img < _keyImages.Count; ++i_img)
                {
                    _keyImages[i_img].Source = null;
                }
            }
        }

        public String KeyToString(int key)
        {
            if (key >= vkKeyA && key <= vkKeyZ)
            {
                int charBase = 'A';
                int charJump = (char)(key - vkKeyA);
                int charKey = charBase + charJump;
                string uri = "pack://application:,,,/Images/KeyLetter";
                uri += (char)charKey;
                uri += ".png";
                return uri;
            }

            if (key >= vkKey0 && key <= vkKey9)
            {
                int charBase = '0';
                int charJump = (char)(key - vkKey0);
                int charKey = charBase + charJump;
                string uri = "pack://application:,,,/Images/KeyLetter";
                uri += (char)charKey;
                uri += ".png";
                return uri;
            }

            switch (key)
            {
                case vkShiftLeft:
                    return "pack://application:,,,/Images/KeyShift.png";
                case vkCtrlLeft:
                    return "pack://application:,,,/Images/KeyCtrl.png";
                case vkAltLeft:
                    return "pack://application:,,,/Images/KeyAlt.png";
                case vkKeyApostrophe:
                    return "pack://application:,,,/Images/KeyApostrophe.png";
                case vkKeySubtract:
                    return "pack://application:,,,/Images/KeySubtract.png";
                case vkKeyEquals:
                    return "pack://application:,,,/Images/KeyEquals.png";
                case vkKeyTilde:
                    return "pack://application:,,,/Images/KeyTilde.png";
                case vkKeyOpenBracket:
                    return "pack://application:,,,/Images/KeyOpenBracket.png";
                case vkKeyCloseBracket:
                    return "pack://application:,,,/Images/KeyCloseBracket.png";
                case vkKeyOemPipe:
                    return "pack://application:,,,/Images/KeyOemPipe.png";
            }

            return null;
        }

        public void SetImage(Image img, String uriPath)
        {
            if (uriPath == null)
                return;
            img.Source = new BitmapImage(new Uri(uriPath));
            RenderOptions.SetBitmapScalingMode(img, System.Windows.Media.BitmapScalingMode.Fant);
            float aspect = (float)img.Source.Width / (float)img.Source.Height;
            img.Width = 32 * aspect;
            img.Height = 32;
            img.Stretch = System.Windows.Media.Stretch.Fill;
        }

        void PushKeyDown(int key)
        {
            foreach (var k in _keysDown)
            {
                if (key == k)
                {
                    return;
                }
            }
            _keysDown.Add(key);
        }

        void PopKeyDown(int key)
        {
            foreach (var k in _keysDown)
            {
                if (key == k)
                {
                    _keysDown.Remove(key);
                    return;
                }
            }
        }

        void PushModifierDown(int mod)
        {
            foreach (var m in _modifiersDown)
            {
                if (mod == m)
                {
                    return;
                }
            }
            //Console.WriteLine("Add Modifier: {0}", mod);
            _modifiersDown.Add(mod);
        }

        void PopModifierDown(int mod)
        {
            foreach (var m in _modifiersDown)
            {
                if (mod == m)
                {
                    //Console.WriteLine("Remove Modifier: {0}", mod);
                    _modifiersDown.Remove(mod);
                    return;
                }
            }
        }

        private bool _movingWindow = false;
        private int _holdPosX = 0;
        private int _holdPosY = 0;
        private int _mouse0X = 0;
        private int _mouse0Y = 0;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            _movingWindow = true;
            Point ms = PointToScreen(e.GetPosition(this));
            _mouse0X = (int)ms.X;
            _mouse0Y = (int)ms.Y;
            _holdPosX = (int)base.Left;
            _holdPosY = (int)base.Top;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_movingWindow == true)
            {
                Point ms = PointToScreen(e.GetPosition(this));
                int mouse1X = (int)ms.X;
                int mouse1Y = (int)ms.Y;
                base.Left = _holdPosX + mouse1X - _mouse0X;
                base.Top = _holdPosY + mouse1Y - _mouse0Y;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _movingWindow = false;
            Mouse.Capture(null);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

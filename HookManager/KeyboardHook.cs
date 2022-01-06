using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;

namespace HookManager
{
    /// <summary>
    /// 键盘钩子
    /// </summary>
    public class KeyboardHook:HookBase
    {
        //键盘事件  
        public event KeyEventHandler OnKeyUp;
        public event KeyEventHandler OnKeyDown;
        private bool iskeydown = false;
        private Keys lastkeydownKeys = Keys.None;

        #region 重写基类
        protected override int HookID=> WH_KEYBOARD_LL;
        internal override int HookProcEvent(int nCode, Int32 wParam, IntPtr lParam)
        {
            //如果正常运行并且用户要监听键盘的消息  
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyEventArgs keyEventArgs = new KeyEventArgs(vkCode.ToString().ToEnum<Keys>());
                switch (wParam)
                {
                    case WM_HOTKEY:
                       // log.Add("WM_HOTKEY:" + vkCode.ToString().ToEnum<Keys>().ToString());
                        break;
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        // OnKeyDown?.Invoke(this, keyEventArgs);
                        if (!iskeydown || keyEventArgs.KeyCode != lastkeydownKeys)
                        {
                            OnKeyDown?.Invoke(this, keyEventArgs);
                        }
                        else 
                        {
                            if (iskeydown && keyEventArgs.KeyCode == lastkeydownKeys)
                            {
                                 Keys[] keys = { Keys.LControlKey, Keys.RControlKey, Keys.ControlKey, Keys.LShiftKey, Keys.RShiftKey, Keys.ShiftKey, Keys.LMenu, Keys.RMenu, Keys.Alt, };
                                if (!keys.ToList().Contains(keyEventArgs.KeyCode))
                                {
                                    OnKeyDown?.Invoke(this, keyEventArgs);
                                }
                            }
                        }
                        iskeydown = true;
                        lastkeydownKeys = keyEventArgs.KeyCode;
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                      
                        OnKeyUp?.Invoke(this, keyEventArgs);
                        iskeydown = false;
                        lastkeydownKeys = Keys.None;
                        break;
                }
            }
            return CallNextHookEx(hwndHook, nCode, wParam, lParam);
        }
        #endregion
    }
}

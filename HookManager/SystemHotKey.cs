using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace HookManager
{
    /// <summary>
    /// 系统热键注册对象
    /// </summary>
    public class SystemHotKey : NativeWindow
    {
        /// <summary>
        /// 如果函数执行成功，返回值不为0。
        /// 如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hWnd">要取消热键的窗口的句柄</param>
        /// <param name="id">要取消热键的ID</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        /// <summary>
        /// 辅助键名称。
        /// Alt, Ctrl, Shift, WindowsKey
        /// </summary>
        [Flags()]
        public enum KeyModifiers { None = 0, Alt = 1, Ctrl = 2, Shift = 4, WindowsKey = 8 }

        #region private
        private const int WM_HOTKEY = 0x0312;// 热键消息
        private IntPtr _hwnd;
        private Dictionary<int, Action> hotkeydictionary = new Dictionary<int, Action>();

        /// <summary>
        ///析构
        /// </summary>
        ~SystemHotKey()
        {
            if (hotkeydictionary.Count > 0)
            {
                for (int i = hotkeydictionary.Count - 1; i >= 0; i--)
                {
                    UnRegHotKey(hotkeydictionary.ToArray()[i].Key);
                }
            }
            ReleaseHandle();
        }
        #endregion

        #region public
        /// <summary>
        ///初始化
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        public SystemHotKey(IntPtr hwnd)
        {
            _hwnd = hwnd;
            this.AssignHandle(_hwnd);
        }
        /// <summary>
        /// 添加热键
        /// </summary>
        /// <param name="hotKeyId"></param>
        /// <param name="keyModifiers"></param>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public void AddHotKey(int hotKeyId, KeyModifiers keyModifiers, Keys key, Action action)
        {
            if (_hwnd != IntPtr.Zero)
            {
                if (!hotkeydictionary.Keys.Contains(hotKeyId))
                {
                    RegHotKey(_hwnd, hotKeyId, keyModifiers, key);
                    hotkeydictionary.Add(hotKeyId, action);
                }
            }
        }
        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hotKeyId"></param>
        /// <param name="keyModifiers"></param>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public void UnRegHotKey(int hotKeyId)
        {
            if (_hwnd != IntPtr.Zero)
            {
                UnRegHotKey(_hwnd, hotKeyId);

                if (hotkeydictionary.Keys.Contains(hotKeyId))
                {
                    hotkeydictionary.Remove(hotKeyId);
                }
            }
        }
        #endregion

        #region protected
        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="keyModifiers">组合键</param>
        /// <param name="key">热键</param>
        protected void RegHotKey(IntPtr hwnd, int hotKeyId, KeyModifiers keyModifiers, Keys key)
        {
            if (!RegisterHotKey(hwnd, hotKeyId, keyModifiers, key))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 1409)
                {
                    throw new Exception("RegHotKey failed,HotKey has  be Occupied ");
                }
                else
                {
                    throw new Exception("RegHotKey failed,error code: " + errorCode);
                }
            }
        }
        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        protected void UnRegHotKey(IntPtr hwnd, int hotKeyId)
        {
            //注销指定的热键
            UnregisterHotKey(hwnd, hotKeyId);
        }
        #region 消息处理
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //热键消息
                case WM_HOTKEY:
                    {
                        int KeyInfo = m.WParam.ToInt32();
                        if (hotkeydictionary.Count > 0)
                        {
                            hotkeydictionary[KeyInfo]?.Invoke();
                        }
                        break;
                    }
            }
            base.WndProc(ref m);
        }
        #endregion
        #endregion
    }
}

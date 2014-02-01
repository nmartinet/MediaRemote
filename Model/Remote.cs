using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace MediaRemote.Model {
  class Remote {

    #region attributes
    private List<User32Window> _desktop_windows;
    public int[] shortcut { get; set; }

    private CancellationTokenSource _tokenSource;
    private CancellationToken _token;
    private bool _running;

    //convert list of structs to list of
    //classes to work for populating gui lsits
    public List<User32Window> desktop_windows {
      get {
        return _desktop_windows;
      }    
    }
    public IntPtr selected_window_hWnd { get; set; }

    public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    public string textFilter = string.Empty;
    public string typeFIlter = string.Empty;
  
    #endregion
    #region Constants


    #endregion
    #region Structs

    //we don't need it but msgs need points
    [SerializableAttribute]
    public struct POINT {
      public int X;
      public int Y;
    }

    [SerializableAttribute]
    public struct MSG {
      public IntPtr hwnd;
      public UInt32 message;
      public IntPtr wParam;
      public IntPtr lParam;
      public UInt32 time;
      public POINT pt;
    }

    public struct User32Window {
      public IntPtr hWnd;
      public string text;
      public string type;
    }

    public struct User32ChildWindow {
      public IntPtr hWnd;
      public string type;
    }

    #endregion
    #region DLL Imports
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int EnumChildWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int RegisterHotKey(IntPtr hDesktop, int id, uint modifier, uint key);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int UnregisterHotKey(IntPtr hDesktop, int id);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
    #endregion
    #region DLL Methods
    public List<User32Window> get_windows() {
      var windows_list = new List<User32Window>();

      EnumDelegate enum_desktop_windows_callback = delegate(IntPtr hWnd, int lParam) {
        StringBuilder string_builder = new StringBuilder(255);
        User32Window win = new User32Window();

        win.hWnd = hWnd;
        GetWindowText(hWnd, string_builder, string_builder.Capacity + 1);
        win.text = string_builder.ToString();

        GetClassName(hWnd, string_builder, string_builder.Capacity + 1);
        win.type = string_builder.ToString();

        windows_list.Add(win);
        return true;
      };
      EnumDesktopWindows(IntPtr.Zero, enum_desktop_windows_callback, IntPtr.Zero);
      return windows_list;
    }

    private List<User32ChildWindow> get_child_windows(IntPtr top_window_pointer) {
      var child_windows_list = new List<User32ChildWindow>();

      EnumDelegate enum_child_windows_callback = delegate(IntPtr hWnd, int lParam) {
        StringBuilder string_builder = new StringBuilder(255);
        User32ChildWindow child = new User32ChildWindow();

        child.hWnd = hWnd;
        GetClassName(hWnd, string_builder, string_builder.Capacity + 1);

        child.type = string_builder.ToString();
        child_windows_list.Add(child);

        return true;
      };

      EnumChildWindows(top_window_pointer, enum_child_windows_callback, IntPtr.Zero);

      return child_windows_list;
    }

    #endregion
    #region internal methods

    public void update_windows_list() {
      var windows = get_windows();

      if(textFilter != string.Empty)
        windows = windows.Where(w => w.text.Contains(textFilter)).ToList();

      if(typeFIlter != string.Empty)
        windows = windows.Where(w => w.type.Contains(typeFIlter)).ToList();

      _desktop_windows = windows;
    }


    #endregion
    #region public methods
    
    public Remote() {
      _running = false;
      update_windows_list(); 
    }

    #endregion

    public void ToggleShortcut(){
      if(!_running) {
        _running = true;
        _tokenSource = new CancellationTokenSource();
        _token = _tokenSource.Token;
       Task.Factory.StartNew(() => ShortcutLoop(_token), _token);

      } else {
        _tokenSource.Cancel();
        _running = false;

        
      }
    }

    private void ShortcutLoop(CancellationToken ct) { 
       RegisterHotKey(IntPtr.Zero, 1, (uint)shortcut[0], (uint)shortcut[1]);
       bool running = true;
       while(running) {
         MSG msg;
         int retVal = PeekMessage(out msg, IntPtr.Zero, 0, 0, 1);
         if(retVal != 0) {
           System.Threading.Thread.Sleep(500);

           PostMessage(selected_window_hWnd, 256, 0x20, 1);
           PostMessage(selected_window_hWnd, 257, 0x20, 1);
         }
         if(ct.IsCancellationRequested) {
           UnregisterHotKey(IntPtr.Zero, 1);
           running = false;
         }
       }
    }




  }
}

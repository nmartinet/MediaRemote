using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaRemote.Model {

  class ShortcutSelector {
    private class KeyComparer : IComparer<int> {
      public int Compare(int x, int y) {
        int tmpx = subValue(x);
        int tmpy = subValue(y);
        return tmpx.CompareTo(tmpy);
      }
      private int subValue(int x) {
        switch(x) {
          case 17: return -4;
          case 18: return -3;
          case 91: return -2;
          case 16: return -1;
          default: return x;
        }
      
      }
    }
    enum KeyMods {
      Ctrl = 17,
      LeftCtrl = 162,
      RightCtrl = 163,

      Alt = 18,
      LeftAlt = 164,
      RightAlt = 165,

      Shift = 16,
      LeftShift = 160,
      RightShift = 161,

      Win = 91,

      LeftClick = 1,
      RightClick = 2,
      MidClick = 4

    }
    enum IgnoreKeys {
      LeftCtrl = 162,
      RightCtrl = 163,
      LeftAlt = 164,
      RightAlt = 165,
      LeftShift = 160,
      RightShift = 161,
      LeftClick = 1,
      RightClick = 2,
      MidClick = 4
    }



    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    static extern short GetKeyState(int lpKeyState);

    public List<int> GetShortcut() {
      bool running = true;
      List<int> res = new List<int>();

      while(running) {
        res.Clear();
        var array = new byte[256];
        GetKeyState(0);
        GetKeyboardState(array);
        for(int i = 0; i < 256; i++)
          if((array[i] & 0x80) != 0)
            res.Add(i);

        if(res.Count > 0)
          foreach( var k in res )
            if(!(Enum.IsDefined(typeof(KeyMods), k )))
              running = false;

      };
      foreach(int v in Enum.GetValues(typeof(IgnoreKeys)))
        res.Remove(v);

      return res;
    }
    public string KeyCodeToName(int keyCode) {
      return ((System.Windows.Forms.Keys)keyCode).ToString();
    }
    public string KeyCodeToCleanName(int keyCode) {
      string res = string.Empty;
      switch(keyCode) {
        case 16:
          res = "Shift";
          break;
        case 17:
          res = "Ctrl";
          break;
        case 18:
          res = "Alt";
          break;
        case 91:
          res = "Win";
          break;
        default:
          res = ((System.Windows.Forms.Keys)keyCode).ToString();
          break;
      }
      return res;
    }
    public string ShortcutString(List<int> keyCodes) {
      string res = string.Empty;
      keyCodes.Sort(new KeyComparer());
      res = keyCodes.Select(k => KeyCodeToCleanName(k)).Aggregate<string>((a, b) => a + "+" + b);

      return res;    
    }

    public int[] RegisterHotkeyValue(List<int> keyCodes) {
      int[] res = new int[2];

      foreach(int k in keyCodes) {



      }

      return res;
    }
  }
}






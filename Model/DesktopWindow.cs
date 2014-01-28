using System;
using System.ComponentModel;

namespace MediaRemote.Model {
  class DesktopWindow : INotifyPropertyChanged {
    IntPtr _hWnd;
    public IntPtr hWnd {
      get { return _hWnd; }
      set {
        if (_hWnd != value) {
          _hWnd = value;
          RaisePropertyChanged("hWnd");
        }
      }
    }
    string _text;
    public string text {
      get { return _text; }
      set {
        if (_text != value) {
          _text = value;
          RaisePropertyChanged("text");
        }
      }
    }
    string _type;
    public string type {
      get { return _type; }
      set {
        if (_type != value) {
          _type = value;
          RaisePropertyChanged("type");
        }
      }
    }

    void RaisePropertyChanged(string prop) {
      if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
    }
    public event PropertyChangedEventHandler PropertyChanged;
  }
}

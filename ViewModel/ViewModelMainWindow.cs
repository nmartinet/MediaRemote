using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MediaRemote.Model;
using MediaRemote.Helpers;
using System.Diagnostics;
using System.Windows.Forms;

using System.Threading;
using System.Threading.Tasks;


namespace MediaRemote.ViewModel {
  class ViewModelMainWindow : ViewModelBase {

    #region Properties
    private Remote _rmt;

    private ObservableCollection<DesktopWindow> _desktopWindows;
    public ObservableCollection<DesktopWindow> desktopWindows {
      get { return _desktopWindows; }
      set {
        if(value != _desktopWindows) {
          _desktopWindows = value;
          RaisePropertyChanged("desktopWindows");
        }
      }
    }

    private DesktopWindow _selectedItem { get; set; }
    public DesktopWindow selectedItem {
      get { return _selectedItem; }
      set {
        if(value != _selectedItem ) {
          _selectedItem = value;
          RaisePropertyChanged("selectedItemIndex");
        }
      }
    }
    
    private ShortcutSelector _shSelector;

    private string _ToggleButtonValue { get; set;  }
    public string ToggleButtonValue {
      get { return _ToggleButtonValue; }
      set{
        if(value != _ToggleButtonValue) {
          _ToggleButtonValue = value;
          RaisePropertyChanged("ToggleButtonValue");
        }            
      } 
    }

    private bool _running;
    
    private List<int> _shortcutIDs { get; set; }

    private string _shortcutString { get; set; }
    public string shortcutString {
      get { return _shortcutString; }
      set {
        if(value != _shortcutString) {
          _shortcutString = value;
          RaisePropertyChanged("shortcutString");
        }
      }
    }

    private string _shortcutSelectorButtonString { get; set; }
    public string shortcutSelectorButtonString {
      get { return _shortcutSelectorButtonString; }
      set {
        if(value != _shortcutSelectorButtonString ) {
          _shortcutSelectorButtonString = value;
          RaisePropertyChanged("shortcutSelectorButtonString ");
        }
      }
    }
    public Boolean shortcutSelectorButtonState {
      get;
      set;
    }


    public RelayCommand RefreshWindowsListCommand { get; set; }
    public RelayCommand CaptureShortcutCommand { get; set; }
    public RelayCommand ToggleListeningCommand { get; set; }

    #endregion

    public ViewModelMainWindow() {
      _rmt = new Remote();

      _running = false;

      _rmt.textFilter = "Grooveshark";
      _rmt.typeFIlter = "Mozilla";

      _rmt.update_windows_list();

      _shSelector = new ShortcutSelector();

      _shortcutString = "None selected";
      _shortcutSelectorButtonString = "Capture Shortcut";
      _ToggleButtonValue = "Run";
      shortcutSelectorButtonState = true;

      RefreshWindowsListCommand = new RelayCommand(RefreshWindowsList);
      CaptureShortcutCommand = new RelayCommand(CpatureShortcut);
      ToggleListeningCommand = new RelayCommand(ToggleListening);

      _desktopWindows = new ObservableCollection<DesktopWindow>();
      UpdateWindowsList();
    }

    private void UpdateWindowsList() {
      _rmt.desktop_windows.ForEach(wnd => _desktopWindows.Add(new DesktopWindow() { hWnd = wnd.hWnd, text = wnd.text, type = wnd.type }));
      desktopWindows = _desktopWindows;
      RaisePropertyChanged("desktopWindows");
    }

    async void getShortcut() {
      _shortcutSelectorButtonString = "Capturing";
      RaisePropertyChanged("shortcutSelectorButtonString");

      shortcutSelectorButtonState = false;
      RaisePropertyChanged("shortcutSelectorButtonState");

      _shortcutIDs = await Task.Run(() => _shSelector.GetShortcut());
      _shortcutString = _shSelector.ShortcutString(_shortcutIDs);
      RaisePropertyChanged("shortcutString");
      _shortcutSelectorButtonString = "Capture Shortcut";
      RaisePropertyChanged("shortcutSelectorButtonString");

      shortcutSelectorButtonState = true;
      RaisePropertyChanged("shortcutSelectorButtonState");

    }

    void CpatureShortcut(object param) {
      getShortcut();
    }

    void ToggleListening(object param) {
      Debug.Print("toggel win");
      if(!_running) {
        _running = true;
        _rmt.selected_window_hWnd = _selectedItem.hWnd;
        _rmt.shortcut = _shSelector.RegisterHotkeyValue(_shortcutIDs);
        _rmt.ToggleShortcut();
        _ToggleButtonValue = "Stop";
        RaisePropertyChanged("ToggleButtonValue");
        
      } else {
        _running = false;
        _rmt.ToggleShortcut();

        _ToggleButtonValue = "Run";
        RaisePropertyChanged("ToggleButtonValue");
      }
    }

    
    void RefreshWindowsList(object param) {
      _rmt.get_windows();
      _desktopWindows = new ObservableCollection<DesktopWindow>();
      _rmt.desktop_windows.ForEach(wnd => _desktopWindows.Add(new DesktopWindow() { hWnd = wnd.hWnd, text = wnd.text, type = wnd.type }));
    }


  }
}

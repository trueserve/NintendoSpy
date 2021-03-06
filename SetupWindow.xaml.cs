﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

using NintendoSpy.Readers;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace NintendoSpy
{
    public partial class SetupWindow : Window
    {
        SetupWindowViewModel _vm;
        DispatcherTimer _portListUpdateTimer;
        List <Skin> _skins;

        public SetupWindow ()
        {
            InitializeComponent ();
            _vm = new SetupWindowViewModel ();
            DataContext = _vm;

            if (! Directory.Exists ("skins")) {
                MessageBox.Show ("Could not find skins folder!", "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
                Close ();
                return;
            }

            var results = Skin.LoadAllSkinsFromParentFolder("skins");
            _skins = results.SkinsLoaded;

            if (results.ParseErrors.Count > 0) {
                showSkinParseErrors (results.ParseErrors);
            }

            var mode = InputSource.DEFAULT;

            if (Properties.Settings.Default.mode.Length > 0) {
                var nmode = InputSource.ALL.Where(x => x.TypeTag == Properties.Settings.Default.mode);
                if (nmode.Count() >= 1) mode = nmode.First();
            }
            
            _vm.Sources.UpdateContents(InputSource.ALL);
            _vm.Sources.SelectedItem = mode;

            _portListUpdateTimer = new DispatcherTimer ();
            _portListUpdateTimer.Interval = TimeSpan.FromSeconds (1);
            _portListUpdateTimer.Tick += (sender, e) => updatePortList(_vm.Sources.SelectedItem.DevicePortType);
            _portListUpdateTimer.Start ();

            _vm.DelayInMilliseconds = Properties.Settings.Default.delay;
        }

        void showSkinParseErrors (List <string> errs) {
            StringBuilder msg = new StringBuilder ();
            msg.AppendLine ("Some skins were unable to be parsed:");
            foreach (var err in errs) msg.AppendLine (err);
            MessageBox.Show (msg.ToString (), "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void updatePortList (int type) {
            switch (type) {
                case 0x00: {
                    MessageBox.Show("updatePortList Failure");
                    break;
                }
                case 0x01: { // serial 
                    _vm.Ports.UpdateContents(SerialPort.GetPortNames());
                    break;
                }
                case 0x04: { // pc gamepad
                    string[] inputnum = new string[8] { "1", "2", "3", "4", "5", "6", "7", "8" };
                    _vm.Ports.UpdateContents(inputnum);
                    break;
                }
                default: { // xinput and dinput 0-3
                    string[] inputnum = new string[4] {"1", "2", "3", "4"};
                    _vm.Ports.UpdateContents(inputnum);
                    break;
                }
            }
        }

        void goButton_Click (object sender, RoutedEventArgs e) 
        {
            // save settings
            Properties.Settings.Default.mode = _vm.Sources.SelectedItem.TypeTag;
            Properties.Settings.Default.skin = _vm.Skins.SelectedItem.Name;
            Properties.Settings.Default.skinbg = _vm.Backgrounds.SelectedItem.Name;
            Properties.Settings.Default.delay = _vm.DelayInMilliseconds;
            Properties.Settings.Default.Save();

            // then proceed
            this.Hide ();

            try {
                var reader = _vm.Sources.SelectedItem.BuildReader(_vm.Ports.SelectedItem);
                if (_vm.DelayInMilliseconds > 0)
                    reader = new DelayedControllerReader(reader, _vm.DelayInMilliseconds);

                new ViewWindow (_vm.Skins.SelectedItem,
                                _vm.Backgrounds.SelectedItem, 
                                reader)
                    .ShowDialog ();
            }
#if DEBUG
            catch (ConfigParseException ex) {
#else
            catch (Exception ex) {
#endif
                MessageBox.Show (ex.Message, "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Show ();
        }

        private void SourceSelectComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            var _selfirst = 1;
            
            if (_vm.Sources.SelectedItem == null) return;
            _vm.Skins.UpdateContents(_skins.Where (x => x.Type == _vm.Sources.SelectedItem));

            if (Properties.Settings.Default.mode == _vm.Sources.SelectedItem.TypeTag) {
                if (Properties.Settings.Default.skin.Length > 0) {
                    var res = _skins.Where(x => x.Name == Properties.Settings.Default.skin).ToList();
                    if (res.Count >= 1) {
                        _vm.Skins.SelectedItem = res[0];
                        _selfirst = 0;
                    }
                }
            }
            
            if (_selfirst > 0) {
                _vm.Skins.SelectFirst();
            }

            _vm.DevicePortOptionVisibility = _vm.Sources.SelectedItem.DevicePortType > 0 ? Visibility.Visible : Visibility.Hidden;
            updatePortList (_vm.Sources.SelectedItem.DevicePortType);
            _vm.Ports.SelectFirst();
        }

        private void Skin_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            var _selfirst = 1;
            
            if (_vm.Skins.SelectedItem == null) return;
            _vm.Backgrounds.UpdateContents(_vm.Skins.SelectedItem.Backgrounds);

            if (Properties.Settings.Default.skin == _vm.Skins.SelectedItem.Name) {
                if (Properties.Settings.Default.skinbg.Length > 0) {
                    var res = _vm.Skins.SelectedItem.Backgrounds.Where(x => x.Name == Properties.Settings.Default.skinbg).ToList();
                    if (res.Count >= 1) {
                        _vm.Backgrounds.SelectedItem = res[0];
                        _selfirst = 0;
                    }
                }
            }

            if (_selfirst > 0) {
                _vm.Backgrounds.SelectFirst();
            }
        }
    }

    public class SetupWindowViewModel : INotifyPropertyChanged
    {
        public class ListView <T>
        {
            List <T> _items;

            public CollectionView Items { get; private set; }
            public T SelectedItem { get; set; }

            public ListView () {
                _items = new List <T> ();
                Items = new CollectionView (_items);
            }

            public void UpdateContents (IEnumerable <T> items) {
                _items.Clear ();
                _items.AddRange (items);
                Items.Refresh ();
            }
            
            public void SelectFirst () {
                if (_items.Count > 0) SelectedItem = _items [0];
            }
        }

        public ListView <string> Ports { get; set; }
        public ListView <Skin> Skins { get; set; }
        public ListView <Skin.Background> Backgrounds { get; set; }
        public ListView <InputSource> Sources { get; set; }
        public int DelayInMilliseconds { get; set; }

        Visibility _devicePortOptionVisibility;
        public Visibility DevicePortOptionVisibility {
            get { return _devicePortOptionVisibility; }
            set {
                _devicePortOptionVisibility = value;
                NotifyPropertyChanged ("DevicePortOptionVisibility");
            }
        }

        public SetupWindowViewModel () {
            Ports   = new ListView <string> ();
            Skins   = new ListView <Skin> ();
            Sources = new ListView <InputSource> ();
            Backgrounds = new ListView <Skin.Background> ();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged (string prop) {
            if (PropertyChanged == null) return;
            PropertyChanged (this, new PropertyChangedEventArgs (prop));
        }
    }
}


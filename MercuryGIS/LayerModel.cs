using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MercuryGIS
{
    public class LayerModel : INotifyPropertyChanged
    {
        #region Data

        bool _isChecked = true;
        Visibility _isVisible = Visibility.Visible;
        Visibility _color_visible = Visibility.Collapsed;
        SolidColorBrush brush = null;

        LayerModel _parent;

        #endregion // Data
        

        public LayerModel(string name)
        {
            this.Name = name;
            this.Children = new List<LayerModel>();
        }

        public LayerModel(string name, Color color, Visibility isVisible = Visibility.Collapsed, Visibility color_visible = Visibility.Visible)
        {
            this.Name = name;
            this._isVisible = isVisible;
            this._color_visible = color_visible;
            this.brush = new SolidColorBrush(color);
            this.Children = new List<LayerModel>();
        }

        void Initialize()
        {
            foreach (LayerModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #region Properties

        public List<LayerModel> Children { get; private set; }

        public string Name { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value); }
        }

        public Visibility IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        public Visibility Color_IsVisible
        {
            get { return _color_visible; }
            set { _color_visible = value; }
        }

        public Brush Brush
        {
            get { return brush; }
        }

        void SetIsChecked(bool value)
        {
            if (value == _isChecked)
                return;
            _isChecked = value;
            this.OnPropertyChanged("IsChecked");
        }

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
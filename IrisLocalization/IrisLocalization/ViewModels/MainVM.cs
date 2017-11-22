using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using IrisLocalization.Models;
using Microsoft.Win32;

namespace IrisLocalization.ViewModels
{
    public class MainVM : ObservableObject
    {
        private ImageModel model;
        private bool isDebug;

        public BitmapImage Image { get; private set; }
        public BitmapImage VerticalProjection { get; private set; }
        public BitmapImage HorizontalProjection { get; private set; }

        public bool IsDebug
        {
            get { return isDebug; }
            set
            {
                isDebug = value;
                RaisePropertyChanged("IsDebug");
                ReloadImage();
            }
        }

        public ICommand Loaded => new RelayCommand(() =>
        {
            try
            {
                LoadImageFromPath(@"E:\Downloads\eye\eye3.jpg");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        });

        public ICommand LoadImage => new RelayCommand(() =>
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (dialog.FileName != null && File.Exists(dialog.FileName))
            {
                try
                {
                   LoadImageFromPath(dialog.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        });

        private void LoadImageFromPath(string path)
        {
            model = new ImageModel(path);
            ReloadImage();
        }

        private void ReloadImage()
        {
            Image = IsDebug ? model.GetDebugImage() : model.GetImageResult();
            VerticalProjection = model.GetVerticalProjection();
            HorizontalProjection = model.GetHorizontalProjection();
            RaisePropertyChanged("Image");
            RaisePropertyChanged("VerticalProjection");
            RaisePropertyChanged("HorizontalProjection");
        }
    }
}

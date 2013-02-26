namespace GrayHills.ForensicToolkit.VideoInspector.ViewModel
{
    using GrayHills.ForensicToolkit.Common;
    using GrayHills.ForensicToolkit.VideoInspector.Model;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.IO;
using System;

    public class FrameViewModel : ViewModelBase<Frame>
    {
        public TimeSpan Time
        {
            get { return Model.Time; }
        }

        public ImageSource ImageSource
        {
            get
            {
                var image = new BitmapImage();

                MemoryStream stream = new MemoryStream(Model.ImageFile.ToArray());
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();

                return image;
            }
        }

        public FrameViewModel(Frame model)
        {
            Model = model;
        }
    }
}

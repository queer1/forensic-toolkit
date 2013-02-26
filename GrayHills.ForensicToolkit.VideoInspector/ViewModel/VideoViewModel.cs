namespace GrayHills.ForensicToolkit.VideoInspector.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using GrayHills.ForensicToolkit.Common;
    using GrayHills.ForensicToolkit.VideoInspector.Model;
    using DirectShowLib;
    using DirectShowLib.DES;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Security.Cryptography;

    public class VideoViewModel : ViewModelBase<Video>, INotifyPropertyChanged
    {
        private WeakReference mediaDetectorWR;
        private int totalFrameCaptures, framesCaptured;
        private ObservableCollection<FrameViewModel> frames;
        
        public ReadOnlyObservableCollection<FrameViewModel> Frames { get; private set; }

        public int TotalFrameCaptures 
        {
            get { return totalFrameCaptures; }
            set
            {
                if (totalFrameCaptures != value)
                {
                    totalFrameCaptures = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TotalFrameCaptures"));
                }
            }
        }

        public int FramesCaptured
        {
            get { return framesCaptured; }
            set
            {
                if (framesCaptured != value)
                {
                    framesCaptured = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("FramesCaptured"));
                }
            }
        }

        public string Filename
        {
            get { return Model.Filename; }
            set { Model.Filename = value; }
        }

        public TimeSpan Length
        {
            get { return Model.Length.GetValueOrDefault(); }
            private set
            {
                if (Model.Length != value)
                {
                    Model.Length = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("Length"));
                }
            }
        }

        private bool isComplete;

        public bool IsComplete
        {
            get { return isComplete; }
            set
            {
                if (isComplete != value)
                {
                    isComplete = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("IsComplete"));
                }
            }
        }

        private void LoadFrames()
        {
            frames = new ObservableCollection<FrameViewModel>(
                            Model.Frames.Select(f => new FrameViewModel(f)));
            Frames = new ReadOnlyObservableCollection<FrameViewModel>(frames);
        }

        private string currentOperation;
        public string CurrentOperation
        {
            get { return currentOperation; }
            set
            {
                if (currentOperation != value)
                {
                    currentOperation = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentOperation"));
                }
            }
        }

        public VideoViewModel(Video model)
        {
            // todo - make user adjustable
            Model = model;
            TotalFrameCaptures = 1;

            CurrentOperation = "Waiting...";

            ThreadPool.QueueUserWorkItem(callback => ExtractFrames());         
        }

        public void ExtractFrames()
        {
            const int interval = 60;
            CurrentOperation = "Loading...";
            string filename = Path.Combine(Model.OriginalPath, Model.Filename);
            var mediaDetector = (IMediaDet)new MediaDet();
            mediaDetector.put_Filename(filename);
            CurrentOperation = "Processing...";
            mediaDetectorWR = new WeakReference(mediaDetector);
            double length;
            mediaDetector.get_StreamLength(out length);
            Length = new TimeSpan(Convert.ToInt64(length) * 10000000);

            TotalFrameCaptures = Convert.ToInt32(
                   Math.Floor(length / interval));

            for (int i = 0; i < TotalFrameCaptures; i++)
            {
                var frame = new Frame()
                {
                    Key = Guid.NewGuid(),
                    ImageFile = ExportFrame((i * interval) + interval),
                    Video = Model,
                    Time = new TimeSpan(0, 0, (i * interval) + interval)
                };

                FramesCaptured++;
            }

            GC.Collect();

            LoadFrames();

            CurrentOperation = "Generating Checksum...";

            byte[] fileBytes = FileHelper.GetBytes(filename);
            var md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(fileBytes);

            Model.Checksum = BitConverter.ToString(checksum).Replace("-", String.Empty);

            CurrentOperation = "Complete!";
            IsComplete = true;

            GC.Collect();
        }

        private byte[] ExportFrame(int time)
        {
            int width = 624, height = 352;
            Bitmap bitmap = null;

            lock (mediaDetectorWR)
            {
                IMediaDet mediaDetector = null;

                if (mediaDetectorWR.Target == null)
                {
                    mediaDetector = LoadMediaDetector();
                }
                else
                {
                    mediaDetector = (IMediaDet)mediaDetectorWR.Target;
                }

                int bufferSize;
                mediaDetector.GetBitmapBits(time, out bufferSize, IntPtr.Zero, width, height);
                IntPtr buffer = IntPtr.Zero;
                buffer = Marshal.AllocCoTaskMem(bufferSize);
                mediaDetector.GetBitmapBits(time, out bufferSize, buffer, width, height);

                BitmapInfoHeader bitmapHeader = (BitmapInfoHeader)Marshal.PtrToStructure(buffer, typeof(BitmapInfoHeader));
                IntPtr bitmapData;

                if (IntPtr.Size == 4)
                    bitmapData = new IntPtr(buffer.ToInt32() + bitmapHeader.Size);
                else
                    bitmapData = new IntPtr(buffer.ToInt64() + bitmapHeader.Size);

                bitmap = new Bitmap(bitmapHeader.Width, bitmapHeader.Height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmapHeader.Width, bitmapHeader.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                CopyMemory(bmpData.Scan0, bitmapData, width * height * 3);
                bitmap.UnlockBits(bmpData);

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            }

            return GetBytes(bitmap);
        }

        private IMediaDet LoadMediaDetector()
        {
            var mediaDetector = (IMediaDet)new MediaDet();
            mediaDetector.put_Filename(Path.Combine(Model.OriginalPath, Model.Filename));
            mediaDetectorWR = new WeakReference(mediaDetector);
            return mediaDetector;
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        private byte[] GetBytes(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(ms, ImageFormat.Jpeg);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}

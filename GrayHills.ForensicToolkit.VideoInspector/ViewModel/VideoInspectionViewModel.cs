namespace GrayHills.ForensicToolkit.VideoInspector.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using GrayHills.ForensicToolkit.Common;
    using GrayHills.ForensicToolkit.Common.ViewModelCommanding;
    using GrayHills.ForensicToolkit.VideoInspector.Model;
    using System.Windows.Forms;
    using System.IO;
    using System.Data.Linq;
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.ComponentModel;

    public class VideoInspectionViewModel : CommandSink, INotifyPropertyChanged
    {
        private ForensicToolkitDBDataContext db;
        public VideoInspection Model { get; private set; }

        public static readonly RoutedCommand AddVideosCommand = new RoutedCommand();
        public static readonly RoutedCommand CommitCommand = new RoutedCommand();

        private ObservableCollection<VideoViewModel> videos;
        public ReadOnlyObservableCollection<VideoViewModel> Videos { get; private set; }

        private VideoViewModel selectedVideo;
        public VideoViewModel SelectedVideo
        {
            get { return selectedVideo; }
            set
            {
                selectedVideo = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SelectedVideo"));
            }
        }

        public VideoInspectionViewModel()
            : this(new VideoInspection())
        {

        }

        public VideoInspectionViewModel(VideoInspection model)
        {
            Model = model;

            if (model.Key == Guid.Empty)
                model.Key = Guid.NewGuid();

            base.RegisterCommand(AddVideosCommand, param => true, param => AddVideos());
            base.RegisterCommand(CommitCommand, param => true, param => Commit());

            db = new ForensicToolkitDBDataContext();
            model.Case = db.Cases.Single();
            db.VideoInspections.InsertOnSubmit(model);
            videos = new ObservableCollection<VideoViewModel>();
            this.Videos = new ReadOnlyObservableCollection<VideoViewModel>(videos);
        }

        public void Commit()
        {
            db.SubmitChanges();
        }

        public void AddVideos()
        {
            DirectoryDialog frmd = new DirectoryDialog();

            frmd.BrowseFor = DirectoryDialog.BrowseForTypes.FilesAndDirectories;
            frmd.Title = "Select a file or a folder";

            if (frmd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string input = frmd.Selected;

                if (File.Exists(input))
                {
                    AddVideo(input);
                }
                else if (Directory.Exists(input))
                {
                    // todo - add setting for recursiveness
                    foreach (string file in Directory.GetFiles(input))
                    {
                        AddVideo(file);
                    }
                }
            }
        }

        private void AddVideo(string filename)
        {
            Video video = new Video()
            {
                Key = Guid.NewGuid(),
                Filename = Path.GetFileName(filename),
                OriginalPath = Path.GetDirectoryName(filename)
            };

            Model.Videos.Add(video);
            videos.Add(new VideoViewModel(video));
        }

        public bool IsComplete
        {
            get { return Model.Ended.HasValue; }
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

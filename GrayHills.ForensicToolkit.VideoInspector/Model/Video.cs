namespace GrayHills.ForensicToolkit.VideoInspector.Model
{
    using DirectShowLib.DES;
    using System.IO;
    using System.Data.Linq;
    using DirectShowLib;

    public partial class Video
    {
        public static bool GetIsValidVideo(string fullyQualifiedFilename)
        {
            bool ret = false;

            if (File.Exists(fullyQualifiedFilename))
            {
                try
                {
                    IMediaDet mdet = (IMediaDet)new MediaDet();
                    int hr = mdet.put_Filename(fullyQualifiedFilename);
                    DsError.ThrowExceptionForHR(hr); // hack, but works
                    ret = true;
                }
                catch { /* do nothing */ }
            }

            return ret;
        }
    }
}

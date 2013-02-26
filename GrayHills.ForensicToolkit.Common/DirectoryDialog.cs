namespace GrayHills.ForensicToolkit.Common
{
    using System;
    using System.Text;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    public class DirectoryDialog
    {
        public struct BROWSEINFO
        {
            public IntPtr hWndOwner;
            public int pIDLRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public int ulFlags;
            public int lpfnCallback;
            public int lParam;
            public int iImage;
        }

        const int MAX_PATH = 260;

        public enum BrowseForTypes : int
        {
            Computers = 4096,
            Directories = 1,
            FilesAndDirectories = 16384,
            UserEditable = 16,
            FileSystemAncestors = 8
        }

        [DllImport("ole32", EntryPoint = "CoTaskMemFree", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int CoTaskMemFree(IntPtr hMem);
        [DllImport("kernel32", EntryPoint = "lstrcat", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr lstrcat(string lpString1, string lpString2);
        [DllImport("shell32", EntryPoint = "SHBrowseForFolder", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);
        [DllImport("shell32", EntryPoint = "SHGetPathFromIDList", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SHGetPathFromIDList(IntPtr pidList, StringBuilder lpBuffer);

        protected bool RunDialog(IntPtr hWndOwner)
        {
            BROWSEINFO udtBI = new BROWSEINFO();
            IntPtr lpIDList = default(IntPtr);
            GCHandle hTitle = GCHandle.Alloc(Title, GCHandleType.Pinned);
            udtBI.hWndOwner = hWndOwner;
            udtBI.lpszTitle = Title;
            udtBI.ulFlags = (int)BrowseFor;
            StringBuilder buffer = new StringBuilder(MAX_PATH);
            buffer.Length = MAX_PATH;
            udtBI.pszDisplayName = buffer.ToString();
            lpIDList = SHBrowseForFolder(ref udtBI);
            hTitle.Free();
            if (lpIDList.ToInt64() != 0)
            {
                if (BrowseFor == BrowseForTypes.Computers)
                {
                    m_Selected = udtBI.pszDisplayName.Trim();
                }
                else
                {
                    StringBuilder path = new StringBuilder(MAX_PATH);
                    SHGetPathFromIDList(lpIDList, path);
                    m_Selected = path.ToString();
                }
                CoTaskMemFree(lpIDList);
            }
            else
            {
                return false;
            }
            return true;
        }

        public DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            IntPtr handle = default(IntPtr);
            if ((owner != null))
            {
                handle = owner.Handle;
            }
            else
            {
                handle = IntPtr.Zero;
            }
            if (RunDialog(handle))
            {
                return DialogResult.OK;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        public string Title
        {
            get { return m_Title; }
            set
            {
                if (object.ReferenceEquals(value, DBNull.Value))
                {
                    throw new ArgumentNullException();
                }
                m_Title = value;
            }
        }

        public string Selected
        {
            get { return m_Selected; }
        }

        public BrowseForTypes BrowseFor
        {
            get { return m_BrowseFor; }
            set { m_BrowseFor = value; }
        }

        private BrowseForTypes m_BrowseFor = BrowseForTypes.Directories;
        private string m_Title = "";
        private string m_Selected = "";

        public DirectoryDialog()
        {
        }
    }
}

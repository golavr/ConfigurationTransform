using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class DeleteFileOnWindowFrameClose : DeleteFileOnClose, IDeleteFileOnWindowFrameClose
    {
        public DeleteFileOnWindowFrameClose(string filePath) : this(filePath, new FileWrapper())
        {
        }

        public DeleteFileOnWindowFrameClose(string filePath, IFileWrapper fileWrapper) : base(filePath, fileWrapper)
        {
        }

        public int OnShow(int fShow)
        {
            var frameshow = (__FRAMESHOW) fShow;
            switch (frameshow)
            {
                case __FRAMESHOW.FRAMESHOW_WinClosed:
                    DeleteFile();
                    break;
            }
            return VSConstants.S_OK;
        }

        public int OnMove()
        {
            return VSConstants.S_OK;
        }

        public int OnSize()
        {
            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable)
        {
            return VSConstants.S_OK;
        }

    }
}
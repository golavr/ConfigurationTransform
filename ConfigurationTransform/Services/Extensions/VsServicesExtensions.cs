using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class VsServicesExtensions
    {
        public static MessageBoxResult ShowMessageBox(this IVsServices source, string message,
                                         OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                         OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
        {
            var result = source.ShowMessageBox("ConfigurationTransform", message, buttons, icon);
            var messageBoxResult = (MessageBoxResult) result;
            return messageBoxResult;
        }

        

        public static bool TryRegisterCloseAndDeleteFile(this IVsWindowFrame source, string path, IDeleteFileOnWindowFrameClose deleteHandler = null)
        {
            // get window with notifications
            var vsWindowFrame = source as IVsWindowFrame2;

            if (vsWindowFrame == null) return false;
            // register to window event, delete file when window close
            var deleteFileOnWindowFrameClose = deleteHandler ?? new DeleteFileOnWindowFrameClose(path);
            uint cookie;
            vsWindowFrame.Advise(deleteFileOnWindowFrameClose, out cookie);
            return true;
        }
    }
}
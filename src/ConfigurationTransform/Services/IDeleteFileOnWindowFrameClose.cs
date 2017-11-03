using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IDeleteFileOnWindowFrameClose : IVsWindowFrameNotify, IDeleteFileOnClose
    {
    }
}
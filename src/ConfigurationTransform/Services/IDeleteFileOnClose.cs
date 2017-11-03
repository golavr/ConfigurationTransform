namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IDeleteFileOnClose
    {
        string FilePath { get; }

        void DeleteFile();
    }
}
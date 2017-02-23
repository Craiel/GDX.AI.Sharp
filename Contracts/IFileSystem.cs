namespace GDX.AI.Sharp.Contracts
{
    using CarbonCore.Utils.IO;

    public interface IFileSystem
    {
        CarbonFile GetFile(string fileName);
    }
}

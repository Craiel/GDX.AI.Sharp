namespace GDX.AI.Sharp
{
    using CarbonCore.Utils.IO;

    using Contracts;

    // TODO: Stub, needs to be implemented properly
    public class StandaloneFileSystem : IFileSystem
    {
        public CarbonFile GetFile(string fileName)
        {
            return new CarbonFile(fileName);
        }
    }
}

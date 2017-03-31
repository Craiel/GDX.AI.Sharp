namespace GDX.AI.Sharp.IO
{
    using System.Collections.Generic;
    using System.IO;

    using CarbonCore.Utils.IO;

    public class DirectoryFileProvider : BaseFileProvider
    {
        private readonly IList<FileStream> openStreams;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DirectoryFileProvider(CarbonDirectory root)
        {
            this.Root = root;
            this.openStreams = new List<FileStream>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public CarbonDirectory Root { get; }

        public override Stream BeginWrite(CarbonFile file)
        {
            CarbonFile fullFile = this.Root.ToFile(file);
            fullFile.GetDirectory().Create();

            var stream = fullFile.OpenCreate();
            this.openStreams.Add(stream);
            return stream;
        }

        public override Stream BeginRead(CarbonFile file)
        {
            CarbonFile fullFile = this.Root.ToFile(file);
            if (fullFile.Exists)
            {
                var stream = fullFile.OpenRead();
                this.openStreams.Add(stream);
                return stream;
            }

            return null;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                foreach (FileStream stream in this.openStreams)
                {
                    stream.Dispose();
                }

                this.openStreams.Clear();
            }
        }
    }
}

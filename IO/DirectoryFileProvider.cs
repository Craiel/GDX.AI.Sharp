using ManagedDirectory = Craiel.UnityEssentials.IO.ManagedDirectory;
using ManagedFile = Craiel.UnityEssentials.IO.ManagedFile;
using ManagedFileResult = Craiel.UnityEssentials.IO.ManagedFileResult;

namespace Craiel.GDX.AI.Sharp.IO
{
    using System.Collections.Generic;
    using System.IO;

    public class DirectoryFileProvider : BaseFileProvider
    {
        private readonly IList<FileStream> openStreams;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DirectoryFileProvider(ManagedDirectory root)
        {
            this.Root = root;
            this.openStreams = new List<FileStream>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ManagedDirectory Root { get; private set; }

        public override Stream BeginWrite(ManagedFile file)
        {
            ManagedFile fullFile = this.Root.ToFile(file);
            fullFile.GetDirectory().Create();

            var stream = fullFile.OpenCreate();
            this.openStreams.Add(stream);
            return stream;
        }

        public override Stream BeginRead(ManagedFile file)
        {
            ManagedFile fullFile = this.Root.ToFile(file);
            if (fullFile.Exists)
            {
                var stream = fullFile.OpenRead();
                this.openStreams.Add(stream);
                return stream;
            }

            return null;
        }

        public override ManagedFile[] Find(string pattern)
        {
            ManagedFileResult[] results = this.Root.GetFiles(pattern, SearchOption.AllDirectories);
            ManagedFile[] result = new ManagedFile[results.Length];
            for (var i = 0; i < results.Length; i++)
            {
                result[i] = results[i].Relative;
            }

            return result;
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

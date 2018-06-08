using ManagedFile = Craiel.UnityEssentials.Runtime.IO.ManagedFile;

namespace Craiel.GDX.AI.Sharp.Runtime.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LiteDB;

    public class DBFileProvider : BaseFileProvider
    {
        private readonly LiteDatabase database;

        private readonly IList<LiteFileStream> openStreams;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DBFileProvider(ManagedFile file)
        {
            if (!file.Exists)
            {
                // Ensure the directory exists
                file.GetDirectory().Create();
            }

            this.database = new LiteDatabase(file.GetPath());

            this.openStreams = new List<LiteFileStream>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override Stream BeginWrite(ManagedFile file)
        {
            var stream = this.database.FileStorage.OpenWrite(file.GetPathUsingAlternativeSeparator(), file.FileName);
            this.openStreams.Add(stream);
            return stream;
        }

        public override Stream BeginRead(ManagedFile file)
        {
            var stream = this.database.FileStorage.OpenRead(file.GetPathUsingAlternativeSeparator());
            this.openStreams.Add(stream);
            return stream;
        }

        public override ManagedFile[] Find(string pattern)
        {
            IList<ManagedFile> result = new List<ManagedFile>();
            foreach (LiteFileInfo fileInfo in this.database.FileStorage.Find(pattern))
            {
                result.Add(new ManagedFile(new ManagedFile(fileInfo.Id).GetPathUsingDefaultSeparator()));
            }

            return result.ToArray();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                foreach (LiteFileStream stream in this.openStreams)
                {
                    stream.Dispose();
                }

                this.openStreams.Clear();
                this.database.Dispose();
            }
        }
    }
}

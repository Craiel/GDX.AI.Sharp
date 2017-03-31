namespace GDX.AI.Sharp.IO
{
    using System.Collections.Generic;
    using System.IO;

    using CarbonCore.Utils.IO;

    using LiteDB;

    public class DBFileProvider : BaseFileProvider
    {
        private readonly LiteDatabase database;

        private readonly IList<LiteFileStream> openStreams;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public DBFileProvider(CarbonFile file)
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
        public override Stream BeginWrite(CarbonFile file)
        {
            var stream = this.database.FileStorage.OpenWrite(file.GetPath(), file.FileName);
            this.openStreams.Add(stream);
            return stream;
        }

        public override Stream BeginRead(CarbonFile file)
        {
            var stream = this.database.FileStorage.OpenRead(file.GetPath());
            this.openStreams.Add(stream);
            return stream;
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

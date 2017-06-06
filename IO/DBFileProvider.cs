namespace GDX.AI.Sharp.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CarbonCore.Utils.IO;

    using LiteDB;
    using NLog;

    public class DBFileProvider : BaseFileProvider
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

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
            var stream = this.database.FileStorage.OpenWrite(file.GetPathUsingAlternativeSeparator(), file.FileName);
            this.openStreams.Add(stream);
            return stream;
        }

        public override Stream BeginRead(CarbonFile file)
        {
            var stream = this.database.FileStorage.OpenRead(file.GetPathUsingAlternativeSeparator());
            this.openStreams.Add(stream);
            return stream;
        }

        public override CarbonFile[] Find(string pattern)
        {
            IList<CarbonFile> result = new List<CarbonFile>();
            foreach (LiteFileInfo fileInfo in this.database.FileStorage.Find(pattern))
            {
                result.Add(new CarbonFile(new CarbonFile(fileInfo.Id).GetPathUsingDefaultSeparator()));
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

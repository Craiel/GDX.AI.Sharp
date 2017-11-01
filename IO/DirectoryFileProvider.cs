namespace Assets.Scripts.Craiel.GDX.AI.Sharp.IO
{
    using System.Collections.Generic;
    using System.IO;
    
    using Essentials.IO;

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
        public CarbonDirectory Root { get; private set; }

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

        public override CarbonFile[] Find(string pattern)
        {
            CarbonFileResult[] results = this.Root.GetFiles(pattern, SearchOption.AllDirectories);
            CarbonFile[] result = new CarbonFile[results.Length];
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

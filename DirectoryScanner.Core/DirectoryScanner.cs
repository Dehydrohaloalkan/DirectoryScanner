using DirectoryScanner.Core.Model;

namespace DirectoryScanner.Core
{
    public class DirectoryScanner
    {
        private DirectoryTree Tree { get; set; }
        private readonly CancellationTokenSource _tokenSource;
        private TaskQueue _taskQueue;
        private readonly ushort _maxThreadCount;

        public DirectoryScanner(ushort maxThreadCount)
        {
            if (maxThreadCount <= 0)
            {
                throw new ArgumentException("Max thread count should be greater than 0");
            }
            
            _tokenSource = new CancellationTokenSource();
            _maxThreadCount = maxThreadCount;
        }

        public void Start(string path)
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                Tree = new DirectoryTree()
                {
                    Name = fileInfo.Name,
                    FullName = fileInfo.FullName,
                    Size = fileInfo.Length,
                    Percent = 100,
                    IsDirectory = false,
                };
                return;
            }

            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Directory {path} does not exist");
            }

            var directoryInfo = new DirectoryInfo(path);
            Tree = new DirectoryTree()
            {
                Name = directoryInfo.Name,
                FullName = directoryInfo.FullName,
                IsDirectory = true,
                Percent = 100,
                Childrens = new List<DirectoryTree>(),
            };
            _taskQueue = new TaskQueue(_maxThreadCount, _tokenSource);
            _taskQueue.EnqueueTask(() => ScanDirectory(Tree));
        }

        public DirectoryTree Stop()
        {
            _taskQueue.Dispose();
            Tree.RecalculateSize();
            Tree.RecalculatePercents();
            return Tree;
        }

        public DirectoryTree GetResult()
        {
            _taskQueue.WaitEnd();
            Tree.RecalculateSize();
            Tree.RecalculatePercents();
            return Tree;
        }

        private void ScanDirectory(DirectoryTree node)
        {
            var directoryInfo = new DirectoryInfo(node.FullName);
            var token = _tokenSource.Token;

            List<DirectoryInfo>? directories;
            try
            {
                directories = directoryInfo.GetDirectories()
                    .Where(d => d.LinkTarget == null)
                    .ToList();
            }
            catch (Exception)
            {
                directories = null;
            }

            List<FileInfo>? files;
            try
            {
                files = directoryInfo.GetFiles()
                    .Where(f => f.LinkTarget == null)
                    .ToList();
            }
            catch (Exception)
            {
                files = null;
            }

            if (directories != null)
            {
                foreach (var directory in directories)
                {
                    if (token.IsCancellationRequested)
                        return;

                    var tree = new DirectoryTree()
                    {
                        Name = directory.Name,
                        FullName = directory.FullName,
                        IsDirectory = true,
                        Childrens = new List<DirectoryTree>(),
                    };
                    node.Childrens.Add(tree);
                    _taskQueue.EnqueueTask(() => ScanDirectory(tree));
                }
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                        return;

                    var tree = new DirectoryTree()
                    {
                        Name = file.Name,
                        FullName = file.FullName,
                        Size = file.Length,
                        IsDirectory = false,
                    };
                    node.Childrens.Add(tree);
                }
            }

        }
    }
}


var directoryScanner = new DirectoryScanner.Core.DirectoryScanner(25);

directoryScanner.Start("D:/");

var tree = directoryScanner.GetResult();

int u = 1;
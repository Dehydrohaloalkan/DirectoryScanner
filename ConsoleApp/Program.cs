

var directoryScanner = new DirectoryScanner.Core.DirectoryScanner();

directoryScanner.Start("C:/", 120);

directoryScanner.WaitEnd();

var tree = directoryScanner.Tree;

int u = 1;
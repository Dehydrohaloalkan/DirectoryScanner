using System.Collections;
using System.IO;
using System.Text;
using DirectoryScanner.Core.Model;

namespace DirectoryScanner.Test;

public class Tests
{ 
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void DirectoryScanner_ScanDirectory_ReturnDirectoryTree()
    {
        const string root = "TestDir";
        const string first = "First";
        const string second = "Second";
        const string file = "file.txt";

        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (!Directory.Exists(Path.Combine(root, first))) Directory.CreateDirectory(Path.Combine(root, first));
        if (!Directory.Exists(Path.Combine(root, second))) Directory.CreateDirectory(Path.Combine(root, second));
        if (!File.Exists(Path.Combine(root, first, file)))
        {
            using var fs = File.Create(Path.Combine(root, first, file));
            var info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
            fs.Write(info, 0, info.Length);
        }

        var scanner = new Core.DirectoryScanner(5);
        scanner.Start(root);
        var tree = scanner.GetResult();

        Assert.Multiple(() =>
        {
            Assert.That(tree.Name, Is.EqualTo(root));
            Assert.That(tree.Childrens.Count, Is.EqualTo(2));
            Assert.That(tree.Childrens.Select(c => c.Name).ToList(), Does.Contain(first));
            Assert.That(tree.Childrens.Select(c => c.Name).ToList(), Does.Contain(second));
            Assert.That(tree.Size, Is.Not.Zero);
        });
    }

    [Test]
    public void DirectoryScanner_StopScanDirectory_ReturnDirectoryTree()
    {
        const string root = "C:/Windows/System32";
        
        var scanner = new Core.DirectoryScanner(50);
        scanner.Start(root);
        var fullTree = scanner.GetResult();
        
        Task.Run(() => scanner.Start(root));
        Thread.Sleep(10);
        var stopTree = scanner.Stop();

        Assert.Multiple(() =>
        {
            Assert.That(stopTree.Size, Is.Not.EqualTo(fullTree.Size));
        });
    }
}
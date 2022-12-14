using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryScanner.Core.Model;

namespace DirectoryScanner.WPF.Model;

public class ModelTree
{
    public string Name { get; set; }
    public long Size { get; set; }
    public float Percent { get; set; }
    public string Icon { get; set; }
    public string IconColor { get; set; }
    public List<ModelTree> Children { get; set; }

    public ModelTree(DirectoryTree tree)
    {
        Name = tree.Name;
        Size = tree.Size;
        Percent = tree.Percent;
        Icon = tree.IsDirectory ? "Folder" : "File";
        IconColor = tree.IsDirectory ? "Orange" : "CornflowerBlue"; 
        Children = tree.Childrens?.Select(c => new ModelTree(c)).ToList();
    }

    public ModelTree()
    {
    }
}
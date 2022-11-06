namespace DirectoryScanner.Core.Model;

public class DirectoryTree
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    
    public List<DirectoryTree> Childrens { get; set; }
}
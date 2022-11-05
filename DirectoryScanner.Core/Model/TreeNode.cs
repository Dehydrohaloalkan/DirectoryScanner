namespace DirectoryScanner.Core.Model;

public class TreeNode
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public long Size { get; set; } 
    public bool IsDirectory { get; set; }
    
}
namespace DirectoryScanner.Core.Model;

public class DirectoryTree
{
    public TreeNode Node { get; set; }
    public List<DirectoryTree> Childrens { get; set; }

}
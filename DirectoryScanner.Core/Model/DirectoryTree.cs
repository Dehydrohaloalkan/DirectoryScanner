namespace DirectoryScanner.Core.Model;

public class DirectoryTree
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public long Size { get; set; }
    public float Percent { get; set; }
    public bool IsDirectory { get; set; }
    
    public List<DirectoryTree> Childrens { get; set; }

    public long RecalculateSize()
    {
        if (IsDirectory)
        {
            Size = Childrens.Count != 0 ? Childrens.Select(c => c.RecalculateSize()).Sum() : 0;
        }
        return Size;
    }

    public void RecalculatePercents()
    {
        if (!IsDirectory) return;
        foreach (var children in Childrens)
        {
            children.Percent = (float)children.Size / Size * 100;
            children.RecalculatePercents();  
        }
    }
}
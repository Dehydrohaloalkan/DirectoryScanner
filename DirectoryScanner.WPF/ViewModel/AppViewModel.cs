using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DirectoryScanner.WPF.Command;
using System.Windows.Forms;

namespace DirectoryScanner.WPF.ViewModel;

public class AppViewModel : INotifyPropertyChanged
{
    public RelayCommand SetDirectoryCommand { get; set; }
    public RelayCommand StartCommand { get; set; }
    public RelayCommand StopCommand { get; set; }

    private Core.DirectoryScanner _directoryScanner;

    private string? _path;
    public string? Path
    {
        get => _path;
        set
        {
            _path = value;
            OnPropertyChanged();
        }
    }

    private ushort _maxThreadCount;

    public ushort MaxThreadCount
    {
        get => _maxThreadCount;
        set
        {
            _maxThreadCount = value;
            OnPropertyChanged();
        }
    }

    private Core.Model.DirectoryTree _tree;

    public Core.Model.DirectoryTree Tree
    {
        get => _tree;
        set
        {
            _tree = value;
            OnPropertyChanged();
        }
    }

    private volatile bool _isScanning = false;
    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            _isScanning = value;
            OnPropertyChanged();
        }
    }

    public AppViewModel()
    {
        SetDirectoryCommand = new RelayCommand(_ =>
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Path = folderBrowserDialog.SelectedPath;
            }
        });
        
        StartCommand = new RelayCommand(_ =>
        {
            Task.Run(() =>
            {
                _directoryScanner = new Core.DirectoryScanner(MaxThreadCount);
                IsScanning = true;
                _directoryScanner.Start(Path);
                IsScanning = false;
                Tree = _directoryScanner.GetResult();
            });
        }, _ => Path != null && !IsScanning);
        
        StopCommand = new RelayCommand(_ =>
        {
            Tree = _directoryScanner.Stop();
            IsScanning = false;
        }, _ => IsScanning);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
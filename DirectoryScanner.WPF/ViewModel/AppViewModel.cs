using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DirectoryScanner.WPF.Command;
using System.Windows.Forms;
using DirectoryScanner.WPF.Model;

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

    private ushort _maxThreadCount = 10;

    public ushort MaxThreadCount
    {
        get => _maxThreadCount;
        set
        {
            _maxThreadCount = value;
            OnPropertyChanged();
        }
    }

    private ModelTree _tree;

    public ModelTree Tree
    {
        get => _tree;
        set
        {
            _tree = value;
            OnPropertyChanged();
        }
    }

    private bool _isScanning;
    public bool IsScanning
    {
        get => _isScanning;
        set
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
            IsScanning = true;
            Task.Run(() =>
            {
                _directoryScanner = new Core.DirectoryScanner(MaxThreadCount);
                _directoryScanner.Start(Path);
                var directoryTree = _directoryScanner.GetResult();
                Tree = new ModelTree()
                {
                    Children = new List<ModelTree>()
                    {
                        new(directoryTree)
                    }
                };
                IsScanning = false;
            });
        }, _ => Path != null && !IsScanning);
        
        StopCommand = new RelayCommand(_ =>
        {
            var directoryTree = _directoryScanner.Stop();
            IsScanning = false;
            Tree = new ModelTree()
            {
                Children = new List<ModelTree>()
                {
                    new(directoryTree)
                }
            };
        }, _ => IsScanning);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
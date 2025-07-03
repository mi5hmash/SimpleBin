using System.Runtime.InteropServices;
using System.Security.Principal;
using Timer = System.Threading.Timer;


namespace SimpleBin; 


public class BinHelper : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = [];
    public delegate void BinUpdateHandler(object sender, FileSystemEventArgs e);
    public event BinUpdateHandler? Update;
    private Timer? _debounceTimer ;
    private readonly Lock _lock = new ();
    private bool _isDisposed;
    
    public BinHelper()
    {
        string? sid = (WindowsIdentity.GetCurrent().User?.Value) 
            ?? throw new InvalidOperationException("No such user");
        string binPath = Path.Combine("$Recycle.Bin", sid);
        
        var bins = DriveInfo.GetDrives().Where(d => d is
        {
            IsReady: true, 
            DriveType: DriveType.Fixed or DriveType.Network
        }).Select(d => Path.Combine(d.RootDirectory.FullName, binPath));

        foreach (var bin in bins)
        {
            if(!Directory.Exists(bin)) continue;

            var watcher = new FileSystemWatcher(bin)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
                InternalBufferSize = 65536,
                Filter = "$I*",
                NotifyFilter = NotifyFilters.FileName
            };

            watcher.Created += OnBinChanged;
            watcher.Deleted += OnBinChanged;
            _watchers.Add(watcher);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Shqueryrbinfo
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBinW(string? pszRootPath, ref Shqueryrbinfo pShQueryRbInfo);

    internal (long biteSize, long itemCount) GetBinSize()
    {
        const string? pszRootPath = null; //need to watch size from all disks
        var info = new Shqueryrbinfo();
        info.cbSize = Marshal.SizeOf(info);
        const int okCode = 0;

        var result = SHQueryRecycleBinW(pszRootPath, ref info);

        if (result != okCode) throw new Exception("SHQueryRecycleBinW failed");

        return (info.i64Size, info.i64NumItems);
    }


    [Flags]
    private enum RecycleBinFlags : uint
    {
        SherbNoconfirmation = 0x00000001,
        SherbNoprogressui = 0x00000002,
        SherbNosound = 0x00000004
    }


    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SHEmptyRecycleBinW(
        IntPtr hwnd,
        string? pszRootPath,
        RecycleBinFlags dwFlags);

    internal bool ClearBin()
    {
        const int okCode = 0;
        var parentWindow = IntPtr.Zero;
        const string? pszRootPath = null; //need to clear data from all disks

        RecycleBinFlags flags = RecycleBinFlags.SherbNoconfirmation |
                                RecycleBinFlags.SherbNosound;

        int resultCode = SHEmptyRecycleBinW(parentWindow, pszRootPath, flags);

        if (resultCode != okCode) throw new Exception("SHEmptyRecycleBinW failed");

        return true;
    }

    private void OnBinChanged(object? sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new(DebounceCallback, e, 500, Timeout.Infinite);
        }
    }

    private void DebounceCallback(object? state)
    {
        lock (_lock)
        {
            Update?.Invoke(this, (FileSystemEventArgs)state!);
            _debounceTimer!.Dispose();   
            _debounceTimer = null;
        }
    }

    public bool IsBinEmpty() => GetBinSize().itemCount == 0; 
    
    public void Dispose()
    {
        if (!_isDisposed)
        {
            lock (_lock)
            {
                _debounceTimer?.Dispose();
                _debounceTimer = null;
            }
        
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            
            _isDisposed = true;
        }
    }
}
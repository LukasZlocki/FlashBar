using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FlashBar.Infrastructure;

internal static class WindowsShortcutIconCache
{
    private const uint ShellGetIcon = 0x00000100;
    private const int GdiPlusSuccess = 0;
    private static readonly Guid PngEncoder = new("557cf406-1a04-11d3-9a73-0000f81ef32e");

    public static string? GetIconPath(string shortcutPath)
    {
        string? temporaryPath = null;
        try
        {
            var cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FlashBar",
                "Icons");
            var cacheKey = CreateCacheKey(shortcutPath);
            var iconPath = Path.Combine(cacheDirectory, $"{cacheKey}.png");
            if (File.Exists(iconPath))
            {
                return iconPath;
            }

            Directory.CreateDirectory(cacheDirectory);
            var shellInfo = new ShellFileInfo
            {
                DisplayName = string.Empty,
                TypeName = string.Empty,
            };
            var result = SHGetFileInfo(
                shortcutPath,
                0,
                out shellInfo,
                (uint)Marshal.SizeOf<ShellFileInfo>(),
                ShellGetIcon);
            if (result == IntPtr.Zero || shellInfo.IconHandle == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                temporaryPath = Path.Combine(cacheDirectory, $"{Guid.NewGuid():N}.tmp");
                SaveIconAsPng(shellInfo.IconHandle, temporaryPath);
                File.Move(temporaryPath, iconPath, overwrite: true);
                temporaryPath = null;
                return iconPath;
            }
            finally
            {
                DestroyIcon(shellInfo.IconHandle);
            }
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }

    private static string CreateCacheKey(string shortcutPath)
    {
        var fullPath = Path.GetFullPath(shortcutPath);
        var lastWriteTime = File.GetLastWriteTimeUtc(fullPath).Ticks;
        var source = Encoding.UTF8.GetBytes($"{fullPath.ToUpperInvariant()}|{lastWriteTime}");
        return Convert.ToHexString(SHA256.HashData(source));
    }

    private static void SaveIconAsPng(IntPtr iconHandle, string destinationPath)
    {
        var startupInput = new GdiPlusStartupInput { Version = 1 };
        CheckGdiPlusStatus(GdiplusStartup(out var token, ref startupInput, IntPtr.Zero));
        try
        {
            CheckGdiPlusStatus(GdipCreateBitmapFromHICON(iconHandle, out var bitmap));
            try
            {
                var encoder = PngEncoder;
                CheckGdiPlusStatus(GdipSaveImageToFile(bitmap, destinationPath, ref encoder, IntPtr.Zero));
            }
            finally
            {
                GdipDisposeImage(bitmap);
            }
        }
        finally
        {
            GdiplusShutdown(token);
        }
    }

    private static void CheckGdiPlusStatus(int status)
    {
        if (status != GdiPlusSuccess)
        {
            throw new InvalidOperationException($"GDI+ operation failed with status {status}.");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShellFileInfo
    {
        public IntPtr IconHandle;
        public int IconIndex;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GdiPlusStartupInput
    {
        public uint Version;
        public IntPtr DebugEventCallback;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressBackgroundThread;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressExternalCodecs;
    }

    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfoW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string path,
        uint fileAttributes,
        out ShellFileInfo fileInfo,
        uint fileInfoSize,
        uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr iconHandle);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdiplusStartup(out IntPtr token, ref GdiPlusStartupInput input, IntPtr output);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern void GdiplusShutdown(IntPtr token);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipCreateBitmapFromHICON(IntPtr iconHandle, out IntPtr bitmap);

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipSaveImageToFile(IntPtr image, string fileName, ref Guid encoder, IntPtr encoderParameters);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDisposeImage(IntPtr image);
}

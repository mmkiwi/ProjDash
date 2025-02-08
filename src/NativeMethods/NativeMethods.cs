using System.Runtime.CompilerServices;

using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using Serilog;

using static Windows.Win32.PInvoke;

namespace MMKiwi.ProjDash.Native;



public static unsafe partial class NativeMethods
{
    public static bool IsWindowOnTop(nint hWndPtr)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(5))
        {
            return false;
        }

        HWND hWnd = new(hWndPtr);
        if(!GetWindowRect(hWnd, out RECT windowRect))
            return false;
        
        HWND hChildWnd = hWnd;

        HWND topWnd = GetWindow(hChildWnd, GET_WINDOW_CMD.GW_HWNDPREV);
        Span<char> windowTitle = stackalloc char[20];
        do
        {
            GetWindowRect(topWnd, out RECT topWndRect);

            int length = 0;
            fixed (char* windowTitlePtr = windowTitle)
                GetWindowText(topWnd, new(windowTitlePtr), windowTitle.Length);

            string windowTitleStr = new(windowTitle[..length]);

            bool isVisible = IsWindowVisible(topWnd);
            bool isIconic = IsIconic(topWnd);

            if (isVisible && !isIconic)
            {
                Log.Verbose($"Checking \"{windowTitleStr}\" {topWndRect} {windowRect} {isVisible} {isIconic}");
                if (IntersectRect(out RECT tempRect, topWndRect, windowRect) && tempRect.Area() > 0)
                {
                    Log.Verbose("Window is not fully visible");
                    return false;
                }
            }

            topWnd = GetWindow(topWnd, GET_WINDOW_CMD.GW_HWNDPREV);
        } while (!topWnd.IsNull);

        Log.Verbose("Window is fully visible");
        return true;
    }
/*
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HWnd GetWindow(HWnd hWnd, GetWindowMode mode);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetWindowTextW(HWnd hwnd, Span<char> text, int maxCount);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(HWnd hWnd, out Rect lprect);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(HWnd hWnd);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(HWnd hWnd);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial HWnd GetAncestor(HWnd hWnd, GetAncestorFlags gaFlags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IntersectRect(out Rect lprcDst, in Rect lprcSrc1, in Rect lprcSrc2);


    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(nint hrgn);


    [StructLayout(LayoutKind.Sequential)]
    readonly record struct Rect
    {
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;

        public int Area => (Right - Left) * (Bottom - Top);
    }

    private class HWnd : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HWnd() : base(false)
        {
        }

        public HWnd(nint ptr) : base(false)
        {
            this.SetHandle(ptr);
        }

        protected override bool ReleaseHandle()
        {
            return false;
        }
    }

    private class HRgn : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HRgn() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return DeleteObject(handle);
        }
    }

    enum GetAncestorFlags : uint
    {
        GA_PARENT = 1,
        GA_ROOT = 2,
        GA_ROOTOWNER = 3
    }

    enum GetWindowMode : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6,
    }
    */

    private static int Area(this RECT rect)
    {
        return rect.Width * rect.Height;
    }
}
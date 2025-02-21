// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Drawing;
using System.Drawing.Imaging;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;

using ReactiveUI;

using Bitmap = Avalonia.Media.Imaging.Bitmap;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MMKiwi.ProjDash.GUI.Dialogs;

public partial class IconDialog : ReactiveWindow<IconDialogViewModel>
{
    public IconDialog()
    {
        if (Design.IsDesignMode)
        {
            ViewModel = new IconDialogViewModel(@"C:\Windows\System32\shell32.dll");
        }

        InitializeComponent();

        ReactiveCommand<int?, Unit> closeDialog = ReactiveCommand.Create((int? result) => Close(result));

        this.WhenActivated(d =>
        {
            ViewModel!.Save.InvokeCommand(closeDialog).DisposeWith(d);
            ViewModel!.Cancel.InvokeCommand(closeDialog).DisposeWith(d);
        });
    }
}

public class IconDialogViewModel : ViewModelBase
{
    public IconDialogViewModel(string filePath)
    {
        FilePath = filePath;
        NumIcons = NativeMethods.ExtractIconW(IntPtr.Zero, filePath, -1);
        Icons = [..Enumerable.Range(0, NumIcons).Select(LoadIcon)];

        var isSel = this.WhenAnyValue(vm => vm.SelectedIndex).Select(si => si is not null);
        Save = ReactiveCommand.Create(() => SelectedIndex, isSel);
        Cancel = ReactiveCommand.Create(() => (int?)null);
    }

    public ReactiveCommand<Unit, int?> Save { get; }
    public ReactiveCommand<Unit, int?> Cancel { get; }

    public IReadOnlyList<Bitmap?> Icons { get; set; }

    public int? SelectedIndex { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    private string FilePath { get; }
    private int NumIcons { get; }

    private Bitmap? LoadIcon(int index)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            return null;
        }

        Icon? icon = Icon.ExtractIcon(FilePath, index, 24);
        if (icon is null)
            return null;
        System.Drawing.Bitmap bmp = icon.ToBitmap();
        var bitmapdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);
        Bitmap bitmap1 = new(Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul,
            bitmapdata.Scan0,
            new PixelSize(bitmapdata.Width, bitmapdata.Height),
            new Vector(96, 96),
            bitmapdata.Stride);
        bmp.UnlockBits(bitmapdata);
        bmp.Dispose();
        return bitmap1;
    }
}
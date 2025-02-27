﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using MMKiwi.ProjDash.GUI.Dialogs;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.IconEditors;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class FileIconEditor : ReactiveUserControl<FileIconViewModel>
{
    public FileIconEditor()
    {
        InitializeComponent();
    }

    public async Task BrowseForIconAsync()
    {
        if (ViewModel is not { } viewModel)
            return;
        var pickedFile = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select an icon",
                FileTypeFilter = OperatingSystem.IsWindowsVersionAtLeast(6, 1)
                    ?
                    [
                        new FilePickerFileType("All Icons") { Patterns = [..ImageFileTypes, ..ExecutableFileTypes] },
                        new FilePickerFileType("Image Files") { Patterns = [..ImageFileTypes] },
                        new FilePickerFileType("Executable files") { Patterns = [..ExecutableFileTypes] },
                        FilePickerFileTypes.All
                    ]
                    :
                    [
                        new FilePickerFileType("Image Files") { Patterns = [..ImageFileTypes] },
                        FilePickerFileTypes.All
                    ]
            }).ConfigureAwait(false);
        if (pickedFile is not [{ } file])
        {
            return;
        }

        var stream = await file.OpenReadAsync().ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            Span<byte> fileHeader = stackalloc byte[12];
            stream.ReadExactly(fileHeader);
            string mimeType;
            byte[]? fileData;

            string filePath =
                file.TryGetLocalPath() ??
                throw new NotSupportedException(); // Shouldn't ever hit this since we're on Windows
            switch (fileHeader)
            {
                case [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, ..]:
                    // PNG
                    mimeType = "image/png";
                    fileData = await ReadFileFullyAsync(stream).ConfigureAwait(false);
                    break;
                case [0xFF, 0xD8, 0xFF, ..]:
                    //JPG
                    mimeType = "image/jpeg";
                    fileData = await ReadFileFullyAsync(stream).ConfigureAwait(false);
                    break;
                case [0x47, 0x49, 0x46, 0x46, 0x38, 0x37 or 0x39, 0x61, ..]:
                    //GIF
                    mimeType = "image/gif";
                    fileData = await ReadFileFullyAsync(stream).ConfigureAwait(false);
                    break;
                case [0x42, 0x4D, ..]:
                    //BMP
                    mimeType = "image/bmp";
                    fileData = await ReadFileFullyAsync(stream).ConfigureAwait(false);
                    break;
                case [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..]:
                    // webp
                    mimeType = "image/webp";
                    fileData = await ReadFileFullyAsync(stream).ConfigureAwait(false);
                    break;
                case [0x00, 0x00, 0x01, 0x00, ..]:
                case [0x4D, 0x5A, ..]:
                    if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                        throw new PlatformNotSupportedException();
                    //exe or dll
                    mimeType = "image/png";
                    // Don't need stream for exe files
                    stream.Close();
                    int numIcons = NativeMethods.ExtractIconW(IntPtr.Zero, filePath, -1);
                    int? iconIndex = 0;
                    switch (numIcons)
                    {
                        case 0:
                            _ = await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    ErrorDialog dialog = new()
                                    {
                                        ViewModel = new ErrorDialogViewModel
                                        {
                                            MainMessage = $"Could not find any icons in {filePath}",
                                            PrimaryButtonText = "Ok"
                                        }
                                    };
                                    return await dialog
                                        .ShowDialog<ErrorDialogResult>((TopLevel.GetTopLevel(this) as Window)!)
                                        .ConfigureAwait(false);
                                })
                                .ConfigureAwait(false);
                            return;
                        case > 1:
                            iconIndex = await Dispatcher.UIThread
                                .InvokeAsync(async () =>
                                {
                                    IconDialog dialog = new() { ViewModel = new IconDialogViewModel(filePath) };
                                    return await dialog.ShowDialog<int?>((TopLevel.GetTopLevel(this) as Window)!)
                                        .ConfigureAwait(false);
                                })
                                .ConfigureAwait(false);
                            break;
                    }

                    if (iconIndex is null)
                        return;

                    Icon? icon = Icon.ExtractIcon(filePath, iconIndex.Value, 24);
                    if (icon is null)
                        return;

                    var bmp = icon.ToBitmap();
                {
                    using MemoryStream ms = new();
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    fileData = ms.ToArray();
                }
                    break;
                default:
                    // show exception
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            ErrorDialog dialog = new()
                            {
                                ViewModel = new ErrorDialogViewModel
                                {
                                    MainMessage =
                                        $"Error loading file {filePath}, not a supported file format.",
                                    PrimaryButtonText = "Ok"
                                }
                            };
                            await dialog.ShowDialog<ErrorDialogResult>((TopLevel.GetTopLevel(this) as Window)!)
                                .ConfigureAwait(false);
                        })
                        .ConfigureAwait(false);
                    return;
            }

            viewModel.IconUri = $"data:{mimeType};base64,{Convert.ToBase64String(fileData)}";
        }
    }


    private static async Task<byte[]> ReadFileFullyAsync(Stream input)
    {
        input.Seek(0, SeekOrigin.Begin);
        using MemoryStream ms = new((int)input.Length);
        await input.CopyToAsync(ms).ConfigureAwait(false);
        return ms.GetBuffer();
    }

    private static IReadOnlyList<string> ExecutableFileTypes => ["*.ico", "*.exe", "*.dll"];
    private static IReadOnlyList<string> ImageFileTypes => ["*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.webp"];
}

internal static partial class NativeMethods
{
    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int ExtractIconW(IntPtr hInst, string lpszExeFileName, int nIconIndex);
}
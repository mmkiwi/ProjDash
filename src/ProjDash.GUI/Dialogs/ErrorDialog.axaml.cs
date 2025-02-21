// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI.Dialogs;

public partial class ErrorDialog : ReactiveWindow<ErrorDialogViewModel>
{
    public ErrorDialog()
    {
        Log.Verbose("ErrorDialog");
        CloseCommand = ReactiveCommand.Create((ErrorDialogResult result) => Close(result));
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            try
            {
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                ViewModel = new ErrorDialogViewModel
                {
                    MainMessage = "Test Main Message", PrimaryButtonText = "Ok", Exception = ex
                };
            }
        }
    }

    public ReactiveCommand<ErrorDialogResult, Unit> CloseCommand { get; }
}
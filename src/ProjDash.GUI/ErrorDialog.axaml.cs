using System.ComponentModel;
using System.Reactive;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI;

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
                ViewModel = new ErrorDialogViewModel()
                {
                    MainMessage = "Test Main Message", PrimaryButtonText = "Ok", Exception = ex
                };
            }
        }
    }

    public ReactiveCommand<ErrorDialogResult, Unit> CloseCommand { get; }
}
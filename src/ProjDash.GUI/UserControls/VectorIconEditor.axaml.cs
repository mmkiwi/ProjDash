using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel.IconEditors;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class VectorIconEditor : ReactiveUserControl<VectorIconViewModel>
{
    public VectorIconEditor()
    {
        InitializeComponent();
    }
}
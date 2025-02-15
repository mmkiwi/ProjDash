using System.Reactive.Linq;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.ViewModel.IconEditors;

public sealed class MaterialIconViewModel : IconViewModel
{
    public MaterialIconViewModel(IconRef.MaterialIcon? icon)
    {
        Icon = icon switch
        {
            null => null,
            _ when icon.Reference.StartsWith("mdi-") => icon.Reference[4..],
            not null => icon.Reference,
        };
        _iconRef = this.WhenAnyValue(vm => vm.Icon)
            .Select(ic => ic is null ? null : new IconRef.MaterialIcon { Reference = $"mdi-{ic}" })
            .ToProperty(this, vm => vm.IconRef);
    }

    public string? Icon { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    private readonly ObservableAsPropertyHelper<IconRef?> _iconRef;
    public override IconRef? IconRef => _iconRef.Value;

    public override string? DisplayName => "Built-in Icon";
    public override string FieldLabel => "Icon Key";
    public override bool CanChangeColor => true;
}
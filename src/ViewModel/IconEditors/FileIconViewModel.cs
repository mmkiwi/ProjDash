using System.Reactive.Linq;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.ViewModel.IconEditors;

public sealed class FileIconViewModel : IconViewModel
{
    private readonly ObservableAsPropertyHelper<IconRef?> _iconRef;

    public FileIconViewModel(IconRef.DataUriIcon? projectLinkIcon)
    {
        IconUri = projectLinkIcon?.Reference;
        
        _iconRef = this.WhenAnyValue(vm => vm.IconUri)
            .Select(ic => ic is null ? null : new IconRef.DataUriIcon { Reference = ic })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.IconRef);
        
    }

    public string? IconUri { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    public override IconRef? IconRef => _iconRef.Value;
    public override string? DisplayName => "Image file (.ico, .jpg, .png, etc.)";
    public override string FieldLabel => "Browse for icon";
    public override bool CanChangeColor => false; 
}
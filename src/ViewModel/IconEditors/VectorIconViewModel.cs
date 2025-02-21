// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive.Linq;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.ViewModel.IconEditors;

public class VectorIconViewModel : IconViewModel
{
    public VectorIconViewModel(IconRef.ImportIcon? projectLinkIcon, IEnumerable<string> icons)
    {
        Icons = icons;
        SelectedIcon = projectLinkIcon?.Reference;
        _iconRef = this.WhenAnyValue(vm => vm.SelectedIcon)
            .Select(ic => ic is null ? null : new IconRef.ImportIcon { Reference = ic })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.IconRef);
    }

    public IEnumerable<string> Icons { get; }


    private readonly ObservableAsPropertyHelper<IconRef?> _iconRef;
    public override IconRef? IconRef => _iconRef.Value;

    public string? SelectedIcon { get; set => this.RaiseAndSetIfChanged(ref field, value); }
    public override string DisplayName => "Vector Icon";
    public override string FieldLabel => "Icon";
    public override bool CanChangeColor => true;
}
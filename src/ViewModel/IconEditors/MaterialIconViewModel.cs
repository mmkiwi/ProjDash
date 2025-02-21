// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive.Disposables;
using System.Reactive.Linq;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace MMKiwi.ProjDash.ViewModel.IconEditors;

public sealed class MaterialIconViewModel : IconViewModel
{
    public MaterialIconViewModel(IconRef.MaterialIcon? icon)
    {
        Icon = icon switch
        {
            null => null,
            _ when icon.Reference.StartsWith("mdi-") => icon.Reference[4..],
            not null => icon.Reference
        };
        _iconRef = this.WhenAnyValue(vm => vm.Icon)
            .Select(static ic => ic is null ? null : new IconRef.MaterialIcon { Reference = $"mdi-{ic}" })
            .ToProperty(this, vm => vm.IconRef);
        
        this.WhenActivated(d=>
            this.ValidationRule(vm=>vm.Icon,
                static ic => ic is not null,
                "Please set an icon").DisposeWith(d)
            );
    }

    public string? Icon { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    private readonly ObservableAsPropertyHelper<IconRef?> _iconRef;
    public override IconRef? IconRef => _iconRef.Value;

    public override string DisplayName => "Built-in Icon";
    public override string FieldLabel => "Icon Key";
    public override bool CanChangeColor => true;
}
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive.Disposables;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

namespace MMKiwi.ProjDash.ViewModel.IconEditors;

public abstract class IconViewModel : ViewModelBase
{
    private protected IconViewModel()
    {
        this.WhenActivated(d =>
        {
            this.ValidationRule(vm => vm.DisplayName,
                name => !string.IsNullOrWhiteSpace(name),
                "Please specify a link name").DisposeWith(d);
            this.ValidationRule(vm => vm.IconRef,
                ir => ir is not null,
                "Please set an icon").DisposeWith(d);
        });
    }

    public abstract IconRef? IconRef { get; }
    public abstract string? DisplayName { get; }

    public abstract string FieldLabel { get; }

    public abstract bool CanChangeColor { get; }
}
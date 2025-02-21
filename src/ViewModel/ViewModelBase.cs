// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace MMKiwi.ProjDash.ViewModel;

public abstract class ViewModelBase : ReactiveValidationObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
}
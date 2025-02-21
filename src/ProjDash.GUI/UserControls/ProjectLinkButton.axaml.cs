// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectLinkButton : ReactiveUserControl<ProjectLink>
{
    public ProjectLinkButton()
    {
        Open = ReactiveCommand.CreateFromTask<Uri>(OpenImpl);
        InitializeComponent();
    }

    public ReactiveCommand<Uri, Unit> Open { get; }

    private async Task OpenImpl(Uri uri)
    {
        await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(uri).ConfigureAwait(true);
    }
}
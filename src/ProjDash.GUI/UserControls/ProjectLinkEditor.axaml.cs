// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectLinkEditor : ReactiveUserControl<ProjectLinkViewModel>
{
    public ProjectLinkEditor()
    {
        if (Design.IsDesignMode)
            ViewModel = new ProjectLinkViewModel(new ProjectLink
            {
                Name = "Test Link",
                Uri = new Uri("http://example.com"),
                Icon = new IconRef.MaterialIcon { Reference = "close" }
            });
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (ViewModel is null)
                return;
            ViewModel.WhenActivated(d);
        });
    }
}
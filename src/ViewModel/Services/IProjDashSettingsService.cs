// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MMKiwi.ProjDash.ViewModel.Services;

public interface IProjDashSettingsService
{
    public string SchemaPath { get; }

    public string SettingsPath { get; }

    public string WindowSettingsPath { get; }
}

public class FileProjDashSettingsService : IProjDashSettingsService
{
    public string SchemaPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.schema.json");
    
    public string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.json");

    public string WindowSettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.wnd.json");
}
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.Tests.ViewModel;

public class ProjectLinkViewModelTests
{
    [Fact]
    public void TestPrettyUriFormatUri()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = new Uri("http://example.com") });
        Assert.Equal("http://example.com/", vm.Uri);
    }
    
    [Fact]
    public void TestImport()
    {
        var icon = IconRef.DataUri("data:");
        using ProjectLinkViewModel vm = new(new ProjectLink()
        {
            Name = "test", Uri = new Uri("http://example.com"),
            Color = "#ffffff",
            Icon = icon
        });
        Assert.Equal("http://example.com/", vm.Uri);
        Assert.Equal("#ffffff", vm.Color);
        Assert.Equal(icon, vm.SelectedIcon!.IconRef);
        Assert.Equal("test", vm.Name);
    }

    [Fact]
    public void TestPrettyUriFormatPath()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = new Uri("file://C:/Path/to/file") });
        Assert.Equal("C:\\Path\\to\\file", vm.Uri);
    }
    
    [Fact]
    public void TestPrettyUriNull()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = null! });
        Assert.Null(vm.Uri);
    }
    
    [Fact]
    public void TestInvalidUri()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = null! });
        vm.Activator.Activate();
        vm.Uri = "THIS ISN'T A URI";
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestLoadIcons()
    {
        using MainWindowViewModel main = new();
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = new Uri("http://example.com") });
        vm.Activator.Activate();
        vm.Name = " ";
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestInvalidName()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = new Uri("http://example.com") });
        vm.Activator.Activate();
        vm.Name = " ";
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestInvalidIcon()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = new Uri("http://example.com") });
        vm.Activator.Activate();
        var invalidVm = new FileIconViewModel(null);
        invalidVm.Activator.Activate();
        vm.SelectedIcon = invalidVm;
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestNullUri()
    {
        using ProjectLinkViewModel vm = new(new ProjectLink() { Name = "test", Uri = null! });
        vm.Activator.Activate();
        vm.Uri = null;
        Assert.False(vm.ValidationContext.IsValid);
    }
}
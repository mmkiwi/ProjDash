// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MMKiwi.ProjDash.Tests.ViewModel.Helpers;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.Tests.ViewModel;

public class MaterialIconViewModelTests
{
    [Fact]
    public void TestNull()
    {
        MaterialIconViewModel vm = new(null);
        Assert.Null(vm.Icon);
        Assert.Null(vm.IconRef);
    }
    
    [Fact]
    public void TestNotNullPrefix()
    {
        MaterialIconViewModel vm = new(IconRef.Material("mdi-icon"));
        Assert.NotNull(vm.IconRef);
        Assert.NotNull(vm.Icon);
        Assert.Equal("icon", vm.Icon);
    }
    
    [Fact]
    public void TestNotNull()
    {
        MaterialIconViewModel vm = new(IconRef.Material("icon"));
        Assert.NotNull(vm.IconRef);
        Assert.NotNull(vm.Icon);
        Assert.Equal("icon", vm.Icon);
    }
    
    [Fact]
    public void TestIconRaisesEvents()
    {
        MaterialIconViewModel vm = new(null);

        vm.ShouldNotifyOn(s => s.IconRef)
            .When(s => s.Icon = "Some new value");

        Assert.NotNull(vm.IconRef?.Reference);
    }

    [Fact]
    public void TestInvalid()
    {
        using MaterialIconViewModel vm = new(null);
        using var _ = vm.Activator.Activate();
        vm.Icon = "Some new value";
        vm.Icon = "";
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestValid()
    {
        using MaterialIconViewModel vm = new(null);
        using var _ = vm.Activator.Activate();
        vm.Icon = "Some new value";
        Assert.True(vm.ValidationContext.IsValid);
    }
}
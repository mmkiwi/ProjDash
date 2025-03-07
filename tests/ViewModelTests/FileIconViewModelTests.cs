// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MMKiwi.ProjDash.Tests.ViewModel.Helpers;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI.Validation.Extensions;

namespace MMKiwi.ProjDash.Tests.ViewModel;

public class FileIconViewModelTests
{
    [Fact]
    public void TestNull()
    {
        FileIconViewModel vm = new(null);
        Assert.Null(vm.IconUri);
        Assert.Null(vm.IconRef);
    }
    
    [Fact]
    public void TestNotNull()
    {
        FileIconViewModel vm = new(IconRef.DataUri("test"));
        Assert.NotNull(vm.IconUri);
        Assert.NotNull(vm.IconRef);
    }

    [Fact]
    public void TestIconRaisesEvents()
    {
        FileIconViewModel vm = new(null);

        vm.ShouldNotifyOn(s => s.IconRef)
            .When(s => s.IconUri = "Some new value");

        Assert.Equal(vm.IconUri, vm.IconRef!.Reference);
    }

    [Fact]
    public void TestInvalid()
    {
        using FileIconViewModel vm = new(null);
        using var _ = vm.Activator.Activate();
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestValid()
    {
        using FileIconViewModel vm = new(null);
        using var _ = vm.Activator.Activate();
        vm.IconUri = "Some new value";
        Assert.True(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestValidProvidedValue()
    {
        using FileIconViewModel vm = new(IconRef.DataUri("test"));
        using var _ = vm.Activator.Activate();
        Assert.Equal("test", vm.IconUri);
        Assert.True(vm.ValidationContext.IsValid);
    }
}
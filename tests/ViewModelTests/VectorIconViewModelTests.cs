
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MMKiwi.ProjDash.Tests.ViewModel.Helpers;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.Tests.ViewModel;

public class VectorIconViewModelTests
{
    [Fact]
    public void TestNull()
    {
        VectorIconViewModel vm = new(null, []);
        Assert.Null(vm.SelectedIcon);
        Assert.Null(vm.IconRef);
    }
    
    [Fact]
    public void TestNotNull()
    {
        VectorIconViewModel vm = new(IconRef.Import("test"), ["test", "test2"]);
        Assert.NotNull(vm.IconRef);
        Assert.NotNull(vm.SelectedIcon);
        Assert.Equal("test", vm.SelectedIcon);
    }
    
    
    [Fact]
    public void TestIconRaisesEvents()
    {
        VectorIconViewModel vm = new(null, []);

        vm.ShouldNotifyOn(s => s.IconRef)
            .When(s => s.SelectedIcon = "Some new value");

        Assert.NotNull(vm.IconRef?.Reference);
    }

    [Fact]
    public void TestInvalid()
    {
        using VectorIconViewModel vm = new(null, []);
        using var _ = vm.Activator.Activate();
        Assert.False(vm.ValidationContext.IsValid);
    }
    
    [Fact]
    public void TestInvalidNotInCollection()
    {
        using VectorIconViewModel vm = new(null, ["test1", "test2", "test3"]);
        using var _ = vm.Activator.Activate();
        vm.SelectedIcon = "Some new value";
        Assert.False(vm.ValidationContext.IsValid);
    }

    
    [Fact]
    public void TestValid()
    {
        using VectorIconViewModel vm = new(null, ["test1", "test2", "test3"]);
        using var _ = vm.Activator.Activate();
        vm.SelectedIcon = "test1";
        Assert.True(vm.ValidationContext.IsValid);
    }
}
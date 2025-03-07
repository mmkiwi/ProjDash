// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.ComponentModel;
using System.Linq.Expressions;

namespace MMKiwi.ProjDash.Tests.ViewModel.Helpers;

public static class NotifyPropertyChanged
{
    public static NotifyExpectation<T>
        ShouldNotifyOn<T, TProperty>(this T owner, Expression<Func<T, TProperty>> propertyPicker)
        where T : INotifyPropertyChanged
    {
        return CreateExpectation(owner, propertyPicker, true);
    }

    public static NotifyExpectation<T> ShouldNotNotifyOn<T, TProperty>(this T owner,
        Expression<Func<T, TProperty>> propertyPicker)
        where T : INotifyPropertyChanged
    {
        return CreateExpectation(owner, propertyPicker, false);
    }

    private static NotifyExpectation<T>
        CreateExpectation<T, TProperty>(T owner, Expression<Func<T, TProperty>> pickProperty, bool eventExpected)
        where T : INotifyPropertyChanged
    {
        string propertyName = ((MemberExpression)pickProperty.Body).Member.Name;
        return new NotifyExpectation<T>(owner, propertyName, eventExpected);
    }
}
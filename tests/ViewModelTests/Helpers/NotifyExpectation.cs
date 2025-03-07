using System.ComponentModel;

namespace MMKiwi.ProjDash.Tests.ViewModel.Helpers;

public readonly ref struct NotifyExpectation<T> where T : INotifyPropertyChanged
{
    private readonly T _owner;
    private readonly string _propertyName;
    private readonly bool _eventExpected;

    public NotifyExpectation(T owner, string propertyName, bool eventExpected)
    {
        this._owner = owner;
        this._propertyName = propertyName;
        this._eventExpected = eventExpected;
    }

    public void When(Action<T> action)
    {
        bool eventWasRaised = false;
        string propertyName = _propertyName;
        this._owner.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                eventWasRaised = true;
            }
        };
        action(this._owner);
        Assert.Equal(this._eventExpected, eventWasRaised);
    }
}
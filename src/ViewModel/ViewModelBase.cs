using ReactiveUI;

namespace MMKiwi.ProjDash.ViewModel;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
}
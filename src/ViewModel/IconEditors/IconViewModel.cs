using System.Reactive.Disposables;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

namespace MMKiwi.ProjDash.ViewModel;

public abstract class IconViewModel : ViewModelBase, IValidatableViewModel
{
    private protected IconViewModel()
    {
        this.WhenActivated(d =>
        {
            this.ValidationRule(vm => vm.DisplayName,
                name => !string.IsNullOrWhiteSpace(name),
                "Please specify a link name").DisposeWith(d);
            this.ValidationRule(vm => vm.IconRef,
                ir => ir is not null,
                "Please set an icon").DisposeWith(d);
        });
    }

    public abstract IconRef? IconRef { get; }
    public abstract string? DisplayName { get; }

    public IValidationContext ValidationContext { get; } = new ValidationContext();
    public abstract string FieldLabel { get; }
    
    public abstract bool CanChangeColor { get; }
}
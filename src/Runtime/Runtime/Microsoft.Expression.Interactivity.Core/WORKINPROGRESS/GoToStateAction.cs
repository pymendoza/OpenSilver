#if MIGRATION

using System.Windows;
using System.Windows.Interactivity;

namespace Microsoft.Expression.Interactivity.Core
{
    //
    // Summary:
    //     An action that will transition a FrameworkElement to a specified VisualState
    //     when invoked.
    //
    // Remarks:
    //     If the TargetName property is set, this action will attempt to change the state
    //     of the targeted element. If not, it walks the element tree in an attempt to locate
    //     an alternative target that defines states. ControlTemplate and UserControl are
    //     two common possibilities.
    public class GoToStateAction : TargetedTriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty UseTransitionsProperty;
        public static readonly DependencyProperty StateNameProperty;

        public GoToStateAction() { }

        //
        // Summary:
        //     Determines whether or not to use a VisualTransition to transition between states.
        public bool UseTransitions { get; set; }
        //
        // Summary:
        //     The name of the VisualState.
        public string StateName { get; set; }

        //
        // Summary:
        //     This method is called when some criteria is met and the action is invoked.
        //
        // Parameters:
        //   parameter:
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Could not change the target to the specified StateName.
        protected override void Invoke(object parameter) { }
        //
        // Summary:
        //     Called when the target changes. If the TargetName property isn't set, this action
        //     has custom behavior.
        //
        // Parameters:
        //   oldTarget:
        //
        //   newTarget:
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Could not locate an appropriate FrameworkElement with states.
        protected override void OnTargetChanged(FrameworkElement oldTarget, FrameworkElement newTarget) { base.OnTargetChanged(oldTarget, newTarget); }
    }
}

#endif
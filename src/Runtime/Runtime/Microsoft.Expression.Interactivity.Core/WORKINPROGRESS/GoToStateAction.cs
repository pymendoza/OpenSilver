#if MIGRATION

using System;
using System.Globalization;
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

        /// <summary>
        /// This method is called when some criteria is met and the action is invoked.
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="InvalidOperationException">Could not change the target to the specified StateName.</exception>
        protected override void Invoke(object parameter)
        {
            if (this.AssociatedObject != null)
            {
                this.InvokeImpl(this.StateTarget);
            }
        }

        internal void InvokeImpl(FrameworkElement stateTarget)
        {
            if (stateTarget != null)
            {
                VisualStateUtilities.GoToState(stateTarget, this.StateName, this.UseTransitions);
            }
        }

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
        protected override void OnTargetChanged(FrameworkElement oldTarget, FrameworkElement newTarget) 
        { 
            base.OnTargetChanged(oldTarget, newTarget);

            FrameworkElement frameworkElement = null;

            if (string.IsNullOrEmpty(this.TargetName) && !this.IsTargetObjectSet)
            {
                bool successful = VisualStateUtilities.TryFindNearestStatefulControl(this.AssociatedObject as FrameworkElement, out frameworkElement);
                if (!successful && frameworkElement != null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                            "Target '{0}' does not define any VisualStateGroups.",
                                            frameworkElement.Name));
                }
            }
            else
            {
                frameworkElement = this.Target;
            }

            this.StateTarget = frameworkElement;
        }

        private bool IsTargetObjectSet
        {
            get
            {
                bool isLocallySet = this.ReadLocalValue(TargetedTriggerAction.TargetObjectProperty) != DependencyProperty.UnsetValue;
                // if the value can be set indirectly (via trigger, style, etc), should also check ValueSource, but not a concern for behaviors right now.
                return isLocallySet;
            }
        }

        private FrameworkElement StateTarget
        {
            get;
            set;
        }
    }
}

#endif
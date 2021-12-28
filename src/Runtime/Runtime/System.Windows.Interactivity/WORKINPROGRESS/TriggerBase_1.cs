#if !MIGRATION
using Windows.UI.Xaml;
#endif

#if MIGRATION

namespace System.Windows.Interactivity
{
    /// <summary>
    /// Represents an object that can invoke actions conditionally.
    /// </summary>
    /// <typeparam name="T">The type to which this trigger can be attached.</typeparam>
    /// <remarks>
    ///		TriggerBase is the base class for controlling actions. Override OnAttached() and 
    ///		OnDetaching() to hook and unhook handlers on the AssociatedObject. You may 
    ///		constrain the types that a derived TriggerBase may be attached to by specifying 
    ///		the generic parameter. Call InvokeActions() to fire all Actions associated with 
    ///		this TriggerBase.
    ///	</remarks>
    public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBase&lt;T&gt;"/> class.
        /// </summary>
        protected TriggerBase()
            : base(typeof(T))
        {
        }

        /// <summary>
        /// Gets the object to which the trigger is attached.
        /// </summary>
        /// <value>The associated object.</value>
        protected new T AssociatedObject
        {
            get
            {
                return (T)base.AssociatedObject;
            }
        }

        /// <summary>
        /// Gets the type constraint of the associated object.
        /// </summary>
        /// <value>The associated object type constraint.</value>
        protected sealed override Type AssociatedObjectTypeConstraint
        {
            get
            {
                return base.AssociatedObjectTypeConstraint;
            }
        }
    }
}

#endif
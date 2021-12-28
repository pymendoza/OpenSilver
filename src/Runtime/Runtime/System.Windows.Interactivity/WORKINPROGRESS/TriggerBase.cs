#if MIGRATION

namespace System.Windows.Interactivity
{
    public abstract partial class TriggerBase
    {
        private Type associatedObjectTypeConstraint;
        //private static readonly DependencyPropertyKey ActionsPropertyKey = DependencyProperty.RegisterReadOnly("Actions",
        //                                                                                                    typeof(TriggerActionCollection),
        //                                                                                                    typeof(TriggerBase),
        //                                                                                                    new FrameworkPropertyMetadata());

        //private static readonly DependencyPropertyKey ActionsPropertyKey = DependencyProperty.RegisterReadOnly("Actions",
        //                                                                                                    typeof(TriggerActionCollection),
        //                                                                                                    typeof(TriggerBase),
        //                                                                                                    new FrameworkPropertyMetadata());

        //public static readonly DependencyProperty ActionsProperty = ActionsPropertyKey.DependencyProperty;

        internal TriggerBase(Type associatedObjectTypeConstraint)
        {
            this.associatedObjectTypeConstraint = associatedObjectTypeConstraint;
            TriggerActionCollection newCollection = new TriggerActionCollection();
            //this.SetValue(ActionsPropertyKey, newCollection);
            this.SetValue(ActionsProperty, newCollection);
        }


        [OpenSilver.NotImplemented]
        protected virtual Type AssociatedObjectTypeConstraint { get; }

        [OpenSilver.NotImplemented]
        protected virtual void OnDetaching()
        {

        }
    }
}

#endif
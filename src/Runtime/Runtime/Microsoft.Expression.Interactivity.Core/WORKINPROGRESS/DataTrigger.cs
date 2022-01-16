#if MIGRATION

using System.Windows;

namespace Microsoft.Expression.Interactivity.Core
{
	//
	// Summary:
	//     Represents a trigger that performs actions when the bound data meets a specified
	//     condition.
	public partial class DataTrigger : PropertyChangedTrigger
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DataTrigger), new PropertyMetadata(OnValueChanged));

		public static readonly DependencyProperty ComparisonProperty = DependencyProperty.Register("Comparison", typeof(ComparisonConditionType), typeof(DataTrigger), new PropertyMetadata(OnComparisonChanged));

		public DataTrigger()
		{
			
		}

		//
		// Summary:
		//     Gets or sets the type of comparison to be performed between the specified values.
		//     This is a dependency property.
		public ComparisonConditionType Comparison
		{
			get { return (ComparisonConditionType)GetValue(ComparisonProperty); }
			set { SetValue(ComparisonProperty, value); }
		}
		//
		// Summary:
		//     Gets or sets the value to be compared with the property value of the data object.
		//     This is a dependency property.
		public object Value
		{
			get { return (object)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		protected override void OnAttached()
		{
			base.OnAttached();

			//fixes issue #11. We want to evaluate the binding's initial value when the element is first loaded
			if (AssociatedObject is FrameworkElement element)
			{
				element.Loaded += OnElementLoaded;
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			UnsubscribeElementLoadedEvent();
		}

		private void OnElementLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				EvaluateBindingChange(e);
			}
			finally
			{
				UnsubscribeElementLoadedEvent();
			}
		}

		private void UnsubscribeElementLoadedEvent()
		{
			if (AssociatedObject is FrameworkElement element)
			{
				element.Loaded -= OnElementLoaded;
			}
		}

		/// <summary>
		/// Called when the binding property has changed. 
		/// </summary>
		/// <param name="args"><see cref="T:System.Windows.DependencyPropertyChangedEventArgs"/> argument.</param>
		protected override void EvaluateBindingChange(object args)
		{
			if (this.Compare())
			{
				// Fire the actions when the binding data has changed
				this.InvokeActions(args);
			}
		}

		private static void OnValueChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			DataTrigger trigger = (DataTrigger)sender;
			trigger.EvaluateBindingChange(args);
		}

		private static void OnComparisonChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			DataTrigger trigger = (DataTrigger)sender;
			trigger.EvaluateBindingChange(args);
		}

		private bool Compare()
		{
			if (this.AssociatedObject != null)
			{
				return ComparisonLogic.EvaluateImpl(this.Binding, this.Comparison, this.Value);
			}
			return false;
		}
	}
}

#endif
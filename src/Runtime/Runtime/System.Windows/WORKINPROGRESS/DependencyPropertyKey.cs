using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

#if MIGRATION

namespace System.Windows
{
    //
    // Summary:
    //     Provides a dependency property identifier for limited write access to a read-only
    //     dependency property.
    public sealed class DependencyPropertyKey
    {
        private DependencyProperty _dp;

        //
        // Summary:
        //     Gets the dependency property identifier associated with this specialized read-only
        //     dependency property identifier.
        //
        // Returns:
        //     The relevant dependency property identifier.
        public DependencyProperty DependencyProperty => _dp;

        internal DependencyPropertyKey(DependencyProperty dp)
        {
            _dp = dp;
        }

        //
        // Summary:
        //     Overrides the metadata of a read-only dependency property that is represented
        //     by this dependency property identifier.
        //
        // Parameters:
        //   forType:
        //     The type on which this dependency property exists and metadata should be overridden.
        //
        //   typeMetadata:
        //     Metadata supplied for this type.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Attempted metadata override on a read-write dependency property (cannot be done
        //     using this signature).
        //
        //   T:System.ArgumentException:
        //     Metadata was already established for the property as it exists on the provided
        //     type.
        public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
        {
            if (_dp == null)
            {
                throw new InvalidOperationException();
            }

            //_dp.OverrideMetadata(forType, typeMetadata, this);
        }

        internal void SetDependencyProperty(DependencyProperty dp)
        {
            _dp = dp;
        }
    }
}

#endif
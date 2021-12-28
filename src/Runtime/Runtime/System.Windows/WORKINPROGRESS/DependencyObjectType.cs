using System;
using System.Collections.Generic;
using System.Text;

#if MIGRATION

namespace System.Windows
{
    //
    // Summary:
    //     Implements an underlying type cache for all System.Windows.DependencyObject derived
    //     types.
    public class DependencyObjectType
    {
        private int _id;

        private Type _systemType;

        private DependencyObjectType _baseDType;

        private static Dictionary<Type, DependencyObjectType> DTypeFromCLRType = new Dictionary<Type, DependencyObjectType>();

        private static int DTypeCount = 0;

        private static object _lock = new object();

        //
        // Summary:
        //     Gets a zero-based unique identifier for constant-time array lookup operations.
        //
        // Returns:
        //     An internal identifier.
        public int Id => _id;

        //
        // Summary:
        //     Gets the common language runtime (CLR) system type represented by this System.Windows.DependencyObjectType.
        //
        // Returns:
        //     The CLR system type represented by this System.Windows.DependencyObjectType.
        public Type SystemType => _systemType;

        //
        // Summary:
        //     Gets the System.Windows.DependencyObjectType of the immediate base class of the
        //     current System.Windows.DependencyObjectType.
        //
        // Returns:
        //     The type of the base class.
        public DependencyObjectType BaseType => _baseDType;

        //
        // Summary:
        //     Gets the name of the represented common language runtime (CLR) system type.
        //
        // Returns:
        //     The name of the represented CLR system type.
        public string Name => SystemType.Name;

        //
        // Summary:
        //     Returns a System.Windows.DependencyObjectType that represents a given system
        //     (CLR) type.
        //
        // Parameters:
        //   systemType:
        //     The system (CLR) type to convert.
        //
        // Returns:
        //     A System.Windows.DependencyObjectType that represents the system (CLR) type.
        public static DependencyObjectType FromSystemType(Type systemType)
        {
            if (systemType == null)
            {
                throw new ArgumentNullException("systemType");
            }

            //if (!typeof(DependencyObject).IsAssignableFrom(systemType))
            //{
            //    throw new ArgumentException(MS.Internal.WindowsBase.SR.Get("DTypeNotSupportForSystemType", systemType.Name));
            //}

            return FromSystemTypeInternal(systemType);
        }

        //[FriendAccessAllowed]
        internal static DependencyObjectType FromSystemTypeInternal(Type systemType)
        {
            lock (_lock)
            {
                return FromSystemTypeRecursive(systemType);
            }
        }

        private static DependencyObjectType FromSystemTypeRecursive(Type systemType)
        {
            if (!DTypeFromCLRType.TryGetValue(systemType, out DependencyObjectType value))
            {
                value = new DependencyObjectType();
                value._systemType = systemType;
                DTypeFromCLRType[systemType] = value;
                if (systemType != typeof(DependencyObject))
                {
                    value._baseDType = FromSystemTypeRecursive(systemType.BaseType);
                }

                value._id = DTypeCount++;
            }

            return value;
        }

        //
        // Summary:
        //     Determines whether the specified object is an instance of the current System.Windows.DependencyObjectType.
        //
        // Parameters:
        //   dependencyObject:
        //     The object to compare with the current System.Windows.DependencyObjectType.
        //
        // Returns:
        //     true if the class represented by the current System.Windows.DependencyObjectType
        //     is in the inheritance hierarchy of the System.Windows.DependencyObject passed
        //     as d; otherwise, false.
        public bool IsInstanceOfType(DependencyObject dependencyObject)
        {
            return true;

            //if (dependencyObject != null)
            //{
            //    DependencyObjectType dependencyObjectType = dependencyObject.DependencyObjectType;
            //    do
            //    {
            //        if (dependencyObjectType.Id == Id)
            //        {
            //            return true;
            //        }

            //        dependencyObjectType = dependencyObjectType._baseDType;
            //    }
            //    while (dependencyObjectType != null);
            //}

            //return false;
        }

        //
        // Summary:
        //     Determines whether the current System.Windows.DependencyObjectType derives from
        //     the specified System.Windows.DependencyObjectType.
        //
        // Parameters:
        //   dependencyObjectType:
        //     The System.Windows.DependencyObjectType to compare.
        //
        // Returns:
        //     true if the dependencyObjectType parameter and the current System.Windows.DependencyObjectType
        //     represent types of classes, and the class represented by the current System.Windows.DependencyObjectType
        //     derives from the class represented by dependencyObjectType. Otherwise, false.
        //     This method also returns false if dependencyObjectType and the current System.Windows.DependencyObjectType
        //     represent the same class.
        public bool IsSubclassOf(DependencyObjectType dependencyObjectType)
        {
            if (dependencyObjectType != null)
            {
                for (DependencyObjectType baseDType = _baseDType; baseDType != null; baseDType = baseDType._baseDType)
                {
                    if (baseDType.Id == dependencyObjectType.Id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //
        // Summary:
        //     Returns the hash code for this System.Windows.DependencyObjectType.
        //
        // Returns:
        //     A 32-bit signed integer hash code.
        public override int GetHashCode()
        {
            return _id;
        }

        private DependencyObjectType()
        {
        }
    }
}

#endif
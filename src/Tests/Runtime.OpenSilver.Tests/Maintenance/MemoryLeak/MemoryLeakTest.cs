﻿
/*===================================================================================
*
*   Copyright (c) Userware/OpenSilver.net
*
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*
\*====================================================================================*/


using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSilver.Internal.Xaml.Context;

#if MIGRATION
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;
#endif

namespace Runtime.OpenSilver.Tests.Maintenance.MemoryLeak
{
    [TestClass]
    public class MemoryLeakTest
    {
        [TestMethod]
        public void Element_Must_Be_Collected()
        {
            var c = new GarbageCollectorTracker();
            CreateRemoveElement(c);
            CollectGarbage();
            Assert.IsTrue(c.IsCollected);
        }

        [TestMethod]
        public void CoreDispatcher_Should_Release_Callback()
        {
            var c = new GarbageCollectorTracker();
            InvokeCoreDispatcher(c);
            //The next situation is possible:
            //Callback was executed, but the rest of the body in Task.Run(inside CoreDispatcher.BeginInvokeInternal) has not been completed.
            //As a result, the action in Task.Run is not collected and it has a link to ItemWithTrackableCallback.
            //That is why we need to try to collect several times. If we did not collect after 10 attempts with a delay,
            //it means that something is broken.
            for (var i = 0; i < 10; i++)
            {
                CollectGarbage();
                if (c.CollectedResetEvent.WaitOne(100))
                {
                    return;
                }
            }
            Assert.IsTrue(c.IsCollected);
        }

        [TestMethod]
        public void WebBrowser_Must_Be_Collected()
        {
            var c = new GarbageCollectorTracker();
            CreateRemoveWebBrowser(c);
            CollectGarbage();
            Assert.IsTrue(c.IsCollected);
        }

        [TestMethod]
        public void PasswordBoxView_Must_Be_Collected()
        {
            void CreateRemovePasswordBoxView(GarbageCollectorTracker tracker)
            {
                var pwbView = new PasswordBoxViewWithTrackingComponent(tracker);
                Application.Current.MainWindow.Content = pwbView;
                Application.Current.MainWindow.Content = null;
            }

            var c = new GarbageCollectorTracker();
            CreateRemovePasswordBoxView(c);
            CollectGarbage();
            Assert.IsTrue(c.IsCollected);
        }

        [TestMethod]
        public void DependencyObject_Should_Release_InheritedContext()
        {
            var c = new GarbageCollectorTracker();
            CreateDependencyObject(c);
            CollectGarbage();

            Assert.IsTrue(c.IsCollected);

            DependencyObject CreateDependencyObject(GarbageCollectorTracker tracker)
            {
                var depObj = new DependencyObject();
                _ = new MyFrameworkElement(tracker) { MyProperty = depObj };
                return depObj;
            }
        }

        [TestMethod]
        public void FrameworkElement_Should_Release_TemplatedParent()
        {
            var c = new GarbageCollectorTracker();
            var child = CreateFrameworkElementWithTemplateParent(c);
            CollectGarbage();

            Assert.IsTrue(c.IsCollected);

            FrameworkElement CreateFrameworkElementWithTemplateParent(GarbageCollectorTracker tracker)
            {
                var templatedParent = new ControlWithTrackingComponent(tracker)
                {
                    Template = new ControlTemplate
                    {
                        TargetType = typeof(ControlWithTrackingComponent),
                        Template = new TemplateContent(
                        new XamlContext(),
                        (owner, context) => new Border { TemplatedParent = owner }),
                    }
                };
                templatedParent.ApplyTemplate();
                var border = (Border)VisualTreeHelper.GetChild(templatedParent, 0);
                templatedParent.Template = null;
                return border;
            }
        }

        private void CreateRemoveElement(GarbageCollectorTracker c)
        {
            var tc = new FrameworkElementWithTrackingComponent(c);
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Content = tc;
            mainWindow.Content = new Grid();
        }

        private void CreateRemoveWebBrowser(GarbageCollectorTracker c)
        {
            var tc = new WebBrowserWithTrackingComponent(c);
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Content = tc;
            mainWindow.Content = new Grid();
        }

        private void InvokeCoreDispatcher(GarbageCollectorTracker c)
        {
            var resetEvent = new ManualResetEvent(false);
            var trackableCallback = new ItemWithTrackableCallback(c, resetEvent);
#if MIGRATION
            Dispatcher.INTERNAL_GetCurrentDispatcher().BeginInvoke(trackableCallback.Callback);
#else
            CoreDispatcher.INTERNAL_GetCurrentDispatcher().BeginInvoke(trackableCallback.Callback);
#endif
            resetEvent.WaitOne(5000);
        }

        private static void CollectGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private class MyFrameworkElement : FrameworkElementWithTrackingComponent
        {
            public MyFrameworkElement(GarbageCollectorTracker gcTracker)
                : base(gcTracker)
            {
            }

            public static readonly DependencyProperty MyPropertyProperty =
                DependencyProperty.Register(
                    nameof(MyProperty),
                    typeof(object),
                    typeof(MyFrameworkElement),
                    null);

            public object MyProperty
            {
                get => GetValue(MyPropertyProperty);
                set => SetValue(MyPropertyProperty, value);
            }
        }
    }
}

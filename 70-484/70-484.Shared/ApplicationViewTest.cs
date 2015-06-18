using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.ViewManagement;

namespace _70_484
{
    public class ApplicationViewTest
    {
        public ApplicationViewOrientation GetOrientation()
        {
            var view = ApplicationView.GetForCurrentView();
            return view.Orientation;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium
{
    public interface INavigable
    {
        INavigationSolver Navigation { get; set; }

        void OnOpeningAnimationEnd();
    }
}

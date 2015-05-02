using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Component
{
    public class RelayResultCommand<Tin, TResult> : IResultCommand where Tin : class
    {
        private Func<Tin,TResult> _Function;
        public RelayResultCommand(Func<Tin, TResult> iFunction)
        {
            _Function = iFunction;
        }
        public object Execute(object iargument)
        {
            return _Function(iargument as Tin);
        }
    }

    public class RelayResultCommand<TResult> : IResultCommand
    {
        private Func< TResult> _Function;
        public RelayResultCommand(Func<TResult> iFunction)
        {
            _Function = iFunction;
        }
        public object Execute(object iargument)
        {
            return _Function();
        }
    }
}

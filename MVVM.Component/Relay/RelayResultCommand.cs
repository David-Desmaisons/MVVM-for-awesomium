using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Component
{
    public class RelayResultCommand<Tin, TResult> : IResultCommand where Tin : class
    {
        private Func<Tin, Task<TResult>> _ResultBuilder;
        public RelayResultCommand(Func<Tin, TResult> iFunction)
        {
            _ResultBuilder = (iargument) =>
                {
                    var tcs = new TaskCompletionSource<TResult>();
                    tcs.SetResult(iFunction(iargument));
                    return tcs.Task;
                };
        }

        public RelayResultCommand(Func<Tin, Task<TResult>> iResultBuilder)
        {
            _ResultBuilder = iResultBuilder;
        }

        public Task<object> Execute(object iargument)
        {
            return _ResultBuilder(iargument as Tin).ContinueWith<object>(t=>t.Result,TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public class RelayResultCommand<TResult> : IResultCommand
    {

        private Func< TResult> _Function;
        public RelayResultCommand(Func<TResult> iFunction)
        {
            _Function = iFunction;
        }
        public Task<object> Execute(object iargument)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(_Function());
            return tcs.Task;
        }
    }
}

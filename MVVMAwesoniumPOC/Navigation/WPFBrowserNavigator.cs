using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using MVVMAwesomium.Infra;

namespace MVVMAwesomium
{
    public class WPFBrowserNavigator : INavigationBuilder, INavigationSolver
    {
        private WebControl _WebControl;
        private IAwesomiumBindingFactory _IAwesomiumBindingFactory;
        private IAwesomeBinding _IAwesomeBinding;
        private bool _Disposed = false;

        public WPFBrowserNavigator(WebControl iWebControl, IAwesomiumBindingFactory iAwesomiumBindingFactory=null)
        {
            _WebControl = iWebControl;
            _IAwesomiumBindingFactory = iAwesomiumBindingFactory ?? new AwesomiumBindingFactory() { ManageWebSession = false };
        }

        private IAwesomeBinding Binding
        {
            get { return _IAwesomeBinding; }
            set
            {
                if (_IAwesomeBinding != null)
                    _IAwesomeBinding.Dispose();

                _IAwesomeBinding = value;
                if (_Disposed && (_IAwesomeBinding!=null))
                {
                    _IAwesomeBinding.Dispose();
                    _IAwesomeBinding = null;
                }
            }
        }

        private IDictionary<Type, IDictionary<string, Uri>> _Mapper = new Dictionary<Type, IDictionary<string, Uri>>();


        private void Register(Type itype, Uri uri, string id)
        {
            IDictionary<string, Uri> res = _Mapper.FindOrCreateEntity(itype, t => new Dictionary<string, Uri>());
            res.Add(id ?? string.Empty, uri);
        }

       
        public void Register<T>(string iPath, string Id = null)
        {
            Register(typeof(T), new Uri(string.Format("{0}\\{1}", Assembly.GetCallingAssembly().GetPath(), iPath)), Id);
        }

        public void RegisterAbsolute<T>(string iPath, string Id = null)
        {
            Register(typeof(T), new Uri(iPath), Id);
        }

        public void Register<T>(Uri iPath, string Id = null)
        {
            Register(typeof(T), iPath, Id);
        }


        private Uri SolveType(Type iType, string id)
        {
            IDictionary<string, Uri> dicres = null;
            Uri res = null;

            foreach (Type InType in iType.GetBaseTypes())
            {
                if (_Mapper.TryGetValue(InType, out dicres))
                {
                    if (dicres.TryGetValue(id, out res))
                        return res;
                }
            }
            return null;
        }

        public Task Navigate(Uri iUri, object iViewModel, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iUri == null)
                throw new Exception("ViewModel not registered");

            if (OnNavigate != null)
                OnNavigate(this, new NavigationEvent(iViewModel));

            bool needrefresh = _WebControl.Source == iUri;
     
            if ((!_WebControl.IsDocumentReady) || needrefresh)
            {
                if (needrefresh)
                    _WebControl.Reload(false);
                else
                    _WebControl.Source = iUri;
                return _IAwesomiumBindingFactory.Bind(_WebControl, iViewModel, iMode).ContinueWith(t=>Binding = t.Result);
            }
                
            TaskCompletionSource<IAwesomeBinding> tcs = new TaskCompletionSource<IAwesomeBinding>();

            UrlEventHandler sourceupdate = null;
            sourceupdate = (o, e) =>
            {
                _WebControl.AddressChanged -= sourceupdate;
                _IAwesomiumBindingFactory.Bind(_WebControl, iViewModel, iMode).ContinueWith(t =>
                {
                    Binding = t.Result;
                    tcs.SetResult(Binding);
                });
            };

            _WebControl.AddressChanged += sourceupdate;

            _WebControl.Source = iUri;

            return tcs.Task;
        }

        public Task Navigate(object iViewModel, string Id = null, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iViewModel == null)
                return TaskHelper.Ended();

            return Navigate(SolveType(iViewModel.GetType(), Id??string.Empty), iViewModel, iMode);
        }

        public void Dispose()
        {
            _Disposed = true;
            Binding = null;
            UseINavigable = false;
        }


        public event EventHandler<NavigationEvent> OnNavigate;


        private bool _UseINavigable = false;
        public bool UseINavigable
        {
            get
            {
                return _UseINavigable;
            }
            set
            {
                if (_UseINavigable == value)
                    return;

                if (_UseINavigable=value)
                    OnNavigate += WPFBrowserNavigator_OnNavigate;
                else
                    OnNavigate -= WPFBrowserNavigator_OnNavigate;
            }
        }

        private void WPFBrowserNavigator_OnNavigate(object sender, NavigationEvent e)
        {
            INavigable nv = e.ViewModel as INavigable;
            if (nv != null)
                nv.Navigation = this;
        }
    }
}

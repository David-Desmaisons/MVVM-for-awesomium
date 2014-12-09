using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MVVMAwesomium.Binding.AwesomiumBinding
{
    public class PropertyAbstraction : IProperty
    {
        private Func<object, object> _Getter;
        private Action<object, object> _Setter;

        public PropertyAbstraction(PropertyInfo iPo)
        {
            Name = iPo.Name;
            if (!iPo.CanWrite)
                throw new ArgumentException(string.Format("Property error: Property {0 }should be settable",iPo));

            ParameterExpression p = Expression.Parameter(typeof(object), "p");
            var getter = Expression.Call(Expression.Convert(p, iPo.ReflectedType), iPo.GetGetMethod());
            Expression<Func<object, object>> getterbuilder = Expression.Lambda<Func<object, object>>(getter, p);
            _Getter = getterbuilder.Compile();

            if (iPo.CanWrite)
            {
                ParameterExpression p1 = Expression.Parameter(typeof(object), "p1");
                ParameterExpression p2 = Expression.Parameter(typeof(object), "p2");
                var setter = Expression.Call(Expression.Convert(p1, iPo.ReflectedType), iPo.GetSetMethod(), new Expression[] { Expression.Convert(p2, iPo.PropertyType) });

                Expression<Action<object, object>> setterbuilder = Expression.Lambda<Action<object, object>>(setter, p1, p2);
                _Setter = setterbuilder.Compile();
            }
        }

        public string Name { get; private set; }

        public bool IsSettable { get { return _Setter != null; } }

        public object this[object ifather]
        {
            get
            {
                return _Getter(ifather);
            }
            set
            {
                if (_Setter == null)
                    return;

                  _Setter(ifather,value);
            }              
        }
    }
}

﻿namespace tomenglertde.ResXManager.VSIX
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    using tomenglertde.ResXManager.Infrastructure;
    using tomenglertde.ResXManager.Model;

    using TomsToolbox.Core;

    [Export(typeof(Configuration))]
    internal class DteConfiguration : Configuration
    {
        private readonly DteSolution _solution;

        [ImportingConstructor]
        public DteConfiguration(DteSolution solution, ITracer tracer)
            : base(tracer)
        {
            Contract.Requires(solution != null);
            Contract.Requires(tracer != null);

            _solution = solution;
        }

        public override bool IsScopeSupported
        {
            get
            {
                return true;
            }
        }

        public override ConfigurationScope Scope
        {
            get
            {
                return (_solution.Globals != null) ? ConfigurationScope.Solution : ConfigurationScope.Global;
            }
        }

        protected override T GetValue<T>(Expression<Func<T>> propertyExpression, T defaultValue)
        {
            T value;

            return TryGetValue(GetKey(PropertySupport.ExtractPropertyName(propertyExpression)), out value) ? value : base.GetValue(propertyExpression, defaultValue);
        }

        private bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);
            
            return TryGetValue(_solution.Globals, key, ref value);
        }

        private static bool TryGetValue<T>(EnvDTE.Globals globals, string key, ref T value)
        {
            try
            {
                if ((globals != null) && (globals.VariableExists[key]))
                {
                    value = ConvertFromString<T>(globals[key] as string);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        protected override void InternalSetValue<T>(T value, Expression<Func<T>> propertyExpression)
        {
            var globals = _solution.Globals;

            if (globals != null)
            {
                var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
                var key = GetKey(propertyName);

                globals[key] = ConvertToString<T>(value);
                globals.VariablePersists[key] = true;

                OnPropertyChanged(propertyName);
            }
            else
            {
                base.InternalSetValue(value, propertyExpression);
            }
        }

        private static string GetKey(string propertyName)
        {
            return "RESX_" + propertyName;
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_solution != null);
        }
    }
}
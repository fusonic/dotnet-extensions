using System;
using System.Collections.Generic;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.SimpleInjector
{
    /// <summary>
    /// Lifestyle per test context.
    /// The whole thing is based on SimpleInjector.Integration.Web.WebRequestLifestyle, which fortunately does the exact same thing. It just uses HttpContext.Current instead of TestContext.Current.
    /// </summary>
    public sealed class TestScopedLifestyle : ScopedLifestyle
    {
        private static readonly object scopeKey = new object();

        public TestScopedLifestyle() : base(nameof(TestScopedLifestyle))
        { }

        internal static void CleanupTestScopes()
        {
            var scopes = (List<Scope>)TestContext.Items[scopeKey];

            if (!(scopes is null))
                DisposeScopes(scopes);
        }

        protected override Scope? GetCurrentScopeCore(Container container) => GetOrCreateScope(container);
        protected override Func<Scope?> CreateCurrentScopeProvider(Container container) => () => GetOrCreateScope(container);

        private static Scope? GetOrCreateScope(Container container)
        {
            var context = TestContext.Current;
            if (context is null)
                return null;

            var scopes = (List<Scope>)context.Items[scopeKey];
            if (scopes is null)
                context.Items[scopeKey] = scopes = new List<Scope>(capacity: 2);

            var scope = FindScopeForContainer(scopes, container);
            if (scope is null)
                scopes.Add(scope = new Scope(container));

            return scope;
        }

        private static Scope? FindScopeForContainer(List<Scope> scopes, Container container)
        {
            foreach (var scope in scopes)
            {
                if (scope.Container == container)
                    return scope;
            }

            return null;
        }

        private static void DisposeScopes(List<Scope> scopes)
        {
            if (scopes.Count == 1)
                scopes[0].Dispose();
            else if (scopes.Count > 1)
                DisposeScopesInReverseOrder(scopes);
        }

        private static void DisposeScopesInReverseOrder(List<Scope> scopes)
        {
            // Here we use a 'master' scope that will hold the real scopes. This allows all scopes
            // to be disposed, even if a scope's Dispose method throws an exception. Scopes will
            // also be disposed in opposite order of creation.
            using var masterScope = new Scope(scopes[0].Container!);
            foreach (var scope in scopes)
            {
                masterScope.RegisterForDisposal(scope);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Hangfire;

namespace Hangfire
{
    public class UnityJobActivator : JobActivator
    {
        private readonly IDependencyResolver _dependencyResolver;

        /// <summary>
        /// Initialize a new instance of the <see cref="T:UnityJobActivator"/> class
        /// </summary>
        /// <param name="container">The unity container to be used</param>

        public UnityJobActivator(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }


        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return _dependencyResolver.GetService(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new UnityScope(_dependencyResolver.BeginScope());
        }

        class UnityScope : JobActivatorScope
        {
            private readonly IDependencyScope _dependencyScope;

            public UnityScope(IDependencyScope dependencyScope)
            {
                _dependencyScope = dependencyScope;
            }

            public override object Resolve(Type type)
            {
                return _dependencyScope.GetService(type);
            }

            public override void DisposeScope()
            {
                _dependencyScope.Dispose();
            }
        }
    }
}
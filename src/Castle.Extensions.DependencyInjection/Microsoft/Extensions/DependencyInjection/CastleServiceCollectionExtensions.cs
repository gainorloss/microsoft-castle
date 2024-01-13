using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class CastleCoreServiceCollectionExtensions
    {
        /// <summary>
        /// 因为是使用替换的方式，所以必须要放到最后 galoS@2024-1-11 19:22:28
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection ConfigureCastleDynamicProxy(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ProxyGenerator>();

            var descriptors = services.Where(svc => IsInterceptorType(svc.ServiceType)).ToList();

            foreach (var descriptor in descriptors)
            {
                var lifetime = descriptor.Lifetime;

                if (descriptor.ImplementationType != null)
                {
                    services.AddByLifetime(descriptor.ServiceType
                        , sp => BuildProxy(sp, descriptor.ServiceType, descriptor.ImplementationType)
                        , lifetime);
                    services.Remove(descriptor);
                    continue;
                }
                if (descriptor.ImplementationInstance != null)
                {
                    services.AddByLifetime(descriptor.ServiceType
                        , sp => BuildProxy(sp, descriptor.ServiceType, descriptor.ImplementationInstance.GetType())
                        , lifetime);
                    services.Remove(descriptor);
                    continue;
                }
                if (descriptor.ImplementationFactory != null)
                {
                    services.AddByLifetime(descriptor.ServiceType
                        , sp => BuildProxy(sp, descriptor.ServiceType, descriptor.ImplementationFactory(sp).GetType())
                        , lifetime);
                    services.Remove(descriptor);
                    continue;
                }
            }

            return services;
        }

        private static object BuildProxy(IServiceProvider sp, Type serviceType, Type? implementationType)
        {
            var generator = sp.GetRequiredService<ProxyGenerator>();
            #region 当前服务类是否有注入 galoS@2024-1-11 17:53:14
            var constructorArgs = new object[] { };
            if (implementationType.GetConstructors().Any(i => i.GetParameters().Any()))
            {
                var paraTypes = implementationType.GetConstructors().Where(i => i.GetParameters().Any()).SelectMany(i => i.GetParameters().Select(para => para.ParameterType));
                constructorArgs = paraTypes.Select(i => sp.GetRequiredService(i)).ToArray();
            }
            #endregion

            var interceptors = GetInterceptors(serviceType, sp);//获取拦截器 galoS@2024-1-12 14:47:47

            var proxy = serviceType.IsClass
            ? generator.CreateClassProxy(serviceType, constructorArgs, interceptors.ToArray())
            : generator.CreateInterfaceProxyWithTarget(serviceType
            , ActivatorUtilities.CreateInstance(sp, implementationType, constructorArgs)//ActivatorUtilities 获取 实例 galoS@2024-1-13 21:40:58
            , interceptors.ToArray());
            return proxy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceCollection AddByLifetime(
            this IServiceCollection services,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(serviceType, implementationFactory);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(serviceType, implementationFactory);
                    break;
                case ServiceLifetime.Transient:
                default:
                    services.AddTransient(serviceType, implementationFactory);
                    break;
            }
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsInterceptorType(Type type)//查找：标注InterceptorAttribute和标注InterceptorBaseAttribute子类特性的类型 galoS@2024年1月12日14:06:21
        {
            var @is = (type.IsClass && type.GetMethods().Any(i => i.GetCustomAttributes().Any(i => IsInterceptorAttibuteType(i.GetType()))))
             || (type.IsInterface && type.GetMethods().Any(i => i.GetCustomAttributes().Any(i => IsInterceptorAttibuteType(i.GetType()))));
            return @is;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static IEnumerable<IInterceptor> GetInterceptors(Type type, IServiceProvider provider)//查找：标注InterceptorAttribute和标注InterceptorBaseAttribute子类特性的类型 galoS@2024年1月12日14:06:21
        {
            var annotations = GetInterceptorAnnotations(type);
            var annotationInterceptors = annotations.Where(i => i.GetType().IsAssignableTo(typeof(InterceptorBaseAttribute))).Cast<IInterceptor>();
            var serviceInterceptors = annotations.Where(i => i.GetType() == typeof(InterceptorAttribute)).Cast<InterceptorAttribute>().Select(i => provider.GetRequiredService(i.InterceptorType) as IInterceptor);
            return annotationInterceptors.Concat(serviceInterceptors);
        }

        private static IEnumerable<Attribute> GetInterceptorAnnotations(Type type)
        {
            return type.GetMethods().SelectMany(i => i.GetCustomAttributes().Where(i => IsInterceptorAttibuteType(i.GetType())));
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        private static bool IsInterceptorAttibuteType(Type attributeType)//查找：标注InterceptorAttribute和标注InterceptorBaseAttribute子类特性的类型 galoS@2024年1月12日14:06:21
        {
            return attributeType == typeof(InterceptorAttribute) || attributeType.IsAssignableTo(typeof(InterceptorBaseAttribute));
        }
    }
}

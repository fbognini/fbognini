using AutoMapper;
using fbognini.Common.Application.Mappings;
using fbognini.Core.Data;
using fbognini.Core.Mappings;
using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace fbognini.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            services.AddAutoMapper(config =>
            {
                config.AddCustomMappingProfile(assembly);
            });

            services.AddMediatR(assembly);
            services.AddMediatRBehaviors();
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }

        public static IServiceCollection AddMediatRBehaviors(this IServiceCollection services, bool logging = true, bool validation = false)
        {
            if (logging)
            {
                services.AddTransient(typeof(IRequestPreProcessor<>), typeof(RequestLogger<>));
            }

            if (validation)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            }

            return services;
        }


        public static void AddCustomMappingProfile<TTypeMarker>(this IMapperConfigurationExpression config)
        {
            config.AddCustomMappingProfile(typeof(TTypeMarker));
        }

        public static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Type[] typeMarkers)
        {
            config.AddCustomMappingProfile(typeMarkers.Select(t => t.GetTypeInfo().Assembly).ToArray());
        }

        public static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Assembly[] assemblies)
        {
            var allTypes = assemblies.SelectMany(a => a.ExportedTypes);

            var list = allTypes
                .Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces().Contains(typeof(IHaveCustomMapping)))
                .Select(type => (IHaveCustomMapping)Activator.CreateInstance(type));

            var profile = new CustomMappingProfile(list);

            config.AddProfile(profile);
        }
    }

}

using AutoMapper;

namespace fbognini.Core.Mappings
{
    public abstract class Mappable<TEntity1, TEntity2> : IHaveCustomMapping
    {
        public void CreateMappings(Profile profile)
        {
            var exp = profile.CreateMap<TEntity1, TEntity2>();
            var reverse = profile.CreateMap<TEntity2, TEntity1>();

            CustomMappings(exp);
            CustomMappings(reverse);
        }

        public virtual void CustomMappings(IMappingExpression<TEntity1, TEntity2> mapping)
        {
        }

        public virtual void CustomMappings(IMappingExpression<TEntity2, TEntity1> mapping)
        {
        }
    }

    public abstract class Mappable<TSource, TDestination1, TDestination2> : Mappable<TSource, TDestination1>
    {
        public new void CreateMappings(Profile profile)
        {
            var exp = profile.CreateMap<TSource, TDestination2>();
            var reverse = profile.CreateMap<TDestination2, TSource>();

            CustomMappings(exp);
            CustomMappings(reverse);

            base.CreateMappings(profile);
        }

        public virtual void CustomMappings(IMappingExpression<TSource, TDestination2> mapping)
        {
        }

        public virtual void CustomMappings(IMappingExpression<TDestination2, TSource> mapping)
        {
        }
    }

    public abstract class Mappable<TSource, TDestination1, TDestination2, TDestination3> : Mappable<TSource, TDestination1, TDestination2>
    {
        public new void CreateMappings(Profile profile)
        {
            var exp = profile.CreateMap<TSource, TDestination3>();
            var reverse = profile.CreateMap<TDestination3, TSource>();

            CustomMappings(exp);
            CustomMappings(reverse);

            base.CreateMappings(profile);
        }

        public virtual void CustomMappings(IMappingExpression<TSource, TDestination3> mapping)
        {
        }

        public virtual void CustomMappings(IMappingExpression<TDestination3, TSource> mapping)
        {
        }
    }

    public abstract class Mappable<TSource, TDestination1, TDestination2, TDestination3, TDestination4> : Mappable<TSource, TDestination1, TDestination2, TDestination3>
    {
        public new void CreateMappings(Profile profile)
        {
            var exp = profile.CreateMap<TSource, TDestination4>();
            var reverse = profile.CreateMap<TDestination4, TSource>();

            CustomMappings(exp);
            CustomMappings(reverse);

            base.CreateMappings(profile);
        }

        public virtual void CustomMappings(IMappingExpression<TSource, TDestination4> mapping)
        {
        }

        public virtual void CustomMappings(IMappingExpression<TDestination4, TSource> mapping)
        {
        }
    }
}

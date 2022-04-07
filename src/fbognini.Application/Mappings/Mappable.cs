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
}

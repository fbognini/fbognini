using AutoMapper;
using fbognini.Core.Mappings;
using System.Collections.Generic;

namespace fbognini.Common.Application.Mappings
{
    public class CustomMappingProfile : Profile
    {
        public CustomMappingProfile()
        {
            AllowNullCollections = true;
        }

        public CustomMappingProfile(IEnumerable<IHaveCustomMapping> haveCustomMappings)
            : this()
        {
            foreach (var item in haveCustomMappings)
                item.CreateMappings(this);
        }
    }

    
}

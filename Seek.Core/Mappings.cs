using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Seek.Core.Dtos;
using Seek.Core.Models.Auth;

namespace Seek.Core
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            #region-> auth_structure
            #region-> user_model
            CreateMap<auth_model, auth_responses_dto>()
                    .ForMember(global => global.Token, opt => opt.MapFrom(src => src.HashedToken))
                    .ForMember(global => global.Refresh_Token, opt => opt.MapFrom(src => src.Hashed_Refresh_Token));

            CreateMap<auth_login_dto,auth_model>()
                    .ForMember(global => global.HashedLogin, opt => opt.MapFrom(src => src.Login))
                    .ForMember(global => global.HashedPassword, opt => opt.MapFrom(src => src.Password));

            CreateMap<auth_register_dto, auth_model>()
                       .ForMember(global => global.HashedLogin, opt => opt.MapFrom(src => src.Login))
                       .ForMember(global => global.HashedPassword, opt => opt.MapFrom(src => src.Password));

                #endregion
            #endregion
        }
    }
}

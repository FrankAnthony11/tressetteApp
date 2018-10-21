using AutoMapper;
using TresetaApp.Models;
using TresetaApp.Models.Dtos;

namespace TresetaApp.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Player, PlayerDto>();

            CreateMap<Game, GameDto>()
                .ForMember(dest => dest.DeckSize, opt =>
                {
                    opt.MapFrom(src => src.Deck.Count);
                });

            CreateMap<Team, TeamDto>();

            CreateMap<GameSetup, GameSetupDto>().ForMember(dest => dest.IsPasswordProtected, opt =>
            {
                opt.MapFrom(src => src.Password.Length > 0);
            });
        }
    }
}
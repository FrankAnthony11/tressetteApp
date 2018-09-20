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
        }
    }
}
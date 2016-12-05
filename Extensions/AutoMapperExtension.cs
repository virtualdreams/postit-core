using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using notes.Core.Models;
using notes.Helper;
using notes.Models;

namespace notes.Extensions
{
    static public class AutoMapperExtensions
	{
		static public IServiceCollection AddAutoMapper(this IServiceCollection services)
		{
			var _autoMapperConfig = new MapperConfiguration(config =>{
				config.CreateMap<Note, NoteModel>()
					.ForMember(d => d.Tags, map => map.MapFrom(s => String.Join(", ", s.Tags ?? new string[] {})))
					.ForMember(d => d.Age, map => map.MapFrom(s => s.Id.CreationTime.ToLocalTime().ToMinutes().ToWords()));

				config.CreateMap<User, UserModel>();
			});

			_autoMapperConfig.AssertConfigurationIsValid();

			services.AddSingleton<IMapper>(am => _autoMapperConfig.CreateMapper());

			return services;
		}
	}
}
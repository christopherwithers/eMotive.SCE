using AutoMapper;
using eMotive.Services.Objects;
using Email = eMotive.Models.Objects.Email.Email;


namespace eMotive.Services
{
    public class AutoMapperServicesConfiguration
    {
        public static void Configure()
        {
            ConfigureEmailMapping();
        }

        private static void ConfigureEmailMapping()
        {
            Mapper.CreateMap<Email, EditableEmail>();
            Mapper.CreateMap<EditableEmail, Email>();//.ForMember(m => m.Message, o => o.MapFrom(n => System.Net.WebUtility.HtmlDecode(n.Message)));
        }
    }
}

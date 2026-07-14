using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineJobRecruitmentSystem.Application.Profiles;
using Xunit;

namespace OnlineJobRecruitmentSystem.Tests.Profiles
{
    public class MapperProfileTests
    {
        [Fact]
        public void MapperProfile_Configuration_IsValid()
        {
            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<MapperProfile>(),
                NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
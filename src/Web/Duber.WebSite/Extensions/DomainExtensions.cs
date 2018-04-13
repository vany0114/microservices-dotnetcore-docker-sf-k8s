using System.Collections.Generic;
using System.Linq;
using Duber.Domain.Driver.Model;
using Duber.Domain.User.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Duber.WebSite.Extensions
{
    public static class DomainExtensions
    {
        public static List<SelectListItem> ToSelectList(this IList<User> list)
        {
            return list.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
        }

        public static List<SelectListItem> ToSelectList(this IList<Driver> list)
        {
            return list.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Duber.Domain.Driver.Model;
using Duber.Domain.User.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    public static class MvcExtensions
    {
        public static IEnumerable<string> AllErrors(this ModelStateDictionary modelState)
        {
            var result = new List<string>();
            var erroneousFields = modelState.Where(ms => ms.Value.Errors.Any())
                .Select(x => new { x.Value.Errors });

            foreach (var erroneousField in erroneousFields)
            {
                var fieldErrors = erroneousField.Errors
                    .Select(error => error.ErrorMessage);
                result.AddRange(fieldErrors);
            }

            return result;
        }
    }
}

using System.Web.Mvc;

namespace TourDulich.Areas.Admin.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var quyenSession = session["Quyen"];
            bool laAdmin = quyenSession != null && int.TryParse(quyenSession.ToString(), out int quyen) && quyen == 0;

            if (session["NguoiDung"] == null || !laAdmin)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(403, "Bạn không có quyền truy cập.");
                    return;
                }

                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Dangnhap", action = "Index", area = "Admin" }
                    )
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}

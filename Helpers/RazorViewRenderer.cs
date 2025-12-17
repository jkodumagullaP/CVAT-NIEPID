using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;

namespace CAT.AID.Web.Helpers
{
    public static class RazorViewRenderer
    {
        public static async Task<string> RenderViewToStringAsync(
            Controller controller,
            string viewPath,
            object model)
        {
            controller.ViewData.Model = model;

            using var sw = new StringWriter();

            var services = controller.HttpContext.RequestServices;
            var engine = services.GetRequiredService<ICompositeViewEngine>();

            var viewResult = engine.GetView(null, viewPath, false);

            if (!viewResult.Success)
                throw new FileNotFoundException($"View not found: {viewPath}");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}

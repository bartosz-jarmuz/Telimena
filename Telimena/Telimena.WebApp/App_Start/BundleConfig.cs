using System.Web.Optimization;

namespace Telimena.WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css", 
                "~/Content/site.css",
                "~/Content/font-awesome.css"
                ,"~/Content/Ionicons/css/ionicons.min.css",
                "~/admin-lte/css/AdminLTE.css", 
                "~/admin-lte/css/skins/_all-skins.min.css"
                ,"~/admin-lte/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css", 
                "~/admin-lte/plugins/iCheck/all.min.css"));

            bundles.Add(new ScriptBundle("~/adminlte/js").Include(
                "~/admin-lte/js/adminlte.min.js"
                , "~/admin-lte/js/control-menu.js"
                , "~/admin-lte/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js"
                , "~/admin-lte/plugins/iCheck/icheck.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/telimena").Include(
                 "~/Scripts/telimena-common.js"));

        }
    }
}
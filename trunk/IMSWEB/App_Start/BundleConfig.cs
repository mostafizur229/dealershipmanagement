using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;

namespace IMSWEB
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.VirtualPathProvider = new ScriptBundlePathProvider(HostingEnvironment.VirtualPathProvider);

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/bower_components/jquery/dist/jquery.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/bower_components/bootstrap/dist/js/bootstrap.min.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/otherjs").Include(
                      "~/bower_components/jquery-ui/jquery-ui.min.js",
                      "~/bower_components/raphael/raphael.min.js",
                      "~/bower_components/morris.js/morris.min.js",
                      "~/bower_components/jquery-sparkline/dist/jquery.sparkline.min.js",
                      "~/plugins/jvectormap/jquery-jvectormap-1.2.2.min.js",
                      "~/plugins/jvectormap/jquery-jvectormap-world-mill-en.js",
                      "~/bower_components/jquery-knob/dist/jquery.knob.min.js",
                      "~/bower_components/moment/min/moment.min.js",
                      "~/bower_components/bootstrap-daterangepicker/daterangepicker.js",
                      "~/bower_components/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js",
                      "~/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js",
                      "~/bower_components/jquery-slimscroll/jquery.slimscroll.min.js",
                      "~/bower_components/fastclick/lib/fastclick.js",
                      "~/bower_components/datatables.net/js/jquery.dataTables.min.js",
                      "~/bower_components/datatables.net-bs/js/dataTables.bootstrap.min.js",
                      "~/dist/js/adminlte.min.js",
                      "~/bower_components/chart.js/Chart.js",
                      "~/Scripts/jquery.unobtrusive-ajax.min.js",
                      "~/dist/js/demo.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/bower_components/bootstrap/dist/css/bootstrap.min.css",
                      "~/bower_components/font-awesome/css/font-awesome.min.css",
                      "~/bower_components/Ionicons/css/ionicons.min.css",
                      "~/bower_components/morris.js/morris.css",
                      "~/bower_components/jvectormap/jquery-jvectormap.css",
                      "~/bower_components/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css",
                      "~/bower_components/bootstrap-daterangepicker/daterangepicker.css",
                      "~/bower_components/datatables.net-bs/css/dataTables.bootstrap.min.css",
                      "~/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css",
                      "~/dist/css/AdminLTE.min.css",
                      "~/dist/css/skins/_all-skins.min.css",
                      "~/dist/css/style.css"
                      ));

            bundles.Add(new StyleBundle("~/Login/Css").Include(
                        "~/bower_components/bootstrap/dist/css/bootstrap.min.css",
                        "~/bower_components/font-awesome/css/font-awesome.min.css",
                        "~/bower_components/Ionicons/css/ionicons.min.css",
                        "~/dist/css/AdminLTE.min.css",
                        "~/plugins/iCheck/square/blue.css"
                        ));
            bundles.Add(new ScriptBundle("~/Login/Loginjs").Include(
                      "~/plugins/iCheck/icheck.min.js"
                      ));


            #region Admin Dashboard CSS

            bundles.Add(new StyleBundle("~/Content/adminCss").Include(
                    //"~/Content/assets/libs/air-datepicker/css/datepicker.min.css",
                    "~/bower_components/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css",
                    "~/Content/assets/libs/jqvmap/jqvmap.min.css",
                    "~/Content/assets/libs/alertifyjs/build/css/alertify.min.css",
                    "~/Content/assets/libs/alertifyjs/build/css/themes/default.min.css",
                    "~/Content/assets/css/bootstrap.min.css",
                    "~/Content/assets/css/icons.min.css",
                    "~/Content/select2/select2.min.css",
                    "~/Content/select2/select2-bootstrap.min.css",
                    "~/Content/assets/css/app.min.css",
                    "~/Content/assets/css/theme-style-override.css"
                    ));
            #endregion

            #region Admin Dashboard JS

            bundles.Add(new ScriptBundle("~/bundles/adminjs").Include(
                      "~/Content/assets/libs/jquery/jquery.min.js",
                      "~/Content/assets/libs/bootstrap/js/bootstrap.bundle.min.js",
                      "~/Content/assets/libs/metismenu/metisMenu.min.js",
                      "~/Content/assets/libs/simplebar/simplebar.min.js"
                      ));
            bundles.Add(new ScriptBundle("~/bundles/adminjs2").Include(
                     //"~/Content/assets/libs/air-datepicker/js/datepicker.min.js",
                     //"~/Content/assets/libs/air-datepicker/js/i18n/datepicker.en.js",
                     "~/Content/assets/libs/alertifyjs/build/alertify.min.js",
                     "~/Content/assets/js/pages/alertifyjs.init.js",
                     "~/Content/assets/libs/jquery-knob/jquery.knob.min.js",
                     "~/Content/assets/libs/jqvmap/jquery.vmap.min.js",
                     "~/Content/assets/libs/jqvmap/maps/jquery.vmap.usa.js",
                     "~/Content/assets/js/pages/dashboard.init.js",
                     "~/bower_components/jquery-ui/jquery-ui.min.js",
                     //"~/Scripts/jquery-ui.js",
                     "~/bower_components/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js",
                     "~/Scripts/jquery.unobtrusive-ajax.min.js",
                     "~/Scripts/select2/select2.min.js",
                     "~/Content/assets/js/app.js"
                     ));
            #endregion

            #region Admin Login CSS
            bundles.Add(new StyleBundle("~/Content/adminLoginCss").Include(
                    "~/Content/assets/css/bootstrap.min.css",
                    "~/Content/assets/css/icons.min.css",
                    "~/Content/assets/css/app.min.css"
                    ));
            #endregion

            #region Admin Login JS
            bundles.Add(new ScriptBundle("~/bundles/adminLoginjs").Include(
                    "~/Content/assets/libs/jquery/jquery.min.js",
                    "~/Content/assets/libs/bootstrap/js/bootstrap.bundle.min.js",
                    "~/Content/assets/libs/metismenu/metisMenu.min.js",
                    "~/Content/assets/libs/simplebar/simplebar.min.js",
                    "~/Content/assets/libs/node-waves/waves.min.js"
                    ));
            #endregion

            #region Admin Databale CSS

            bundles.Add(new StyleBundle("~/Content/dataTableCss").Include(
                "~/Content/assets/libs/datatables.net-bs4/css/dataTables.bootstrap4.min.css",
                "~/Content/assets/libs/datatables.net-buttons-bs4/css/buttons.bootstrap4.min.css",
                "~/Content/assets/libs/datatables.net-responsive-bs4/css/responsive.bootstrap4.min.css"
                ));
            #endregion

            #region Admin Databale CSS

            bundles.Add(new ScriptBundle("~/bundles/dataTablejs").Include(
                    "~/Content/assets/libs/datatables.net/js/jquery.dataTables.min.js",
                    "~/Content/assets/libs/datatables.net-bs4/js/dataTables.bootstrap4.min.js",
                    "~/Content/assets/libs/datatables.net-buttons/js/dataTables.buttons.min.js",
                    "~/Content/assets/libs/datatables.net-buttons-bs4/js/buttons.bootstrap4.min.js",
                    "~/Content/assets/libs/datatables.net-buttons/js/buttons.html5.min.js",
                    "~/Content/assets/libs/datatables.net-buttons/js/buttons.print.min.js",
                    "~/Content/assets/libs/datatables.net-buttons/js/buttons.colVis.min.js",
                    "~/Content/assets/libs/datatables.net-responsive/js/dataTables.responsive.min.js",
                    "~/Content/assets/libs/datatables.net-responsive-bs4/js/responsive.bootstrap4.min.js",
                    "~/Content/assets/js/pages/datatables.init.js"
                    ));
            #endregion

            BundleTable.EnableOptimizations = false;
        }

        //private static void AddAppBundle(BundleCollection bundles)
        //{
        //    var preDOMScriptBundle = new ScriptBundle("~/IMSPreDOMScripts");
        //    var postDOMScriptBundle = new ScriptBundle("~/IMSPostDOMScripts");
        //    var styleBundle = new ScriptBundle("~/IMSStyles");
        //    var scriptDir = "Scripts";
        //    var styleDir = "Content";
        //    string SidebarDir = "SideBar";
        //    var adminAppDirFullPath = HttpContext.Current.Server.MapPath(string.Format("~/{0}", scriptDir));

        //    if (Directory.Exists(adminAppDirFullPath))
        //    {
        //        preDOMScriptBundle.Include(
        //            string.Format("~/{0}/jquery.js", scriptDir),
        //            string.Format("~/{0}/jquery-ui.js", scriptDir),
        //            string.Format("~/{0}/jquery.ui.autocomplete.scroll.js", scriptDir),
        //            string.Format("~/{0}/fusion-chart/fusioncharts.js", scriptDir),
        //            string.Format("~/{0}/fusion-chart/fusioncharts.charts.js", scriptDir),
        //            string.Format("~/{0}/fusion-chart/fusioncharts.theme.fint.js", scriptDir),
        //            string.Format("~/{0}/fusion-chart/fusioncharts-jquery-plugin.js", scriptDir),
        //            string.Format("~/{0}/toastr/toastr.js", scriptDir),
        //            string.Format("~/{0}/data-table/bootstrap-table.min.js", scriptDir),
        //            string.Format("~/{0}/data-table/tableExport.js", scriptDir),
        //            string.Format("~/{0}/data-table/bootstrap-table-export.js", scriptDir),
        //            string.Format("~/{0}/datetime-picker/moment-with-locales.min.js", scriptDir),
        //            string.Format("~/{0}/datetime-picker/bootstrap-datetimepicker.min.js", scriptDir),
        //            string.Format("~/{0}/select2/select2.min.js", scriptDir));

        //        postDOMScriptBundle.Include(
        //            string.Format("~/{0}/jquery.validate*", scriptDir),
        //            string.Format("~/{0}/jquery.unobtrusive-ajax.min.js", scriptDir),
        //            string.Format("~/{0}/modernizr-*", scriptDir),
        //            string.Format("~/{0}/bootstrap.js", scriptDir),
        //            string.Format("~/{0}/respond.js", scriptDir),
        //            string.Format("~/{0}/site.js", scriptDir),
        //            string.Format("~/{0}/js/SideBar.js", SidebarDir));
        //    }

        //    adminAppDirFullPath = HttpContext.Current.Server.MapPath(string.Format("~/{0}", styleDir));
        //    if (Directory.Exists(adminAppDirFullPath))
        //    {
        //        styleBundle.Include(
        //            string.Format("~/{0}/bootstrap.css", styleDir),
        //            string.Format("~/{0}/font-awesome.min.css", styleDir),
        //            string.Format("~/{0}/toastr/toastr.css", styleDir),
        //            string.Format("~/{0}/data-table/bootstrap-table.min.css", styleDir),
        //            string.Format("~/{0}/datetime-picker/bootstrap-datetimepicker.min.css", styleDir),
        //            string.Format("~/{0}/select2/select2.min.css", styleDir),
        //            string.Format("~/{0}/select2/select2-bootstrap.min.css", styleDir),
        //            string.Format("~/{0}/site.css", styleDir),
        //             string.Format("~/{0}/css/SideBar.css", SidebarDir)
        //            );
        //    }

        //    bundles.Add(preDOMScriptBundle);
        //    bundles.Add(postDOMScriptBundle);
        //    bundles.Add(styleBundle);
        //}
    }

    class ScriptBundlePathProvider : VirtualPathProvider
    {
        private readonly VirtualPathProvider _virtualPathProvider;

        public ScriptBundlePathProvider(VirtualPathProvider virtualPathProvider)
        {
            _virtualPathProvider = virtualPathProvider;
        }

        public override bool FileExists(string virtualPath)
        {
            return _virtualPathProvider.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return _virtualPathProvider.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return _virtualPathProvider.GetDirectory(virtualDir);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            return _virtualPathProvider.DirectoryExists(virtualDir);
        }
    }
}

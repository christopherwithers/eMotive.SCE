using System.Web.Optimization;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(eMotive.SCE.App_Start.BundleConfig), "RegisterBundles")]

namespace eMotive.SCE.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles()
        {
            // Add @Styles.Render("~/Content/bootstrap") in the <head/> of your _Layout.cshtml view
            // Add @Scripts.Render("~/bundles/bootstrap") after jQuery in your _Layout.cshtml view
            // When <compilation debug="true" />, MVC4 will render the full readable version. When set to <compilation debug="false" />, the minified version will be rendered automatically
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/jquery*", "~/Scripts/bootstrap.ajax.js","~/Scripts/bootstrap*", "~/Scripts/jquery.gritter.min.js", "~/Scripts/jscolor.js", "~/Scripts/Json2.js", /*"~/Scripts/jquery.blockUi*",*/ "~/Scripts/moment.js"));
            //  BundleTable.Bundles.Add(new ScriptBundle("~/bundles/knockout").Include("~/Scripts/knockout*", ));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/bootstrap.min.css", "~/Content/bootstrap-responsive.min.css", "~/Content/jquery.gritter.css"));


            // BundleTable.Bundles.IgnoreList.Clear();
            // AddDefaultIgnorePatterns(BundleTable.Bundles.IgnoreList);

            BundleTable.Bundles.Add(new ScriptBundle("~/Scripts/Frameworks").Include(
                "~/Scripts/jquery*",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/jquery.blockUI.js",
                "~/Scripts/bootstrap.Ajax.js"));

            BundleTable.Bundles.Add(new ScriptBundle("~/Scripts/Contensis/Core").Include(
                "~/Scripts/Contensis/WebResource1.js",
                "~/Scripts/Contensis/navigation.js",
                "~/Scripts/Contensis/corefunctionsRevised.js",
                "~/Scripts/Contensis/Searchbox.js"
                ));

            BundleTable.Bundles.Add(new StyleBundle("~/Content/ContensisCss/Styles").Include(
                "~/Content/ContensisCss/0000reset.css",
                "~/Content/ContensisCss/0100style.css",
                "~/Content/ContensisCss/0200navi.css",
                "~/Content/ContensisCss/0333conversion.css"));

            BundleTable.Bundles.Add(new StyleBundle("~/Content/Frameworks").Include(
                "~/Content/bootstrap-responsive.min.css",
                "~/Content/bootstrap.min.css",
                "~/Content/jquery-ui.css"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using IMSWEB.Model;
using IMSWEB.Service;
using Autofac;
using IMSWEB.Data;
using Microsoft.AspNet.Identity;

namespace IMSWEB
{
    public static class HtmlNavigationMenuExtension
    {
        static List<MenuItem> _menuItems;

        public static string BuildMenuNavigation(this HtmlHelper helper)
        {
     
            if (helper.ViewContext.RouteData.Values["controller"].ToString() == "Account" &&
                helper.ViewContext.RouteData.Values["action"].ToString() == "Login")
            {
                return "";
            }

            // Session check
            if (HttpContext.Current.Session["NavigationMenu"] == null)
                GetMenuByUserRole();

            _menuItems =
                (List<MenuItem>)HttpContext.Current.Session["NavigationMenu"];

            if (_menuItems == null)
                return "";

            string currentController =
                helper.ViewContext.RouteData.Values["controller"]
                .ToString()
                .ToLower();

            StringBuilder navigationMenu = new StringBuilder();

            // =========================
            // Sidebar Start
            // =========================

            navigationMenu.Append(@"

<ul class='metismenu list-unstyled side-menu'
    id='side-menu'>

    <li class='menu-title'>
        MENU
    </li>

");

            // =========================
            // Dashboard Menu First
            // =========================

            bool dashboardActive =
                currentController.Contains("dashboard");

            navigationMenu.AppendFormat(@"

<li>

    <a href='/Home/Index'
       class='waves-effect {0}'>

        <i class='uim uim-airplay'></i>

        <span class='menu-text-large'>
            Dashboard
        </span>

    </a>

</li>

",
            dashboardActive ? "active-menu" : "");

            // =========================
            // Dynamic Root Menus
            // =========================

            var rootMenus = _menuItems
                .Where(x =>
                    x.ParentId == 0 &&
                    !x.Title.ToLower().Contains("dashboard"))
                .OrderBy(x => x.Sequence)
                .ToList();

            foreach (var item in rootMenus)
            {
                bool hasChildren =
                    _menuItems.Any(x => x.ParentId == item.Id);

                bool isActive =
                    !string.IsNullOrEmpty(item.Url) &&
                    item.Url.ToLower().Contains(currentController);

                bool childActive =
                    _menuItems.Any(x =>
                        x.ParentId == item.Id &&
                        !string.IsNullOrEmpty(x.Url) &&
                        x.Url.ToLower().Contains(currentController));

                navigationMenu.Append("<li>");

                // =========================
                // Parent Menu With Child
                // =========================

                if (hasChildren)
                {
                    navigationMenu.AppendFormat(@"

<a href='javascript:void(0);'
   class='has-arrow waves-effect {2}'
   aria-expanded='{3}'>

    {0}

    <span class='menu-text-large'>
        {1}
    </span>

</a>

",
                        string.IsNullOrEmpty(item.Icon)
                            ? "<i class='uim uim-circle'></i>"
                            : item.Icon,

                        item.Title,

                        childActive ? "active-menu" : "",

                        childActive ? "true" : "false"
                    );

                    GenerateSubMenu(
                        item,
                        navigationMenu,
                        currentController
                    );
                }
                else
                {
                    navigationMenu.AppendFormat(@"

<a href='{0}'
   class='waves-effect {3}'>

    {1}

    <span class='menu-text-large'>
        {2}
    </span>

</a>

",
                        string.IsNullOrEmpty(item.Url)
                            ? "#"
                            : item.Url,

                        string.IsNullOrEmpty(item.Icon)
                            ? "<i class='uim uim-circle'></i>"
                            : item.Icon,

                        item.Title,

                        isActive ? "active-menu" : ""
                    );
                }

                navigationMenu.Append("</li>");
            }

            navigationMenu.Append("</ul>");

            return navigationMenu.ToString();
        }

        // =====================================
        // Generate Sub Menu
        // =====================================

        private static void GenerateSubMenu(
    MenuItem parentItem,
    StringBuilder navigationMenu,
    string currentController)
        {
            var childMenus = _menuItems
                .Where(x => x.ParentId == parentItem.Id)
                .OrderBy(x => x.Sequence)
                .ToList();

            if (!childMenus.Any())
                return;

            navigationMenu.Append(@"

<ul class='sub-menu'
    aria-expanded='false'>

");

            foreach (var child in childMenus)
            {
                bool hasChildren =
                    _menuItems.Any(x => x.ParentId == child.Id);

                bool isActive =
                    !string.IsNullOrEmpty(child.Url) &&
                    child.Url.ToLower().Contains(currentController);

                navigationMenu.Append("<li>");

                // =========================
                // Parent Menu
                // =========================

                if (hasChildren)
                {
                    navigationMenu.AppendFormat(@"

<a href='javascript:void(0);'
   class='has-arrow waves-effect'>

    <span class='menu-text-large'>
        {0}
    </span>

</a>

",
                        child.Title);

                    // Recursive submenu
                    GenerateSubMenu(
                        child,
                        navigationMenu,
                        currentController
                    );
                }
                else
                {
                    // =========================
                    // Child Menu
                    // =========================

                    navigationMenu.AppendFormat(@"

<a href='{0}'
   class='{2}'>

    <span class='menu-text-large'>
        {1}
    </span>

</a>

",
                        string.IsNullOrEmpty(child.Url)
                            ? "#"
                            : child.Url,

                        child.Title,

                        isActive ? "active-menu" : ""
                    );
                }

                navigationMenu.Append("</li>");
            }

            navigationMenu.Append("</ul>");
        }
        // =====================================
        // Get Menu By User Role
        // =====================================

        private static void GetMenuByUserRole()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DbFactory>()
                .As<IDbFactory>();

            builder.RegisterGeneric(typeof(BaseRepository<>))
                .As(typeof(IBaseRepository<>));

            builder.RegisterType<UnitOfWork>()
                .As<IUnitOfWork>();

            IContainer container = builder.Build();

            RoleService roleService = new RoleService(
                container.Resolve<IBaseRepository<ApplicationRole>>(),
                container.Resolve<IBaseRepository<ApplicationUserRole>>(),
                container.Resolve<IUnitOfWork>()
            );

            MenuService menuService = new MenuService(
                container.Resolve<IBaseRepository<MenuItem>>(),
                container.Resolve<IBaseRepository<ApplicationRoleMenu>>(),
                container.Resolve<IUnitOfWork>()
            );

            int userId =
                HttpContext.Current.User.Identity.GetUserId<int>();

            var roles =
                roleService.GetUserRoleByUserId(userId);

            var roleIds =
                roles.Select(x => x.RoleId).ToList();

            var menus =
                menuService.GetMenuByUserRole(roleIds);

            HttpContext.Current.Session["NavigationMenu"] = menus;
        }
    }
}
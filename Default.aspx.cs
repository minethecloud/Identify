using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        IPrincipal user = ClaimsPrincipal.Current;

        if (user.Identity.IsAuthenticated)
        {
            uxLoggedInContent.Visible = true;
            uxAnonymousAccess.Visible = false;
            uxAuthenicatedUser.Text = user.Identity.Name;
            uxRoles.Text = GetRoles(user);

        }
        else
        {
            uxAnonymousAccess.Visible = true;
            uxLoggedInContent.Visible = false;
        }
    }

    private string GetRoles(IPrincipal user)
    {
        var roles = new List<string>();

        foreach (var claim in ClaimsPrincipal.Current.Claims
            .Where(claim => claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Where(claim => !roles.Contains(claim.Value)))
        {
            roles.Add(claim.Value);
        }
        return FormatList(roles);
    }

    private string FormatList(List<string> roles)
    {
        var sb = new StringBuilder();
        foreach (var role in roles)
        {
            sb.AppendLine(role);
        }
      return  sb.ToString();
    }
}
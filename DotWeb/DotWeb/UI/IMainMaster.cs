using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    /// <summary>
    /// Master pages for this web application must implement this interface.
    /// </summary>
    public interface IMainMaster
    {
        /// <summary>
        /// Master page must contain <see cref="ContentPlaceHolder"/> called PageTitle.
        /// </summary>
        ContentPlaceHolder PageTitle { get; }

        /// <summary>
        /// Master page must contain <see cref="ContentPlaceHolder"/> called MainContent.
        /// </summary>
        ContentPlaceHolder MainContent { get; }
    }
}

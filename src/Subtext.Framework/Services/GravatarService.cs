#region Disclaimer/Info

///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext@googlegroups.com 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Subtext.Framework.Services
{
    //TODO: This service is a bit hard to use. We should refactor it so it has 
    //      a bit more smarts about Subtext. Such as it can figure out the default image FQDN URL
    public class GravatarService
    {
        public GravatarService(NameValueCollection settings) : this(settings["GravatarUrlFormatString"], settings.GetBoolean("GravatarEnabled"))
        {
        }

        public GravatarService(string urlFormatString, bool enabled)
        {
            UrlFormatString = urlFormatString;
            Enabled = enabled;
        }

        public bool Enabled { get; private set; }

        public string UrlFormatString { get; private set; }

        public string GenerateUrl(string email)
        {
            var emailForUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(email))
            {
                emailForUrl = GetMD5(email.ToLowerInvariant() ?? string.Empty).ToLowerInvariant();
            }
            
            return string.Format(CultureInfo.InvariantCulture, UrlFormatString, emailForUrl);
        }

        private static string GetMD5(string value)
        {
            var algorithm = MD5.Create();
            
            var data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            var hash = string.Join(string.Empty, data.Select(b => b.ToString("x2")));

            return hash;
        }
    }
}
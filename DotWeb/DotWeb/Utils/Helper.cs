#define EF6
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotWeb.Utils
{
    public static class Helper
    {
        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static List<PropertyInfo> GetDbSetProperties(this DbContext context)
        {
            var dbSetProperties = new List<PropertyInfo>();
            var properties = context.GetType().GetProperties();

            foreach (var property in properties)
            {
                var setType = property.PropertyType;

#if EF5 || EF6
            var isDbSet = setType.IsGenericType && (typeof (IDbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()) || setType.GetInterface(typeof (IDbSet<>).FullName) != null);
#elif EF7
            var isDbSet = setType.IsGenericType && (typeof (DbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()));
#endif

                if (isDbSet)
                {
                    dbSetProperties.Add(property);
                }
            }

            return dbSetProperties;
        }

        public static string ReadStringFromResource(Assembly assembly, string name)
        {
            string content = "";
            string[] names = assembly.GetManifestResourceNames();
            using (Stream stream = assembly.GetManifestResourceStream(assembly.ManifestModule.Name.Replace(".dll", "._data.") + name))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
            }

            return content;
        }

        // Convert the string to camel case.
        public static string ToCamelCase(this string inputStr)
        {
            // If there are 0 or 1 characters, just return the string.
            if (inputStr == null || inputStr.Length < 2)
                return inputStr.ToLower();

            // Split the string into words.
            return string.Concat(inputStr.Substring(0, 1).ToLower(), inputStr.Substring(1, inputStr.Length - 1));
        }

        public static void DisplayData(System.Data.DataTable table)
        {
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Console.WriteLine("============================");
            }
        }
        public static void WriteData(string name, System.Data.DataTable table)
        {
            var sb = new StringBuilder();
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    sb = sb.AppendFormat("{0} = {1}{2}", col.ColumnName, row[col], Environment.NewLine);
                }
                sb = sb.AppendFormat("============================{0}", Environment.NewLine);
            }
            File.WriteAllText(string.Concat(Helper.GetAssemblyDirectory(), Path.DirectorySeparatorChar, string.Format("Helper.WriteData.{0}.txt", name)), sb.ToString());
        }

        public static string ToTitleCase(this string inputStr)
        {
            var words = inputStr.Split(new char[] { '_' });
            var sb = new StringBuilder();
            foreach (var word in words)
                sb = sb.AppendFormat("{0}{1}", word.First().ToString().ToUpper(), string.Join("", word.Skip(1)));

            return Regex.Replace(sb.ToString(), "[a-z][A-Z]", m => m.Value[0] + " " + m.Value[1]);
        }

    }
}

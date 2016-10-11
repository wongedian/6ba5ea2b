#define EF6
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DotWeb.Utils
{
    /// <summary>
    /// The helper class. This is for any arbitrary methods that have not (or can not) be grouped into its own class.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Get directory path of executing assembly.
        /// </summary>
        /// <returns>String, in the form of file system path.</returns>
        public static string GetExecutingAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Obtains properties of <see cref="DbContext"/> instance.
        /// </summary>
        /// <param name="context"><see cref="DbContext"/> being inspected.</param>
        /// <returns>List of <see cref="PropertyInfo"/> of context being inspected.</returns>
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

        /// <summary>
        /// This method can be used to read text from embedded resource of an assembly. This can be useful read attached text
        /// resource files. For example, to attach initial data as text files, and using data initializer (or data migration) to fill database with
        /// the context attached text files.
        /// </summary>
        /// <param name="assembly">The assembly where embedded resource located.</param>
        /// <param name="name">Resource file name.</param>
        /// <returns>String, the content of embedded resource file.</returns>
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

        /// <summary>
        /// This helper method is mainly used in a Console application to display datatable context. This is for development/debugging purpose only.
        /// </summary>
        /// <param name="table">Data table being rendered.</param>
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

        /// <summary>
        /// This helper method is used to write data table content to a text file. This is for development/debugging purpose only.
        /// </summary>
        /// <param name="name">The outpuf file name.</param>
        /// <param name="table">Data table being rendered.</param>
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
            File.WriteAllText(string.Concat(Helper.GetExecutingAssemblyDirectory(), Path.DirectorySeparatorChar, string.Format("Helper.WriteData.{0}.txt", name)), sb.ToString());
        }

        /// <summary>
        /// Convert arbitrary input string to its title case. For example HelloWorld => Hello World; Hello_world => Hello world.
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
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

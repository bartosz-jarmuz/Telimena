namespace Telimena.Client
{
    using System.Reflection;

    public class AssemblyInfo
    {
        public AssemblyInfo(Assembly assembly)
        {
            var assName = assembly.GetName();
            this.Name = assName.Name;
            this.FullName = assName.FullName;
            this.Version = assName.Version.ToString();
            this.Company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            this.Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            this.Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            this.Title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            this.Product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            this.Trademark = assembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark;
        }

        public string Product { get; set; }
        public string Trademark { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string FullName { get; set; }

        public AssemblyInfo()
        {
        }

        public string Name { get;  set; }
        public string Version { get;  set; }
    }
}
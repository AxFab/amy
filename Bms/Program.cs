using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Bms
{
  class Glob
  {
    public static IEnumerable<string> Enumerate (string glob)
    {
      int i = glob.IndexOf('*');
      string dir = glob.Substring(0, i);
      string ext = glob.Substring(i);
      foreach (string file in Directory.EnumerateFiles(dir, ext))
        yield return file;
    }
  }

  class Item
  {
    public string Name { get; protected set; }
    public Guid Guid { get; protected set; }

    public Item(XElement dom)
    {
      XAttribute attr;
      attr = dom.Attribute("name");
      Name = attr != null ? attr.Value : anonymous();

      XElement elm;
      elm = dom.Element("Guid");
      Guid = elm != null ? Guid.Parse(elm.Value) : Guid.NewGuid();
    }

    private string anonymous ()
    {
      const string base64 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_,";
      Random rd = new Random();
      string str = "";
      for (int i = 0; i < 8; ++i)
        str += base64[rd.Next(64)];
      return str;
    }
  }

  class Delivery : Item
  {
    List<string> sources_ = new List<string>();
    List<string> dependancies_ = new List<string>();
    List<Delivery> references_ = new List<Delivery>();
    

    public Delivery (XElement dom, Project project)
      : base(dom)
    {
      FileInfo cur = new FileInfo(".");
      foreach (XElement child in dom.Elements("Source")) {
        string url = child.Value;
        if (url.Contains("*"))
          foreach (string file in Glob.Enumerate(url)) {
            FileInfo fi = new FileInfo(file);
            sources_.Add(fi.FullName.Replace(cur.FullName + '\\', ""));
          }
        else {
          FileInfo fi = new FileInfo(url);
          sources_.Add(fi.FullName.Replace(cur.FullName+'\\', ""));
        }
      }

      dependancies_.Add("System");
      dependancies_.Add("System.Core");
      dependancies_.Add("System.Xml");
      dependancies_.Add("System.Xml.Linq");


      foreach (XElement child in dom.Elements("Reference")) {
        string refer = child.Value;
        references_.Add(project.Delivery(refer));
      }
    }

    public void BuildCsProj ()
    {
      BuildCsProjVS2012();
    }

    public void BuildCsProjVS2012()
    {
      XNamespace nms = "http://schemas.microsoft.com/developer/msbuild/2003";
      XElement root = new XElement(nms + "Project",
        new XAttribute("ToolsVersion", "4.0"),
        new XAttribute("DefaultTargets", "Build"),
        new XElement(nms + "Import",
          new XAttribute("Project", @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"),
          new XAttribute("Condition", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')")
          )
        );
      XElement prop = new XElement(nms + "PropertyGroup",
        new XElement(nms + "Configuration",
          new XAttribute("Condition", " '$(Configuration)' == '' "),
          "Debug"
          ),
        new XElement(nms + "Platform",
          new XAttribute("Condition", " '$(Platform)' == '' "),
          "AnyCPU"
          ),
        new XElement(nms + "ProjectGuid", "{" + Guid.ToString().ToUpper() + "}"),
        new XElement(nms + "OutputType", "Exe"),
        new XElement(nms + "AppDesignerFolder", "Properties"),
        new XElement(nms + "RootNamespace", Name),
        new XElement(nms + "AssemblyName", Name.ToLower()),
        new XElement(nms + "TargetFrameworkVersion", "v4.0"),
        new XElement(nms + "FileAlignment", "512"),
        new XElement(nms + "WarningLevel", "4"),
        new XElement(nms + "TargetFrameworkProfile")
        );
      XElement build = new XElement(nms + "PropertyGroup",
        new XAttribute("Condition", "'$(Configuration)|$(Platform)' == 'Debug|x86'"),
        new XElement(nms + "DebugSymbols", "true"),
        new XElement(nms + "OutputPath", "bin\\"+Name+"\\"),
        new XElement(nms + "DefineConstants", "DEBUG;TRACE"),
        new XElement(nms + "DebugType", "full"),
        new XElement(nms + "Optimize", "false"),
        new XElement(nms + "PlatformTarget", "x86"),
        new XElement(nms + "ErrorReport", "prompt"),
        new XElement(nms + "CodeAnalysisRuleSet", "ManagedMinimumRules.ruleset")
        );
      XElement depGrp = new XElement(nms + "ItemGroup");
      foreach (string src in dependancies_)
        depGrp.Add(new XElement(nms + "Reference",
          new XAttribute("Include", src)
          ));
      
      //<ItemGroup>
      //  <ProjectReference Include="Niut.csproj">
      //    <Project>{a96075f4-8a87-46e4-9a92-a659fcfe5689}</Project>
      //    <Name>Niut</Name>
      //  </ProjectReference>
      //</ItemGroup>

      XElement srcGrp = new XElement(nms + "ItemGroup");
      foreach (string src in sources_)
        srcGrp.Add(new XElement(nms + "Compile",
          new XAttribute("Include", src)
          ));
      XElement refGrp = new XElement(nms + "ItemGroup");
      foreach (Delivery refer in references_)
        refGrp.Add(new XElement(nms + "ProjectReference",
          new XAttribute("Include", refer.Name + ".csproj"),
          new XElement(nms + "Project", "{" + refer.Guid.ToString() + "}"),
          new XElement(nms + "Name", refer.Name)
          ));
      root.Add(prop);
      root.Add(build);
      root.Add(depGrp);
      root.Add(srcGrp);
      root.Add(refGrp);
      root.Add(new XElement(nms + "Import",
        new XAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets")
        ));
      root.Save(this.Name + ".csproj");
    }
  }

  class Project : Item
  {
    List<Delivery> deliveries_ = new List<Delivery>();

    public Project (XElement dom)
      : base(dom)
    {
      foreach (XElement child in dom.Elements("Delivery")) {
        var delivery = new Delivery(child, this);
        deliveries_.Add(delivery);
        delivery.BuildCsProj();
      }
    }

    public Delivery Delivery (string name)
    {
      foreach (Delivery delv in deliveries_) {
        if (delv.Name == name)
          return delv;
      }
      return null;
    }
  }

  class Program
  {
    static void Main(string[] args) 
    {
      XElement build = XElement.Load("build.xml");
      Project project = new Project(build);
    }
  }
}
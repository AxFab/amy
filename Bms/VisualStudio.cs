using System;
using System.Xml.Linq;

namespace Bms
{
  class VisualStudio
  {
    static XNamespace nms = "http://schemas.microsoft.com/developer/msbuild/2003";
    static XObject[] cs_proj_2012 = new XObject[] {
        new XAttribute("ToolsVersion", "4.0"),
        new XAttribute("DefaultTargets", "Build"),
        new XElement(nms + "Import",
          new XAttribute("Project", @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"),
          new XAttribute("Condition", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')")
          )
    };

    public void BuildCsProjVS2012 (Delivery delv)
    {
      XElement root = new XElement(nms + "Project", cs_proj_2012);

      XElement prop = new XElement(nms + "PropertyGroup",
        new XElement(nms + "Configuration",
          new XAttribute("Condition", " '$(Configuration)' == '' "),
          "Debug"
          ),
        new XElement(nms + "Platform",
          new XAttribute("Condition", " '$(Platform)' == '' "),
          "AnyCPU"
          ),
        new XElement(nms + "ProjectGuid", "{" + delv.Guid.ToString().ToUpper() + "}"),
        new XElement(nms + "OutputType", "Exe"),
        new XElement(nms + "AppDesignerFolder", "Properties"),
        new XElement(nms + "RootNamespace", delv.Name),
        new XElement(nms + "AssemblyName", delv.Name.ToLower()),
        new XElement(nms + "TargetFrameworkVersion", "v4.0"),
        new XElement(nms + "FileAlignment", "512"),
        new XElement(nms + "WarningLevel", "4"),
        new XElement(nms + "TargetFrameworkProfile")
        );

      XElement build = new XElement(nms + "PropertyGroup",
        new XAttribute("Condition", "'$(Configuration)|$(Platform)' == 'Debug|x86'"),
        new XElement(nms + "DebugSymbols", "true"),
        new XElement(nms + "OutputPath", "bin\\" + delv.Name + "\\"),
        new XElement(nms + "DefineConstants", "DEBUG;TRACE"),
        new XElement(nms + "DebugType", "full"),
        new XElement(nms + "Optimize", "false"),
        new XElement(nms + "PlatformTarget", "x86"),
        new XElement(nms + "ErrorReport", "prompt"),
        new XElement(nms + "CodeAnalysisRuleSet", "ManagedMinimumRules.ruleset")
        );
      /*
      XElement depGrp = new XElement(nms + "ItemGroup");
      foreach (string src in delv.dependancies_)
        depGrp.Add(new XElement(nms + "Reference",
          new XAttribute("Include", src)
          ));

      XElement srcGrp = new XElement(nms + "ItemGroup");
      foreach (string src in delv.sources_)
        srcGrp.Add(new XElement(nms + "Compile",
          new XAttribute("Include", src)
          ));
      XElement refGrp = new XElement(nms + "ItemGroup");
      foreach (Delivery refer in delv.references_)
        refGrp.Add(new XElement(nms + "ProjectReference",
          new XAttribute("Include", refer.Name + ".csproj"),
          new XElement(nms + "Project", "{" + refer.Guid.ToString() + "}"),
          new XElement(nms + "Name", refer.Name)
          ));
      
      root.Add(prop);
      root.Add(build);
      root.Add(depGrp);
      root.Add(srcGrp);
      root.Add(refGrp);*/
      root.Add(new XElement(nms + "Import",
        new XAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets")
        ));
      root.Save(delv.Name + ".csproj");
    }

  }

}

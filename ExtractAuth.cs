using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var files = Directory.GetFiles(".", "*Controller.cs", SearchOption.AllDirectories);
        using var writer = new StreamWriter("AUTHORIZATION_ANALYSIS.md");
        writer.WriteLine("# API Authorization Analysis");
        writer.WriteLine();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var classNameMatch = Regex.Match(content, @"class\s+(\w+Controller)");
            if (!classNameMatch.Success) continue;
            
            var className = classNameMatch.Groups[1].Value;
            var classRouteMatch = Regex.Match(content, @"\[Route\(""(.*?)""\)\]\s*(?:\[[^\]]+\]\s*)*public\s+class\s+" + className);
            var classRoute = classRouteMatch.Success ? classRouteMatch.Groups[1].Value : "";

            var classAuthMatch = Regex.Match(content, @"\[Authorize(?:.*?\)\]|\])\s*(?:\[[^\]]+\]\s*)*public\s+class\s+" + className);
            var classAuth = "";
            if (classAuthMatch.Success)
            {
                var matchVal = Regex.Match(classAuthMatch.Value, @"\[Authorize(.*?)\]");
                classAuth = matchVal.Success ? matchVal.Value : "[Authorize]";
            }
            var classAnonMatch = Regex.Match(content, @"\[AllowAnonymous\]\s*(?:\[[^\]]+\]\s*)*public\s+class\s+" + className);
            if (classAnonMatch.Success) classAuth = "[AllowAnonymous]";

            writer.WriteLine($"## {className}");
            if (!string.IsNullOrEmpty(classRoute)) writer.WriteLine($"**Base Route:** {classRoute}");
            writer.WriteLine($"**Class Level Authorization:** {classAuth}");
            writer.WriteLine();
            writer.WriteLine("| Method | HTTP Verb | Route | Authorization | Access Control Explanation |");
            writer.WriteLine("|---|---|---|---|---|");

            var methodRegex = new Regex(@"(?:\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\(""(.*?)""\))?\]\s*)+(?:\[(.*?)\]\s*)*public\s+(?:async\s+)?(?:Task<.*?>|IActionResult|ActionResult<.*?>|Task)\s+(\w+)\(");
            var matches = methodRegex.Matches(content);

            var methodInfos = new List<string>();

            // To get auth attributes, we can extract the block of attributes preceding the method
            // We can match everything from the previous '}' or ';' up to the method name.
            // Let's iterate lines to be safer.
            var lines = File.ReadAllLines(file);
            string currentHttpVerb = "";
            string currentRoute = "";
            string currentAuth = "";
            string currentMethodName = "";
            bool inMethodAttrs = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("[Http"))
                {
                    inMethodAttrs = true;
                    var verbMatch = Regex.Match(line, @"\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)");
                    if (verbMatch.Success) currentHttpVerb = verbMatch.Groups[1].Value.Replace("Http", "").ToUpper();
                    
                    var routeMatch = Regex.Match(line, @"\(""(.*?)""\)");
                    if (routeMatch.Success) currentRoute = routeMatch.Groups[1].Value;
                }
                else if (inMethodAttrs && line.StartsWith("[Authorize"))
                {
                    currentAuth = line;
                }
                else if (inMethodAttrs && line.StartsWith("[AllowAnonymous"))
                {
                    currentAuth = "[AllowAnonymous]";
                }
                else if (inMethodAttrs && line.StartsWith("public"))
                {
                    var nameMatch = Regex.Match(line, @"public\s+(?:async\s+)?(?:Task(?:<.*?>)?|IActionResult|ActionResult(?:<.*?>)?|\w+)\s+(\w+)\(");
                    if (nameMatch.Success)
                    {
                        currentMethodName = nameMatch.Groups[1].Value;
                        string finalAuth = string.IsNullOrEmpty(currentAuth) ? (string.IsNullOrEmpty(classAuth) ? "None" : classAuth) : currentAuth;
                        string fullRoute = classRoute + (string.IsNullOrEmpty(currentRoute) ? "" : "/" + currentRoute);
                        
                        string explanation = "";
                        if (finalAuth == "[AllowAnonymous]") explanation = "Public access, no token required.";
                        else if (finalAuth == "[Authorize]" || finalAuth == "None") explanation = "Requires valid authentication token.";
                        else if (finalAuth.Contains("Roles")) 
                        {
                            var rolesMatch = Regex.Match(finalAuth, @"Roles\s*=\s*""(.*?)""");
                            if (rolesMatch.Success)
                                explanation = $"Requires valid token AND user must have one of these roles: {rolesMatch.Groups[1].Value}";
                        }
                        
                        writer.WriteLine($"| {currentMethodName} | {currentHttpVerb} | {fullRoute.Replace("//", "/")} | {finalAuth} | {explanation} |");
                    }
                    // Reset
                    currentHttpVerb = ""; currentRoute = ""; currentAuth = ""; currentMethodName = ""; inMethodAttrs = false;
                }
                else if (line.StartsWith("public") && line.Contains("(") && !line.Contains("class") && !line.Contains("interface"))
                {
                    // Reset if we hit another public method without http attributes
                    currentHttpVerb = ""; currentRoute = ""; currentAuth = ""; currentMethodName = ""; inMethodAttrs = false;
                }
            }
        }
    }
}

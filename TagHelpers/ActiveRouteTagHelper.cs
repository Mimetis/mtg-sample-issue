﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mtg.Sample.TagHelpers
{
    [HtmlTargetElement(Attributes = "is-active-route")]
    public class ActiveRouteTagHelper : TagHelper
    {
     
        public ActiveRouteTagHelper()
        {
        }

        private IDictionary<string, string> _routeValues;

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("asp-page")]
        public string Page { get; set; }


        [HtmlAttributeName("asp-area")]
        public string Area { get; set; }

        /// <summary>Additional parameters for the route.</summary>
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (this._routeValues == null)
                    this._routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return this._routeValues;
            }
            set
            {
                this._routeValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive())
            {
                MakeActive(output);
            }

            output.Attributes.RemoveAll("is-active-route");
        }

        private bool ShouldBeActive()
        {
            string currentController = string.Empty;
            string currentAction = string.Empty;

            if (ViewContext.RouteData.Values["Controller"] != null)
                currentController = ViewContext.RouteData.Values["Controller"].ToString();

            if (ViewContext.RouteData.Values["Action"] != null)
                currentAction = ViewContext.RouteData.Values["Action"].ToString();

            if (Controller != null)
            {
                if (!string.IsNullOrWhiteSpace(Controller) && Controller.ToLower() != currentController.ToLower())
                    return false;

                if (!string.IsNullOrWhiteSpace(Action) && Action.ToLower() != currentAction.ToLower())
                    return false;
            }

            if (Page != null)
            {
                // Getting the Area / Page combo from the current a href node
                var relativePath = Page.StartsWith("/") ? Page : $"/{Page}";

                if (!string.IsNullOrWhiteSpace(Area))
                    relativePath = $"{Area}{relativePath}";

                // Getting the current page context
                var routeData = ViewContext.RouteData.Values["page"].ToString();

                if (ViewContext.RouteData.Values.TryGetValue("area", out var areaData))
                    routeData = $"{areaData}{routeData}";

                // Get directory root menu
                var rootRouteData = Path.GetDirectoryName(routeData);
                var rootRelativePath = Path.GetDirectoryName(relativePath);

                rootRouteData = rootRouteData == "\\" ? routeData : rootRouteData;
                rootRelativePath = rootRelativePath == "\\" ? relativePath : rootRelativePath;


                if (!string.Equals(rootRouteData, rootRelativePath, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            foreach (KeyValuePair<string, string> routeValue in RouteValues)
            {
                if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
                    ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void MakeActive(TagHelperOutput output)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                classAttr = new TagHelperAttribute("class", "active");
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("active") < 0)
            {
                output.Attributes.SetAttribute("class", classAttr.Value == null
                    ? "active"
                    : classAttr.Value.ToString() + " active");
            }
        }
    }
}
﻿using System;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Cassette.Scripts;

namespace Cassette.Web
{
    class CassetteRouting : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        const string RoutePrefix = "_cassette";

        public CassetteRouting(IUrlModifier urlModifier)
        {
            this.urlModifier = urlModifier;
        }

        public void InstallRoutes(RouteCollection routes, IBundleContainer bundleContainer)
        {
            InstallBundleRoute<ScriptBundle>(routes, bundleContainer);
            InstallBundleRoute<StylesheetBundle>(routes, bundleContainer);
            InstallBundleRoute<HtmlTemplateBundle>(routes, bundleContainer);

            InstallRawFileRoute(routes);

            InstallAssetCompileRoute(routes, bundleContainer);
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            return urlModifier.Modify(string.Format("{0}/{1}/{2}_{3}",
                RoutePrefix,
                ConventionalBundlePathName(bundle.GetType()),
                bundle.Path.Substring(2),
                bundle.Assets[0].Hash.ToHexString()
            ));
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(string.Format(
                "{0}?{1}",
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
            ));
        }

        public string CreateAssetCompileUrl(Bundle bundle, IAsset asset)
        {
            return urlModifier.Modify(string.Format(
                "{0}/compile/{1}?{2}",
                RoutePrefix,
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
            ));
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            filename = filename.Substring(2); // Remove the "~/"
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);
            
            return urlModifier.Modify(string.Format("{0}/file/{1}_{2}_{3}",
                RoutePrefix,
                ConvertToForwardSlashes(name),
                hash,
                extension
            ));
        }

        void InstallBundleRoute<T>(RouteCollection routes, IBundleContainer bundleContainer)
            where T : Bundle
        {
            var url = GetBundleRouteUrl<T>();
            var handler = new DelegateRouteHandler(
                requestContext => new BundleRequestHandler<T>(bundleContainer, requestContext)
            );
            Trace.Source.TraceInformation("Installing {0} route handler for \"{1}\".", typeof(T).FullName, url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        string GetBundleRouteUrl<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                RoutePrefix,
                ConventionalBundlePathName(typeof(T))
            );
        }

        void InstallRawFileRoute(RouteCollection routes)
        {
            const string url = RoutePrefix + "/file/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new RawFileRequestHandler(requestContext)
            );
            Trace.Source.TraceInformation("Installing raw file route handler for \"{0}\".", url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        void InstallAssetCompileRoute(RouteCollection routes, IBundleContainer bundleContainer)
        {
            // Used to return compiled coffeescript, less, etc.
            const string url = RoutePrefix + "/compile/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new AssetRequestHandler(
                    requestContext,
                    bundleContainer
                )
            );
            Trace.Source.TraceInformation("Installing asset route handler for \"{0}\".", url);
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        static string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}

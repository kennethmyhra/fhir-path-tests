using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.FhirPath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FhirPathTests
{
    public class CustomNavigator : ScopedNavigator
    {
        public CustomNavigator(IElementNavigator wrapped, FhirPathCompiler compiler) : base(wrapped)
        {
            _compiler = compiler;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        //public CustomNavigator(IElementNavigator wrapped, FhirPathCompiler compiler = null)
        //{
        //    _wrapped = wrapped;
        //    _compiler = compiler ?? new FhirPathCompiler();
        //}

        //private class Cache
        //{
        //    public string Id;
        //    public IEnumerable<CustomNavigator> ContainedResources;
        //    public IEnumerable<BundledResource> BundledResources;
        //    public string FullUrl;
        //}

        //private Cache _cache = new Cache();
        //private IElementNavigator _wrapped = null;
        private FhirPathCompiler _compiler = null;

        //public CustomNavigator Parent { get; private set; } = null;

        //public string Name => _wrapped.Name;

        //public string Type => _wrapped.Type;

        //public object Value => _wrapped.Value;

        //public string Location => _wrapped.Location;

        //public IElementNavigator Clone()
        //{
        //    return new CustomNavigator(_wrapped)
        //    {
        //        Parent = this.Parent,
        //        ResourceContext = this.ResourceContext
        //    };
        //}

        //public bool MoveToFirstChild(string nameFilter = null)
        //{
        //    CustomNavigator me = null;

        //    if (this.AtResource)
        //    {
        //        me = (CustomNavigator)this.Clone();

        //        // Is the current position not a contained resource?
        //        if (Parent?.ContainedResources().FirstOrDefault() == null)
        //        {
        //            ResourceContext = _wrapped.Clone();
        //        }
        //    }

        //    if (!_wrapped.MoveToFirstChild(nameFilter)) return false;

        //    // If the current position is a resource, we'll be the new _parentScope
        //    if (me != null)
        //        Parent = me;

        //    _cache = new Cache();

        //    return true;
        //}

        //public bool MoveToNext(string nameFilter = null)
        //{
        //    _cache = new Cache();
        //    return _wrapped.MoveToNext(nameFilter);
        //}

        //public bool AtResource => Type != null ? Char.IsUpper(Type[0]) && ModelInfo.IsKnownResource(Type) : false;
        //public bool AtBundle => Type != null ? Type == "Bundle" : false;
        //public IElementNavigator ResourceContext { get; private set; } = null;

        //public IEnumerable<CustomNavigator> Parents()
        //{
        //    var scan = this.Parent;

        //    while (scan != null)
        //    {
        //        yield return scan;

        //        scan = scan.Parent;
        //    }
        //}

        //public string Id()
        //{
        //    if (_cache.Id == null)
        //    {
        //        _cache.Id = AtResource ? "#" + _wrapped.Children("id").FirstOrDefault()?.Value as string : null;
        //    }

        //    return _cache.Id;
        //}

        //public IEnumerable<CustomNavigator> ContainedResources()
        //{
        //    if (_cache.ContainedResources == null)
        //    {
        //        if (AtResource)
        //            _cache.ContainedResources = this.Children("contained").Cast<CustomNavigator>();
        //        else
        //            _cache.ContainedResources = Enumerable.Empty<CustomNavigator>();
        //    }
        //    return _cache.ContainedResources;
        //}

        //public class BundledResource
        //{
        //    public string FullUrl;
        //    public ScopedNavigator Resource;
        //}

        //public IEnumerable<BundledResource> BundledResources()
        //{
        //    if (_cache.BundledResources == null)
        //    {
        //        if (AtBundle)
        //            _cache.BundledResources = from e in this.Children("entry")
        //                                      let fullUrl = e.Children("fullUrl").FirstOrDefault()?.Value as string
        //                                      let resource = e.Children("resource").FirstOrDefault() as ScopedNavigator
        //                                      select new BundledResource { FullUrl = fullUrl, Resource = resource };
        //        else
        //            _cache.BundledResources = Enumerable.Empty<BundledResource>();
        //    }

        //    return _cache.BundledResources;
        //}

        //public string FullUrl()
        //{
        //    if (_cache.FullUrl == null)
        //    {
        //        foreach (var parent in Parents())
        //        {
        //            if (parent.AtBundle)
        //            {
        //                var fullUrl = parent.BundledResources()
        //                    .SingleOrDefault(be => this.Location.StartsWith(be.Resource.Location))
        //                    ?.FullUrl;
        //                if (fullUrl != null) _cache.FullUrl = fullUrl;
        //            }
        //        }
        //    }

        //    return _cache.FullUrl;
        //}

    }
}

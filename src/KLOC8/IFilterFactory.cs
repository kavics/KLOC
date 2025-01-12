using System;
using System.Collections.Generic;
using System.Linq;

namespace KLOC8;

public interface IFilterFactory
{
    IFilter CreateFilter(string? path);
}

public class FilterFactory(IDisk disk, IEnumerable<IFilter> filters) : IFilterFactory
{
    public IFilter CreateFilter(string? path)
    {
        Type type;
        if (string.Compare(path, "builtin", StringComparison.InvariantCultureIgnoreCase) == 0)
            type = typeof(CommonListFilter);
        else if (path == null)
            type = typeof(KlocIgnoreFileFilter);
        else
            type = typeof(GlobalKlocIgnoreFileFilter);

        var filter = filters.FirstOrDefault(f => f.GetType() == type);
        if(filter == null)
            throw new InvalidOperationException($"Cannot create a filter.");

        if(filter is GlobalKlocIgnoreFileFilter globalFilter)
            globalFilter.SetFilterPath(path!);

        return filter;
    }
}

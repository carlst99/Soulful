using System;
using System.Collections.Generic;
using System.Text;

namespace Soulful.Core.Services
{
    public interface ICardLoaderService
    {
        List<string> PackKeys { get; }
        Dictionary<string, string> PackNames { get; }
    }
}

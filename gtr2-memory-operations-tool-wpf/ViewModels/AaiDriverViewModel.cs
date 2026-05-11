using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Gtr2MemOpsTool.ViewModels
{
    public class AaiDriverViewModel
    {
        public ObservableCollection<AaiDriver> AaiDrivers { get; } =
            [
                new AaiDriver { Name = "Alpha" },
                new AaiDriver { Name = "Beta" },
            ];
    }
}

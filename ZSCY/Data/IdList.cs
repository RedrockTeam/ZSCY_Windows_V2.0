using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSCY.Data
{
    public class uIdList
    {
        public string uId { get; set; }
    }

    public class AuIdList
    {
        public ObservableCollection<uIdList> muIdList = new ObservableCollection<uIdList>();
        public string week { get; set; }

    }
}

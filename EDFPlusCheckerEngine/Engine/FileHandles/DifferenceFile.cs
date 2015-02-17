using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    class DifferenceFile
    {
        public List<Trigger[]> AddList;
        public List<Trigger> RemoveList;

        public bool IsEmpty()
        {
            return ( AddList.Count + RemoveList.Count) == 0;
        }

        public DifferenceFile(List<Trigger[]> addList, List<Trigger> removeList)
        {
            this.AddList = addList;
            this.RemoveList = removeList;
        }
    }
}

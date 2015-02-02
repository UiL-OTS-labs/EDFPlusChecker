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
        public List<Trigger[]> RemoveList;
        public List<Trigger[]> ReplaceList;

        public bool IsEmpty()
        {
            return ( AddList.Count + RemoveList.Count + ReplaceList.Count ) == 0;
        }

        public DifferenceFile(List<Trigger[]> addList, List<Trigger[]> removeList, List<Trigger[]> replaceList)
        {
            this.AddList = addList;
            this.RemoveList = removeList;
            this.ReplaceList = replaceList;
        }
    }
}

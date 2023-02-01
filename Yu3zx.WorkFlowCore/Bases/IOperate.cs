using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public interface IOperate
    {
        /// <summary>
        /// 进行工作
        /// </summary>
        /// <returns></returns>
        bool DoWork();

    }
}

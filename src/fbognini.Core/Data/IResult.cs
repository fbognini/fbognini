using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Data
{
    public interface IResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}

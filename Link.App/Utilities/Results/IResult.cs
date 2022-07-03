using System;
using System.Collections.Generic;
using System.Text;

namespace Link.Core.Utilities.Results
{
    public interface IResult
    {
        bool IsSuccess { get; set; }
        string Message { get; set; }
    }
}

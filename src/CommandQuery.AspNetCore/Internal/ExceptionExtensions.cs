﻿using System;

namespace CommandQuery.AspNetCore.Internal
{
    internal static class ExceptionExtensions
    {
        public static Error ToError(this Exception exception)
        {
            return new Error { Message = exception.Message };
        }
    }
}
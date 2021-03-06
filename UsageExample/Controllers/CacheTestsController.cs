﻿using System;
using System.Collections.Generic;
using Como.WebApi.Caching.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace UsageExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheTestsController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Produces("text/html")]
        [Cached(MyCacheConstants.CacheGroupValues,
            ScopeName = MyCacheConstants.CacheScopePerUser,
            ExpireAfter = "00:00:10",
            SlidingExpiration = false)]
        public string Get()
        {
            return $@"<html>
    <body>
        Cached at {DateTime.Now:HH:mm:ss}.
        Will expire at {DateTime.Now.AddSeconds(10):HH:mm:ss}.
        <script>
            setTimeout(()=> location.reload(), 1000);
        </script>
    </body>
</html>";
        }

        // GET api/values
        [HttpGet("null")]
        [Produces("application/json")]
        [Cached(MyCacheConstants.CacheGroupValues,
            ScopeName = MyCacheConstants.CacheScopePerUser,
            ExpireAfter = "00:00:10",
            SlidingExpiration = false)]
        public string GetNull()
        {
            return null;
        }

        // GET api/values/5
        [HttpGet("invalidate")]
        [DelayedInvalidatesCache("00:00:03", MyCacheConstants.CacheGroupValues)]
        public ActionResult<IEnumerable<string>> Invalidate()
        {
            return new[] {$"will expire at {DateTime.Now.AddSeconds(3):HH:mm:ss}"};
        }
    }
}
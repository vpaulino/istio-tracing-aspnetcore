﻿using Istio.Tracing.Propagation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Istio.Tracing.Propagation
{
    public class HeadersPropagationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<HeadersPropagationDelegatingHandler> logger;

        public HeadersPropagationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<HeadersPropagationDelegatingHandler> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var istioHeaders = httpContextAccessor.HttpContext?.RequestServices.GetService<IIstioHeadersHolder>();

            if (istioHeaders != null)
            {
                AddHeaderIfNotNull(request, IstioHeaders.REQUEST_ID, istioHeaders.RequestId);
                AddHeaderIfNotNull(request, IstioHeaders.B3_TRACE_ID, istioHeaders.B3TraceId);
                AddHeaderIfNotNull(request, IstioHeaders.B3_SPAN_ID, istioHeaders.B3SpanId);
                AddHeaderIfNotNull(request, IstioHeaders.B3_PARENT_SPAN_ID, istioHeaders.B3ParentSpanId);
                AddHeaderIfNotNull(request, IstioHeaders.B3_SAMPLED, istioHeaders.B3Sampled);
                AddHeaderIfNotNull(request, IstioHeaders.B3_FLAGS, istioHeaders.B3Flags);
                AddHeaderIfNotNull(request, IstioHeaders.OT_SPAN_CONTEXT, istioHeaders.OtSpanContext);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private void AddHeaderIfNotNull(HttpRequestMessage request, string headerName, string headerValue)
        {
            if (!string.IsNullOrWhiteSpace(headerValue))
                request.Headers.TryAddWithoutValidation(headerName, headerValue);
            else
                logger.LogTrace("Not adding header {headerName} to the client. It is null or empty.", headerName);
        }
    }
}

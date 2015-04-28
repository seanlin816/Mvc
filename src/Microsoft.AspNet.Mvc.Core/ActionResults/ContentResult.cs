// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    public class ContentResult : ActionResult
    {
        private readonly MediaTypeHeaderValue DefaultContentType = new MediaTypeHeaderValue("text/plain")
        {
            Charset = Encodings.UTF8EncodingWithoutBOM.WebName
        };

        public string Content { get; set; }

        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        public override async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var response = context.HttpContext.Response;

            MediaTypeHeaderValue contentTypeHeader = ContentType;
            Encoding encoding;
            if(contentTypeHeader == null)
            {
                contentTypeHeader = DefaultContentType;
                encoding = Encodings.UTF8EncodingWithoutBOM;
            }
            else
            {
                if(string.IsNullOrEmpty(contentTypeHeader.Charset))
                {
                    encoding = Encodings.UTF8EncodingWithoutBOM;

                    // 1. Do not modify the user supplied content type
                    // 2. Parse here to handle parameters apart from charset
                    contentTypeHeader = MediaTypeHeaderValue.Parse(contentTypeHeader.ToString());
                    contentTypeHeader.Charset = encoding.WebName;
                }
                else
                {
                    if (string.Equals(
                        contentTypeHeader.Charset,
                        Encodings.UTF8EncodingWithoutBOM.WebName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        encoding = Encodings.UTF8EncodingWithoutBOM;
                    }
                    else if (string.Equals(
                        contentTypeHeader.Charset,
                        Encodings.UTF16EncodingLittleEndian.WebName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        encoding = Encodings.UTF16EncodingLittleEndian;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(contentTypeHeader.Charset);
                    }
                }
            }

            response.ContentType = contentTypeHeader.ToString();

            if (StatusCode != null)
            {
                response.StatusCode = StatusCode.Value;
            }

            if (Content != null)
            {
                await response.WriteAsync(Content, encoding);
            }
        }
    }
}
